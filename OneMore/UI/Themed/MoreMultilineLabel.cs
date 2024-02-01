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
	internal class MoreMultilineLabel : Panel, IThemedControl
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


		public void ApplyTheme(ThemeManager manager)
		{
			BackColor = Parent != null
				? Parent.BackColor
				: manager.GetThemedColor("Control", ThemedBack);

			ForeColor = Enabled
				? manager.GetThemedColor("ControlText", ThemedFore)
				: manager.GetThemedColor("GrayText");
		}


		[Category("OneMore"), Description("Text to display")]
		public override string Text
		{
			get => label.Text;
			set => label.Text = base.Text = value;
		}


		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);
			label.MaximumSize = new Size(Width, 0);
		}
	}
}
