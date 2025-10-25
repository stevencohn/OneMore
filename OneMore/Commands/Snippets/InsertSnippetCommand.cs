//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	/// <summary>
	/// Insert a custom snippet onto the page at the current insert point. 
	/// This can run as a direct command from the My Custom Snippets menu or can be invoked
	/// as to expand the name of a custom snippet to its contents using the Alt+F3 shortcut
	/// </summary>
	internal class InsertSnippetCommand : Command
	{
		private const string DateTimePattern = @"{=DATETIME\(([^""]+)\)}";
		private const string BodyTag = "{SNIPPET_BODY}";


		public InsertSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var path = args[0] as string;
			string snippet = null;

			var provider = new SnippetsProvider();

			await using var one = new OneNote(out var page, out _);

			if (!page.ConfirmBodyContext())
			{
				ShowError(Resx.Error_BodyContext);
				return;
			}

			// Either invoked via Alt+F3 to expand the selected name to a snippet
			// or invoked from the My Custom snippets menu...

			if (string.IsNullOrWhiteSpace(path))
			{
				// assume Expand command and infer name from current word...

				path = new PageEditor(page).GetSelectedText();
				if (!string.IsNullOrWhiteSpace(path))
				{
					snippet = await provider.LoadByName(path);
					if (!string.IsNullOrEmpty(snippet))
					{
						// remove placeholder
						var updated = new PageEditor(page).EditSelected((s) =>
						{
							if (s is XText text)
							{
								text.Value = string.Empty;
								return text;
							}

							var element = (XElement)s;
							element.Value = string.Empty;
							return element;
						});

						if (updated)
						{
							await one.Update(page);
						}
					}
				}
			}
			else
			{
				snippet = await provider.Load(path);
			}

			if (string.IsNullOrWhiteSpace(snippet))
			{
				ShowError(string.Format(Resx.InsertSnippets_CouldNotLoad, path));
				return;
			}

			var clippy = new ClipboardProvider();
			await clippy.StashState();

			try
			{
				snippet = await Expand(page, snippet);

				await clippy.Clear();
				var success = await clippy.SetHtml(snippet);
				if (success)
				{
					await ClipboardProvider.Paste(true);
				}
				else
				{
					ShowInfo(Resx.Clipboard_locked);
				}
			}
			finally
			{
				var success = await clippy.RestoreState();
				if (!success)
				{
					ShowInfo(Resx.Clipboard_norestore);
				}
			}
		}


		private async Task<string> Expand(Page page, string snippet)
		{
			// process datetime patterns
			snippet = Regex.Replace(snippet, DateTimePattern, (m) =>
			{
				var format = m.Groups[1].Value;
				try
				{
					return System.DateTime.Now.ToString(format);
				}
				catch
				{
					// invalid format, return default datetime formatted string
					return System.DateTime.Now.ToString();
				}
			}, RegexOptions.IgnoreCase);

			if (snippet.ContainsICIC(BodyTag))
			{
				snippet = await ExpandSnippetBody(page, snippet);
			}

			return snippet;
		}


		private async Task<string> ExpandSnippetBody(Page page, string snippet)
		{
			var range = new SelectionRange(page);
			_ = range.GetSelection(true);
			if (range.Scope != SelectionScope.TextCursor)
			{
				// selection range found so move it into snippet
				var editor = new PageEditor(page)
				{
					// the extracted content will be selected=all, keep it that way
					KeepSelected = true
				};

				var content = editor.ExtractSelectedContent(breakParagraph: true);

				if (!content.HasElements)
				{
					ShowError(Resx.Error_BodyContext);
					logger.WriteLine("error reading page content!");
					return snippet;
				}

				editor.Deselect();
				editor.FollowWithCurosr(content);

				// copy the selected content to the clipboard where it can be transformed to HTML
				await ClipboardProvider.Copy();

				var html = await ClipboardProvider.GetHtml();
				html = ClipboardProvider.UnwrapHtml(html);

				snippet = snippet.ReplaceIgnoreCase(BodyTag, html);

				// recalculate preamble offsets
				snippet = ClipboardProvider.WrapWithHtmlPreamble(
					ClipboardProvider.UnwrapHtml(snippet));
			}

			return snippet;
		}
	}
}
