//************************************************************************************************
// Copyright © 2023 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;


	internal partial class NavigatorWindow : LocalizableForm
	{
		private int maxLeft;
		private int maxTop;

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
		}


		private async void PositionOnLoad(object sender, EventArgs e)
		{
			// deal with primary/secondary displays in either duplicate or extended mode...

			Rectangle area;
			using var one = new OneNote();
			var screen = Screen.FromHandle(one.WindowHandle);
			Location = screen.WorkingArea.Location;
			area = screen.WorkingArea;

			MaximumSize = new Size(MaximumSize.Width, area.Height - 50);

			var (scalingX, scalingY) = UIHelper.GetScalingFactors();

			// must add to area.X here to handle extended mode in which the coord of the secondary
			// display is an extension of the first, so X would be greater than zero...

			Left = (int)(area.X + (area.Width - Width - (10 * scalingX)));
			Top = (int)((SystemInformation.CaptionHeight + 5) * scalingY);

			maxLeft = Left;
			maxTop = area.Height - Height - 50;

			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				Left = (int)(10 * scalingX);
			}

			Navigated(null, await provider.ReadHistory());
		}


		private void RestrictOnMove(object sender, EventArgs e)
		{
			if (maxLeft > 0)
			{
				if (Left < 10) Left = 10;
				if (Left > maxLeft) Left = maxLeft;
				if (Top < 10) Top = 10;
				if (Top > maxTop) Top = maxTop;
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
			historyBox.Items.Clear();
			historyBox.Items.AddRange(e.ToArray());
		}
	}
}
