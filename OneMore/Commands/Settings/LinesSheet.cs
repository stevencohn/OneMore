//************************************************************************************************
// Copyright © 2021 teven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
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

			var settings = provider.GetCollection(Name);

			colorBox.BackColor = settings.Get<Color>("color", Color.Black);
			lengthBox.Value = settings.Get<decimal>("length", 100);
		}


		private void ChangeLineColor(object sender, System.EventArgs e)
		{
			var location = PointToScreen(colorBox.Location);

			using (var dialog = new UI.MoreColorDialog(Resx.PageColorDialog_Text,
				location.X + colorBox.Bounds.Location.X + (colorBox.Width / 2),
				location.Y - 50))
			{
				dialog.Color = colorBox.BackColor;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					colorBox.BackColor = dialog.Color;
				}
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
