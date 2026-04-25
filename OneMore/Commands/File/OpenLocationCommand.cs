//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


	/// <summary>
	/// Open the currently selected section or section group in the file explorer or browser, 
	/// depending on the path type.
	/// </summary>
	internal class OpenLocationCommand : Command
	{
		private const string SectionName = "Section";
		private const string SectionGroupName = "SectionGroup";
		private const string isCurrentlyViewedAtt = "isCurrentlyViewed";


		public OpenLocationCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote();
			var notebook = await one.GetNotebook();
			var ns = one.GetNamespace(notebook);

			var node = notebook.Descendants(ns + SectionName)
				.LastOrDefault(e =>
					e.Attribute(isCurrentlyViewedAtt) is not null &&
					e.Attribute(isCurrentlyViewedAtt).Value == "true");

			if (node is null)
			{
				node = notebook.Descendants(ns + SectionGroupName)
					.LastOrDefault(e =>
						e.Attribute(isCurrentlyViewedAtt) is not null &&
						e.Attribute(isCurrentlyViewedAtt).Value == "true");
			}

			if (node is not null)
			{
				var path = node.Attribute("path")?.Value;
				if (path is not null)
				{
					if (
						// check if well-formed URL
						Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
						uri.Scheme == Uri.UriSchemeHttps)
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = path,
							UseShellExecute = true
						});
						return;
					}

					if (path.EndsWith(".one"))
					{
						// we only need the directory path
						path = Path.GetDirectoryName(path);
					}

					if (
						// check if local file path
						Path.IsPathRooted(path) && Directory.Exists(path))
					{
						Process.Start("explorer.exe", path);
						return;
					}
				}
			}

			UI.MoreMessageBox.ShowWarning(owner, Resx.OpenLocationCommand_NoPath);
		}
	}
}
