//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
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


	/// <summary>
	/// Collapse multiple consecutive empty lines into a single empty line. Also removes empty
	/// headers, custom and standard.
	/// </summary>
	internal class RemoveEmptyCommand : Command
	{
		public RemoveEmptyCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic))
			{
				logger.StartClock();

				var modified = OutdentEmptyLines(page, ns);
				modified = CollapseEmptyLines(page, ns) || modified;
				modified = IndentEmptyLines(page, ns) || modified;

				logger.WriteTime("removed empty lines, now saving...");

				if (modified)
				{
					await one.Update(page);
				}
			}
		}


		/*
		 * Outdent empty indented lines so we can then easily check if there are consecutive
		 * empty lines that need to be collapse. An indended paragraph pattern is:
		 * 
		 * <OEChildren>
		 *   <OE>
		 *     <OEChildren> indented </o>
		 *   </OE>
		 * </>
		 * 
 		 */

		private bool OutdentEmptyLines(Page page, XNamespace ns)
		{
			var children = page.Root
				.Elements(ns + "Outline").Elements(ns + "OEChildren")
				.Where(e => !e.Parent.Elements(ns + "Meta")
					.Any(m => m.Attribute("name").Value.Equals(MetaNames.TaggingBank)))
				.ToList();

			if (children.Any())
			{
				return OutdentEmptyLines(children.First().Parent, children, ns);
			}

			return false;
		}


		private bool OutdentEmptyLines(XElement parent, List<XElement> children, XNamespace ns)
		{
			// recursively find empty indented lines and outdent them

			var modified = false;

			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				if (child.HasElements)
				{
					OutdentEmptyLines(child, child.Elements(ns + "OE").Elements(ns + "OEChildren").ToList(), ns);

					if (child.TextValue().Trim() == string.Empty)
					{
						// move contents of OEChildren to the containing OE
						// and remove the OEChildren element

						var kids = child.Elements();
						child.Remove();
						parent.Add(kids);
						modified = true;
					}
				}
			}

			return modified;
		}


		/*
		 * Find consecutive empty lines that need to be collapsed...
		 */

		public bool CollapseEmptyLines(Page page, XNamespace ns)
		{
			var elements =
				(from e in page.Root.Descendants(ns + "OE")
				 let t = e.Elements().FirstOrDefault()
				 where (t?.Name.LocalName == "T") && (t.TextValue().Trim().Length == 0)
				 select e)
				.ToList();

			if (elements?.Any() != true)
			{
				return false;
			}

			var modified = false;

			var quickStyles = page.GetQuickStyles()
				.Where(s => s.StyleType == StyleType.Heading);

			var customStyles = new ThemeProvider().Theme.GetStyles()
				.Where(e => e.StyleType == StyleType.Heading)
				.ToList();

			foreach (var element in elements)
			{
				// is this a known Heading style?
				var attr = element.Attribute("quickStyleIndex");
				if (attr != null)
				{
					var index = int.Parse(attr.Value, CultureInfo.InvariantCulture);
					if (quickStyles.Any(s => s.Index == index))
					{
						// remove empty standard heading
						element.Remove();
						modified = true;
						continue;
					}
				}

				// is this a custom Heading style?
				var style = new Style(element.CollectStyleProperties(true));
				if (customStyles.Any(s => s.Equals(style)))
				{
					// remove empty custom heading
					element.Remove();
					modified = true;
					continue;
				}

				// is this an empty paragraph preceded by an empty paragraph?
				if (element.PreviousNode != null &&
					element.PreviousNode.NodeType == System.Xml.XmlNodeType.Element)
				{
					var prev = element.PreviousNode as XElement;

					if (prev.Name.LocalName == "OE")
					{
						var t = prev.Elements().Last();
						if (t.Name.LocalName == "T" && t.TextValue().Trim().Length == 0)
						{
							// remove consecutive empty line
							prev.Remove();
							modified = true;
						}
					}
				}
			}

			return modified;
		}


		/*
		 * Indent outdented empty lines so "section" or related paragraphs can be collapsed
		 * together under a shared heading. The pattern is as follows, where the empty T is
		 * left outdented from para2 but should be indented to the same level:
		 * 
		 * <OE>
		 *   <OEChildren>...</>
		 * </OE>
		 * <OE>
		 *   <T> -empty-and-outdented- </T>
		 *   <OEChildren>
		 *     para2
		 *   </OEChildren>
		 * </OE>
		 * 
		 * This needs to be "flattened" to:
		 * 
		 * <OE>
		 *   <OEChildren>...</>
		 *   <OEChildren><OE><T> -empty- </T></OE></>
		 *   <OEChildren> para2 </>
		 * </OE>
		 */

		public bool IndentEmptyLines(Page page, XNamespace ns)
		{
			var elements = page.Root.Descendants(ns + "OE").Elements(ns + "T")
				.Where(e => e.PreviousNode == null && e.TextValue().Trim().Length == 0)
				.ToList();

			if (elements?.Any() != true)
			{
				return false;
			}

			var modified = false;

			foreach (var element in elements)
			{
				var next = element.NextNode as XElement;
				if (next?.Name.LocalName == "OEChildren")
				{
					var prev = element?.Parent.PreviousNode as XElement;
					if (prev?.Name.LocalName == "OE")
					{
						// move empty line to its own paragraph
						prev.Add(new XElement(ns + "OEChildren",
							new XElement(ns + "OE", element)
							));

						// move remaining siblings to previous container
						prev.Add(element.NodesAfterSelf());

						// remove the offending paragraph
						element.Parent.Remove();

						modified = true;
					}
				}
			}

			return modified;
		}
	}
}
