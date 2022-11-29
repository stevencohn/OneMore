//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	/// <summary>
	/// Restores the auto-sizing behavior of manually resized cotnainers on the page
	/// </summary>
	internal class RestoreAutosizeCommand : Command
	{
		private readonly int MaxWidth = 600;


		public RestoreAutosizeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out var page, out var ns))
			{
				var sizes = page.Root.Descendants(ns + "Outline")
					.Elements(ns + "Size")
					.Where(e =>
						e.Attribute("isSetByUser") != null &&
						e.Attribute("isSetByUser").Value == "true");

				if (sizes != null)
				{
					var modified = false;

					foreach (var size in sizes)
					{
						size.SetAttributeValue("isSetByUser", "false");

						// must modify both width and height in order for this to take effect

						size.SetAttributeValue("width", $"{MaxWidth}.0");

						size.GetAttributeValue("height", out float height);
						size.SetAttributeValue("height", (height + 1).ToString("F04"));

						modified = true;
					}

					if (modified)
					{
						await one.Update(page);
					}
				}
			}

			await Task.Yield();
		}
	}
}
