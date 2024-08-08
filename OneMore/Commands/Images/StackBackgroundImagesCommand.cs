//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Repositions background images vertically on the page with minimal whitespace
	/// </summary>
	internal class StackBackgroundImagesCommand : Command
	{
		private const int ImageMargin = 15;

		private OneNote one;
		private IEnumerable<XElement> images = null;


		public StackBackgroundImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using (one = new OneNote(out var page, out var ns, OneNote.PageDetail.All))
			{
				images = page.Root.Elements(ns + "Image");
				if ((!images.Any() || images.Count() < 2))
				{
					ShowError(Resx.StackBackgroundImagesCommand_noImages);
					return;
				}

				if (StackImages(page))
				{
					await one.Update(page);
				}
			}
		}


		/// <summary>
		/// Entry point for AdjustImagesCommand so this functionality can be shared
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public bool StackImages(Page page)
		{
			var ns = page.Namespace;

			if (images == null)
			{
				images = page.Root.Elements(ns + "Image");
				if (!images.Any() || images.Count() < 2)
				{
					return false;
				}
			}

			var top = int.MinValue;
			var updated = false;

			foreach (var image in images)
			{
				var position = image.Element(ns + "Position");
				if (top == int.MinValue)
				{
					// presume the first one is at the correct top position
					top = (int)position.GetAttributeDouble("y");
				}
				else
				{
					var y = (int)position.GetAttributeDouble("y");
					if (y != top)
					{
						position.SetAttributeValue("y", top.ToString(CultureInfo.InvariantCulture));
						updated = true;
					}
				}

				var size = image.Element(ns + "Size");
				var height = (int)size.GetAttributeDouble("height");
				top += height + ImageMargin;
			}

			return updated;
		}
	}
}
