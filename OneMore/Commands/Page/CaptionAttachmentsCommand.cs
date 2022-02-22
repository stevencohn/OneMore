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
	using Resx = River.OneMoreAddIn.Properties.Resources;


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
			using (var one = new OneNote(out var page, out ns, OneNote.PageDetail.Selection))
			{
				var files = page.Root.Descendants(ns + "InsertedFile")?
					.Where(e => e.Attribute("selected")?.Value == "all");

				if (files?.Any() != true)
				{
					files = page.Root.Descendants(ns + "InsertedFile");
				}

				if (files?.Any() != true)
				{
					UIHelper.ShowError(Resx.Error_NoAttachments);
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

					var table = new Table(ns);
					table.AddColumn(0f); // OneNote will set width accordingly

					var caption = file.Attribute("preferredName")?.Value;
					if (caption == null)
					{
						caption = Path.GetFileName(file.Attribute("pathSource").Value);
					}

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

					file.ReplaceWith(table.Root);

					updated = true;
				}

				if (updated)
				{
					await one.Update(page);
				}
			}
		}


		private static Style GetStyle()
		{
			Style style = null;

			// use custom Caption style if it exists

			var styles = new ThemeProvider().Theme.GetStyles();
			if (styles?.Count > 0)
			{
				style = styles.FirstOrDefault(s => s.Name.Equals("Caption"));
			}

			// otherwise use default style

			if (style == null)
			{
				style = new Style
				{
					Color = "#5B9BD5", // close to CornflowerBlue
					FontSize = "10pt",
					IsBold = true
				};
			}

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
