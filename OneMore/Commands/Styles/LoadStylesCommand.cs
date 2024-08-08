//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Load custom styles from a specified file and update the style gallery
	/// </summary>
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

				logger.WriteLine($"loaded theme {theme.Key}");
				ribbon.Invalidate();
			}

			await Task.Yield();
		}


		public Theme LoadTheme()
		{
			var path = ThemeProvider.GetCustomThemeDirectory();
			if (!Directory.Exists(path) ||
				!Directory.EnumerateFiles(path, "*.xml").Any())
			{
				path = ThemeProvider.GetThemeDirectory();
				PathHelper.EnsurePathExists(path);
			}

			using var dialog = new OpenFileDialog
			{
				DefaultExt = "xml",
				Filter = Resx.LoadStyleTheme_filter,
				Multiselect = false,
				Title = Resx.LoadStyleTheme_Title,
				ShowHelp = true,            // stupid, but this is needed to avoid hang
				AutoUpgradeEnabled = true,  // simpler UI, faster
				InitialDirectory = path
			};

			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return null;
			}

			var provider = new ThemeProvider(dialog.FileName);
			var theme = provider.Theme;
			if (theme is not null)
			{
				var styles = theme.GetStyles();
				if (styles.Count > 0)
				{
					ThemeProvider.RecordTheme(theme.Key);
					return theme;
				}
			}

			ShowError(Resx.LoadStyleTheme_errorLoading);
			return null;
		}
	}
}
