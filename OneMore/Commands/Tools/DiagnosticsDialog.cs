//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	internal partial class DiagnosticsDialog : UI.MoreForm
	{
		private const int Timeout = 5000;
		private int time;
		private readonly string okText;


		public DiagnosticsDialog()
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.DiagnosticsDialog_Text;

				Localize(new string[]
				{
					"introLabel",
					"okButton=word_OK"
				});
			}

			time = Timeout;
			okText = okButton.Text;
			okButton.Text = $"{okText} ({(time / 1000)})";
		}


		public DiagnosticsDialog(string path)
			: this()
		{
			linkLabel.Text = path;

			using var g = CreateGraphics();
			var size = g.MeasureString(path, linkLabel.Font);
			var reqwidth = size.Width + 40;
			if (reqwidth > Width)
			{
				Width = (int)reqwidth;
			}
		}


		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			timer.Start();
		}


		private void Tick(object sender, EventArgs e)
		{
			time -= timer.Interval;
			if (time > 0)
			{
				okButton.Text = $"{okText} ({(time / 1000)})";
				return;
			}

			CloseWindow(sender, e);
		}


		private void ShowLogOnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(linkLabel.Text);
			Close();
		}


		private void CloseWindow(object sender, EventArgs e)
		{
			timer.Stop();
			timer.Dispose();
			Close();
		}
	}
}
