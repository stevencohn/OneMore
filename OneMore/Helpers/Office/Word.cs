//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Runtime.InteropServices;
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

			try
			{
				target = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				object format = MSWord.WdSaveFormat.wdFormatHTML;

				// open document
				word.Documents.Open(ref source);
				var doc = word.ActiveDocument;

				// save as HTML
				doc.SaveAs2(ref target, ref format);
				doc.Close();

				// read HTML
				string html = File.ReadAllText(target.ToString());
				return html;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}
			finally
			{
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

			try
			{
				// create new document
				var doc = word.Documents.Add();

				// insert new paragraph at the end of document; \endofdoc is a predefined bookmark
				var para = doc.Content.Paragraphs.Add(doc.Bookmarks[@"\endofdoc"].Range);
				para.Range.Paste();

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

			word.Documents.Open(docPath);
			var doc = word.ActiveDocument;

			var find = word.Selection.Find;
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
				var text = word.Selection.Text.Substring(2, word.Selection.Text.Length - 4);

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

						RenderAttachment(word.Selection, target, text, embedded);

						updated = true;
					}
				}
			}

			if (updated)
			{
				doc.Save();
			}
		}


		private void RenderAttachment(
			MSWord.Selection selection, string target, string text, bool embedded)
		{
			var ext = Path.GetExtension(target);

			var association = RegistryHelper.GetAssociation(ext);
			if (association != null)
			{
				selection.Text = String.Empty;

				object filename = target;
				object linkToFile = !embedded;
				object displayAsIcon = true;
				object iconIndex = association.IconIndex;
				object iconFilename = association.IconFilename;
				object iconLabel = Path.GetFileName(target);

				selection.InlineShapes.AddOLEObject(
					association.ProgID,
					ref filename,
					ref linkToFile,
					ref displayAsIcon,
					ref iconFilename,
					ref iconIndex,
					ref iconLabel
					);

				return;
			}

			// get a well-formed URL but also decode it so avoid encoding problems
			// with wide unicode characters in the path, Chinese, Hebrew, etc.

			selection.Text = text;

			object range = word.Selection.Range;
			object uri = System.Web.HttpUtility.UrlDecode(new Uri(target).AbsoluteUri);
			word.Selection.Hyperlinks.Add(range, ref uri);
		}
	}
}
