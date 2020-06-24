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

				var quickStyles = page.Elements(ns + "QuickStyleDef")
					.Select(p => new Style(new QuickStyleDef(p)))
					.ToList();

				var outline = page.Elements(ns + "Outline").LastOrDefault();
				var bounds = GetBounds(ns, outline);


				// merge each of the subsequently selectec pages into the active page

				foreach (var selected in selections.ToList())
				{
					var page2 = manager.GetPage(selected.Attribute("ID").Value);

					var quickStyles2 = page2.Elements(ns + "QuickStyleDef")
						.Select(p => new Style(new QuickStyleDef(p)))
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

						page.Add(outline2);
					}

					selected.Remove();
				}


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
