//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Colorizer;
	using System.IO;
	using System.Linq;


	internal class ColorizeCommand : Command
	{
		public ColorizeCommand()
		{
		}


		public override void Execute(params object[] args)
		{
			var colorizer = new Colorizer(args[0] as string);

			using (var one = new OneNote(out var page, out var ns))
			{
				var updated = false;

				var runs = page.Root.Descendants(ns + "T")
					.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"));

				if (runs != null)
				{
					foreach (var run in runs)
					{
						var cdata = run.GetCData();
						cdata.Value = colorizer.ColorizeOne(run.GetCData().GetWrapper().Value);
						updated = true;
					}

					if (updated)
					{
						one.Update(page);
					}
				}
			}
		}
	}
}
