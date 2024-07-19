//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions

namespace River.OneMoreAddIn.Commands
{
	using OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Generate a report, as a new page in this current section, showing how much disk space
	/// is consumed by OneNote notebooks and their recycle bins, sections, and pages.
	/// </summary>
	internal class AnalyzeCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string Header2Shading = "#F2F2F2";
		private const string HeaderCss = "font-family:'Segoe UI Light';font-size:10.0pt";
		private const string LineCss = "font-family:'Courier New';font-size:10.0pt";
		private const string CloudSym = "<span style='font-family:\"Segoe UI Emoji\"'>\u2601</span>";
		private const string FloppySym = "<span style='font-family:\"Segoe UI Emoji\"'>&#128427</span>";
		private const string OfflineSym = "<span style='font-family:\"Symbol\"'>\u00C6</span>";
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

		private sealed class Book
		{
			public bool Online;
			public string Name;
			public string Path;
		}


		public AnalyzeCommand()
		{
			// prevent replay
			IsCancelled = true;
		}


		public override async Task Execute(params object[] args)
		{
			await using (one = new OneNote())
			{
				(backupPath, defaultPath, _) = one.GetFolders();

				if (!Directory.Exists(backupPath))
				{
					ShowError(Resx.AnalyzeCommand_NoBackups);
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

				logger.StartClock();

				one.CreatePage(one.CurrentSectionId, out var pageId);
				var page = await one.GetPage(pageId);
				page.Title = Resx.AnalyzeCommand_Title;
				page.SetMeta(MetaNames.AnalysisReport, "true");
				page.Root.SetAttributeValue("lang", "yo");

				ns = page.Namespace;
				PageNamespace.Set(ns);

				heading1Index = page.GetQuickStyle(Styles.StandardStyles.Heading1).Index;
				heading2Index = page.GetQuickStyle(Styles.StandardStyles.Heading2).Index;

				using (progress = new UI.ProgressDialog())
				{
					progress.SetMaximum(5);
					progress.Show();

					try
					{
						var container = page.EnsureContentContainer();
						var notebooks = await one.GetNotebooks();

						var prev = false;

						if (showNotebookSummary)
						{
							ReportNotebooks(container, notebooks);
							ReportCache(container);
							prev = true;
						}

						if (showSectionSummary)
						{
							if (prev)
							{
								WriteHorizontalLine(page, container);
							}

							await ReportSections(container, notebooks);
							prev = true;
						}

						if (pageDetail == AnalysisDetail.Current)
						{
							if (prev)
							{
								WriteHorizontalLine(page, container);
							}

							await ReportPages(container, await one.GetSection(), null, pageId);
						}
						else if (pageDetail == AnalysisDetail.All)
						{
							if (prev)
							{
								WriteHorizontalLine(page, container);
							}

							await ReportAllPages(container, await one.GetNotebook(), null, pageId);
						}

						progress.SetMessage("Updating report...");
						await one.Update(page);
					}
					catch (Exception exc)
					{
						logger.WriteLine("error analyzing storage", exc);
					}
				}

				logger.WriteTime("analysis completed", true);

				await one.NavigateTo(pageId);

				logger.WriteTime("analysis report completed");
			}
		}


		private void ReportNotebooks(XElement container, XElement notebooks)
		{
			progress.SetMessage("Notebooks...");
			progress.Increment();

			var backupUri = new Uri(backupPath).AbsoluteUri;
			var folderUri = new Uri(defaultPath).AbsoluteUri;

			container.Add(
				new Paragraph("Summary").SetQuickStyle(heading1Index),
				new Paragraph(Resx.AnalyzeCommand_SummarySummary),
				new Paragraph(new ContentList(ns,
					new Bullet($"<span style='font-style:italic'>Default location</span>: <a href=\"{folderUri}\">{defaultPath}</a>"),
					new Bullet($"<span style='font-style:italic'>Backup location</span>: <a href=\"{backupUri}\">{backupPath}</a>")
					)),
				new Paragraph(string.Empty)
				);

			var table = new Table(ns, 1, 4)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 250);
			table.SetColumnWidth(1, 70);
			table.SetColumnWidth(2, 70);
			table.SetColumnWidth(3, 70);

			var row = table[0];
			row.SetShading(HeaderShading);
			row[0].SetContent(new Paragraph("Notebook").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph("Backups").SetStyle(HeaderCss).SetAlignment("center"));
			row[2].SetContent(new Paragraph("RecycleBin").SetStyle(HeaderCss).SetAlignment("center"));
			row[3].SetContent(new Paragraph("Total").SetStyle(HeaderCss).SetAlignment("center"));

			long total = 0;

			var books = new SortedDictionary<string, Book>();
			foreach (var notebook in notebooks.Elements(ns + "Notebook"))
			{
				var name = notebook.Attribute("name").Value;
				books.Add(name, new Book
				{
					Online = true,
					Name = name,
					Path = notebook.Attribute("path").Value
				});
			}

			var orphans = Directory.GetDirectories(backupPath)
				.Select(d => new Book
				{
					Online = false,
					Name = Path.GetFileNameWithoutExtension(d),
					Path = d
				})
				.Where(b =>
					b.Name != Resx.AnalyzeCommand_OpenSections &&
					b.Name != Resx.AnalyzeCommand_QuickNotes &&
					!books.ContainsKey(b.Name));

			orphans.ForEach(b => books.Add(b.Name, b));

			foreach (var book in books.Values)
			{
				row = table.AddRow();

				var name = book.Name;

				var remote = book.Path.Contains("https://") ||
					!Directory.Exists(book.Path);
				//!Directory.Exists(Path.Combine(defaultPath, name));

				row[0].SetContent(
					book.Online
					? $"{name} {(remote ? CloudSym : FloppySym)}"
					: $"<span style=\"font-style:italic;color:#747474\">{name}</span> {OfflineSym}"
					);

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

						row[1].Value = size - relength;
						row[2].Value = relength;
						row[3].Value = size;

						row[1].SetContent(new Paragraph(row[1].LongValue.ToBytes(1)).SetAlignment("right"));
						row[2].SetContent(new Paragraph(relength.ToBytes(1)).SetAlignment("right"));
						row[3].SetContent(new Paragraph(size.ToBytes(1)).SetAlignment("right"));
					}
					else
					{
						row[1].SetContent(new Paragraph(size.ToBytes(1)).SetAlignment("right"));
						row[3].SetContent(new Paragraph(size.ToBytes(1)).SetAlignment("right"));
					}

					total += size;
				}
			}

			if (total > 0)
			{
				// add total row
				row = table.AddRow();
				row[3].SetContent(new Paragraph(total.ToBytes(1)).SetAlignment("right"));

				// heatmap
				var values1 = new List<decimal>();
				var values2 = new List<decimal>();
				var values3 = new List<decimal>();
				table.Rows.ForEach(r =>
				{
					if (r[1].Value > 0) values1.Add(r[1].Value);
					if (r[2].Value > 0) values2.Add(r[2].Value);
					if (r[3].Value > 0) values3.Add(r[3].Value);
				});

				if (values1.Any() && values2.Any() && values3.Any())
				{
					var map1 = new Heatmap(values1);
					var map2 = new Heatmap(values2);
					var map3 = new Heatmap(values3);
					table.Rows.ForEach(r =>
					{
						if (r[1].Value > 0)
							r[1].ShadingColor = $"#{map1.MapToRGB(r[1].Value):x6}";

						if (r[2].Value > 0)
							r[2].ShadingColor = $"#{map2.MapToRGB(r[2].Value):x6}";

						if (r[3].Value > 0)
							r[3].ShadingColor = $"#{map3.MapToRGB(r[3].Value):x6}";
					});
				}
			}

			container.Add(
				new Paragraph(table.Root),
				new Paragraph(string.Empty)
				);
		}


		private void ReportCache(XElement container)
		{
			// internal cache folder...

			progress.SetMessage("Cache...");
			progress.Increment();

			container.Add(
				new Paragraph("Cache").SetQuickStyle(heading2Index),
				new Paragraph(Resx.AnalyzeCommand_CacheSummary)
				);

			var cachePath = Path.Combine(Path.GetDirectoryName(backupPath), "cache");
			if (!Directory.Exists(cachePath))
			{
				container.Add(
					new Paragraph(string.Empty),
					new Paragraph(Resx.AnalyzeCommand_NoCache),
					new Paragraph(string.Empty)
					);

				return;
			}

			var dir = new DirectoryInfo(cachePath);
			var total = dir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(f => f.Length);

			var cacheUri = new Uri(cachePath).AbsoluteUri;

			container.Add(
				new Paragraph(ns,
					new ContentList(ns,
						new Bullet($"<span style='font-style:italic'>Cache size</span>: {total.ToBytes(1)}"),
						new Bullet($"<span style='font-style:italic'>Cache location</span>: <a href=\"{cacheUri}\">{cachePath}</a>")
						)
					),
				new Paragraph(string.Empty)
				);
		}


		private void WriteHorizontalLine(Page page, XElement container)
		{
			if (divider is null)
			{
				divider = string.Empty.PadRight(80, '═');
				page.EnsurePageWidth(divider, "Courier new", 11f, one.WindowHandle);
			}

			container.Add(new Paragraph(new XElement(ns + "T",
				new XAttribute("style", LineCss),
				new XCData($"{divider}<br/>")
				)));
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async Task ReportSections(XElement container, XElement notebooks)
		{
			progress.SetMessage("Sections...");
			progress.Increment();

			container.Add(
				new Paragraph("Sections").SetQuickStyle(heading1Index),
				new Paragraph(Resx.AnalyzeCommand_SectionSummary),
				new Paragraph(string.Empty)
				);

			// discover hierarchy bit by bit to avoid loading huge amounts of memory at once
			foreach (var book in notebooks.Elements(ns + "Notebook"))
			{
				container.Add(
					new Paragraph($"{book.Attribute("name").Value} Notebook")
						.SetQuickStyle(heading2Index));

				var table = new Table(ns, 1, 5)
				{
					HasHeaderRow = true,
					BordersVisible = true
				};

				table.SetColumnWidth(0, 250);
				table.SetColumnWidth(1, 70);
				table.SetColumnWidth(2, 70);
				table.SetColumnWidth(3, 70);
				table.SetColumnWidth(4, 70);

				var row = table[0];
				row.SetShading(HeaderShading);
				row[0].SetContent(new Paragraph("Section").SetStyle(HeaderCss));
				row[1].SetContent(new Paragraph("# of Pages").SetStyle(HeaderCss).SetAlignment("center"));
				row[2].SetContent(new Paragraph("Size on Disk").SetStyle(HeaderCss).SetAlignment("center"));
				row[3].SetContent(new Paragraph("# of Copies").SetStyle(HeaderCss).SetAlignment("center"));
				row[4].SetContent(new Paragraph("Total Size").SetStyle(HeaderCss).SetAlignment("center"));

				var notebook = await one.GetNotebook(book.Attribute("ID").Value);

				var (totalPages, totalSize) = await ReportSections(table, notebook, null);

				row = table.AddRow();
				row[1].SetContent(new Paragraph(totalPages.ToString()).SetAlignment("right"));
				row[4].SetContent(new Paragraph(totalSize.ToBytes(1)).SetAlignment("right"));

				// heatmap
				var values1 = new List<decimal>();
				var values3 = new List<decimal>();
				table.Rows.ForEach(r =>
				{
					if (r[2].Value > 0) values1.Add(r[2].Value);
					if (r[4].Value > 0) values3.Add(r[2].Value);
				});

				if (values1.Any() && values3.Any())
				{
					var map1 = new Heatmap(values1);
					var map3 = new Heatmap(values3);
					table.Rows.ForEach(r =>
					{
						if (r[2].Value > 0)
							r[2].ShadingColor = $"#{map1.MapToRGB(r[2].Value):x6}";

						if (r[4].Value > 0)
							r[4].ShadingColor = $"#{map3.MapToRGB(r[4].Value):x6}";
					});
				}

				container.Add(
					new Paragraph(table.Root),
					new Paragraph(string.Empty)
					);
			}
		}

		private async Task<(int, long)> ReportSections(Table table, XElement folder, string folderPath)
		{
			int totalPages = 0;
			long totalSize = 0;
			var folderName = folder.Attribute("name").Value;

			var sections = folder.Elements(ns + "Section")
				.Where(e => e.Attribute("isInRecycleBin") is null
					&& e.Attribute("locked") is null);

			foreach (var section in sections)
			{
				var name = section.Attribute("name").Value;
				progress.SetMessage($"Section {name}");
				progress.Increment();

				var title = folderPath is null ? name : Path.Combine(folderPath, name);
				var subp = folderPath is null ? folderName : Path.Combine(folderPath, folderName);

				var path = section.Attribute("path").Value;
				var remote = path.Contains("https://");

				string filePath;
				if (remote)
				{
					filePath = Path.Combine(backupPath, subp);
				}
				else
				{
					filePath = Path.Combine(defaultPath, subp);
					if (!Directory.Exists(filePath))
					{
						filePath = Path.GetDirectoryName(path);
					}
				}

				var row = table.AddRow();

				if (Directory.Exists(filePath))
				{
					row[0].SetContent(new Paragraph(title));

					var expanded = await one.GetSection(section.Attribute("ID").Value);
					var count = expanded.Elements().Count();
					totalPages += count;
					row[1].Value = count;
					row[1].SetContent(new Paragraph(count.ToString()).SetAlignment("right"));

					var filter = remote ? $"{name}.one (On *).one" : $"{name}.one";
					var files = Directory.EnumerateFiles(filePath, filter).ToList();
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
							row[2].Value = first;
							row[2].SetContent(new Paragraph(first.ToBytes(1)).SetAlignment("right"));

							if (remote)
							{
								row[3].SetContent(new Paragraph(files.Count.ToString()).SetAlignment("right"));
							}

							row[4].Value = all;
							row[4].SetContent(new Paragraph(all.ToBytes(1)).SetAlignment("right"));
							totalSize += all;
						}
					}
					else
					{
						logger.WriteLine($"empty section {name} in {filePath}");
					}
				}
				else
				{
					row[0].SetContent(new Paragraph($"{title} <span style='font-style:italic'>(backup not found)</span>"));
				}
			}

			// section groups...

			var groups = folder.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") is null);

			foreach (var group in groups)
			{
				var path = folderPath is null ? folderName : Path.Combine(folderPath, folderName);
				var (p, s) = await ReportSections(table, group, path);
				totalPages += p;
				totalSize += s;
			}

			return (totalPages, totalSize);
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

		private async Task ReportAllPages(
			XElement container, XElement folder, string folderPath, string skipId)
		{
			var sections = folder.Elements(ns + "Section")
				.Where(e => e.Attribute("isInRecycleBin") is null
					&& e.Attribute("locked") is null);

			foreach (var section in sections)
			{
				await ReportPages(
					container, await one.GetSection(section.Attribute("ID").Value), folderPath, skipId);
			}

			var groups = folder.Elements(ns + "SectionGroup")
				.Where(e => e.Attribute("isRecycleBin") is null);

			foreach (var group in groups)
			{
				var name = group.Attribute("name").Value;
				folderPath = folderPath is null ? name : Path.Combine(folderPath, name);

				await ReportAllPages(container, group, folderPath, skipId);
			}
		}


		private async Task ReportPages(
			XElement container, XElement section, string folderPath, string skipId)
		{
			var name = section.Attribute("name").Value;
			var title = folderPath is null ? name : Path.Combine(folderPath, name);

			progress.SetMessage($"{title} pages...");

			container.Add(new Paragraph($"{title} Section Pages").SetQuickStyle(heading1Index));
			if (!shownPageSummary)
			{
				container.Add(new Paragraph(Resx.AnalyzeCommand_PageSummary));
				shownPageSummary = true;
			}
			container.Add(new Paragraph(string.Empty));

			var table = new Table(ns, 0, 1)
			{
				HasHeaderRow = true,
				BordersVisible = true
			};

			table.SetColumnWidth(0, 500);

			var pages = section.Elements(ns + "Page")
				.Where(e => e.Attribute("ID").Value != skipId);

			progress.SetMaximum(pages.Count());

			if (pages.Any())
			{
				foreach (var page in pages)
				{
					await ReportPage(table, page.Attribute("ID").Value);
				}
			}
			else
			{
				var row = table.AddRow();
				row[0].SetContent(new Paragraph(Resx.word_Empty).SetStyle("font-style:italic"));
			}

			container.Add(
				new Paragraph(table.Root),
				new Paragraph(string.Empty)
				);
		}


		private async Task ReportPage(Table table, string pageId)
		{
			var page = await one.GetPage(pageId, OneNote.PageDetail.All);

			if (page.GetMetaContent(MetaNames.AnalysisReport) == "true")
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
			row[0].SetContent(new Paragraph(
				$"<a href='{link}'>{page.Title}</a> ({length.ToBytes(1)})").SetStyle(HeaderCss));

			var images = page.Root.Descendants(ns + "Image")
				.Where(e => e.Attribute("xpsFileIndex") is null && e.Attribute("sourceDocument") is null)
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

			detail.SetColumnWidth(0, 300);
			detail.SetColumnWidth(1, 70);
			detail.SetColumnWidth(2, 70);

			row = detail[0];
			row.SetShading(Header2Shading);
			row[0].SetContent(new Paragraph("Image/File").SetStyle(HeaderCss));
			row[1].SetContent(new Paragraph("XML Size").SetStyle(HeaderCss).SetAlignment("center"));
			row[2].SetContent(new Paragraph("Native Size").SetStyle(HeaderCss).SetAlignment("center"));

			if (images.Count > 0)
			{
				if (thumbnailSize == 0)
				{
					SummarizeImages(detail, images);
				}
				else
				{
					foreach (var image in images)
					{
						ReportImage(detail, image);
					}
				}
			}

			foreach (var file in files)
			{
				row = detail.AddRow();
				var name = file.Attribute("preferredName").Value;

				var path = file.Attribute("pathSource")?.Value;
				var original = path is not null;
				if (!original)
				{
					path = file.Attribute("pathCache")?.Value;
				}

				if (path is null)
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
						row[2].SetContent(new Paragraph(size.ToBytes(1)).SetAlignment("right"));
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
				new Paragraph(string.Empty)
				));
		}


		private void SummarizeImages(Table detail, List<XElement> images)
		{
			var total = images.Sum(i => i.Element(ns + "Data").Value.Length);

			var data = total.ToBytes(1);
			var estimate = ((int)(total * 0.733)).ToBytes(1);

			var row = detail.AddRow();

			row[0].SetContent(new Paragraph(
				string.Format(Resx.AnalyzeCommand_totalImages, images.Count))
				.SetStyle("font-style:italic"));

			row[1].SetContent(new Paragraph(
				string.Format(Resx.AnalyzeCommand_total, data))
				.SetAlignment("right"));

			row[2].SetContent(new Paragraph(
				string.Format(Resx.AnalyzeCommand_est, estimate))
				.SetAlignment("right"));
		}


		private void ReportImage(Table detail, XElement image, bool printout = false)
		{
			var row = detail.AddRow();
			var data = image.Element(ns + "Data").Value;

			var bytes = Convert.FromBase64String(image.Element(ns + "Data").Value);
			using (var stream = new MemoryStream(bytes, 0, bytes.Length))
			{
				using var raw = Image.FromStream(stream);
				XElement img = null;
				if (thumbnailSize > 0)
				{
					if (raw.Width > thumbnailSize || raw.Height > thumbnailSize)
					{
						// maintain aspect ratio of image thumbnails
						var zoom = raw.Width - thumbnailSize > raw.Height - thumbnailSize
							? ((float)thumbnailSize) / raw.Width
							: ((float)thumbnailSize) / raw.Height;

						Image thumbnail = null;

						try
						{
							thumbnail = new Bitmap(raw,
								(int)(raw.Width * zoom),
								(int)(raw.Height * zoom));

							img = MakeImage(thumbnail);
						}
						catch (Exception exc)
						{
							image.GetAttributeValue("format", out var format, "?");

							var cid = image.Element(ns + "CallbackID")?.Attribute("callbackID")?.Value
								?? "{unknown}";

							var info = image.Ancestors(ns + "Page")
								.Select(e => new { Name = e.Attribute("name").Value, ID = e.Attribute("ID").Value })
								.First();

							var msg = $"{format} caused {exc.Message}; callbackID {cid} on page '{info.Name}', ID {info.ID}";
							logger.WriteLine(msg, exc);
							row[0].SetContent(msg);
						}
						finally
						{
							thumbnail?.Dispose();
						}
					}
					else
					{
						img = MakeImage(raw);
					}
				}

				if (img is not null)
				{
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

			row[1].SetContent(new Paragraph(data.Length.ToBytes(1)).SetAlignment("right"));
			row[2].SetContent(new Paragraph(bytes.Length.ToBytes(1)).SetAlignment("right"));
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
