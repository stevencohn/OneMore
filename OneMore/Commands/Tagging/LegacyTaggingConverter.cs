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
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// A temporary solution to upgrade legacy Page tags to modern and more efficient Hastags.
	/// The Page tags commands will be removed in a future version of OneMore.
	/// </summary>
	internal class LegacyTaggingConverter : Loggable
	{
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
		/// If there are legacy Page tags, ask the user if they want to upgrade to Hashtags
		/// and, if yes, then perform the upgrade.
		/// </summary>
		/// <param name="owner">The owning window so we can center dialogs</param>
		/// <returns>True if tags are or have been upgraded, otherwise false.</returns>
		public async Task<bool> UpgradeLegacyTags(IWin32Window owner)
		{
			if (!await NeedsConversion())
			{
				Converted = true;
				return false;
			}

			var provider = new SettingsProvider();
			var settings = provider.GetCollection("tagging");

			using var ltdialog = new LegacyTaggingDialog();

			if (ltdialog.ShowDialog(owner) == DialogResult.OK)
			{
				using var progress = new ProgressDialog();
				progress.ShowDialogWithCancel(
					async (dialog, token) => await UpgradeLegacyTags(dialog, token));

				settings.Add("converted", true);
				Converted = true;

				// scan now?

				if (HashtagProvider.DatabaseExists())
				{
					var scheduler = new HashtagScheduler();
					if (!scheduler.ScheduleExists && scheduler.State == ScanningState.Ready)
					{
						using var scanner = new HashtagScanner();
						(_, _) = await scanner.Scan();
					}
				}
			}

			if (ltdialog.HideQuestion)
			{
				settings.Add("ignore", true);
			}

			if (settings.IsModified)
			{
				provider.SetCollection(settings);
				provider.Save();
			}

			return Converted;
		}


		public async Task<bool> NeedsConversion()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("tagging");
			if (settings.Get("converted", false))
			{
				return false;
			}

			if (settings.Get("ignore", false))
			{
				// previously opted to ignore upgrade
				return false;
			}

			var count = await GetLegacyTagCount();
			return count > 0;
		}


		private async Task<int> GetLegacyTagCount()
		{
			await using var one = new OneNote();

			root = await one.SearchMeta(string.Empty, MetaNames.TaggingLabels);
			if (root == null)
			{
				// may need to restart OneNote?
				return 0;
			}

			ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			var metas = root.Descendants(ns + "Meta")
				.Where(e => e.Attribute("name").Value == MetaNames.TaggingLabels)
				.Select(e => e.Attribute("content").Value);

			var tags = new List<string>();
			foreach (var meta in metas)
			{
				var items = meta
					.Split(
						new string[] { AddIn.Culture.TextInfo.ListSeparator },
						StringSplitOptions.RemoveEmptyEntries)
					.Select(s => s.Trim().ToLower());

				foreach (var item in items)
				{
					if (!tags.Contains(item))
					{
						tags.Add(item);
					}
				}
			}

			return tags.Count;
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

					// clear out the tagging label meta element value
					page.Root.Elements(ns + "Meta")
						.Where(e => e.Attribute("name").Value == MetaNames.TaggingLabels)
						.ForEach(e => e.Attribute("content").Value = string.Empty);

					if (!token.IsCancellationRequested)
					{
						await one.Update(page);
						PagesConverted++;
					}
				}

				dialog.Increment();
			}

			return !token.IsCancellationRequested;
		}
	}
}
