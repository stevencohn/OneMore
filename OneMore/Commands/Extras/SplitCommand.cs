//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

#pragma warning disable S125 // Sections of code should not be commented out

namespace River.OneMoreAddIn.Commands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class SplitCommand : Command
	{
		public SplitCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			using (var dialog = new SplitDialog())
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
				{
					SplitPage(dialog.SplitByHeading, dialog.Tagged ? dialog.TagSymbol : -1);
				}
			}
		}


		private void SplitPage(bool byHeading, int tagSymbol)
		{

			System.Diagnostics.Debugger.Launch();

			using (var one = new OneNote(out var page, out var ns))
			{
				List<XElement> headers;

				if (byHeading)
				{
					headers = page.GetHeadings(one)
						.Where(h => h.Level == 0)
						.Select(h => h.Root)
						.ToList();
				}
				else
				{
					// CDATA: <a href="onenote:#three&amp;
					//	section-id={A640CEA0-536E-4ED0-ACC1-428AAB96501F}&amp;
					//	page-id={C9709FD9-6044-4A82-BB2E-E829884B364A}&amp;
					//	end&amp;base-path=https://d..."

					headers = page.Root.Descendants(ns + "T")
						.Where(e =>
							e.FirstNode.NodeType == XmlNodeType.CDATA &&
							((XCData)e.FirstNode).Value.StartsWith("<a href=\"onenote:#") &&
							e.Parent.Elements().Count() == 1)
						.Select(e => e.Parent)
						.ToList();
				}

				if (tagSymbol >= 0)
				{
					// find the index of the tagdef of tagSymbol
					var tagIndex = page.Root.Elements(ns + "TagDef")
						.Where(e => e.Attribute("symbol").Value == tagSymbol.ToString())
						.Select(e => e.Attribute("index").Value).FirstOrDefault();

					// filter tagged breaks
					headers = headers.Where(e =>
						e.Elements(ns + "Tag").Any(t => t.Attribute("index").Value == tagIndex))
						.ToList();
				}

				//for (int i = headers.Count - 1; i >= 0; i--)
				//{
				//	var header = headers[i];
				//	logger.WriteLine($"break [{header.Value}]");

				//	var cdata = header.GetCData();
				//	if (!cdata.Value.StartsWith())

				//	var content = header.ElementsAfterSelf();
				//}
			}
		}
	}
}
