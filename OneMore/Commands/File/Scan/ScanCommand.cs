//************************************************************************************************
// Copyright © 2025 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Models;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class ScanCommand : Command
	{
		public ScanCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var device = ScannerManager.ListScannerDevices()[0];
			using var manager = new ScannerManager(device);

			var data = manager.Scan();

			if (string.IsNullOrWhiteSpace(data))
			{
				return;
			}

			await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Basic);

			var container = page.EnsureContentContainer();
			PageNamespace.Set(ns);

			container.Add(new XElement(ns + "OE",
				new XElement(ns + "Image",
					new XAttribute("format", "png"),
					new XElement(ns + "Size",
						new XAttribute("width", $"{manager.ImageWidth}.0"),
						new XAttribute("height", $"{manager.ImageHeight}.0")),
					new XElement(ns + "Data", data)
				))
			);

			await one.Update(page);
		}
	}
}
