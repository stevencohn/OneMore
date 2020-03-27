//************************************************************************************************
// Copyright © 2016 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;


	internal class InsertLineCommand : Command
	{
		private const int LineCharCount = 100;


		public InsertLineCommand () : base()
		{
		}


		public bool IsBodyContext ()
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var found = page
					.Elements(ns + "Outline")?
					.Descendants(ns + "T")?
					.Attributes("selected")?
					.Any(a => a.Value.Equals("all"));

				ribbon.Invalidate();

				return found ?? true;
			}
		}


		public void Execute (char c)
		{
			try
			{
				InsertLine(c);
			}
			catch (Exception exc)
			{
				logger.WriteLine($"Error executing {nameof(InsertLineCommand)}", exc);
			}
		}


		private void InsertLine (char c)
		{
			using (var manager = new ApplicationManager())
			{
				var page = manager.CurrentPage();
				var ns = page.GetNamespaceOfPrefix("one");

				var current =
					(from e in page.Descendants(ns + "OE")
					 where e.Elements(ns + "T").Attributes("selected").Any(a => a.Value.Equals("all"))
					 select e).FirstOrDefault();

				if (current != null)
				{
					string line = string.Empty.PadRight(LineCharCount, c);

					EnsurePageWidth(page, line, (IntPtr)manager.Application.Windows.CurrentWindow.WindowHandle);

					current.AddAfterSelf(
						new XElement(ns + "OE",
							new XElement(ns + "T",
								new XAttribute("style", "font-family:'Courier New';font-size:10.0pt"),
								new XCData(line + "<br/>")
							)
						));
				}

				manager.UpdatePageContent(page);
			}
		}


		private void EnsurePageWidth (XElement page, string line, IntPtr handle)
		{
			// detect page width

			var ns = page.GetNamespaceOfPrefix("one");

			var element = page.Element(ns + "Outline")?.Element(ns + "Size");
			if (element != null)
			{
				var attr = element.Attribute("width");
				if (attr != null)
				{
					var outlineWidth = double.Parse(attr.Value);

					// measure line to ensure page width is sufficient

					using (var g = Graphics.FromHwnd(handle))
					{
						using (var font = new Font("Courier New", 10f))
						{
							var stringSize = g.MeasureString(line, font);
							var stringPoints = stringSize.Width * 72 / g.DpiX;

							if (stringPoints > outlineWidth)
							{
								attr.Value = stringPoints.ToString("#0.00");

								// must include isSetByUser or width doesn't take effect!
								if (element.Attribute("isSetByUser") == null)
								{
									element.Add(new XAttribute("isSetByUser", "true"));
								}
							}
						}
					}
				}
			}
		}
	}
}
