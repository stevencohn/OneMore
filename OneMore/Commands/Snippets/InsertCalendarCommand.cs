//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;
	using System.Linq;
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


		public void Execute()
		{
			int year;
			int month;
			bool large;
			using (var dialog = new CalendarDialog())
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				year = dialog.Year;
				month = dialog.Month;
				large = dialog.Large;
			}

			using (var manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
				ns = page.Namespace;

				try
				{
					var root = MakeCalendar(year, month, large);
					page.AddNextParagraph(root);

					var header = MakeHeader(year, month);
					page.AddNextParagraph(header);

					manager.UpdatePageContent(page.Root);
				}
				catch (Exception exc)
				{
					logger.WriteLine(exc);
				}
			}
		}


		private XElement MakeCalendar(int year, int month, bool large)
		{
			var date = new DateTime(year, month, 1);
			var first = (int)date.DayOfWeek;
			var last = DateTime.DaysInMonth(year, month);
			var alignment = large ? "left" : "right";

			var rowCount = last == 28 && first == 0 ? 5 : 6;

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
			var format = CultureInfo.CurrentUICulture.DateTimeFormat;
			var dow = 0;
			var css = large ? HeaderCss : $"{HeaderCss};text-align:right";

			foreach (var cell in header.Cells)
			{
				cell.ShadingColor = HeaderShading;

				var name = large
					? format.GetDayName((DayOfWeek)dow).ToUpper()
					: format.GetShortestDayName((DayOfWeek)dow).ToUpper();

				cell.SetContent(new XElement(ns + "OE",
					new XAttribute("alignment", alignment),
					new XAttribute("style", css),
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
				css = large ? GhostCss : $"{GhostCss};text-align:right";

				for (int i = 0; i < first; i++)
				{
					int d = prevLast - first + i;
					row.Cells.ElementAt(i).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", alignment),
						new XAttribute("style", css),
						new XElement(ns + "T", new XCData(d.ToString()))
						));
				}
			}

			// days...

			int day = 1;
			int rindex = 1;
			dow = first;
			row = table.Rows.ElementAt(rindex);
			css = large ? GhostCss : $"{DailyCss};text-align:right";
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
				css = large ? GhostCss : $"{GhostCss};text-align:right";
				while (dow < 7)
				{
					row.Cells.ElementAt(dow).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", alignment),
						new XAttribute("style", GhostCss),
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

			var format = CultureInfo.CurrentUICulture.DateTimeFormat;
			var monthName = format.GetMonthName(month);

			return new XElement(ns + "OE",
				new XAttribute("quickStyleIndex", quick.Index),
				new XElement(ns + "T", new XCData($"{monthName} {year}"))
				);
		}
	}
}
