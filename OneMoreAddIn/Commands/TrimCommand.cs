//************************************************************************************************
// Copyright © 2019 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	internal class TrimCommand : Command
	{
		public TrimCommand() : base()
		{
		}


		public void Execute()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var selections =
					from e in page.Element(ns + "Outline").Descendants(ns + "T")
					where e.Attributes("selected").Any(a => a.Value.Equals("all"))
					select e;

				if (selections != null)
				{
					System.Diagnostics.Debugger.Launch();

					if (selections.Count() == 1)
					{
						if (selections.First().GetCData().Value.Length == 0)
						{
							// if zero-length selection then select all content
							selections = page.Element(ns + "Outline").Descendants(ns + "T");
						}
					}

					foreach (var selection in selections)
					{
						if ((selection == selection.Parent.LastNode) &&
							(selection.LastNode?.NodeType == XmlNodeType.CDATA))
						{
							var cdata = selection.GetCData();
							if (cdata.Value.Length > 0)
							{
								var wrapper = cdata.GetWrapper();

								var text = wrapper.DescendantNodes().OfType<XText>().LastOrDefault();
								if (text?.Value.Length > 0)
								{
									var match = Regex.Match(text.Value, @"([\s]|&#160;|&nbsp;)*$");
									if (match.Success)
									{
										text.ReplaceWith(text.Value.Substring(0, match.Index));

										selection.FirstNode.ReplaceWith(
											new XCData(wrapper.GetInnerXml()));
									}
								}
							}
						}
					}

					manager.UpdatePageContent(page);
				}
			}
		}
	}
}
