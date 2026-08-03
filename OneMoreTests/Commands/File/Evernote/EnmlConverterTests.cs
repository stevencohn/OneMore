//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.File.Evernote
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Linq;
	using System.Xml.Linq;


	[TestClass]
	public class EnmlConverterTests
	{
		private static EvernoteNote NoteWithContent(string enmlBody)
		{
			return new EvernoteNote
			{
				Title = "Test Note",
				Content = XElement.Parse($"<en-note>{enmlBody}</en-note>")
			};
		}


		[TestMethod]
		public void Convert_BoldAndItalic_EmitsMarkdownMarkers()
		{
			var note = NoteWithContent("<div><b>bold</b> and <i>italic</i></div>");
			var converter = new EnmlConverter(note, r => null, r => null);

			var markdown = converter.Convert();

			StringAssert.Contains(markdown, "**bold**");
			StringAssert.Contains(markdown, "*italic*");
		}


		[TestMethod]
		public void Convert_HashTags_EmitsLeadingHashtagLine()
		{
			var note = NoteWithContent("<div>content</div>");
			note.Tags.Add("work");
			note.Tags.Add("2024 ideas");

			var converter = new EnmlConverter(note, r => null, r => null);
			var markdown = converter.Convert();

			var firstLine = markdown.Split('\n')[0].Trim();
			StringAssert.Contains(firstLine, "#work");
			// leading digit forces a double-hash and spaces become hyphens
			StringAssert.Contains(firstLine, "##2024-ideas");
		}


		[TestMethod]
		public void Convert_EnTodoChecked_EmitsGfmTaskListSyntax()
		{
			var note = NoteWithContent(
				"<div><en-todo checked=\"true\"/>Buy milk</div>" +
				"<div><en-todo checked=\"false\"/>Buy eggs</div>");

			var converter = new EnmlConverter(note, r => null, r => null);
			var markdown = converter.Convert();

			StringAssert.Contains(markdown, "- [x] Buy milk");
			StringAssert.Contains(markdown, "- [ ] Buy eggs");
		}


		[TestMethod]
		public void Convert_EnCrypt_EmitsPlaceholderAndSetsFlag()
		{
			var note = NoteWithContent("<div><en-crypt hint=\"birthday\">ZW5jcnlwdGVk</en-crypt></div>");

			var converter = new EnmlConverter(note, r => null, r => null);
			var markdown = converter.Convert();

			Assert.IsTrue(converter.EncounteredEncryption);
			StringAssert.Contains(markdown, "birthday");
			StringAssert.Contains(markdown, "Encrypted content omitted");
		}


		[TestMethod]
		public void Convert_ImageMedia_InvokesImageWriterAndEmitsMarkdownImage()
		{
			var resource = new EvernoteResource
			{
				Hash = "abc123",
				Mime = "image/png",
				FileName = "photo.png",
				Data = new byte[] { 1, 2, 3 }
			};

			var note = NoteWithContent("<div><en-media hash=\"abc123\" type=\"image/png\"/></div>");
			note.Resources.Add(resource);

			var writtenPaths = 0;
			var converter = new EnmlConverter(
				note,
				r => { writtenPaths++; return @"C:\temp\photo.png"; },
				r => throw new System.Exception("should not write as attachment"));

			var markdown = converter.Convert();

			Assert.AreEqual(1, writtenPaths);
			StringAssert.Contains(markdown, "![photo.png]");
			StringAssert.Contains(markdown, "file:///");
			Assert.AreEqual(0, converter.UnattachedFiles.Count);
		}


		[TestMethod]
		public void Convert_NonImageMedia_InvokesAttachmentWriterAndRecordsUnattached()
		{
			var resource = new EvernoteResource
			{
				Hash = "def456",
				Mime = "application/pdf",
				FileName = "report.pdf",
				Data = new byte[] { 1, 2, 3 }
			};

			var note = NoteWithContent("<div><en-media hash=\"def456\" type=\"application/pdf\"/></div>");
			note.Resources.Add(resource);

			var converter = new EnmlConverter(
				note,
				r => throw new System.Exception("should not write as image"),
				r => @"C:\temp\Test Note_def456_report.pdf");

			var markdown = converter.Convert();

			StringAssert.Contains(markdown, "[report.pdf]");
			Assert.AreEqual(1, converter.UnattachedFiles.Count);
			Assert.IsFalse(resource.IsImage);
		}


		[TestMethod]
		public void Convert_UnorderedList_EmitsBulletItems()
		{
			var note = NoteWithContent("<ul><li>one</li><li>two</li></ul>");
			var converter = new EnmlConverter(note, r => null, r => null);

			var markdown = converter.Convert();
			var lines = markdown.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

			Assert.IsTrue(lines.Contains("- one"));
			Assert.IsTrue(lines.Contains("- two"));
		}
	}
}
