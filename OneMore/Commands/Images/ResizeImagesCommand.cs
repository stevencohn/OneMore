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
	using Resx = Properties.Resources;


	/// <summary>
	/// Resize and adjust images on the page
	/// </summary>
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
					// starting at Outline should exclude all background images
					elements = page.Root.Elements(ns + "Outline").Descendants(ns + "Image");
				}

				if (elements != null && elements.Any())
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
				else
				{
					UIHelper.ShowMessage(Resx.ResizeImagesDialog_noImages);
				}
			}
		}


		private async Task ResizeOne(XElement element)
		{
			var size = element.Element(ns + "Size");
			int viewWidth = (int)decimal.Parse(size.Attribute("width").Value, CultureInfo.InvariantCulture);
			int viewHeight = (int)decimal.Parse(size.Attribute("height").Value, CultureInfo.InvariantCulture);

			using var image = ReadImage(element);

			// when pasting an image onto the page, width or height can be zero
			// OneNote ignores both if either is zero so we'll do the same...
			if (viewWidth == 0 || viewHeight == 0)
			{
				viewWidth = image.Width;
				viewHeight = image.Height;
			}

			using var dialog = new ResizeImagesDialog(image, viewWidth, viewHeight);
			var result = dialog.ShowDialog();
			if (result != DialogResult.OK)
			{
				return;
			}

			if (dialog.NeedsRewrite)
			{
				using var data = dialog.GetImage();
				if (data != null)
				{
					WriteImage(element, data);
				}
			}

			size.SetAttributeValue("width", dialog.ImageWidth.ToString(CultureInfo.InvariantCulture));
			size.SetAttributeValue("height", dialog.ImageHeight.ToString(CultureInfo.InvariantCulture));
			size.SetAttributeValue("isSetByUser", "true");

			logger.WriteLine($"resized from {viewWidth} x {viewHeight} to {dialog.ImageWidth} x {dialog.ImageHeight}");

			await one.Update(page);
		}


		private async Task ResizeMany(IEnumerable<XElement> elements)
		{
			using var dialog = new ResizeImagesDialog();
			var result = dialog.ShowDialog();
			if (result != DialogResult.OK)
			{
				return;
			}

			foreach (var element in elements)
			{
				using var image = ReadImage(element);
				var size = element.Element(ns + "Size");

				var viewWidth = (int)decimal.Parse(
					size.Attribute("width").Value, CultureInfo.InvariantCulture);

				var viewHeight = (int)decimal.Parse(
					size.Attribute("height").Value, CultureInfo.InvariantCulture);

				// when pasting an image onto the page, width or height can be zero
				// OneNote ignores both if either is zero so we'll do the same...
				if (viewWidth == 0 || viewHeight == 0)
				{
					viewWidth = image.Width;
					viewHeight = image.Height;
				}

				int width, height;
				if (dialog.Percent > 0)
				{
					width = (int)(viewWidth * (dialog.Percent / 100));
					height = (int)(viewHeight * (dialog.Percent / 100));
				}
				else
				{
					width = (int)dialog.ImageWidth;

					height = dialog.LockAspect
						? (int)(viewHeight * (dialog.ImageWidth / viewWidth))
						: (int)dialog.ImageHeight;
				}

				if (dialog.ResizeOption == ResizeOption.All ||
					(dialog.ResizeOption == ResizeOption.OnlyShrink && viewWidth > width) ||
					(dialog.ResizeOption == ResizeOption.OnlyEnlarge && viewWidth < width))
				{
					if (dialog.NeedsRewrite)
					{
						Image data = null;
						try
						{
							data = dialog.Adjust(image.Resize(width, height));
							WriteImage(element, data);
						}
						finally
						{
							data?.Dispose();
						}
					}

					size.SetAttributeValue("width", width.ToString(CultureInfo.InvariantCulture));
					size.SetAttributeValue("height", height.ToString(CultureInfo.InvariantCulture));
					size.SetAttributeValue("isSetByUser", "true");

					logger.WriteLine($"resized from {viewWidth} x {viewHeight} to {width} x {height}");
				}
				else
				{
					logger.WriteLine($"skipped image with size {viewWidth} x {viewHeight}");
				}
			}

			await one.Update(page);
		}


		private Image ReadImage(XElement image)
		{
			var data = Convert.FromBase64String(image.Element(ns + "Data").Value);
			using var stream = new MemoryStream(data, 0, data.Length);
			return Image.FromStream(stream);
		}



		private void WriteImage(XElement element, Image image)
		{
			var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));

			var data = element.Element(ns + "Data");
			data.Value = Convert.ToBase64String(bytes);
		}
	}
}
