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
	using PageInfo = OneNote.HierarchyInfo;


	internal partial class NavigatorWindow : LocalizableForm
	{
		private const int WindowMargin = 20;

		private Screen screen;
		private Point corral;
		private readonly List<PageInfo> history;
		private readonly List<PageInfo> pinned;


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

			history = new List<PageInfo>();
			pinned = new List<PageInfo>();

			var rowWidth = Width - SystemInformation.VerticalScrollBarWidth;

			historyBox.FullRowSelect = true;
			historyBox.Columns.Add(
				new MoreColumnHeader(string.Empty, rowWidth) { AutoSizeItems = true });
		}


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


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void Navigated(object sender, List<string> e)
		{
			if (historyBox.InvokeRequired)
			{
				historyBox.Invoke(new Action(() => Navigated(sender, e)));
			}

			if (ResolveReferences(history, e))
			{
				ShowPageOutline(history[0]);

				historyBox.Items.Clear();
				historyBox.BeginUpdate();

				history.ForEach(h =>
				{
					var control = new HistoryListViewItem("x", h.Name, h.Link);
					var item = historyBox.AddHostedItem(control);
				});

				historyBox.Items[0].Selected = true;
				historyBox.EndUpdate();

				historyBox.Invalidate();
			}
		}


		private bool ResolveReferences(List<PageInfo> details, List<string> ids)
		{
			using var one = new OneNote();
			var list = new List<PageInfo>();
			var updated = false;

			// iterate manually to check both existence and order
			for (int i = 0;  i < ids.Count; i++)
			{
				var id = ids[i];
				var j = details.FindIndex(d => d.PageId == id);

				var item = j < 0
					? one.GetPageInfo(id)
					: details[j];

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


		private void ShowPageOutline(PageInfo info)
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
