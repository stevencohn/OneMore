//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;


	internal class MoreNumericUpDown : NumericUpDown
	{
		private readonly ThemeManager manager;
		private Color foreColor;
		private Color grayColor;


		public MoreNumericUpDown()
			: base()
		{
			manager = ThemeManager.Instance;
		}


		public string PreferredBack { get; set; }

		public string PreferredFore { get; set; } = "ControlText";


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);

			if (Enabled)
			{
				ForeColor = foreColor;
			}
			else
			{
				if (grayColor == Color.Empty)
				{
					foreColor = string.IsNullOrEmpty(PreferredFore)
						? ForeColor
						: manager.GetThemedColor(PreferredFore);

					grayColor = manager.GetThemedColor("GrayText");
				}

				ForeColor = grayColor;
			}
		}
	}
}
