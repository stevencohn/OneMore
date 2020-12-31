//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ResizeImagesCommand : Command
	{
		private OneNote one;
		private Page page;
		private XNamespace ns;


		public ResizeImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out page, out ns, OneNote.PageDetail.All))
			{
				var images = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if ((images == null) || !images.Any())
				{
					images = page.Root.Descendants(ns + "Image");
				}

				if (images != null)
				{
					if (images.Count() == 1)
					{
						// resize single selected image only
						await ResizeOne(images.First());
					}
					else
					{
						// select many iamges, or all if none selected
						await ResizeMany(images);
					}
				}
			}
		}


		private static Size GetOriginalSize(XElement image, XNamespace ns)
		{
			var data = Convert.FromBase64String(image.Element(ns + "Data").Value);
			using (var stream = new MemoryStream(data, 0, data.Length))
			{
				using (var img = Image.FromStream(stream))
				{
					return new Size
					{
						Width = img.Width,
						Height = img.Height
					};
				}
			}
		}


		private async Task ResizeOne(XElement image)
		{
			var size = image.Element(ns + "Size");
			int width = (int)decimal.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);
			int height = (int)decimal.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

			using (var dialog = new ResizeImagesDialog(width, height))
			{
				dialog.SetOriginalSize(GetOriginalSize(image, ns));

				var result = dialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					size.Attribute("width").Value = dialog.WidthPixels.ToString(CultureInfo.InvariantCulture);
					size.Attribute("height").Value = dialog.HeightPixels.ToString(CultureInfo.InvariantCulture);

					await one.Update(page);
				}
			}
		}


		private async Task ResizeMany(IEnumerable<XElement> images)
		{
			using (var dialog = new ResizeImagesDialog(1, 1, true))
			{
				var result = dialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					var width = dialog.WidthPixels.ToString(CultureInfo.InvariantCulture);

					foreach (var image in images)
					{
						var size = image.Element(ns + "Size");
						var imageWidth = (int)decimal.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);
						var imageHeight = (int)decimal.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

						size.Attribute("width").Value = width;

						size.Attribute("height").Value =
							((int)(imageHeight * (dialog.WidthPixels / imageWidth))).ToString(CultureInfo.InvariantCulture);
					}

					await one.Update(page);
				}
			}
		}
	}
}
