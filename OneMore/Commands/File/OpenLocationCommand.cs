//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	internal class OpenLocationCommand : Command
	{
		public OpenLocationCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var notebook = await one.GetNotebook();
			var ns = one.GetNamespace(notebook);

			var node = notebook.Descendants(ns + "Section")
				.LastOrDefault(e =>
					e.Attribute("isCurrentlyViewed") is not null &&
					e.Attribute("isCurrentlyViewed").Value == "true");

			if (node is null)
			{
				node = notebook.Descendants(ns + "SectionGroup")
					.LastOrDefault(e =>
						e.Attribute("isCurrentlyViewed") is not null &&
						e.Attribute("isCurrentlyViewed").Value == "true");
			}

			if (node is null)
			{
				UI.MoreMessageBox.ShowWarning(owner, Resx.OpenLocationCommand_NoPath);
				return;
			}

			var path = node.Attribute("path")?.Value;
			if (path is null)
			{
				UI.MoreMessageBox.ShowWarning(owner, Resx.OpenLocationCommand_NoPath);
				return;
			}

			if (path.EndsWith(".one"))
			{
				path = Path.GetDirectoryName(path);
			}

			if (!Path.IsPathRooted(path) || !Directory.Exists(path))
			{
				UI.MoreMessageBox.ShowWarning(owner, Resx.OpenLocationCommand_NoPath);
				return;
			}

			System.Diagnostics.Process.Start("explorer.exe", path);
		}
	}
}
