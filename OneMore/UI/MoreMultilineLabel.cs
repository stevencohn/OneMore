//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;


	/// <summary>
	/// Manages a multi-line label with auto-wrapping. It must be Docked to its parent
	/// with Fill mode or left/right anchored.
	/// </summary>
	internal class MoreMultilineLabel : Panel, ILoadControl
	{
		private readonly MoreLabel label;
		private string themedBack;
		private string themedFore;


		public MoreMultilineLabel()
			: base()
		{
			label = new MoreLabel
			{
				AutoSize = true,
				Dock = DockStyle.Fill
			};

			Controls.Add(label);
		}


		public string ThemedBack
		{
			get => themedBack;
			set => themedBack = label.ThemedBack = value;
		}


		public string ThemedFore
		{
			get => themedFore;
			set => themedFore = label.ThemedFore = value;
		}


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;

			BackColor = Parent != null
				? Parent.BackColor
				: manager.GetColor("Control", ThemedBack);

			ForeColor = Enabled
				? manager.GetColor("ControlText", ThemedFore)
				: manager.GetColor("GrayText");

			label.MaximumSize = new Size(Width - Padding.Left - Padding.Right, 0);
		}


		[Category("More"),
			Description("Text to display"),
			EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
		public override string Text
		{
			get => label.Text;
			set => label.Text = value;
		}


		protected override void OnResize(EventArgs eventargs)
		{
			// magic to make the label wrap
			base.OnResize(eventargs);
			label.MaximumSize = new Size(Width - Padding.Left - Padding.Right, 0);
		}
	}
}
