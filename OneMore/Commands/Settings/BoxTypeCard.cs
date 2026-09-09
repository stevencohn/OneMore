//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Settings
{
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// One collapsible card in the "Box Types" section of the Snippets settings sheet,
	/// showing a swatch/title header that expands into an editable form for a single
	/// info/note/warn-style box type (built-in or custom). Its Width is set explicitly by
	/// the hosting BoxTypesPanel to match whatever space is available, rather than a fixed
	/// pixel width.
	/// </summary>
	internal class BoxTypeCard : MoreUserControl
	{
		private const int HeaderHeight = 44;
		private const int SwatchSize = 32;
		private const int RowHeight = 56;
		private const int ContentHeight = 20 + 24 + 10 + (RowHeight * 3) + 10 + 32 + 24; // buffered

		// fixed width of the expand/collapse indicator area at the right of the header,
		// computed once by EnsureIndicatorWidth() to fit the wider of the "Expand"/"Collapse"
		// labels so it stays constant as a card toggles - see UpdateIndicator()
		private static int indicatorWidth;

		// "#" + exactly 6 hex digits, matching the shipped InfoBoxThemes.json format (#RRGGBB)
		private static readonly Regex HexColorPattern = new(@"^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

		// 1-6 hex digits, no "#" - max Unicode codepoint is 10FFFF
		private static readonly Regex CodepointPattern = new(@"^[0-9A-Fa-f]{1,6}$", RegexOptions.Compiled);

		// Every card is built against the same (unparented, ambient) base font, so these
		// derived fonts are identical across every card - shared once here instead of
		// reallocating the same GDI Font object per card (there can be dozens of cards).
		private static Font boldTitleFont;
		private static Font smallTextFont;
		private static Font arrowFont;
		private static Font smallFieldFont;
		private static Font smallLinkFont;

		private readonly Swatch swatch;
		private readonly Panel shadingPanel;
		private readonly Label titleLabel;
		private readonly Label subLabel;
		private readonly Panel indicatorPanel;
		private readonly Label indicatorArrow;
		private readonly Label indicatorText;
		private readonly Panel contentPanel;

		// Built lazily by EnsureContentBuilt() the first time the card is expanded, since
		// most cards stay collapsed on initial render and building their ~35 controls up
		// front for every card is pure waste until the user actually opens one.
		private MoreTextBox titleBox;
		private MorePictureBox shadingSwatch;
		private MoreTextBox shadingBox;
		private MorePictureBox titleColorSwatch;
		private MoreTextBox titleColorBox;
		private MorePictureBox textColorSwatch;
		private MoreTextBox textColorBox;
		private MorePictureBox symbolColorSwatch;
		private MoreTextBox symbolColorBox;
		private MoreNumericUpDown symbolSizeBox;
		private MoreTextBox symbolCodeBox;

		private bool expanded;
		private bool contentBuilt;


		public BoxTypeCard(BoxType box)
		{
			Box = box;

			SuspendLayout();

			// Width is managed explicitly by the hosting cards panel (see BoxTypesPanel.LayoutCards),
			// not via Dock - this control is itself undocked, but positions/sizes its own
			// children with plain Dock/absolute layout.
			Height = HeaderHeight;
			BackColor = manager.GetColor("ControlLightLight");
			Margin = new Padding(0, 0, 0, 12);

			EnsureSharedFonts(Font);
			EnsureIndicatorWidth();

			// header (collapsed) row...

			// Explicit (not ambient) so it matches indicatorPanel's own MorePanel-themed
			// "Control" background exactly - otherwise the strip between the shading fill and
			// the indicator (which doesn't belong to either) shows the card's own, different,
			// ambient background and reads as a stray gap.
			var chrome = manager.GetColor("Control");

			var headerPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = HeaderHeight,
				Cursor = Cursors.Hand,
				BackColor = chrome
			};
			headerPanel.Click += ToggleExpanded;

			// The shading fill previews how the box will actually look on a page, extended
			// across the full header width up to the fixed-width indicator area at the right,
			// so title/sublabel text can sit on top of it. It's a plain absolutely-positioned
			// Panel, sent to the back (below) so it paints behind the swatch/labels/indicator,
			// resized on every headerPanel resize.
			var shadingColor = SafeColor(box.Shading);

			shadingPanel = new Panel
			{
				Location = new Point(0, 0),
				Height = HeaderHeight,
				BackColor = shadingColor
			};
			// shadingPanel is opaque and sits on top of most of headerPanel, so headerPanel's
			// own Click handler never sees clicks landing on it - it needs its own hookup too.
			shadingPanel.Click += ToggleExpanded;
			headerPanel.Resize += (s, e) =>
				shadingPanel.Width = headerPanel.ClientSize.Width - indicatorWidth;

			// The header's text labels are plain Labels, not the themed More* controls,
			// because their color must track the box's own shading/title color rather than
			// the Settings dialog's light/dark chrome - MoreLabel.OnLoad() would otherwise
			// unconditionally overwrite ForeColor to the dialog's theme text color. They're
			// parented to shadingPanel (not headerPanel), and their BackColor matches it,
			// because ThemeManager.Colorize() unconditionally resets a plain Label's BackColor
			// to its immediate parent's BackColor on load - parenting them to headerPanel
			// directly would silently overwrite this the first time the dialog loads.
			var chromeText = GetChromeTextColor(shadingColor);

			swatch = new Swatch(box)
			{
				Location = new Point(8, (HeaderHeight - SwatchSize) / 2),
				Size = new Size(SwatchSize, SwatchSize)
			};

			titleLabel = new Label
			{
				AutoSize = true,
				Font = boldTitleFont,
				Location = new Point(48, 6),
				ForeColor = SafeColor(box.TitleColor),
				BackColor = shadingColor,
				Text = box.Title
			};

			subLabel = new Label
			{
				AutoSize = true,
				Location = new Point(48, 24),
				ForeColor = chromeText,
				BackColor = shadingColor,
				Font = smallTextFont,
				Text = box.IsBuiltin ? Resx.word_Builtin : Resx.word_Custom
			};

			// Sits outside the shading fill, over the same chrome background set on
			// headerPanel above (MorePanel.OnLoad reapplies this same "Control" color itself,
			// so this is belt-and-suspenders, not a second source of truth). BottomBorderSize
			// is zeroed because MorePanel draws one by default, sized to this panel's own
			// 90px width rather than the full card - not a divider we asked for, and a
			// partial-width one reads as a rendering bug rather than a deliberate line.
			indicatorPanel = new MorePanel
			{
				Dock = DockStyle.Right,
				Width = indicatorWidth,
				BackColor = chrome,
				BottomBorderSize = 0
			};

			indicatorArrow = new MoreLabel
			{
				AutoSize = true,
				Font = arrowFont
			};

			indicatorText = new MoreLabel
			{
				AutoSize = true,
				Font = smallTextFont
			};

			indicatorPanel.Controls.Add(indicatorArrow);
			indicatorPanel.Controls.Add(indicatorText);
			UpdateIndicator();

			foreach (var control in new Control[]
				{ indicatorPanel, indicatorArrow, indicatorText, swatch, titleLabel, subLabel })
			{
				control.Click += ToggleExpanded;
			}

			shadingPanel.Controls.Add(titleLabel);
			shadingPanel.Controls.Add(subLabel);

			headerPanel.Controls.Add(shadingPanel);
			headerPanel.Controls.Add(indicatorPanel);
			headerPanel.Controls.Add(swatch);

			// z-order in the Controls collection runs opposite to what "added first" suggests -
			// index 0 is frontmost (what BringToFront/SetChildIndex(_, 0) rely on) - so the fill
			// must be explicitly sent to the back rather than just added before its siblings.
			shadingPanel.SendToBack();

			// content (expanded) form is built lazily - see EnsureContentBuilt() - since most
			// cards stay collapsed on initial render. This placeholder exists from the start so
			// Controls order/z-order (and Dock=Top layout below headerPanel) is stable.
			contentPanel = new Panel
			{
				Dock = DockStyle.Top,
				Height = 0,
				Visible = false,
				Padding = new Padding(12, 8, 12, 8),
				BackColor = BackColor
			};

			Controls.Add(contentPanel);
			Controls.Add(headerPanel);

			ResumeLayout(false);
		}


		public BoxType Box { get; }


		public event EventHandler Duplicate;
		public event EventHandler Delete;
		public event EventHandler Changed;


		public bool Expanded
		{
			get => expanded;
			set
			{
				if (expanded == value)
				{
					return;
				}

				expanded = value;

				if (expanded)
				{
					EnsureContentBuilt();
				}

				contentPanel.Visible = expanded;
				UpdateIndicator();
				Height = HeaderHeight + (expanded ? ContentHeight : 0);
			}
		}


		/// <summary>
		/// Initializes the small set of Font instances shared by every card, using the given
		/// (unparented, ambient) base font - the same value every card would otherwise compute
		/// for itself, since none of these fonts are read until after construction begins and
		/// before the card is parented into the cards panel.
		/// </summary>
		private static void EnsureSharedFonts(Font baseFont)
		{
			if (boldTitleFont is not null)
			{
				return;
			}

			boldTitleFont = new Font(baseFont, FontStyle.Bold);
			smallTextFont = new Font(baseFont.FontFamily, 8f);
			arrowFont = new Font(baseFont.FontFamily, 12f);
			smallFieldFont = new Font(baseFont.FontFamily, Math.Max(8f, baseFont.Size - 2f));
			smallLinkFont = new Font(baseFont.FontFamily, Math.Max(8f, baseFont.Size - 1.5f));
		}


		/// <summary>
		/// Computes the width of the expand/collapse indicator panel once, sized to fit
		/// whichever of the "Expand"/"Collapse" labels is wider (each rendered as a larger
		/// arrow glyph plus a smaller word, matching UpdateIndicator's layout), so the panel's
		/// width - and therefore the shaded header fill's right edge - stays fixed as a card
		/// toggles between the two states instead of jumping with the label's own width.
		/// </summary>
		private static void EnsureIndicatorWidth()
		{
			if (indicatorWidth > 0)
			{
				return;
			}

			int Measure(string full)
			{
				var arrowSize = TextRenderer.MeasureText(full.Substring(0, 1), arrowFont);
				var wordSize = TextRenderer.MeasureText(full.Substring(1).TrimStart(), smallTextFont);
				return arrowSize.Width + 4 + wordSize.Width + 12;
			}

			indicatorWidth = Math.Max(Measure(Resx.BoxTypeCard_Expand), Measure(Resx.BoxTypeCard_Collapse));
		}


		/// <summary>
		/// Builds the expanded content form (title box, color fields, symbol size/codepoint,
		/// footer links) the first time the card is expanded. Safe to call repeatedly; only
		/// the first call does anything.
		/// </summary>
		private void EnsureContentBuilt()
		{
			if (contentBuilt)
			{
				return;
			}

			contentBuilt = true;

			contentPanel.SuspendLayout();
			contentPanel.Height = ContentHeight;

			var titleCaption = MakeCaption(Resx.BoxTypeCard_TitleText, DockStyle.Top);

			titleBox = new MoreTextBox
			{
				Dock = DockStyle.Top,
				Height = 24,
				Text = Box.Title
			};
			titleBox.TextChanged += (s, e) =>
			{
				Box.Title = titleBox.Text;
				titleLabel.Text = titleBox.Text;
				swatch.Invalidate();
				RaiseChanged();
			};

			var titleSpacer = MakeSpacer(10);

			var fieldsTable = new TableLayoutPanel
			{
				Dock = DockStyle.Top,
				Height = RowHeight * 3,
				ColumnCount = 2,
				RowCount = 3,
				CellBorderStyle = TableLayoutPanelCellBorderStyle.None
			};
			fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			for (var i = 0; i < 3; i++)
			{
				fieldsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
			}

			(shadingSwatch, shadingBox) = AddColorField(
				fieldsTable, 0, 0, Resx.BoxTypeCard_Background, Box.Shading, c =>
				{
					Box.Shading = c;
					swatch.Invalidate();
					var shading = SafeColor(c);
					shadingPanel.BackColor = shading;
					titleLabel.BackColor = shading;
					subLabel.BackColor = shading;
					subLabel.ForeColor = GetChromeTextColor(shading);
				});

			(titleColorSwatch, titleColorBox) = AddColorField(
				fieldsTable, 1, 0, Resx.BoxTypeCard_TitleColor, Box.TitleColor, c =>
				{
					Box.TitleColor = c;
					titleLabel.ForeColor = SafeColor(c);
				});

			(symbolColorSwatch, symbolColorBox) = AddColorField(
				fieldsTable, 0, 1, Resx.BoxTypeCard_SymbolColor, Box.SymbolColor, c =>
				{
					Box.SymbolColor = c;
					swatch.Invalidate();
				});

			(textColorSwatch, textColorBox) = AddColorField(
				fieldsTable, 1, 1, Resx.BoxTypeCard_TextColor, Box.TextColor,
				c => Box.TextColor = c);

			var sizeGroup = new Panel
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(0),
				Margin = new Padding(0)
			};
			sizeGroup.Controls.Add(MakeCaption(Resx.BoxTypeCard_SymbolSize, DockStyle.Top, 0));

			symbolSizeBox = new MoreNumericUpDown
			{
				// nudged a few px left of x=28 (where the hex textboxes above sit) to
				// compensate for NumericUpDown's own text inset being wider than a plain
				// textbox's, so the "22" itself lines up with the hex value above it
				Location = new Point(28, 20),
				Size = new Size(80, 24),
				Minimum = 12,
				Maximum = 48,
				Value = Box.SymbolSize,
				Font = smallFieldFont
			};
			symbolSizeBox.ValueChanged += (s, e) =>
			{
				Box.SymbolSize = (int)symbolSizeBox.Value;
				swatch.Invalidate();
				RaiseChanged();
			};
			sizeGroup.Controls.Add(symbolSizeBox);
			fieldsTable.Controls.Add(sizeGroup, 0, 2);

			var codeGroup = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 0, 0, 0) };

			var codeRow = new Panel { Dock = DockStyle.Top, Height = 26 };

			// fixed width sized for up to 6 hex digits (max Unicode codepoint is 10FFFF),
			// with an explicit gap before the button rather than Dock=Fill/Right, which
			// would otherwise stretch a large blank gap between them at full card width.
			// Nudged a few px right of x=28 (where the hex textboxes above nominally sit)
			// to actually line up with them, per visual inspection.
			symbolCodeBox = new MoreTextBox
			{
				Location = new Point(32, 0),
				Size = new Size(84, 22),
				Text = Box.Symbol,
				Font = smallFieldFont
			};
			symbolCodeBox.TextChanged += (s, e) =>
			{
				MarkValidity(symbolCodeBox, CodepointPattern.IsMatch(symbolCodeBox.Text));
				Box.Symbol = symbolCodeBox.Text;
				swatch.Invalidate();
				RaiseChanged();
			};
			MarkValidity(symbolCodeBox, CodepointPattern.IsMatch(symbolCodeBox.Text));

			// "..." rather than a localized "Choose symbol..." label, so it needs no translation;
			// AutoSize=false with a narrow fixed width, just enough for the text plus modest padding
			var chooseButton = new MoreButton
			{
				Location = new Point(symbolCodeBox.Right + 8, 1),
				AutoSize = false,
				Size = new Size(32, 22),
				Text = "..."
			};
			chooseButton.Click += ChooseSymbol;

			codeRow.Controls.Add(symbolCodeBox);
			codeRow.Controls.Add(chooseButton);

			// Dock=Top stacks such that the LAST control added ends up visually topmost
			codeGroup.Controls.Add(codeRow);
			codeGroup.Controls.Add(MakeCaption(Resx.BoxTypeCard_SymbolCode, DockStyle.Top, 2));
			fieldsTable.Controls.Add(codeGroup, 1, 2);

			var fieldsSpacer = MakeSpacer(10);

			// footer...

			var footer = new Panel { Dock = DockStyle.Top, Height = 32 };

			var duplicateLink = new MoreLinkLabel
			{
				AutoSize = true,
				Location = new Point(0, 6),
				Font = smallLinkFont,
				Text = Resx.BoxTypeCard_Duplicate
			};
			duplicateLink.LinkClicked += (s, e) => Duplicate?.Invoke(this, EventArgs.Empty);
			footer.Controls.Add(duplicateLink);

			if (Box.IsBuiltin)
			{
				var resetLink = new MoreLinkLabel
				{
					AutoSize = true,
					Location = new Point(duplicateLink.Width + 16, 6),
					Font = smallLinkFont,
					Text = Resx.BoxTypeCard_ResetToDefault
				};
				resetLink.LinkClicked += ResetToDefault;
				footer.Controls.Add(resetLink);
			}
			else
			{
				var deleteLink = new MoreLinkLabel
				{
					Dock = DockStyle.Right,
					AutoSize = false,
					Width = 140,
					TextAlign = ContentAlignment.MiddleRight,
					ForeColor = Color.FromArgb(0xc0, 0x39, 0x2b),
					Font = smallLinkFont,
					Text = Resx.BoxTypeCard_DeleteBoxType
				};
				deleteLink.LinkClicked += ConfirmDelete;
				footer.Controls.Add(deleteLink);
			}

			// Dock=Top stacks such that the LAST control added ends up visually topmost
			contentPanel.Controls.Add(footer);
			contentPanel.Controls.Add(fieldsSpacer);
			contentPanel.Controls.Add(fieldsTable);
			contentPanel.Controls.Add(titleSpacer);
			contentPanel.Controls.Add(titleBox);
			contentPanel.Controls.Add(titleCaption);

			contentPanel.ResumeLayout(false);

			// This card's own one-time theming pass (MoreUserControl.OnLoad) already ran
			// back when the card was first shown, collapsed, before any of the above
			// existed - so none of it was themed by that walk. Re-run the same two-step
			// theming sequence MoreForm.OnLoad/SheetBase.OnLoad use elsewhere (Colorize via
			// InitializeTheme, then the ILoadControl.OnLoad() walk) scoped to the whole
			// card; re-applying to the already-themed header is harmless (idempotent).
			manager.InitializeTheme(this);
			LoadControlThemes(Controls);
		}


		private static void LoadControlThemes(Control.ControlCollection controls)
		{
			foreach (Control child in controls)
			{
				if (child is ILoadControl loader)
				{
					loader.OnLoad();
				}

				if (child.Controls.Count > 0)
				{
					LoadControlThemes(child.Controls);
				}
			}
		}


		/// <summary>
		/// Sets the arrow/text of the expand-collapse indicator, since "Expand"/"Collapse"
		/// differ in width and the arrow is deliberately a larger font than the word next to
		/// it (so a single Label can't be used for both). The containing panel's own width is
		/// fixed (see EnsureIndicatorWidth()) so it doesn't shift as the label text changes.
		/// </summary>
		private void UpdateIndicator()
		{
			var full = expanded ? Resx.BoxTypeCard_Collapse : Resx.BoxTypeCard_Expand;
			indicatorArrow.Text = full.Substring(0, 1);
			indicatorText.Text = full.Substring(1).TrimStart();

			indicatorArrow.Location = new Point(0, (HeaderHeight - indicatorArrow.Height) / 2);
			indicatorText.Location = new Point(indicatorArrow.Right + 4, (HeaderHeight - indicatorText.Height) / 2);
		}


		private static MoreLabel MakeCaption(string text, DockStyle dock, int leftPad = 0)
		{
			// Dock=Top pins the control itself to x=0, so a caption that needs to line up
			// with a value control offset further right (e.g. past where a color swatch
			// would otherwise sit) has to get there via left Padding on the AutoSize text,
			// not via Location.
			return new MoreLabel
			{
				AutoSize = true,
				Dock = dock,
				Padding = new Padding(leftPad, 0, 0, 4),
				ForeColor = Color.FromArgb(0x88, 0x88, 0x88),
				Font = smallTextFont,
				Text = text
			};
		}


		private static Panel MakeSpacer(int height)
		{
			return new Panel { Dock = DockStyle.Top, Height = height };
		}


		private (MorePictureBox, MoreTextBox) AddColorField(
			TableLayoutPanel table, int col, int row, string caption, string hex, Action<string> apply)
		{
			var group = new Panel
			{
				Dock = DockStyle.Fill,
				Margin = col == 0 ? new Padding(0, 0, 10, 0) : new Padding(10, 0, 0, 0)
			};

			var fieldRow = new Panel { Dock = DockStyle.Top, Height = 24 };

			var colorSwatch = new MorePictureBox
			{
				Location = new Point(0, 1),
				Size = new Size(22, 22),
				BorderStyle = BorderStyle.FixedSingle,
				Cursor = Cursors.Hand,
				BackColor = SafeColor(hex)
			};

			// fixed width sized for "#RRGGBB" (7 chars) - no need to stretch with the card
			var hexBox = new MoreTextBox
			{
				Location = new Point(28, 0),
				Size = new Size(84, 22),
				Font = smallFieldFont,
				Text = hex
			};

			colorSwatch.Click += (s, e) =>
			{
				var location = PointToScreen(colorSwatch.Location);
				using var dialog = new MoreColorDialog(caption,
					location.X + colorSwatch.Width / 2, location.Y - 50)
				{
					Color = colorSwatch.BackColor
				};

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					colorSwatch.BackColor = dialog.Color;
					hexBox.Text = dialog.Color.ToRGBHtml();
				}
			};

			hexBox.TextChanged += (s, e) =>
			{
				MarkValidity(hexBox, HexColorPattern.IsMatch(hexBox.Text));
				var color = SafeColor(hexBox.Text);
				colorSwatch.BackColor = color;
				apply(hexBox.Text);
				RaiseChanged();
			};

			// auto-prepend "#" if the user typed/pasted a bare hex value and tabbed/clicked away
			hexBox.Leave += (s, e) =>
			{
				if (hexBox.Text.Length > 0 && !hexBox.Text.StartsWith("#", StringComparison.Ordinal))
				{
					hexBox.Text = "#" + hexBox.Text;
				}
			};

			MarkValidity(hexBox, HexColorPattern.IsMatch(hexBox.Text));

			fieldRow.Controls.Add(colorSwatch);
			fieldRow.Controls.Add(hexBox);

			// Dock=Top stacks such that the LAST control added ends up visually topmost
			group.Controls.Add(fieldRow);
			group.Controls.Add(MakeCaption(caption, DockStyle.Top));

			table.Controls.Add(group, col, row);

			return (colorSwatch, hexBox);
		}


		/// <summary>
		/// Simple visual cue for basic pattern validation on the hex color and codepoint
		/// fields: invalid input turns the text red rather than blocking typing or rejecting
		/// the value outright (the underlying Box field/swatch already fall back gracefully
		/// via SafeColor/Swatch.OnPaint's try/catch for genuinely malformed values).
		/// </summary>
		private void MarkValidity(Control control, bool valid)
		{
			control.ForeColor = valid ? manager.GetColor("WindowText") : Color.Red;
		}


		private static Color SafeColor(string hex)
		{
			try
			{
				return ColorTranslator.FromHtml(hex);
			}
			catch
			{
				return Color.Gray;
			}
		}


		/// <summary>
		/// A neutral gray for chrome/meta text (not user-configured content) that stays
		/// readable against the given shading fill, brightness-thresholded the same way
		/// as Page.GetBestTextColor()/ColorExtensions.IsDark() elsewhere in this codebase.
		/// </summary>
		private static Color GetChromeTextColor(Color background)
		{
			return background.GetBrightness() < 0.5
				? Color.FromArgb(0xcc, 0xcc, 0xcc)
				: Color.FromArgb(0x88, 0x88, 0x88);
		}


		private void ToggleExpanded(object sender, EventArgs e)
		{
			Expanded = !Expanded;
		}


		private void ChooseSymbol(object sender, EventArgs e)
		{
			using var dialog = new EmojiDialog();
			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			foreach (var emoji in dialog.GetEmojis())
			{
				symbolCodeBox.Text = char.ConvertToUtf32(emoji.Glyph, 0)
					.ToString("X", CultureInfo.InvariantCulture);
				break;
			}
		}


		private void ResetToDefault(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var defaults = new BoxTypesProvider().GetDefault(Box.Id);
			if (defaults is null)
			{
				return;
			}

			Box.Title = defaults.Title;
			Box.Shading = defaults.Shading;
			Box.Symbol = defaults.Symbol;
			Box.SymbolColor = defaults.SymbolColor;
			Box.SymbolSize = defaults.SymbolSize;
			Box.TitleColor = defaults.TitleColor;
			Box.TextColor = defaults.TextColor;

			titleLabel.Text = Box.Title;
			titleBox.Text = Box.Title;
			shadingBox.Text = Box.Shading;
			shadingSwatch.BackColor = SafeColor(Box.Shading);
			titleColorBox.Text = Box.TitleColor;
			titleColorSwatch.BackColor = SafeColor(Box.TitleColor);
			textColorBox.Text = Box.TextColor;
			textColorSwatch.BackColor = SafeColor(Box.TextColor);
			symbolColorBox.Text = Box.SymbolColor;
			symbolColorSwatch.BackColor = SafeColor(Box.SymbolColor);
			symbolSizeBox.Value = Box.SymbolSize;
			symbolCodeBox.Text = Box.Symbol;

			swatch.Invalidate();
			RaiseChanged();
		}


		private void ConfirmDelete(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var result = MoreMessageBox.Show(this,
				string.Format(Resx.BoxTypeCard_ConfirmDelete, Box.Title),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				Delete?.Invoke(this, EventArgs.Empty);
			}
		}


		private void RaiseChanged()
		{
			Changed?.Invoke(this, EventArgs.Empty);
		}


		/// <summary>
		/// Small 32x32 preview of a box's shading + centered symbol, used in the collapsed
		/// header and refreshed live as the card's fields are edited.
		/// </summary>
		private sealed class Swatch : Panel
		{
			private readonly BoxType box;

			public Swatch(BoxType box)
			{
				this.box = box;
				DoubleBuffered = true;
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				Color shading;
				try
				{
					shading = ColorTranslator.FromHtml(box.Shading);
				}
				catch
				{
					shading = Color.LightGray;
				}

				using (var brush = new SolidBrush(shading))
				{
					e.Graphics.FillRectangle(brush, ClientRectangle);
				}

				if (string.IsNullOrWhiteSpace(box.Symbol))
				{
					return;
				}

				try
				{
					var codepoint = int.Parse(box.Symbol, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					var glyph = char.ConvertFromUtf32(codepoint);
					var color = ColorTranslator.FromHtml(box.SymbolColor);
					var size = Math.Max(8f, box.SymbolSize * 0.7f);

					using var font = new Font(BoxTypesProvider.SymbolFont, size);
					using var brush = new SolidBrush(color);
					using var format = new StringFormat
					{
						Alignment = StringAlignment.Center,
						LineAlignment = StringAlignment.Center
					};

					e.Graphics.DrawString(glyph, font, brush, ClientRectangle, format);
				}
				catch
				{
					// malformed codepoint/color; leave swatch blank
				}
			}
		}
	}
}
