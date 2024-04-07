//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class LegacyTaggingConverter
	{
		private XElement root;
		private XNamespace ns;


		public async Task<int> GetLegacyTagCount()
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


		public async Task<bool> UpgradeLegacyTags(ProgressDialog dialog, CancellationToken token)
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

				var bank = page.Root.Descendants(ns + "Meta")
					.FirstOrDefault(e => e.Attribute("name").Value == MetaNames.TaggingBank);

				if (bank is not null &&
					bank.Attribute("content") is XAttribute flag &&
					flag.Value == "1")
				{
					var cdata = bank.ElementsAfterSelf(ns + "OEChildren")
						.Elements(ns + "OE")
						.Elements(ns + "T")
						.Nodes().OfType<XCData>();

					foreach (var datum in cdata)
					{
						var tags = datum.Value.Split(
							new string[] { AddIn.Culture.TextInfo.ListSeparator },
							StringSplitOptions.RemoveEmptyEntries);

						for (var i = 0; i < tags.Length; i++)
						{
							var tag = tags[i].Trim();
							if (tag.Length > 0 && tag[0] != '#')
							{
								tags[i] = $"#{tag}";
							}
						}

						var sep = page.IsRightToLeft()
							? $" {AddIn.Culture.TextInfo.ListSeparator}"
							: $"{AddIn.Culture.TextInfo.ListSeparator} ";

						datum.Value = string.Join(sep, tags);
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
					}
				}

				dialog.Increment();
			}

			return !token.IsCancellationRequested;
		}
	}
}
