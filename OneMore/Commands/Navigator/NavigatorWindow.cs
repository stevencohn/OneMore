//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Extensions;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;
	using HierarchyInfo = OneNote.HierarchyInfo;


	internal partial class NavigatorWindow : LocalizableForm
	{
		private const int WindowMargin = 20;

		private static string visitedID;

		private Screen screen;
		private Point corral;
		private readonly List<HierarchyInfo> history;
		private readonly List<HierarchyInfo> pinned;


		// disposed
		private readonly NavigationProvider provider;


		public NavigatorWindow()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				//Text = Resx.NavigatorWindow_Text;

				Localize(new string[]
				{
					"closeButton"
				});
			}

			ManualLocation = true;
			TopMost = true;

			provider = new NavigationProvider();
			provider.Navigated += Navigated;

			history = new List<HierarchyInfo>();
			pinned = new List<HierarchyInfo>();

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth * 2;

			historyBox.FullRowSelect = true;
			historyBox.Columns.Add(
				new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });
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

			Left = corral.X;
			Top = SystemInformation.CaptionHeight + WindowMargin;

			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				Left = WindowMargin;
			}

			// designer defines width but height is calculated
			MaximumSize = new Size(MaximumSize.Width, screen.WorkingArea.Height - (WindowMargin * 2));

			Navigated(null, await provider.ReadHistory());
		}


		private void SetLimitsOnSizeChanged(object sender, EventArgs e)
		{
			// SizeChanged is invoked after Load which sets screenArea
			corral = screen.GetBoundedLocation(this);
		}


		private void RestrictOnMove(object sender, EventArgs e)
		{
			if (corral.X > 0)
			{
				if (Left < 10) Left = 10;
				if (Left > corral.X) Left = corral.X;
				if (Top < 10) Top = 10;
				if (Top > corral.Y) Top = corral.Y;
			}
		}


		private void TopOnShown(object sender, EventArgs e)
		{
			TopMost = false;
			TopMost = true;
			TopLevel = true;
			this.ForceTopMost();
		}


		private void CloseOnClick(object sender, EventArgs e)
		{
			Close();
		}
		#endregion Handlers


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		public static void SetVisited(string ID)
		{
			// need to lock?
			visitedID = ID;
		}


		private void Navigated(object sender, List<HistoryRecord> e)
		{
			if (historyBox.InvokeRequired)
			{
				historyBox.Invoke(new Action(() => Navigated(sender, e)));
				return;
			}

			if (e.Count > 0 && e[0].PageID == visitedID)
			{
				// user clicked ths page in navigator; don't reorder the list or they'll lose
				// their context and get confused
				return;
			}

			if (ResolveReferences(history, e))
			{
				visitedID = null;

				ShowPageOutline(history[0]);

				historyBox.Items.Clear();
				historyBox.BeginUpdate();

				history.ForEach(info =>
				{
					var control = new HistoryListViewItem("x", info);
					var item = historyBox.AddHostedItem(control);
				});

				historyBox.Items[0].Selected = true;
				historyBox.EndUpdate();

				historyBox.Invalidate();
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

				var item = j < 0
					? one.GetPageInfo(record.PageID)
					: details[j];

				item.Visited = record.Visited;

				list.Add(item);

				updated |= (j != i);
			}

			if (updated)
			{
				details.Clear();
				details.AddRange(list);
			}

			return updated;
		}


		private void ShowPageOutline(HierarchyInfo info)
		{
			using var one = new OneNote();
			var page = one.GetPage(info.PageId, OneNote.PageDetail.Basic);
			var headings = page.GetHeadings(one);

			head1Label.Text = page.Title;

			pageBox.Items.Clear();
			pageBox.Items.AddRange(headings.Select(h => h.Text).ToArray());
		}
	}
}
