//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Cli;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// CLI-only command that removes hashtags from the tag bank on one or more pages.
	/// If the bank is empty after removal it is deleted from the page.
	/// </summary>
	internal class RemoveHashtagCommand : Command, ICliPageCommand
	{
		public RemoveHashtagCommand()
		{
			IsCancelled = true;
		}


		public string CommandName => "RemoveHashtag";

		public string Description => "Remove hashtags from the tag bank on one or more pages";

		public CliParameterDefinition DefineParameters() =>
			new CliParameterDefinition()
			.AddString("notebook", "Name of the notebook to update", required: true)
			.AddString("section",
				"Path of the section to update; omit to update all sections in the notebook",
				required: false)
			.AddString("page", "Name of the page to update; requires the section parameter",
				required: false)
			.AddString("tags",
				"space-separated hashtags to remove, e.g. \"#work #project\"",
				required: true);


		public override async Task Execute(params object[] args)
		{
			var cliParams = args[0] as CliParameterSet;

			cliParams.TryGet("tags", out string tags);
			if (string.IsNullOrWhiteSpace(tags))
			{
				logger.WriteLine("RemoveHashtagCommand: no tags specified");
				return;
			}

			tags = NormalizeTags(tags);

			await using var one = new OneNote();
			cliParams.TryGet("pageId", out string pageId);
			await ProcessPage(one, pageId, tags);
		}


		private static string NormalizeTags(string tags)
		{
			var tokens = tags
				.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

			return string.Join(" ", tokens.Select(t => t.StartsWith("#") ? t : "#" + t));
		}


		private async Task ProcessPage(OneNote one, string pageId, string tags)
		{
			var page = await one.GetPage(pageId);
			if (page == null) return;

			var changed = RemoveTagsFromBank(one, page, tags);
			if (changed)
			{
				await one.Update(page);

				logger.Verbose($"RemoveHashtagCommand: updated page: {page.Title}");
			}
		}


		private static bool RemoveTagsFromBank(OneNote one, Page page, string tags)
		{
			var ns = page.Namespace;

			var outline = page.Root.Elements(ns + "Outline")
				.FirstOrDefault(e => e.Elements().Any(x =>
					x.Name.LocalName == "Meta" &&
					x.Attribute("name")?.Value == MetaNames.TaggingBank));

			if (outline is null) return false;

			var run = outline.Descendants(ns + "T").FirstOrDefault();
			if (run?.GetCData() is not XCData cdata) return false;

			var updated = cdata.Value;
			foreach (var tag in tags.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
			{
				updated = Regex.Replace(updated,
					@$"(?i)\s*{Regex.Escape(tag)}\b", string.Empty);
			}

			if (updated == cdata.Value) return false;

			// strip HTML tags to check whether any hashtag tokens remain
			var plainText = Regex.Replace(updated, @"<[^>]+>", string.Empty).Trim();
			if (!Regex.IsMatch(plainText, @"#\w+"))
			{
				// bank is now empty — delete the outline element entirely
				if (outline.GetAttributeValue("objectID", out string objectId))
				{
					one.DeleteContent(page.PageId, objectId);
				}

				outline.Remove();
			}
			else
			{
				cdata.Value = updated;
			}

			return true;
		}
	}
}
