//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Drawing;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// One row of the Colors tab's element list: a color swatch, area label, a two-way-bound
	/// hex text field, and Edit.../Reset links. Reports changes via ColorChanged/ResetClicked
	/// so the owning dialog can apply them to the selected theme and repaint its preview.
	/// </summary>
	internal sealed class ThemeColorRow : Panel
	{
		private static readonly Regex HexPattern = new(@"^#?[0-9A-Fa-f]{6}$", RegexOptions.Compiled);
		private static readonly float xScaling;
		private static readonly float yScaling;

		private readonly ColorSwatchControl swatch;
		private readonly MoreLabel label;
		private readonly MoreTextBox hexBox;
		private readonly MoreLinkLabel editLink;
		private readonly MoreLinkLabel resetLink;

		private Color color = Color.Empty;
		private bool suppressEvents;
		private bool showingPlaceholder;


		static ThemeColorRow()
		{
			(xScaling, yScaling) = Scaling.GetScalingFactors();
		}


		public ThemeColorRow(string areaName)
		{
			// dynamically-added children (label/hexBox/links) read Parent.BackColor when
			// MoreForm's one-time ILoadControl walk reaches them, so this must be themed
			// explicitly rather than left at Panel's plain default
			BackColor = ThemeManager.Instance.GetColor("Window");

			Margin = new Padding(0);
			Height = (int)(30 * yScaling);

			var swatchSize = (int)(18 * yScaling);

			swatch = new ColorSwatchControl
			{
				Location = new Point(0, (Height - swatchSize) / 2),
				Size = new Size(swatchSize, swatchSize)
			};
			swatch.Click += (s, e) => OpenColorDialog();
			Controls.Add(swatch);

			label = new MoreLabel
			{
				AutoSize = false,
				TextAlign = ContentAlignment.MiddleLeft,
				Location = new Point(swatch.Right + 8, 0),
				Size = new Size((int)(190 * xScaling), Height),
				Text = areaName
			};
			Controls.Add(label);

			var hexWidth = (int)(78 * xScaling);
			var hexHeight = (int)(24 * yScaling);
			hexBox = new MoreTextBox
			{
				TextAlign = HorizontalAlignment.Center,
				Font = new Font("Consolas", 8.5f),
				Location = new Point(label.Right + 8, (Height - hexHeight) / 2),
				Size = new Size(hexWidth, hexHeight)
			};
			hexBox.TextChanged += HexBoxTextChanged;
			hexBox.GotFocus += HexBoxGotFocus;
			hexBox.Leave += HexBoxLeave;
			Controls.Add(hexBox);

			editLink = new MoreLinkLabel
			{
				AutoSize = true,
				Text = Resx.word_Edit,
				Location = new Point(hexBox.Right + 10, (Height - 20) / 2)
			};
			editLink.LinkClicked += (s, e) => OpenColorDialog();
			Controls.Add(editLink);

			resetLink = new MoreLinkLabel
			{
				AutoSize = true,
				Text = Resx.word_Reset,
				Location = new Point(editLink.Right + 10, (Height - 20) / 2)
			};
			resetLink.LinkClicked += (s, e) => PerformReset();
			Controls.Add(resetLink);

			// self-determined from the (already DPI-scaled) children just laid out above,
			// rather than a caller-supplied constant - a hardcoded outer width here would
			// drift out of sync with these children's own xScaling/yScaling math at any DPI
			// other than the one it was picked for, clipping editLink/resetLink at high DPI
			Width = resetLink.Right + (int)(10 * xScaling);

			SetColor(Color.Empty);
		}


		/// <summary>
		/// Raised whenever this row's color changes as a result of user interaction
		/// (hex typed, Edit... dialog, or Reset).
		/// </summary>
		public event EventHandler<Color> ColorChanged;

		/// <summary>
		/// Raised specifically when the user clears this row back to its unset default.
		/// </summary>
		public event EventHandler ResetClicked;


		public Color Color => color;


		/// <summary>
		/// Pushes a color into this row (e.g. when a different theme is selected)
		/// without raising ColorChanged/ResetClicked.
		/// </summary>
		public void SetColor(Color value)
		{
			suppressEvents = true;

			color = value;
			swatch.Color = value;

			if (value.IsEmpty)
			{
				showingPlaceholder = true;
				hexBox.ForeColor = ThemeManager.Instance.GetColor("GrayText");
				hexBox.Text = Resx.word_Default;
			}
			else
			{
				showingPlaceholder = false;
				hexBox.ForeColor = ThemeManager.Instance.GetColor("WindowText");
				hexBox.Text = value.ToRGBHtml();
			}

			resetLink.Enabled = !value.IsEmpty;

			suppressEvents = false;
		}


		private void OpenColorDialog()
		{
			var location = PointToScreen(Point.Empty);

			using var dialog = new MoreColorDialog(label.Text, location.X + 75, location.Y + 60)
			{
				Color = color.IsEmpty ? Color.White : color
			};

			if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
			{
				return;
			}

			SetColor(dialog.Color);
			ColorChanged?.Invoke(this, dialog.Color);
		}


		private void PerformReset()
		{
			if (color.IsEmpty)
			{
				return;
			}

			SetColor(Color.Empty);
			ResetClicked?.Invoke(this, EventArgs.Empty);
		}


		private void HexBoxGotFocus(object sender, EventArgs e)
		{
			if (showingPlaceholder)
			{
				suppressEvents = true;
				showingPlaceholder = false;
				hexBox.ForeColor = ThemeManager.Instance.GetColor("WindowText");
				hexBox.Text = string.Empty;
				suppressEvents = false;
			}
		}


		private void HexBoxTextChanged(object sender, EventArgs e)
		{
			if (suppressEvents || showingPlaceholder)
			{
				return;
			}

			var text = hexBox.Text.Trim();
			if (HexPattern.IsMatch(text))
			{
				var html = text.StartsWith("#") ? text : $"#{text}";
				var parsed = ColorTranslator.FromHtml(html);

				color = parsed;
				swatch.Color = parsed;
				resetLink.Enabled = true;

				ColorChanged?.Invoke(this, parsed);
			}
		}


		private void HexBoxLeave(object sender, EventArgs e)
		{
			if (suppressEvents)
			{
				return;
			}

			var text = hexBox.Text.Trim();

			if (text.Length == 0)
			{
				var wasSet = !color.IsEmpty;
				SetColor(Color.Empty);

				if (wasSet)
				{
					ResetClicked?.Invoke(this, EventArgs.Empty);
				}

				return;
			}

			if (!HexPattern.IsMatch(text))
			{
				// invalid/garbage input; revert to the last known-good value
				SetColor(color);
			}
		}
	}
}
