//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Interop.Word;
	using System;
	using System.IO;


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
			try
			{
				object target = Path.GetTempFileName();
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

			return null;
		}


		public string ConvertClipboardToHtml()
		{
			try
			{
				// create new document
				var doc = word.Documents.Add();

				// insert new paragraph at the end of document; \endofdoc is a predefined bookmark
				var para = doc.Content.Paragraphs.Add(doc.Bookmarks[@"\endofdoc"].Range);
				para.Range.Paste();

				// save as HTML
				object target = Path.GetTempFileName();
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

			return null;
		}
	}
}
