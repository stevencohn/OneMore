//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using WIA;


	internal class ScannerManager
	{
		private readonly DeviceInfo device;


		public ScannerManager(DeviceInfo device)
		{
			this.device = device;
		}


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
		public static List<DeviceInfo> ListScannerDevices()
		{
			var devices = new List<DeviceInfo>();
			var manager = new DeviceManager();

			for (int i = 1; i <= manager.DeviceInfos.Count; i++)
			{
				var info = manager.DeviceInfos[i];
				if (info.Type == WiaDeviceType.ScannerDeviceType)
				{
					devices.Add(info);
				}
			}

			return devices;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="format"></param>
		/// <param name="useFeeder"></param>
		/// <returns></returns>
		public string Scan(string format = ScanFormatID.wiaFormatJPEG, bool useFeeder = false)
		{
			var scanner = device.Connect().Items[1];

			//Set(scanner.Properties, "Document Handling Select", useFeeder ? 0x01 : 0x02); // 0x01 = Feeder, 0x02 = Flatbed
			Set(scanner.Properties, "Horizontal Resolution", 300);
			Set(scanner.Properties, "Vertical Resolution", 300);
			Set(scanner.Properties, "Current Intent", ScanIntents.Color);

			var imageFile = (ImageFile)scanner.Transfer(format);
			var bytes = (byte[])imageFile.FileData.get_BinaryData();

			using var stream = new MemoryStream(bytes);
			using var image = Image.FromStream(stream);

			ImageHeight = image.Height;
			ImageWidth = image.Width;

			return Convert.ToBase64String(bytes);
		}


		/// <summary>
		/// Sets a property value by name or ID.
		/// </summary>
		private static void Set(IProperties properties, /* boxed */ object key, object value)
		{
			foreach (Property property in properties)
			{
				if (property.PropertyID.Equals(key) || property.Name.Equals(key))
				{
					property.set_Value(value);
					return;
				}
			}
		}
	}
}
