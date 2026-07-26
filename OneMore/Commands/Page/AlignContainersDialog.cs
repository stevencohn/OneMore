//************************************************************************************************
// Copyright © 2026 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using Resx = Properties.Resources;


	/// <summary>
	/// Specifies how selected containers should be aligned relative to an anchor container
	/// (the first container in document order, except Right which anchors on the widest
	/// container so alignment can never push another container off the left edge).
	/// </summary>
	internal enum ContainerAlignment
	{
		Left,
		Right,
		Top
	}


	internal partial class AlignContainersDialog : UI.MoreForm
	{
		private readonly bool allSelected;


		public AlignContainersDialog(int selectedCount, bool allSelected)
		{
			this.allSelected = allSelected;

			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.AlignContainersDialog_Text;

				Localize(new string[]
				{
					"allLabel",
					"selectedButton",
					"allButton",
					"alignGroup",
					"leftButton",
					"rightButton",
					"topButton",
					"overlapCheckBox",
					"okButton=word_OK",
					"cancelButton=word_Cancel"
				});
			}

			selectedButton.Text = string.Format(selectedButton.Text, selectedCount);

			allLabel.Visible = allSelected;
			selectedButton.Visible = !allSelected;
			allButton.Visible = !allSelected;
		}


		public ContainerAlignment Alignment =>
			rightButton.Checked ? ContainerAlignment.Right :
			topButton.Checked ? ContainerAlignment.Top :
			ContainerAlignment.Left;


		public bool PreventOverlaps => overlapCheckBox.Checked;


		public bool ApplyToAll => allSelected || allButton.Checked;
	}
}
