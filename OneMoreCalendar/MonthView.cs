//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace OneMoreCalendar
{
	using OneMoreCalendar.Properties;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	internal partial class MonthView : ThemedUserControl, ICalendarView
	{
		private enum Hottype { Day, Page, Up, Down }

		private sealed class Hotspot
		{
			public Hottype Type;
			public Rectangle Bounds;

			// the box the title is truncated to when painted; hover draws and erases within it
			public Rectangle Clip;
			public bool InMonth;
			public CalendarDay Day;
			public CalendarPage Page;
		}

		private sealed class BellSpot
		{
			public Rectangle Bounds;
			public CalendarPage Page;
		}


		//private const string HeadBackColor = "#FFF4E8F3";
		//private const string TodayHeadColor = "#FFD6A6D3";
		private static readonly string LessGlyph = Resources.MonthView_LessGlyph; // \u23F6
		private static readonly string MoreGlyph = Resources.MonthView_MoreGlyph; // \u23F7
		private static readonly string CopyGlyph = Resources.MonthView_CopyGlyph; // \ud83d\uddc7
		private static readonly string CreateGlyph = Resources.MonthView_CreatePageGlyph;

		// Font.Height includes generous internal leading; pack page title rows closer
		// together than a full line height so a day's entries don't look so spread out
		private const float RowSpacingFactor = 0.8f;

		private readonly Font hotFont;
		private readonly Font moreFont;
		private readonly Font copyFont;
		private readonly Font deletedFont;
		private readonly Font italicFont;
		private readonly Font italicHotFont;
		private readonly Size moreSize;
		private readonly StringFormat format;
		private readonly List<Hotspot> hotspots;
		private readonly List<BellSpot> bells;
		private readonly BellHover bellHover;
		private readonly MoreButton copyButton;
		private readonly MoreButton createButton;
		private readonly ToolTip tooltip;

		private DateTime date;
		private CalendarDays days;
		private DayOfWeek firstDow;
		private Hotspot hotspot;
		private int dowOffset;
		private int headHeight;
		private int maxItems;
		private int rowLineHeight;
		private int moreWidth;
		private int moreHeight;
		private int weeks;


		public MonthView()
		{
			InitializeComponent();

			hotFont = new Font(Font, FontStyle.Regular | FontStyle.Underline);
			deletedFont = new Font(Font, FontStyle.Regular | FontStyle.Strikeout);
			italicFont = new Font(Font, FontStyle.Italic);
			italicHotFont = new Font(Font, FontStyle.Italic | FontStyle.Underline);
			moreFont = new Font("Segoe UI", 14.0f, FontStyle.Regular);
			moreSize = TextRenderer.MeasureText(MoreGlyph, Font);

			hotspots = new List<Hotspot>();
			bells = new List<BellSpot>();

			bellHover = new BellHover(this);
			Disposed += (_, _) => bellHover.Dispose();

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

			var createSize = TextRenderer.MeasureText(CreateGlyph, copyFont);
			createButton = new MoreButton
			{
				Font = copyFont,
				PreferredBack = Theme.MonthDayBack,
				PreferredFore = Theme.LinkColor,
				Text = CreateGlyph,
				Size = new Size(createSize.Width + 4, createSize.Height + 2),
				Visible = false
			};

			createButton.MouseDown += ClickCreatePageButton;

			tooltip = new ToolTip(components);
			tooltip.SetToolTip(copyButton, Resources.MonthView_CopyLinks);
			tooltip.SetToolTip(createButton, Resources.MonthView_CreatePage);

			// GenericTypographic has no leading bearing, matching how TitleRenderer lays out
			// emoji titles so the swatch-to-title gap is the same on every row
			format = new StringFormat(StringFormat.GenericTypographic)
			{
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoWrap
			};

			date = DateTime.Now.StartOfMonth();
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// copyButton and moreSize were sized/measured using a logical (96 DPI) glyph
			// measurement in the constructor, before DeviceDpi was valid; rescale now that it
			// is, and compact the padding around the glyph the same way as day header/rows
			copyButton.Size = new Size(this.Scaled(copyButton.Width), this.Scaled(copyButton.Height));
			createButton.Size = new Size(this.Scaled(createButton.Width), this.Scaled(createButton.Height));

			moreWidth = (int)(this.Scaled(moreSize.Width) * RowSpacingFactor);
			moreHeight = (int)(this.Scaled(moreSize.Height) * RowSpacingFactor);
		}


		public event CalendarDayHandler ClickedDay;
		public event CalendarHoverHandler HoverPage;
		public event CalendarPageHandler ClickedPage;
		public event CalendarPageMenuHandler PageMenu;
		public event CalendarCreatedPageHandler ClickedCreatePage;


		public void SetRange(DateTime startDate, DateTime endDate, CalendarPages pages)
		{
			bellHover.Cancel();
			date = startDate.StartOfMonth();

			firstDow = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.FirstDayOfWeek;
			MakeDayList(pages);

			for (int i = Controls.Count - 1; i >= 0; i--)
			{
				if (Controls[i] is MoreButton)
				{
					var c = Controls[i];
					if (c != copyButton && c != createButton)
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

			var settings = SettingsProvider.Current;

			var first = date.DayOfWeek;
			var last = DateTime.DaysInMonth(date.Year, date.Month);

			int dow;
			if (firstDow == DayOfWeek.Sunday)
			{
				dow = (int)first;
			}
			else
			{
				dow = first == DayOfWeek.Sunday ? 6 : (int)first - 1;
			}

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


		/// <summary>
		/// True if the given page is showing on the given day specifically because of its
		/// creation date rather than its last-modified date - only meaningful when both the
		/// Created and Modified settings are enabled, since only then can the same page show
		/// up on two different days. Used to render the created-day occurrence in italics.
		/// </summary>
		private static bool ShowsAsCreated(CalendarDay day, CalendarPage page)
		{
			var settings = SettingsProvider.Current;
			return settings.Created && settings.Modified
				&& page.Created.Date.Equals(day.Date)
				&& !page.Modified.Date.Equals(day.Date);
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			bellHover.Cancel();

			Hotspot spot;

			if (e.Button == MouseButtons.Right)
			{
				spot = hotspots.Find(h => h.Bounds.Contains(e.Location));
				if (spot?.Type == Hottype.Page)
				{
					PageMenu?.Invoke(this, new CalendarPageMenuEventArgs(
						spot.Page, spot.Bounds, PointToScreen(e.Location)));
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


		private void ScrollDay(CalendarDay day, int direction)
		{
			var offset = day.ScrollOffset + direction;
			if (offset < 0 || offset > day.Pages.Count - maxItems)
			{
				return;
			}

			bellHover.Cancel();

			day.ScrollOffset = offset;

			using var g = CreateGraphics();
			PaintDay(g, day);
		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			//base.OnMouseMove(e);

			var bell = bells.Find(b => b.Bounds.Contains(e.Location));
			bellHover.Track(e.Location, bell?.Page, bell?.Bounds ?? Rectangle.Empty);

			var spot = hotspots.Find(h => h.Bounds.Contains(e.Location));

			// moving within same spot?
			if (spot == hotspot)
			{
				if (hotspot is not null)
				{
					Cursor = Cursors.Hand;
				}
				return;
			}

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

					if (createButton.Visible)
					{
						createButton.Visible = false;
						Controls.Remove(createButton);
					}
				}
				else if (hotspot.Type == Hottype.Page)
				{
					using (var g = CreateGraphics())
					{
						using var brush = hotspot.Page.Modified.Month == date.Month
							? new SolidBrush(Theme.MonthPrimary)
							: new SolidBrush(Theme.MonthSecondary);

						g.FillRectangle(brush, hotspot.Clip);

						if (hotspot.Page.IsDeleted)
						{
							TitleRenderer.DrawTitle(g, hotspot.Page.Title, deletedFont, Brushes.Gray,
								hotspot.Clip, format);
						}
						else
						{
							using var titleBrush = new SolidBrush(hotspot.InMonth
								? Theme.MonthTodayFore : Theme.MonthDayFore);

							var titleFont = ShowsAsCreated(hotspot.Day, hotspot.Page) ? italicFont : Font;
							TitleRenderer.DrawTitle(g, hotspot.Page.Title, titleFont, titleBrush, hotspot.Clip, format);
						}
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(null));
				}

				hotspot = null;
				Cursor = Cursors.Default;
			}

			// highlight hovered...

			if (spot is not null)
			{
				if (spot.Type == Hottype.Day)
				{
					// right edge inside the header box, same margin copyButton always used
					var right = spot.Bounds.X + spot.Bounds.Width - this.Scaled(1);

					// match the header fill, which differs for today
					var headBack = spot.Day.Date.Date.Equals(DateTime.Now.Date)
						? Theme.MonthTodayBack
						: Theme.MonthDayBack;

					copyButton.PreferredBack = headBack;
					createButton.PreferredBack = headBack;

					if (spot.Day.Pages.Count > 0)
					{
						Controls.Add(copyButton);

						// constrain to the (compacted) header box so the button never
						// protrudes below its bottom edge
						var buttonHeight = Math.Min(copyButton.Height, headHeight - this.Scaled(2));
						copyButton.Height = buttonHeight;

						copyButton.Location = new Point(
							right - copyButton.Width,
							spot.Bounds.Y + ((spot.Bounds.Height - buttonHeight) / 2));

						copyButton.Tag = spot.Day;
						copyButton.Visible = true;

						// createButton sits immediately to the left of copyButton
						right = copyButton.Location.X - this.Scaled(2);
					}

					// createButton is always available, even on days with no pages yet
					Controls.Add(createButton);

					var createHeight = Math.Min(createButton.Height, headHeight - this.Scaled(2));
					createButton.Height = createHeight;

					createButton.Location = new Point(
						right - createButton.Width,
						spot.Bounds.Y + ((spot.Bounds.Height - createHeight) / 2));

					createButton.Tag = spot.Day;
					createButton.Visible = true;
				}
				else if (spot.Type == Hottype.Page)
				{
					using (var g = CreateGraphics())
					{
						using var fill = new SolidBrush(spot.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary);
						g.FillRectangle(fill, spot.Clip);

						using var fore = new SolidBrush(Theme.Highlight);
						var hoverFont = spot.Page.IsDeleted ? deletedFont
							: ShowsAsCreated(spot.Day, spot.Page) ? italicHotFont
							: hotFont;

						TitleRenderer.DrawTitle(g, spot.Page.Title, hoverFont, fore, spot.Clip, format);
					}

					HoverPage?.Invoke(this, new CalendarPageEventArgs(spot.Page));
				}

				hotspot = spot;
				Cursor = Cursors.Hand;
			}
		}


		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			bellHover.Cancel();
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			SuspendLayout();

			base.OnPaint(e);

			hotspots.Clear();
			bells.Clear();

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
				dowOffset = this.Scaled(dowFont.Height);

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
					var clip = new Rectangle(dayWidth * i, this.Scaled(1), dayWidth, dowOffset + this.Scaled(2));
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

			// headFont.Height is measured at a fixed logical (96 DPI) reference regardless
			// of the control's real DPI, so scale it explicitly; also compact away the
			// font's generous internal leading
			headHeight = this.Scaled((int)(headFont.Height * RowSpacingFactor) + 2);

			// measure real glyph height instead of using Font.Height (which includes
			// generous internal leading meant for paragraph spacing); MeasureString on
			// the live Graphics context already reflects the actual DPI/font rendering
			// in effect, whether local or over RDP, so no manual scaling or fudge
			// factor is needed here
			rowLineHeight = (int)Math.Ceiling(e.Graphics.MeasureString("Ap", Font).Height);

			// how many lines fit in each day box; must match the padding PaintDay
			// actually carves out of day.Bounds for its content box
			maxItems = (((Height - dowOffset) / weeks) - headHeight - this.Scaled(8)) / rowLineHeight;

			var now = DateTime.Now.Date;

			foreach (var day in days)
			{
				// header...

				var box = new Rectangle(
					col * dayWidth, row * dayHeight + dowOffset,
					dayWidth, headHeight);

				var today = day.Date.Date.Equals(now.Date);

				e.Graphics.FillRectangle(
					// compare only date part
					today ? todayBack : headBack,
					box);

				e.Graphics.DrawRectangle(gridPen, box);

				e.Graphics.DrawString(day.Date.Day.ToString(), headFont,
					day.InMonth ? inbrush : outbrush,
					box.X + this.Scaled(3), box.Y + this.Scaled(1));

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
					col * dayWidth + this.Scaled(1), row * dayHeight + headHeight + this.Scaled(2) + dowOffset,
					dayWidth - this.Scaled(2), dayHeight - headHeight - this.Scaled(2)
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

			// clear this day's own stale page hotspots (e.g. from a previous ScrollDay
			// repaint) before re-adding fresh ones below. Scoped to this day's bounds rather
			// than matched by page identity: when both the Created and Modified settings are
			// enabled, the same CalendarPage can have a separate hotspot on another day too,
			// and removing by page reference alone would wipe out that other day's hotspot
			// (making it unclickable) every time this day repaints.
			hotspots.RemoveAll(h => h.Type == Hottype.Page && day.Bounds.Contains(h.Bounds.Location));
			bells.RemoveAll(b => day.Bounds.Contains(b.Bounds.Location));

			// content box with padding
			var box = new Rectangle(
				day.Bounds.X + this.Scaled(3), day.Bounds.Y + this.Scaled(2),
				day.Bounds.Width - this.Scaled(8),
				day.Bounds.Height - this.Scaled(8));

			for (int i = day.ScrollOffset, t = 0; i < day.Pages.Count && t < maxItems; i++, t++)
			{
				var page = day.Pages[i];

				var top = box.Top + (rowLineHeight * t);

				// hard guard: never draw a row that would overflow the day cell's
				// bottom edge, regardless of whether the maxItems estimate above was
				// exactly right for this font/DPI/rendering environment
				if (top + rowLineHeight > box.Bottom)
				{
					break;
				}

				// shrink width if showing scroller glyphs
				var width = day.Pages.Count > maxItems && i >= maxItems - 2
					? box.Width - moreWidth
					: box.Width;

				var left = box.Left;

				// section color swatch - always the leftmost element
				if (page.SectionColor != Color.Empty && SettingsProvider.Current.Markers)
				{
					var swatchWidth = this.Scaled(4);
					var swatchRect = new RectangleF(
						left, top + this.Scaled(1),
						swatchWidth, rowLineHeight - this.Scaled(2));

					using var swatchBrush = new SolidBrush(page.SectionColor);
					g.FillRectangle(swatchBrush, swatchRect);

					var gap = swatchWidth + this.Scaled(6);
					left += gap;
					width -= gap;
				}

				if (page.HasReminders)
				{
					width -= this.Scaled(14);
					var bellSize = this.Scaled(12);
					var bellBounds = new Rectangle(left, top + this.Scaled(3), bellSize, bellSize);
					g.DrawImage(Properties.Resources.Reminder_01_24_Y, bellBounds);
					bells.Add(new BellSpot { Bounds = bellBounds, Page = page });
					left += this.Scaled(14);
				}

				// max length of string with ellipses
				var clip = new Rectangle(left, top, width, rowLineHeight);

				var font = page.IsDeleted ? deletedFont
					: ShowsAsCreated(day, page) ? italicFont
					: Font;

				using var brush = new SolidBrush(page.IsDeleted || day.InMonth
					? Theme.MonthTodayFore
					: Theme.MonthDayFore);

				// actual length of string for hyperlink hovering
				var size = TitleRenderer.DrawTitle(g, page.Title, font, brush, clip, format);
				hotspots.Add(new Hotspot
				{
					Type = Hottype.Page,
					Bounds = new Rectangle(clip.X, clip.Y, size.Width + this.Scaled(2), size.Height),
					Clip = clip,
					Day = day,
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
						new Point(box.Right - moreWidth - this.Scaled(1), box.Bottom - (moreHeight * 2) - this.Scaled(7)));
				}
				else
				{
					day.UpButton.PreferredBack = day.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary;
					day.UpButton.Location =
						new Point(box.Right - moreWidth - this.Scaled(1), box.Bottom - (moreHeight * 2) - this.Scaled(7));
				}

				if (day.DownButton is null)
				{
					MakeScrollButton(Hottype.Down, day,
						new Point(box.Right - moreWidth - this.Scaled(1), box.Bottom - moreHeight - this.Scaled(4)));
				}
				else
				{
					day.DownButton.PreferredBack = day.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary;
					day.DownButton.Location =
						new Point(box.Right - moreWidth - this.Scaled(1), box.Bottom - moreHeight - this.Scaled(4));
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
				PreferredBack = day.InMonth ? Theme.MonthPrimary : Theme.MonthSecondary,
				PreferredFore = Theme.LinkColor,
				Location = location,
				Text = type == Hottype.Up ? LessGlyph : MoreGlyph,
				Size = new Size(moreWidth + this.Scaled(4), moreHeight + this.Scaled(2)),
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
				Logger.Current.Debug($"copying links for {day.Pages.Count} pages on {day.Date:yyyy-MM-dd}");

				// fill and correct hyperlinks...

				var candidates = day.Pages
					.Where(p => p.Hyperlink is null)
					.Select(p => p);

				ProgressDialog progress = null;
				var canceled = false;

				try
				{
					if (candidates.Any())
					{
						var empties = new CalendarPages(candidates);

						copyButton.Enabled = false;

						progress = new ProgressDialog();
						progress.SetMessage(Resources.MonthView_GatheringLinks);
						progress.Show(FindForm());

						var one = new OneNoteProvider();
						await one.GetPageLinks(empties, progress.Token,
							max => progress.SetMaximum(max),
							async page =>
							{
								progress.SetMessage(page.Title);
								progress.Increment();
								await Task.Yield();
							});

						canceled = progress.Token.IsCancellationRequested;
					}

					if (canceled)
					{
						return;
					}

					// copy...

					var count = PageClipboard.CopyLinks(day.Pages);
					if (count > 0)
					{
						Logger.Current.WriteLine($"copied {count} hyperlinks from {day.Date}");
					}
				}
				finally
				{
					copyButton.Enabled = true;
					progress?.Close();
					progress?.Dispose();
				}
			}
		}


		private void ClickCreatePageButton(object sender, EventArgs e)
		{
			if (((MoreButton)sender).Tag is CalendarDay day)
			{
				Logger.Current.Debug($"picking section to create a page for {day.Date:yyyy-MM-dd}");

				createButton.Enabled = false;

				// this app may already hold its own COM connection to OneNote (from loading
				// the page index), so we must not try to launch OneNote ourselves here - a
				// headless server started first can prevent a subsequent interactive launch
				// from ever taking over. Just ask the user to start it themselves.
				if (!OneNoteProvider.IsRunningInteractively())
				{
					Logger.Current.WriteLine("cannot create page, OneNote is not running");
					MoreMessageBox.Show(FindForm(), Resources.MonthView_CreatePageNotRunning);
					createButton.Enabled = true;
					return;
				}

				try
				{
					using var one = new OneNote();

					// bring OneNote forward first, otherwise the QuickFiling dialog it owns
					// opens behind this calendar window
					OneNoteProvider.SetForegroundWindow(one.WindowHandle);

					one.SelectLocation(
						Resources.MonthView_CreatePageQFTitle,
						Resources.MonthView_CreatePageQFDescription,
						OneNote.Scope.Sections,
						sectionId => CreatePageInSection(day, sectionId));
				}
				catch (Exception exc)
				{
					// only reached if the QuickFiling dialog itself fails to open;
					// once it opens, CreatePageInSection re-enables the button
					Logger.Current.WriteLine("error opening section picker", exc);
					createButton.Enabled = true;
				}
			}
		}


		private async Task CreatePageInSection(CalendarDay day, string sectionId)
		{
			try
			{
				if (string.IsNullOrEmpty(sectionId))
				{
					// user canceled the QuickFiling picker
					return;
				}

				Logger.Current.WriteLine($"creating page for {day.Date:yyyy-MM-dd} in section {sectionId}");

				var pageId = await new OneNoteProvider().CreatePage(day.Date, sectionId);

				ClickedCreatePage?.Invoke(this, new CalendarCreatedPageEventArgs(day.Date, pageId));
			}
			catch (Exception exc)
			{
				Logger.Current.WriteLine($"error creating page for {day.Date:yyyy-MM-dd}", exc);

				MoreMessageBox.ShowError(FindForm(), Resources.MonthView_CreatePageError);
			}
			finally
			{
				createButton.Enabled = true;
			}
		}


		private void ClickScrollButton(object sender, EventArgs e)
		{
			if (((MoreButton)sender).Tag is Hotspot spot)
			{
				ScrollDay(spot.Day, spot.Type == Hottype.Up ? -1 : 1);
			}
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			bellHover.Cancel();

			var day = days?.Find(d => d.Bounds.Contains(e.Location));
			if (day is not null && (day.UpButton is not null || day.DownButton is not null))
			{
				ScrollDay(day, e.Delta < 0 ? 1 : -1);
			}
		}


		protected override void OnResize(System.EventArgs e)
		{
			base.OnResize(e);
			Invalidate();
		}
	}
}
