//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Linq;
	using System.Windows.Forms;
	using System.Xml;
	using System.Xml.Linq;


	internal class AddTitleIconCommand : Command
	{
		public AddTitleIconCommand() : base()
		{
		}


		public void Execute()
		{
			string[] codes = null;

			using (var dialog = new EmojiDialog())
			{
				var result = dialog.ShowDialog(owner);

				if (result == DialogResult.OK)
				{
					codes = dialog.GetSelectedCodes();
				}
			}

			if (codes == null || codes.Length == 0)
			{
				return;
			}

			Page page = null;
			using (var manager = new ApplicationManager())
			{
				page = new Page(manager.CurrentPage());
			}

			var title = page.Root.Element(page.Namespace + "Title")?
				.Elements(page.Namespace + "OE")?.Elements(page.Namespace + "T")
				.FirstOrDefault();

			if (title != null)
			{
				if (title.FirstNode?.NodeType == XmlNodeType.CDATA)
				{
					var wrapper = XElement.Parse("<x>" + title.FirstNode.Parent.Value + "</x>");
					var wns = wrapper.GetDefaultNamespace();

					var emojii =
						(from e in wrapper.Elements(wns + "span")
						 where e.Attributes("style").Any(a => a.Value.Contains("Segoe UI Emoji"))
						 select e).FirstOrDefault();

					if (emojii != null)
					{
						emojii.Value = string.Join(string.Empty, codes) + emojii.Value;
					}
					else
					{
						wrapper.AddFirst(new XElement("span",
							new XAttribute("style", "font-family:'Segoe UI Emoji';font-size:16pt;"),
							string.Join(string.Empty, codes)
							));
					}

					var decoded = string.Concat(wrapper.Nodes()
						.Select(x => x.ToString()).ToArray())
						.Replace("&amp;", "&");

					title.FirstNode.ReplaceWith(new XCData(decoded));

					using (var manager = new ApplicationManager())
					{
						manager.UpdatePageContent(page.Root);
					}
				}
			}
		}
	}
}
