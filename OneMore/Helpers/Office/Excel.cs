//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using MSExcel = Microsoft.Office.Interop.Excel;


	internal class Excel : IDisposable
	{
		private MSExcel.Application excel;
		private bool disposed;


		public Excel()
		{
			excel = new MSExcel.Application
			{
				Visible = false
			};
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				excel.Quit();
				Marshal.ReleaseComObject(excel);

				excel = null;
				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}



		/// <summary>
		/// Read and return the valus of each cell in the first column of the first worksheet
		/// </summary>
		/// <param name="path">The full path of the workbook</param>
		/// <returns>A List of strings</returns>
		public List<string> ExtractSimpleList(string path)
		{
			object missing = System.Reflection.Missing.Value;
			var names = new List<string>();

			MSExcel.Workbook book = null;
			MSExcel.Worksheet sheet = null;
			MSExcel.Range range = null;

			try
			{
				// https://learn.microsoft.com/en-us/office/vba/api/excel.workbooks.open
				book = excel.Workbooks.Open(path, ReadOnly: true);

				sheet = (MSExcel.Worksheet)book.Worksheets[1];
				range = sheet.UsedRange;

				for (int r = 1; r <= range.Rows.Count; r++)
				{
					var value = ((MSExcel.Range)sheet.Cells[r, 1]).Value;
					if (value != null && value != missing)
					{
						names.Add(value as string);
					}
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine("error reading spreadsheet", exc);
			}
			finally
			{
				if (range != null)
					Marshal.ReleaseComObject(range);

				if (sheet != null)
					Marshal.ReleaseComObject(sheet);

				if (book != null)
					Marshal.ReleaseComObject(book);
			}

			return names;
		}
	}
}
