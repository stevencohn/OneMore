//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class EditTableThemesDialog : UI.MoreForm
	{
		private const int PreviewMargin = 15;

		/// <summary>
		/// TableTheme overrides Equals/GetHashCode with value-based semantics (and its hash
		/// is derived from Name), so it cannot safely be used as a Dictionary key directly -
		/// renaming a theme after it's been snapshotted would change its hash code and corrupt
		/// lookups. This comparer keys snapshots by object identity instead.
		/// </summary>
		private sealed class ReferenceComparer : IEqualityComparer<TableTheme>
		{
			public bool Equals(TableTheme x, TableTheme y) => ReferenceEquals(x, y);

			public int GetHashCode(TableTheme obj) =>
				System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
		}


		private readonly TableThemePainter painter;
		private readonly List<ThemeColorRow> colorRows = new();
		private readonly List<ThemeFontRoleRow> fontRoleRows = new();
		private readonly Dictionary<TableTheme, TableTheme> snapshots = new(new ReferenceComparer());

		private List<TableTheme> systemThemes;
		private List<TableTheme> userThemes;

		// the subset of userThemes that are actually persisted (as of dialog-open or the
		// last successful Save) - excludes New/Duplicate themes still pending this session
		// and the auto-inserted blank placeholder, so the Duplicate gallery only ever offers
		// real, saved themes as sources
		private List<TableTheme> savedUserThemes;

		private TableTheme.ColorFont colorfont;
		private Font sampleFont;
		private int selectedFontRoleIndex = -1;
		private bool reorganizing;
		private bool listChanged;


		public EditTableThemesDialog()
		{
			InitializeComponent();

			BuildColorRows();
			BuildFontRoleRows();

			if (NeedsLocalizing())
			{
				Text = Resx.EditTableThemesDialog_Text;

				Localize(new string[]
				{
					"themeLabel=word_Theme",
					"newButton=word_New",
					"duplicateButton=word_Duplicate",
					"renameButton=word_Rename",
					"deleteButton=word_Delete",
					"colorsTab",
					"fontsTab",
					"previewGroup=word_Preview",
					"resetAllLink=EditTableThemesDialog_resetButton.Text",
					"saveButton=word_Save",
					"cancelButton=word_Cancel",
					"fontsGroup=word_Font",
					"sampleLabel=EditTableThemesDialog_sample",
					"resetToDefaultLink=EditTableThemesDialog_resetToDefault",
					"applyFontButton=word_Apply"
				});
			}

			previewBox.Image = new Bitmap(previewBox.Width, previewBox.Height);

			var bounds = new Rectangle(
				PreviewMargin, PreviewMargin,
				previewBox.Width - (PreviewMargin * 2),
				previewBox.Height - (PreviewMargin * 2));

			painter = new TableThemePainter(previewBox.Image, bounds, SystemColors.Window);

			sampleBorderPanel.Paint += PaintSampleBorder;
		}


		public EditTableThemesDialog(List<TableTheme> systemThemes, List<TableTheme> userThemes)
			: this()
		{
			this.systemThemes = systemThemes;
			this.userThemes = userThemes;

			// capture before the blank-placeholder insertion below, so that placeholder
			// never counts as a saved theme
			savedUserThemes = new List<TableTheme>(userThemes);

			if (userThemes.Count == 0)
			{
				userThemes.Add(new TableTheme { Name = Resx.phrase_NewStyle });
			}

			familyBox.LoadFontFamilies();

			PopulateCombo();
		}


		public bool Modified { get; private set; }


		protected override void OnLoad(EventArgs e)
		{
			colorsListPanel.BackColor = manager.GetColor("Window");
			fontsListPanel.BackColor = manager.GetColor("Window");
			sampleBorderPanel.BackColor = manager.GetColor("Window");

			base.OnLoad(e);

			// MoreForm's one-time ILoadControl walk (just run, inside base.OnLoad) reset
			// every hex box's ForeColor to the plain "WindowText" default (MoreTextBox.OnLoad
			// always does this, unconditionally), clobbering the gray-for-placeholder /
			// black-for-real-value distinction ThemeColorRow.SetColor already applied during
			// construction. Reapply it now that the walk is done - SetColor is idempotent
			// (suppressEvents guards it) so this is a pure visual resync, no side effects.
			foreach (var row in colorRows)
			{
				row.SetColor(row.Color);
			}

			FixFontToolstripLayout();
		}


		/// <summary>
		/// Fixes two knock-on effects of the same root cause, both only visible away from
		/// 96 DPI: oversized Bold/Italic/Underline/color buttons, and no gap between the
		/// font toolbar and the sample text panel below it.
		///
		/// MoreToolStrip.OnLoad() (part of the ILoadControl walk that just ran, inside
		/// base.OnLoad above) sizes its own Height and ImageScalingSize from
		/// UI.Scaling.GetScalingFactors() - a raw physical/logical DPI ratio - while these
		/// Designer-declared items' own baseline Size(34,22)/(39,22), and fontToolstrip's own
		/// Designer-declared Y position, instead get scaled by the form's independent,
		/// Font-metric-based AutoScaleMode.Font pass. The two scaling sources agree closely
		/// enough at 96 DPI to look fine, but can diverge at other DPIs: since each item's
		/// AutoSize (WinForms' default for ToolStripItem) re-expands to fit whichever scaled
		/// image it lands with, that shows up as oversized buttons; and since the toolstrip's
		/// own Height can grow taller than the gap the form's own scaling pass reserved for
		/// it before sampleBorderPanel's fixed Designer-time Y, that shows up as no visible
		/// gap (or overlap) above the sample panel.
		///
		/// Run from OnLoad, after base.OnLoad(e) - i.e. after both the form's own
		/// autoscale pass and MoreToolStrip's own OnLoad have already happened - so this is
		/// unambiguously the last word over these values regardless of exactly when either
		/// of those actually run relative to the constructor.
		/// </summary>
		private void FixFontToolstripLayout()
		{
			var (scaleX, scaleY) = Scaling.GetScalingFactors();

			boldButton.AutoSize = false;
			italicButton.AutoSize = false;
			underlineButton.AutoSize = false;
			colorButton.AutoSize = false;

			var itemSize = new Size((int)(34 * scaleX), (int)(22 * scaleY));
			boldButton.Size = itemSize;
			italicButton.Size = itemSize;
			underlineButton.Size = itemSize;
			colorButton.Size = new Size((int)(39 * scaleX), (int)(22 * scaleY));

			// MoreToolStrip.OnLoad() already computed fontToolstrip's own Width (as part of
			// the same walk, earlier in base.OnLoad) from these items' sizes as they stood
			// BEFORE the resize just above - recompute it the same way (Items' widths + 16)
			// now that they've changed, or the last button clips past the stale, now-too-
			// narrow strip
			fontToolstrip.Width = fontToolstrip.Items
				.OfType<ToolStripItem>()
				.Sum(i => i.Width) + 16;

			// reposition from the toolstrip's real, now-settled Bottom edge rather than
			// trusting the Designer's fixed Y coordinate to still clear it
			sampleBorderPanel.Top = fontToolstrip.Bottom + (int)(12 * scaleY);
		}


		// Row construction - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void BuildColorRows()
		{
			var names = Regex.Split(Resx.EditTableThemesDialog_elements, @"\r\n|\r|\n");
			var y = 0;

			for (var i = 0; i < names.Length; i++)
			{
				var index = i;

				// ThemeColorRow sizes its own Width from its (DPI-scaled) children - do not
				// override it with a hardcoded constant here, see its constructor comment
				var row = new ThemeColorRow(names[i])
				{
					Location = new Point(0, y)
				};

				row.ColorChanged += (s, color) => ColorRowColorChanged(index, color);
				row.ResetClicked += (s, e) => ColorRowColorChanged(index, row.Color);

				colorsListPanel.Controls.Add(row);
				colorRows.Add(row);

				y += row.Height;
			}

			// colorsListPanel's own Designer-declared width is scaled by the form's
			// AutoScaleMode.Font pass, which runs once and never revisits controls added
			// afterward (these rows). Rather than trust that pass and this row's own
			// Scaling.GetScalingFactors()-based math to agree pixel-for-pixel at every DPI,
			// size the panel from the rows' actual (already-correct) width directly.
			if (colorRows.Count > 0)
			{
				var contentWidth = colorRows.Max(r => r.Width);
				colorsListPanel.Width = contentWidth + SystemInformation.VerticalScrollBarWidth + 12;
			}
		}


		private void BuildFontRoleRows()
		{
			var names = Regex.Split(Resx.EditTableThemesDialog_fontElements, @"\r\n|\r|\n");
			var y = 0;

			// ThemeFontRoleRow's children just fill whatever width they're given, so match
			// fontsListPanel's own actual current width instead of a separate hardcoded
			// constant that could drift out of sync with it at a different DPI
			var rowWidth = fontsListPanel.ClientSize.Width;

			for (var i = 0; i < names.Length; i++)
			{
				var index = i;
				var row = new ThemeFontRoleRow(names[i])
				{
					Location = new Point(0, y),
					Width = rowWidth
				};

				row.Selected += (s, e) => FontRoleSelected(index);

				fontsListPanel.Controls.Add(row);
				fontRoleRows.Add(row);

				y += row.Height;
			}
		}


		// Theme selection - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Only user-defined themes ever appear in the combo - built-in themes are read-only
		// and are reachable only as sources from the Duplicate menu (see ShowDuplicateMenu).

		private void PopulateCombo(TableTheme selectTheme = null)
		{
			reorganizing = true;

			combo.Items.Clear();

			foreach (var theme in userThemes.OrderBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase))
			{
				combo.Items.Add(theme);
			}

			var target = 0;
			if (selectTheme is not null)
			{
				for (var i = 0; i < combo.Items.Count; i++)
				{
					if (ReferenceEquals(combo.Items[i], selectTheme))
					{
						target = i;
						break;
					}
				}
			}

			if (combo.Items.Count > 0)
			{
				combo.SelectedIndex = target;
			}

			reorganizing = false;

			ChooseTheme(this, EventArgs.Empty);
		}


		private void ChooseTheme(object sender, EventArgs e)
		{
			if (reorganizing || combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			if (!snapshots.ContainsKey(theme))
			{
				var snap = new TableTheme();
				theme.CopyTo(snap);
				snapshots[theme] = snap;
			}

			LoadColorsIntoRows(theme);
			RepaintPreview(theme);
			LoadFontSummaries(theme);

			FontRoleSelected(0);

			UpdateButtonStates();
		}


		// Colors tab - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void LoadColorsIntoRows(TableTheme theme)
		{
			for (var i = 0; i < colorRows.Count; i++)
			{
				colorRows[i].SetColor(GetColorProperty(theme, i));
			}
		}


		private void RepaintPreview(TableTheme theme)
		{
			painter.Paint(theme);
			previewBox.Invalidate();
		}


		private void ColorRowColorChanged(int index, Color color)
		{
			if (combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			SetColorProperty(theme, index, color);
			RepaintPreview(theme);
			UpdateButtonStates();
		}


		private void ResetAllColors(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			for (var i = 0; i < colorRows.Count; i++)
			{
				colorRows[i].SetColor(Color.Empty);
				SetColorProperty(theme, i, Color.Empty);
			}

			RepaintPreview(theme);
			UpdateButtonStates();
		}


		private static Color GetColorProperty(TableTheme theme, int index) => index switch
		{
			0 => theme.WholeTable,
			1 => theme.FirstColumnStripe,
			2 => theme.SecondColumnStripe,
			3 => theme.FirstRowStripe,
			4 => theme.SecondRowStripe,
			5 => theme.FirstColumn,
			6 => theme.LastColumn,
			7 => theme.HeaderRow,
			8 => theme.TotalRow,
			9 => theme.HeaderFirstCell,
			10 => theme.HeaderLastCell,
			11 => theme.TotalFirstCell,
			12 => theme.TotalLastCell,
			_ => Color.Empty
		};


		private static void SetColorProperty(TableTheme theme, int index, Color color)
		{
			switch (index)
			{
				case 0: theme.WholeTable = color; break;
				case 1: theme.FirstColumnStripe = color; break;
				case 2: theme.SecondColumnStripe = color; break;
				case 3: theme.FirstRowStripe = color; break;
				case 4: theme.SecondRowStripe = color; break;
				case 5: theme.FirstColumn = color; break;
				case 6: theme.LastColumn = color; break;
				case 7: theme.HeaderRow = color; break;
				case 8: theme.TotalRow = color; break;
				case 9: theme.HeaderFirstCell = color; break;
				case 10: theme.HeaderLastCell = color; break;
				case 11: theme.TotalFirstCell = color; break;
				case 12: theme.TotalLastCell = color; break;
			}
		}


		// Fonts tab - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void LoadFontSummaries(TableTheme theme)
		{
			for (var i = 0; i < fontRoleRows.Count; i++)
			{
				var font = GetFontRole(theme, i);
				fontRoleRows[i].Summary = font?.ToString() ?? Resx.word_Default;
			}
		}


		private void FontRoleSelected(int index)
		{
			if (combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			for (var i = 0; i < fontRoleRows.Count; i++)
			{
				fontRoleRows[i].IsSelected = i == index;
			}

			selectedFontRoleIndex = index;
			LoadFontRoleIntoPanel(theme, index);
		}


		private void LoadFontRoleIntoPanel(TableTheme theme, int index)
		{
			colorfont = new TableTheme.ColorFont(GetFontRole(theme, index));

			reorganizing = true;

			if (familyBox.Items.Count > 0)
			{
				var name = colorfont.Font?.FontFamily.Name ?? StyleBase.DefaultFontFamily;
				var famIndex = familyBox.Items.IndexOf(name);
				familyBox.SelectedIndex = famIndex < 0 ? 0 : famIndex;
			}

			if (sizeBox.Items.Count > 0)
			{
				var size = colorfont.Font?.Size ?? StyleBase.DefaultFontSize;
				var sizeIndex = sizeBox.Items.IndexOf(size.ToString("0.#", AddIn.Locale));
				sizeBox.SelectedIndex = sizeIndex < 0 ? 0 : sizeIndex;
			}

			boldButton.Checked = colorfont.Font?.Bold ?? false;
			italicButton.Checked = colorfont.Font?.Italic ?? false;
			underlineButton.Checked = colorfont.Font?.Underline ?? false;

			reorganizing = false;

			UpdateSample();
		}


		private void UpdateSample()
		{
			if (colorfont is null)
			{
				return;
			}

			var font = MakeFont();
			sampleLabel.Font = font;

			var previous = sampleFont;
			sampleFont = font;
			previous?.Dispose();

			sampleLabel.ForeColor = colorfont.Foreground.IsEmpty
				? ThemeManager.Instance.GetColor("ControlText")
				: colorfont.Foreground;
		}


		private void ChangeFontFont(object sender, EventArgs e)
		{
			if (reorganizing || colorfont is null)
			{
				return;
			}

			var previous = colorfont.Font;
			colorfont.Font = MakeFont();
			previous?.Dispose();

			UpdateSample();
		}


		private void ChangeFontColor(object sender, EventArgs e)
		{
			var location = PointToScreen(fontToolstrip.Location);

			using var dialog = new MoreColorDialog("Text Color",
				location.X + colorButton.Bounds.Location.X,
				location.Y + colorButton.Bounds.Height + 4)
			{
				Color = colorfont.Foreground
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				colorfont.Foreground = dialog.Color;
				UpdateSample();
			}
		}


		private void SetFontColorDefault(object sender, EventArgs e)
		{
			colorfont.Foreground = Color.Empty;
			UpdateSample();
		}


		private Font MakeFont()
		{
			var text = sizeBox.Text.Trim();
			if (!float.TryParse(text, NumberStyles.Integer | NumberStyles.AllowDecimalPoint,
				AddIn.Locale, out var size))
			{
				size = (float)StyleBase.DefaultFontSize;
			}

			var style = FontStyle.Regular;
			if (boldButton.Checked) style |= FontStyle.Bold;
			if (italicButton.Checked) style |= FontStyle.Italic;
			if (underlineButton.Checked) style |= FontStyle.Underline;

			return new Font(familyBox.Text, size, style);
		}


		private void ApplyFont(object sender, EventArgs e)
		{
			if (combo.SelectedItem is not TableTheme theme || selectedFontRoleIndex < 0)
			{
				return;
			}

			SetFontRole(theme, selectedFontRoleIndex, colorfont);

			fontRoleRows[selectedFontRoleIndex].Summary = colorfont.ToString();

			// re-clone so continued edits act on a fresh pending buffer, matching the
			// "switching roles without Apply discards the edit" model even right after Apply
			colorfont = new TableTheme.ColorFont(colorfont);

			UpdateButtonStates();
		}


		private void ResetFontRoleToDefault(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (combo.SelectedItem is not TableTheme theme || selectedFontRoleIndex < 0)
			{
				return;
			}

			SetFontRole(theme, selectedFontRoleIndex, null);

			fontRoleRows[selectedFontRoleIndex].Summary = Resx.word_Default;

			LoadFontRoleIntoPanel(theme, selectedFontRoleIndex);

			UpdateButtonStates();
		}


		private static TableTheme.ColorFont GetFontRole(TableTheme theme, int index) => index switch
		{
			0 => theme.DefaultFont,
			1 => theme.HeaderFont,
			2 => theme.TotalFont,
			3 => theme.FirstColumnFont,
			4 => theme.LastColumnFont,
			_ => null
		};


		private static void SetFontRole(TableTheme theme, int index, TableTheme.ColorFont font)
		{
			switch (index)
			{
				case 0: theme.DefaultFont?.Dispose(); theme.DefaultFont = font; break;
				case 1: theme.HeaderFont?.Dispose(); theme.HeaderFont = font; break;
				case 2: theme.TotalFont?.Dispose(); theme.TotalFont = font; break;
				case 3: theme.FirstColumnFont?.Dispose(); theme.FirstColumnFont = font; break;
				case 4: theme.LastColumnFont?.Dispose(); theme.LastColumnFont = font; break;
			}
		}


		private void PaintSampleBorder(object sender, PaintEventArgs e)
		{
			using var pen = new Pen(manager.GetColor("ButtonBorder")) { DashStyle = DashStyle.Dash };
			e.Graphics.DrawRectangle(pen, 0, 0, sampleBorderPanel.Width - 1, sampleBorderPanel.Height - 1);
		}


		// Dirty tracking - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private bool IsDirty(TableTheme theme)
		{
			return snapshots.TryGetValue(theme, out var snap) && !theme.Equals(snap);
		}


		private bool AnyDirty()
		{
			return listChanged || userThemes.Any(IsDirty);
		}


		private void UpdateButtonStates()
		{
			var hasSelection = combo.SelectedItem is TableTheme;

			renameButton.Enabled = hasSelection;
			deleteButton.Enabled = hasSelection;

			saveButton.Enabled = AnyDirty();
		}


		private static string GenerateUniqueName(string baseName, List<string> existingNames)
		{
			if (!existingNames.Contains(baseName))
			{
				return baseName;
			}

			var i = 2;
			string candidate;
			do
			{
				candidate = $"{baseName} ({i})";
				i++;
			}
			while (existingNames.Contains(candidate));

			return candidate;
		}


		// Theme management - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void CreateNewTheme(object sender, EventArgs e)
		{
			var name = GenerateUniqueName(
				Resx.phrase_NewStyle, userThemes.Select(t => t.Name).ToList());

			var theme = new TableTheme { Name = name };

			userThemes.Add(theme);
			listChanged = true;

			PopulateCombo(theme);
		}


		/// <summary>
		/// Shows a gallery popup of theme thumbnails (built-in themes first, then
		/// user-defined ones) to duplicate from, replicating the ribbon's own Table Theme
		/// gallery. This popup is a plain owned modeless window (Show(this)), not a true
		/// Win32-modal child - EditTableThemesDialog is itself already shown modally
		/// (ShowDialog) over OneNote's window, so Windows keeps it above OneNote
		/// automatically; owning the popup by this dialog is enough to keep the popup above
		/// the dialog in turn, without needing MoreForm.RunModeless's cross-process
		/// (dllhost.exe vs ONENOTE.EXE) foreground-window machinery that RemoveDuplicates'
		/// SimilarityChip/SimilarityPopup need only because those host dialogs are modeless.
		/// </summary>
		private void ShowDuplicateGallery(object sender, EventArgs e)
		{
			var popup = new ThemeGalleryPopup(systemThemes, savedUserThemes);
			popup.ThemeSelected += (s, theme) => DuplicateFrom(theme);
			popup.FormClosed += (s, e2) => popup.Dispose();

			popup.StartPosition = FormStartPosition.Manual;
			popup.ManualLocation = true;
			popup.Location = GetGalleryLocation(popup);

			popup.Show(this);
		}


		private Point GetGalleryLocation(ThemeGalleryPopup popup)
		{
			var anchor = duplicateButton.PointToScreen(new Point(0, duplicateButton.Height));
			var size = popup.PreferredSize;

			var working = Screen.FromControl(duplicateButton).WorkingArea;

			var x = Math.Min(anchor.X, working.Right - size.Width);
			x = Math.Max(x, working.Left);

			var y = Math.Min(anchor.Y, working.Bottom - size.Height);
			y = Math.Max(y, working.Top);

			return new Point(x, y);
		}


		/// <summary>
		/// Clones source (built-in or user-defined) into a new, immediately-selected
		/// user-defined theme with an auto-generated name - no name prompt, matching
		/// BoxTypesPanel.DuplicateBox; the user can Rename it afterward if they want.
		/// </summary>
		private void DuplicateFrom(TableTheme source)
		{
			var copy = new TableTheme();
			source.CopyTo(copy);

			copy.Name = GenerateUniqueName(
				string.Format(Resx.EditTableThemesDialog_copyTitle, source.Name),
				userThemes.Select(t => t.Name).ToList());

			userThemes.Add(copy);
			listChanged = true;

			PopulateCombo(copy);
		}


		private void RenameTheme(object sender, EventArgs e)
		{
			if (combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			var names = userThemes.Select(t => t.Name).ToList();

			using var dialog = new RenameDialog(names, theme.Name) { Rename = true };
			if (dialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}

			theme.Name = dialog.Value;
			listChanged = true;

			PopulateCombo(theme);
		}


		private void DeleteTheme(object sender, EventArgs e)
		{
			if (combo.SelectedItem is not TableTheme theme)
			{
				return;
			}

			if (MoreMessageBox.Show(Owner, Resx.EditTableThemesDialog_deleteStyle,
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}

			userThemes.Remove(theme);
			savedUserThemes.Remove(theme);
			snapshots.Remove(theme);
			listChanged = true;

			if (userThemes.Count == 0)
			{
				// keep the combo from ever going fully empty
				userThemes.Add(new TableTheme { Name = Resx.phrase_NewStyle });
			}

			PopulateCombo();
		}


		private void SaveAll(object sender, EventArgs e)
		{
			// a "New Style" theme nobody has touched (still the default name, still every
			// color/font unset) is just the "always have something to show" placeholder,
			// not a real theme the user asked to keep - never let it leak into the saved
			// file, even though it stays visible in the combo for the rest of this session
			// in case the user wants to go back and actually customize it
			var persisted = userThemes.Where(t => !IsUntouchedNewStyle(t)).ToList();

			new TableThemeProvider().SaveUserThemes(persisted);
			Modified = true;

			listChanged = false;
			snapshots.Clear();
			savedUserThemes = persisted;

			foreach (var theme in userThemes)
			{
				var snap = new TableTheme();
				theme.CopyTo(snap);
				snapshots[theme] = snap;
			}

			DialogResult = DialogResult.OK;
			Close();
		}


		/// <summary>
		/// True if theme is still exactly the blank state CreateNewTheme produces: the
		/// default "New Style" name, never renamed, and every color/font still unset.
		/// </summary>
		private static bool IsUntouchedNewStyle(TableTheme theme)
		{
			return theme.Name == Resx.phrase_NewStyle
				&& theme.WholeTable.IsEmpty
				&& theme.FirstColumnStripe.IsEmpty
				&& theme.SecondColumnStripe.IsEmpty
				&& theme.FirstRowStripe.IsEmpty
				&& theme.SecondRowStripe.IsEmpty
				&& theme.FirstColumn.IsEmpty
				&& theme.LastColumn.IsEmpty
				&& theme.HeaderRow.IsEmpty
				&& theme.TotalRow.IsEmpty
				&& theme.HeaderFirstCell.IsEmpty
				&& theme.HeaderLastCell.IsEmpty
				&& theme.TotalFirstCell.IsEmpty
				&& theme.TotalLastCell.IsEmpty
				&& theme.DefaultFont is null
				&& theme.HeaderFont is null
				&& theme.TotalFont is null
				&& theme.FirstColumnFont is null
				&& theme.LastColumnFont is null;
		}


		private void ConfirmClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				// already saved via SaveAll
				return;
			}

			if (AnyDirty())
			{
				if (MoreMessageBox.Show(Owner, Resx.EditTableThemesDialog_discard,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}
	}
}
