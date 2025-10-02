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
			try
			{
				for (int i = 1; i <= manager.DeviceInfos.Count; i++)
				{
					if (manager.DeviceInfos[i].DeviceID == deviceID)
					{
						info = manager.DeviceInfos[i];
						break;
					}
				}
			}
			finally
			{
				Marshal.ReleaseComObject(manager);
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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public ScanCapabilities GetCapabilities()
		{
			var device = info.Connect();
			var caps = new ScanCapabilities();

			try
			{
				caps.Model = GetModel(device);
				caps.Intents = GetSupportedIntents(device);

				if (device.Properties.Set("Document Handling Capabilities", ScanHandling.Flatbed))
				{
					caps.FlatbedResoltuions = GetSupportedResolutions(device);
				}

				caps.HasFeeder = device.Properties.Get<bool>("Document Handling Capabilities");
				if (caps.HasFeeder)
				{
					if (device.Properties.Set("Document Handling Capabilities", ScanHandling.Feeder))
					{
						caps.FeederResoltuions = GetSupportedResolutions(device);
					}
				}
			}
			finally
			{
				Marshal.ReleaseComObject(device);
			}

			return caps;
		}


		private static string GetModel(Device device)
		{
			try
			{
				var name = device.Properties.Get<string>("Model name");
				if (name is null)
				{
					return string.Empty;
				}

				var number = device.Properties.Get<string>("Model number");
				if (number is null)
				{
					return name;
				}

				return $"{name} ({number})";
			}
			catch { /* noop */ }

			return string.Empty;
		}


		private static IEnumerable<int> GetSupportedIntents(Device device)
		{
			var item = device.Items[1];

			if (item.Properties.Set("Current Intent", ScanIntents.Color))
			{
				yield return ScanIntents.Color;
			}

			if (item.Properties.Set("Current Intent", ScanIntents.Grayscale))
			{
				yield return ScanIntents.Grayscale;
			}

			if (item.Properties.Set("Current Intent", ScanIntents.BlackAndWhite))
			{
				yield return ScanIntents.BlackAndWhite;
			}

			yield return default;
		}


		private static IEnumerable<int> GetSupportedResolutions(Device device)
		{
			var item = device.Items[1];
			var supported = new List<int>();
			int[] testRes = { 75, 100, 150, 200, 300, 600, 1200 };

			foreach (int dpi in testRes)
			{
				try
				{
					if (item.Properties.Set("Horizontal Resolution", dpi) &&
						item.Properties.Set("Vertical Resolution", dpi))
					{
						supported.Add(dpi);
					}
				}
				catch { /* noop */ }
			}

			return supported;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		/// <param name="useFeeder"></param>
		/// <returns></returns>
		public string Scan(string format = ScanFormatID.wiaFormatJPEG, bool useFeeder = false)
		{
			var device = info.Connect();
			var item = device.Items[1];
			ImageFile file = null;

			try
			{
				// 0x01 = Feeder, 0x02 = Flatbed
				item.Properties.Set("Document Handling Select", useFeeder ? 0x01 : 0x02);
				item.Properties.Set("Horizontal Resolution", 300);
				item.Properties.Set("Vertical Resolution", 300);
				item.Properties.Set("Current Intent", ScanIntents.Color);

				file = (ImageFile)item.Transfer(format);
				var bytes = (byte[])file.FileData.get_BinaryData();

				using var stream = new MemoryStream(bytes);
				using var image = Image.FromStream(stream);

				ImageHeight = image.Height;
				ImageWidth = image.Width;

				return Convert.ToBase64String(bytes);
			}
			finally
			{
				Marshal.ReleaseComObject(file);
				Marshal.ReleaseComObject(item);
				Marshal.ReleaseComObject(device);
			}
		}
	}
}
