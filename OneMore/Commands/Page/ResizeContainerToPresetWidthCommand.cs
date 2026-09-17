//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Settings;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Snaps the current/selected container(s) to the preset container width - the same
	/// width remembered by Arrange Containers - without repositioning them or opening any
	/// dialog once a preset width has been configured.
	/// </summary>
	internal class ResizeContainerToPresetWidthCommand : Command
	{
		public override async Task Execute(params object[] args)
		{
			using var guard = EnterOnce();
			if (guard is null) { return; }

			await using var one = new OneNote(out var page, out var ns);

			var containers = page.BodyOutlines
				.Where(e => 
					e.Attribute("selected") is XAttribute a && 
					(a.Value == "all" || a.Value == "partial"))
				.ToList();

			if (!containers.Any())
			{
				ShowInfo(Resx.ArrangeContainersCommand_noContainers);
				return;
			}

			var settings = new SettingsProvider().GetCollection("containers");
			var width = settings.Get("width", 0);

			if (width == 0)
			{
				using var dialog = new ArrangeContainersDialog(focusWidth: true);
				if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}

				width = dialog.PageWidth;
				if (width == 0)
				{
					return;
				}
			}

			foreach (var container in containers)
			{
				var size = container.Element(ns + "Size");
				size.SetAttributeValue("isSetByUser", "true");
				size.SetAttributeValue("width", width.ToString());
			}

			await one.Update(page);
		}
	}
}
