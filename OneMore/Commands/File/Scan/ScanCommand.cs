//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class ScanCommand : Command
	{
		private static bool commandIsActive;


		public ScanCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive)
			{
				return;
			}

			commandIsActive = true;

			int imageWidth, imageHeight;
			string imageData;

			try
			{
				var infos = ScannerManager.ListScannerDevices();

				if (!infos.Any())
				{
					MoreMessageBox.Show(owner, Resx.ScanDialog_noScanners,
						MessageBoxButtons.OK, MessageBoxIcon.Information);

					return;
				}

				try
				{
					using var dialog = new ScanDialog(infos);
					var result = dialog.ShowDialog(owner);

					if (result != DialogResult.OK)
					{
						return;
					}

					imageData = dialog.ImageData;
					imageWidth = dialog.ImageWidth;
					imageHeight = dialog.ImageHeight;
				}
				finally
				{
					foreach (var info in infos)
					{
						Marshal.ReleaseComObject(info);
					}
				}
			}
			finally
			{
				commandIsActive = false;
			}

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic);

			var container = page.EnsureContentContainer();
			PageNamespace.Set(ns);

			container.Add(new XElement(ns + "OE",
				new XElement(ns + "Image",
					new XAttribute("format", "png"),
					new XElement(ns + "Size",
						new XAttribute("width", $"{imageWidth}.0"),
						new XAttribute("height", $"{imageHeight}.0")),
					new XElement(ns + "Data", imageData)
				))
			);

			await one.Update(page);
		}
	}
}
