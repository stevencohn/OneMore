//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Runtime.InteropServices;
	using WIA;


	internal class ScannerManager : IDisposable
	{
		private readonly DeviceInfo info;
		private bool disposed;


		public ScannerManager(string deviceID)
		{
			var manager = new DeviceManager();
			for (int i = 1; i <= manager.DeviceInfos.Count; i++)
			{
				if (manager.DeviceInfos[i].DeviceID == deviceID)
				{
					info = manager.DeviceInfos[i];
					break;
				}
			}

			if (info is null)
			{
				throw new InvalidDataException("invalid DeviceID");
			}
		}


		#region Lifecycle
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Marshal.ReleaseComObject(info);
				}

				disposed = true;
			}
		}


		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion Lifecycle


		/// <summary>
		/// Gets the width of the scanned Image.
		/// </summary>
		public int ImageHeight { get; private set; }


		/// <summary>
		/// Gets the height of the scanned Image.
		/// </summary>
		public int ImageWidth { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<DeviceInfo> ListScannerDevices()
		{
			var infos = new List<DeviceInfo>();
			var manager = new DeviceManager();

			try
			{
				for (int i = 1; i <= manager.DeviceInfos.Count; i++)
				{
					var info = manager.DeviceInfos[i];
					if (info.Type == WiaDeviceType.ScannerDeviceType)
					{
						infos.Add(info);
					}
				}
			}
			finally
			{
				Marshal.ReleaseComObject(manager);
			}

			return infos;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		/// <param name="useFeeder"></param>
		/// <returns></returns>
		public string Scan(string format = ScanFormatID.wiaFormatJPEG, bool useFeeder = false)
		{
			var scanner = info.Connect().Items[1];

			// 0x01 = Feeder, 0x02 = Flatbed
			scanner.Properties.Set("Document Handling Select", useFeeder ? 0x01 : 0x02);
			scanner.Properties.Set("Horizontal Resolution", 300);
			scanner.Properties.Set("Vertical Resolution", 300);
			scanner.Properties.Set("Current Intent", ScanIntents.Color);

			var file = (ImageFile)scanner.Transfer(format);
			var bytes = (byte[])file.FileData.get_BinaryData();

			using var stream = new MemoryStream(bytes);
			using var image = Image.FromStream(stream);

			ImageHeight = image.Height;
			ImageWidth = image.Width;

			return Convert.ToBase64String(bytes);
		}
	}
}
