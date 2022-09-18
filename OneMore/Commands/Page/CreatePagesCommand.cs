//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
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

				if (cursor.ElementsBeforeSelf(ns + "List").Any())
				{
					ReadList(cursor.Parent.Parent, onlySelected);
				}

				/*
				var tables = page.Root.Descendants(ns + "Table");
				tables.ForEach(t =>
				{
					logger.WriteLine("table");
					t.Elements(ns + "Row").ForEach(r =>
					{
						var cell = r.Elements(ns + "Cell").FirstOrDefault();
						if (cell != null)
						{
							logger.WriteLine($"cell: {cell.TextValue()}");
							names.Add(cell.TextValue());
						}
					});
				});

				page.Root.Descendants(ns + "Image")
					.Where(e => e.Elements(ns + "OCRData").Elements(ns + "OCRText").Any())
					.ForEach(i =>
					{
						logger.WriteLine("image");
						i.Elements(ns + "OCRData").Elements(ns + "OCRText").ForEach(t =>
						{
							t.TextValue().Split('\n')
							.ForEach(s =>
							{
								logger.WriteLine($"ocr: {s}");
								names.Add(s);
							});
						});
					});
				*/
			}

			await Task.Yield();
		}


		private void ReadList(XElement container, bool onlySelected)
		{
			logger.WriteLine("list");
			var elements = onlySelected
				? container.Elements(ns + "OE").Where(e => e.Attribute("selected") != null)
				: container.Elements(ns + "OE");

			elements.ForEach(o =>
			{
				logger.WriteLine($"item: {o.TextValue()}");
				names.Add(o.TextValue());
			});
		}
	}
}
