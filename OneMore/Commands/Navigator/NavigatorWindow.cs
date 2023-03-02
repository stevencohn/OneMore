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
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using HierarchyInfo = OneNote.HierarchyInfo;
	using Resx = Properties.Resources;


	internal partial class NavigatorWindow : LocalizableForm
	{
		private const int WindowMargin = 20;
		private const int HeaderIndent = 16;

		private static string visitedID;

		private Screen screen;
		private Point corral;
		private readonly bool corralled;
		private readonly List<HierarchyInfo> history;
		private readonly List<HierarchyInfo> pinned;
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
					"pinnedHeadLabel=word_Pinned",
					"historyHeadLabel=word_History",
					"closeButton"
				});
			}

			ManualLocation = true;

			provider = new NavigationProvider();
			provider.Navigated += Navigated;
			trash.Add(provider);

			history = new List<HierarchyInfo>();
			pinned = new List<HierarchyInfo>();

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
			// SizeChanged is invoked after Load which sets screenArea
			corral = screen.GetBoundedLocation(this);
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
			BringToFront();
			TopMost = true;
			Activate();
			Focus();
		}


		private void CloseOnClick(object sender, EventArgs e)
		{
			var settings = new SettingsProvider();
			var collection = settings.GetCollection("navigator");
			collection.Add("left", Left);
			collection.Add("top", Top);
			collection.Add("width", Width);
			collection.Add("heigth", Height);
			settings.SetCollection(collection);
			settings.Save();

			Close();
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

			var action = new Action(() =>
			{
				if (ResolveReferences(pinned, pins))
				{
					pinnedBox.BeginUpdate();
					pinnedBox.Items.Clear();

					pinned.ForEach(info =>
					{
						var control = new HistoryListViewItem(info);
						var item = pinnedBox.AddHostedItem(control);
						item.Tag = info;
					});

					pinnedBox.EndUpdate();
					pinnedBox.Invalidate();
				}
			});

			if (pinnedBox.InvokeRequired)
			{
				pinnedBox.Invoke(action);
			}
			else
			{
				action();
			}
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
				if (e.Count > 0 && e[0].PageID == visitedID)
				{
					// user clicked ths page in navigator; don't reorder the list or they'll lose
					// their context and get confused, but refresh the headings pane
					LoadPageHeadings(e[0].PageID);
					visitedID = null;
					return;
				}

				if (ResolveReferences(history, e))
				{
					visitedID = null;

					ShowPageOutline(history[0]);

					historyBox.BeginUpdate();
					historyBox.Items.Clear();

					history.ForEach(info =>
					{
						var control = new HistoryListViewItem(info);
						var item = historyBox.AddHostedItem(control);
						item.Tag = info;
					});

					historyBox.Items[0].Selected = true;
					historyBox.EndUpdate();
					historyBox.Invalidate();
				}
			}
			catch (Exception exc)
			{
				logger.WriteLine($"error navigating", exc);
			}
		}


		private bool ResolveReferences(List<HierarchyInfo> details, List<HistoryRecord> records)
		{
			using var one = new OneNote();
			var list = new List<HierarchyInfo>();
			var updated = false;

			// iterate manually to check both existence and order
			for (int i = 0;  i < records.Count; i++)
			{
				var record = records[i];
				var j = details.FindIndex(d => d.PageId == record.PageID);

				try
				{
					var item = j < 0
						? one.GetPageInfo(record.PageID)
						: details[j];

					if (item != null)
					{
						item.Visited = record.Visited;

						var parentID = one.GetParent(record.PageID);
						_ = one.GetHierarchyNode(parentID);

						list.Add(item);

						updated |= (j != i);
					}
				}
				catch (Exception exc)
				{
					logger.WriteLine($"navigator resolve skipping page {record.PageID}", exc);
				}
			}

			if (updated)
			{
				details.Clear();
				details.AddRange(list);
			}

			return updated;
		}


		private async void PinOnClick(object sender, EventArgs e)
		{
			if (historyBox.SelectedItems.Count == 0)
			{
				return;
			}

			var list = new List<string>();
			foreach (IMoreHostItem host in historyBox.SelectedItems)
			{
				if (host.Tag is HierarchyInfo info)
				{
					list.Add(info.PageId);
				}
			}

			if (list.Count > 0)
			{
				SetVisited(list.Last());
				await provider.PinPages(list);
				await LoadPinned();
			}
		}


		private async void UnpinOnClick(object sender, EventArgs e)
		{
			if (pinnedBox.SelectedItems.Count == 0)
			{
				return;
			}

			var list = new List<string>();
			foreach (IMoreHostItem host in pinnedBox.SelectedItems)
			{
				if (host.Tag is HierarchyInfo info)
				{
					list.Add(info.PageId);
				}
			}

			if (list.Count > 0)
			{
				var i = historyBox.SelectedItems.Count - 1;
				if (historyBox.SelectedItems[i] is IMoreHostItem item &&
					item.Control.Tag is HierarchyInfo info)
				{
					SetVisited(info.PageId);
				}

				await provider.UnpinPages(list);
				await LoadPinned();
			}
		}


		private void ShowPageOutline(HierarchyInfo info)
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
