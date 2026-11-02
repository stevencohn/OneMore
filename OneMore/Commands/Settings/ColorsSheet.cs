//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class ColorsSheet : SheetBase
	{
		private const string LegacyName = "LinesSheet";

		// the colors are applied to page content so, as in MoreColorPicker, the swatches show
		// how OneNote renders them on a dark page while the stored value stays the applied color
		private bool darkPage;


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

			darkPage = MoreColorPicker.IsDarkPage(Color.Empty);

			// black is rendered as a light color on a dark page
			var defaultColor = Color.Black;

			var settings = provider.GetCollection(LegacyName);
			if (settings.Count > 0)
			{
				lengthBox.Value = settings.Get<decimal>("length", 100);
				SetSwatch(lineColorBox, settings.Get("color", defaultColor));
			}
			else
			{
				settings = provider.GetCollection(Name);
				lengthBox.Value = settings.Get<decimal>("lineLength", 100);
				SetSwatch(lineColorBox, settings.Get("lineColor", defaultColor));
				strikeBox.Checked = settings.Get("strikeApply", false); ;
				SetSwatch(strikeColorBox, settings.Get("strikeColor", defaultColor));
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

				using var picker = new MoreColorPicker(Color.Empty,
					location.X + box.Bounds.Location.X + (box.Width / 2),
					location.Y - 50,
					// parent should be groupbox, so grab its text as dialog title
					box.Parent.Text)
				{
					Color = GetSwatch(box),
					AutomaticVisible = false
				};

				if (picker.ShowDialog(this) == DialogResult.OK)
				{
					SetSwatch(box, picker.Color);
				}
			}
		}


		private void SetSwatch(MorePictureBox box, Color color)
		{
			box.Tag = color;
			box.BackColor = darkPage ? ColorPalette.ToDark(color) : color;
		}


		private static Color GetSwatch(MorePictureBox box)
		{
			return box.Tag is Color color ? color : box.BackColor;
		}


		public override bool CollectSettings()
		{
			provider.RemoveCollection(LegacyName);

			var settings = provider.GetCollection(Name);
			settings.Add("lineColor", GetSwatch(lineColorBox).ToRGBHtml());
			settings.Add("lineLength", ((int)(lengthBox.Value)));
			settings.Add("strikeApply", strikeBox.Checked.ToString());
			settings.Add("strikeColor", GetSwatch(strikeColorBox).ToRGBHtml());
			provider.SetCollection(settings);

			return false;
		}
	}
}
