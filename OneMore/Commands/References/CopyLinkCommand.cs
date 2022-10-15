//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Copies the hyperlink to the current page or paragraph to the clipboard
	/// </summary>
	internal class CopyLinkCommand : Command
	{
		private const string RightArrow = "\u2192";


		public CopyLinkCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var specific = (bool)args[0];
			string hyperlink = null;
			string text = null;

			using (var one = new OneNote(out var page, out var ns))
			{
				if (specific)
				{
					var selected = page.Root.Elements(ns + "Outline")
						.Where(e => !e.Elements(ns + "Meta")
							.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)))
						.Descendants(ns + "OE")
						.LastOrDefault(e => e.Attributes().Any(a => a.Name == "selected"));

					if (selected != null)
					{
						if (selected.GetAttributeValue("objectID", out var objectID))
						{
							hyperlink = one.GetHyperlink(page.PageId, objectID);
							if (!string.IsNullOrEmpty(hyperlink))
							{
								// remove any descendant Images to prevent bad XML parsing
								// from brokets in OCR text, also remove indented paragraphs
								selected.Descendants(ns + "Image").Remove();
								selected.Descendants(ns + "OEChildren").Remove();

								text = selected.TextValue();
								if (text.Length > 20)
								{
									text = $"{text.Substring(0, 20)}...";
								}
							}
						}
					}
					else
					{
						UIHelper.ShowError(Resx.Error_BodyContext);
					}
				}
				else
				{
					hyperlink = one.GetHyperlink(page.PageId, null);
				}

				if (!string.IsNullOrEmpty(hyperlink))
				{
					// build from hierarchy to avoid splitting on '/' where there is a slash
					// in any part of the name

					var crumbs = new StringBuilder();
					var id = one.GetParent(page.PageId);
					while (!string.IsNullOrEmpty(id))
					{
						var node = one.GetHierarchyNode(id);

						// following line could be used to hyperlink each part like a breadcrumb
						//crumbs.Insert(0, $"<a href={node.Link}>{node.Name}</a> {RightArrow} ");

						crumbs.Insert(0, $"{node.Name} {RightArrow} ");
						id = one.GetParent(id);
					}

					crumbs.Append(page.Title);
					var path = specific ? $"{crumbs} {RightArrow} <i>{text}</i>" : crumbs.ToString();

					var builder = new StringBuilder();
					builder.AppendLine("<html>");
					builder.AppendLine("<body>");
					builder.AppendLine("<!--StartFragment-->");
					builder.AppendLine($"<a href='{hyperlink}'>{path}</a>");
					builder.AppendLine("<!--EndFragment-->");
					builder.AppendLine("</body>");
					builder.AppendLine("</html>");
					var html = PasteRtfCommand.AddHtmlPreamble(builder.ToString());

					// copy hyperlink to clipboard
					await new ClipboardProvider().SetHtml(html);

					MoreBubbleWindow.Show(owner, specific
						? Resx.CopyLinkCommand_LinkToParagraph
						: Resx.CopyLinkCommand_LinkToPage);
				}
			}

			await Task.Yield();
		}
	}
}
