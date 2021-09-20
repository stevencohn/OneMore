//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S1075 // URIs should not be hardcoded

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertQRCommand : Command
	{
		private const string GetUri = "http://chart.apis.google.com/chart?cht=qr&chs={1}x{1}&chl={0}";
		private const int Size = 250;
		private const int MaxLength = 2048;


		public InsertQRCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var text = page.GetSelectedText();

				if (text.Length == 0)
				{
					UIHelper.ShowMessage(Resx.InsertQRCommand_NoSelection);
					return;
				}

				var url = string.Format(GetUri, HttpUtility.HtmlEncode(text), Size);

				if (url.Length > MaxLength)
				{
					var max = MaxLength - GetUri.Length;
					UIHelper.ShowMessage(string.Format(Resx.InsertQRCommand_MaxLength, max));
					return;
				}

				var image = await GetQRCodeImage(url);

				var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));
				var data = Convert.ToBase64String(bytes);

				(float factorX, float factorY) = UIHelper.GetScalingFactors();
				var scaledX = Size / factorX;
				var scaledY = Size / factorY;

				var content = new XElement(ns + "Image",
					new XAttribute("alt", "QR Code"),
					new XElement(ns + "Size",
						new XAttribute("width", $"{scaledX:0.0}"),
						new XAttribute("height", $"{scaledY:0.0}"),
						new XAttribute("isSetByUser", "true")
						),
					new XElement(ns + "Data",
						data
						)
					);

				page.AddNextParagraph(content);
				await one.Update(page);
			}
		}


		public async Task<Image> GetQRCodeImage(string url)
		{
			var client = HttpClientFactory.Create();

			using (var response = await client.GetAsync(url))
			{
				if (response.IsSuccessStatusCode)
				{
					using (var stream = await response.Content.ReadAsStreamAsync())
					{
						return Image.FromStream(stream);
					}
				}
			}

			return null;
		}
	}
}
