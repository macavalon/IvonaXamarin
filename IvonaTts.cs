using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;

using Newtonsoft.Json;


namespace Ivona
{
	public class VoiceJson
	{
		[JsonProperty("Voice")]
		public Voice voice { get; set; }
	}

	public class Voice
	{
		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("Language")]
		public string Language { get; set; }

		[JsonProperty("Gender")]
		public string Gender { get; set; }

	}

	public class IvonaTts
	{
		// Please replace this with your own values
		const string AccessKey = "";
		const string SecretKey = "";

		
        public IvonaTts ()
		{
		}
        
        private string character;

		public Boolean SynthesizeToFile (string text, string destinationFileName)
		{
			Boolean success = false;
			byte[] voice = IvonaCreateSpeech(text);
			if (voice.Length != 0) {
				File.WriteAllBytes (destinationFileName, voice);
				success = true;
			}

			return success;
		}

		private  byte[] IvonaCreateSpeech(string text)
		{
			var date = DateTime.UtcNow;

			const string algorithm = "AWS4-HMAC-SHA256";
			const string regionName = "eu-west-1";
			const string serviceName = "tts";
			const string method = "POST";
			const string canonicalUri = "/CreateSpeech";
			const string canonicalQueryString = "";
			const string contentType = "application/json";


			const string host = serviceName + "." + regionName + ".ivonacloud.com";

			String Name = "Chipmunk";
			String Gender = "Male";
			String Language = "en-US";

            //example character setup
            
            character = "Emma";
            
			if (character == "Geraint") {
				Language = "en-GB-WLS";
				Name = "Geraint";
			} else if (character == "Giorgio") {
				Language = "it-IT";
				Name = "Giorgio";
			} else if (character == "Gwyneth") {
				Language = "en-GB-WLS";
				Name = "Gwyneth";
				Gender = "Female";
			} else if (character == "Emma") {
				Language = "en-GB";
				Name = "Emma";
				Gender = "Female";
			} else if (character == "Eric") {
				Name = "Eric";
			} else if (character == "Justin") {
				Name = "Justin";
			} else if (character == "Salli") {
				Name = "Salli";
				Gender = "Female";
            }



			var Voice = new
						{
							Name = Name.ToString(),
					        Language = Language.ToString(), 
							Gender = Gender.ToString()
						};

			var obj = new
			{
				Input = new
				{
					Data = text,
					Type = "text/plain"
				},
				OutputFormat = new
				{
					Codec = "MP3",
					SampleRate = 22050
				},
				Parameters = new
				{
					Rate = "medium",
					Volume = "medium",
					SentenceBreak = 500,
					ParagraphBreak = 800
				},
				Voice
			};


			var requestPayload = JsonConvert.SerializeObject(obj);


			var hashedRequestPayload = HexEncode(Hash(ToBytes(requestPayload)));

			var dateStamp = date.ToString("yyyyMMdd");
			var requestDate = date.ToString("yyyyMMddTHHmmss") + "Z";
			var credentialScope = string.Format("{0}/{1}/{2}/aws4_request", dateStamp, regionName, serviceName);

			var headers = new SortedDictionary<string, string>
			{
				{"content-type", contentType},
				{"host", host},
				{"x-amz-date", requestDate}
			};

			string canonicalHeaders = "";
			foreach (KeyValuePair<string,string>kvp in headers)
			{
				canonicalHeaders += kvp.Key.ToLowerInvariant();
				canonicalHeaders += ":";
				canonicalHeaders += kvp.Value.Trim();
				canonicalHeaders += "\n";
			}

			const string signedHeaders = "content-type;host;x-amz-date";

			// Task 1: Create a Canonical Request For Signature Version 4

			var canonicalRequest = method + '\n' + canonicalUri + '\n' + canonicalQueryString +
				'\n' + canonicalHeaders + '\n' + signedHeaders + '\n' + hashedRequestPayload;

			var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));


			// Task 2: Create a String to Sign for Signature Version 4
			// StringToSign  = Algorithm + '\n' + RequestDate + '\n' + CredentialScope + '\n' + HashedCanonicalRequest

			var stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", algorithm, requestDate, credentialScope,
				hashedCanonicalRequest);


			// Task 3: Calculate the AWS Signature Version 4

			// HMAC(HMAC(HMAC(HMAC("AWS4" + kSecret,"20130913"),"eu-west-1"),"tts"),"aws4_request")
			byte[] signingKey = GetSignatureKey(SecretKey, dateStamp, regionName, serviceName);

			// signature = HexEncode(HMAC(derived-signing-key, string-to-sign))
			var signature = HexEncode(HmacSha256(stringToSign, signingKey));


			// Task 4: Prepare a signed request
			// Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

			var authorization =
				string.Format("{0} Credential={1}/{2}/{3}/{4}/aws4_request, SignedHeaders={5}, Signature={6}",
					algorithm, AccessKey, dateStamp, regionName, serviceName, signedHeaders, signature);

			// Send the request

			var webRequest = WebRequest.Create("https://" + host + canonicalUri);

			webRequest.Method = method;
			webRequest.Timeout = 20000;
			webRequest.ContentType = contentType;
			webRequest.Headers.Add("X-Amz-date", requestDate);
			webRequest.Headers.Add("Authorization", authorization);
			webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
			webRequest.ContentLength = requestPayload.Length;

			using (Stream newStream = webRequest.GetRequestStream())
			{
				newStream.Write(ToBytes(requestPayload), 0, requestPayload.Length);
				newStream.Flush();
			}

			try
			{
				var response = (HttpWebResponse)webRequest.GetResponse();

				using (Stream responseStream = response.GetResponseStream())
				{
					if (responseStream != null)
					{
						using (var memoryStream = new MemoryStream())
						{
							responseStream.CopyTo(memoryStream);
							return memoryStream.ToArray();
						}
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine ("failed to get tts ");
			}

			return new byte[0];
		}

		public Boolean ListVoices(string Language, String Gender, List<Voice> voices)
		{
			var date = DateTime.UtcNow;

			const string algorithm = "AWS4-HMAC-SHA256";
			const string regionName = "eu-west-1";
			const string serviceName = "tts";
			const string method = "POST";
			const string canonicalUri = "/ListVoices";
			const string canonicalQueryString = "";
			const string contentType = "application/json";


			const string host = serviceName + "." + regionName + ".ivonacloud.com";

			var Voice = new
			{
				Language = "en-US", // giorgio is italian
				Gender = Gender.ToString()
			};

			var obj = new
			{
				//Voice
			};



			//var requestPayload = new JavaScriptSerializer().Serialize(obj);
			var requestPayload = JsonConvert.SerializeObject(obj);


			var hashedRequestPayload = HexEncode(Hash(ToBytes(requestPayload)));

			var dateStamp = date.ToString("yyyyMMdd");
			var requestDate = date.ToString("yyyyMMddTHHmmss") + "Z";
			var credentialScope = string.Format("{0}/{1}/{2}/aws4_request", dateStamp, regionName, serviceName);

			var headers = new SortedDictionary<string, string>
			{
				{"content-type", contentType},
				{"host", host},
				{"x-amz-date", requestDate}
			};


			string canonicalHeaders = "";
			foreach (KeyValuePair<string,string>kvp in headers)
			{
				canonicalHeaders += kvp.Key.ToLowerInvariant();
				canonicalHeaders += ":";
				canonicalHeaders += kvp.Value.Trim();
				canonicalHeaders += "\n";
			}

			const string signedHeaders = "content-type;host;x-amz-date";

			// Task 1: Create a Canonical Request For Signature Version 4

			var canonicalRequest = method + '\n' + canonicalUri + '\n' + canonicalQueryString +
				'\n' + canonicalHeaders + '\n' + signedHeaders + '\n' + hashedRequestPayload;

			var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));


			// Task 2: Create a String to Sign for Signature Version 4
			// StringToSign  = Algorithm + '\n' + RequestDate + '\n' + CredentialScope + '\n' + HashedCanonicalRequest

			var stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", algorithm, requestDate, credentialScope,
				hashedCanonicalRequest);


			// Task 3: Calculate the AWS Signature Version 4

			// HMAC(HMAC(HMAC(HMAC("AWS4" + kSecret,"20130913"),"eu-west-1"),"tts"),"aws4_request")
			byte[] signingKey = GetSignatureKey(SecretKey, dateStamp, regionName, serviceName);

			// signature = HexEncode(HMAC(derived-signing-key, string-to-sign))
			var signature = HexEncode(HmacSha256(stringToSign, signingKey));


			// Task 4: Prepare a signed request
			// Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

			var authorization =
				string.Format("{0} Credential={1}/{2}/{3}/{4}/aws4_request, SignedHeaders={5}, Signature={6}",
					algorithm, AccessKey, dateStamp, regionName, serviceName, signedHeaders, signature);

			// Send the request

			var webRequest = WebRequest.Create("https://" + host + canonicalUri);

			webRequest.Method = method;
			webRequest.Timeout = 20000;
			webRequest.ContentType = contentType;
			webRequest.Headers.Add("X-Amz-date", requestDate);
			webRequest.Headers.Add("Authorization", authorization);
			webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
			webRequest.ContentLength = requestPayload.Length;

			using (Stream newStream = webRequest.GetRequestStream())
			{
				newStream.Write(ToBytes(requestPayload), 0, requestPayload.Length);
				newStream.Flush();
			}

			Boolean success = false;

			try
			{
				var response = (HttpWebResponse)webRequest.GetResponse();

				using (Stream responseStream = response.GetResponseStream())
				{
					if (responseStream != null)
					{

						StreamReader reader = new StreamReader (responseStream);
						// Read the content.
						String responseFromServer = reader.ReadToEnd ();


						voices = JsonConvert.DeserializeObject<List<Voice>>(responseFromServer);

						success = true;

						
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine ("failed to get tts ");
			}

			return success;
		}

		private static byte[] GetSignatureKey(String key, String dateStamp, String regionName, String serviceName)
		{
			byte[] kDate = HmacSha256(dateStamp, ToBytes("AWS4" + key));
			byte[] kRegion = HmacSha256(regionName, kDate);
			byte[] kService = HmacSha256(serviceName, kRegion);
			return HmacSha256("aws4_request", kService);
		}

		private static byte[] ToBytes(string str)
		{
			return Encoding.UTF8.GetBytes(str.ToCharArray());
		}

		private static string HexEncode(byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
		}

		private static byte[] Hash(byte[] bytes)
		{
			return SHA256.Create().ComputeHash(bytes);
		}

		private static byte[] HmacSha256(String data, byte[] key)
		{
			return new HMACSHA256(key).ComputeHash(ToBytes(data));
		}
	}
}





