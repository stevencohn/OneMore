//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Newtonsoft.Json.Linq;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Inserts a snippet that resembles the Confluence Info or Warning macros.
	/// </summary>
	internal class InsertInfoBoxCommand : Command
	{

		public InsertInfoBoxCommand()
		{
		}


		/// <summary>
		/// Insert a new info or warning table with starter content
		/// </summary>
		/// <param name="keyword">
		/// The keyword associated wit the type of block to insert: info, note, warn
		/// </param>
		public override async Task Execute(params object[] args)
		{
			var keyword = (string)args[0];
			Resx.Culture = AddIn.Culture;

			await using var one = new OneNote(out var page, out var ns);
			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			var theme = JObject.Parse(Resx.InfoBoxThemes)[keyword];

			var symbolStyle =
				$"font-family:'{theme["symbolFont"]}';font-size:{theme["symbolSize"]}.0pt;" +
				$"color:{theme["symbolColor"]};text-align:center";

			var normalStyle = page.GetQuickStyle(StandardStyles.Normal);
			normalStyle.Color = theme["textColor"].ToString();

			// find anchor and optional selected content...

			var range = new SelectionRange(page);
			range.GetSelection();

			XElement content;
			XElement anchor = null;

			if (range.Scope == SelectionScope.TextCursor)
			{
				content = new XElement(ns + "OE",
					new XAttribute("style", normalStyle.ToCss()),
					new XElement(ns + "T",
						new XAttribute("selected", "all"),
						new XCData(Resx.phrase_YourContentHere)
					));

				var editor = new PageEditor(page);
				editor.ExtractSelectedContent();
				anchor = editor.Anchor;
			}
			else
			{
				var editor = new PageEditor(page)
				{
					// the extracted content will be selected=all, keep it that way
					KeepSelected = true
				};

				content = editor.ExtractSelectedContent();
				anchor = editor.Anchor;

				editor.Deselect();
				editor.FollowWithCurosr(content);
			}

			// inner table...

			var inner = new Table(ns);
			inner.AddColumn(37f, true);
			inner.AddColumn(100f);

			var row = inner.AddRow();

			var symbol = char.ConvertFromUtf32(
				int.Parse(theme["symbol"].ToString(), System.Globalization.NumberStyles.HexNumber));

			row.Cells.ElementAt(0).SetContent(
				new XElement(ns + "OE",
					new XAttribute("alignment", "center"),
					new XAttribute("style", symbolStyle),
					new Meta(ns, Style.HintMeta, "skip"),
					new XElement(ns + "T",
						new XCData($"<span style='font-weight:bold'>{symbol}</span>"))
				));

			var title = Resx.ResourceManager.GetString(theme["titlex"].ToString(), AddIn.Culture);

			normalStyle.Color = theme["titleColor"].ToString();
			normalStyle.IsBold = true;

			row.Cells.ElementAt(1).SetContent(
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("style", normalStyle.ToCss()),
						new XElement(ns + "T",
							new XCData($"<span style='font-weight:bold'>{title}</span>"))
						),
					content.Name.LocalName == "OEChildren" ? content.Elements() : content
				));

			// outer table...

			var outer = new Table(ns)
			{
				BordersVisible = true
			};

			outer.AddColumn(600f, true);
			row = outer.AddRow();

			var cell = row.Cells.ElementAt(0);

			cell.ShadingColor = theme["shading"].ToString();
			cell.SetContent(inner);

			// update...

			if (anchor == null)
			{
				var editor = new PageEditor(page);
				editor.AddNextParagraph(outer.Root);
			}
			else
			{
				var localName = anchor.Name.LocalName;
				var box = new XElement(ns + "OE", outer.Root);

				if (localName.In("OE", "HTMLBlock"))
				{
					anchor.AddAfterSelf(box);
				}
				else // if (localName.In("OEChildren", "Outline"))
				{
					anchor.AddFirst(box);
				}
			}

			await one.Update(page);
		}
	}
}

