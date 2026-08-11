//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Finds all images locked to the page background and moves them into the last
	/// outline on the page, in document order, top-to-bottom then left-to-right.
	/// </summary>
	internal class RestoreBackgroundImagesCommand : Command, ICliPageCommand
	{
		private const double MinOutlineWidth = 600.0;


		public RestoreBackgroundImagesCommand()
		{
		}


		#region CLI Implementation

		public string CommandName => "RestoreBackgroundImages";

		public string Description => "Move page-background images into the page body";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook", required: true)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page", required: false);

		#endregion CLI Implementation


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("pageId", out string pageId);
				if (string.IsNullOrWhiteSpace(pageId)) { return; }
				await using var one = new OneNote();
				var page = await one.GetPage(pageId, OneNote.PageDetail.All);
				await Run(one, page, page.Namespace);
				return;
			}

			await using var ribbon = new OneNote(out var rpage, out var rns, OneNote.PageDetail.All);
			await Run(ribbon, rpage, rns);
		}


		private async Task Run(OneNote one, Page page, XNamespace ns)
		{
			var images = page.Root.Elements(ns + "Image").ToList();
			if (!images.Any())
			{
				ShowInfo(Resx.RestoreBackgroundImagesCommand_noImages);
				return;
			}

			// top-to-bottom, then left-to-right
			var ordered = images
				.OrderBy(i => i.Element(ns + "Position") is XElement p ? p.GetAttributeDouble("y") : 0)
				.ThenBy(i => i.Element(ns + "Position") is XElement p ? p.GetAttributeDouble("x") : 0)
				.ToList();

			// full copies of the untouched originals, kept in case something goes wrong
			// after the real background images have already been deleted from OneNote
			var backups = ordered.Select(i => new XElement(i)).ToList();

			var container = FindLastContainer(page, ns);
			var objectIDs = new List<string>();

			foreach (var image in ordered)
			{
				var objectID = image.Attribute("objectID")?.Value;
				if (!string.IsNullOrEmpty(objectID))
				{
					objectIDs.Add(objectID);
				}

				// Position is meaningless once flowed into the outline body. And the
				// omHash bookkeeping attribute that Page stamps on every 1st-gen child
				// when the page loads is only ever stripped from 1st-gen children before
				// saving; once this image is nested inside the outline it's no longer
				// 1st-gen so that stripping never happens, and OneNote's schema validator
				// rejects the unrecognized attribute
				image.Element(ns + "Position")?.Remove();
				image.Attribute("omHash")?.Remove();

				// when a document such as a PDF is printed to OneNote, each page is added
				// as a top-level background image marked with these XPS attributes; as
				// with cropping (see CropImageCommand), OneNote silently refuses to accept
				// any edit to such an image - including moving it into the outline body -
				// unless these markers are removed first. No exception or schema error is
				// raised; the edit is just dropped, which is how the image gets lost
				image.Attributes("xpsFileIndex").Remove();
				image.Attributes("originalPageNumber").Remove();
				image.Attributes("isPrintOut").Remove();

				image.Remove();
				container.Add(new XElement(ns + "OE", image));
			}

			// validate before deleting anything from OneNote so a bad update can never
			// destroy an image we have no way to bring back
			page.OptimizeForSave(false);
			var errors = new List<string>();
			if (!OneNote.ValidateSchema(page.Root, errors))
			{
				logger.WriteLine(
					$"aborting, restored page would fail validation:{Environment.NewLine}" +
					string.Join(Environment.NewLine, errors));

				ShowError(Resx.RestoreBackgroundImagesCommand_invalid);
				return;
			}

			try
			{
				// background images live outside the normal OE content model so simply
				// omitting them from the updated XML isn't enough to remove them; each
				// one must be explicitly deleted by its objectID
				foreach (var objectID in objectIDs)
				{
					one.DeleteContent(page.PageId, objectID);
				}

				await one.Update(page);
			}
			catch (Exception exc)
			{
				logger.WriteLine("error restoring background images, restoring originals", exc);

				foreach (var backup in backups)
				{
					// this element was hashed as a 1st-gen child when the page was loaded;
					// both the stale hash and its now-deleted objectID must go so OneNote
					// treats it as new content to add rather than unchanged content to skip
					backup.Attribute("omHash")?.Remove();
					backup.Attribute("objectID")?.Remove();
					page.Root.Add(backup);
				}

				try
				{
					await one.Update(page);
					ShowError(Resx.RestoreBackgroundImagesCommand_restoredAfterError);
				}
				catch (Exception exc2)
				{
					logger.WriteLine("failed to restore background images after error", exc2);
					ShowError(Resx.RestoreBackgroundImagesCommand_lostImages);
				}
			}
		}


		private static XElement FindLastContainer(Page page, XNamespace ns)
		{
			var outline = page.BodyOutlines.LastOrDefault();
			XElement container;

			if (outline is null)
			{
				container = page.EnsureContentContainer();
				outline = container.Parent;
			}
			else
			{
				container = outline.Elements(ns + "OEChildren").LastOrDefault();
				if (container is null)
				{
					container = new XElement(ns + "OEChildren");
					outline.Add(container);
				}
			}

			EnsureMinimumWidth(outline, ns);

			return container;
		}


		/// <summary>
		/// Widens the outline if needed so restored images, which can be much wider than
		/// a narrow existing outline (e.g. a printed PDF page), aren't squeezed down
		/// </summary>
		private static void EnsureMinimumWidth(XElement outline, XNamespace ns)
		{
			var size = outline.Element(ns + "Size");
			if (size is null)
			{
				size = new XElement(ns + "Size",
					new XAttribute("width", FormattableString.Invariant($"{MinOutlineWidth:0.0}")));

				// Size must follow Position per PageObject schema
				var position = outline.Element(ns + "Position");
				if (position is null)
				{
					outline.AddFirst(size);
				}
				else
				{
					position.AddAfterSelf(size);
				}
			}
			else if (size.GetAttributeDouble("width") < MinOutlineWidth)
			{
				size.SetAttributeValue("width", FormattableString.Invariant($"{MinOutlineWidth:0.0}"));
			}
		}
	}
}
