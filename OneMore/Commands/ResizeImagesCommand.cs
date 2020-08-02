//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Core;
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ResizeImagesCommand : Command
	{
		public ResizeImagesCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage(Microsoft.Office.Interop.OneNote.PageInfo.piAll));
				var ns = page.Namespace;

				var image = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all")
					.FirstOrDefault();

				if (image != null)
				{
					// resize selected image only

					var size = image.Element(ns + "Size");
					int width = (int)decimal.Parse(size.Attribute("width").Value);
					int height = (int)decimal.Parse(size.Attribute("height").Value);

					using (var dialog = new ResizeImagesDialog(width, height))
					{
						dialog.SetOriginalSize(GetOriginalSize(image, ns));

						var result = dialog.ShowDialog(owner);
						if (result == DialogResult.OK)
						{
							size.Attribute("width").Value = dialog.WidthPixels.ToString();
							size.Attribute("height").Value = dialog.HeightPixels.ToString();

							manager.UpdatePageContent(page.Root);
						}
					}
				}
				else
				{
					// no selected image so resize all
					ResizeAllImages(page.Root, ns, manager);
				}
			}
		}

		private Size GetOriginalSize(XElement image, XNamespace ns)
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


		private void ResizeAllImages(XElement root, XNamespace ns, ApplicationManager manager)
		{
			var images = root.Descendants(ns + "Image");
			if (images?.Count() > 0)
			{
				using (var dialog = new ResizeImagesDialog(1, 1, true))
				{
					var result = dialog.ShowDialog(owner);
					if (result == DialogResult.OK)
					{
						var width = dialog.WidthPixels.ToString();

						foreach (var image in images)
						{
							var size = image.Element(ns + "Size");
							var imageWidth = (int)decimal.Parse(size.Attribute("width").Value);
							var imageHeight = (int)decimal.Parse(size.Attribute("height").Value);

							size.Attribute("width").Value = width;

							size.Attribute("height").Value = 
								((int)(imageHeight * (dialog.WidthPixels / imageWidth))).ToString();
						}

						manager.UpdatePageContent(root);
					}
				}
			}
		}
	}
}
