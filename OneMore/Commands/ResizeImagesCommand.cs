//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
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
				var page = new Page(manager.CurrentPage());
				var ns = page.Namespace;

				var size = page.Root.Descendants(ns + "Image")?
					.Where(e => e.Attribute("selected")?.Value == "all")
					.FirstOrDefault()?
					.Element(ns + "Size");

				if (size != null)
				{
					// resize selected image only

					int width = (int)decimal.Parse(size.Attribute("width").Value);
					int height = (int)decimal.Parse(size.Attribute("height").Value);

					using (var dialog = new ResizeImagesDialog(width, height))
					{
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
