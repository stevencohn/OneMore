//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ColorsSheet : SheetBase
	{
		private const string LegacyName = "LinesSheet";


		public ColorsSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "ColorsSheet";
			Title = Resx.ColorsSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"linesGroupBox",
					"lengthLabel=word_Length",
					"lineColorLabel=word_Color",
					"lineClickLabel=ColorsSheet_clickLabel.Text",
					"strikeGroupBox",
					"strikeBox",
					"strikeColorLabel=word_Color",
					"strikeClickLabel=ColorsSheet_clickLabel.Text",
					"lengthLabel=word_label"
				});
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// reader-makes-right, converting from LinesSheet to ColorsSheet element...

			var defaultColor = ThemeManager.Instance.GetColor("ControlText");

			var settings = provider.GetCollection(LegacyName);
			if (settings.Count > 0)
			{
				lengthBox.Value = settings.Get<decimal>("length", 100);
				lineColorBox.BackColor = settings.Get("color", defaultColor);
			}
			else
			{
				settings = provider.GetCollection(Name);
				lengthBox.Value = settings.Get<decimal>("lineLength", 100);
				lineColorBox.BackColor = settings.Get("lineColor", defaultColor);
				strikeBox.Checked = settings.Get("strikeApply", false); ;
				strikeColorBox.BackColor = settings.Get("strikeColor", defaultColor);
			}
		}


		public static string GetStrikethroughForeColor()
		{
			// must be called after ThemeManager does its thing
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("ColorsSheet");

			return settings.Get<bool>("strikeApply")
				? settings.Get<string>("strikeColor")
				: null;
		}


		private void ApplyOnCheckedChanged(object sender, EventArgs e)
		{
			strikeColorBox.Enabled = strikeBox.Checked;

			strikeClickLabel.ForeColor = strikeBox.Checked
				? ThemeManager.Instance.GetColor("ControlText")
				: ThemeManager.Instance.GetColor("GrayText");

		}


		private void ChangeLineColor(object sender, EventArgs e)
		{
			if (sender is MorePictureBox box)
			{
				var location = PointToScreen(box.Location);

				using var dialog = new MoreColorDialog(
					// parent should be groupbox, so grab its text as dialog title
					box.Parent.Text,
					location.X + box.Bounds.Location.X + (box.Width / 2),
					location.Y - 50);

				dialog.Color = box.BackColor;

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					box.BackColor = dialog.Color;
				}
			}
		}


		public override bool CollectSettings()
		{
			provider.RemoveCollection(LegacyName);

			var settings = provider.GetCollection(Name);
			settings.Add("lineColor", lineColorBox.BackColor.ToRGBHtml());
			settings.Add("lineLength", ((int)(lengthBox.Value)));
			settings.Add("strikeApply", strikeBox.Checked.ToString());
			settings.Add("strikeColor", strikeColorBox.BackColor.ToRGBHtml());
			provider.SetCollection(settings);

			return false;
		}
	}
}
