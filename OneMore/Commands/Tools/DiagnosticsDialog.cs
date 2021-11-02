//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Diagnostics;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal partial class DiagnosticsDialog : UI.LocalizableForm
	{
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
		}


		public DiagnosticsDialog(string path)
			: this()
		{
			linkLabel.Text = path;

			using (var g = CreateGraphics())
			{
				var size = g.MeasureString(path, linkLabel.Font);
				var reqwidth = size.Width + 40;
				if (reqwidth > Width)
				{
					Width = (int)reqwidth;
				}
			}
		}


		private void ClickPath(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(linkLabel.Text);
			Close();
		}
	}
}
