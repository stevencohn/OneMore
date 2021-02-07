//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class InsertCalendarCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string DailyCss = "font-family:Calibri;font-size:11.0pt";
		private const string GhostCss = "font-family:Calibri;font-size:11.0pt;color:#BFBFBF";

		private Page page;
		private XNamespace ns;


		public InsertCalendarCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			int year;
			int month;
			bool large;
			bool indent;

			using (var dialog = new InsertCalendarDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				year = dialog.Year;
				month = dialog.Month;
				large = dialog.Large;
				indent = dialog.Indent;
			}

			using (var one = new OneNote(out page, out ns))
			{
				var root = MakeCalendar(year, month, large);
				var header = MakeHeader(year, month);

				if (indent)
				{
					header.Add(new XElement(ns + "OEChildren",
							new XElement(ns + "OE",
							root)
						));

					page.AddNextParagraph(header);
				}
				else
				{
					page.AddNextParagraph(root);
					page.AddNextParagraph(header);
				}

				await one.Update(page);
			}
		}


		private XElement MakeCalendar(int year, int month, bool large)
		{
			var date = new DateTime(year, month, 1);
			var first = (int)date.DayOfWeek;
			var last = DateTime.DaysInMonth(year, month);
			var alignment = large ? "left" : "right";

			// calc table rows including header
			int term = first + last;
			int rowCount = term / 7 + (term % 7 == 0 ? 1 : 2);

			var table = new Table(ns, rowCount, 7)
			{
				BordersVisible = true
			};

			if (large)
			{
				for (int i = 0; i < 7; i++)
				{
					table.SetColumnWidth(i, 80f);
				}
			}

			// headers...

			var header = table.Rows.First();
			var format = AddIn.Culture.DateTimeFormat;
			var dow = 0;
			foreach (var cell in header.Cells)
			{
				cell.ShadingColor = HeaderShading;

				var name = large
					? format.GetDayName((DayOfWeek)dow).ToUpper()
					: format.GetShortestDayName((DayOfWeek)dow).ToUpper();

				cell.SetContent(new XElement(ns + "OE",
					new XAttribute("alignment", alignment),
					new XAttribute("style", HeaderCss),
					new XElement(ns + "T", new XCData(name))
					));

				dow++;
			}

			TableRow row;

			// previous month days...

			if (large && first > 0)
			{
				var prev = date.Subtract(new TimeSpan(-1, 0, 0, 0));
				var prevLast = DateTime.DaysInMonth(prev.Year, prev.Month);
				row = table.Rows.ElementAt(1);
				for (int i = 0; i < first; i++)
				{
					int d = prevLast - first + i;
					row.Cells.ElementAt(i).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", alignment),
						new XAttribute("style", GhostCss),
						new XElement(ns + "T", new XCData(d.ToString()))
						));
				}
			}

			// days...

			int day = 1;
			int rindex = 1;
			dow = first;
			row = table.Rows.ElementAt(rindex);
			while (day <= last)
			{
				var cell = row.Cells.ElementAt(dow);

				var content = new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
					new XAttribute("alignment", alignment),
					new XAttribute("style", DailyCss),
					new XElement(ns + "T", new XCData(day.ToString()))
					));

				if (large)
				{
					content.Add(
						new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
						new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
						new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty)))
						);
				}

				cell.SetContent(content);

				day++;
				dow++;

				if (dow > 6 && day <= last)
				{
					dow = 0;
					rindex++;
					row = table.Rows.ElementAt(rindex);
				}
			}

			// next month days...

			if (large && dow < 7)
			{
				day = 1;
				while (dow < 7)
				{
					row.Cells.ElementAt(dow).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", alignment),
						new XAttribute("style", HeaderCss),
						new XElement(ns + "T", new XCData(day.ToString()))
						));

					dow++;
					day++;
				}
			}

			return table.Root;
		}


		private XElement MakeHeader(int year, int month)
		{
			var quick = page.GetQuickStyle(Styles.StandardStyles.Heading2);

			var format = AddIn.Culture.DateTimeFormat;
			var monthName = format.GetMonthName(month);

			return new XElement(ns + "OE",
				new XAttribute("quickStyleIndex", quick.Index),
				new XElement(ns + "T", new XCData($"{monthName} {year}"))
				);
		}
	}
}
