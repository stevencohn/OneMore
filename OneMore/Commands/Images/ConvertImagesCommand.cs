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
	/// Convert images, mainly PNG, to JPEG to compress storage requirement of page
	/// </summary>
	internal class ConvertImagesCommand : Command
	{
		public ConvertImagesCommand()
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

				if (elements.Any())
				{
					var count = 0;
					foreach (var element in elements)
					{
						// convert base64 to image
						var dataElement = element.Element(ns + "Data");
						var data = Convert.FromBase64String(dataElement.Value);
						using var input = new MemoryStream(data, 0, data.Length);
						using var image = Image.FromStream(input);

						if (image.GetSignature() != ImageSignature.JPG)
						{
							using var output = new MemoryStream();
							image.Save(output, ImageFormat.Jpeg);

							dataElement.Value = image.ToBase64String();

							count++;
						}
					}

					if (count > 0)
					{
						await one.Update(page);

						UI.MoreBubbleWindow.Show(string.Format(
							Resx.ConvertImagesCommand_Converted, count));
					}
					else
					{
						UI.MoreBubbleWindow.Show(Resx.ConvertImagesCommand_NoImages);
					}
				}
			}
		}
	}
}
