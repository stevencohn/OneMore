//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CropImageCommand : Command
	{
		private ApplicationManager manager;
		private Page page;
		private XNamespace ns;


		public CropImageCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage(PageInfo.piAll));
				ns = page.Namespace;

				var images = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if (images?.Count() == 1)
				{
					CropImage(images.First());
				}
				else
				{
					UIHelper.ShowError(Resx.CropImage_oneImage);
				}
			}
		}


		private void CropImage(XElement element)
		{
			var data = element.Element(ns + "Data");
			var binhex = Convert.FromBase64String(data.Value);
			using (var stream = new MemoryStream(binhex, 0, binhex.Length))
			{
				using (var image = Image.FromStream(stream))
				{
					var size = element.Element(ns + "Size");
					size.ReadAttributeValue("width", out float width, image.Width);
					size.ReadAttributeValue("height", out float height, image.Height);

					var scales = new SizeF(
						width / image.Width,
						height / image.Height
						);

					using (var dialog = new CropImageDialog(image))
					{
						var result = dialog.ShowDialog(owner);
						if (result == DialogResult.OK)
						{
							var bytes = (byte[])new ImageConverter()
								.ConvertTo(dialog.Image, typeof(byte[]));

							data.Value = Convert.ToBase64String(bytes);



							(float dpiX, float dpiY) = UIHelper.GetDpiValues();

							// the image may have a different resolution than the screen so combine both to compensate
							var scalingX = dpiX / image.HorizontalResolution;
							var scalingY = dpiY / image.VerticalResolution;

							var setWidth = (int)Math.Round(image.Width * scalingX);
							var setHeight = (int)Math.Round(image.Height * scalingY);


							setWidth = (int)Math.Round(dialog.Image.Width * scales.Width);
							setHeight = (int)Math.Round(dialog.Image.Height * scales.Height);


							logger.WriteLine(
								$"DONE dpi:({dpiX},{dpiY}) imgres:{image.HorizontalResolution}x{image.VerticalResolution} " +
								$"scaling:({scalingX},{scalingY}) imgsiz:{image.Width}x{image.Height} | " +
								$"oldsize:{width}x{height} setsiz:{setWidth}x{setHeight}"
								);


							// OneNote does it's own scaling so we need to adjust by using both
							// its scaling factor and the screen's High DPI scaling factor

							//(float factorX, float factorY) = UIHelper.GetScalingFactors();

							//float scaleX = image.Width / width / factorX;
							//float scaleY = image.Height / height / factorY;

							//var setWidth = Math.Round(dialog.Image.Width * scaleX);
							//var setHeight = Math.Round(dialog.Image.Height * scaleY);

							size.SetAttributeValue("width", $"{setWidth:0.0}");
							size.SetAttributeValue("height", $"{setHeight:0.0}");
							size.SetAttributeValue("isSetByUser", "true");

							//logger.WriteLine(
							//	$"FINAL factors:({factorX},{factorY}) oldsiz:{width}x{height} scales:({scaleX},{scaleY}) " +
							//	$"imgsiz:{image.Width}x{image.Height} setsiz:{setWidth}x{setHeight}"
							//	);

							manager.UpdatePageContent(page.Root);
						}
					}
				}
			}
		}
	}
}
