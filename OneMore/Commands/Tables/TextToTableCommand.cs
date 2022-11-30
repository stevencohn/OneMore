//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;


	internal class TextToTableCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";


		public TextToTableCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Selection);
			var selections = page.Root
				.Descendants(page.Namespace + "OE")
				.Elements(page.Namespace + "T")
				.Where(e => e.Attribute("selected")?.Value == "all")
				.Select(e => e.Parent)
				.ToList();

			if (selections.Count == 0 || (selections.Count == 1 && selections[0].Value == string.Empty))
			{
				UIHelper.ShowInfo(one.Window, Resx.TextToTable_NoText);
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

				await one.Update(page);
			}
		}


		private Table TextToTable(XNamespace ns, List<XElement> selections)
		{
			// when pasting text with tabs \t from Notepad into OneNote, OneNote will preserve
			// the tabs internally but the presentation XML through the COM API will transform
			// them to a sequence of 8x "&nbsp;" so we can assume this represents a tab
			string nbsp = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";

			TextToTableDialog.Delimeter delimetedBy;
			string delimeter;
			int cols, rows;
			bool hasHeader;
			bool unquote;

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
				else if (first.Contains(nbsp))
				{
					dialog.DelimetedBy = TextToTableDialog.Delimeter.Nbsp;
					dialog.Columns = first.Split(new string[] { nbsp }, StringSplitOptions.None).Length;
				}
				else
				{
					dialog.DelimetedBy = TextToTableDialog.Delimeter.Paragraphs;
					dialog.Columns = 1;
				}

				dialog.Rows = selections.Count;

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
				{
					return null;
				}

				rows = dialog.Rows;
				cols = dialog.Columns;

				delimetedBy = dialog.DelimetedBy;
				delimeter = delimetedBy switch
				{
					TextToTableDialog.Delimeter.Commas => ",",
					TextToTableDialog.Delimeter.Tabs => "\t",
					TextToTableDialog.Delimeter.Nbsp => nbsp,
					TextToTableDialog.Delimeter.Other => dialog.CustomDelimeter,
					_ => string.Empty,
				};

				hasHeader = dialog.HasHeader;
				unquote = dialog.Unquote;
			}

			var table = new Table(ns, rows, cols)
			{
				BordersVisible = true
			};

			for (int r = 0; r < selections.Count; r++)
			{
				var selection = selections[r];
				var row = table.Rows.ElementAt(r);

				if (delimetedBy == TextToTableDialog.Delimeter.Paragraphs)
				{
					SetCellContent(row.Cells.ElementAt(0), selection.Value.Trim(), r, unquote, hasHeader);
				}
				else
				{
					var parts = selection.Value.Split(new string[] { delimeter }, StringSplitOptions.None);
					for (int c = 0; c < parts.Length && c < cols; c++)
					{
						SetCellContent(row.Cells.ElementAt(c), parts[c].Trim(), r, unquote, hasHeader);
					}
				}
			}

			return table;
		}


		public void SetCellContent(TableCell cell, string text, int rownum, bool unquote, bool hasHeader)
		{
			if (unquote)
			{
				if (text.Length > 12)
				{
					if (text.StartsWith("&quot;") && text.EndsWith("&quot;"))
					{
						text = text.Substring(6, text.Length - 12);
					}
				}
			}

			if (rownum == 0 && hasHeader)
			{
				text = $"<span style=\"font-weight:bold\">{text}</span>";
				cell.ShadingColor = HeaderShading;
			}

			cell.SetContent(text);
		}
	}
}
