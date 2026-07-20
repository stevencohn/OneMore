//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// Prompts for a regular expression and a style, either a built-in OneNote style or
	/// a custom OneMore style, and applies that style to every match found on the
	/// current page.
	/// </summary>
	internal class ApplyConditionalFormatCommand : Command, ICliPageCommand
	{
		#region ICliCommand

		public string CommandName => "ConditionalFormat";
		public string Description => "Apply a style to text on a page matching a regular expression";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of notebook (omit when --pageId is used)", required: false)
			.AddString("section", "Path of section", required: false)
			.AddString("page", "Name of page (omit or * for all pages in section)", required: false)
			.AddString("pageId", "Direct page ID, overrides --notebook / --section / --page",
				required: false)
			.AddString("regex", "Regular expression pattern to match", required: true)
			.AddString("style", "Name of the built-in or custom style to apply", required: true)
			.AddBoolean("custom", "Treat the style name as a custom OneMore style " +
				"instead of a built-in OneNote style", required: false, defaultValue: false);

		#endregion ICliCommand


		public override async Task Execute(params object[] args)
		{
			var cliParams = args.Length > 0 ? args[0] as CliParameterSet : null;
			if (cliParams != null)
			{
				await ExecuteAsCli(cliParams);
				return;
			}

			using var dialog = new ConditionalFormatDialog();
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			await using var one = new OneNote(out var page, out _);

			var style = dialog.UseBuiltIn
				? page.GetQuickStyle(dialog.SelectedStandardStyle)
				: dialog.SelectedCustomStyle;

			if (style is null)
			{
				return;
			}

			var regex = new Regex(dialog.Pattern);
			var editor = new ConditionalFormatEditor(regex, style);
			var count = editor.Apply(page);

			logger.WriteLine($"found {count} matches on {page.Title}");

			if (count > 0)
			{
				await one.Update(page);
			}
			else
			{
				ShowInfo(Resx.ApplyConditionalFormatCommand_NoMatches);
			}
		}


		private async Task ExecuteAsCli(CliParameterSet cliParams)
		{
			cliParams.TryGet("pageId", out string pageId);
			if (string.IsNullOrWhiteSpace(pageId))
			{
				return;
			}

			var pattern = cliParams.Get<string>("regex");
			var styleName = cliParams.Get<string>("style");
			var custom = cliParams.TryGet("custom", out bool c) && c;

			await using var one = new OneNote();
			var page = await one.GetPage(pageId, OneNote.PageDetail.All);

			var style = custom
				? FindCustomStyle(styleName)
				: FindStandardStyle(page, styleName);

			if (style is null)
			{
				logger.WriteLine($"style not found: {styleName}");
				return;
			}

			var regex = new Regex(pattern);
			var editor = new ConditionalFormatEditor(regex, style);
			var count = editor.Apply(page);

			logger.Verbose($"found {count} matches on {page.Title}");

			if (count > 0)
			{
				await one.Update(page);
			}
		}


		private static Style FindCustomStyle(string styleName)
		{
			return new ThemeProvider().Theme.GetStyles()
				.FirstOrDefault(s => s.Name.Equals(styleName, StringComparison.OrdinalIgnoreCase));
		}


		private static Style FindStandardStyle(Page page, string styleName)
		{
			// allow display-style names with spaces, e.g. "Heading 1" or "Page Title"
			var name = styleName.Replace(" ", string.Empty);

			return Enum.TryParse<StandardStyles>(name, true, out var standard)
				? page.GetQuickStyle(standard)
				: null;
		}
	}
}
