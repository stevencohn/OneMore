//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertCalendarCommand : Command
	{
		private struct Day
		{
			public int Date;
			public bool InMonth;
		}


		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string BrightCss = ";color:#FEFEFE";
		private const string DailyCss = "font-family:Calibri;font-size:11.0pt";
		private const string GhostCss = "font-family:Calibri;font-size:11.0pt;color:#BFBFBF";

		private Page page;
		private XNamespace ns;


		public InsertCalendarCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out page, out ns))
			{
				if (!page.ConfirmBodyContext())
				{
					UIHelper.ShowError(Resx.Error_BodyContext);
					return;
				}

				using (var dialog = new InsertCalendarDialog())
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					logger.WriteLine($"making calendar for {dialog.Month}/{dialog.Year}");

					var days = MakeDayList(dialog.Year, dialog.Month, dialog.FirstDay);

					var root = MakeCalendar(days, dialog.FirstDay, dialog.Large, dialog.HeaderShading);
					var header = MakeHeader(dialog.Year, dialog.Month);

					if (dialog.Indent)
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
				}

				await one.Update(page);
			}
		}


		private List<Day> MakeDayList(int year, int month, DayOfWeek firstDay)
		{
			var days = new List<Day>();

			var date = new DateTime(year, month, 1);

			var first = date.DayOfWeek;
			var last = DateTime.DaysInMonth(date.Year, date.Month);

			var firstCol = firstDay == DayOfWeek.Sunday
				? (int)first
				: first == DayOfWeek.Sunday ? 6 : (int)first - 1;

			if (firstCol > 0)
			{
				var prev = date.Subtract(new TimeSpan(1, 0, 0, 0));
				var prevLast = DateTime.DaysInMonth(prev.Year, prev.Month);

				for (int p = prevLast - firstCol + 1; p <= prevLast; p++)
				{
					days.Add(new Day { Date = p });
				}
			}

			for (int day = 1; day <= last; day++)
			{
				days.Add(new Day { Date = day, InMonth = true });
			}

			var rest = 7 - days.Count % 7;
			for (firstCol = 1; firstCol <= rest; firstCol++)
			{
				days.Add(new Day { Date = firstCol });
			}

			return days;
		}


		private XElement MakeCalendar(
			List<Day> days, DayOfWeek firstDay, bool large, string shading)
		{
			var alignment = large ? "left" : "right";

			var table = new Table(ns, days.Count / 7 + 1, 7)
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

			var css = ColorTranslator.FromHtml(shading).GetBrightness() < 0.5
				? $"{HeaderCss}{BrightCss}"
				: HeaderCss;

			var header = table[0];
			var format = AddIn.Culture.DateTimeFormat;
			var dow = firstDay == DayOfWeek.Sunday ? 0 : 1;
			foreach (var cell in header.Cells)
			{
				cell.ShadingColor = shading;

				var name = large
					? format.GetDayName((DayOfWeek)(dow % 7)).ToUpper()
					: format.GetShortestDayName((DayOfWeek)(dow % 7)).ToUpper();

				cell.SetContent(
					new Paragraph(ns, name).SetAlignment(alignment).SetStyle(css));

				dow++;
			}

			// days...

			int c = 0;
			int r = 1;
			var row = table[r];

			foreach (var day in days)
			{
				if (large || day.InMonth)
				{
					if (day.InMonth)
					{
						var content = new XElement(ns + "OEChildren",
							new Paragraph(ns, day.Date.ToString())
								.SetAlignment(alignment).SetStyle(DailyCss));

						if (large)
						{
							content.Add(
								new Paragraph(ns, string.Empty),
								new Paragraph(ns, string.Empty),
								new Paragraph(ns, string.Empty));
						}

						row[c].SetContent(content);
					}
					else if (large)
					{
						row[c].SetContent(
							new Paragraph(ns, day.Date.ToString())
								.SetAlignment(alignment).SetStyle(GhostCss));
					}
				}

				c++;
				if (c > 6)
				{
					c = 0;
					r++;
					if (r < table.RowCount)
					{
						row = table[r];
					}
				}
			}

			return table.Root;
		}


		private XElement MakeHeader(int year, int month)
		{
			var quick = page.GetQuickStyle(Styles.StandardStyles.Heading2);
			var monthName = AddIn.Culture.DateTimeFormat.GetMonthName(month);

			return new Paragraph(ns, $"{monthName} {year}").SetQuickStyle(quick.Index);
		}
	}
}
