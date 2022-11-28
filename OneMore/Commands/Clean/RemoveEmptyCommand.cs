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
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Collapse multiple consecutive empty lines into a single empty line. Also removes empty
	/// headers, custom and standard.
	/// </summary>
	internal class RemoveEmptyCommand : Command
	{
		private Page page;
		private XNamespace ns;
		private IEnumerable<XElement> range;
		private bool all;


		public RemoveEmptyCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var result = UIHelper.ShowQuestion(Resx.RemoveEmptyCommand_option, false, true);
			if (result == DialogResult.Cancel)
			{
				return;
			}

			all = result == DialogResult.Yes;

			logger.StartClock();
			using (var one = new OneNote())
			{
				page = one.GetPage(OneNote.PageDetail.Selection);
				ns = page.Namespace;
			}

			range = page.GetSelectedElements();
			logger.WriteLine($"found {range.Count()} runs, scope={page.SelectionScope}");

			var modified = OutdentEmptyLines();
			modified = CollapseEmptyLines() || modified;
			modified = IndentEmptyLines() || modified;

			logger.WriteTime("saving", true);

			if (modified)
			{
				using var one = new OneNote();
				await one.Update(page);
				logger.WriteTime("saved");
			}
		}


		private bool OutdentEmptyLines()
		{
			/* Outdent empty indented lines so we can then easily check if there are consecutive
			 * empty lines that need to be collapse. An indended paragraph pattern is:
			 * 
			 * <OEChildren>
			 *   <OE>
			 *     <OEChildren> indented OE...
			 */

			var children = range.Descendants(ns + "OEChildren").ToList();
			if (children.Any())
			{
				return OutdentEmptyLines(children.First().Parent, children);
			}

			return false;
		}


		private bool OutdentEmptyLines(XElement parent, List<XElement> children)
		{
			// recursively find empty indented lines and outdent them

			var modified = false;

			for (var i = 0; i < children.Count; i++)
			{
				var child = children[i];
				if (child.HasElements)
				{
					OutdentEmptyLines(child, child.Elements(ns + "OE").Elements(ns + "OEChildren").ToList());

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


		public bool CollapseEmptyLines()
		{
			// find consecutive empty paragraphs that need to be collapsed...

			var elements = range
				.Select(e => e.Parent)
				.Distinct()
				.Where(e => e.TextValue().Trim().Length == 0)
				.ToList();

			if (elements?.Any() != true)
			{
				//logger.WriteLine("no blank lines found");
				return false;
			}

			//logger.WriteLine($"found {elements.Count} collapsable lines");

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

				if (all)
				{
					element.Remove();
					modified = true;
					continue;
				}

				// is this an empty paragraph preceded by an empty paragraph?
				if (element.PreviousNode is XElement prev && prev.Name.LocalName == "OE")
				{
					// does previous paragraph end with an empty run?
					var t = prev.Elements().Last();
					if (t.Name.LocalName == "T" && t.TextValue().Trim().Length == 0)
					{
						// remove consecutive empty line
						prev.Remove();
						modified = true;
					}
				}
			}

			// clean up left-over empty OEChildrens
			page.Root.Descendants(ns + "OEChildren")
				.Where(e => !e.HasElements)
				.Remove();

			return modified;
		}


		public bool IndentEmptyLines()
		{
			/* Indent outdented empty lines so "section" or related paragraphs can be collapsed
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

			var elements = range
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
