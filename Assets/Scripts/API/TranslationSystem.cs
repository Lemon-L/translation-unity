//        __ \/_
//       (' \`\
//    _\, \ \\/ 
//     /`\/\ \\
//          \ \\    
//           \ \\/\/_
//           /\ \\'\
//         __\ `\\\
//          /|`   `\\
//                 \\
//                  \\
//                   \\     ,
//                    `---'    
//         .............................................  
//                  GEKKO <=> NEVER BUG 
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.IO;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace MicrosoftTranslatorSdk
{
	
	public class TranslationSystem : MonoBehaviour
	{
		
		static TranslationSystem m_TranslationSystem = null;

		public static TranslationSystem Instance {
			get {
				if (null == m_TranslationSystem) {
					m_TranslationSystem = new GameObject ("TranslationSystem").AddComponent<TranslationSystem> ();
				}
				return m_TranslationSystem;
			}
		}

		private TranslationSystem ()
		{
			
		}

		void Awake ()
		{
			DontDestroyOnLoad (this);
			m_TranslationSystem = this;
			Initialize ();
		}

		//Access token expires every 10 minutes. Renew it every 9 minutes only.
		private const int RefreshTokenDuration = 9 * 60;

		private AdmAuthentication admAuth;

		private void Initialize ()
		{
			//Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
			//Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
			admAuth = new AdmAuthentication (TranslationConfig.CLIENT_ID, TranslationConfig.CLIENT_SECRET);
			InvokeRepeating ("RefreshToken", RefreshTokenDuration, RefreshTokenDuration);
		}

		private void RefreshToken ()
		{
			admAuth.RenewAccessToken ();
		}

		private string headerValue {
			get { 
				return "Bearer " + admAuth.GetAccessToken ().access_token;
			}
		}

		#region API
		public void DetectMethod (string textToDetect)
		{
			//Keep appId parameter blank as we are sending access token in authorization header.
			string uri = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + textToDetect;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create (uri);
			httpWebRequest.Headers.Add ("Authorization", headerValue);
			WebResponse response = null;
			try {
				response = httpWebRequest.GetResponse ();
				using (Stream stream = response.GetResponseStream ()) {
					StreamReader streamReader = new StreamReader (stream, Encoding.UTF8);
					var result = streamReader.ReadToEnd ();
					streamReader.Close ();
					Debug.Log (Regex.Replace (result, "<([^>]*)>", ""));
				}
			} catch {
				throw;
			} finally {
				if (response != null) {
					response.Close ();
					response = null;
				}
			}
		}

		public string TranslateMethod (string textToDetect, string to){
			string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + WWW.EscapeURL (textToDetect) + "&from=" + "&to=" + to;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create (uri);
			httpWebRequest.Headers.Add ("Authorization", headerValue);
			WebResponse response = null;
			try {
				response = httpWebRequest.GetResponse ();
				using (Stream stream = response.GetResponseStream ()) {
					StreamReader streamReader = new StreamReader (stream, Encoding.UTF8);
					var result = streamReader.ReadToEnd ();
					streamReader.Close ();
					return Regex.Replace (result, "<([^>]*)>", "");
				}
			} catch (WebException e) {
				return e.Message;
			} finally {
				if (response != null) {
					response.Close ();
					response = null;
				}
			}
		}

		public void TranslateMethod (string textToDetect, string to, Action<string> callback){
			StartCoroutine (Translate (textToDetect, to, callback));
		}
		IEnumerator Translate(string textToDetect, string to, Action<string> callback){
			string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + WWW.EscapeURL (textToDetect) + "&from=" + "&to=" + to;
			Dictionary<string,string> headers = new Dictionary<string, string> ();
			headers.Add ("Authorization", headerValue);
			WWW www = new WWW (uri, null, headers);
			yield return www;
			if (www.error != null) {
				callback (www.error);
			} else {
				var result = Regex.Replace (www.text, "<([^>]*)>", "");
				callback (result);
			}
		}
		#endregion
	}
}