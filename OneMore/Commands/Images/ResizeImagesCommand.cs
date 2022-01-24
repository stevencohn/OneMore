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
				var elements = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if ((elements == null) || !elements.Any())
				{
					elements = page.Root.Descendants(ns + "Image")
						.Where(e => e.Attribute("backgroundImage")?.Value != "true");
				}

				if (elements != null)
				{
					if (elements.Count() == 1)
					{
						// resize single selected image only
						await ResizeOne(elements.First());
					}
					else
					{
						// select many iamges, or all if none selected
						await ResizeMany(elements);
					}
				}
			}
		}


		private async Task ResizeOne(XElement element)
		{
			var size = element.Element(ns + "Size");
			int viewWidth = (int)decimal.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);
			int viewHeight = (int)decimal.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

			using (var image = ReadImage(element))
			{
				using (var dialog = new ResizeImagesDialog(image, viewWidth, viewHeight))
				{
					var result = dialog.ShowDialog(owner);
					if (result == DialogResult.OK)
					{
						if (!dialog.PreserveSize)
						{
							using (var data = dialog.GetImage())
							{
								if (data != null)
								{
									WriteImage(element, data);
								}
							}
						}

						size.SetAttributeValue("width", dialog.WidthPixels.ToString(CultureInfo.InvariantCulture));
						size.SetAttributeValue("height", dialog.HeightPixels.ToString(CultureInfo.InvariantCulture));
						size.SetAttributeValue("isSetByUser", "true");

						await one.Update(page);
					}
				}
			}
		}


		private async Task ResizeMany(IEnumerable<XElement> elements)
		{
			using (var dialog = new ResizeImagesDialog())
			{
				var result = dialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					var width = dialog.WidthPixels.ToString(CultureInfo.InvariantCulture);

					foreach (var element in elements)
					{
						var size = element.Element(ns + "Size");

						var imageWidth = (int)decimal.Parse(
							size.Attribute("width").Value, CultureInfo.InvariantCulture);

						var imageHeight = (int)decimal.Parse(
							size.Attribute("height").Value, CultureInfo.InvariantCulture);

						var height = (int)(imageHeight * (dialog.WidthPixels / imageWidth));

						if (!dialog.PreserveSize)
						{
							using (var image = ReadImage(element))
							{
								using (var data = image.Resize((int)dialog.WidthPixels, height, dialog.Quality))
								{
									WriteImage(element, data);
								}
							}
						}

						size.SetAttributeValue("width",  width);
						size.SetAttributeValue("height", height.ToString(CultureInfo.InvariantCulture));
						size.SetAttributeValue("isSetByUser", "true");
					}

					await one.Update(page);
				}
			}
		}


		private Image ReadImage(XElement image)
		{
			var data = Convert.FromBase64String(image.Element(ns + "Data").Value);
			using (var stream = new MemoryStream(data, 0, data.Length))
			{
				return Image.FromStream(stream);
			}
		}



		private void WriteImage(XElement element, Image image)
		{
			var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));

			var data = element.Element(ns + "Data");
			data.Value = Convert.ToBase64String(bytes);
		}
	}
}
