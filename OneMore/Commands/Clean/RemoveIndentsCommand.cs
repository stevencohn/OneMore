//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Removes all indentation from selected content by flattening the OE/OEChildren
	/// hierarchy into a flat sequence of sibling OEs.
	/// </summary>
	internal class RemoveIndentsCommand : Command
	{
		private XNamespace ns;


		public RemoveIndentsCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out var page, out ns);
			if (!page.IsValid)
			{
				return;
			}

			var range = new SelectionRange(page);
			var runs = range.GetSelections();

			if (range.Scope == SelectionScope.TextCursor)
			{
				ShowInfo(Resx.Error_SelectContent);
				return;
			}

			var selectedOEs = runs
				.Select(t => t.Parent)
				.Where(e => e?.Name.LocalName == "OE")
				.Distinct()
				.ToList();

			if (FlattenSelected(selectedOEs))
			{
				await one.Update(page);
			}
		}


		private bool FlattenSelected(List<XElement> selectedOEs)
		{
			if (!selectedOEs.Any())
			{
				return false;
			}

			var processed = new HashSet<XElement>();
			var modified = false;

			foreach (var oe in selectedOEs)
			{
				if (processed.Contains(oe))
				{
					continue;
				}

				processed.Add(oe);

				var oecChildren = oe.Element(ns + "OEChildren");
				if (oecChildren is null)
				{
					continue;
				}

				var descendants = new List<XElement>();
				CollectOEs(oecChildren, descendants);

				foreach (var desc in descendants)
				{
					processed.Add(desc);
					desc.Remove();
				}

				oe.Element(ns + "OEChildren")?.Remove();

				var insertAfter = oe;
				foreach (var desc in descendants)
				{
					desc.Element(ns + "OEChildren")?.Remove();
					insertAfter.AddAfterSelf(desc);
					insertAfter = desc;
				}

				modified = true;
			}

			return modified;
		}


		private void CollectOEs(XElement container, List<XElement> result)
		{
			foreach (var oe in container.Elements(ns + "OE"))
			{
				result.Add(oe);
				var children = oe.Element(ns + "OEChildren");
				if (children is not null)
				{
					CollectOEs(children, result);
				}
			}
		}
	}
}
