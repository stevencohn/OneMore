//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Text;
	using System.Xml.Linq;
	using Newtonsoft.Json;


	/// <summary>
	/// Represents a Meta element
	/// </summary>
	internal class Meta : XElement
	{
		private const string JDateFormat = "yyyy-MM-ddTHH:mm";


		/// <summary>
		/// Instantiates a new meta with the given content and predefined namespace
		/// </summary>
		/// <remarks>
		/// PageNamespace.Value must be set prior to using this constructor
		/// </remarks>
		public Meta(string name, string content)
			: this(PageNamespace.Value, name, content)
		{
		}


		/// <summary>
		/// Initializes a new meta with the given content and namespace
		/// </summary>
		/// <param name="ns">A namespace</param>
		public Meta(XNamespace ns, string name, string content)
			: base(ns + "Meta")
		{
			Add(new XAttribute("name", name));
			Add(new XAttribute("content", content));
		}


		/// <summary>
		/// Gets the name of this meta element
		/// </summary>
		public string MetaName => Attribute("name").Value;


		/// <summary>
		/// Gets the value of this meta element.
		/// Note this may return encoded content; if so, use DecodeContent instead
		/// </summary>
		public string Content => Attribute("content").Value;


		/// <summary>
		/// Sets the value of this meta element
		/// </summary>
		/// <param name="content"></param>
		public XElement SetContent(string content)
		{
			SetAttributeValue("content", content);
			return this;
		}


		/// <summary>
		/// Decodes the encoded content of this meta and returns it as a deserialized object
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize</typeparam>
		/// <returns>An instance of T</returns>
		public T DecodeContent<T>()
		{
			var value = Attribute("content").Value;
			var bytes = Convert.FromBase64String(value);
			using (var stream = new MemoryStream(bytes))
			{
				using (var zipper = new GZipStream(stream, CompressionMode.Decompress))
				{
					// TODO: Future: check Version property for reader-makes-right
					using (var reader = new StreamReader(zipper))
					{
						var content = JsonConvert.DeserializeObject<T>(
							reader.ReadToEnd(),
							new JsonSerializerSettings { DateFormatString = JDateFormat });

						return content;
					}
				}
			}
		}


		/// <summary>
		/// Sets teh value of this meta element to a serialized, compressed, and encoded string
		/// </summary>
		/// <typeparam name="T">The Type of the content object to serialize</typeparam>
		/// <param name="content">The object to serialize</param>
		/// <returns></returns>
		public XElement EncodeContent<T>(T content)
		{
			// serialize the object to JSON
			// compress using gzip which should decrease size 6:1
			// encode as base64 which will increase size 1:3 (base64 = [A-Za-z0-9+/])

			var json = JsonConvert.SerializeObject(content,
				new JsonSerializerSettings { DateFormatString = JDateFormat });

			using (var stream = new MemoryStream())
			{
				using (var zipper = new GZipStream(stream, CompressionMode.Compress))
				{
					var bytes = Encoding.Unicode.GetBytes(json);
					zipper.Write(bytes, 0, bytes.Length);
				}

				SetAttributeValue("content", Convert.ToBase64String(stream.ToArray()));
			}

			return this;
		}
	}
}
