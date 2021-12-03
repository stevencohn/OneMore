//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Helpers.Office
{
	using Microsoft.Office.Core;
	using Microsoft.Office.Interop.PowerPoint;
	using System;
	using System.IO;
	using System.Runtime.InteropServices;


	internal class PowerPoint : IDisposable
	{
		private const int DesiredWidth = 550;

		private Application power;
		private bool disposed;


		public PowerPoint()
		{
			power = new Application();
		}


		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				power.Quit();
				Marshal.ReleaseComObject(power);

				power = null;
				disposed = true;
			}
		}


		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}


		public string ConvertFileToImages(string source)
		{
			try
			{
				var presentation = power.Presentations.Open(
					source, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);

				var count = presentation.Slides.Count;
				if (count == 0)
				{
					Logger.Current.WriteLine("presentation is empty");
					return null;
				}

				var setup = presentation.PageSetup;
				Logger.Current.WriteLine(
					$"presentation size= {setup.SlideWidth} x {setup.SlideHeight} ({setup.SlideSize})");

				var width = DesiredWidth;
				var height = (int)(setup.SlideHeight * (DesiredWidth / setup.SlideWidth));

				var path = Path.Combine(
					Path.GetTempPath(),
					Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				Logger.Current.WriteLine($"exporting {presentation.Slides.Count} slides to {path}");

				presentation.Export(path, "JPG", width, height);

				return path;
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}

			return null;
		}
	}
}
