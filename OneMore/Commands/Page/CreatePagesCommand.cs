//************************************************************************************************
// Copyright © 2022 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Generates pages in the current section from a list of page names where the list could
	/// be a bulleted list, a numbered list, a column from a table, the first column from an
	/// embedded spreadsheet, or OCR text from an embedded image.
	/// </summary>
	internal class CreatePagesCommand : Command
	{
		private const string RightArrow = "→";

		private Page page;
		private XNamespace ns;
		private readonly List<(string Name, XElement Source)> names;


		public CreatePagesCommand()
		{
			names = new List<(string Name, XElement Source)>();
		}


		public override async Task Execute(params object[] args)
		{
			await using var one = new OneNote(out page, out ns, OneNote.PageDetail.Selection);

			var cursor = GetContextCursor(out var onlySelected);
			if (cursor == null)
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			ReadNamesFromContext(cursor, onlySelected);
			if (!names.Any())
			{
				ShowError(Resx.CreatePagesCommand_NoNamesFound);
				return;
			}

			bool createLinks;
			using (var dialog = new CreatePagesDialog(names.Count))
			{
				if (dialog.ShowDialog(owner) != DialogResult.OK)
				{
					return;
				}

				createLinks = dialog.CreateLinks;
			}

			var progress = new UI.ProgressDialog(async (self, token) =>
			{
				logger.Start();
				logger.StartClock();

				var sectionId = one.CurrentSectionId;
				self.SetMaximum(names.Count);

				var crumbs = createLinks ? BuildCrumbs(one) : null;
				var linkedSource = false;

				try
				{
					foreach (var (name, source) in names)
					{
						if (!token.IsCancellationRequested)
						{
							self.SetMessage(name);

							one.CreatePage(sectionId, out var pageId);
							var newpage = await one.GetPage(pageId);
							newpage.Title = name;

							if (createLinks && source != null &&
								source.GetAttributeValue("objectID", out var objectId))
							{
								AddForwardLink(source, one.GetHyperlink(pageId, newpage.TitleID));
								AddBacklink(newpage, crumbs, one.GetHyperlink(page.PageId, objectId), name);
								linkedSource = true;
							}

							await one.Update(newpage);

							self.Increment();

							// purposely slowing it down so UI can catch up, yuck
							await Task.Delay(100);
						}
					}

					if (linkedSource)
					{
						await one.Update(page);
					}
				}
				finally
				{
					self.Close();
				}

				logger.WriteTime("create pages complete");
				logger.End();
			});

			progress.RunModeless();
		}


		private string BuildCrumbs(OneNote one)
		{
			var crumbs = new StringBuilder();

			var id = one.GetParent(page.PageId);
			while (!string.IsNullOrEmpty(id))
			{
				var node = one.GetHierarchyNode(id);
				crumbs.Insert(0, $"{node.Name} {RightArrow} ");
				id = one.GetParent(id);
			}

			crumbs.Append(page.Title);
			return crumbs.ToString();
		}


		private IEnumerable<XElement> GetTextRuns(XElement source)
		{
			// a List item's T runs are direct children whereas a table Cell's content
			// is nested under its first paragraph, Cell/OEChildren/OE/T...

			var oe = source.Name.LocalName == "Cell"
				? source.Elements(ns + "OEChildren").Elements(ns + "OE").FirstOrDefault()
				: source;

			return oe?.Elements(ns + "T") ?? Enumerable.Empty<XElement>();
		}


		private void AddForwardLink(XElement source, string link)
		{
			foreach (var run in GetTextRuns(source))
			{
				var cdata = run.GetCData();
				if (cdata != null)
				{
					cdata.Value = $"<a href='{link}'>{cdata.Value}</a>";
				}
			}
		}


		private void AddBacklink(Page newpage, string crumbs, string link, string text)
		{
			if (text.Length > 20)
			{
				text = $"{text.Substring(0, 20)}...";
			}

			var fragment = $"<a href='{link}'>{crumbs} {RightArrow} <i>{text}</i></a>";

			var newns = newpage.Namespace;
			var index = newpage.GetQuickStyle(Styles.StandardStyles.Quote).Index;
			var paragraph = new Paragraph(newns, fragment).SetQuickStyle(index);

			newpage.EnsureContentContainer().AddFirst(paragraph, new Paragraph(newns));
		}


		private XElement GetContextCursor(out bool onlySelected)
		{
			onlySelected = false;

			var range = new Models.SelectionRange(page);
			var selections = range.GetSelections(anyElement: true);

			if (range.Scope == SelectionScope.TextCursor)
			{
				return selections.First();
			}

			if (range.Scope == SelectionScope.Run || // allow single List item
				range.Scope == SelectionScope.Range ||
				range.Scope == SelectionScope.Block)
			{
				onlySelected = true;
				return selections.First();
			}

			return null;
		}


		private void ReadNamesFromContext(XElement cursor, bool onlySelected)
		{
			if (ContextIsList(cursor))
			{
				ReadList(cursor, onlySelected);
			}
			else if (ContextIsTable(cursor))
			{
				ReadTable(cursor, onlySelected);
			}
			else if (ContextIsExcelFile(out var source))
			{
				ReadExcel(source);
			}
			else if (ContextIsImage(out var image))
			{
				ReadImage(image);
			}
		}


		private bool ContextIsList(XElement cursor)
		{
			return cursor.ElementsBeforeSelf(ns + "List").Any();
		}


		private void ReadList(XElement cursor, bool onlySelected)
		{
			logger.WriteLine("list");
			var container = cursor.Parent.Parent;
			var elements = onlySelected
				? container.Elements(ns + "OE").Where(e => e.Attribute("selected") != null)
				: container.Elements(ns + "OE");

			elements.ForEach(o =>
			{
				logger.WriteLine($"item: {o.TextValue()}");
				names.Add((o.TextValue(), o));
			});
		}


		private bool ContextIsTable(XElement cursor)
		{
			// should have selected Cell
			return cursor.Name.LocalName == "Cell";
		}


		private void ReadTable(XElement cursor, bool onlySelected)
		{
			logger.WriteLine("table");

			// Cell
			var anchor = cursor;

			var table = anchor.FirstAncestor(ns + "Table");
			var index = anchor.ElementsBeforeSelf(ns + "Cell").Count();

			table.Elements(ns + "Row").ForEach(r =>
			{
				var cell = r.Elements(ns + "Cell").ElementAt(index);
				if (cell != null &&
					(!onlySelected || (cell.Attribute("selected") != null)))
				{
					logger.WriteLine($"cell: {cell.TextValue()}");
					names.Add((cell.TextValue(), cell));
				}
			});
		}


		private bool ContextIsExcelFile(out string source)
		{
			source = (
				from e in page.Root.Descendants(ns + "InsertedFile")
				where e.Attribute("selected")?.Value == "all"
				let s = e.Attribute("pathSource").Value
				let x = System.IO.Path.GetExtension(s)
				where x == ".xlsx" || x == ".xls"
				select s
				).FirstOrDefault();

			return source != null;
		}


		private void ReadExcel(string source)
		{
			logger.WriteLine("excel");
			using var excel = new Excel();
			var list = excel.ExtractSimpleList(source);
			list.ForEach(l =>
			{
				logger.WriteLine($"item: {l}");
				names.Add((l, null));
			});
		}


		private bool ContextIsImage(out XElement image)
		{
			image = page.Root.Descendants(ns + "Image")
				.FirstOrDefault(e =>
					e.Attribute("selected")?.Value == "all" &&
					e.Elements(ns + "OCRData").Elements(ns + "OCRText").Any());

			return image != null;
		}


		private void ReadImage(XElement image)
		{
			logger.WriteLine("image");
			image.Elements(ns + "OCRData")
				.Elements(ns + "OCRText")
				.ForEach(t =>
				{
					t.TextValue().Split('\n').ForEach(s =>
					{
						logger.WriteLine($"ocr: {s}");
						names.Add((s, null));
					});
				});
		}
	}
}
