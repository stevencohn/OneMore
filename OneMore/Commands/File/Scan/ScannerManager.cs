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
			var device = info.Connect() as IDevice;
			var caps = new ScanCapabilities();

			try
			{
				caps.Model = GetModel(device);
				caps.Intents = GetSupportedIntents(device);
				caps.BedHeight = device.Properties.Get<int>(PropertyNames.BedHeight);
				caps.BedWidth = device.Properties.Get<int>(PropertyNames.BedWidth);

				var mask = device.Properties.Get<int>(PropertyNames.DocumentHandling);
				if ((mask & ScanHandling.Flatbed) > 0)
				{
					caps.FlatbedResoltuions = GetSupportedResolutions(device);
				}

				if ((mask & ScanHandling.Feeder) > 0)
				{
					caps.FeederResoltuions = GetSupportedResolutions(device);
				}
			}
			finally
			{
				Marshal.ReleaseComObject(device);
			}

			return caps;
		}


		private static string GetModel(IDevice device)
		{
			try
			{
				var name = device.Properties.Get<string>(PropertyNames.ModelName);
				if (name is null)
				{
					return string.Empty;
				}

				var number = device.Properties.Get<string>(PropertyNames.ModelNumber);
				if (number is null)
				{
					return name;
				}

				return $"{name} ({number})";
			}
			catch { /* noop */ }

			return string.Empty;
		}


		private static IEnumerable<int> GetSupportedIntents(IDevice device)
		{
			var item = device.Items[1];
			var intents = new List<int>();

			if (item.Properties.Set(PropertyNames.CurrentIntent, ScanIntents.Color))
			{
				intents.Add(ScanIntents.Color);
			}

			if (item.Properties.Set(PropertyNames.CurrentIntent, ScanIntents.Grayscale))
			{
				intents.Add(ScanIntents.Grayscale);
			}

			if (item.Properties.Set(PropertyNames.CurrentIntent, ScanIntents.BlackAndWhite))
			{
				intents.Add(ScanIntents.BlackAndWhite);
			}

			return intents;
		}


		private static IEnumerable<int> GetSupportedResolutions(IDevice device)
		{
			var item = device.Items[1];
			var supported = new List<int>();
			int[] testDPIs = { 75, 100, 150, 200, 300, 600, 1200 };

			foreach (int dpi in testDPIs)
			{
				try
				{
					if (item.Properties.Set(PropertyNames.HorizontalResolution, dpi) &&
						item.Properties.Set(PropertyNames.VerticalResolution, dpi))
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
		public string Scan()
		{
			var device = info.Connect();
			var item = device.Items[1];
			ImageFile file = null;

			try
			{
				item.Properties.Set(PropertyNames.HandlingSelect, false ? 0x01 : 0x02);
				item.Properties.Set(PropertyNames.HorizontalResolution, 300);
				item.Properties.Set(PropertyNames.VerticalResolution, 300);
				item.Properties.Set(PropertyNames.CurrentIntent, ScanIntents.Color);

				file = (ImageFile)item.Transfer(ScanFormatID.wiaFormatJPEG);
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
