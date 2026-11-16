//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable CS3003  // Type is not CLS-compliant

namespace River.OneMoreAddIn.Commands
{
	using System.Drawing;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	/// <summary>
	/// Prompts for the scope of pages whose cursor will be reset to the title.
	/// Built in code to avoid a designer/resx pair for a single control.
	/// </summary>
	internal class ResetCursorDialog : UI.MoreForm
	{
		private readonly UI.ScopeSelector scopeSelector;
		private readonly UI.MoreButton okButton;


		public ResetCursorDialog()
		{
			Text = Resx.ribResetCursorButton_Label;
			AutoScaleMode = AutoScaleMode.Font;
			AutoScaleDimensions = new SizeF(9F, 20F);
			ClientSize = new Size(460, 300);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			Padding = new Padding(15);

			var group = new UI.MoreGroupBox
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Location = new Point(18, 18),
				Size = new Size(424, 190),
				Padding = new Padding(15),
				ShowOnlyTopEdge = true,
				Text = Resx.word_Scope,
				TabStop = false
			};

			scopeSelector = new UI.ScopeSelector
			{
				Location = new Point(18, 34),
				Margin = new Padding(0),
				Size = new Size(345, 140),
				Scopes = UI.SelectorScope.Page | UI.SelectorScope.Section |
					UI.SelectorScope.Notebook | UI.SelectorScope.Notebooks,
				TabIndex = 0
			};

			group.Controls.Add(scopeSelector);

			okButton = new UI.MoreButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				DialogResult = DialogResult.OK,
				Location = new Point(236, 242),
				Size = new Size(100, 38),
				Text = Resx.word_OK,
				TabIndex = 1
			};

			var cancelButton = new UI.MoreButton
			{
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
				DialogResult = DialogResult.Cancel,
				Location = new Point(342, 242),
				Size = new Size(100, 38),
				Text = Resx.word_Cancel,
				TabIndex = 2
			};

			Controls.Add(group);
			Controls.Add(okButton);
			Controls.Add(cancelButton);

			AcceptButton = okButton;
			CancelButton = cancelButton;
		}


		protected override void OnShown(System.EventArgs e)
		{
			base.OnShown(e);
			ActiveControl = okButton;
		}


		/// <summary>
		/// Gets the scope of pages to reset.
		/// </summary>
		public UI.SelectorScope Scope => scopeSelector.Scope;
	}
}
