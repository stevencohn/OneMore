﻿//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Styles;
	using System.IO;
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
			var path = Path.Combine(PathHelper.GetAppDataPath(), Resx.ThemesFolder);
			PathHelper.EnsurePathExists(path);

			using var dialog = new OpenFileDialog
			{
				DefaultExt = "xml",
				Filter = Resx.LoadStyleTheme_filter,
				Multiselect = false,
				Title = Resx.LoadStyleTheme_Title,
				ShowHelp = true,			// stupid, but this is needed to avoid hang
				AutoUpgradeEnabled = true,	// simpler UI, faster
				InitialDirectory = path
			};

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

			ShowError(Resx.LoadStyleTheme_errorLoading);
			return null;
		}
	}
}
