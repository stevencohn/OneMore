//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class TagBankCommand : Command
	{
		private const string BankStyleName = "omWordBank";

		private const string RibbonSymbol = "26";   // the award ribbon Tag symbol
		private const int BankType = 23;            // custom TagDef type for word bank outline


		public TagBankCommand()
		{
		}


		/// <summary>
		/// Gets the word bank outlne
		/// </summary>
		public XElement BankOutline { get; private set; }


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out var ns);

			var updated = args.Length > 0 && (bool)args[0]
				? MakeWordBank(page, ns)
				: RemoveWordBank(one, page, ns);

			if (updated)
			{
				await one.Update(page);
			}
		}


		/// <summary>
		/// Exposed publicly so that HashtaggerCommand can add a bank to a page and then
		/// insert tags into it.
		/// </summary>
		/// <param name="page"></param>
		/// <param name="ns"></param>
		/// <returns></returns>
		public bool MakeWordBank(Page page, XNamespace ns)
		{
			var quickIndex = MakeQuickStyle(page);
			var tagIndex = MakeRibbonTagDef(page, BankType);

			BankOutline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => e.Elements().Any(x =>
					x.Name.LocalName == "Meta" &&
					x.Attribute("name").Value == MetaNames.TaggingBank));

			if (BankOutline is not null)
			{
				return false;
			}

			var tag = new XElement(ns + "Tag",
				new XAttribute("index", tagIndex),
				new XAttribute("completed", "true"),
				new XAttribute("disabled", "false"));

			// a single space to imply style
			var content = $"<span style='font-weight:bold'> </span>";
			var cdata = new XCData(content);

			BankOutline = new XElement(ns + "Outline",
				new XElement(ns + "Position",
					// 245 accounts for "Wednesday, December 30, 2020 12:12pm"
					new XAttribute("x", "245"),
					new XAttribute("y", "43"),
					new XAttribute("z", "0")),
				new XElement(ns + "Size",
					new XAttribute("width", "400"),
					new XAttribute("height", "11"),
					new XAttribute("isSetByUser", "true")),
				new XElement(ns + "Meta",
					new XAttribute("name", MetaNames.TaggingBank),
					new XAttribute("content", "1")),
				new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
						new XAttribute("quickStyleIndex", quickIndex),
						tag,
						new XElement(ns + "T", cdata)
					))
				);

			page.Root.Elements(ns + "Title").First().AddAfterSelf(BankOutline);
			return true;
		}


		private int MakeQuickStyle(Page page)
		{
			var styles = page.GetQuickStyles();
			var style = styles.Find(s => s.Name == BankStyleName);

			if (style is not null)
			{
				return style.Index;
			}

			var quick = StandardStyles.Citation.GetDefaults();
			quick.Index = styles.Max(s => s.Index) + 1;
			quick.Name = BankStyleName;
			quick.IsBold = true;

			page.AddQuickStyleDef(quick.ToElement(page.Namespace));

			return quick.Index;
		}


		private string MakeRibbonTagDef(Page page, int tagType)
		{
			var index = page.GetTagDefIndex(RibbonSymbol);
			index ??= page.AddTagDef(RibbonSymbol, "Page Tags", tagType);
			return index;
		}


		private bool RemoveWordBank(OneNote one, Page page, XNamespace ns)
		{
			var outline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => e.Elements().Any(x =>
					x.Name.LocalName == "Meta" &&
					x.Attribute("name").Value == MetaNames.TaggingBank));

			if (outline is null)
			{
				return false;
			}

			var text = outline.TextValue(true);
			if (text?.Trim().Length > 0)
			{
				if (MoreMessageBox.ShowQuestion(owner, Resx.TagBankCommand_confirm) != DialogResult.Yes)
				{
					return false;
				}
			}

			if (outline.GetAttributeValue("objectID", out string id))
			{
				one.DeleteContent(page.PageId, id);
			}

			// remove it from the DOM so it doesn't get added back in!
			outline.Remove();
			return true;
		}
	}
}
