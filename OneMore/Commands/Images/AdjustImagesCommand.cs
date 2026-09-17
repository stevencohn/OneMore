//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;
	using SC = ImageEditor.SizeConstraint;


	#region Wrappers
	internal class AdjustImageFromClipboardCommand : AdjustImagesCommand
	{
		public AdjustImageFromClipboardCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(true);
		}
	}
	internal class AdjustImagesOnPageCommand : AdjustImagesCommand
	{
		public AdjustImagesOnPageCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	internal class PastePresetWidthImageCommand : AdjustImagesCommand
	{
		public PastePresetWidthImageCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(true, true);
		}
	}
	#endregion Wrappers


	/// <summary>
	/// Resize and adjust images on the page
	/// </summary>
	internal class AdjustImagesCommand : Command
	{
		// estimated per-level indent at 100% display scaling; scaled by the current
		// monitor's DPI factor since the visual indent step grows with it
		private const int IndentUnit = 36;
		// floor so a deeply nested preset-width paste never collapses to something illegible
		private const int MinPresetWidth = 150;

		private bool scopeFore;
		private bool pasting;
		private bool presetOnly;


		public AdjustImagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.All);

			pasting = args.Length > 0 && args[0] is bool b && b;
			presetOnly = args.Length > 1 && args[1] is bool p && p;

			var elements = pasting
				? await PreparePastingElement(page, ns)
				: FindOnPageElements(page, ns);


			if (elements is not null && elements.Any())
			{
				var updated = elements.Count == 1
					// single selected image
					? ResizeOne(elements[0])
					// multiple selections or all if none selected
					: ResizeMany(elements, page);

				if (updated)
				{
					// must force update if any embedded images on the background, such as
					// PowerPoint slides, as OneNote will complain about invalid XML otherwise
					var embedded = elements.Any(e =>
						e.Attribute("xpsFileIndex") is not null ||
						e.Attribute("originalPageNumber") is not null ||
						e.Attribute("isPrintOut") is not null);

					logger.WriteLine($"embedded:{embedded}");
					await one.Update(page, force: embedded);
				}
			}
			else if (pasting)
			{
				ShowError(Resx.AdjustImagesDialog_noImageToPaste);
			}
			else
			{
				ShowError(Resx.AdjustImagesDialog_noImages);
			}
		}


		private async Task<List<XElement>> PreparePastingElement(Page page, XNamespace ns)
		{
			var image = await ClipboardProvider.GetImage();

			if (image is null)
			{
				logger.WriteLine("no clipboard image found");
				return default;
			}

			var element =
				new XElement(ns + "Image",
					new XAttribute(XNamespace.Xmlns + OneNote.Prefix, ns),
					new XAttribute("selected", "all"),
					new XElement(ns + "Size",
						new XAttribute("width", $"{image.Width:00}"),
						new XAttribute("height", $"{image.Height:00}"),
						new XAttribute("isSetByUser", "true")),
					new XElement(ns + "Data", image.ToBase64String())
				);

			if (FindEmptyCursorParagraph(page, ns) is XElement paragraph)
			{
				// cursor is a blinking caret in an otherwise empty paragraph; insert directly
				// into it rather than going through anchor/extraction math, which anchors
				// relative to sibling paragraphs and, once a paragraph is indented into its
				// own nested OEChildren with no siblings, lands before it instead of in it
				paragraph.Elements().Where(e => e.Name.LocalName != "List").Remove();
				paragraph.Add(element);
				return new List<XElement> { element };
			}

			var editor = new PageEditor(page);
			editor.ExtractSelectedContent(breakParagraph: true);

			var content = new XElement(ns + "OE", element);

			if (editor.Anchor.Name.LocalName.In("OE", "HTMLBlock"))
			{
				editor.Anchor.AddAfterSelf(content);
			}
			else // if (localName.In("OEChildren", "Outline"))
			{
				editor.Anchor.AddFirst(content);
			}

			return new List<XElement> { element };
		}


		// Finds the OE containing the cursor when it is nothing more than a blinking caret
		// in an empty paragraph (no real text/content), so the image can be inserted directly
		// into that paragraph. Returns null for anything else (real selected text/content, no
		// selection found at all, etc.) so the caller falls back to the normal extract/anchor
		// path, which already handles those cases correctly.
		private static XElement FindEmptyCursorParagraph(Page page, XNamespace ns)
		{
			var marked = page.Root.Elements(ns + "Outline").Descendants(ns + "OE")
				.Elements()
				.Where(e => e.Attribute("selected")?.Value == "all")
				.ToList();

			if (marked.Count != 1 || marked[0].Name.LocalName != "T" ||
				marked[0].GetCData().Value != string.Empty)
			{
				return null;
			}

			var paragraph = marked[0].Parent;
			var content = paragraph.Elements().Where(e => e.Name.LocalName != "List").ToList();

			return content.Count == 1 ? paragraph : null;
		}


		private List<XElement> FindOnPageElements(Page page, XNamespace ns)
		{
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

			return elements;
		}


		private bool ResizeOne(XElement element)
		{
			var wrapper = new OneImage(element);
			using var image = wrapper.ReadImage();

			if (presetOnly)
			{
				var collection = new SettingsProvider().GetCollection("images");
				if (collection.Contains("mruWidth"))
				{
					ApplyPresetWidth(element, wrapper, image, collection.Get("mruWidth", 500));
					return true;
				}
			}

			using var dialog = new AdjustImagesDialog(image, wrapper.Width, wrapper.Height, presetOnly);
			var result = dialog.ShowDialog(owner);
			if (result == DialogResult.OK)
			{
				var editor = dialog.GetImageEditor(image);
				if (pasting || editor.IsReady || (editor.AutoSize && wrapper.IsSetByUser))
				{
					editor.Apply(wrapper);
					return true;
				}
			}

			return false;
		}


		// Applies the stored preset width directly to the wrapper, estimating a reduction
		// for indented insertion points since OneNote does not expose their rendered width.
		private static void ApplyPresetWidth(XElement element, OneImage wrapper, Image image, int width)
		{
			// IndentLevel counts every OEChildren between the element and its nearest
			// Outline/Cell, including the one baseline OEChildren that even a non-indented
			// OE always sits in, so subtract 1 to get the actual (relative) indent depth
			var indent = Math.Max(0, PageEditor.IndentLevel(element) - 1);

			//var (scaleX, _) = UI.Scaling.GetScalingFactors();
			var scaleX = 1.0f;
			var reduction = (int)Math.Round(indent * IndentUnit * scaleX);

			var adjusted = Math.Max(MinPresetWidth, width - reduction);

			if (adjusted >= image.Width)
			{
				// never enlarge an image smaller than the preset; keep its original view size
				wrapper.SetSize(image.Width, image.Height, true);
				return;
			}

			var height = (int)Math.Round(image.Height * ((double)adjusted / image.Width));
			wrapper.SetSize(adjusted, height, true);
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
