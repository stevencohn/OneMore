//************************************************************************************************
// Copyright © 2024 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.UI;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Resx = Properties.Resources;


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

			var bounds = one.GetCurrentMainWindowBounds();

			var editor = new PageEditor(page)
			{
				AllContent = (page.SelectionScope != SelectionScope.Region)
			};

			var content = await editor.ExtractSelectedContent();
			var paragraphs = content.Elements(ns + "OE").ToList();

			var reader = new PageReader(page)
			{
				ColumnDivider = "|",
				TableSides = "|"
			};

			var filepath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var text = reader.ReadTextFrom(paragraphs, page.SelectionScope != SelectionScope.Region);

			var title = editor.AllContent
				? $"<p style=\"font-family:Calibri;font-size:20pt\">{page.Title}</p>"
				: string.Empty;

			var body = title + OneMoreDig.ConvertMarkdownToHtml(filepath, text);

			filepath = Path.Combine(
				Path.GetDirectoryName(filepath),
				Path.GetFileNameWithoutExtension(filepath)) + ".htm";

			File.WriteAllText(filepath, body);

			await SingleThreaded.Invoke(() =>
			{
				// WebView2 needs a message pump so host in its own invisible worker dialog
				using var form = new WebViewDialog(new System.Uri(filepath));

				form.Width = bounds.Width / 2 + 100;
				form.Height = bounds.Height - 200;
				form.Left = bounds.Left + (bounds.Width / 2) - 50;
				form.Top = bounds.Top + 100;
				form.TopMost = true;

				form.Text = Resx.PreviewMarkdownCommand_Title;

				form.ShowDialog(owner);
			});
		}
	}
}
