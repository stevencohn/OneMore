//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All Rights Reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using System.Drawing;
	using System.Windows.Forms;
	using River.OneMoreAddIn.Commands;
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
					"deepRadio"
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


			DrawScheme(e.Graphics, picture.BackColor, scheme, extendBox.Checked);
		}


		private static void DrawScheme(
			Graphics g, Color backColor, HighlightColorScheme scheme, bool extended)
		{
			// LightGrayBorder = #E2E4E7

			g.Clear(backColor);

			int x = 0;
			foreach (var color in scheme.Colors)
			{
				using (var brush = new SolidBrush(ColorTranslator.FromHtml(color)))
				{
					g.FillRectangle(brush, x, BoxTop, BoxSize, BoxSize);
					g.DrawRectangle(Pens.LightGray, x, BoxTop, BoxSize - 1, BoxSize);
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
						g.DrawRectangle(Pens.LightGray, x, BoxTop, BoxSize - 1, BoxSize);
					}

					x += BoxSize + BoxSpacing;
				}
			}
		}
	}
}
