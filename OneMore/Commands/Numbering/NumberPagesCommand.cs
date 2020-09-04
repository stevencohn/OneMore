//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class NumberPagesCommand : Command
	{
		private class PageBasics
		{
			public string ID;
			public string Name;
			public int PageLevel;
		}


		private ApplicationManager manager;
		private XNamespace ns;


		public NumberPagesCommand()
		{
		}


		public void Execute()
		{
			using (var dialog = new PageNumberingDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					using (manager = new ApplicationManager())
					{
						var section = manager.CurrentSection();
						ns = section.GetNamespaceOfPrefix("one");

						var pages = section.Elements(ns + "Page")
							.Select(e => new PageBasics
							{
								ID = e.Attribute("ID").Value,
								Name = e.Attribute("name").Value,
								PageLevel = int.Parse(e.Attribute("pageLevel").Value)
							})
							.ToList();

						if (pages.Count > 0)
						{
							var index = 0;
							ApplyNumbering(pages, ref index, pages[0].PageLevel, false, string.Empty);
						}
					}
				}
			}
		}


		private void ApplyNumbering(List<PageBasics> pages, ref int index, int level, bool numeric, string prefix)
		{
			int counter = 1;

			while (index < pages.Count && pages[index].PageLevel == level)
			{
				var page = manager.GetPage(pages[index].ID, PageInfo.piBasic);

				var cdata = page.Element(ns + "Title")
					.Element(ns + "OE")
					.Element(ns + "T")
					.GetCData();

				cdata.Value = BuildPrefix(counter, numeric, level, prefix) + " " + cdata.Value;
				manager.UpdatePageContent(page);

				index++;
				counter++;

				if (index < pages.Count && pages[index].PageLevel > level)
				{
					ApplyNumbering(pages, ref index, pages[index].PageLevel, numeric, $"{prefix}{counter - 1}.");
				}
			}
		}


		private string BuildPrefix(int counter, bool numeric, int level, string prefix)
		{
			if (!numeric)
			{
				switch (level % 3)
				{
					case 0:
						return $"({counter})";

					case 1:
						return $"({counter.ToAlphabetic().ToLower()})";

					case 2:
						return $"({counter.ToRoman().ToLower()})";
				}
			}

			return $"({prefix}{counter})";
		}
	}
}
