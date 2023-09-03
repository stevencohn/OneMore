//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Compress images on page by resizing down to view width and height.
	/// Quality will be compromised.
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
						var data = element.Element(ns + "Data");
						var dataLen = data.Value.Length;

						var bytes = Convert.FromBase64String(data.Value);
						using var input = new MemoryStream(bytes, 0, bytes.Length);
						using var image = Image.FromStream(input);

						var size = element.Element(ns + "Size");
						int viewWidth = (int)decimal.Parse(
							size.Attribute("width").Value, CultureInfo.InvariantCulture);
						int viewHeight = (int)decimal.Parse(
							size.Attribute("height").Value, CultureInfo.InvariantCulture);

						if (image.Width > viewWidth || image.Height > viewHeight)
						{
							using var resized = image.Resize(viewWidth, viewHeight, false);

							var value = resized.ToBase64String();
							if (value.Length < dataLen)
							{
								logger.WriteLine($"compressed {dataLen} to {value.Length}");
								data.Value = value;

								delta += dataLen - value.Length;
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
