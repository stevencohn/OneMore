//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved. 
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// A temporary solution to upgrade legacy Page tags to modern and more efficient Hastags.
	/// The Page tags commands will be removed in a future version of OneMore.
	/// </summary>
	internal class LegacyTaggingConverter : Loggable
	{
		public const string SettingsName = "tagging";

		private XElement root;
		private XNamespace ns;


		/// <summary>
		/// Gets a Boolean value indicating whether Page tags were just or have previously
		/// been upgraded.
		/// </summary>
		public bool Converted { get; private set; } = false;


		/// <summary>
		/// The number of pages containing legacy Page tags that were affected.
		/// </summary>
		public int PagesConverted { get; private set; } = 0;


		/// <summary>
		/// The number of legacy Page tags that were upgraded.
		/// </summary>
		public int TagsConverted { get; private set; } = 0;


		/// <summary>
		/// Perform the upgrade
		/// </summary>
		/// <returns>True if tags are or have been upgraded, otherwise false.</returns>
		public async Task<bool> UpgradeLegacyTags()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection(SettingsName);

			if (settings.Get("converted", false))
			{
				logger.Verbose("legacy hashtags previously converted");
				Converted = true;
				return true;
			}

			if (!await HasLegacyTags())
			{
				settings.Add("converted", true);
				provider.SetCollection(settings);
				provider.Save();

				logger.Verbose("no legacy hashtags to convert");
				Converted = true;
				return true;
			}

			logger.WriteLine("converting legacy hashtags");

			using var progress = new ProgressDialog();
			progress.ShowDialogWithCancel(
				async (pdialog, token) => await UpgradeLegacyTags(pdialog, token));

			settings.Add("converted", true);
			if (settings.IsModified)
			{
				provider.SetCollection(settings);
				provider.Save();
			}

			Converted = true;

			// scan now?

			if (HashtagProvider.CatalogExists())
			{
				var scheduler = new HashtagScheduler();
				if (!scheduler.ScheduleExists && scheduler.State == ScanningState.Ready)
				{
					using var scanner = new HashtagScanner();
					await scanner.Scan();
					scanner.Report("legacy converter");
				}
			}

			return Converted;
		}


		private async Task<bool> HasLegacyTags()
		{
			await using var one = new OneNote();

			root = await one.SearchMeta(string.Empty, MetaNames.TaggingLabels);
			if (root == null)
			{
				// error parsing hierarchy XML but OK
				return false;
			}

			// page level tags are only deleted if their content attribute is empty, but
			// page tags never considered that so may exist AND have empty content.
			// So need to check if any have content values...

			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			var found = root.Descendants(ns + "Meta").Any(e =>
				e.Attribute("name").Value == MetaNames.TaggingLabels &&
				e.Attribute("content") != null &&
				!string.IsNullOrWhiteSpace(e.Attribute("content").Value));

			return found;
		}


		private async Task<bool> UpgradeLegacyTags(ProgressDialog dialog, CancellationToken token)
		{
			await using var one = new OneNote();

			var items = root.Descendants(ns + "Page");
			dialog.SetMaximum(items.Count());

			foreach (var item in items)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				dialog.SetMessage(item.Attribute("name").Value);
				var page = await one.GetPage(item.Attribute("ID").Value, OneNote.PageDetail.Basic);

				if (token.IsCancellationRequested)
				{
					break;
				}

				var updated = false;

				// update legacy tags in the tagging bank by prefixing them with a hashtag...

				var bank = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.TaggingBank);

				if (bank is not null &&
					bank.Attribute("content") is XAttribute flag)
				{
					logger.WriteLine($"converting page tags on {page.PageId} \"{page.Title}\"");

					var cdata = bank.ElementsAfterSelf(ns + "OEChildren")
						.Elements(ns + "OE")
						.Elements(ns + "T")
						.Nodes().OfType<XCData>();

					foreach (var datum in cdata)
					{
						// build new list to filter out duplicates
						var hashtags = new List<string>();

						var tags = datum.Value.Split(
							new string[] { AddIn.Culture.TextInfo.ListSeparator },
							StringSplitOptions.RemoveEmptyEntries);

						foreach (var tag in tags)
						{
							var hashtag = tag.Trim();
							if (hashtag.Length > 0)
							{
								if (hashtag[0] != '#')
								{
									hashtag = $"#{hashtag}";
								}

								if (!hashtags.Contains(hashtag))
								{
									hashtags.Add(hashtag);
									TagsConverted++;
								}
							}
						}

						var sep = page.IsRightToLeft()
							? $" {AddIn.Culture.TextInfo.ListSeparator}"
							: $"{AddIn.Culture.TextInfo.ListSeparator} ";

						datum.Value = string.Join(sep, hashtags);
					}

					// clear the tagging bank meta element value
					flag.Value = string.Empty;
					updated = true;
				}

				// remove the omTaggingLabels Meta element from the page...

				if (!token.IsCancellationRequested)
				{
					// delete the omTaggingLabels Meta element by removing its content attribute,
					// should only be one
					var rogue = page.Root.Elements(ns + "Meta")
						.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.TaggingLabels &&
							e.Attribute("content") != null);

					if (rogue is not null &&
						rogue.Attribute("content") is XAttribute content)
					{
						logger.Verbose($"removing meta on {page.PageId} \"{page.Title}\"");
						content.Remove();
						updated = true;
					}
				}

				// save page if any updates...

				if (!token.IsCancellationRequested && updated)
				{
					logger.Verbose($"saving page {page.PageId} \"{page.Title}\"");
					await one.Update(page);
					PagesConverted++;
				}

				dialog.Increment();
			}

			return !token.IsCancellationRequested;
		}
	}
}
