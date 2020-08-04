//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class ResizeImagesCommand : Command
	{
		private ApplicationManager manager;
		private Page page;
		private XNamespace ns;


		public ResizeImagesCommand() : base()
		{
		}


		public void Execute()
		{
			using (manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage(Microsoft.Office.Interop.OneNote.PageInfo.piAll));
				ns = page.Namespace;

				var images = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if ((images == null) || (images.Count() == 0))
				{
					images = page.Root.Descendants(ns + "Image");
				}

				if (images != null)
				{
					if (images.Count() == 1)
					{
						// resize single selected image only
						ResizeOne(images.First());
					}
					else
					{
						// select many iamges, or all if none selected
						ResizeMany(images);
					}
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


		private void ResizeOne(XElement image)
		{
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


		private void ResizeMany(IEnumerable<XElement> images)
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

					manager.UpdatePageContent(page.Root);
				}
			}
		}
	}
}
