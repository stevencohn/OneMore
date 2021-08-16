//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class AnalyzeCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string InfoCss = "font-family:Calibri;font-size:11.0pt";
		private const string DetailCss = "font-family:Calibri;font-size:11.0pt;color:#BFBFBF";

		private OneNote one;
		private XNamespace ns;
		private string backupPath;

		private int heading2Index;


		public AnalyzeCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				(backupPath, _, _) = one.GetFolders();

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = one.GetPage(pageId);
				ns = page.Namespace;

				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

				page.Title = "OneNote Storage Analysis";

				var container = page.EnsureContentContainer();
				//page.EnsurePageWidth("", "Calibri", 11, one.WindowHandle);

				var hierarchy = one.GetNotebook(one.CurrentNotebookId, OneNote.Scope.Pages);
				foreach (var section in hierarchy.Elements(ns + "Section"))
				{
					var contents = ReportSection(section, true);
					foreach (var content in contents)
					{
						container.Add(content);
					}
				}

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		public IEnumerable<XElement> ReportSection(XElement section, bool deep)
		{
			var contents = new List<XElement>
			{
				new XElement(ns + "OE", new XElement(ns + "T", new XCData(string.Empty))),
				MakeSectionHeader(section.Attribute("name").Value)
			};

			var table = new Table(ns, 2, deep ? 5 : 3)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			var row = table[0];
			foreach (var cell in row.Cells)
			{
				cell.ShadingColor = HeaderShading;
			}

			row[0].SetContent(MakeColumnHeader("Hierarchy"));
			row[1].SetContent(MakeColumnHeader("Size on disk"));
			row[2].SetContent(MakeColumnHeader("Size in memory"));
			row[3].SetContent(MakeColumnHeader("# images/files"));
			row[4].SetContent(MakeColumnHeader("Size of images/files"));

			var path = Path.Combine(backupPath, Path.GetFileName(section.Attribute("path").Value));
			if (File.Exists(path))
			{

			}

			contents.Add(new XElement(ns + "OE", table.Root));

			return contents;
		}


		private XElement MakeSectionHeader(string text)
		{
			return new XElement(ns + "OE",
				new XAttribute("quickStyleIndex", heading2Index),
				new XElement(ns + "T", new XCData(text))
				);
		}


		private XElement MakeColumnHeader(string text)
		{
			return new XElement(ns + "OE",
				new XAttribute("style", HeaderCss),
				new XElement(ns + "T", new XCData(text))
				);
		}
	}
}
