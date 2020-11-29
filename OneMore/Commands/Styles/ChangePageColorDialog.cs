//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class ChangePageColorDialog : UI.LocalizableForm
	{
		private readonly Color DarkColor = Color.FromArgb(0x21, 0x21, 0x21);

		private Color pageColor;
		private bool initialized = false;


		public ChangePageColorDialog(Color pageColor)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.ChangePageColorDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"lightButton",
					"darkButton",
					"customButton",
					"cancelButton",
					"okButton"
				});
			}

			this.pageColor = pageColor;

			if (pageColor.Equals(Color.White))
			{
				lightButton.Checked = true;
			}
			else if (pageColor.Equals(DarkColor))
			{
				darkButton.Checked = true;
			}
			else
			{
				customButton.Checked = true;
				SetCustomOption(pageColor);
			}
		}

		protected override void OnShown(EventArgs e)
		{
			Location = new Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
			initialized = true;
		}


		public string PageColor
		{
			get
			{
				if (pageColor.Equals(Color.White))
				{
					return "automatic";
				}

				return pageColor.ToRGBHtml();
			}
		}

		
		private void SetCustomOption(Color color)
		{
			pageColor = color;
			customButton.BackColor = color;
			customButton.ForeColor = color.GetBrightness() < 0.5 ? Color.White : Color.Black;
		}

		private void SetLightColor(object sender, EventArgs e)
		{
			pageColor = Color.White;
		}

		private void SetDarkColor(object sender, EventArgs e)
		{
			pageColor = DarkColor;
		}


		private void ChooseCustomColor(object sender, EventArgs e)
		{
			if (initialized && customButton.Checked)
			{
				var location = PointToScreen(customButton.Location);

				using (var dialog = new UI.ColorDialogEx(Resx.PageColorDialog_Text,
					location.X + customButton.Bounds.Location.X + (customButton.Width / 2),
					location.Y - 200))
				{
					dialog.Color = pageColor;

					if (dialog.ShowDialog() == DialogResult.OK)
					{
						SetCustomOption(dialog.Color);
					}
				}
			}
		}
	}
}
