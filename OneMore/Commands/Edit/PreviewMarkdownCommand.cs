﻿//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;


	/// <summary>
	/// Preview the selected markdown text in a separate window.
	/// </summary>
	internal class PreviewMarkdownCommand : Command
	{
		public PreviewMarkdownCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			using var one = new OneNote(out var page, out var ns);
			page.GetTextCursor();

			if (page.SelectionScope != SelectionScope.Region)
			{
				ShowError("Select markdown text to preview");
				return;
			}

			var bounds = one.GetCurrentMainWindowBounds();

			var editor = new PageEditor(page);
			var content = await editor.ExtractSelectedContent();
			var paragraphs = content.Elements(ns + "OE").ToList();

			var reader = new PageReader(page)
			{
				ColumnDivider = "|",
				TableSides = "|"
			};

			var text = reader.ReadTextFrom(paragraphs, page.SelectionScope != SelectionScope.Region);

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var body = OneMoreDig.ConvertMarkdownToHtml(filepath, text);

			filepath = Path.Combine(
				Path.GetDirectoryName(filepath),
				Path.GetFileNameWithoutExtension(filepath)) + ".htm";

			File.WriteAllText(filepath, body);
			logger.WriteLine($"filepath {new System.Uri(filepath)}");
			logger.WriteLine(body);

			await SingleThreaded.Invoke(() =>
			{
				// WebView2 needs a message pump so host in its own invisible worker dialog
				using var form = new WebViewWorkerDialog(
					startup:
					new WebViewWorker(async (webview) =>
					{
						//logger.WriteLine($"starting up webview with {uri}");
						webview.Source = new System.Uri(filepath);
						await Task.Yield();
						return true;
					}),
					work: null);

				form.Opacity = 1.0;
				form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
				form.Width = bounds.Width / 2 + 100;
				form.Height = bounds.Height - 100;
				form.Left = bounds.Left + (bounds.Width / 2) - 50;
				form.Top = bounds.Top + 50;
				form.TopMost = true;

				form.Icon = Properties.Resources.OneMore_Icon;
				form.Text = "Markdown Preview";

				form.ShowDialog(owner);
			});
		}
	}
}