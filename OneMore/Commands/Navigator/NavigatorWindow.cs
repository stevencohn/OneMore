//************************************************************************************************
// Copyright © 2023 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Extensions;
	using River.OneMoreAddIn.Models;
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


	internal partial class NavigatorWindow : MoreForm
	{
		private const int WindowMargin = ScreenExtensions.ReasonableMargin;
		private const int HeaderIndent = 18;
		private const int MinimizedBounds = -32000;

		private static string visitedID;

		private Screen screen;
		private Rectangle corral;
		private Point location;
		private bool minimized;
		private readonly int depth;
		private readonly bool reading;
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
				tooltip.SetToolTip(deleteHistoryButton, Resx.NavigatorWindow_deleteHistoryButton_Tooltip);
			}

			ManualLocation = true;

			// User settings
			var settings = new SettingsProvider().GetCollection(nameof(NavigatorSheet));
			reading = !settings.Get("hidePinned", false);
			corralled = settings.Get("corralled", false); //|| Screen.AllScreens.Length == 1;
			depth = settings.Get("depth", NavigationService.DefaultHistoryDepth);

			provider = new NavigationProvider();
			provider.Navigated += ShowHistory;
			trash.Add(provider);

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;

			if (reading)
			{
				pinnedBox.FullRowSelect = true;
				pinnedBox.Columns.Add(
					new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });
			}
			else
			{
				subContainer.Panel1Collapsed = true;
				copyHistoryButton.Left = pinButton.Left;
				historyToolPanel.Controls.Remove(pinButton);
			}

			historyBox.FullRowSelect = true;
			historyBox.Columns.Add(
				new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });

			pinButton.Rescale();
			unpinButton.Rescale();
			refreshButton.Rescale();
			upButton.Rescale();
			downButton.Rescale();
			copyPinnedButton.Rescale();
			copyHistoryButton.Rescale();
		}


		protected override void OnLoad(EventArgs e)
		{
			//logger.WriteLine($"NavigatorWindow.OnLoad calling base...");

			base.OnLoad(e);
			//logger.WriteLine($"NavigatorWindow.OnLoad after base");

			BackColor = manager.GetColor("Control");

			var viewColor = manager.GetColor("ListView");

			pageBox.BackColor = viewColor;

			if (reading)
			{
				pinnedBox.BackColor = viewColor;
				pinnedBox.HighlightBackground = manager.GetColor("LinkHighlight");
			}

			historyBox.BackColor = viewColor;
			historyBox.HighlightBackground = manager.GetColor("LinkHighlight");
		}


		#region Window Management
		private async void PositionOnLoad(object sender, EventArgs e)
		{
			//logger.WriteLine($"NavigatorWindow.PositionOnLoad");

			// deal with primary/secondary displays in either duplicate or extended mode...
			// Load is invoked prior to SizeChanged

			screen = null;

			var settings = new SettingsProvider().GetCollection("navigator");
			var device = settings.Get<string>("device");
			if (device is not null)
			{
				screen = Array.Find(Screen.AllScreens, s => s.DeviceName == device);
			}

			if (screen is null)
			{
				await using var one = new OneNote();
				screen = Screen.FromHandle(one.WindowHandle);
			}

			//logger.WriteLine($"NavigatorWindow.PositionOnLoad screen primary:{screen.Primary} " +
			//	$"dev:{screen.DeviceName} bounds:{screen.Bounds.X}x{screen.Bounds.Y} " +
			//	$"Bounds:{screen.Bounds.Width}x{screen.Bounds.Height} " +
			//	$"WorkArea:{screen.WorkingArea.Width}x{screen.WorkingArea.Height}");

			// move this window into the coordinate space of the active screen
			Location = screen.WorkingArea.Location;
			//logger.WriteLine($"NavigatorWindow.PositionOnLoad location:{location.X}x{location.Y}");

			corral = GetCorral(screen);

			if (settings.Contains("left") && settings.Contains("top"))
			{
				Left = settings.Get("left", Left);
				Top = settings.Get("top", Top);
				//logger.WriteLine($"NavigatorWindow.PositionOnLoad setting left:{Left} Top:{Top}");
			}
			else
			{
				// set to corral origin regardless of whether "corralled" is set
				Left = corral.X;
				Top = corral.Y;
				//logger.WriteLine($"NavigatorWindow.PositionOnLoad corral left:{Left} Top:{Top}");

				if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
				{
					Left = WindowMargin;
					//logger.WriteLine($"NavigatorWindow.PositionOnLoad corral RTL left:{Left} Top:{Top}");
				}
			}

			// it's possible that screen resolution has changed since settings were saved
			// and that left/top is now off the visible screen...
			if (Left > screen.WorkingArea.Right)
			{
				Left = WindowMargin;
				//logger.WriteLine($"NavigatorWindow.PositionOnLoad override Left:{Left}");
			}

			if (Top > screen.WorkingArea.Bottom)
			{
				Top = WindowMargin;
				//logger.WriteLine($"NavigatorWindow.PositionOnLoad override Top:{Top}");
			}

			if (settings.Contains("width") && settings.Contains("height"))
			{
				Width = settings.Get("width", Width);
				Height = settings.Get("height", Height);
				//logger.WriteLine($"NavigatorWindow.PositionOnLoad width:{Width} height:{Height}");
			}

			//logger.WriteLine($"NavigatorWindow.PositionOnLoad data...");

			// designer defines width but height is calculated
			MaximumSize = new Size(MaximumSize.Width, screen.WorkingArea.Height - (WindowMargin * 2));

			// restore splitter positions
			mainContainer.SplitterDistance = settings.Get("splitter1", mainContainer.SplitterDistance);
			subContainer.SplitterDistance = settings.Get("splitter2", subContainer.SplitterDistance);

			// load data
			if (reading)
			{
				await LoadPinned();
			}

			ShowHistory(null, await provider.ReadHistoryLog());
		}


		private Rectangle GetCorral(Screen screen)
		{
			var bounds = screen.GetBoundedLocation(this);
			var left = screen.WorkingArea.X + WindowMargin;
			var top = screen.WorkingArea.Top + WindowMargin + SystemInformation.CaptionHeight;
			return new Rectangle(left, top, bounds.X - left, bounds.Y - top);
		}


		private void SetLimitsOnSizeChanged(object sender, EventArgs e)
		{
			if (screen == null)
			{
				// too early; Load event hasn't fired yet
				return;
			}

			// SizeChanged is invoked after Load which sets screenArea
			corral = GetCorral(screen);

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;

			if (reading)
			{
				pinnedBox.Columns[0].Width = rowWidth;
			}

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


		private void TopOnShownOnActivate(object sender, EventArgs e)
		{
			if (minimized)
			{
				Location = location;
				minimized = false;
			}

			Elevate(false);
			PanelFocusOnClick(historyHeadPanel, e);
		}


		private void PanelFocusOnClick(object sender, EventArgs e)
		{
			if (sender == pageHeadLabel || sender == pageHeadPanel)
			{
				pageBox.Focus();
			}
			else if (sender == pinnedHeadLabel || sender == pinnedHeadPanel)
			{
				pinnedBox.Focus();
				if (pinnedBox.SelectedItems.Count == 0 && pinnedBox.Items.Count > 0)
				{
					pinnedBox.Items[0].Selected = true;
				}
			}
			else if (sender == historyHeadLabel || sender == historyHeadPanel)
			{
				historyBox.Focus();
				if (historyBox.SelectedItems.Count == 0 && historyBox.Items.Count > 0)
				{
					historyBox.Items[0].Selected = true;
				}
			}
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
			if (WindowState == FormWindowState.Normal)
			{
				screen = Array.Find(Screen.AllScreens, s =>
					Left >= s.WorkingArea.Left && Left <= s.WorkingArea.Right &&
					Top >= s.WorkingArea.Top && Top <= s.WorkingArea.Bottom);

				screen ??= Array.Find(Screen.AllScreens, s => s.Primary);

				var settings = new SettingsProvider();
				var collection = settings.GetCollection("navigator");

				if (screen is not null)
				{
					collection.Add("device", screen.DeviceName);
				}

				collection.Add("left", Left);
				collection.Add("top", Top);
				collection.Add("width", Width);
				collection.Add("height", Height);
				collection.Add("splitter1", mainContainer.SplitterDistance);
				collection.Add("splitter2", subContainer.SplitterDistance);
				settings.SetCollection(collection);
				settings.Save();
			}
		}
		#endregion Window Management


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -



		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Page headings...

		#region Page headings
		private async Task LoadPageHeadings(string pageID)
		{
			if (pageBox.InvokeRequired)
			{
				pageBox.BeginInvoke(new Action(async () => await LoadPageHeadings(pageID)));
				return;
			}

			await using var one = new OneNote();
			var page = await one.GetPage(pageID ?? one.CurrentPageId, OneNote.PageDetail.Basic);

			logger.Verbose($"LoadPageHeadings [{page.Title}]");
			logger.StartClock();

			var headings = page.GetHeadings(one, linked: false);
			logger.DebugTime($"GetHeadings <---", keepRunning: true);

			// need to escape ampersand because it is used to indicate keyboard accelerators
			var title = page.Title.Replace("&", "&&");
			pageHeadLabel.Text = page.TitleID == null
				? Resx.phrase_QuickNote
				: title;

			pageBox.Controls.Clear();

			// inject the page Title as the top-most Heading
			if (page.TitleID != null)
			{
				var ns = page.Root.GetNamespaceOfPrefix(OneNote.Prefix);
				var root = page.Root
						.Elements(ns + "Title")
						.Elements(ns + "OE")
						.FirstOrDefault();

				if (root != null)
				{
					headings.Insert(0, new()
					{
						// defer URL lookup until clicked
						Link = string.Empty,
						Root = root,
						Text = title
					});
				}
			}

			if (headings.Count == 0)
			{
				logger.VerboseTime("LoadPageHeadings done, 0 headings");
				return;
			}

			var font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
			trash.Add(font);

			logger.DebugTime($"suspending layout", keepRunning: true);
			pageBox.SuspendLayout();

			var margin = SystemInformation.VerticalScrollBarWidth * 2;

			foreach (var heading in headings)
			{
				var wrapper = new XElement("T", new XCData(heading.Text));
				var text = wrapper.TextValue(true);

				var leftpad = heading.Level * HeaderIndent;
				var leftmar = leftpad + 4;

				var link = new MoreLinkLabel
				{
					Text = text,
					Tag = heading,
					Font = font,
					Padding = new Padding(0),
					Margin = new Padding(leftmar, 0, 0, 4),
					Width = pageBox.Width - leftmar - margin
				};

				using var g = link.CreateGraphics();
				var size = g.MeasureString(text, font);
				link.Height = (int)(size.Height + link.Padding.Top + link.Padding.Bottom);

				link.LinkClicked += new LinkLabelLinkClickedEventHandler(async (s, e) =>
				{
					if (s is MoreLinkLabel label)
					{
						await using var one = new OneNote();
						var heading = (Heading)label.Tag;
						if (heading.Link == string.Empty)
						{
							logger.Verbose($"fetching URL for [{heading.Text}]");
							heading.Link = page.GetHyperlink(heading.Root, one);
						}
						else
						{
							logger.Verbose($"found URL for [{heading.Text}]");
						}

						await one.NavigateTo(heading.Link);
					}
				});

				pageBox.Controls.Add(link);
				pageBox.SetFlowBreak(link, true);

				((ILoadControl)link).OnLoad();
			}

			pageBox.ResumeLayout();
			logger.DebugTime($"resumed layout", keepRunning: true);

			await UpdateTitles(page);

			logger.DebugTime("LoadPageHeadings done");
		}


		private async Task UpdateTitles(Page page)
		{
			if (TitleChanged(historyBox, page) || TitleChanged(pinnedBox, page))
			{
				await provider.RecordHistory(page.PageId, depth);
			}
		}


		private static bool TitleChanged(ListView box, Page page)
		{
			foreach (IMoreHostItem host in box.Items)
			{
				if (host.Tag is HistoryRecord record)
				{
					if (record.PageId == page.PageId &&
						record.Name != page.Title)
					{
						return true;
					}
				}
			}

			return false;
		}


		private async Task ShowPageOutline(HistoryRecord info)
		{
			await LoadPageHeadings(info.PageId);
		}


		private async void RefreshPageHeadings(object sender, EventArgs e)
		{
			await LoadPageHeadings(null);
		}


		private async void DoKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				await LoadPageHeadings(null);
				e.Handled = true;
			}
		}


		private void ResizePageBox(object sender, EventArgs e)
		{
			var margin = SystemInformation.VerticalScrollBarWidth * 2;

			foreach (MoreLinkLabel link in pageBox.Controls)
			{
				var leftpad = ((Heading)link.Tag).Level * HeaderIndent;
				var leftmar = leftpad + 4;
				link.Width = pageBox.Width - leftmar - margin;

			}
		}
		#endregion Page headings


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Pinned reading list...

		private async Task LoadPinned()
		{
			var pins = await provider.ReadPinned();
			ShowPins(pins);
		}


		private void ShowPins(List<HistoryRecord> pinned)
		{
			if (pinnedBox.InvokeRequired)
			{
				pinnedBox.BeginInvoke(new Action(() => ShowPins(pinned)));
				return;
			}

			pinnedBox.BeginUpdate();
			pinnedBox.Items.Clear();

			var viewColor = manager.GetColor("ListView");
			pinned.ForEach(record =>
			{
				var control = new HistoryControl(record)
				{
					BackColor = viewColor
				};

				control.ApplyTheme(manager);

				var item = pinnedBox.AddHostedItem(control);
				item.Tag = record;
			});

			pinnedBox.EndUpdate();
			pinnedBox.EnableItemEventBubbling();
		}

		#region Pin control
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
				SetVisited(records[records.Count - 1].PageId);
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
		#endregion Pin control


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// History list...

		private async void ShowHistory(object sender, HistoryLog log)
		{
			if (historyBox.InvokeRequired)
			{
				historyBox.BeginInvoke(new Action(() => ShowHistory(sender, log)));
				return;
			}

			try
			{
				// update pinned panel...

				var records = new List<HistoryRecord>();
				foreach (IMoreHostItem host in historyBox.SelectedItems)
				{
					if (host.Tag is HistoryRecord record)
					{
						records.Add(record);
					}
				}

				// new items not currently displayed; user must have pressed Ctrl+Shift+B
				if (log.Pinned.Except(records).Any())
				{
					ShowPins(log.Pinned);
				}

				// update history panel...

				if (log.History.Count > 0 && log.History[0].PageId == visitedID)
				{
					// user clicked ths page in navigator; don't reorder the list or they'll lose
					// their context and get confused, but refresh the headings pane
					await ShowPageOutline(log.History[0]);
					visitedID = null;
					return;
				}

				visitedID = null;

				await ShowPageOutline(log.History[0]);

				UpdateHistoryBox(log);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating", exc);
			}
		}


		private void UpdateHistoryBox(HistoryLog log)
		{
			if (historyBox.InvokeRequired)
			{
				historyBox.BeginInvoke(new Action(() => UpdateHistoryBox(log)));
				return;
			}

			historyBox.BeginUpdate();
			historyBox.Items.Clear();
			var viewColor = manager.GetColor("ListView");

			log.History.ForEach(record =>
			{
				var control = new HistoryControl(record)
				{
					BackColor = viewColor
				};

				control.ApplyTheme(manager);

				var item = historyBox.AddHostedItem(control);
				item.Tag = record;
			});

			historyBox.Items[0].Selected = true;
			historyBox.EndUpdate();
			historyBox.EnableItemEventBubbling();
		}


		private void Control_DoubleClick(object sender, EventArgs e)
		{
			logger.WriteLine("double");
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		#region Control
		private async void CopyLinks(object sender, EventArgs e)
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


		private async void DeleteHistoryRecords(object sender, EventArgs e)
		{
			var result = MoreMessageBox.Show(Owner,
				string.Format(Resx.NavigatorWindow_confirmDelete, historyBox.SelectedItems.Count),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
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

			await provider.DeleteHistory(records);
			var log = await provider.ReadHistoryLog();
			UpdateHistoryBox(log);
		}


		private void HistoryKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				foreach (ListViewItem item in historyBox.Items)
				{
					item.Selected = true;
				}
			}
		}


		public static void SetVisited(string ID)
		{
			// called from HistoryControl!
			// need to lock?
			visitedID = ID;
		}
		#endregion Control
	}
}
