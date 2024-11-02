//************************************************************************************************
// Copyright © 2018 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Settings;
	using River.OneMoreAddIn.UI;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using System.Windows.Forms;
	using System.Xml.Linq;
	using Resx = Properties.Resources;


	internal class SearchAndReplaceCommand : Command
	{
		private Page currentPage;
		private OneNote.Scope scope;
		private SearchAndReplaceEditor editor;


		public SearchAndReplaceCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			editor = await MakeEditor();
			if (editor is null)
			{
				return;
			}

			var found = scope == OneNote.Scope.Self
				? await SearchPage()
				: await SearchHierarchy();

			if (found)
			{
				SaveSettings();
			}
		}


		private async Task<SearchAndReplaceEditor> MakeEditor()
		{
			await using var one = new OneNote(out currentPage, out _);
			var text = new PageEditor(currentPage).GetSelectedText();

			using var dialog = new SearchAndReplaceDialog();

			if (text.Length > 0)
			{
				dialog.WhatText = text;
			}

			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return null;
			}

			scope = dialog.Scope;

			if (dialog.RawXml is null)
			{
				// let user insert a newline char
				var withText = dialog.WithText.Replace("\\n", "\n");

				return new SearchAndReplaceEditor(
					dialog.WhatText,
					replacement: withText,
					enableRegex: dialog.UseRegex,
					caseSensitive: dialog.MatchCase
					);
			}

			return new SearchAndReplaceEditor(
				dialog.WhatText,
				element: dialog.RawXml,
				enableRegex: dialog.UseRegex,
				caseSensitive: dialog.MatchCase
				);
		}


		private async Task<bool> SearchPage()
		{
			var count = editor.SearchAndReplace(currentPage);
			logger.WriteLine($"found {count} matches on {currentPage.Title}");

			if (count > 0)
			{
				using var one = new OneNote();
				await one.Update(currentPage);
				return true;
			}

			return false;
		}


		public async Task<bool> SearchHierarchy()
		{
			using var one = new OneNote();
			var hierarchy = scope switch
			{
				OneNote.Scope.Pages => await one.GetSection(),
				OneNote.Scope.Sections => await one.GetNotebook(OneNote.Scope.Pages),
				_ => await one.GetNotebooks(OneNote.Scope.Pages)
			};

			var ns = hierarchy.GetNamespaceOfPrefix(OneNote.Prefix);

			var pagerefs = hierarchy.Descendants(ns + "Page")
				.Where(e => e.Attribute("isInRrecycleBin") is null);

			if (!pagerefs.Any())
			{
				logger.WriteLine("no pages found in scope");
				return false;
			}

			var totalPages = 0;
			var totalMatches = 0;
			using var progress = new ProgressDialog();
			progress.SetMaximum(pagerefs.Count());

			var result = progress.ShowDialogWithCancel(async (dialog, token) =>
			{
				foreach (var pageref in pagerefs)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}

					// build full page path, including section, section groups, and notebook
					var path = pageref.Attribute("name").Value;
					var parent = pageref.Parent;
					while (parent is not null && parent.Name.LocalName != "Notebooks")
					{
						path = $"{parent.Attribute("name").Value}/{path}";
						parent = parent.Parent;
					}

					progress.Increment();
					progress.SetMessage(path);

					var page = await one.GetPage(
						pageref.Attribute("ID").Value, OneNote.PageDetail.Basic);

					var matches = editor.SearchAndReplace(page);
					logger.WriteLine($"found {matches} matches on /{path}");

					if (matches > 0)
					{
						await one.Update(page);
						totalMatches += matches;
						totalPages++;
					}
				}

				return true;
			});

			if (result == DialogResult.Cancel)
			{
				logger.WriteLine("search and replace cancelled, " +
					$"{totalPages} of {pagerefs.Count()} pages searched");
			}
			else
			{
				if (totalMatches == 0 && totalPages == 0)
				{
					totalPages = pagerefs.Count();
				}

				ShowInfo(string.Format(
					Resx.SearchAndReplaceCommand_Results, totalMatches, totalPages));
			}

			return totalPages > 0;
		}


		private void SaveSettings()
		{
			var provider = new SettingsProvider();
			var settings = provider.GetCollection("SearchReplace");

			// remember WhatText and options...

			var whats = settings.Get<XElement>("whats");
			if (whats == null)
			{
				whats = new XElement("whats");
				settings.Add("whats", whats);
			}

			// only add if not already present
			if (!whats.Elements().Any(e => e.Value == editor.SearchPattern))
			{
				if (whats.Elements().Count() > 8)
				{
					whats.Elements().Last().Remove();
				}

				whats.AddFirst(new XElement("whatText",
					new XAttribute("matchCase", editor.CaseSensitive.ToString()),
					new XAttribute("useRegex", editor.EnableRegex.ToString()),
					HttpUtility.HtmlEncode(editor.SearchPattern)
					));
			}

			// remember WithText...

			var withs = settings.Get<XElement>("withs");
			if (withs == null)
			{
				withs = new XElement("withs");
				settings.Add("withs", withs);
			}

			// only add if not already present
			if (!withs.Elements().Any(e => e.Value == editor.ReplacementString))
			{
				if (withs.Elements().Count() > 8)
				{
					withs.Elements().Last().Remove();
				}

				withs.AddFirst(new XElement("withText",
					HttpUtility.HtmlEncode(editor.ReplacementString)
					));
			}

			provider.SetCollection(settings);
			provider.Save();
		}
	}
}
