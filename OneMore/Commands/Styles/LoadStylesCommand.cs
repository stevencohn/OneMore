//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Styles;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Load custom styles from a specified file and update the style gallery
	/// </summary>
	internal class LoadStylesCommand : Command, ICliCommand
	{
		public LoadStylesCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public string CommandName => "LoadStyles";
		public string Description => "Load a style theme by name";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("theme", "Name of the theme to load, e.g. Oranges", required: true);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				cliParams.TryGet("theme", out string key);
				if (!string.IsNullOrWhiteSpace(key))
				{
					var theme = ApplyTheme(key);
					if (theme != null)
						logger.WriteLine($"loaded theme {theme.Key}");
				}

				await Task.Yield();
				return;
			}

			var ribbonTheme = LoadTheme();
			if (ribbonTheme != null)
			{
				logger.WriteLine($"loaded theme {ribbonTheme.Key}");
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

			var theme = ApplyTheme(dialog.FileName);
			if (theme is null)
				ShowError(Resx.LoadStyleTheme_errorLoading);

			return theme;
		}


		private Theme ApplyTheme(string key)
		{
			var provider = new ThemeProvider(key);
			var theme = provider.Theme;
			if (theme?.GetStyles().Count > 0)
			{
				ThemeProvider.RecordTheme(theme.Key);
				return theme;
			}

			return null;
		}
	}
}
