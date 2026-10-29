//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Drawing;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.UI;
	using Resx = Properties.Resources;


	internal partial class HighlightsSheet : SheetBase
	{
		private const int BoxSpacing = 6;
		private const int BoxSize = 37;
		private const int BoxTop = 2;

		private readonly HighlightColorSchemes schemes;


		public HighlightsSheet(SettingsProvider provider) : base(provider)
		{
			InitializeComponent();

			Name = "HighlightsSheet";
			Title = Resx.HighlightsSheet_Title;

			if (NeedsLocalizing())
			{
				Localize(new string[]
				{
					"introBox",
					"themesGroup",
					"extendBox",
					"normalRadio",
					"fadedRadio",
					"deepRadio",
					"modeLabel",
					"lightRadio",
					"darkRadio"
				});
			}

			var settings = provider.GetCollection(Name);
			var theme = settings.Get<string>("theme");
			if (theme == "Faded")
			{
				fadedRadio.Checked = true;
			}
			else if (theme == "Deep")
			{
				deepRadio.Checked = true;
			}

			schemes = new HighlightColorSchemes();

			normalPicture.Paint += PaintScheme;
			fadedPicture.Paint += PaintScheme;
			deepPicture.Paint += PaintScheme;

			extendBox.Checked = settings.Get<bool>("extended");

			// preview defaults to the Office theme; not persisted
			if (Office.IsBlackThemeEnabled())
			{
				darkRadio.Checked = true;
			}

			ApplyBackdrop();
		}


		public override bool CollectSettings()
		{
			string theme = "Normal";
			if (fadedRadio.Checked) theme = "Faded";
			else if (deepRadio.Checked) theme = "Deep";

			var settings = provider.GetCollection(Name);
			settings.Add("theme", theme);

			if (extendBox.Checked)
			{
				settings.Add("extended", extendBox.Checked);
			}
			else
			{
				settings.Remove("extended");
			}

			provider.SetCollection(settings);

			// restart not required
			return false;
		}


		private void ClickPicture(object sender, System.EventArgs e)
		{
			if (sender == fadedPicture)
			{
				fadedRadio.Checked = true;
			}
			else if (sender == deepPicture)
			{
				deepRadio.Checked = true;
			}
			else
			{
				normalRadio.Checked = true;
			}
		}

		private bool Dark => darkRadio.Checked;


		private void ChangeMode(object sender, System.EventArgs e)
		{
			ApplyBackdrop();
		}


		// one backdrop behind all rows emulates a page of the selected color
		private void ApplyBackdrop()
		{
			var color = Dark ? ColorPalette.Theme(true)[0] : Color.White;
			backdrop.BackColor = color;
			normalPicture.BackColor = color;
			fadedPicture.BackColor = color;
			deepPicture.BackColor = color;
			DrawSchemes(null, null);
		}


		private void DrawSchemes(object sender, System.EventArgs e)
		{
			normalPicture.Invalidate();
			fadedPicture.Invalidate();
			deepPicture.Invalidate();
		}


		private void PaintScheme(object sender, PaintEventArgs e)
		{
			var picture = (PictureBox)sender;

			var scheme = picture switch
			{
				var p when p == fadedPicture => schemes.GetScheme("Faded"),
				var p when p == deepPicture => schemes.GetScheme("Deep"),
				_ => schemes.GetScheme("Bright")
			};


			DrawScheme(e.Graphics, picture.BackColor, scheme.GetSet(Dark), extendBox.Checked, Dark);
		}


		private static void DrawScheme(
			Graphics g, Color backColor, HighlightColorSet scheme, bool extended, bool dark)
		{
			// LightGrayBorder = #E2E4E7

			g.Clear(backColor);
			var border = dark ? Pens.DimGray : Pens.LightGray;

			int x = 0;
			foreach (var color in scheme.Colors)
			{
				using (var brush = new SolidBrush(ColorTranslator.FromHtml(color)))
				{
					g.FillRectangle(brush, x, BoxTop, BoxSize, BoxSize);
					g.DrawRectangle(border, x, BoxTop, BoxSize - 1, BoxSize);
				}

				x += BoxSize + BoxSpacing;
			}

			if (extended)
			{
				foreach (var color in scheme.Extended)
				{
					using (var brush = new SolidBrush(ColorTranslator.FromHtml(color)))
					{
						g.FillRectangle(brush, x, BoxTop, BoxSize, BoxSize);
						g.DrawRectangle(border, x, BoxTop, BoxSize - 1, BoxSize);
					}

					x += BoxSize + BoxSpacing;
				}
			}
		}
	}
}
