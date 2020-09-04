//************************************************************************************************
// Copyright © 2018 Steven M Cohn.  Yada yada...
//************************************************************************************************

namespace River.OneMoreAddIn.Dialogs
{
	using System.Windows.Forms;


	public partial class ProgressDialog : Form
	{
		public ProgressDialog()
		{
			InitializeComponent();
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
	}
}
