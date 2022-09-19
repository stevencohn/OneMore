//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;


	internal class CreatePagesCommand : Command
	{
		private Page page;
		private XNamespace ns;
		private readonly List<string> names;


		public CreatePagesCommand()
		{
			names = new List<string>();
		}


		public override async Task Execute(params object[] args)
		{
			using (var one = new OneNote(out page, out ns, OneNote.PageDetail.Selection))
			{
				var onlySelected = false;

				var cursor = page.GetTextCursor();
				if (cursor == null)
				{
					cursor = page.GetSelectedElements().FirstOrDefault();
					if (cursor == null)
					{
						logger.WriteLine("no context");
						return;
					}

					onlySelected = page.SelectionScope == SelectionScope.Region;
				}

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

			await Task.Yield();
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
				names.Add(o.TextValue());
			});
		}


		private bool ContextIsTable(XElement cursor)
		{
			return cursor.Parent?.Parent?.Parent?.Name.LocalName == "Cell";
		}


		private void ReadTable(XElement cursor, bool onlySelected)
		{
			logger.WriteLine("table");
			var anchor = cursor.Parent.Parent.Parent;
			var table = anchor.FirstAncestor(ns + "Table");
			var index = anchor.ElementsBeforeSelf(ns + "Cell").Count();

			table.Elements(ns + "Row").ForEach(r =>
			{
				var cell = r.Elements(ns + "Cell").ElementAt(index);
				if (cell != null && 
					(!onlySelected || 
					(onlySelected && cell.Attribute("selected") != null)))
				{
					logger.WriteLine($"cell: {cell.TextValue()}");
					names.Add(cell.TextValue());
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
			using (var excel = new Excel())
			{
				var list = excel.ExtractSimpleList(source);
				list.ForEach(l => logger.WriteLine($"item: {l}"));
				names.AddRange(list);
			}
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
						names.Add(s);
					});
				});
		}
	}
}
