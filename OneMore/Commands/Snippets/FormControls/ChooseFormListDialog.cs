//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Asks the user which list a new List form control should offer.
	/// </summary>
	internal sealed class ChooseFormListDialog : MoreForm
	{
		private readonly MoreComboBox listBox;


		public ChooseFormListDialog(IEnumerable<string> names)
		{
			// authored at 150% like the designer-built dialogs (AutoScaleDimensions 9F,20F) so
			// MoreForm and AutoScaleMode.Font scale it for the actual monitor DPI

			var okButton = new MoreButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				BackColor = Color.FromArgb(240, 240, 240),
				DialogResult = DialogResult.OK,
				ForeColor = Color.Black,
				Location = new Point(238, 97),
				Name = "okButton",
				ShowBorder = true,
				Size = new Size(116, 38),
				TabIndex = 2,
				Text = Resx.word_OK,
				ThemedBack = null,
				ThemedFore = null,
				UseVisualStyleBackColor = true
			};

			var cancelButton = new MoreButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				BackColor = Color.FromArgb(240, 240, 240),
				DialogResult = DialogResult.Cancel,
				ForeColor = Color.Black,
				Location = new Point(358, 97),
				Name = "cancelButton",
				ShowBorder = true,
				Size = new Size(116, 38),
				TabIndex = 3,
				Text = Resx.word_Cancel,
				ThemedBack = null,
				ThemedFore = null,
				UseVisualStyleBackColor = true
			};

			var label = new Label
			{
				AutoSize = true,
				Location = new Point(23, 26),
				Name = "listLabel",
				TabIndex = 0,
				Text = Resx.FormControlsSheet_listsLabel_Text
			};

			listBox = new MoreComboBox
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				DropDownStyle = ComboBoxStyle.DropDownList,
				Location = new Point(118, 23),
				Name = "listBox",
				Size = new Size(356, 28),
				TabIndex = 1
			};

			foreach (var name in names)
			{
				listBox.Items.Add(name);
			}

			if (listBox.Items.Count > 0)
			{
				listBox.SelectedIndex = 0;
			}

			SuspendLayout();

			AcceptButton = okButton;
			AutoScaleDimensions = new SizeF(9F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.ControlLightLight;
			CancelButton = cancelButton;
			ClientSize = new Size(490, 150);
			ForeColor = SystemColors.ControlText;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ChooseFormListDialog";
			Padding = new Padding(20, 20, 0, 0);
			StartPosition = FormStartPosition.CenterParent;
			Text = Resx.ribInsertListControlButton_Label;

			Controls.Add(listBox);
			Controls.Add(label);
			Controls.Add(okButton);
			Controls.Add(cancelButton);

			ResumeLayout(false);
			PerformLayout();
		}


		/// <summary>
		/// Gets the name of the chosen list; valid when the dialog result is OK.
		/// </summary>
		public string SelectedList => listBox.SelectedItem as string;
	}
}
