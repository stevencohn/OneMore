//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
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

		private class Quickie
		{
			public XElement Element { get; private set; }
			public QuickStyleDef Style { get; private set; }
			public Quickie(XElement element) { Element = element; Style = new QuickStyleDef(element); }
		}


		public MergeCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var section = manager.CurrentSection();
				var ns = section.GetNamespaceOfPrefix("one");

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

				System.Diagnostics.Debugger.Launch();

				var page = manager.GetPage(active.Attribute("ID").Value);

				var quickies = page.Elements(ns + "QuickStyleDef")
					.Select(p => new Quickie(p))
					.ToList();

				var outline = page.Elements(ns + "Outline").LastOrDefault();
				var bounds = GetBounds(ns, outline);


				// merge each of the subsequently selectec pages into the active page

				foreach (var selected in selections.ToList())
				{
					var page2 = manager.GetPage(selected.Attribute("ID").Value);

					var quickies2 = page2.Elements(ns + "QuickStyleDef")
						.Select(p => new Quickie(p))
						.ToList();

					foreach (var outline2 in page2.Elements(ns + "Outline"))
					{
						var bounds2 = GetBounds(ns, outline2);
						var position = outline2.Elements(ns + "Position").FirstOrDefault();
						position.Attribute("y").Value = (bounds2.Top + bounds.Height + 30).ToString("#0.0");
						bounds.Height += bounds2.Height + 30;

						// remove its IDs so the page can apply its own
						outline2.Attributes("objectID").Remove();
						outline2.Descendants().Attributes("objectID").Remove();


						// resolve quick styles
						foreach (var quickie2 in quickies2)
						{
							var quickie = quickies.Find(q => q.Equals(quickie2));
							var same = false;
							if (quickie != null)
							{
								same = quickie.Style.Index == quickie2.Style.Index;
							}
							else
							{
								// O(n) is OK here; there should only be a few
								quickie2.Style.Index = quickies.Max(q => q.Style.Index) + 1;
								quickies.Add(quickie2);
							}

							if (!same)
							{
								var quickStyleIndex = quickie.Style.Index.ToString();

								var elements = outline2.Descendants()
									.Where(e => e.Attribute("quickStyleIndex")?.Value == quickStyleIndex);

								foreach (var element in elements)
								{
									element.Attribute("quickStyleIndex").Value = quickStyleIndex;
								}
							}
						}


						page.Add(outline2);
					}

					selected.Remove();
				}

				//manager.UpdatePageContent(page);
				//manager.UpdateHierarchy(section);
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
	}
}
