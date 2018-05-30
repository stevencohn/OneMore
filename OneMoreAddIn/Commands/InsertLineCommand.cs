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
				_Execute(c);
			}
			catch (Exception exc)
			{
				logger.WriteLine("Error executing InsertLineCommand", exc);
			}
		}

		private void _Execute (char c)
		{
			//System.Diagnostics.Debugger.Launch();

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
					string line = string.Empty.PadRight(90, c);

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

			var sizeElement =
				(from e in page.Descendants(ns + "T")
				 let a = e.Ancestors(ns + "Outline").Elements(ns + "Size").FirstOrDefault()
				 where e.Attributes("selected").Any(a => a.Value.Equals("all"))
				 select a).FirstOrDefault();

			if (sizeElement != null)
			{
				var widthAttribute = sizeElement.Attribute("width");
				if (widthAttribute != null)
				{
					var width = double.Parse(widthAttribute.Value);

					// measure line to ensure page width is sufficient

					using (var g = Graphics.FromHwnd(handle))
					{
						using (var font = new Font("Courier New", 10f))
						{
							var size = g.MeasureString(line, font);
							if (size.Width > width)
							{
								var points = size.Width * 72 / g.DpiX;
								widthAttribute.Value = (points).ToString();

								// must include isSetByUser or width doesn't take effect!
								if (sizeElement.Attribute("isSetByUser") == null)
								{
									sizeElement.Add(new XAttribute("isSetByUser", "true"));
								}
							}
						}
					}
				}
			}
		} // EnsurePageWidth

	}
}
