//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Compress images on page without losing quality, by converting PNG to JPEG
	/// </summary>
	internal class CompressImagesCommand : Command
	{
		public CompressImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All))
			{
				var elements = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if ((elements == null) || !elements.Any())
				{
					// include background images
					elements = page.Root.Descendants(ns + "Image");
				}

				var count = 0;
				var delta = 0;

				if (elements.Any())
				{
					foreach (var element in elements)
					{
						// convert base64 to image
						var dataElement = element.Element(ns + "Data");
						var size = dataElement.Value.Length;

						var data = Convert.FromBase64String(dataElement.Value);
						using var input = new MemoryStream(data, 0, data.Length);
						using var image = Image.FromStream(input);

						if (image.GetSignature() != ImageSignature.JPG)
						{
							// convert to JPG
							using var output = new MemoryStream();
							image.Save(output, ImageFormat.Jpeg);
							using var jpg = Image.FromStream(output);

							var value = jpg.ToBase64String();
							if (value.Length < size)
							{
								dataElement.Value = value;

								delta += size - value.Length;
								count++;
							}
						}
					}

					if (count > 0)
					{
						await one.Update(page);
					}
				}

				if (count > 0)
				{
					UI.MoreBubbleWindow.Show(string.Format(
						Resx.ConvertImagesCommand_Converted, count, delta));
				}
				else
				{
					UI.MoreBubbleWindow.Show(Resx.ConvertImagesCommand_NoImages);
				}
			}
		}
	}
}
