//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using OneMoreCalendar.Properties;
	using River.OneMoreAddIn;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Windows.Forms;


	internal partial class MonthView : ThemedUserControl, ICalendarView
	{
		private enum Hottype { Day, Page, Up, Down }

		private sealed class Hotspot
		{
			public Hottype Type;
			public Rectangle Bounds;
			public bool InMonth;
			public CalendarDay Day;
			public CalendarPage Page;
		}


		//private const string HeadBackColor = "#FFF4E8F3";
		//private const string TodayHeadColor = "#FFD6A6D3";
		private const string LessGlyph = "⏶"; // \u23F6
		private const string MoreGlyph = "⏷"; // \u23F7
		private const string CopyGlyph = "🗇"; // \ud83d\uddc7

		private readonly IntPtr hand;
		private readonly Font hotFont;
		private readonly Font moreFont;
		private readonly Font copyFont;
		private readonly Font deletedFont;
		private readonly Size moreSize;
		private readonly StringFormat format;
		private readonly List<Hotspot> hotspots;
		private readonly MoreButton copyButton;
		private readonly ToolTip tooltip;

		private DateTime date;
		private CalendarDays days;
		private DayOfWeek firstDow;
		private Hotspot hotspot;
		private int onFormat;
		private int dowOffset;
		private int maxItems;
		private int weeks;


		[DllImport("user32.dll")]
		private static extern int RegisterClipboardFormat(string Format);



		public MonthView()
		{
			InitializeComponent();

			hand = Native.LoadCursor(IntPtr.Zero, Native.IDC_HAND);
			hotFont = new Font(Font, FontStyle.Regular | FontStyle.Underline);
			deletedFont = new Font(Font, FontStyle.Regular | FontStyle.Strikeout);
			moreFont = new Font("Segoe UI", 14.0f, FontStyle.Regular);
			moreSize = TextRenderer.MeasureText(MoreGlyph, Font);

			hotspots = new List<Hotspot>();

			copyFont = new Font("Segoe UI Symbol", 9.0f, FontStyle.Regular);
			var copySize = TextRenderer.MeasureText(CopyGlyph, copyFont);
			copyButton = new MoreButton
			{
				Font = copyFont,
				PreferredBack = Theme.MonthDayBack,
				PreferredFore = Theme.LinkColor,
				Text = CopyGlyph,
				Size = new Size(copySize.Width + 4, copySize.Height + 2),
				Visible = false
			};

			copyButton.MouseDown += ClickCopyPageButton;

			tooltip = new ToolTip(components);
			tooltip.SetToolTip(copyButton, "Copy links to all pages from this day");

			format = new StringFormat
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};

			date = DateTime.Now.StartOfMonth();
		}


		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;
		public event CalendarSnapshotHandler SnappedPage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			date = startDate.StartOfMonth();

			firstDow = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
			MakeDayList(pages);

			for (int i = Controls.Count - 1; i >= 0; i--)
			{
				if (Controls[i] is MoreButton)
				{
					var c = Controls[i];
					if (c != copyButton)
					{
						Controls.RemoveAt(i);
						c.Dispose();
					}
				}
			}

			Invalidate();
		}


		private void MakeDayList(CalendarPages pages)
		{
			days = new CalendarDays();

			var settings = new SettingsProvider();

			var first = date.DayOfWeek;
			var last = DateTime.DaysInMonth(date.Year, date.Month);

			var dow = firstDow == DayOfWeek.Sunday
				? (int)first
				: first == DayOfWeek.Sunday ? 6 : (int)first - 1;

			var runner = date.Date;

			// previous month
			if (dow > 0)
			{
				runner = runner.AddDays(-dow);
				for (int i = 0; i < dow; i++)
				{
					MakeDay(days, pages, runner, settings.Modified, settings.Created);
					runner = runner.AddDays(1.0);
				}
			}

			// month
			for (int i = 1; i <= last; i++)
			{
				MakeDay(days, pages, runner, settings.Modified, settings.Created, true);
				runner = runner.AddDays(1.0);
			}

			// next month
			var rest = 7 - days.Count % 7;
			if (rest < 7)
			{
				for (int i = 0; i < rest; i++)
				{
					MakeDay(days, pages, runner, settings.Modified, settings.Created);
					runner = runner.AddDays(1.0);
				}
			}
		}

		private void MakeDay(
			CalendarDays days, CalendarPages pages,
			DateTime date, bool modified, bool created, bool inMonth = false)
		{
			var day = new CalendarDay { Date = date, InMonth = inMonth };

			// filtering prioritizes modified over created and prevent pages from being
			// displayed twice in the month if both created and modified in the same month
			var pags = pages.Where(p =>
				(modified && p.Modified.Date.Equals(date)) ||
				(created && p.Created.Date.Equals(date))
				);

			pags.ForEach(p => day.Pages.Add(p));

			days.Add(day);
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			Hotspot spot;

			if (e.Button == MouseButtons.Right)
			{
				spot = hotspots.Find(h => h.Bounds.Contains(e.Location));
				if (spot?.Type == Hottype.Page)
				{
					SnappedPage?.Invoke(this,
						new CalendarSnapshotEventArgs(spot.Page, spot.Bounds));
				}

				return;
			}

			spot = hotspots.Find(h => h.Bounds.Contains(e.Location));
			switch (spot?.Type)
			{
				case Hottype.Page:
					ClickedPage?.Invoke(this, new CalendarPageEventArgs(spot.Page));
					break;

				case Hottype.Day:
					ClickedDay?.Invoke(this, new CalendarDayEventArgs(spot.Day.Date));
					break;
			}
		}


		private void ScrollDay(Hotspot spot)
		{
			var offset = spot.Day.ScrollOffset + (spot.Type == Hottype.Up ? -1 : 1);
			if (offset < 0 || offset > spot.Day.Pages.Count - maxItems)
			{
				return;
			}

			spot.Day.ScrollOffset = offset;

			using var g = CreateGraphics();
			PaintDay(g, spot.Day);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			//base.OnMouseMove(e);

			var spot = hotspots.Find(h => h.Bounds.Contains(e.Location));

			// moving within same spot?
			if (spot == hotspot)
			{
				if (hotspot is not null)
				{
					Native.SetCursor(hand);
				}
				return;
			}

			var width = Width / 7 - 8;

			// clear previously active...

			if (hotspot is not null)
			{
				if (hotspot.Type == Hottype.Day)
				{
					if (copyButton.Visible)
					{
						copyButton.Visible = false;
						Controls.Remove(copyButton);
					}
				}
				else if (hotspot.Type == Hottype.Page)
				{
					using (var g = CreateGraphics())
					{
						using var brush = hotspot.Page.Modified.Month == date.Month
							? new SolidBrush(Theme.MonthPrimary)
							: new SolidBrush(Theme.MonthSecondary);

						g.FillRectangle(brush, hotspot.Bounds);

						if (hotspot.Page.IsDeleted)
						{
							g.DrawString(hotspot.Page.Title, deletedFont, Brushes.Gray,
								new Rectangle(hotspot.Bounds.X, hotspot.Bounds.Y, width, hotspot.Bounds.Height),
								format);
						}
						else
						{
							var titleBrush = new SolidBrush(hotspot.InMonth
								? Theme.MonthTodayFore : Theme.MonthDayFore);

							g.DrawString(hotspot.Page.Title, Font, titleBrush, hotspot.Bounds, format);
						}
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(null));
				}

				hotspot = null;
				Native.SetCursor(Cursors.Default.Handle);
			}

			// highlight hovered...

			if (spot is not null)
			{
				if (spot.Type == Hottype.Day)
				{
					if (spot.Day.Pages.Count > 0)
					{
						Controls.Add(copyButton);

						copyButton.Location = new Point(
							spot.Bounds.X + spot.Bounds.Width - copyButton.Width - 1,
							spot.Bounds.Y + 1);

						copyButton.Tag = spot.Day;
						copyButton.Visible = true;
					}
				}
				else if (spot.Type == Hottype.Page)
				{
					using (var g = CreateGraphics())
					{
						using var fill = new SolidBrush(spot.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary);
						g.FillRectangle(fill, spot.Bounds);

						using var fore = new SolidBrush(Theme.Highlight);
						g.DrawString(spot.Page.Title,
							spot.Page.IsDeleted ? deletedFont : hotFont, fore,
							new Rectangle(spot.Bounds.X, spot.Bounds.Y, width, spot.Bounds.Height),
							format);
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(spot.Page));
				}

				hotspot = spot;
				Native.SetCursor(hand);
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			SuspendLayout();

			base.OnPaint(e);

			hotspots.Clear();

			if (days is not null && days.Count > 0)
			{
				PaintGrid(e);
				PaintDays(e);
			}

			ResumeLayout();
		}


		private void PaintGrid(PaintEventArgs e)
		{
			e.Graphics.Clear(Theme.BackColor);

			// day of week names...

			var dowFont = new Font("Segoe UI Light", 10.0f, FontStyle.Regular);
			if (dowFont.Name != "Segoe UI Light")
			{
				dowFont.Dispose();
				dowFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			}

			try
			{
				var culture = Thread.CurrentThread.CurrentUICulture.DateTimeFormat;
				dowOffset = dowFont.Height;

				using var dowFormat = new StringFormat
				{
					Alignment = StringAlignment.Center,
					FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit,
					Trimming = StringTrimming.EllipsisCharacter
				};

				var dayWidth = Width / 7;

				// day names and vertical lines...

				using var pen = new Pen(Theme.MonthGrid, 0.1f);
				var dow = firstDow == DayOfWeek.Sunday ? 0 : 1;

				for (int i = 0; i < 7; i++, dow++)
				{
					var name = culture.GetDayName((DayOfWeek)(dow % 7)).ToUpper();
					var clip = new Rectangle(dayWidth * i, 1, dayWidth, dowFont.Height + 2);
					using var brush = new SolidBrush(Theme.MonthDayFore);
					e.Graphics.DrawString(name, dowFont, brush, clip, dowFormat);

					if (i < 6)
					{
						var x = (i + 1) * dayWidth;
						e.Graphics.DrawLine(pen, x, dowOffset, x, e.ClipRectangle.Height);
					}
				}

				// horizontal lines...

				weeks = days.Count / 7;
				var dayHeight = (Height - dowOffset) / weeks;
				for (int i = 1; i < weeks; i++)
				{
					e.Graphics.DrawLine(pen,
						0, i * dayHeight + dowOffset,
						e.ClipRectangle.Width, i * dayHeight + dowOffset);
				}
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine(exc);
			}
			finally
			{
				dowFont.Dispose();
			}
		}


		private void PaintDays(PaintEventArgs e)
		{
			var dayWidth = Width / 7;
			var dayHeight = (Height - dowOffset) / weeks;
			var row = 0;
			var col = 0;

			using var headFont = new Font("Segoe UI", 10.0f, FontStyle.Regular);
			using var headFore = new SolidBrush(Theme.MonthDayFore);
			using var headBack = new SolidBrush(Theme.MonthDayBack);
			using var todayBack = new SolidBrush(Theme.MonthTodayBack);
			using var gridPen = new Pen(Theme.MonthGrid, 0.1f);
			using var inbrush = new SolidBrush(Theme.MonthTodayFore);
			using var outbrush = new SolidBrush(Theme.MonthDayFore);

			// how many lines fit in each day box
			maxItems = (((Height - dowOffset) / weeks) - headFont.Height - 2) / Font.Height;

			var now = DateTime.Now.Date;

			foreach (var day in days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight + dowOffset,
					dayWidth, headFont.Height + 2);

				var today = day.Date.Date.Equals(now.Date);

				e.Graphics.FillRectangle(
					// compare only date part
					today ? todayBack : headBack,
					box);

				e.Graphics.DrawRectangle(gridPen, box);

				e.Graphics.DrawString(day.Date.Day.ToString(), headFont,
					day.InMonth ? inbrush : outbrush,
					box.X + 3, box.Y + 1);

				// record day header box
				hotspots.Add(new Hotspot
				{
					Type = Hottype.Day,
					Bounds = box,
					Day = day,
					InMonth = day.InMonth
				});

				// day content box
				day.Bounds = new Rectangle(
					col * dayWidth + 1, row * dayHeight + headFont.Height + 4 + dowOffset,
					dayWidth - 2, dayHeight - headFont.Height - 4
					);

				PaintDay(e.Graphics, day);

				col++;
				if (col > 6)
				{
					col = 0;
					row++;
				}
			}
		}


		private void PaintDay(Graphics g, CalendarDay day)
		{
			using var backBrush = new SolidBrush(day.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary);
			g.FillRectangle(backBrush, day.Bounds);

			if (day.Pages.Count == 0)
			{
				return;
			}

			day.Pages.ForEach(p =>
			{
				var index = hotspots.FindIndex(h => h.Page == p);
				if (index >= 0) hotspots.RemoveAt(index);
			});

			// content box with padding
			var box = new Rectangle(
				day.Bounds.X + 3, day.Bounds.Y + 2,
				day.Bounds.Width - 8,
				day.Bounds.Height - 8);

			for (int i = day.ScrollOffset, t = 0; i < day.Pages.Count && t < maxItems; i++, t++)
			{
				var page = day.Pages[i];

				// shrink width if showing scroller glyphs
				var width = day.Pages.Count > maxItems && i >= maxItems - 2
					? box.Width - moreSize.Width
					: box.Width;

				var left = box.Left;
				var top = box.Top + (Font.Height * t);
				if (page.HasReminders)
				{
					width -= 14;
					left += 14;
					g.DrawImage(Properties.Resources.Reminder_01_24_Y,
						box.Left, top + 3, 12f, 12f);
				}

				// max length of string with ellipses
				var clip = new Rectangle(left, top, width, Font.Height);

				var font = page.IsDeleted ? deletedFont : Font;
				using var brush = new SolidBrush(page.IsDeleted || day.InMonth
					? Theme.MonthTodayFore
					: Theme.MonthDayFore);

				g.DrawString(page.Title, font, brush, clip, format);

				// actual length of string for hyperlink hovering
				var size = g.MeasureString(page.Title, font, clip.Width, format).ToSize();
				hotspots.Add(new Hotspot
				{
					Type = Hottype.Page,
					Bounds = new Rectangle(clip.X, clip.Y, size.Width + 2, size.Height),
					Page = page,
					InMonth = day.InMonth
				});
			}

			// scroll buttons
			if (maxItems < day.Pages.Count)
			{
				if (day.UpButton is null)
				{
					MakeScrollButton(Hottype.Up, day,
						new Point(box.Right - moreSize.Width - 1, box.Bottom - (moreSize.Height * 2) - 7));
				}
				else
				{
					day.UpButton.Location =
						new Point(box.Right - moreSize.Width - 1, box.Bottom - (moreSize.Height * 2) - 7);
				}

				if (day.DownButton is null)
				{
					MakeScrollButton(Hottype.Down, day,
						new Point(box.Right - moreSize.Width - 1, box.Bottom - moreSize.Height - 4));
				}
				else
				{
					day.DownButton.Location =
						new Point(box.Right - moreSize.Width - 1, box.Bottom - moreSize.Height - 4);
				}
			}
			else
			{
				if (day.UpButton is not null)
				{
					day.UpButton.Dispose();
					day.UpButton = null;
				}

				if (day.DownButton is not null)
				{
					day.DownButton.Dispose();
					day.DownButton = null;
				}
			}
		}


		private void MakeScrollButton(Hottype type, CalendarDay day, Point location)
		{
			// this Hotspot is only used to restore the location of the button
			// not as a hover region
			var spot = new Hotspot
			{
				Type = type,
				Bounds = new Rectangle(location.X, location.Y, 0, 0),
				Day = day
			};

			var button = new MoreButton
			{
				Font = moreFont,
				PreferredBack = Theme.MonthPrimary,
				PreferredFore = Theme.LinkColor,
				Location = location,
				Text = type == Hottype.Up ? LessGlyph : MoreGlyph,
				Size = new Size(moreSize.Width + 4, moreSize.Height + 2),
				Tag = spot
			};

			button.MouseDown += ClickScrollButton;
			Controls.Add(button);

			if (type == Hottype.Up)
			{
				day.UpButton = button;
			}
			else
			{
				day.DownButton = button;
			}
		}


		private async void ClickCopyPageButton(object sender, EventArgs e)
		{
			if (((MoreButton)sender).Tag is CalendarDay day)
			{
				// fill and correct hyperlinks...

				var candidates = day.Pages
					.Where(p => p.Hyperlink is null)
					.Select(p => p);

				if (candidates.Any())
				{
					var empties = new CalendarPages(candidates);

					var one = new OneNoteProvider();
					await one.GetPageLinks(empties);

					// hyperlinks may be returned from the OneNote API backwards from the
					// expected format so this matches the two parts that needs to be swapped
					var regex = new Regex(@"onenote:(#.+?&end)&base-path=(https:.+)");

					foreach (var page in empties)
					{
						if (page.Hyperlink.StartsWith("onenote:https:"))
						{
							// hyperlink is correct, just strip onenote: part
							page.Hyperlink = page.Hyperlink.Substring(8);
						}
						else
						{
							var match = regex.Match(page.Hyperlink);
							if (match.Success)
							{
								// hyperlink is reversed, so correct it
								page.Hyperlink = $"onenote:{match.Groups[2].Value}{match.Groups[1].Value}";
							}
						}
					}
				}

				// copy...

				var pages = day.Pages.Where(p => p.Hyperlink is not null).ToList();
				if (pages.Any())
				{
					Logger.Current.WriteLine($"copying {pages.Count} hyperlinks from {day.Date}");

					// HTML Format (for pasting in Word and other non-OneNote rich text apps)
					var html = new StringBuilder();
					// Text (for pasting into Notepad
					var text = new StringBuilder();
					// OneNote Link (special case for pasting into OneNote)
					var onlink = new StringBuilder();

					var many = pages.Count > 1;
					foreach (var page in pages)
					{
						var web = $"<a href=\"{page.Hyperlink}\">{page.Title}</a>";
						onlink.AppendLine(many ? $"<p lang=en-US>{web}</p>{Environment.NewLine}" : web);

						var both = $"{web} (<a href=\"{page.WebHyperlink}\">Web view</a>)";
						html.AppendLine(many ? $"<p lang=en-US>{both}</p>{Environment.NewLine}" : both);

						text.AppendLine(page.WebHyperlink);
						text.AppendLine(page.Hyperlink);
					}

					// build out the clipboard...

					// do not use the System.Windows.Clipboard classes here because they
					// screw up the DPI of the app window!!!
					var data = new DataObject();

					var wrap = ClipboardProvider.WrapWithHtmlPreamble(
						Resources.HtmlClipboardPreamble + Environment.NewLine +
						html.ToString()
						);

					data.SetText(wrap, TextDataFormat.Html);

					var t = text.ToString().Trim();
					data.SetText(t, TextDataFormat.Text);
					data.SetText(t, TextDataFormat.UnicodeText);

					if (onFormat == 0)
					{
						onFormat = RegisterClipboardFormat("OneNote Link");
					}

					wrap = ClipboardProvider.WrapWithHtmlPreamble(
						Resources.HtmlClipboardPreamble + Environment.NewLine +
						onlink.ToString()
						);

					var stream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(wrap));
					data.SetData("OneNote Link", stream);

					Clipboard.SetDataObject(data, true, 3, 100);
				}
			}
		}


		private void ClickScrollButton(object sender, EventArgs e)
		{
			if (((MoreButton)sender).Tag is Hotspot spot)
			{
				ScrollDay(spot);
			}
		}


		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Invalidate();
		}
	}
}
