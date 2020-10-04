//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Xml.Linq;


	internal class CalendarCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt;text-align:right";
		private const string DailyCss = "font-family:Calibri;font-size:11.0pt;text-align:right";
		private const string GhostCss = "font-family:Calibri;font-size:11.0pt;color:#CFCFCF;text-align:right";

		private Page page;
		private XNamespace ns;


		public CalendarCommand()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
				ns = page.Namespace;

				try
				{
					var root = MakeCalendar(2020, 9, true);
					page.AddNextParagraph(root);
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

			var header = table.Rows.First();
			var format = CultureInfo.CurrentUICulture.DateTimeFormat;
			var dow = 0;
			foreach (var cell in header.Cells)
			{
				cell.ShadingColor = HeaderShading;

				var name = large
					? format.GetDayName((DayOfWeek)dow).ToUpper()
					: format.GetShortestDayName((DayOfWeek)dow).ToUpper();

				cell.SetContent(new XElement(ns + "OE",
					new XAttribute("alignment", "right"),
					new XAttribute("style", HeaderCss),
					new XElement(ns + "T", new XCData(name))
					));

				dow++;
			}

			TableRow row;

			if (large && first > 0)
			{
				// fill in previous month days...
				var prev = date.Subtract(new TimeSpan(-1, 0, 0, 0));
				var prevLast = DateTime.DaysInMonth(prev.Year, prev.Month);
				row = table.Rows.ElementAt(1);
				for (int i = 0; i < first; i++)
				{
					int d = prevLast - first + i;
					row.Cells.ElementAt(i).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", "right"),
						new XAttribute("style", GhostCss),
						new XElement(ns + "T", new XCData(d.ToString()))
						));
				}
			}

			int day = 1;
			int rindex = 1;
			dow = first;
			row = table.Rows.ElementAt(rindex);
			while (day <= last)
			{
				var cell = row.Cells.ElementAt(dow);

				var content = new XElement(ns + "OEChildren",
					new XElement(ns + "OE",
					new XAttribute("alignment", "right"),
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

			if (large && dow < 7)
			{
				// fill in next month days...
				day = 1;
				while (dow < 7)
				{
					row.Cells.ElementAt(dow).SetContent(new XElement(ns + "OE",
						new XAttribute("alignment", "right"),
						new XAttribute("style", GhostCss),
						new XElement(ns + "T", new XCData(day.ToString()))
						));

					dow++;
					day++;
				}
			}

			return table.Root;
		}
	}
}
