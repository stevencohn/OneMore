//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using Snippets.TocGenerators;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class InsertTocDialog : MoreForm
	{
		private readonly TocParameters parameters;


		public InsertTocDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertTocCommand_TOC;

				Localize(new string[]
				{
					"pageGroup=word_Page",
					"pageRadio",
					"topBox",
					"rightAlignBox",
					"secondaryBox",
					"todoLabel",
					"locationLabel",
					"locationBox",
					"styleLabel",
					"styleBox",
					"levelsLabel",
					"sectionGroup=word_Section",
					"sectionRadio",
					"sectionTimeBox=InsertTocDialog_timeBox",
					"sectionPagePreviewBox=InsertTocDialog_previewBox",
					"notebookGroup=word_Notebook",
					"notebookRadio",
					"notebookTimeBox=InsertTocDialog_timeBox",
					"pagesBox",
					"notebookPagePreviewBox=InsertTocDialog_previewBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			PopulateTodoBox();

			todoBox.SelectedIndex = 0;
			locationBox.SelectedIndex = 0;
			styleBox.SelectedIndex = 0;
		}


		private void PopulateTodoBox()
		{
			var list = new ImageList
			{
				ImageSize = new Size(24, 24),           // the size of each icon in the strip
				ColorDepth = ColorDepth.Depth32Bit,     // preserve alpha/transparency
			};

			list.Images.AddStrip(Resx.TocTodoIconStrip);
			var lines = Resx.InsertTocDialog_todoBox_Text.Split(
				new string[] { Environment.NewLine },
				StringSplitOptions.RemoveEmptyEntries);

			// add None item
			todoBox.Items.Add(new MoreComboBox.ComboItem(lines[0]));

			for (var i = 1; i < lines.Length; i++)
			{
				// offset Image index -1 from lines Index because we already used "None" at [0]
				todoBox.Items.Add(new MoreComboBox.ComboItem(lines[i], list.Images[i - 1]));
			}
		}


		public InsertTocDialog(TocParameters parameters)
			: this()
		{
			this.parameters = parameters;

			if (parameters.Contains("section"))
			{
				sectionRadio.Checked = true;
				sectionPagePreviewBox.Checked = parameters.Contains("preview");
				sectionTimeBox.Checked = parameters.Contains("time");
			}
			else if (parameters.Contains("notebook"))
			{
				notebookRadio.Checked = true;
				pagesBox.Checked = parameters.Contains("pages");
				notebookPagePreviewBox.Checked = parameters.Contains("preview");
				notebookTimeBox.Checked = parameters.Contains("time");
			}
			else // page is default
			{
				pageRadio.Checked = true;
				topBox.Checked = parameters.Contains("links");
				rightAlignBox.Checked = parameters.Contains("align");
				secondaryBox.Checked = parameters.Contains("secondary");

				if (parameters.Contains("here")) locationBox.SelectedIndex = 2;
				else if (parameters.Contains("over")) locationBox.SelectedIndex = 1;
				else locationBox.SelectedIndex = 0;

				if (parameters.Find(p => p.StartsWith("style")) is string style &&
					int.TryParse(style.Substring(5), out var index))
				{
					styleBox.SelectedIndex = index;
				}

				levelsBox.Value = 6;
				if (parameters.Find(p => p.StartsWith("level")) is string level &&
					int.TryParse(level.Substring(5), out var value))
				{
					levelsBox.Value = value;
				}

				if (parameters.Find(p => p.StartsWith("todo")) is string todo &&
					int.TryParse(todo.Substring(4), out var todoIndex) &&
					todoIndex > 0)
				{
					todoBox.SelectedIndex = todoIndex + 1;
				}
			}
		}


		// main radio boxes: page, section, notebook
		private void ChangeScopeRadioSelection(object sender, EventArgs e)
		{
			// only handle IsChecked...
			if (sender is MoreRadioButton box && !box.Checked)
			{
				return;
			}

			if (sender == pageRadio)
			{
				sectionRadio.Checked = false;
				notebookRadio.Checked = false;
				topBox.Enabled = true;
				rightAlignBox.Enabled = topBox.Checked;
				secondaryBox.Enabled = true;
				todoBox.Enabled = true;
				locationBox.Enabled = true;
				styleBox.Enabled = true;
				levelsBox.Enabled = true;
				sectionPagePreviewBox.Enabled = false;
				sectionTimeBox.Enabled = false;
				pagesBox.Enabled = false;
				notebookPagePreviewBox.Enabled = false;
				notebookTimeBox.Enabled = false;
			}
			else if (sender == sectionRadio)
			{
				pageRadio.Checked = false;
				notebookRadio.Checked = false;
				topBox.Enabled = pagesBox.Enabled = false;
				rightAlignBox.Enabled = false;
				secondaryBox.Enabled = false;
				todoBox.Enabled = false;
				locationBox.Enabled = false;
				styleBox.Enabled = false;
				levelsBox.Enabled = false;
				sectionPagePreviewBox.Enabled = true;
				sectionTimeBox.Enabled = true;
				notebookPagePreviewBox.Enabled = false;
				notebookTimeBox.Enabled = false;
			}
			else
			{
				pageRadio.Checked = false;
				sectionRadio.Checked = false;
				topBox.Enabled = false;
				rightAlignBox.Enabled = false;
				secondaryBox.Enabled = false;
				todoBox.Enabled = false;
				locationBox.Enabled = false;
				styleBox.Enabled = false;
				levelsBox.Enabled = false;
				sectionPagePreviewBox.Enabled = false;
				sectionTimeBox.Enabled = false;
				pagesBox.Enabled = true;
				notebookPagePreviewBox.Enabled = pagesBox.Checked;
				notebookTimeBox.Enabled = true;
			}
		}


		private void ToggleTopBox(object sender, EventArgs e)
		{
			if (topBox.Checked)
			{
				rightAlignBox.Enabled = true;
			}
			else
			{
				rightAlignBox.Enabled = rightAlignBox.Checked = false;
			}
		}


		private void TogglePagesBox(object sender, EventArgs e)
		{
			if (pagesBox.Checked)
			{
				notebookPagePreviewBox.Enabled = true;
			}
			else
			{
				notebookPagePreviewBox.Enabled = notebookPagePreviewBox.Checked = false;
			}
		}


		private void CollectParametersOnOK(object sender, EventArgs e)
		{
			parameters.Clear();

			if (pageRadio.Checked)
			{
				parameters.Add("page");
				parameters.Add($"style{styleBox.SelectedIndex}");
				parameters.Add($"level{levelsBox.Value}");
				if (topBox.Checked) parameters.Add("links");
				if (rightAlignBox.Checked) parameters.Add("align");
				if (secondaryBox.Checked) parameters.Add("secondary");

				if (locationBox.SelectedIndex == 2) parameters.Add("here");
				else if (locationBox.SelectedIndex == 1) parameters.Add("over");

				if (todoBox.SelectedIndex > 0)
					parameters.Add($"todo{todoBox.SelectedIndex - 1}");
			}
			else if (sectionRadio.Checked)
			{
				parameters.Add("section");
				if (sectionPagePreviewBox.Checked) parameters.Add("preview");
				if (sectionTimeBox.Checked) parameters.Add("time");
			}
			else
			{
				parameters.Add("notebook");
				if (pagesBox.Checked) parameters.Add("pages");
				if (notebookPagePreviewBox.Checked) parameters.Add("preview");
				if (notebookTimeBox.Checked) parameters.Add("time");
			}
		}
	}
}
