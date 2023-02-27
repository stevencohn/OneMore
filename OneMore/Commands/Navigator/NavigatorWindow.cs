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


	internal partial class NavigatorWindow : LocalizableForm
	{
		private const int WindowMargin = 20;

		private Screen screen;
		private Point corral;
		private readonly List<OneNote.HierarchyInfo> history;
		private readonly List<OneNote.HierarchyInfo> pinned;


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

			history = new List<OneNote.HierarchyInfo>();
			pinned = new List<OneNote.HierarchyInfo>();
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
			if (ResolveReferences(history, e))
			{
				historyBox.Items.Clear();
				historyBox.Items.AddRange(history.Select(h => h.Name).ToArray());
				historyBox.Invalidate();

				//historyBox.Items.AddRange(history.ToArray());
			}
		}


		private bool ResolveReferences(List<OneNote.HierarchyInfo> details, List<string> ids)
		{
			using var one = new OneNote();
			var list = new List<OneNote.HierarchyInfo>();
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
	}
}
