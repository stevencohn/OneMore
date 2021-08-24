//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class LoadStylesCommand : Command
	{
		public LoadStylesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public Theme Theme { private set; get; }


		public override async Task Execute(params object[] args)
		{
			using (var dialog = new OpenFileDialog())
			{
				dialog.DefaultExt = "xml";
				dialog.AddExtension = true;
				dialog.Filter = "Theme files (*.xml)|*.xml|All files (*.*)|*.*";
				dialog.Multiselect = false;
				dialog.Title = "Open Style Theme";
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang

				var path = Path.Combine(PathFactory.GetAppDataPath(), Resx.ThemesFolder);
				PathFactory.EnsurePathExists(path);

				var result = dialog.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					var provider = new ThemeProvider(dialog.FileName);
					var theme = provider.Theme;

					var styles = theme.GetStyles();
					if (styles.Count > 0)
					{
						Theme = theme;

						ThemeProvider.RecordTheme(theme.Key);
						ribbon?.Invalidate();
					}
					else
					{
						UIHelper.ShowError("Cloud not load style theme file");
					}
				}
			}

			await Task.Yield();
		}
	}
}
