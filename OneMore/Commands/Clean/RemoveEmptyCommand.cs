//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Xml.Linq;


	internal class RemoveEmptyCommand : Command
	{
		public RemoveEmptyCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var elements =
					(from e in page.Root.Descendants(page.Namespace + "OE")
					 where e.Elements().Count() == 1
					 let t = e.Elements().First()
					 where (t != null) && (t.Name.LocalName == "T") && (t.Value.Length == 0)
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
								element.Remove();
								modified = true;
								continue;
							}
						}

						// is this a custom Heading style?
						var style = new Style(element.CollectStyleProperties(true));
						if (customStyles.Any(s => s.Equals(style)))
						{
							element.Remove();
							modified = true;
							continue;
						}

						// is this an empty paragraph preceded by an empty paragraph?
						if (element.PreviousNode != null &&
							element.PreviousNode.NodeType == System.Xml.XmlNodeType.Element)
						{
							var prev = element.PreviousNode as XElement;

							if (prev.Name.LocalName == "OE" &&
								prev.Elements().Count() == 1)
							{
								var t = prev.Elements().First();
								if (t.Name.LocalName == "T" && t.Value.Length == 0)
								{
									element.Remove();
									modified = true;
								}
							}
						}
					}

					if (modified)
					{
						one.Update(page);
					}
				}
			}
		}
	}
}
