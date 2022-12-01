//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Prefix the page title with one or more chosen icons
	/// </summary>
	internal class AddTitleIconCommand : Command
	{
		private string[] codes;


		public AddTitleIconCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			// check for replay
			var element = args?.FirstOrDefault(a => a is XElement e && e.Name.LocalName == "codes") as XElement;
			if (!string.IsNullOrEmpty(element?.Value))
			{
				codes = element.Value.Split(',');
			}

			if (codes == null)
			{
				using var dialog = new AddTitleIconDialog();
				if (dialog.ShowDialog() == DialogResult.Cancel)
				{
					IsCancelled = true;
					return;
				}

				codes = dialog.GetSelectedCodes();
			}

			await AddIcons(codes);
		}


		private async Task AddIcons(string[] codes)
		{
			using var one = new OneNote(out var page, out var ns);

			var cdata = page.Root
				.Elements(page.Namespace + "Title")
				.Elements(page.Namespace + "OE")	// should have exactly one OE
				.Elements(page.Namespace + "T")		// but may have one or more Ts
				.DescendantNodes().OfType<XCData>()
				.FirstOrDefault();

			if (cdata != null)
			{
				var wrapper = cdata.GetWrapper();

				var espan = wrapper.Elements("span")
					.FirstOrDefault(e =>
						e.Attributes("style").Any(a => a.Value.Contains("Segoe UI Emoji")));

				if (espan != null)
				{
					espan.Value = string.Join(string.Empty, codes) + espan.Value;
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

				cdata.ReplaceWith(new XCData(decoded));

				await one.Update(page);
			}
		}


		public override XElement GetReplayArguments()
		{
			if (codes != null && codes.Length > 0)
			{
				return new XElement("codes", string.Join(",", codes));
			}

			return null;
		}
	}
}
