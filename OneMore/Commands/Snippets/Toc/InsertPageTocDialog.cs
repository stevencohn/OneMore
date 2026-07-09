//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using Snippets.Toc;
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class InsertPageTocDialog : MoreForm
	{
		private readonly TocParameters parameters;


		public InsertPageTocDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.InsertPageTocDialog_Text;

				Localize(new string[]
				{
					"topBox",
					"rightAlignBox",
					"secondaryBox",
					"todoLabel",
					"locationLabel",
					"locationBox",
					"styleLabel",
					"styleBox",
					"levelsLabel",
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
			var lines = Resx.InsertPageTocDialog_todoBox_Text.Split(
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


		public InsertPageTocDialog(TocParameters parameters)
			: this()
		{
			this.parameters = parameters;

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


		private void CollectParametersOnOK(object sender, EventArgs e)
		{
			parameters.Clear();

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
	}
}
