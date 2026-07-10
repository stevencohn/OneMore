//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// CLI-only command that adds hashtags to the tag bank at the top of one or more pages.
	/// </summary>
	internal class AddHashtagCommand : Command, ICliPageCommand
	{
		public AddHashtagCommand()
		{
			IsCancelled = true;
		}


		public string CommandName => "AddHashtag";

		public string Description => "Add hashtags to the tag bank on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to update", required: true)
			.AddString("section",
				"Path of the section to update; omit to update all sections in the notebook",
				required: false)
			.AddString("page", "Name of the page to update; requires the section parameter",
				required: false)
			.AddString("tags",
				"space-separated hashtags to add, e.g. \"#work #project\"",
				required: true);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args[0] as CliParameterSet;

			cliParams.TryGet("tags", out string tags);
			if (string.IsNullOrWhiteSpace(tags))
			{
				logger.WriteLine("AddHashtagCommand: no tags specified");
				return;
			}

			tags = NormalizeTags(tags);

			await using var one = new OneNote();
			cliParams.TryGet("pageId", out string pageId);
			await ProcessPage(one, pageId, tags);
		}


		/// <summary>
		/// Normalizes a space/tab-separated list of words into hashtags, adding a leading
		/// '#' to any token that doesn't already have one. Exposed for reuse by other
		/// commands (e.g. CreateJournalCommand) that need to tag pages they create directly
		/// rather than through the CLI.
		/// </summary>
		/// <param name="tags">A space/tab-separated list of words, with or without '#'</param>
		/// <returns>A space-separated list of hashtags, each prefixed with '#'</returns>
		internal static string NormalizeTags(string tags)
		{
			var tokens = tags
				.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			return string.Join(" ", tokens.Select(t => t.StartsWith("#") ? t : "#" + t));
		}


		private async Task ProcessPage(OneNote one, string pageId, string tags)
		{
			var page = await one.GetPage(pageId);
			if (page == null) return;

			if (AddTagsToBank(page, tags))
			{
				await one.Update(page);

				logger.Verbose($"AddHashtagCommand: added tags to page: {page.Title}");
			}
		}


		/// <summary>
		/// Adds the given (already-normalized) hashtags to the tag bank at the top of the
		/// given page, creating the bank if it doesn't already exist. Exposed for reuse by
		/// other commands (e.g. CreateJournalCommand).
		/// </summary>
		/// <param name="page">The page to update; not saved by this method</param>
		/// <param name="tags">A space-separated list of hashtags, each prefixed with '#'</param>
		/// <returns>True if the page was updated and should be saved</returns>
		internal static bool AddTagsToBank(Page page, string tags)
		{
			var ns = page.Namespace;
			var banker = new TagBankCommand();
			banker.MakeWordBank(page, ns);

			if (banker.BankOutline is null) return false;

			var run = banker.BankOutline.Descendants(ns + "T").FirstOrDefault();
			if (run?.GetCData() is XCData cdata)
			{
				cdata.Value = $"{cdata.Value} {tags}";
				return true;
			}

			return false;
		}
	}
}
