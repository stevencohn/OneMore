//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System.Threading;
	using System.Windows.Forms;


	public partial class ProgressDialog : Form
	{
		private CancellationTokenSource source;


		public ProgressDialog()
		{
			InitializeComponent();

			cancelButton.Visible = false;
			Height = 180;
		}


		public ProgressDialog(CancellationTokenSource source)
		{
			InitializeComponent();

			this.source = source;
		}


		public string Message
		{
			set
			{
				messageLabel.Text = value;
				messageLabel.Refresh();
			}
		}


		public int Maximum
		{
			set => progressBar.Maximum = value;
		}


		public int Progress
		{
			set => progressBar.Value = value;
		}


		public void Increment(int step = 1)
		{
			progressBar.Value += step;
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			source.Cancel();
		}
	}
}
