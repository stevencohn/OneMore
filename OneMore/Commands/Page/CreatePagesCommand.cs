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
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class CreatePagesCommand : Command
	{
		private OneNote one;
		private Page page;
		private XNamespace ns;
		private readonly List<string> names;


		public CreatePagesCommand()
		{
			names = new List<string>();
		}


		public override async Task Execute(params object[] args)
		{
			using (one = new OneNote(out page, out ns, OneNote.PageDetail.Selection))
			{
				var cursor = GetContextCursor(out var onlySelected);
				if (cursor == null)
				{
					UIHelper.ShowError(Resx.Error_BodyContext);
					return;
				}

				ReadNamesFromContext(cursor, onlySelected);
				if (!names.Any())
				{
					UIHelper.ShowError(Resx.CreatePagesCommand_NoNamesFound);
					return;
				}

				var msg = string.Format(Resx.CreatePagesCommand_CreatePages, names.Count);
				if (UIHelper.ShowQuestion(msg) != DialogResult.Yes)
				{
					return;
				}

				var progress = new UI.ProgressDialog(async (self, token) =>
				{
					logger.Start();
					logger.StartClock();

					var sectionId = one.CurrentSectionId;
					self.SetMaximum(names.Count);

					try
					{
						foreach (var name in names)
						{
							if (!token.IsCancellationRequested)
							{
								self.SetMessage(name);

								//await one.CreatePage(sectionId, name);
								one.CreatePage(sectionId, out var pageId);
								var newpage = one.GetPage(pageId);
								newpage.Title = name;
								await one.Update(newpage);								
								
								self.Increment();

								// purposely slowing it down so UI can catch up, yuck
								await Task.Delay(100);
							}
						}
					}
					finally
					{
						self.Close();
					}

					logger.WriteTime("create pages complete");
					logger.End();
				});

				await progress.RunModeless();
			}
		}


		private XElement GetContextCursor(out bool onlySelected)
		{
			onlySelected = false;

			var cursor = page.GetTextCursor();
			if (cursor == null)
			{
				cursor = page.GetSelectedElements().FirstOrDefault();
				if (cursor != null)
				{
					onlySelected = page.SelectionScope == SelectionScope.Region;
				}
			}

			return cursor;
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
