//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
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


		public override async Task Execute(params object[] args)
		{
			var theme = LoadTheme();
			if (theme != null)
			{
				ThemeProvider.RecordTheme(theme.Key);
				ribbon.Invalidate();
			}

			await Task.Yield();
		}


		public Theme LoadTheme()
		{
			var path = Path.Combine(PathHelper.GetAppDataPath(), Resx.ThemesFolder);
			PathHelper.EnsurePathExists(path);

			using (var dialog = new OpenFileDialog())
			{
				dialog.DefaultExt = "xml";
				dialog.Filter = "Theme files (*.xml)|*.xml|All files (*.*)|*.*";
				dialog.Multiselect = false;
				dialog.Title = "Open Style Theme";
				dialog.ShowHelp = true; // stupid, but this is needed to avoid hang
				dialog.AutoUpgradeEnabled = true; // simpler UI, faster
				dialog.InitialDirectory = path;

				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return null;
				}

				var theme = new ThemeProvider(dialog.FileName).Theme;
				if (theme != null)
				{
					var styles = theme.GetStyles();
					if (styles.Count > 0)
					{
						return theme;
					}
				}

				UIHelper.ShowError("could not load theme file or them contains no styles");
				return null;
			}
		}
	}
}
