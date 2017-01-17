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
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using JsonFx.Json;

namespace MicrosoftTranslatorSdk
{
	
	public class AdmAuthentication
	{
		public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
		private string clientId;
		private string clientSecret;
		private string request;
		private AdmAccessToken token;

		public AdmAuthentication (string clientId, string clientSecret)
		{
			this.clientId = clientId;
			this.clientSecret = clientSecret;
			//If clientid or client secret has special characters, encode before sending request
			this.request = string.Format ("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", 
				WWW.EscapeURL (clientId), WWW.EscapeURL (clientSecret));
			this.token = HttpPost (DatamarketAccessUri, this.request);
		}

		public AdmAccessToken GetAccessToken ()
		{
			return this.token;
		}

		public void RenewAccessToken ()
		{
			AdmAccessToken newAccessToken = HttpPost (DatamarketAccessUri, this.request);
			//swap the new token with old one
			//Note: the swap is thread unsafe
			this.token = newAccessToken;
			Debug.Log (string.Format ("Renewed token for user: {0} is: {1}", this.clientId, this.token.access_token));
		}

		private AdmAccessToken HttpPost (string DatamarketAccessUri, string requestDetails)
		{
			//Prepare OAuth request 
			WebRequest webRequest = WebRequest.Create (DatamarketAccessUri);
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Method = "POST";
			byte[] bytes = Encoding.ASCII.GetBytes (requestDetails);
			webRequest.ContentLength = bytes.Length;
			using (Stream outputStream = webRequest.GetRequestStream ()) {
				outputStream.Write (bytes, 0, bytes.Length);
			}
			using (WebResponse webResponse = webRequest.GetResponse ()) {
				//Get deserialized object from JSON stream
				StreamReader streamReader = new StreamReader (webResponse.GetResponseStream (), Encoding.UTF8);
				AdmAccessToken token = JsonReader.Deserialize<AdmAccessToken> (streamReader.ReadToEnd ());
				streamReader.Close ();
				return token;
			}
		}
	}


	public class AdmAccessToken
	{
		public string access_token { get; set; }

		public string token_type { get; set; }

		public string expires_in { get; set; }

		public string scope { get; set; }
	}
}