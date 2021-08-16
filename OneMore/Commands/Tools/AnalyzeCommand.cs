//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class AnalyzeCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string RecycleBin = "OneNote_RecycleBin";

		private OneNote one;
		private XNamespace ns;
		private string backupPath;

		private int heading1Index;
		private int heading2Index;


		public AnalyzeCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote())
			{
				(backupPath, _, _) = one.GetFolders();

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = one.GetPage(pageId);
				page.Title = Resx.AnalyzeCommand_Title;

				ns = page.Namespace;
				heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

				var container = page.EnsureContentContainer();
				//page.EnsurePageWidth("", "Calibri", 11, one.WindowHandle);

				var notebooks = one.GetNotebooks(OneNote.Scope.Pages);

				ReportSummary(container, notebooks);

				container.Add(new Paragraph("Details").SetQuickStyle(heading1Index));

				foreach (var notebook in notebooks.Elements(ns + "Notebook"))
				{
					foreach (var section in notebook.Elements(ns + "Section"))
					{
						ReportSection(container, section, true);
					}
				}

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		public void ReportSummary(XElement container, XElement notebooks)
		{
			var lead = "<span style='font-style:italic'>Backup location</span>";
			var backupUri = new Uri(backupPath).AbsoluteUri;

			container.Add(
				new Paragraph(ns, "Summary").SetQuickStyle(heading1Index),
				new Paragraph(ns, Resx.AnalyzeCommand_SummarySummary),
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, $"{lead}: <a href=\"{backupUri}\">{backupPath}</a>"),
				new Paragraph(ns, string.Empty)
				);

			var table = new Table(ns, 1, 4)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			var row = table[0];
			foreach (var cell in row.Cells)
			{
				cell.ShadingColor = HeaderShading;
			}

			row[0].SetContent(new Paragraph(ns, "Notebook").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(ns, "Backups").SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(ns, "Recycled").SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(ns, "Total").SetStyle(HeaderCss));

			foreach (var notebook in notebooks.Elements(ns + "Notebook"))
			{
				var name = notebook.Attribute("name").Value;

				row = table.AddRow();
				row[0].SetContent(name);

				var path = Path.Combine(backupPath, name);
				if (Directory.Exists(path))
				{
					var dir = new DirectoryInfo(path);
					var total = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

					dir = new DirectoryInfo(Path.Combine(path, RecycleBin));
					var rebin = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

					row[1].SetContent(new Paragraph(ns, (total - rebin).ToBytes()).SetAlignment("right"));
					row[2].SetContent(new Paragraph(ns, rebin.ToBytes()).SetAlignment("right"));
					row[3].SetContent(new Paragraph(ns, total.ToBytes()).SetAlignment("right"));
				}
			}

			container.Add(new Paragraph(ns, table.Root));

			// orphans backup folders...

			var knowns = notebooks.Elements(ns + "Notebook")
				.Select(e => e.Attribute("name").Value)
				.ToList();

			knowns.Add(Resx.AnalyzeCommand_OpenSections);
			knowns.Add(Resx.AnalyzeCommand_QuickNotes);

			var orphans = Directory.GetDirectories(backupPath)
				.Select(d => Path.GetFileNameWithoutExtension(d))
				.Except(knowns);

			if (!orphans.Any())
			{
				return;
			}

			container.Add(
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, "Orphans").SetQuickStyle(heading2Index),
				new Paragraph(ns, Resx.AnalyzeCommand_OrphanSummary)
				);

			var children = new XElement(ns + "OEChildren");

			container.Add(
				new Paragraph(ns,
					new XElement(ns + "T", new XCData(string.Empty)),
					children
				));

			foreach (var orphan in orphans)
			{
				var dir = new DirectoryInfo(Path.Combine(backupPath, orphan));
				var size = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

				children.Add(new XElement(ns + "OE",
					new XElement(ns + "List",
						new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
						new XElement(ns + "T", new XCData($"{orphan} ({size.ToBytes()})"))
					));
			}
		}


		public void ReportSection(XElement container, XElement section, bool deep)
		{
			container.Add(
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, section.Attribute("name").Value).SetQuickStyle(heading2Index)
				);

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

			row[0].SetContent(new Paragraph(ns, "Hierarchy").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(ns, "Size on disk").SetStyle(HeaderCss));
			row[2].SetContent(new Paragraph(ns, "Size in memory").SetStyle(HeaderCss));
			row[3].SetContent(new Paragraph(ns, "# images/files").SetStyle(HeaderCss));
			row[4].SetContent(new Paragraph(ns, "Size of images/files").SetStyle(HeaderCss));

			var path = Path.Combine(backupPath, Path.GetFileName(section.Attribute("path").Value));
			if (File.Exists(path))
			{

			}

			container.Add(new Paragraph(ns, table.Root));
		}
	}
}
