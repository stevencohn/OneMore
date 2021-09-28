namespace River.OneMoreAddIn.UI
{
	using System.Windows.Forms;


	/// <summary>
	/// Some system CommonDialog windows like Color dialog, even with top-most bit set using the
	/// SetWindowsPos function, will not appear top-most the first time they are displayed.
	/// This invisible Form hosts the window and plays a trick with TopMost to force the dialog
	/// to the top the first time it is shown.
	/// Can be used with either a Form or a CommonDialog like ColorDialog
	/// </summary>
	internal class WindowElevator : Form
	{
		private readonly object form;


		public WindowElevator(object form)
			: base()
		{
			Width = 0;
			Height = 0;
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.None;
			Opacity = 0;
			Visible = false;
			TopMost = true;

			this.form = form;
		}


		protected override void OnShown(System.EventArgs e)
		{
			base.OnShown(e);

			// this is the trick needed to elevate a system dialog to TopMost
			TopMost = false;
			TopMost = true;

			if (form is Form control)
			{
				DialogResult = control.ShowDialog(this);
			}
			else
			{
				DialogResult = ((CommonDialog)form).ShowDialog(this);
			}

			Close();
		}
	}
}
