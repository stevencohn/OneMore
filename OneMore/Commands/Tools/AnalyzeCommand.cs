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
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class AnalyzeCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string Header2Shading = "#F2F2F2";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string LineCss = "font-family:'Courier New';font-size:10.0pt";
		private const string Cloud = "<span style='font-family:\"Segoe UI Emoji\"'>\u2601</span>";
		private const string RecycleBin = "OneNote_RecycleBin";

		private bool showNotebookSummary;
		private bool showSectionSummary;
		private AnalysisDetail pageDetail;
		private bool shownPageSummary;
		private int thumbnailSize;

		private OneNote one;
		private XNamespace ns;
		private string backupPath;
		private string defaultPath;
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
				(backupPath, defaultPath, _) = one.GetFolders();

				if (!Directory.Exists(backupPath))
				{
					UIHelper.ShowError(owner, Resx.AnalyzeCommand_NoBackups);
					return;
				}

				using (var dialog = new AnalyzeDialog())
				{
					if (dialog.ShowDialog(owner) != DialogResult.OK)
					{
						return;
					}

					showNotebookSummary = dialog.IncludeNotebookSummary;
					showSectionSummary = dialog.IncludeSectionSummary;
					pageDetail = dialog.Detail;
					thumbnailSize = dialog.ThumbnailSize;
				}

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = one.GetPage(pageId);
				page.Title = Resx.AnalyzeCommand_Title;
				page.SetMeta(Page.AnalysisMetaName, "true");

				ns = page.Namespace;
				heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

				using (progress = new UI.ProgressDialog())
				{
					progress.SetMaximum(5);
					progress.Show(owner);

					var container = page.EnsureContentContainer();
					var notebooks = one.GetNotebooks();

					var prev = false;

					if (showNotebookSummary)
					{
						ReportNotebooks(container, notebooks);
						ReportOrphans(container, notebooks);
						ReportCache(container);
						prev = true;
					}

					if (showSectionSummary)
					{
						if (prev)
						{
							WriteHorizontalLine(page, container);
						}

						ReportSections(container, notebooks);
						prev = true;
					}

					if (pageDetail == AnalysisDetail.Current)
					{
						if (prev)
						{
							WriteHorizontalLine(page, container);
						}

						ReportPages(container, one.GetSection(), null, pageId);
					}
					else if (pageDetail == AnalysisDetail.All)
					{
						if (prev)
						{
							WriteHorizontalLine(page, container);
						}

						ReportAllPages(container, one.GetNotebook(), null, pageId);
					}

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

			var backupUri = new Uri(backupPath).AbsoluteUri;
			var folderUri = new Uri(defaultPath).AbsoluteUri;

			container.Add(
				new Paragraph(ns, "Summary").SetQuickStyle(heading1Index),
				new Paragraph(ns, Resx.AnalyzeCommand_SummarySummary),
				new Paragraph(ns, new ContentList(ns,
					new Bullet(ns, $"<span style='font-style:italic'>Default location</span>: <a href=\"{folderUri}\">{defaultPath}</a>"),
					new Bullet(ns, $"<span style='font-style:italic'>Backup location</span>: <a href=\"{backupUri}\">{backupPath}</a>")
					)),
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
				row = table.AddRow();

				var name = notebook.Attribute("name").Value;
				var remote = notebook.Attribute("path").Value.Contains("https://");

				row[0].SetContent(remote ? $"{name} {Cloud}" : name);

				var path = Path.Combine(remote ? backupPath : defaultPath, name);
				if (Directory.Exists(path))
				{
					var dir = new DirectoryInfo(path);
					var size = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

					var repath = Path.Combine(path, RecycleBin);
					if (Directory.Exists(repath))
					{
						dir = new DirectoryInfo(repath);
						var relength = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

						row[1].SetContent(new Paragraph(ns, (size - relength).ToBytes(1)).SetAlignment("right"));
						row[2].SetContent(new Paragraph(ns, relength.ToBytes(1)).SetAlignment("right"));
						row[3].SetContent(new Paragraph(ns, size.ToBytes(1)).SetAlignment("right"));
					}

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

			var list = new ContentList(ns);

			container.Add(
				new Paragraph(ns,
					new XElement(ns + "T", new XCData(string.Empty)),
					list)
				);

			foreach (var orphan in orphans)
			{
				var dir = new DirectoryInfo(Path.Combine(backupPath, orphan));
				var size = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

				list.Add(new Bullet(ns, $"{orphan} ({size.ToBytes(1)})"));
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
				new Paragraph(ns, Resx.AnalyzeCommand_CacheSummary)
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
				new Paragraph(ns,
					new ContentList(ns,
						new Bullet(ns, $"<span style='font-style:italic'>Cache size</span>: {total.ToBytes(1)}"),
						new Bullet(ns, $"<span style='font-style:italic'>Cache location</span>: <a href=\"{cacheUri}\">{cachePath}</a>")
						)
					),
				new Paragraph(ns, string.Empty)
				);
		}


		private void WriteHorizontalLine(Page page, XElement container)
		{
			if (divider == null)
			{
				divider = string.Empty.PadRight(80, '═');
				page.EnsurePageWidth(divider, "Courier new", 11f, one.WindowHandle);
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
				new Paragraph(ns, "Sections").SetQuickStyle(heading1Index),
				new Paragraph(ns, Resx.AnalyzeCommand_SectionSummary),
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
				.Where(e => e.Attribute("isInRecycleBin") == null
					&& e.Attribute("locked") == null);

			foreach (var section in sections)
			{
				var name = section.Attribute("name").Value;
				progress.SetMessage($"Section {name}");
				progress.Increment();

				var title = folderPath == null ? name : Path.Combine(folderPath, name);
				var subp = folderPath == null ? folderName : Path.Combine(folderPath, folderName);

				var remote = section.Attribute("path").Value.Contains("https://");
				var path = Path.Combine(remote ? backupPath : defaultPath, subp);

				var row = table.AddRow();

				if (Directory.Exists(path))
				{
					row[0].SetContent(new Paragraph(ns, title));

					var filter = remote ? $"{name}.one (On *).one" : $"{name}.one";
					var files = Directory.EnumerateFiles(path, filter).ToList();
					if (files.Count > 0)
					{
						long first = 0;
						long all = 0;
						foreach (var file in files)
						{
							if (File.Exists(file))
							{
								var size = new FileInfo(file).Length;
								if (first == 0) first = size;
								all += size;
							}
						}

						if (all > 0)
						{
							row[1].SetContent(new Paragraph(ns, first.ToBytes(1)).SetAlignment("right"));

							if (remote)
							{
								row[2].SetContent(new Paragraph(ns, files.Count.ToString()).SetAlignment("right"));
							}

							row[3].SetContent(new Paragraph(ns, all.ToBytes(1)).SetAlignment("right"));
							total += all;
						}
					}
					else
					{
						logger.WriteLine($"empty section {name} in {path}");
					}
				}
				else
				{
					row[0].SetContent(new Paragraph(ns, $"{title} <span style='font-style:italic'>(backup not found)</span>"));
				}
			}

			// section groups...

			var groups = folder.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null);

			foreach (var group in groups)
			{
				var path = folderPath == null ? folderName : Path.Combine(folderPath, folderName);
				total += ReportSections(table, group, path);
			}

			return total;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private void ReportAllPages(XElement container, XElement folder, string folderPath, string skipId)
		{
			var sections = folder.Elements(ns + "Section")
				.Where(e => e.Attribute("isInRecycleBin") == null
					&& e.Attribute("locked") == null);

			foreach (var section in sections)
			{
				ReportPages(container, one.GetSection(section.Attribute("ID").Value), folderPath, skipId);
			}

			var groups = folder.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") == null);

			foreach (var group in groups)
			{
				var name = group.Attribute("name").Value;
				folderPath = folderPath == null ? name : Path.Combine(folderPath, name);

				ReportAllPages(container, group, folderPath, skipId);
			}
		}


		private void ReportPages(XElement container, XElement section, string folderPath, string skipId)
		{
			var name = section.Attribute("name").Value;
			var title = folderPath == null ? name : Path.Combine(folderPath, name);

			progress.SetMessage($"{title} pages...");

			container.Add(new Paragraph(ns, $"{title} Section Pages").SetQuickStyle(heading1Index));
			if (!shownPageSummary)
			{
				container.Add(new Paragraph(ns, Resx.AnalyzeCommand_PageSummary));
				shownPageSummary = true;
			}
			container.Add(new Paragraph(ns, string.Empty));

			var table = new Table(ns, 0, 1)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 450);

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

			if (page.GetMetaContent(Page.AnalysisMetaName) == "true")
			{
				// skip a previously generated analysis report
				return;
			}

			progress.SetMessage(page.Title);
			progress.Increment();

			var xml = page.Root.ToString(SaveOptions.DisableFormatting);
			long length = xml.Length;

			var row = table.AddRow();
			row.SetShading(HeaderShading);

			var link = one.GetHyperlink(pageId, string.Empty);
			row[0].SetContent(new Paragraph(ns, 
				$"<a href='{link}'>{page.Title}</a> ({length.ToBytes(1)})").SetStyle(HeaderCss));

			var images = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("xpsFileIndex") == null && e.Attribute("sourceDocument") == null)
				.ToList();

			var files = page.Root.Descendants(ns + "InsertedFile")
				.ToList();

			if (images.Count == 0 && files.Count == 0)
			{
				return;
			}

			var detail = new Table(ns, 1, 3)
			{
				BordersVisible = true,
				HasHeaderRow = true
			};

			detail.SetColumnWidth(0, 250);
			detail.SetColumnWidth(1, 70);
			detail.SetColumnWidth(2, 70);

			row = detail[0];
			row.SetShading(Header2Shading);
			row[0].SetContent(new Paragraph(ns, "Image/File").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph(ns, "XML Size").SetStyle(HeaderCss).SetAlignment("center"));
			row[2].SetContent(new Paragraph(ns, "Native Size").SetStyle(HeaderCss).SetAlignment("center"));

			foreach (var image in images)
			{
				ReportImage(detail, image);
			}

			foreach (var file in files)
			{
				row = detail.AddRow();
				var name = file.Attribute("preferredName").Value;

				var path = file.Attribute("pathSource")?.Value;
				var original = path != null;
				if (!original)
				{
					path = file.Attribute("pathCache")?.Value;
				}

				if (path == null)
				{
					row[0].SetContent(name);
				}
				else
				{
					var uri = new Uri(path).AbsoluteUri;
					var exists = File.Exists(path);

					if (original && exists)
					{
						row[0].SetContent(
							$"<a href='{uri}'>{name}</a>");
					}
					else
					{
						row[0].SetContent(
							$"<a href='{uri}'>{name}</a> <span style='font-style:italic'>(orphaned)</span>");
					}

					if (exists)
					{
						var size = new FileInfo(path).Length;
						row[2].SetContent(new Paragraph(ns, size.ToBytes(1)).SetAlignment("right"));
					}
				}

				var key = "xpsFileIndex";
				var index = file.Element(ns + "Printout")?.Attribute(key)?.Value;
				if (string.IsNullOrEmpty(index))
				{
					key = "sourceDocument";
					index = file.Element(ns + "Previews")?.Attribute(key).Value;
				}

				if (!string.IsNullOrEmpty(index))
				{
					var printouts = page.Root.Descendants(ns + "Image")
						.Where(e => e.Attribute(key)?.Value == index);

					foreach (var printout in printouts)
					{
						ReportImage(detail, printout, true);
					}
				}
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


		private void ReportImage(Table detail, XElement image, bool printout = false)
		{
			var row = detail.AddRow();
			var data = image.Element(ns + "Data").Value;

			var bytes = Convert.FromBase64String(image.Element(ns + "Data").Value);
			using (var stream = new MemoryStream(bytes, 0, bytes.Length))
			{
				using (var raw = Image.FromStream(stream))
				{
					XElement img;
					if (raw.Width > thumbnailSize || raw.Height > thumbnailSize)
					{
						// maintain aspect ratio of image thumbnails
						var zoom = raw.Width - thumbnailSize > raw.Height - thumbnailSize
							? ((float)thumbnailSize) / raw.Width
							: ((float)thumbnailSize) / raw.Height;

						// callback is a required argument but is never used
						var callback = new Image.GetThumbnailImageAbort(() => { return false; });
						using (var thumbnail = 
							raw.GetThumbnailImage(
								(int)(raw.Width * zoom),
								(int)(raw.Height * zoom),
								callback, IntPtr.Zero))
						{
							img = MakeImage(thumbnail);
						}
					}
					else
					{
						img = MakeImage(raw);
					}

					if (printout)
					{
						var print = new Table(ns, 1, 2);
						print[0][0].SetContent("Printout:");
						print[0][1].SetContent(img);
						row[0].SetContent(print);

					}
					else
					{
						row[0].SetContent(img);
					}
				}
			}

			row[1].SetContent(new Paragraph(ns, data.Length.ToBytes(1)).SetAlignment("right"));
			row[2].SetContent(new Paragraph(ns, bytes.Length.ToBytes(1)).SetAlignment("right"));
		}


		private XElement MakeImage(Image image)
		{
			var bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));

			return new XElement(ns + "Image",
				new XAttribute("format", "png"),
				new XElement(ns + "Size",
					new XAttribute("width", $"{image.Width}.0"),
					new XAttribute("height", $"{image.Height}.0")),
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

  <one:InsertedFile selected="all" pathCache="C:\Users\steve\AppData\Local\Microsoft\OneNote\16.0\cache\00000065.bin" pathSource="C:\Users\cohns\SkyDrive\Personal\Life\Color Assignments.xlsx" preferredName="Color Assignments.xlsx">
    <one:Previews sourceDocument="{B35B7C0A-A50F-40FF-9052-EDE826112A46}" displayAll="true">
      <one:Preview page="Sheet1" />
    </one:Previews>
  </one:InsertedFile>
</one:OE>
<one:OE alignment="left">
  <one:Image format="emf" sourceDocument="{B35B7C0A-A50F-40FF-9052-EDE826112A46}">
    <one:Size width="613.5" height="481.5" isSetByUser="true" />
		 
		 */
	}
}
