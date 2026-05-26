//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
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
		private static bool commandIsActive = false;
		private XNamespace ns;

		public StylizeImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				await using var one = new OneNote(out var page, out ns, OneNote.PageDetail.All);

				var foreElements = page.Root
					.Elements(ns + "Outline")
					.Descendants(ns + "Image")
					.ToList();

				var backElements = page.Root
					.Elements(ns + "Image")
					.ToList();

				if (!foreElements.Any() && !backElements.Any())
				{
					ShowError(Resx.StylizeImagesCommand_noImages);
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

				var count = 0;
				if (foreSelected.Any() && dialog.ApplyForeground)
				{
					Stylize(foreSelected, dialog.Style);
					count += foreSelected.Count;
					logger.WriteLine($"updated {foreSelected.Count} selected foreground images");
				}

				if (backSelected.Any() && dialog.ApplyBackground)
				{
					Stylize(backSelected, dialog.Style);
					count += backSelected.Count;
					logger.WriteLine($"updated {backSelected.Count} selected background images");
				}

				if (count == 0)
				{
					if (foreElements.Any() && dialog.ApplyForeground)
					{
						Stylize(foreElements, dialog.Style);
						count += foreElements.Count;
						logger.WriteLine($"updated {foreElements.Count} foreground images");
					}

					if (backElements.Any() && dialog.ApplyBackground)
					{
						Stylize(backElements, dialog.Style);
						count += backElements.Count;
						logger.WriteLine($"updated {backElements.Count} background images");
					}
				}

				if (count > 0)
				{
					logger.WriteLine($"updating {count} total images");
					await one.Update(page);
				}
			}
			finally
			{
				commandIsActive = false;
			}
		}


		private void Stylize(IEnumerable<XElement> elements, ImageEditor.Stylization style)
		{
			var editor = new ImageEditor { Style = style };

			foreach (var element in elements)
			{
				// printout images have XPS-linking attributes that prevent OneNote from
				// accepting rewritten pixel data; remove them to allow stylization
				element.Attributes("xpsFileIndex").Remove();
				element.Attributes("originalPageNumber").Remove();
				element.Attributes("isPrintOut").Remove();

				editor.Apply(new OneImage(element));
			}
		}
	}
}
