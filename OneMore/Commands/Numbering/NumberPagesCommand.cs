//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Numbers pages using one of two different outline numbering schemes
	/// </summary>
	internal class NumberPagesCommand : Command
	{
		private sealed class PageBasics
		{
			public string ID;
			public string Name;
			public int Level;
		}


		private OneNote one;
		private XNamespace ns;
		private RemovePageNumbersCommand cleaner;
		private UI.ProgressDialog progress;


		public NumberPagesCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var dialog = new NumberPagesDialog();
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			using (one = new OneNote())
			{
				var section = one.GetSection();
				ns = one.GetNamespace(section);

				var pages = section.Elements(ns + "Page")
					.Select(e => new PageBasics
					{
						ID = e.Attribute("ID").Value,
						Name = e.Attribute("name").Value,
						Level = int.Parse(e.Attribute("pageLevel").Value)
					});

				if (pages.Any())
				{
					logger.StartClock();

					var list = pages.ToList();
					var index = 0;

					if (dialog.CleanupNumbering)
					{
						cleaner = new RemovePageNumbersCommand();
					}

					using (progress = new UI.ProgressDialog())
					{
						progress.SetMaximum(list.Count);
						progress.Show();

						await ApplyNumbering(
							list, index, list[0].Level,
							dialog.NumericNumbering, string.Empty);

						progress.Close();
					}

					logger.StopClock();
					logger.WriteTime("numbered pages");
				}
			}
		}


		private async Task<int> ApplyNumbering(
			List<PageBasics> pages, int index, int level, bool numeric, string prefix)
		{
			int counter = 1;

			while (index < pages.Count && pages[index].Level == level)
			{
				var page = one.GetPage(pages[index].ID, OneNote.PageDetail.Basic);

				var cdata = page.Root.Element(ns + "Title")
					.Element(ns + "OE")
					.Element(ns + "T")
					.GetCData();

				progress.SetMessage(string.Format(Properties.Resources.NumberingPage_Message, pages[index].Name));
				progress.Increment();

				var text = cdata.Value;
				cleaner?.RemoveNumbering(cdata.Value, out text);

				cdata.Value = BuildPrefix(counter, numeric, level, prefix) + " " + text;
				await one.Update(page);

				index++;
				counter++;

				if (index < pages.Count && pages[index].Level > level)
				{
					index = await ApplyNumbering(
						pages, index, pages[index].Level,
						numeric, $"{prefix}{counter - 1}.");
				}
			}

			return index;
		}


		private static string BuildPrefix(int counter, bool numeric, int level, string prefix)
		{
			if (!numeric)
			{
				switch ((level - 1) % 3)
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
