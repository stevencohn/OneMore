//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Converts selected text to a table. Text must be delimited by a comma, space, or other
	/// special character so the command can detect columns.
	/// </summary>
	internal class TextToTableCommand : Command
	{
		private const string HeaderShading = "#DEEBF6";
		private const string nbtab = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
		private const string nbsp = "&nbsp;";
		private const string ucsp = "\u00A0";

		private static bool commandIsActive = false;


		public TextToTableCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			if (commandIsActive) { return; }
			commandIsActive = true;

			try
			{
				await using var one = new OneNote(out var page, out var ns, OneNote.PageDetail.Selection);
				var selections = page.Root
					.Descendants(page.Namespace + "OE")
					.Elements(page.Namespace + "T")
					.Where(e => e.Attribute("selected")?.Value == "all")
					.Select(e => e.Parent)
					.ToList();

				if (selections.Count == 0 || (selections.Count == 1 && selections[0].Value == string.Empty))
				{
					ShowInfo(Resx.TextToTable_NoText);
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
			finally
			{
				commandIsActive = false;
			}
		}


		private Table TextToTable(XNamespace ns, List<XElement> selections)
		{
			// when pasting text with tabs \t from Notepad into OneNote, OneNote will preserve
			// the tabs internally but the presentation XML through the COM API will transform
			// them to a sequence of 8x "&nbsp;" so we can assume this represents a tab

			TextToTableDialog.Delimeter delimetedBy;
			string delimeter;
			int cols, rows;
			bool hasHeader;
			bool unquote;

			using var dialog = new TextToTableDialog();
			PrepareDialog(dialog, selections[0].Value);
			dialog.Rows = selections.Count;

			if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.Cancel)
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
				TextToTableDialog.Delimeter.NbTab => "\t",
				TextToTableDialog.Delimeter.Other => dialog.CustomDelimeter,
				_ => string.Empty,
			};

			hasHeader = dialog.HasHeader;
			unquote = dialog.Unquote;

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
					// special case to avoid exception when XML parsing the &nbsp; so replace
					// string of 8 &nbsp; sequences with a tab and split by that instead...

					var text = delimetedBy == TextToTableDialog.Delimeter.NbTab
						? selection.Value.Replace(nbtab, delimeter)
						: selection.Value;

					var parts = SmartSplit(text, delimeter);

					for (int c = 0; c < parts.Length && c < cols; c++)
					{
						SetCellContent(row.Cells.ElementAt(c), parts[c].Trim(), r, unquote, hasHeader);
					}
				}
			}

			return table;
		}


		private static void PrepareDialog(TextToTableDialog dialog, string first)
		{
			if (first.Contains('\t'))
			{
				dialog.DelimetedBy = TextToTableDialog.Delimeter.Tabs;
				dialog.Columns = first.Split('\t').Length;
			}
			else if (first.Contains(','))
			{
				dialog.DelimetedBy = TextToTableDialog.Delimeter.Commas;
				dialog.Columns = first.Split(',').Length;
			}
			else if (first.Contains(nbtab))
			{
				dialog.DelimetedBy = TextToTableDialog.Delimeter.NbTab;
				dialog.Columns = first.Split(new string[] { nbtab }, StringSplitOptions.None).Length;
			}
			else
			{
				dialog.DelimetedBy = TextToTableDialog.Delimeter.Paragraphs;
				dialog.Columns = 1;
			}
		}


		private static string[] SmartSplit(string xml, string delimeter)
		{
			// handles odd OneNote style normalizations like this in order to preserve
			// styles of each column
			//
			//		<span style='font-weight:bold'>Word, Int, Double, </span><span
			//		style='font-weight: bold;font-style:italic'>Olde</span>
			//

			if (xml.Contains(nbsp))
			{
				xml = Regex.Replace(xml, $"(?:{nbsp})+", ucsp);
			}

			var wrapper = XElement.Parse($"<w>{xml}</w>");
			var parts = new List<string>();
			foreach (var node in wrapper.Nodes())
			{
				if (node is XText text)
				{
					SplitText(parts, text, delimeter);
				}
				else
				{
					SplitElement(parts, node, delimeter);
				}
			}

			return parts.ToArray();
		}


		private static void SplitText(List<string> parts, XText text, string delimeter)
		{
			if (text.Value.Contains(delimeter))
			{
				var values = text.Value
					.Split(new string[] { delimeter }, StringSplitOptions.None);

				for (int i = 0; i < values.Length; i++)
				{
					var v = values[i].Trim();
					if (v.Length > 0 || parts.Count == 0)
					{
						parts.Add(v);
					}
				}
			}
			else
			{
				parts.Add(text.Value);
			}
		}


		private static void SplitElement(List<string> parts, XNode node, string delimeter)
		{
			var element = (XElement)node;
			if (element.Value.Contains(delimeter))
			{
				var values = element.Value
					.Split(new string[] { delimeter }, StringSplitOptions.None);

				for (int i = 0; i < values.Length; i++)
				{
					var v = values[i].Trim();
					if (v.Length > 0 || i < values.Length - 1)
					{
						parts.Add(
							new XElement("span", element.Attributes(), v)
							.ToString(SaveOptions.DisableFormatting));
					}
				}
			}
			else
			{
				parts.Add(node.ToString(SaveOptions.DisableFormatting));
			}
		}


		public static void SetCellContent(
			TableCell cell, string text, int rownum, bool unquote, bool hasHeader)
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
