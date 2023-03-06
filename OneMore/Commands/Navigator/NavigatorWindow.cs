//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Extensions;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using HistoryRecord = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	internal partial class NavigatorWindow : LocalizableForm
	{
		private const int WindowMargin = 20;
		private const int HeaderIndent = 16;
		private const int MinimizedBounds = -32000;

		private static string visitedID;

		private Screen screen;
		private Point corral;
		private Point location;
		private bool minimized;
		private readonly bool corralled;
		private readonly List<IDisposable> trash;


		// disposed
		private readonly NavigationProvider provider;


		public NavigatorWindow()
		{
			InitializeComponent();
			trash = new List<IDisposable>();

			if (NeedsLocalizing())
			{
				Text = Resx.NavigatorWindow_Text;

				Localize(new string[]
				{
					"pinnedHeadLabel",
					"historyHeadLabel=word_History",
					"closeButton=word_Close"
				});

				tooltip.SetToolTip(refreshButton, Resx.NavigatorWindow_refreshButton_Tooltip);
				tooltip.SetToolTip(upButton, Resx.NavigatorWindow_upButton_Tooltip);
				tooltip.SetToolTip(downButton, Resx.NavigatorWindow_downButton_Tooltip);
				tooltip.SetToolTip(unpinButton, Resx.NavigatorWindow_unpinButton_Tooltip);
				tooltip.SetToolTip(pinButton, Resx.NavigatorWindow_pinButton_Tooltip);
				tooltip.SetToolTip(copyPinnedButton, Resx.NavigatorWindow_copyTooltip);
				tooltip.SetToolTip(copyHistoryButton, Resx.NavigatorWindow_copyTooltip);
			}

			ManualLocation = true;

			provider = new NavigationProvider();
			provider.Navigated += Navigated;
			trash.Add(provider);

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;

			pinnedBox.FullRowSelect = true;
			pinnedBox.Columns.Add(
				new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });

			historyBox.FullRowSelect = true;
			historyBox.Columns.Add(
				new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });

			corralled = new SettingsProvider()
				.GetCollection("NavigatorSheet")
				.Get("corralled", false) ||
				Screen.AllScreens.Length == 1;
		}


		#region Handlers
		private async void PositionOnLoad(object sender, EventArgs e)
		{
			// deal with primary/secondary displays in either duplicate or extended mode...
			// Load is invoked prior to SizeChanged

			using var one = new OneNote();
			screen = Screen.FromHandle(one.WindowHandle);

			// move this window into the coordinate space of the active screen
			Location = screen.WorkingArea.Location;

			corral = screen.GetBoundedLocation(this);

			var settings = new SettingsProvider().GetCollection("navigator");
			if (settings.Contains("left") && settings.Contains("top"))
			{
				Left = settings.Get("left", Left);
				Top = settings.Get("top", Top);
			}
			else
			{
				Left = corral.X;
				Top = SystemInformation.CaptionHeight + WindowMargin;

				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					Left = WindowMargin;
				}
			}

			// it's possible that screen resolution has changed since settings were saved
			// and that left/top is now off the visible screen...
			if (Left > screen.WorkingArea.Width) Left = WindowMargin;
			if (Top > screen.WorkingArea.Height) Top = WindowMargin;

			if (settings.Contains("width") && settings.Contains("height"))
			{
				Width = settings.Get("width", Width);
				Height = settings.Get("height", Height);
			}

			// designer defines width but height is calculated
			MaximumSize = new Size(MaximumSize.Width, screen.WorkingArea.Height - (WindowMargin * 2));

			await LoadPinned();
			Navigated(null, await provider.ReadHistory());
		}


		private void SetLimitsOnSizeChanged(object sender, EventArgs e)
		{
			if (screen == null)
			{
				// too early; Load event hasn't fired yet
				return;
			}

			// SizeChanged is invoked after Load which sets screenArea
			corral = screen.GetBoundedLocation(this);

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;
			pinnedBox.Columns[0].Width = rowWidth;
			historyBox.Columns[0].Width = rowWidth;
		}


		private void RestrictOnMove(object sender, EventArgs e)
		{
			if (corralled && corral.X > 0)
			{
				if (Left < 10) Left = 10;
				if (Left > corral.X) Left = corral.X;
				if (Top < 10) Top = 10;
				if (Top > corral.Y) Top = corral.Y;
			}
		}


		private void TopOnShown(object sender, EventArgs e)
		{
			if (minimized)
			{
				Location = location;
				minimized = false;
			}

			BringToFront();
			TopMost = true;
			Activate();
			Focus();
		}


		// TrackMinimizedOnLayout and TrackOnLocationChanged keep track of the window position
		// across minimize/restore actions. This fixes the problem where the window is always
		// restored to 0,0 and instead resets the location to the previous point.
		private void TrackMinimizedOnLayout(object sender, LayoutEventArgs e)
		{
			if (e.AffectedProperty == "Bounds" &&
				Bounds.X == MinimizedBounds &&
				Bounds.Y == MinimizedBounds)
			{
				minimized = true;
			}
		}

		private void TrackOnLocationChanged(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Normal)
			{
				location = Location;
			}
		}


		private void CloseOnClick(object sender, EventArgs e)
		{
			Close();
		}


		private void SaveOnFormClosing(object sender, FormClosingEventArgs e)
		{
			var settings = new SettingsProvider();
			var collection = settings.GetCollection("navigator");
			collection.Add("left", Left);
			collection.Add("top", Top);
			collection.Add("width", Width);
			collection.Add("height", Height);
			settings.SetCollection(collection);
			settings.Save();
		}
		#endregion Handlers


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public static void SetVisited(string ID)
		{
			// need to lock?
			visitedID = ID;
		}


		private async Task LoadPinned()
		{
			var pins = await provider.ReadPinned();
			ShowPins(pins);
		}


		private void ShowPins(List<HistoryRecord> pinned)
		{
			if (pinnedBox.InvokeRequired)
			{
				pinnedBox.Invoke(new Action(() => ShowPins(pinned)));
				return;
			}

			pinnedBox.BeginUpdate();
			pinnedBox.Items.Clear();

			pinned.ForEach(record =>
			{
				var control = new HistoryListViewItem(record);
				var item = pinnedBox.AddHostedItem(control);
				item.Tag = record;
			});

			pinnedBox.EndUpdate();
			pinnedBox.Invalidate();

			pinnedBox.EnableItemEventBubbling();
		}


		private void Navigated(object sender, List<HistoryRecord> e)
		{
			if (historyBox.InvokeRequired)
			{
				historyBox.Invoke(new Action(() => Navigated(sender, e)));
				return;
			}

			try
			{
				if (e.Count > 0 && e[0].PageId == visitedID)
				{
					// user clicked ths page in navigator; don't reorder the list or they'll lose
					// their context and get confused, but refresh the headings pane
					LoadPageHeadings(e[0].PageId);
					visitedID = null;
					return;
				}

				visitedID = null;

				ShowPageOutline(e[0]);

				historyBox.BeginUpdate();
				historyBox.Items.Clear();

				e.ForEach(record =>
				{
					var control = new HistoryListViewItem(record);
					var item = historyBox.AddHostedItem(control);
					item.Tag = record;
				});

				historyBox.Items[0].Selected = true;
				historyBox.EndUpdate();
				historyBox.Invalidate();

				historyBox.EnableItemEventBubbling();
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating", exc);
			}
		}


		private async void PinOnClick(object sender, EventArgs e)
		{
			if (historyBox.SelectedItems.Count == 0)
			{
				return;
			}

			var records = new List<HistoryRecord>();
			foreach (IMoreHostItem host in historyBox.SelectedItems)
			{
				if (host.Tag is HistoryRecord record)
				{
					records.Add(record);
				}
			}

			if (records.Count > 0)
			{
				SetVisited(records.Last().PageId);
				await provider.AddPinned(records);
				await LoadPinned();
			}
		}


		private async void UnpinOnClick(object sender, EventArgs e)
		{
			if (pinnedBox.SelectedItems.Count == 0)
			{
				return;
			}

			var records = new List<HistoryRecord>();
			foreach (IMoreHostItem host in pinnedBox.SelectedItems)
			{
				if (host.Tag is HistoryRecord record)
				{
					records.Add(record);
				}
			}

			if (records.Count > 0)
			{
				var item = historyBox.SelectedItems.Count > 0
					? historyBox.SelectedItems[historyBox.SelectedItems.Count - 1]
					: historyBox.Items[0];

				if (item is IMoreHostItem host &&
					host.Control.Tag is HistoryRecord record)
				{
					SetVisited(record.PageId);
				}

				await provider.UnpinPages(records);
				await LoadPinned();
			}
		}


		private async void MoveUpOnClick(object sender, EventArgs e)
		{
			if (pinnedBox.SelectedItems.Count > 0)
			{
				var first = pinnedBox.SelectedIndices[0];
				if (first > 0)
				{
					// move...
					foreach (ListViewItem item in pinnedBox.SelectedItems)
					{
						item.Remove();
						pinnedBox.Items.Insert(first - 1, item);
					}

					// save...
					await SavePinned();
				}
			}
		}


		private async void MoveDownOnClick(object sender, EventArgs e)
		{
			if (pinnedBox.SelectedItems.Count > 0)
			{
				var last = pinnedBox.SelectedIndices[pinnedBox.SelectedIndices.Count - 1];
				if (last < pinnedBox.Items.Count - 1)
				{
					// move...
					var index = last + 1;
					foreach (ListViewItem item in pinnedBox.SelectedItems)
					{
						item.Remove();

						if (index == pinnedBox.Items.Count)
						{
							pinnedBox.Items.Add(item);
						}
						else
						{
							pinnedBox.Items.Insert(index, item);
						}
					}

					// save...
					await SavePinned();
				}
			}
		}


		private async Task SavePinned()
		{
			var records = new List<HistoryRecord>();
			foreach (IMoreHostItem host in pinnedBox.Items)
			{
				if (host.Tag is HistoryRecord record)
				{
					records.Add(record);
				}
			}

			await provider.SavePinned(records);
		}


		private async void CopyLinksOnClick(object sender, EventArgs e)
		{
			var box = sender == copyPinnedButton ? pinnedBox : historyBox;

			var records = new List<HistoryRecord>();
			foreach (IMoreHostItem host in box.SelectedItems)
			{
				if (host.Tag is HistoryRecord record)
				{
					records.Add(record);
				}
			}

			var hbuilder = new StringBuilder();
			var tbuilder = new StringBuilder();
			var one = records.Count == 1;

			foreach (var record in records)
			{
				if (one)
				{
					hbuilder.Append($"<a href=\"{record.Link}\">{record.Name}</a>");
					tbuilder.Append(record.Link);
				}
				else
				{
					hbuilder.Append($"<p><a href=\"{record.Link}\">{record.Name}</a></p>");
					tbuilder.Append($"{record.Link}\n");
				}

			}

			var board = new ClipboardProvider();

			var html = ClipboardProvider.WrapWithHtmlPreamble(hbuilder.ToString());
			board.Stash(System.Windows.TextDataFormat.Html, html);

			var text = tbuilder.ToString();
			board.Stash(System.Windows.TextDataFormat.Text, text);
			board.Stash(System.Windows.TextDataFormat.UnicodeText, text);

			await board.RestoreState();
		}


		private void ShowPageOutline(HistoryRecord info)
		{
			LoadPageHeadings(info.PageId);
		}


		private void RefreshPageHeadings(object sender, EventArgs e)
		{
			LoadPageHeadings(null);
		}


		private void LoadPageHeadings(string pageID)
		{
			using var one = new OneNote();
			var page = one.GetPage(pageID ?? one.CurrentPageId, OneNote.PageDetail.Basic);
			var headings = page.GetHeadings(one);

			head1Label.Text = page.Title;
			pageBox.Controls.Clear();

			if (headings.Count == 0)
			{
				return;
			}

			var font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
			trash.Add(font);

			var margin = WindowMargin * 2 + SystemInformation.VerticalScrollBarWidth * 2;

			foreach (var heading in headings)
			{
				var wrapper = new XElement("wrapper", heading.Text);
				var text = wrapper.TextValue();

				var leftpad = heading.Level * HeaderIndent;
				var leftmar = leftpad + 4;

				var link = new MoreLinkLabel
				{
					BackColor = Color.Transparent,
					ForeColor = SystemColors.WindowText,
					LinkColor = SystemColors.WindowText,
					VisitedLinkColor = SystemColors.WindowText,
					Text = text,
					Tag = heading,
					Font = font,
					Padding = new Padding(0),
					Margin = new Padding(leftmar, 0, 0, 4),
					Width = pageBox.Width - leftmar - margin
				};

				link.LinkClicked += new LinkLabelLinkClickedEventHandler(async (s, e) =>
				{
					if (s is MoreLinkLabel label)
					{
						using var one = new OneNote();
						var heading = (Models.Heading)label.Tag;
						await one.NavigateTo(heading.Link);
					}
				});

				pageBox.Controls.Add(link);
				pageBox.SetFlowBreak(link, true);
			}
		}
	}
}
