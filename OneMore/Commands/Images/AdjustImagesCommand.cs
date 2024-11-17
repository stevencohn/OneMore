//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
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
	using SC = ImageEditor.SizeConstraint;


	/// <summary>
	/// Resize and adjust images on the page
	/// </summary>
	internal class AdjustImagesCommand : Command
	{
		private bool scopeFore;


		public AdjustImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			// find selected foreground images
			var elements = page.Root
				.Elements(ns + "Outline")
				.Descendants(ns + "Image")?
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (elements.Any())
			{
				scopeFore = true;
			}
			else
			{
				// else find selected background images
				elements = page.Root
					.Elements(ns + "Image")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.ToList();

				if (elements.Any())
				{
					scopeFore = false;
				}
				else
				{
					// else find all foreground images
					elements = page.Root
						.Elements(ns + "Outline")
						.Descendants(ns + "Image")
						.ToList();

					if (elements.Any())
					{
						scopeFore = true;
					}
					else
					{
						// else find all background images
						elements = page.Root.Elements(ns + "Image").ToList();
						scopeFore = false;
					}
				}
			}

			if (elements.Any())
			{
				var updated = elements.Count == 1
					// single selected image
					? ResizeOne(elements[0])
					// multiple selections or all if none selected
					: ResizeMany(elements, page);

				if (updated)
				{
					await one.Update(page);
				}
			}
			else
			{
				ShowError(Resx.AdjustImagesDialog_noImages);
			}
		}


		private bool ResizeOne(XElement element)
		{
			var wrapper = new OneImage(element);
			using var image = wrapper.ReadImage();

			using var dialog = new AdjustImagesDialog(image, wrapper.Width, wrapper.Height);
			var result = dialog.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				var editor = dialog.GetImageEditor(image);
				if (editor.IsReady || (editor.AutoSize && wrapper.IsSetByUser))
				{
					editor.Apply(wrapper);
					return true;
				}
			}

			return false;
		}


		private bool ResizeMany(List<XElement> elements, Page page)
		{
			using var dialog = new AdjustImagesDialog()
			{
				ForegroundImages = scopeFore,
				ImageCount = elements.Count
			};

			var result = dialog.ShowDialog(owner);
			if (result != DialogResult.OK)
			{
				return false;
			}

			var updated = false;
			foreach (var element in elements)
			{
				var wrapper = new OneImage(element);
				using var image = wrapper.ReadImage();

				var editor = dialog.GetImageEditor(image);
				if (editor.IsReady || (editor.AutoSize && wrapper.IsSetByUser))
				{
					// when pasting an image onto the page, width or height can be zero
					// OneNote ignores both if either is zero so we'll do the same...
					var viewWidth = wrapper.Width;
					if (viewWidth == 0)
					{
						viewWidth = image.Width;
					}

					if (editor.Constraint == SC.All ||
						(editor.Constraint == SC.OnlyShrink && viewWidth > editor.Size.Width) ||
						(editor.Constraint == SC.OnlyEnlarge && viewWidth < editor.Size.Width))
					{
						using var edit = editor.Apply(wrapper);
						updated = true;
					}
					else
					{
						logger.WriteLine("skipped image due to constraint: " +
							$"viewWidth:{viewWidth} size=[{wrapper.Width} x {wrapper.Width}]");
					}
				}
			}

			if (dialog.RepositionImages)
			{
				new StackBackgroundImagesCommand().StackImages(page);
				updated = true;
			}

			return updated;
		}
	}
}
