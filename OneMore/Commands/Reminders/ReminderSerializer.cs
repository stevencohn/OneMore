//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json;
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Text;


	internal class ReminderSerializer
	{
		private const string JDateFormat = "yyyy-MM-ddTHH:mm";
		private readonly ILogger logger;


		public ReminderSerializer()
		{
			logger = Logger.Current;
		}


		/// <summary>
		/// Decodes the encoded content of this meta and returns it as a deserialized object
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize</typeparam>
		/// <returns>An instance of T</returns>
		public Reminder Decode(string value)
		{
			try
			{
				var bytes = Convert.FromBase64String(value);
				using (var stream = new MemoryStream(bytes))
				{
					using (var zipper = new GZipStream(stream, CompressionMode.Decompress))
					{
						using (var reader = new StreamReader(zipper))
						{
							var json = reader.ReadToEnd();
							// TODO: read-makes-right version check here...

							var content = JsonConvert.DeserializeObject<Reminder>(
								json,
								new JsonSerializerSettings { DateFormatString = JDateFormat });

							return content;
						}
					}
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error decoding reminder", exc);
			}

			return null;
		}


		/// <summary>
		/// Sets teh value of this meta element to a serialized, compressed, and encoded string
		/// </summary>
		/// <typeparam name="T">The Type of the content object to serialize</typeparam>
		/// <param name="content">The object to serialize</param>
		/// <returns></returns>
		public string Encode(Reminder content)
		{
			// serialize the object to JSON
			// compress using gzip which should decrease size 6:1
			// encode as base64 which will increase size 1:3 (base64 = [A-Za-z0-9+/])

			try
			{
				var json = JsonConvert.SerializeObject(content,
					new JsonSerializerSettings { DateFormatString = JDateFormat });

				using (var stream = new MemoryStream())
				{
					using (var zipper = new GZipStream(stream, CompressionMode.Compress))
					{
						var bytes = Encoding.Default.GetBytes(json);
						zipper.Write(bytes, 0, bytes.Length);
					}

					return Convert.ToBase64String(stream.ToArray());
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine("error encoding reminder", exc);
			}

			return null;
		}
	}
}
