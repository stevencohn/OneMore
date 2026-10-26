//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Helpers.Office;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A themed, borderless color picker shown in place at a screen location. It combines the
	/// Office-style swatches (Automatic, theme, standard and recent colors), a custom mixer
	/// (spectrum, brightness, RGB/HSL values and hex) and New/Current previews. It is the
	/// replacement for the former ColorDialog-based picker: set Color, call ShowDialog and,
	/// on OK, read Color or Automatic.
	/// </summary>
	internal sealed class MoreColorPicker : MoreForm
	{
		private const int FormWidth = 640;
		private const int PaneHeight = 332;

		// room below the Recent Colors for the expand button, regardless of which rows are shown
		private const int GlyphRoom = 24;
		private const int BarHeight = 38;
		private const int TitleHeight = 30;

		private readonly bool darkPage;
		private readonly int x;
		private readonly int y;
		private readonly float scale;

		private readonly ColorSwatchPane swatchPane;
		private readonly SpectrumControl spectrum;
		private readonly BrightnessControl brightness;
		private readonly MoreComboBox modeBox;
		private readonly MoreLabel[] labels = new MoreLabel[3];
		private readonly MoreNumericUpDown[] spinners = new MoreNumericUpDown[3];
		private readonly MoreLabel hexLabel;
		private readonly MoreTextBox hexBox;
		private readonly ColorSwatchControl newSwatch;
		private readonly ColorSwatchControl currentSwatch;
		private readonly MoreLabel darkHint;
		private readonly MoreButton okButton;
		private readonly MoreButton cancelButton;
		private readonly ExpandGlyph expandButton;
		private readonly ToolTip toolTip = new();

		private Color current = Color.Empty;
		private Color newColor = Color.Black;
		private double hue;
		private double saturation;
		private double value;
		private bool automatic;
		private bool eventing;
		private bool shown;
		private bool expanded;


		/// <summary>
		/// Initializes a new picker
		/// </summary>
		/// <param name="pageColor">
		/// The page background color; if dark then swatches are drawn as OneNote renders them
		/// on a dark page. Transparent or empty means the page follows the Office theme.
		/// </param>
		/// <param name="x">Screen X of the left edge; negative to right-justify at -x</param>
		/// <param name="y">Screen Y of the top edge</param>
		/// <param name="title">Optional context shown in the title row, e.g. Highlight Color</param>
		public MoreColorPicker(Color pageColor, int x, int y, string title = null)
			: this(IsDarkPage(pageColor), x, y, title)
		{
		}


		/// <summary>
		/// Initializes a new picker for a color that is not rendered by OneNote against a page
		/// background, such as a section color or a setting, so no dark-mode rendering applies.
		/// </summary>
		/// <param name="x">Screen X of the left edge; negative to right-justify at -x</param>
		/// <param name="y">Screen Y of the top edge</param>
		/// <param name="title">Optional context shown in the title row</param>
		public MoreColorPicker(int x, int y, string title = null)
			: this(false, x, y, title)
		{
		}


		private MoreColorPicker(bool darkPage, int x, int y, string title)
		{
			this.darkPage = darkPage;

			Text = title ?? string.Empty;
			this.x = x;
			this.y = y;

			AutoScaleMode = AutoScaleMode.None;
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ManualLocation = true;
			ShowInTaskbar = false;
			ApplyDialogOpacity = false;
			KeyPreview = true;

			swatchPane = new ColorSwatchPane();
			scale = Scaling.GetDpi(this) / 96f;
			swatchPane.Scale = scale;

			swatchPane.Bounds = new Rectangle(S(1), S(TitleHeight), swatchPane.GetContentSize().Width, S(PaneHeight));
			swatchPane.ColorPicked += c => SetColor(c, false, true);
			swatchPane.AutomaticPicked += PickAutomatic;

			spectrum = new SpectrumControl { Bounds = Rect(300, 14, 180, 158) };
			spectrum.Picked += SpectrumPicked;

			brightness = new BrightnessControl { Bounds = Rect(490, 14, 32, 158) };
			brightness.Picked += BrightnessPicked;

			modeBox = new MoreComboBox
			{
				DropDownStyle = ComboBoxStyle.DropDownList,
				Bounds = Rect(392, 182, 100, 24)
			};

			modeBox.Items.AddRange(new object[] { "RGB", "HSL" });
			modeBox.SelectedIndex = 0;
			modeBox.SelectedIndexChanged += ChangeMode;

			var modeLabel = NewLabel(Resx.MoreColorPicker_colorModel, 300, 182, 90, 24);
			Controls.Add(modeLabel);
			CenterOn(modeLabel, modeBox);

			for (var i = 0; i < 3; i++)
			{
				labels[i] = NewLabel(string.Empty, 300, 212 + (i * 26), 90, 24);
				spinners[i] = new MoreNumericUpDown { Bounds = Rect(392, 212 + (i * 26), 70, 24) };
				spinners[i].ValueChanged += SpinnerChanged;
				Controls.Add(labels[i]);
				Controls.Add(spinners[i]);
				CenterOn(labels[i], spinners[i]);
			}

			hexLabel = NewLabel(Resx.MoreColorPicker_hex, 300, 290, 90, 24);
			hexBox = new MoreTextBox { Bounds = Rect(392, 290, 80, 24), MaxLength = 7 };
			CenterOn(hexLabel, hexBox);
			hexBox.TextChanged += HexChanged;
			hexBox.Leave += (s, e) => ShowHex();

			Controls.Add(NewLabel(Resx.word_New, 560, 24, 64));
			newSwatch = NewPreview(560, 48, 64, 40);
			currentSwatch = NewPreview(560, 88, 64, 40);
			Controls.Add(NewLabel(Resx.MoreColorPicker_current, 560, 134, 64));

			if (darkPage)
			{
				// let the user know the swatches show how OneNote renders colors on a dark page
				darkHint = NewLabel(Resx.MoreColorPicker_darkPage, 556, 172, 72);
				darkHint.Height = S(64);
				darkHint.TextAlign = ContentAlignment.TopCenter;
				Controls.Add(darkHint);
			}

			okButton = new MoreButton
			{
				Text = Resx.word_OK,
				DialogResult = DialogResult.OK
			};

			cancelButton = new MoreButton
			{
				Text = Resx.word_Cancel,
				DialogResult = DialogResult.Cancel
			};

			expandButton = new ExpandGlyph();
			expandButton.Click += ToggleExpanded;

			AcceptButton = okButton;
			CancelButton = cancelButton;

			Controls.AddRange(new Control[]
			{
				swatchPane, spectrum, brightness, modeBox, hexLabel, hexBox,
				okButton, cancelButton, expandButton
			});

			SetModeLabels();

			// start collapsed, showing only the swatches
			ApplyLayout();
		}


		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				toolTip.Dispose();
			}

			base.Dispose(disposing);
		}


		/// <summary>
		/// Gets or sets the color. Before showing, this is the current color; after OK, it is
		/// the chosen color, or Color.Transparent if Automatic was chosen.
		/// </summary>
		public Color Color
		{
			get => automatic ? Color.Transparent : newColor;
			set => current = value;
		}


		/// <summary>
		/// Gets or sets whether the Automatic choice is offered; default true. Set false for
		/// colors that have no automatic equivalent.
		/// </summary>
		public bool AutomaticVisible { get; set; } = true;


		/// <summary>
		/// Gets a value indicating whether the Automatic color was chosen
		/// </summary>
		public bool Automatic => automatic;


		protected override CreateParams CreateParams
		{
			get
			{
				// force a drop-shadow on the borderless window
				const int CS_DROPSHADOW = 0x20000;
				var cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;
			}
		}


		/// <summary>
		/// Gets the color OneNote shows for the given color on this page
		/// </summary>
		private Color Rendered(Color color)
		{
			return darkPage ? ColorPalette.ToDark(color) : color;
		}


		/// <summary>
		/// Determines whether a page with the given background color renders in dark mode;
		/// transparent or empty means the page follows the Office theme.
		/// </summary>
		internal static bool IsDarkPage(Color pageColor)
		{
			return pageColor.IsEmpty || pageColor.A == 0
				? Office.IsBlackThemeEnabled()
				: pageColor.GetBrightness() < 0.5f;
		}


		private int S(int value) => (int)Math.Round(value * scale);


		/// <summary>
		/// Sizes the window and positions the buttons for the collapsed state, showing only the
		/// swatches, or the expanded state, which adds the custom color controls
		/// </summary>
		private void ApplyLayout()
		{
			var width = expanded ? S(FormWidth) : swatchPane.Right + S(1);

			// the swatch pane is as tall as its rows plus room for the expand button, so the
			// button has the same space whether or not the Automatic row is shown
			var paneHeight = swatchPane.RecentBottom > 0
				? swatchPane.RecentBottom + S(GlyphRoom)
				: S(PaneHeight);

			swatchPane.Height = paneHeight;

			var top = S(TitleHeight) + paneHeight + S(6);

			ClientSize = new Size(width, S(TitleHeight) + paneHeight + S(BarHeight));

			// the expand button is fixed in the gap between the Recent Colors and the divider
			// line above the button row, at the right end of the swatch columns, so it stays put
			// when the window changes size
			var recentBottom = swatchPane.Top + swatchPane.RecentBottom;
			var divider = S(TitleHeight) + paneHeight;
			var gap = divider - recentBottom;
			var glyphHeight = Math.Min(S(19), Math.Max(S(8), gap - S(4)));
			var glyphWidth = glyphHeight * 3 / 2;
			expandButton.Bounds = new Rectangle(
				swatchPane.Left + swatchPane.ContentRight - glyphWidth,
				recentBottom + ((gap - glyphHeight) / 2),
				glyphWidth, glyphHeight);
			expandButton.BringToFront();

			var right = width - S(expanded ? 20 : 10);
			cancelButton.Bounds = new Rectangle(right - S(80), top, S(80), S(26));
			okButton.Bounds = new Rectangle(cancelButton.Left - S(10) - S(80), top, S(80), S(26));
			expandButton.Expanded = expanded;

			toolTip.SetToolTip(expandButton,
				expanded ? Resx.MoreColorPicker_collapse : Resx.MoreColorPicker_expand);

			foreach (Control control in Controls)
			{
				if (control != swatchPane && control != okButton &&
					control != cancelButton && control != expandButton)
				{
					control.Visible = expanded;
				}
			}

			Invalidate();
		}


		private void ToggleExpanded(object sender, EventArgs e)
		{
			expanded = !expanded;
			ApplyLayout();

			// keep the wider window on screen
			var area = Screen.FromControl(this).WorkingArea;
			if (Right > area.Right)
			{
				Left = Math.Max(area.Left, area.Right - Width);
			}
		}


		private Rectangle Rect(int rx, int ry, int rw, int rh)
		{
			return new Rectangle(S(rx), S(ry + TitleHeight), S(rw), S(rh));
		}


		private MoreLabel NewLabel(string text, int lx, int ly, int lw, int lh = 20)
		{
			return new MoreLabel
			{
				Text = text,
				AutoSize = false,
				Bounds = Rect(lx, ly, lw, lh),
				TextAlign = ContentAlignment.MiddleLeft
			};
		}


		/// <summary>
		/// Keeps the label vertically centered on its input box. The box may size itself to its
		/// font, so follow its actual bounds rather than the requested ones.
		/// </summary>
		private static void CenterOn(Control label, Control box)
		{
			void Center()
			{
				label.Top = box.Top + ((box.Height - label.Height) / 2);
			}

			box.SizeChanged += (s, e) => Center();
			box.LocationChanged += (s, e) => Center();
			Center();
		}


		private ColorSwatchControl NewPreview(int px, int py, int pw, int ph)
		{
			var swatch = new ColorSwatchControl
			{
				Bounds = Rect(px, py, pw, ph),
				Cursor = Cursors.Default
			};

			Controls.Add(swatch);
			return swatch;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			BackColor = manager.GetColor("Window");
			brightness.BackColor = BackColor;
			expandButton.BackColor = BackColor;

			// labels are themed to the control color; let the picker background show through
			foreach (var label in Controls.OfType<MoreLabel>())
			{
				label.BackColor = Color.Transparent;
			}

			if (darkHint is not null)
			{
				darkHint.ForeColor = manager.GetColor("GrayText");
			}

			swatchPane.Initialize(darkPage, AutomaticVisible, RecentColors.Load());

			// the expand button is anchored to the swatch layout, which is now known
			ApplyLayout();

			if (current.IsEmpty || current.A == 0)
			{
				currentSwatch.Color = Color.Empty;
				if (AutomaticVisible)
				{
					PickAutomatic();
				}
				else
				{
					SetColor(Color.White, false, true);
				}
			}
			else
			{
				currentSwatch.Color = Rendered(current);
				SetColor(current, false, true);
			}

			var screen = Screen.FromPoint(new Point(Math.Abs(x), y)).WorkingArea;
			var left = x < 0 ? -x - Width : x;
			Location = new Point(
				Math.Max(screen.Left, Math.Min(left, screen.Right - Width)),
				Math.Max(screen.Top, Math.Min(y, screen.Bottom - Height)));
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			shown = true;
		}


		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);

			// behave like a popup; clicking away cancels
			if (shown && DialogResult == DialogResult.None && Visible)
			{
				DialogResult = DialogResult.Cancel;
			}
		}


		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK && !automatic)
			{
				RecentColors.Add(newColor);
			}

			base.OnFormClosing(e);
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			using var pen = new Pen(manager.GetColor("ButtonBorder"));
			e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

			// title row and its divider
			var top = S(TitleHeight) - 1;
			if (!string.IsNullOrEmpty(Text))
			{
				using var font = new Font(Font, FontStyle.Bold);
				using var brush = new SolidBrush(manager.GetColor("WindowText"));
				var size = e.Graphics.MeasureString(Text, font);
				e.Graphics.DrawString(Text, font, brush, S(12), (top - size.Height) / 2);
			}

			e.Graphics.DrawLine(pen, 1, top, Width - 2, top);

			// divider above the button bar
			var bar = swatchPane.Bottom;
			e.Graphics.DrawLine(pen, 1, bar, Width - 2, bar);

			// divider between swatches and mixer
			if (expanded)
			{
				var dx = swatchPane.Right;
				e.Graphics.DrawLine(pen, dx, top, dx, bar);
			}
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void PickAutomatic()
		{
			SetColor(darkPage ? Color.White : Color.Black, true, true);
		}


		/// <summary>
		/// Makes the given color the new color and updates all controls
		/// </summary>
		private void SetColor(Color color, bool isAutomatic, bool updateInputs)
		{
			automatic = isAutomatic;
			newColor = Color.FromArgb(color.R, color.G, color.B);

			// the spectrum and slider show colors as OneNote renders them, so on a dark page
			// they are positioned by the rendered color rather than the logical color
			ColorMath.ToHsv(isAutomatic ? newColor : Rendered(newColor), out var h, out var s, out var v);

			// hue is undefined for grays and saturation for black, so keep the last values
			// so the cursors don't jump to the corner
			if (s > 0 && v > 0)
			{
				hue = h;
			}

			if (v > 0)
			{
				saturation = s;
			}

			value = v;

			RefreshInputs(updateInputs, updateInputs);
		}


		private void RefreshInputs(bool numbers, bool hex)
		{
			eventing = true;
			try
			{
				spectrum.SetPosition(hue, saturation);
				brightness.SetColor(hue, saturation, value);
				newSwatch.Color = automatic ? newColor : Rendered(newColor);
				swatchPane.Select(newColor, automatic);

				if (numbers)
				{
					ShowNumbers();
				}

				if (hex)
				{
					ShowHex();
				}
			}
			finally
			{
				eventing = false;
			}
		}


		private void SpectrumPicked(double h, double s)
		{
			hue = h;
			saturation = s;
			SetFromHsv();
		}


		private void BrightnessPicked(double v)
		{
			value = v;
			SetFromHsv();
		}


		private void SetFromHsv()
		{
			// on a dark page the position describes the rendered color; the color applied to the
			// page is the logical color that OneNote renders as that color
			automatic = false;
			var view = ColorMath.FromHsv(hue, saturation, value);
			newColor = darkPage ? ColorPalette.ToLight(view) : view;
			RefreshInputs(true, true);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private bool IsHsl => modeBox.SelectedIndex == 1;


		private void ChangeMode(object sender, EventArgs e)
		{
			SetModeLabels();
			eventing = true;
			try
			{
				ShowNumbers();
			}
			finally
			{
				eventing = false;
			}
		}


		private void SetModeLabels()
		{
			var names = IsHsl
				? new[] { Resx.MoreColorPicker_hue, Resx.word_Saturation, Resx.MoreColorPicker_luminance }
				: new[] { Resx.MoreColorPicker_red, Resx.MoreColorPicker_green, Resx.MoreColorPicker_blue };

			for (var i = 0; i < 3; i++)
			{
				labels[i].Text = names[i] + ":";
			}
		}


		private void ShowNumbers()
		{
			if (IsHsl)
			{
				ColorMath.ToHsl(newColor, out var h, out var s, out var l);
				Set(0, 360, Math.Round(h) % 361);
				Set(1, 100, Math.Round(s * 100));
				Set(2, 100, Math.Round(l * 100));
			}
			else
			{
				Set(0, 255, newColor.R);
				Set(1, 255, newColor.G);
				Set(2, 255, newColor.B);
			}
		}


		private void Set(int index, int maximum, double number)
		{
			var spinner = spinners[index];
			spinner.Minimum = 0;
			spinner.Maximum = maximum;
			spinner.Value = (decimal)Math.Max(0, Math.Min(maximum, number));
		}


		private void SpinnerChanged(object sender, EventArgs e)
		{
			if (eventing)
			{
				return;
			}

			var a = (double)spinners[0].Value;
			var b = (double)spinners[1].Value;
			var c = (double)spinners[2].Value;

			var color = IsHsl
				? ColorMath.FromHsl(a, b / 100.0, c / 100.0)
				: Color.FromArgb((int)a, (int)b, (int)c);

			// leave the numbers alone while the user is stepping or typing
			automatic = false;
			newColor = color;

			ColorMath.ToHsv(Rendered(color), out var h, out var s, out var v);
			if (IsHsl)
			{
				hue = a;
				saturation = s;
			}
			else if (s > 0 && v > 0)
			{
				hue = h;
				saturation = s;
			}

			value = v;
			RefreshInputs(false, true);
		}


		private void ShowHex()
		{
			var wasEventing = eventing;
			eventing = true;
			hexBox.Text = ColorMath.ToHex(newColor);
			eventing = wasEventing;
		}


		private void HexChanged(object sender, EventArgs e)
		{
			if (eventing || !ColorMath.TryParseHex(hexBox.Text, out var color))
			{
				return;
			}

			SetColor(color, false, false);
			eventing = true;
			try
			{
				ShowNumbers();
			}
			finally
			{
				eventing = false;
			}
		}
	}
}
