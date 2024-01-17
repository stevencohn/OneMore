//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Resize and adjust images on the page
	/// </summary>
	internal class StylizeImagesCommand : Command
	{
		private XNamespace ns;

		public StylizeImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out ns, OneNote.PageDetail.All);

			var foreElements = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "Image")
				.ToList();

			var backElements = page.Root
				.Elements(ns + "Image")
				.ToList();

			if (!foreElements.Any() && !backElements.Any())
			{
				UIHelper.ShowMessage(Resx.StylizeImagesCommand_noImages);
				return;
			}

			var foreSelected = foreElements
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			var backSelected = backElements
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			using var dialog = new StylizeImagesDialog(
				foreElements.Count, foreSelected.Count,
				backElements.Count, backSelected.Count);

			var result = dialog.ShowDialog(owner);
			if (result != DialogResult.OK)
			{
				return;
			}

			var updated = false;
			if (foreSelected.Any() && dialog.ApplyForeground)
			{
				Stylize(foreSelected, dialog.Style);
				updated = true;
			}

			if (backSelected.Any() && dialog.ApplyBackground)
			{
				Stylize(backSelected, dialog.Style);
				updated = true;
			}

			if (!updated)
			{
				if (foreElements.Any() && dialog.ApplyForeground)
				{
					Stylize(foreElements, dialog.Style);
					updated = true;
				}

				if (backElements.Any() && dialog.ApplyBackground)
				{
					Stylize(backElements, dialog.Style);
					updated = true;
				}
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		private void Stylize(
			IEnumerable<XElement> elements, StylizeImagesDialog.ImageStyle style)
		{
			foreach (var element in elements)
			{
				using var image = ReadImage(element);

				var stylized = style switch
				{
					StylizeImagesDialog.ImageStyle.GrayScale => image.ToGrayscale(),
					StylizeImagesDialog.ImageStyle.Sepia => image.ToSepia(),
					StylizeImagesDialog.ImageStyle.Polaroid => image.ToPolaroid(),
					_ => image.Inverted()
				};

				WriteImage(element, stylized);
			}
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
