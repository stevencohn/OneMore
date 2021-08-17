//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class AnalyzeCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string LineCss = "font-family:'Courier New';font-size:10.0pt";
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

				var notebooks = one.GetNotebooks();

				ReportNotebooks(container, notebooks);
				ReportOrphans(container, notebooks);
				ReportCache(container);

				var line = string.Empty.PadRight(100, '_');
				page.EnsurePageWidth(line, "Courier new", 10f, one.WindowHandle);

				var lineParagraph = new XElement(ns + "T",
					new XAttribute("style", LineCss),
					new XCData($"{line}<br/>")
					);

				// discover hierarchy bit by bit to avoid loading huge amounts of memory at once
				foreach (var book in notebooks.Elements(ns + "Notebook"))
				{
					container.Add(
						new Paragraph(ns, string.Empty),
						new Paragraph(ns, lineParagraph),
						new Paragraph(ns, $"{book.Attribute("name").Value} Notebook")
							.SetQuickStyle(heading1Index));

					var notebook = one.GetNotebook(book.Attribute("ID").Value);

					foreach (var section in notebook.Elements(ns + "Section"))
					{
						ReportSection(
							container,
							one.GetSection(section.Attribute("ID").Value),
							null, true);
					}

					// section groups...

					foreach (var group in notebook.Elements(ns + "SectionGroup"))
					{
						var groupName = group.Attribute("name").Value;
						foreach (var sec in group.Elements(ns + "Section"))
						{
							ReportSection(container, sec, groupName, true);
						}
					}
				}

				await one.Update(page);
				await one.NavigateTo(pageId);
			}
		}


		public void ReportNotebooks(XElement container, XElement notebooks)
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

			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 70);
			table.SetColumnWidth(3, 70);

			var row = table[0];
			foreach (var cell in row.Cells)
			{
				cell.ShadingColor = HeaderShading;
			}

			row[0].SetContent(new Paragraph(ns, "Notebook").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(ns, "Backups").SetStyle(HeaderCss).SetAlignment("center"));
			row[2].SetContent(new Paragraph(ns, "Recycled").SetStyle(HeaderCss).SetAlignment("center"));
			row[3].SetContent(new Paragraph(ns, "Total").SetStyle(HeaderCss).SetAlignment("center"));

			long total = 0;

			foreach (var notebook in notebooks.Elements(ns + "Notebook"))
			{
				var name = notebook.Attribute("name").Value;

				row = table.AddRow();
				row[0].SetContent(name);

				var path = Path.Combine(backupPath, name);
				if (Directory.Exists(path))
				{
					var dir = new DirectoryInfo(path);
					var size = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

					dir = new DirectoryInfo(Path.Combine(path, RecycleBin));
					var rebin = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

					row[1].SetContent(new Paragraph(ns, (size - rebin).ToBytes(1)).SetAlignment("right"));
					row[2].SetContent(new Paragraph(ns, rebin.ToBytes(1)).SetAlignment("right"));
					row[3].SetContent(new Paragraph(ns, size.ToBytes(1)).SetAlignment("right"));

					total += size;
				}
			}

			if (total > 0)
			{
				row = table.AddRow();
				row[3].SetContent(new Paragraph(ns, total.ToBytes(1)).SetAlignment("right"));
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty)
				);
		}


		public void ReportOrphans(XElement container, XElement notebooks)
		{
			// orphaned backup folders...

			var knowns = notebooks.Elements(ns + "Notebook")
				.Select(e => e.Attribute("name").Value)
				.ToList();

			knowns.Add(Resx.AnalyzeCommand_OpenSections);
			knowns.Add(Resx.AnalyzeCommand_QuickNotes);

			var orphans = Directory.GetDirectories(backupPath)
				.Select(d => Path.GetFileNameWithoutExtension(d))
				.Except(knowns);

			container.Add(
				new Paragraph(ns, "Orphans").SetQuickStyle(heading2Index),
				new Paragraph(ns, Resx.AnalyzeCommand_OrphanSummary)
				);

			if (!orphans.Any())
			{
				container.Add(
					new Paragraph(ns, string.Empty),
					new Paragraph(ns, Resx.AnalyzeCommand_NoOrphans),
					new Paragraph(ns, string.Empty)
					);

				return;
			}

			var children = new XElement(ns + "OEChildren");

			container.Add(
				new Paragraph(ns,
					new XElement(ns + "T", new XCData(string.Empty)),
					children)
				);

			foreach (var orphan in orphans)
			{
				var dir = new DirectoryInfo(Path.Combine(backupPath, orphan));
				var size = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

				children.Add(new XElement(ns + "OE",
					new XElement(ns + "List",
						new XElement(ns + "Bullet", new XAttribute("bullet", "2"))),
						new XElement(ns + "T", new XCData($"{orphan} ({size.ToBytes(1)})"))
					));
			}

			container.Add(new Paragraph(ns, string.Empty));
		}


		public void ReportCache(XElement container)
		{
			// internal cache folder...

			container.Add(
				new Paragraph(ns, "Cache").SetQuickStyle(heading2Index),
				new Paragraph(ns, Resx.AnalyzeCommand_CacheSummary),
				new Paragraph(ns, string.Empty)
				);

			var cachePath = Path.Combine(Path.GetDirectoryName(backupPath), "cache");
			if (!Directory.Exists(cachePath))
			{
				container.Add(
					new Paragraph(ns, string.Empty),
					new Paragraph(ns, Resx.AnalyzeCommand_NoCache),
					new Paragraph(ns, string.Empty)
					);

				return;
			}

			var dir = new DirectoryInfo(cachePath);
			var total = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

			var cacheUri = new Uri(cachePath).AbsoluteUri;

			container.Add(
				new Paragraph(ns, $"Cache: {total.ToBytes(1)}; <a href=\"{cacheUri}\">{cachePath}</a>"),
				new Paragraph(ns, string.Empty)
				);
		}


		public void ReportSection(XElement container, XElement section, string folder, bool deep)
		{
			var name = section.Attribute("name").Value;
			if (folder != null)
			{
				name = Path.Combine(folder, name);
			}

			container.Add(
				new Paragraph(ns, string.Empty),
				new Paragraph(ns, name).SetQuickStyle(heading2Index)
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
				var size = new FileInfo(path).Length;
				row = table.AddRow();
				row[1].SetContent(new Paragraph(ns, size.ToBytes(1)).SetAlignment("right"));
			}

			foreach (var page in section.Elements(ns + "Page"))
			{

			}

			container.Add(new Paragraph(ns, table.Root));
		}
	}
}
