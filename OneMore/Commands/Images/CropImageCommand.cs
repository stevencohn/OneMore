//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#define xLogging

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class CropImageCommand : Command
	{
		private Page page;
		private XNamespace ns;


		public CropImageCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out page, out ns, OneNote.PageDetail.All);

			var images = page.Root.Descendants(ns + "Image")?
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (images == null || images.Count() > 1)
			{
				UIHelper.ShowError(Resx.CropImage_oneImage);
				return;
			}

			var image = images.First();
			if (image.Attributes().Any(a => a.Name == "isPrintOut"))
			{
				if (UIHelper.ShowQuestion(Resx.CropImageDialog_printout) != DialogResult.Yes)
				{
					return;
				}
			}

			await CropImage(one, image);
		}


		private async Task CropImage(OneNote one, XElement element)
		{
			var data = element.Element(ns + "Data");
			var binhex = Convert.FromBase64String(data.Value);

			using var stream = new MemoryStream(binhex, 0, binhex.Length);
			using var image = Image.FromStream(stream);

			var size = element.Element(ns + "Size");
			size.GetAttributeValue("width", out float width, image.Width);
			size.GetAttributeValue("height", out float height, image.Height);

			var scales = new SizeF(width / image.Width, height / image.Height);

			using var dialog = new CropImageDialog(image);
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var bytes = (byte[])new ImageConverter()
					.ConvertTo(dialog.Image, typeof(byte[]));

				data.Value = Convert.ToBase64String(bytes);

				var setWidth = (int)Math.Round(dialog.Image.Width * scales.Width);
				var setHeight = (int)Math.Round(dialog.Image.Height * scales.Height);
#if Logging
				logger.WriteLine(
					$"DONE crop:{dialog.Image.Width}x{dialog.Image.Height} " +
					$"scales:({scales.Width},{scales.Height}) " +
					$"oldsize:{width}x{height} setsiz:{setWidth}x{setHeight}"
					);
#endif
				size.SetAttributeValue("width", $"{setWidth:0.0}");
				size.SetAttributeValue("height", $"{setHeight:0.0}");
				size.SetAttributeValue("isSetByUser", "true");

				// when a document is printed to OneNote as a series of page images,
				// such as a PDF, then each image is added a top-level elements and
				// marked with XPS attributes. These attributes must be removed or
				// OneNote will prevent proper cropping

				element.Attributes("xpsFileIndex").Remove();
				element.Attributes("originalPageNumber").Remove();
				element.Attributes("isPrintOut").Remove();

				await one.Update(page);
			}
		}
	}
}
