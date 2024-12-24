//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Snippets.TocGenerators;
	using System;
	using Resx = Properties.Resources;


	internal partial class InsertTocDialog : UI.MoreForm
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
					"pageRadio",
					"topBox",
					"rightAlignBox",
					"locationLabel",
					"locationBox",
					"styleLabel",
					"styleBox",
					"sectionRadio",
					"previewBox",
					"notebookRadio",
					"pagesBox",
					"preview2Box=InsertTocDialog_previewBox.Text",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			locationBox.SelectedIndex = 0;
			styleBox.SelectedIndex = 0;
		}


		public InsertTocDialog(TocParameters parameters)
			: this()
		{
			this.parameters = parameters;

			if (parameters.Contains("section"))
			{
				sectionRadio.Checked = true;
				sectionPagePreviewBox.Checked = parameters.Contains("preview");
			}
			else if (parameters.Contains("notebook"))
			{
				notebookRadio.Checked = true;
				pagesBox.Checked = parameters.Contains("pages");
				notebookPagePreviewBox.Checked = parameters.Contains("preview");
			}
			else // page is default
			{
				pageRadio.Checked = true;
				topBox.Checked = parameters.Contains("links");
				rightAlignBox.Checked = parameters.Contains("align");

				if (parameters.Contains("here")) locationBox.SelectedIndex = 2;
				else if (parameters.Contains("over")) locationBox.SelectedIndex = 1;
				else locationBox.SelectedIndex = 0;

				if (parameters.Find(p => p.StartsWith("style")) is string style)
				{
					if (int.TryParse(style.Substring(5), out var index))
					{
						styleBox.SelectedIndex = index;
					}
				}
			}
		}


		// main radio boxes: page, section, notebook
		private void ChangeScopeRadioSelection(object sender, EventArgs e)
		{
			if (sender == pageRadio)
			{
				topBox.Enabled = true;
				rightAlignBox.Enabled = topBox.Checked;
				locationBox.Enabled = true;
				styleBox.Enabled = true;
				sectionPagePreviewBox.Enabled = false;
				pagesBox.Enabled = false;
				notebookPagePreviewBox.Enabled = false;
			}
			else if (sender == sectionRadio)
			{
				topBox.Enabled = pagesBox.Enabled = false;
				rightAlignBox.Enabled = false;
				locationBox.Enabled = false;
				styleBox.Enabled = false;
				sectionPagePreviewBox.Enabled = true;
				notebookPagePreviewBox.Enabled = false;
			}
			else
			{
				topBox.Enabled = false;
				rightAlignBox.Enabled = false;
				locationBox.Enabled = false;
				styleBox.Enabled = false;
				sectionPagePreviewBox.Enabled = false;
				pagesBox.Enabled = true;
				notebookPagePreviewBox.Enabled = pagesBox.Checked;
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
				if (topBox.Checked) parameters.Add("links");
				if (rightAlignBox.Checked) parameters.Add("align");

				if (locationBox.SelectedIndex == 2) parameters.Add("here");
				else if (locationBox.SelectedIndex == 1) parameters.Add("over");
			}
			else if (sectionRadio.Checked)
			{
				parameters.Add("section");
				if (sectionPagePreviewBox.Checked) parameters.Add("preview");
			}
			else
			{
				parameters.Add("notebook");
				if (pagesBox.Checked) parameters.Add("pages");
				if (notebookPagePreviewBox.Checked) parameters.Add("preview");
			}
		}
	}
}
