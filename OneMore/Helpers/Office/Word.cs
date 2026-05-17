//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;
	using MSWord = Microsoft.Office.Interop.Word;


	internal class Word : IDisposable
	{
		private MSWord.Application word;
		private bool disposed;


		public Word()
		{
			word = new MSWord.Application
			{
				Visible = false
			};
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				word.Quit();
				Marshal.ReleaseComObject(word);

				word = null;
				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		public string ConvertFileToHtml(object source)
		{
			object target = null;
			MSWord.Document opened = null;
			MSWord.Document doc = null;
			MSWord.WebOptions webOptions = null;

			try
			{
				var tempPath = Path.GetTempPath();
				target = Path.Combine(tempPath, Path.GetRandomFileName());
				object format = MSWord.WdSaveFormat.wdFormatHTML;

				// open document; do not capture/release word.Documents — see PowerPoint.cs
				// for why Office Application-owned collections must not be released
				opened = word.Documents.Open(ref source);
				doc = word.ActiveDocument;

				webOptions = doc.WebOptions;
				webOptions.Encoding = Microsoft.Office.Core.MsoEncoding.msoEncodingUTF8;

				// save as HTML
				doc.SaveAs2(ref target, ref format);
				doc.Close();

				// read HTML
				var tarpath = target.ToString();
				var html = File.ReadAllText(tarpath);

				// patch image paths so images get imported too
				var subfolder = Path.Combine(
					tempPath,
					$"{Path.GetFileNameWithoutExtension(tarpath)}_files");

				if (Directory.Exists(subfolder))
				{
					var matches = Regex.Matches(html, @"<img[^>]+src=""([^""]+)""");
					var builder = new StringBuilder(html);
					for (var i = matches.Count - 1; i >= 0; i--)
					{
						Match match = matches[i];
						builder.Insert(match.Groups[1].Index, tempPath);
					}

					html = builder.ToString();
				}

				return html;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}
			finally
			{
				if (webOptions != null) Marshal.ReleaseComObject(webOptions);
				if (doc != null) Marshal.ReleaseComObject(doc);
				if (opened != null) Marshal.ReleaseComObject(opened);

				try
				{
					if (File.Exists((string)target))
					{
						File.Delete((string)target);
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error cleaning up {target}", exc);
				}
			}

			return null;
		}


		public string ConvertClipboardToHtml()
		{
			object target = null;
			MSWord.Document doc = null;
			MSWord.Range content = null;
			MSWord.Paragraphs paragraphs = null;
			MSWord.Bookmarks bookmarks = null;
			MSWord.Bookmark endBookmark = null;
			MSWord.Range bookmarkRange = null;
			MSWord.Paragraph para = null;
			MSWord.Range paraRange = null;

			try
			{
				// create new document; do not capture/release word.Documents — Application
				// owns it and releasing the collection separates the Application's RCW
				doc = word.Documents.Add();

				// insert new paragraph at the end of document; \endofdoc is a predefined bookmark
				content = doc.Content;
				paragraphs = content.Paragraphs;
				bookmarks = doc.Bookmarks;
				endBookmark = bookmarks[@"\endofdoc"];
				bookmarkRange = endBookmark.Range;
				para = paragraphs.Add(bookmarkRange);
				paraRange = para.Range;
				paraRange.Paste();

				// save as HTML
				target = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				object format = MSWord.WdSaveFormat.wdFormatHTML;

				doc.SaveAs2(ref target, ref format);
				doc.Close();

				// read HTML
				string html = File.ReadAllText(target.ToString());
				return html;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error converting to html", exc);
			}
			finally
			{
				if (paraRange != null) Marshal.ReleaseComObject(paraRange);
				if (para != null) Marshal.ReleaseComObject(para);
				if (bookmarkRange != null) Marshal.ReleaseComObject(bookmarkRange);
				if (endBookmark != null) Marshal.ReleaseComObject(endBookmark);
				if (bookmarks != null) Marshal.ReleaseComObject(bookmarks);
				if (paragraphs != null) Marshal.ReleaseComObject(paragraphs);
				if (content != null) Marshal.ReleaseComObject(content);
				if (doc != null) Marshal.ReleaseComObject(doc);

				try
				{
					if (File.Exists((string)target))
					{
						File.Delete((string)target);
					}
				}
				catch (Exception exc)
				{
					Logger.Current.WriteLine($"error cleaning up {target}", exc);
				}
			}
			return null;
		}


		public void ResolveAttachmentRefs(string docPath, XElement root, bool embedded)
		{
			// This code was first recorded as a Word VM macro by using the Find and Replace UI
			// and then opening the macro in the VB script editor...

			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);
			var path = Path.GetDirectoryName(docPath);

			MSWord.Document opened = null;
			MSWord.Document doc = null;
			MSWord.Selection selection = null;
			MSWord.Find find = null;

			try
			{
				// do not capture/release word.Documents — Application owns it; releasing
				// the collection separates the Application's RCW
				opened = word.Documents.Open(docPath);
				doc = word.ActiveDocument;

				selection = word.Selection;
				find = selection.Find;
				find.ClearFormatting();

				// in a find-and-replace, this is a special control sequence that grabs the
				// contents of the clipboard and uses that as the replacement text
				//find.Replacement.Text = "^c";

				// can use regular expression here
				find.Text = @"\<\<*\.*\>\>";

				find.Forward = true;
				find.Wrap = MSWord.WdFindWrap.wdFindContinue;
				find.Format = false;
				find.MatchCase = false;
				find.MatchWholeWord = false;
				find.MatchKashida = false;
				find.MatchDiacritics = false;
				find.MatchAlefHamza = false;
				find.MatchControl = false;
				find.MatchAllWordForms = false;
				find.MatchSoundsLike = false;
				find.MatchWildcards = true;

				var updated = false;

				while (find.Execute())
				{
					// text without << and >>
					var selectionText = selection.Text;
					var text = selectionText.Substring(2, selectionText.Length - 4);

					var element = root.Descendants(ns + "InsertedFile")
						.FirstOrDefault(e => text.Equals(
							e.Attribute("preferredName")?.Value,
							StringComparison.InvariantCultureIgnoreCase));

					if (element != null)
					{
						var source = element.Attribute("pathSource")?.Value;
						if (string.IsNullOrEmpty(source) || !File.Exists(source))
						{
							source = element.Attribute("pathCache")?.Value;
							if (!string.IsNullOrEmpty(source) && !File.Exists(source))
							{
								// broken link
								source = null;
							}
						}

						if (source != null)
						{
							string target = source;

							if (!embedded)
							{
								// linking to a target file, copied to output directory...
								target = Path.Combine(path, text);

								try
								{
									if (File.Exists(target))
									{
										// attempt to remove to avoid any permissions issues
										File.Delete(target);
									}

									// copy cached/source file to md output directory
									File.Copy(source, target, true);
								}
								catch (Exception exc)
								{
									Logger.Current.WriteLine($"error copying to {target}", exc);
									// error copying
									continue;
								}
							}

							RenderAttachment(selection, target, text, embedded);

							updated = true;
						}
					}
				}

				if (updated)
				{
					doc.Save();
				}
			}
			finally
			{
				if (find != null) Marshal.ReleaseComObject(find);
				if (selection != null) Marshal.ReleaseComObject(selection);
				if (doc != null) Marshal.ReleaseComObject(doc);
				if (opened != null) Marshal.ReleaseComObject(opened);
			}
		}


		private void RenderAttachment(
			MSWord.Selection selection, string target, string text, bool embedded)
		{
			var ext = Path.GetExtension(target);

			var association = RegistryHelper.GetAssociation(ext);
			if (association != null)
			{
				MSWord.InlineShapes inlineShapes = null;
				MSWord.InlineShape ole = null;
				try
				{
					selection.Text = String.Empty;

					object filename = target;
					object linkToFile = !embedded;
					object displayAsIcon = true;
					object iconIndex = association.IconIndex;
					object iconFilename = association.IconFilename;
					object iconLabel = Path.GetFileName(target);

					inlineShapes = selection.InlineShapes;
					ole = inlineShapes.AddOLEObject(
						association.ProgID,
						ref filename,
						ref linkToFile,
						ref displayAsIcon,
						ref iconFilename,
						ref iconIndex,
						ref iconLabel
						);
				}
				finally
				{
					if (ole != null) Marshal.ReleaseComObject(ole);
					if (inlineShapes != null) Marshal.ReleaseComObject(inlineShapes);
				}

				return;
			}

			// get a well-formed URL but also decode it so avoid encoding problems
			// with wide unicode characters in the path, Chinese, Hebrew, etc.

			selection.Text = text;

			MSWord.Range range = null;
			MSWord.Hyperlinks hyperlinks = null;
			MSWord.Hyperlink hyperlink = null;
			try
			{
				range = selection.Range;
				object anchor = range;
				object uri = System.Web.HttpUtility.UrlDecode(new Uri(target).AbsoluteUri);
				hyperlinks = selection.Hyperlinks;
				hyperlink = hyperlinks.Add(anchor, ref uri);
			}
			finally
			{
				if (hyperlink != null) Marshal.ReleaseComObject(hyperlink);
				if (hyperlinks != null) Marshal.ReleaseComObject(hyperlinks);
				if (range != null) Marshal.ReleaseComObject(range);
			}
		}
	}
}
