//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Adds a caption below each selected attachment on the page showing the full name of
	/// the attachment.
	/// </summary>
	internal class CaptionAttachmentsCommand : Command
	{
		private XNamespace ns;


		public CaptionAttachmentsCommand()
		{
		}


		// <one:InsertedFile
		//		pathCache="C:\Users\steve\AppData\Local\Microsoft\OneNote\16.0\cache\000007LE.bin"
		//		pathSource="C:\Users\steve\OneDrive\Desktop\Steven Cohn 091621 3942 Leaf Blaster .pdf"
		//		preferredName="Steven Cohn 091621 3942 Leaf Blaster .pdf" />

		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns, OneNote.PageDetail.Selection);

			var files = page.Root.Descendants(ns + "InsertedFile")
				.Where(e => e.Attribute("selected")?.Value == "all");

			if (!files.Any())
			{
				files = page.Root.Descendants(ns + "InsertedFile");
			}

			if (!files.Any())
			{
				ShowError(Resx.Error_NoAttachments);
				return;
			}

			var updated = false;
			foreach (var file in files.ToList())
			{
				if (AlreadyCaptioned(file))
				{
					continue;
				}

				file.Attribute("selected")?.Remove();

				// floating InsertedFiles (printout attachments) are direct Page children with
				// Position/Size children; remove those before the LINQ clone is made so the
				// clone inside the OE is clean (Position/Size are invalid inside an OE context)
				var posElement = file.Element(ns + "Position");
				var isFloating = file.Parent.Name.LocalName == "Page";
				if (isFloating)
				{
					posElement?.Remove(); // detached; reused for Outline positioning below
					file.Element(ns + "Size")?.Remove();
				}

				var table = new Table(ns);
				table.AddColumn(0f); // OneNote will set width accordingly

				var caption = file.Attribute("preferredName")?.Value;
				caption ??= Path.GetFileName(file.Attribute("pathSource").Value);

				var cdata = new XCData(System.Web.HttpUtility.HtmlEncode(caption));

				var row = table.AddRow();
				var cell = row.Cells.First();

				cell.SetContent(
					new XElement(ns + "OEChildren",
						new XElement(ns + "OE",
							new XAttribute("alignment", "center"),
							file),
						new XElement(ns + "OE",
							new XAttribute("alignment", "center"),
							new XElement(ns + "Meta",
								new XAttribute("name", "om"),
								new XAttribute("content", "caption")),
							new XElement(ns + "T", cdata)
						)
					));

				var style = GetStyle();
				new Stylizer(style).ApplyStyle(cdata);

				if (isFloating)
				{
					// OneNote doesn't remove the original page-level object during UpdatePageContent;
					// force deletion by objectID first (same pattern as AddCaptionCommand)
					var fileId = file.Attribute("objectID")?.Value;
					if (fileId != null)
					{
						one.DeleteContent(page.PageId, fileId);
					}

					// wrap the table in an Outline positioned where the original attachment was
					var outline = new XElement(ns + "Outline",
						posElement ?? new XElement(ns + "Position",
							new XAttribute("x", "36.0"),
							new XAttribute("y", "36.0"),
							new XAttribute("z", "1")),
						new XElement(ns + "OEChildren",
							new XElement(ns + "OE", table.Root)));

					file.ReplaceWith(outline);
				}
				else
				{
					file.ReplaceWith(table.Root);
				}

				updated = true;
			}

			if (updated)
			{
				await one.Update(page);
			}
		}


		private static Style GetStyle()
		{
			Style style = null;

			// use custom Caption style if it exists

			var styles = new ThemeProvider().Theme.GetStyles();
			if (styles?.Count > 0)
			{
				style = styles.Find(s => s.Name.Equals("Caption"));
			}

			// otherwise use default style

			style ??= new Style
			{
				Color = "#5B9BD5", // close to CornflowerBlue
				FontSize = "10pt",
				IsBold = true
			};

			return style;
		}


		private bool AlreadyCaptioned(XElement file)
		{
			return file.Parent.ElementsAfterSelf().FirstOrDefault()?
				.Elements(ns + "Meta")
				.Any(e => e.Attribute("name").Value.Equals("om") &&
					 e.Attribute("content").Value.Equals("caption")) == true;
		}
	}
}
