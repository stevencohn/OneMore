//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class RepairUrlsDialog : UI.MoreForm
	{
		private Color markColor = Color.Red;

		// the highlight is applied to page content so, as in MoreColorPicker, the swatch shows
		// how OneNote renders it on a dark page while markColor stays the applied color
		private readonly Color pageColor;
		private readonly bool darkPage;


		/// <summary>
		/// Initializes a new dialog.
		/// </summary>
		/// <param name="hasPageGroup">
		/// True if the current page is the parent or a child of a page group
		/// </param>
		/// <param name="hasSectionGroup">
		/// True if the current section is inside a section group
		/// </param>
		/// <param name="pageColor">
		/// The background color of the current page, which determines dark-mode rendering of the
		/// highlight color; empty means the page follows the Office theme
		/// </param>
		public RepairUrlsDialog(bool hasPageGroup, bool hasSectionGroup, Color pageColor = default)
		{
			InitializeComponent();

			this.pageColor = pageColor;
			darkPage = UI.MoreColorPicker.IsDarkPage(pageColor);

			pageGroupRadio.Enabled = hasPageGroup;
			sectionGroupRadio.Enabled = hasSectionGroup;

			if (NeedsLocalizing())
			{
				Text = Resx.RepairUrlsDialog_Text;

				Localize(new string[]
				{
					"groupBox",
					"pageRadio",
					"pageGroupRadio",
					"sectionRadio=phrase_TheCurrentSection",
					"sectionGroupRadio",
					"notebookRadio=phrase_AllSectionInTheCurrentNotebook",
					"notebooksRadio=phrase_AllNotebooks",
					"onenoteBox",
					"captionBox",
					"repairBox",
					"markBox",
					"colorLabel",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			ActiveControl = okButton;
		}


		/// <summary>
		/// Gets or sets the color used to highlight links that cannot be repaired.
		/// </summary>
		public Color MarkColor
		{
			get => markColor;
			set
			{
				markColor = value;
				colorSwatch.BackColor = Rendered(value);
			}
		}


		/// <summary>
		/// Gets or sets whether web links are ignored, checking only onenote: links.
		/// </summary>
		public bool OnenoteOnly
		{
			get => onenoteBox.Checked;
			set => onenoteBox.Checked = value;
		}


		/// <summary>
		/// Gets or sets whether links whose text does not match the page name are reported.
		/// </summary>
		public bool CheckCaptions
		{
			get => captionBox.Checked;
			set => captionBox.Checked = value;
		}


		/// <summary>
		/// Gets or sets whether links that can be fixed are repaired.
		/// </summary>
		public bool Repair
		{
			get => repairBox.Checked;
			set => repairBox.Checked = value;
		}


		/// <summary>
		/// Gets or sets whether links that cannot be fixed are highlighted.
		/// </summary>
		public bool Mark
		{
			get => markBox.Checked;
			set => markBox.Checked = value;
		}


		private void MarkChanged(object sender, EventArgs e)
		{
			var enabled = markBox.Checked;
			colorSwatch.Enabled = enabled;

			// A disabled Label ignores its ForeColor and paints its own disabled text, which is
			// black in dark mode. So the label stays enabled and is colored to look disabled.
			// MoreLabel applies its themed colors only when the dialog loads, so recolor it here.
			var manager = UI.ThemeManager.Instance;
			colorLabel.ForeColor = enabled
				? manager.GetColor("ControlText", colorLabel.ThemedFore)
				: manager.GetColor("GrayText");
		}


		public RepairUrlsScope Scope
		{
			get
			{
				if (notebooksRadio.Checked) return RepairUrlsScope.Notebooks;
				if (notebookRadio.Checked) return RepairUrlsScope.Notebook;
				if (sectionGroupRadio.Checked) return RepairUrlsScope.SectionGroup;
				if (sectionRadio.Checked) return RepairUrlsScope.Section;
				if (pageGroupRadio.Checked) return RepairUrlsScope.PageGroup;
				return RepairUrlsScope.Page;
			}
		}


		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			// theming may have reset the swatch and the label
			colorSwatch.BackColor = Rendered(markColor);
			MarkChanged(this, EventArgs.Empty);
		}


		private Color Rendered(Color color)
		{
			return darkPage ? UI.ColorPalette.ToDark(color) : color;
		}


		private void ChooseColor(object sender, EventArgs e)
		{
			using var picker = new UI.MoreColorPicker(pageColor,
				Left + colorSwatch.Left, Top + colorSwatch.Bottom, "Select Highlight Color")
			{
				Color = markColor,
				AutomaticVisible = false
			};

			// use the elevator to force the picker to top-most first time used
			using var elevator = new UI.WindowElevator(picker);
			if (elevator.ShowDialog(this) == DialogResult.OK)
			{
				MarkColor = picker.Color;
			}
		}
	}
}
