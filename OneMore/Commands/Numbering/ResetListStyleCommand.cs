//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Removes stray styling from the numbers or bullets of list items and applies the
	/// default font and size of the page's Normal paragraph style. Acts on the entire list
	/// at the cursor, or only on the selected list items when there is a selection range.
	/// </summary>
	internal class ResetListStyleCommand : Command
	{
		private XNamespace ns;


		public ResetListStyleCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var targets = GetTargets(page);

			var items = targets.Where(e => ListMarker.Find(e) is not null).ToList();
			if (!items.Any())
			{
				ShowError(Resx.ResetListStyleCommand_NoList);
				return;
			}

			var normal = page.GetQuickStyle(StandardStyles.Normal);

			var font = string.IsNullOrWhiteSpace(normal.FontFamily)
				? StyleBase.DefaultFontFamily
				: normal.FontFamily;

			var size = string.IsNullOrWhiteSpace(normal.FontSize)
				? StyleBase.DefaultFontSize.ToString("0.0", CultureInfo.InvariantCulture)
				: normal.FontSize;

			var changed = false;
			foreach (var item in items)
			{
				if (ListMarker.Reset(item, font, size))
				{
					changed = true;
				}
			}

			if (changed)
			{
				await one.Update(page);
			}
		}


		/// <summary>
		/// Gets the OEs to reset. With an empty selection this is every item, including
		/// nested items, of the list containing the cursor; otherwise it is the selected
		/// items and their nested items. Non-list OEs may be included; callers filter them.
		/// </summary>
		private List<XElement> GetTargets(Page page)
		{
			var range = new SelectionRange(page);

			// Scope is not set until the selections have been read
			var selections = range.GetSelections().ToList();

			if (range.Scope is SelectionScope.TextCursor or SelectionScope.None)
			{
				var cursor = selections.FirstOrDefault();

				// a list item looks like OE/List,T so its container is the list's OEChildren
				if (cursor?.Parent is XElement oe &&
					oe.FirstNode is XElement first &&
					first.Name.LocalName == "List" &&
					oe.Parent is XElement container)
				{
					return container.Descendants(ns + "OE").ToList();
				}

				return new List<XElement>();
			}

			return selections
				.Select(t => t.Parent)
				.Where(e => e?.Name.LocalName == "OE")
				.SelectMany(e => e.DescendantsAndSelf(ns + "OE"))
				.Distinct()
				.ToList();
		}
	}
}
