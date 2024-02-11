//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.UI
{
	using System;
	using System.ComponentModel;
	using System.Windows.Forms;


	/// <summary>
	/// This is a standard TextBox that intercepts pressing the Enter key so it does not also
	/// cause the default (accept) button of a dialog to be pressed. Instead, a press of the
	/// Enter key is projected in its own custom event, PressedEnter.
	/// </summary>
	internal class MoreTextBox : TextBox, ILoadControl
	{
		private const int WM_KEYDOWN = 256;


		public MoreTextBox()
		{
			BorderStyle = BorderStyle.FixedSingle;
		}


		/// <summary>
		/// The event fired when the Enter key is pressed in this TextBox
		/// </summary>
		public event EventHandler PressedEnter;


		[Category("More"),
		Description("Catch and process Enter key with ProcessedEnter event")]
		public bool ProcessEnterKey { get; set; }


		public string ThemedBack { get; set; }


		public string ThemedFore { get; set; }


		void ILoadControl.OnLoad()
		{
			var manager = ThemeManager.Instance;
			ForeColor = manager.GetColor(ThemedFore ?? "WindowText");
			BackColor = manager.GetColor(Enabled ? (ThemedBack ?? "Window") : "InactiveWindow");
		}


		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			((ILoadControl)this).OnLoad();
		}


		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			if (ProcessEnterKey && m.Msg == WM_KEYDOWN && keyData == Keys.Enter)
			{
				PressedEnter?.Invoke(this, new EventArgs());

				// stop further propagation
				return true;
			}

			// else default handlers...
			return base.ProcessCmdKey(ref m, keyData);
		}
	}
}
