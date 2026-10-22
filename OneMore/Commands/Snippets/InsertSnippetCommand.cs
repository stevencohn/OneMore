//************************************************************************************************
// Copyright © 2021 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
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
		private const string BodyTag = SnippetMacros.BodyTag;


		public InsertSnippetCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			var path = args[0] as string;
			string snippet = null;
			SnippetMacros macros = null;
			var isXml = false;

			var provider = new SnippetsProvider();

			await using var one = new OneNote(out var page, out _);

			if (page is null || !page.ConfirmBodyContext())
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
					var found = provider.GetPathByName(path);
					isXml = found is not null && SnippetsProvider.IsXml(found);

					snippet = await provider.LoadByName(path);
					if (!string.IsNullOrEmpty(snippet))
					{
						// prompt before touching the page so a cancel leaves it unchanged
						macros = await SnippetMacros.Create(one, page, snippet, owner);
						if (macros is null)
						{
							return;
						}

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

						// XML snippets are inserted into this same page below so one Update suffices
						if (updated && !isXml)
						{
							await one.Update(page);
						}
					}
				}
			}
			else
			{
				isXml = SnippetsProvider.IsXml(path);
				snippet = await provider.Load(path);
				if (!string.IsNullOrWhiteSpace(snippet))
				{
					macros = await SnippetMacros.Create(one, page, snippet, owner);
					if (macros is null)
					{
						return;
					}
				}
			}

			if (string.IsNullOrWhiteSpace(snippet))
			{
				ShowError(string.Format(Resx.InsertSnippets_CouldNotLoad, path));
				return;
			}

			if (isXml)
			{
				if (await InsertXml(one, page, snippet, macros))
				{
					macros.CommitCounters();
				}
				else
				{
					ShowError(string.Format(Resx.InsertSnippets_CouldNotLoad, path));
				}

				return;
			}

			var clippy = new ClipboardProvider();
			await clippy.StashState();

			try
			{
				snippet = await Expand(page, snippet, macros);

				await clippy.Clear();
				var success = await clippy.SetHtml(snippet);
				if (success)
				{
					await ClipboardProvider.Paste();
					macros.CommitCounters();
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


		/// <summary>
		/// Inserts a snippet stored as page XML directly into the page, bypassing the
		/// clipboard so all formatting is preserved.
		/// </summary>
		private async Task<bool> InsertXml(
			OneNote one, Page page, string snippet, SnippetMacros macros)
		{
			var content = SnippetPackage.Parse(snippet, page.Namespace, out var kind, out var root);
			if (content is null || !content.HasElements)
			{
				return false;
			}

			SnippetPackage.ExpandMacros(content, macros);
			SnippetPackage.MergeDefinitions(page, root, content);

			XElement anchor = null;

			if (SnippetPackage.HasBodyToken(content))
			{
				kind = SnippetPackage.KindParagraphs;

				var body = new XElement(page.Namespace + "OEChildren");

				var range = new SelectionRange(page);
				_ = range.GetSelection(true);
				if (range.Scope != SelectionScope.TextCursor)
				{
					// selected content is moved into the snippet; the editor leaves the
					// anchor where the snippet should be placed
					var editor = new PageEditor(page);
					var selected = editor.ExtractSelectedContent(breakParagraph: true);
					if (selected.HasElements)
					{
						body = selected;
						anchor = editor.Anchor;
					}
					else
					{
						logger.WriteLine("error reading page content!");
					}
				}

				SnippetPackage.ExpandBody(content, page.Namespace, body);
			}

			SnippetPackage.Insert(page, content, kind, anchor);

			await one.Update(page);
			return true;
		}


		private async Task<string> Expand(Page page, string snippet, SnippetMacros macros)
		{
			snippet = macros.Expand(snippet);

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
				if (html is null)
				{
					logger.WriteLine("error reading clipboard HTML after copy");
					return snippet;
				}

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
