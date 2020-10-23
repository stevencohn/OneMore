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
		private bool disposedValue;


		public Word()
		{
			word = new Application
			{
				Visible = false
			};
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


		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				word.Quit();
				word = null;
				disposedValue = true;
			}
		}


		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
