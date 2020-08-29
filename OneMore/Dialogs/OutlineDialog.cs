//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant
#pragma warning disable IDE1006 // Words must begin with upper case

namespace River.OneMoreAddIn.Dialogs
{
	using System;
	using System.Windows.Forms;


	public partial class OutlineDialog : Form
	{

		public OutlineDialog()
		{
			InitializeComponent();

			TagSymbol = 0;
		}


		public bool AlphaNumbering => alphaRadio.Enabled && alphaRadio.Checked;

		public bool NumericNumbering => numRadio.Enabled && numRadio.Checked;

		public bool CleanupNumbering => cleanBox.Checked;

		public bool Indent => indentBox.Checked;

		public bool IndentTagged => indentTagBox.Checked;

		public bool RemoveTags => removeTagsBox.Checked;

		public int TagSymbol { get; private set; }


		protected override void OnShown(EventArgs e)
		{
			Location = new System.Drawing.Point(Location.X, Location.Y - (Height / 2));
			UIHelper.SetForegroundWindow(this);
		}


		private void numberingBox_CheckedChanged(object sender, EventArgs e)
		{
			alphaRadio.Enabled = numberingBox.Checked;
			numRadio.Enabled = numberingBox.Checked;
			SetOK();
		}


		private void cleanBox_CheckedChanged(object sender, EventArgs e)
		{
			SetOK();
		}


		private void indentBox_CheckedChanged(object sender, EventArgs e)
		{
			SetOK();
		}


		private void indentTagBox_CheckedChanged(object sender, EventArgs e)
		{
			tagButton.Enabled = removeTagsBox.Enabled = indentTagBox.Checked;
			SetOK();
		}


		private void tagButton_Click(object sender, EventArgs e)
		{
			var location = PointToScreen(tagButton.Location);

			using (var dialog = new TagPickerDialog(
				location.X + tagButton.Bounds.Location.X - tagButton.Width,
				location.Y + tagButton.Bounds.Location.Y))
			{
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					var glyph = dialog.GetGlyph();
					if (glyph != null)
					{
						tagButton.Text = null;
						tagButton.Image = glyph;
					}
					else
					{
						tagButton.Text = "?";
					}

					TagSymbol = dialog.Symbol;
				}
			}
		}


		private void SetOK()
		{
			okButton.Enabled = 
				numberingBox.Checked || cleanBox.Checked || 
				indentBox.Checked || indentTagBox.Checked;
		}


		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}


		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
