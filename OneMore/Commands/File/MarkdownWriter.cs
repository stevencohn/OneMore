//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	internal class MarkdownWriter
	{
		private readonly Page page;
		private readonly List<Style> quicks;
		private readonly Stack<int> qindexes;
		private StreamWriter writer;
		private readonly ILogger logger = Logger.Current;


		public MarkdownWriter(Page page)
		{
			this.page = page;
			quicks = page.GetQuickStyles();
			qindexes = new Stack<int>();
		}


		public void Save(string filename)
		{
			//using (writer = File.CreateText(filename))
			{
				// awful presumptuous here!
				logger.WriteLine($"# {page.Title}");

				page.Root.Descendants(page.Namespace + "OEChildren")
					.Elements()
					.ForEach(e => Write(e));

				logger.WriteLine();
			}
		}


		private void Write(XElement element, string prefix = "", bool newpara = false)
		{
			switch (element.Name.LocalName)
			{
				case "OEChildren":
					DetectQuickStyle(element);
					prefix = $"  {prefix}";
					break;

				case "OE":
					DetectQuickStyle(element);
					logger.Write(prefix);
					newpara = true;
					break;

				case "T":
					DetectQuickStyle(element);
					if (newpara) Setup();
					var text = Cleanup(element.GetCData());
					logger.Write(text);
					newpara = false;
					break;

				case "Bullet":
					logger.Write($"{prefix}- ");
					break;

				case "Number":
					logger.Write($"{prefix}1. ");
					break;
			}

			if (element.HasElements)
			{
				foreach (var child in element.Elements())
				{
					Write(child, prefix, newpara);
				}

				if (element.Name.LocalName == "OE")
				{
					logger.WriteLine();
				}
			}
		}

		private void DetectQuickStyle(XElement element)
		{
			if (element.GetAttributeValue("quickStyleIndex", out int index))
			{
				qindexes.Push(index);
			}
		}

		private void Setup()
		{
			var index = qindexes.Pop();
			var quick = quicks.First(q => q.Index == index);
			switch (quick.Name)
			{
				case "PageTitle": logger.Write("# "); break;
				case "h1": logger.Write("# "); break;
				case "h2": logger.Write("## "); break;
				case "h3": logger.Write("### "); break;
				case "h4": logger.Write("#### "); break;
				case "h5": logger.Write("##### "); break;
				case "h6": logger.Write("###### "); break;
				case "cite": logger.Write("*"); break;
				case "code": logger.Write("`"); break;
				case "p": logger.Write(System.Environment.NewLine); break;
			}
		}

		private string Cleanup(XCData cdata)
		{
			cdata.Value = cdata.Value
				.Replace("\nstyle", " style")
				.Replace("\nhref", " href")
				.Replace(";\nfont-size:", ";font-size:")
				.Replace(";\ncolor:", ";color:")
				.Replace(":\n", ": ");

			var wrapper = cdata.GetWrapper();
			foreach (var span in wrapper.Elements("span"))
			{
				var sat = span.Attribute("style");
				if (sat != null)
				{
					var style = new Style(sat.Value);
					if (style.IsBold || style.IsItalic)
					{
						var stars = style.IsBold && style.IsItalic
							? "***"
							: (style.IsBold ? "**" : "*");

						span.AddBeforeSelf(new XText(stars));
						span.AddAfterSelf(new XText(stars));

						style.IsBold = false;
						style.IsItalic = false;
						sat.Value = style.ToCss();
					}
				}
			}

			return wrapper.GetInnerXml();
		}
	}
}
