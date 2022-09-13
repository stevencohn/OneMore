//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class InsertLineCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertLineCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var c = (char)args[0];

			using (var one = new OneNote(out var page, out var ns))
			{
				if (!page.ConfirmBodyContext())
				{
					UIHelper.ShowError(Resx.Error_BodyContext);
					return;
				}

				var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
				var color = dark ? "#D0D0D0" : "#202020";
				var length = LineCharCount;

				var settings = new SettingsProvider().GetCollection("LinesSheet");
				if (settings != null)
				{
					color = settings.Get<Color>("color", ColorTranslator.FromHtml(color)).ToRGBHtml();
					length = (int)settings.Get<decimal>("length", length);
				}

				var current =
					(from e in page.Root.Descendants(ns + "OE")
					 where e.Elements(ns + "T").Attributes("selected").Any(a => a.Value.Equals("all"))
					 select e).FirstOrDefault();

				string line = string.Empty.PadRight(length, c);

				page.EnsurePageWidth(line, "Courier New", 10f, one.WindowHandle);

				current.AddAfterSelf(
					new XElement(ns + "OE",
						new XElement(ns + "T",
							new XAttribute("style", $"font-family:'Courier New';font-size:10.0pt;color:{color}"),
							new XCData(line + "<br/>")
						)
					));

				await one.Update(page);
			}
		}


		/// <summary>
		/// Used by MergeCommand to separate content of merged pages
		/// </summary>
		/// <param name="one"></param>
		/// <param name="page"></param>
		/// <param name="c"></param>
		public static void AppendLine(OneNote one, Page page, char c)
		{
			var dark = page.GetPageColor(out _, out _).GetBrightness() < 0.5;
			var color = dark ? "#D0D0D0" : "#202020";

			string line = string.Empty.PadRight(LineCharCount, c);

			page.EnsurePageWidth(line, "Courier New", 10f, one.WindowHandle);

			var ns = page.Namespace;
			var outline = page.Root.Elements(ns + "Outline").LastOrDefault();

			if (outline != null)
			{
				var oe = new XElement(ns + "OE",
					new XElement(ns + "T",
						new XAttribute("style", $"font-family:'Courier New';font-size:10.0pt;color:{color}"),
						new XCData(line + "<br/>")
					));

				var oec = outline.Elements(ns + "OEChildren").LastOrDefault();
				if (oec == null)
				{
					outline.Add(new XElement(ns + "OEChildren", oe));
				}
				else
				{
					oec.Add(oe);
				}
			}
		}
	}
}
