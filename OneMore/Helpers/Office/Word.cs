//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Word;
	using System;
	using System.IO;
	using System.Runtime.InteropServices;


	internal class Word : IDisposable
	{
		private Application word;
		private bool disposed;


		public Word()
		{
			word = new Application
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
				object format = WdSaveFormat.wdFormatHTML;

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
				object format = WdSaveFormat.wdFormatHTML;

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
	}
}
