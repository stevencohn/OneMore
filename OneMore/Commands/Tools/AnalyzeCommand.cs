//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System;
	using System.Drawing;
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
		private string divider;

		private int heading1Index;
		private int heading2Index;

		private UI.ProgressDialog progress;


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

				using (progress = new UI.ProgressDialog())
				{
					progress.SetMaximum(10);
					progress.Show(owner);

					var container = page.EnsureContentContainer();
					var notebooks = one.GetNotebooks();

					ReportNotebooks(container, notebooks);
					ReportOrphans(container, notebooks);
					ReportCache(container);

					WriteHorizontalLine(page, container);
					ReportSections(container, notebooks);

					WriteHorizontalLine(page, container);
					ReportPages(container, one.GetSection(), pageId);

					progress.SetMessage("Updating report...");
					await one.Update(page);
				}

				await one.NavigateTo(pageId);
			}
		}


		private void ReportNotebooks(XElement container, XElement notebooks)
		{
			progress.SetMessage("Notebooks...");
			progress.Increment();

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

			table.SetColumnWidth(0, 120);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 70);
			table.SetColumnWidth(3, 70);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(ns, "Notebook").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(ns, "Backups").SetStyle(HeaderCss).SetAlignment("center"));
			row[2].SetContent(new Paragraph(ns, "RecycleBin").SetStyle(HeaderCss).SetAlignment("center"));
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


		private void ReportOrphans(XElement container, XElement notebooks)
		{
			// orphaned backup folders...

			progress.SetMessage("Orphans...");
			progress.Increment();

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


		private void ReportCache(XElement container)
		{
			// internal cache folder...

			progress.SetMessage("Cache...");
			progress.Increment();

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


		private void WriteHorizontalLine(Page page, XElement container)
		{
			if (divider == null)
			{
				divider = string.Empty.PadRight(100, '_');
				page.EnsurePageWidth(divider, "Courier new", 10f, one.WindowHandle);
			}

			container.Add(new Paragraph(ns, new XElement(ns + "T",
				new XAttribute("style", LineCss),
				new XCData($"{divider}<br/>")
				)));
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ReportSections(XElement container, XElement notebooks)
		{
			progress.SetMessage("Sections...");
			progress.Increment();

			container.Add(
				new Paragraph(ns, "Summary by Section").SetQuickStyle(heading1Index),
				new Paragraph(ns, string.Empty)
				);

			// discover hierarchy bit by bit to avoid loading huge amounts of memory at once
			foreach (var book in notebooks.Elements(ns + "Notebook"))
			{
				container.Add(
					new Paragraph(ns, $"{book.Attribute("name").Value} Notebook")
						.SetQuickStyle(heading2Index));

				var table = new Table(ns, 1, 4)
				{
					HasHeaderRow = true,
					BordersVisible = true
				};

				table.SetColumnWidth(0, 200);
				table.SetColumnWidth(1, 70);
				table.SetColumnWidth(2, 70);
				table.SetColumnWidth(3, 70);

				var row = table[0];
				row.SetShading(HeaderShading);
				row[0].SetContent(new Paragraph(ns, "Section").SetStyle(HeaderCss));
				row[1].SetContent(new Paragraph(ns, "Size on Disk").SetStyle(HeaderCss).SetAlignment("center"));
				row[2].SetContent(new Paragraph(ns, "# of Copies").SetStyle(HeaderCss).SetAlignment("center"));
				row[3].SetContent(new Paragraph(ns, "Total Size").SetStyle(HeaderCss).SetAlignment("center"));

				var notebook = one.GetNotebook(book.Attribute("ID").Value);

				var total = ReportSections(table, notebook, null);

				row = table.AddRow();
				row[3].SetContent(new Paragraph(ns, total.ToBytes(1)).SetAlignment("right"));

				container.Add(
					new Paragraph(ns, table.Root),
					new Paragraph(ns, string.Empty)
					);
			}
		}

		private long ReportSections(Table table, XElement folder, string folderPath)
		{
			long total = 0;
			var folderName = folder.Attribute("name").Value;

			var sections = folder.Elements(ns + "Section")
				.Where(e => e.Attribute("isRecycleBin") == null
					&& e.Attribute("isInRecycleBin") == null);

			foreach (var section in sections)
			{
				var name = section.Attribute("name").Value;
				progress.SetMessage($"Section {name}");
				progress.Increment();

				var title = folderPath == null ? name : Path.Combine(folderPath, name);
				var subp = folderPath == null ? folderName : Path.Combine(folderPath, folderName);
				var path = Path.Combine(backupPath, subp);

				var files = Directory.EnumerateFiles(path, $"{name}.one (On *).one").ToList();

				var row = table.AddRow();
				row[0].SetContent(new Paragraph(ns, title));

				if (files.Count > 0)
				{
					long first = 0;
					long all = 0;
					foreach (var file in files)
					{
						var size = new FileInfo(file).Length;
						if (first == 0) first = size;
						all += size;
					}

					row[1].SetContent(new Paragraph(ns, first.ToBytes(1)).SetAlignment("right"));
					row[2].SetContent(new Paragraph(ns, files.Count.ToString()).SetAlignment("right"));
					row[3].SetContent(new Paragraph(ns, all.ToBytes(1)).SetAlignment("right"));
					total += all;
				}
			}

			// section groups...

			var groups = folder.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null
					&& e.Attribute("isInRecycleBin") == null);

			foreach (var group in groups)
			{
				var path = folderPath == null ? folderName : Path.Combine(folderPath, folderName);
				total += ReportSections(table, group, path);
			}

			return total;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ReportPages(XElement container, XElement section, string skipId)
		{
			var name = section.Attribute("name").Value;

			progress.SetMessage($"{name} pages...");

			container.Add(
				new Paragraph(ns, $"{name} Section").SetQuickStyle(heading1Index),
				new Paragraph(ns, string.Empty)
				);

			var table = new Table(ns, 0, 1)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 550);

			var pages = section.Elements(ns + "Page")
				.Where(e => e.Attribute("ID").Value != skipId);

			progress.SetMaximum(pages.Count());

			foreach (var page in pages)
			{
				ReportPage(table, page.Attribute("ID").Value);
			}

			container.Add(
				new Paragraph(ns, table.Root),
				new Paragraph(ns, string.Empty)
				);
		}


		private void ReportPage(Table table, string pageId)
		{
			var page = one.GetPage(pageId, OneNote.PageDetail.All);
			progress.SetMessage(page.Title);
			progress.Increment();

			var xml = page.Root.ToString(SaveOptions.DisableFormatting);
			long length = xml.Length;

			var row = table.AddRow();
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph(ns, 
				$"<span style='font-weight:bold'>{page.Title}</span> ({length.ToBytes(1)})").SetStyle(HeaderCss));

			var images = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("xpsFileIndex") == null)
				.ToList();

			var files = page.Root.Descendants(ns + "InsertedFile")
				.ToList();

			if (images.Count == 0 && files.Count == 0)
			{
				return;
			}

			var detail = new Table(ns, 0, 3)
			{
				BordersVisible = true,
				HasHeaderRow = true
			};

			detail.SetColumnWidth(0, 320);
			detail.SetColumnWidth(1, 70);
			detail.SetColumnWidth(2, 70);

			var callback = new Image.GetThumbnailImageAbort(() => { return false; });

			foreach (var image in images)
			{
				row = detail.AddRow();
				var data = image.Element(ns + "Data").Value;

				var bytes = Convert.FromBase64String(image.Element(ns + "Data").Value);
				using (var stream = new MemoryStream(bytes, 0, bytes.Length))
				{
					using (var raw = Image.FromStream(stream))
					{
						if (raw.Width > 20 || raw.Height > 20)
						{
							using (var thumbnail = raw.GetThumbnailImage(20, 20, callback, IntPtr.Zero))
							{
								row[0].SetContent(new Paragraph(ns, MakeImage(thumbnail)));
							}
						}
						else
						{
							row[0].SetContent(new Paragraph(ns, MakeImage(raw)));
						}
					}
				}

				row[1].SetContent(new Paragraph(ns, data.Length.ToBytes(1)).SetAlignment("right"));
				row[2].SetContent(new Paragraph(ns, bytes.Length.ToBytes(1)).SetAlignment("right"));
			}

			foreach (var file in files)
			{
				row = detail.AddRow();
				var path = file.Attribute("pathCache").Value;

				row[0].SetContent(file.Attribute("preferredName").Value);
			}

			row = table.AddRow();
			row[0].SetContent(new XElement(ns + "OEChildren",
				new XElement(ns + "OE",
					new XElement(ns + "T", new XCData(string.Empty)),
					new XElement(ns + "OEChildren",
						new XElement(ns + "OE", detail.Root)
						)),
				new Paragraph(ns, string.Empty)
				));
		}

		/*
             <one:OEChildren>
                <one:OE alignment="left" quickStyleIndex="2">
                  <one:T><![CDATA[]]></one:T>
                  <one:OEChildren>
                    <one:OE alignment="left">
                      <one:Table bordersVisible="true" hasHeaderRow="true">
                        <one:Columns>
                          <one:Column index="0" width="307.6285095214844" isLocked="true" />
                          <one:Column index="1" width="66.10999298095703" isLocked="true" />
                          <one:Column index="2" width="72.60997772216797" isLocked="true" />
                        </one:Columns>
                        <one:Row>
                          <one:Cell shadingColor="#F2F2F2">
                            <one:OEChildren>
                              <one:OE alignment="left" quickStyleIndex
		*/


		private XElement MakeImage(Image image)
		{
			var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));

			return new XElement(ns + "Image",
				new XAttribute("format", "png"),
				new XElement(ns + "Size",
					new XAttribute("width", "20.0"),
					new XAttribute("height", "20.0")),
				new XElement(ns + "Data", Convert.ToBase64String(bytes))
				);
		}


		/*
<one:Image format="png">
    <one:Size width="140.0" height="38.0" />
    <one:Data>iVBORw0KGgoAAAANSUhEUgAAAIwAAAAmCAYAAAAWR3O2AAAAAXNSR0IArs4c6QAAAARnQU1BAACx


  <one:InsertedFile pathCache="C:\Users\steve\AppData\Local\Temp\{569D95EA-50FE-4C04-BBD9-351BB06B49B0}.bin" pathSource="C:\Users\steve\Downloads\Report_Cards_FHS.pdf" preferredName="Report_Cards_FHS.pdf" lastModifiedTime="2021-08-17T19:28:44.000Z" objectID="{7CA7CD12-44EE-4197-9E96-283E7CD967C9}{109}{B0}">
    <one:Position x="36.0" y="237.9003143310547" z="2" />
    <one:Size width="338.2500305175781" height="64.80000305175781" />
    <one:Printout xpsFileIndex="0" />
  </one:InsertedFile>
  <one:Image format="png" xpsFileIndex="0" originalPageNumber="0" isPrintOut="true" lastModifiedTime="2021-08-17T19:28:45.000Z" objectID="{7CA7CD12-44EE-4197-9E96-283E7CD967C9}{116}{B0}">
    <one:Position x="36.0" y="313.5003051757812" z="0" />
    <one:Size width="613.4400024414062" height="793.4400024414062" isSetByUser="true" />
    <one:Data>iVBORw0KGgoAAAANSUhEUgAAAzAAAAQgCAIAAAC2Gy5ZAAAAAXNSR0IArs4c6QAAAARnQU1BAACx
		 */
	}
}
