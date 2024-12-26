//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;
	using Win = System.Windows;


	#region Wrappers
	internal class CopyLinkToPageCommand : CopyLinkCommand
	{
		public CopyLinkToPageCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(false);
		}
	}
	internal class CopyLinkToParagraphCommand : CopyLinkCommand
	{
		public CopyLinkToParagraphCommand() : base() { }
		public override Task Execute(params object[] args)
		{
			return base.Execute(true);
		}
	}
	#endregion Wrappers


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

			await using var one = new OneNote(out var page, out var ns);

			if (specific)
			{
				var selected = page.BodyOutlines
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
					ShowError(Resx.Error_BodyContext);
				}
			}
			else
			{
				hyperlink = one.GetHyperlink(page.PageId, page.TitleID);
			}

			if (string.IsNullOrEmpty(hyperlink))
			{
				return;
			}

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
			var fragment = $"<a href='{hyperlink}'>{path}</a>";
			var html = ClipboardProvider.WrapWithHtmlPreamble(fragment);

			// copy hyperlink to clipboard
			var board = new ClipboardProvider();
			board.Stash(Win.TextDataFormat.Html, html);
			board.Stash(Win.TextDataFormat.Text, hyperlink);
			await board.RestoreState();

			await Task.Yield();
		}
	}
}
