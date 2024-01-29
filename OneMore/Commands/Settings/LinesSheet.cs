//************************************************************************************************
// Copyright © 2021 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class LinesSheet : SheetBase
	{

		public LinesSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "LinesSheet";
			Title = Resx.LinesSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"colorLabel",
					"clickLabel",
					"lengthLabel"
				});
			}

			introBox.SetMultilineWrapWidth(introBox.Width);
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// set these after ThemeManager does its thing
			var settings = provider.GetCollection(Name);

			lengthBox.Value = settings.Get<decimal>("length", 100);
			colorBox.BackColor = settings.Get("color", Color.Black);
		}


		private void ChangeLineColor(object sender, System.EventArgs e)
		{
			var location = PointToScreen(colorBox.Location);

			using var dialog = new UI.MoreColorDialog(Resx.PageColorDialog_Text,
				location.X + colorBox.Bounds.Location.X + (colorBox.Width / 2),
				location.Y - 50);

			dialog.Color = colorBox.BackColor;

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				colorBox.BackColor = dialog.Color;
			}
		}


		public override bool CollectSettings()
		{
			var settings = provider.GetCollection(Name);
			settings.Add("color", colorBox.BackColor.ToRGBHtml());
			settings.Add("length", ((int)(lengthBox.Value)));
			provider.SetCollection(settings);

			return false;
		}
	}
}
