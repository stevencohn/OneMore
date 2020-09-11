//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml.Linq;


	internal class RemoveSpacingCommand : Command
	{
		private bool spaceBefore;
		private bool spaceAfter;
		private bool spaceBetween;
		private bool includeHeadings;


		public RemoveSpacingCommand() : base()
		{
		}


		public void Execute()
		{
			using (var dialog = new RemoveSpacingDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					spaceBefore = dialog.SpaceBefore;
					spaceAfter = dialog.SpaceAfter;
					spaceBetween = dialog.SpaceBetween;
					includeHeadings = dialog.IncludeHeadings;

					RemoveSpacing();
				}
			}
		}

		private void RemoveSpacing()
		{
			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage());

				var elements =
					(from e in page.Root.Descendants(page.Namespace + "OE")
					 where e.Elements().Count() == 1
					 let t = e.Elements().First()
					 where (t != null) && (t.Name.LocalName == "T") &&
						 ((e.Attribute("spaceBefore") != null) ||
						 (e.Attribute("spaceAfter") != null) ||
						 (e.Attribute("spaceBetween") != null))
					 select e)
					.ToList();

				if (elements != null)
				{
					var quickStyles = page.GetQuickStyles()
						.Where(s => s.StyleType == StyleType.Heading);

					var customStyles = new StyleProvider().GetStyles()
						.Where(e => e.StyleType == StyleType.Heading)
						.ToList();

					var modified = false;

					foreach (var element in elements)
					{
						// is this a known Heading style?
						var attr = element.Attribute("quickStyleIndex");
						if (attr != null)
						{
							var index = int.Parse(attr.Value);
							if (quickStyles.Any(s => s.Index == index))
							{
								if (includeHeadings)
								{
									modified |= CleanElement(element);
								}

								continue;
							}
						}

						// is this a custom Heading style?
						var style = new Style(element.CollectStyleProperties(true));
						if (customStyles.Any(s => s.Equals(style)))
						{
							if (includeHeadings)
							{
								modified |= CleanElement(element);
							}

							continue;
						}

						// normal paragraph
						modified |= CleanElement(element);
					}

					if (modified)
					{
						manager.UpdatePageContent(page.Root);
					}
				}
			}
		}

		private bool CleanElement(XElement element)
		{
			XAttribute attribute;
			bool modified = false;

			if (spaceBefore)
			{
				attribute = element.Attribute("spaceBefore");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			if (spaceAfter)
			{
				attribute = element.Attribute("spaceAfter");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			if (spaceBetween)
			{
				attribute = element.Attribute("spaceBetween");
				if (attribute != null)
				{
					attribute.Remove();
					modified = true;
				}
			}

			return modified;
		}
	}
}
