//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.File.Evernote
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;


	[TestClass]
	public class EnexReaderTests
	{
		private string path;


		[TestInitialize]
		public void Setup()
		{
			path = Path.Combine(Path.GetTempPath(), $"EnexReaderTests-{Guid.NewGuid():N}.enex");
		}


		[TestCleanup]
		public void Teardown()
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}


		private static string BuildResourceBlock(byte[] data, string mime, string fileName, out string hash)
		{
			using var md5 = MD5.Create();
			hash = BitConverter.ToString(md5.ComputeHash(data)).Replace("-", string.Empty).ToLowerInvariant();

			return
				"<resource>" +
				$"<data encoding=\"base64\">{Convert.ToBase64String(data)}</data>" +
				$"<mime>{mime}</mime>" +
				"<resource-attributes>" +
				$"<file-name>{fileName}</file-name>" +
				"</resource-attributes>" +
				"</resource>";
		}


		[TestMethod]
		public void ReadNotes_SingleNote_ParsesTitleTagsAndTimestamps()
		{
			var enex =
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
				"<en-export>" +
				"<note>" +
				"<title>My Note</title>" +
				"<content><![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
				"<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\">" +
				"<en-note><div>Hello&nbsp;World</div></en-note>]]></content>" +
				"<created>20130202T172415Z</created>" +
				"<updated>20130203T101500Z</updated>" +
				"<tag>work</tag>" +
				"<tag>ideas</tag>" +
				"</note>" +
				"</en-export>";

			System.IO.File.WriteAllText(path, enex);

			var notes = EnexReader.ReadNotes(path).ToList();

			Assert.AreEqual(1, notes.Count);
			var note = notes[0];

			Assert.AreEqual("My Note", note.Title);
			CollectionAssert.AreEqual(new[] { "work", "ideas" }, note.Tags.ToArray());
			Assert.AreEqual(new DateTime(2013, 2, 2, 17, 24, 15, DateTimeKind.Utc), note.Created);
			Assert.AreEqual(new DateTime(2013, 2, 3, 10, 15, 0, DateTimeKind.Utc), note.Updated);

			Assert.IsNotNull(note.Content);
			Assert.AreEqual("en-note", note.Content.Name.LocalName);

			// &nbsp; must decode to a literal U+00A0 character rather than being left
			// as an undeclared entity, which would otherwise fail XML parsing
			var expected = "Hello\u00A0World";
			Assert.IsTrue(note.Content.Value.Contains(expected));
		}


		[TestMethod]
		public void ReadNotes_NoteWithResource_ComputesMatchingHash()
		{
			var data = Encoding.UTF8.GetBytes("fake image bytes");
			var block = BuildResourceBlock(data, "image/png", "photo.png", out var expectedHash);

			var enex =
				"<en-export><note>" +
				"<title>With Image</title>" +
				$"<content><![CDATA[<en-note><div><en-media hash=\"{expectedHash}\" type=\"image/png\"/></div></en-note>]]></content>" +
				block +
				"</note></en-export>";

			System.IO.File.WriteAllText(path, enex);

			var note = EnexReader.ReadNotes(path).Single();

			Assert.AreEqual(1, note.Resources.Count);
			var resource = note.Resources[0];

			Assert.AreEqual(expectedHash, resource.Hash);
			Assert.AreEqual("image/png", resource.Mime);
			Assert.AreEqual("photo.png", resource.FileName);
			Assert.IsTrue(resource.IsImage);
			CollectionAssert.AreEqual(data, resource.Data);
		}


		[TestMethod]
		public void ReadNotes_MultipleNotes_ReturnsAllInOrder()
		{
			var enex =
				"<en-export>" +
				"<note><title>First</title><content><![CDATA[<en-note>one</en-note>]]></content></note>" +
				"<note><title>Second</title><content><![CDATA[<en-note>two</en-note>]]></content></note>" +
				"</en-export>";

			System.IO.File.WriteAllText(path, enex);

			var notes = EnexReader.ReadNotes(path).ToList();

			Assert.AreEqual(2, notes.Count);
			Assert.AreEqual("First", notes[0].Title);
			Assert.AreEqual("Second", notes[1].Title);
		}
	}
}
