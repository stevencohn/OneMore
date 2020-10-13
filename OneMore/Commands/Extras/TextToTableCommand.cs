//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using Microsoft.Office.Interop.OneNote;
	using River.OneMoreAddIn.Dialogs;
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Xml.Linq;


	internal class TextToTableCommand : Command
	{
		public TextToTableCommand()
		{
		}


		public void Execute()
		{
			System.Diagnostics.Debugger.Launch();

			using (var manager = new ApplicationManager())
			{
				var page = new Page(manager.CurrentPage(PageInfo.piSelection));

				var selections = page.Root
					.Descendants(page.Namespace + "OE")
					.Elements(page.Namespace + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.ToList();

				if (selections.Count == 0 || (selections.Count == 1 && selections[0].Value == string.Empty))
				{
					UIHelper.ShowMessage(manager.Window, "No text is selected");
					return;
				}

				var table = TextToTable(page.Namespace, selections);

				if (table != null)
				{
					var first = selections[0];
					for (int i = 1; i < selections.Count; i++)
					{
						selections[i].Remove();
					}

					first.ReplaceNodes(table.Root);

					//logger.WriteLine(page.Root.ToString());
					manager.UpdatePageContent(page.Root);
				}
			}
		}


		private Table TextToTable(XNamespace ns, List<XElement> selections)
		{
			TextToTableDialog.Delimeter delimetedBy;
			string delimeter;
			int cols, rows;

			using (var dialog = new TextToTableDialog())
			{
				var first = selections[0].Value;
				if (first.Contains('\t'))
				{
					dialog.DelimetedBy = TextToTableDialog.Delimeter.Tabs;
					dialog.Columns = first.Split(new char[] { '\t' }).Length;
				}
				else if (first.Contains(','))
				{
					dialog.DelimetedBy = TextToTableDialog.Delimeter.Commas;
					dialog.Columns = first.Split(new char[] { ',' }).Length;
				}
				else
				{
					dialog.DelimetedBy = TextToTableDialog.Delimeter.Paragraphs;
					dialog.Columns = 1;
				}

				dialog.Rows = selections.Count;

				if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.Cancel)
				{
					return null;
				}

				rows = dialog.Rows;
				cols = dialog.Columns;

				delimetedBy = dialog.DelimetedBy;
				switch (delimetedBy)
				{
					case TextToTableDialog.Delimeter.Commas: delimeter = ","; break;
					case TextToTableDialog.Delimeter.Tabs: delimeter = "\t"; break;
					case TextToTableDialog.Delimeter.Other: delimeter = dialog.CustomDelimeter; break;
					default: delimeter = string.Empty; break;
				}
			}

			var table = new Table(ns, 0, cols)
			{
				BordersVisible = true
			};

			foreach (var selection in selections)
			{
				var row = table.AddRow();

				if (delimetedBy == TextToTableDialog.Delimeter.Paragraphs)
				{
					row.Cells.ElementAt(0).SetContent(selection.Value);
				}
				else
				{
					var parts = selection.Value.Split(new string[] { delimeter }, StringSplitOptions.None);
					for (int i = 0; i < parts.Length && i < cols; i++)
					{
						row.Cells.ElementAt(i).SetContent(parts[i]);
					}
				}
			}

			if (rows < selections.Count)
			{
				for (int i = rows; i < selections.Count; i++)
				{
					table.AddRow();
				}
			}

			return table;
		}
	}
}
