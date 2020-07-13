//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class MergeCommand : Command
	{

		private struct Bounds
		{
			public double Top;
			public double Height;
		}

		private class QuickRef
		{
			public XElement Element { get; private set; }
			public QuickStyleDef Style { get; private set; }
			public QuickRef(XElement element) { Element = element; Style = new QuickStyleDef(element); }
		}

		private XElement page;
		private XNamespace ns;
		private List<QuickRef> quickies;


		public MergeCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				ns = section.GetNamespaceOfPrefix("one");

				// find first selected - active page

				var active =
					section.Elements(ns + "Page")
					.Where(e => e.Attributes("isCurrentlyViewed").Any(a => a.Value.Equals("true")))
					.FirstOrDefault();

				if (active == null)
				{
					MessageBox.Show(manager.Window, "At least two pages must be selected to merge");
					return;
				}

				var selections =
					section.Elements(ns + "Page")
					.Where(e =>
						e.Attributes("isCurrentlyViewed").Count() == 0 &&
						e.Attributes("selected").Any(a => a.Value.Equals("all")));

				if (active == null)
				{
					MessageBox.Show(manager.Window, "At least two pages must be selected to merge");
					return;
				}


				// get first selected (active) page, reference its quick styles, outline, size

				//System.Diagnostics.Debugger.Launch();

				page = manager.GetPage(active.Attribute("ID").Value);

				quickies = page.Elements(ns + "QuickStyleDef")
					.Select(p => new QuickRef(p))
					.ToList();

				var outline = page.Elements(ns + "Outline").LastOrDefault();
				var bounds = GetBounds(ns, outline);


				// merge each of the subsequently selected pages into the active page

				foreach (var selected in selections.ToList())
				{
					var childPage = manager.GetPage(selected.Attribute("ID").Value);

					foreach (var childOutline in childPage.Elements(ns + "Outline"))
					{
						var childBounds = GetBounds(ns, childOutline);
						var position = childOutline.Elements(ns + "Position").FirstOrDefault();
						position.Attribute("y").Value = (childBounds.Top + bounds.Height + 30).ToString("#0.0");
						bounds.Height += childBounds.Height + 30;

						// remove its IDs so the page can apply its own
						childOutline.Attributes("objectID").Remove();
						childOutline.Descendants().Attributes("objectID").Remove();

						ResolveQuickStyles(childPage, childOutline);

						page.Add(childOutline);
					}

					// remove selected (child) page from the section
					selected.Remove();
				}

				manager.UpdatePageContent(page);
				manager.UpdateHierarchy(section);
			}
		}


		private Bounds GetBounds(XNamespace ns, XElement outline)
		{
			var size = outline.Elements(ns + "Size").FirstOrDefault();
			if (size != null)
			{
				var position = outline.Elements(ns + "Position").FirstOrDefault();
				if (position != null)
				{
					return new Bounds
					{
						Top = (int)Math.Ceiling(double.Parse(position.Attribute("y").Value)),
						Height = (int)Math.Ceiling(double.Parse(size.Attribute("height").Value))
					};
				}
			}

			return new Bounds { Top = 0, Height = 0 };
		}


		private void ResolveQuickStyles(XElement childPage, XElement childOutline)
		{
			// resolve quick styles
			var childQuickies = childPage.Elements(ns + "QuickStyleDef")
				.Where(p => p.Attribute("name")?.Value != "PageTitle")
				.Select(p => new QuickRef(p))
				.ToList();

			foreach (var childQuickie in childQuickies)
			{
				var staleIndex = childQuickie.Style.Index.ToString();

				var quickie = quickies.Find(q => q.Style.Equals(childQuickie.Style));
				var same = false;
				if (quickie != null)
				{
					// found match but is it the same index?
					same = quickie.Style.Index == childQuickie.Style.Index;
				}
				else
				{
					// no match so add it and set index to maxIndex+1
					// O(n) is OK here; there should only be a few
					childQuickie.Style.Index = quickies.Max(q => q.Style.Index) + 1;
					childQuickie.Element.Attribute("index").Value = childQuickie.Style.Index.ToString();
					quickies.Add(childQuickie);

					var last = page.Elements(ns + "QuickStyleDef").LastOrDefault();
					if (last != null)
					{
						last.AddAfterSelf(childQuickie.Element);
					}
					else
					{
						page.AddFirst(childQuickie.Element);
					}
				}

				if (!same)
				{
					// apply new index to child outline elements

					var quickStyleIndex = childQuickie.Style.Index.ToString();

					var elements = childOutline.Descendants()
						.Where(e => e.Attribute("quickStyleIndex")?.Value == staleIndex);

					foreach (var element in elements)
					{
						element.Attribute("quickStyleIndex").Value = quickStyleIndex;
					}
				}
			}
		}
	}
}
