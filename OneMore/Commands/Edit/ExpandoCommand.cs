//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;


	/// <summary>
	/// Expands collapsed elements - heading or paragraphs with collapsed indented children
	/// </summary>
	internal class ExpandoCommand : Command
	{
		public ExpandoCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var expand = (bool)args[0];

			using (var one = new OneNote(out var page, out var ns))
			{
				if (expand)
				{
					var attributes = page.Root.Descendants(ns + "OE")
						.Where(e => e.Attribute("collapsed")?.Value == "1")
						.Select(e => e.Attribute("collapsed"));

					if (attributes.Any())
					{
						attributes.ForEach(a => a.Remove());

						await one.Update(page);
					}
				}
			}
		}
	}
}
