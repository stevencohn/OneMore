//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.Helpers.Office;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.Xml.Linq;


	/// <summary>
	/// Applies a custom background color to the page and optionally applies the currently
	/// loaded custom styles to all content on the page
	/// </summary>
	internal class PageColorCommand : Command
	{
		private string pageColor;
		private ApplyStylesCommand styler;


		public PageColorCommand()
		{
		}


		public override async Task Execute(params object[] args)
		{
			Color color;
			Page page;
			await using (var one = new OneNote(out page, out _))
			{
				color = page.GetPageColor(out var automatic, out _);
				if (automatic)
				{
					color = Color.Transparent;
				}
			}

			using var dialog = new PageColorDialog(color, new ThemeProvider().Theme.Name);
			if (dialog.ShowDialog(owner) != DialogResult.OK)
			{
				return;
			}

			pageColor = MakePageColor(dialog.Color);

			if (dialog.ApplyStyle)
			{
				ThemeProvider.RecordTheme(dialog.ThemeKey);
				styler = new ApplyStylesCommand();
				styler.SetLogger(logger);
			}

			if (dialog.Scope == OneNote.Scope.Self)
			{
				UpdatePageColor(page, pageColor);
				styler?.ApplyTheme(page);

				await using var one = new OneNote();
				await one.Update(page);
			}
			else
			{
				await ColorPages(dialog.Scope, pageColor);
			}
		}


		private async Task ColorPages(OneNote.Scope scope, string pageColor)
		{

			await using var one = new OneNote();

			var root = scope == OneNote.Scope.Pages
				? await one.GetSection()
				: await one.GetNotebook(OneNote.Scope.Pages);

			var ns = root.GetNamespaceOfPrefix(OneNote.Prefix);

			var nodes = root.Descendants(ns + "Page")
				.Where(e => e.Attribute("isInRecycleBin") == null);

			using var progress = new UI.ProgressDialog();
			progress.SetMaximum(nodes.Count());

			progress.ShowDialogWithCancel(async (dialog, token) =>
			{
				foreach (var node in nodes)
				{
					var page = await one.GetPage(node.Attribute("ID").Value, OneNote.PageDetail.Basic);
					if (token.IsCancellationRequested) break;

					progress.SetMessage(page.Title);

					UpdatePageColor(page, pageColor);
					styler?.ApplyTheme(page);

					if (token.IsCancellationRequested) break;

					await one.Update(page);

					progress.Increment();
					if (token.IsCancellationRequested) break;
				}

				return true;
			});
		}


		public static string MakePageColor(Color color)
		{
			var dark = Office.IsBlackThemeEnabled();

			if (color.Equals(Color.Transparent) ||
				(color.Equals(Color.Black) && dark) ||
				(color.Equals(Color.White) && !dark))
			{
				return StyleBase.Automatic;
			}

			return color.ToRGBHtml();
		}


		public bool UpdatePageColor(Page page, string color)
		{
			var element = page.Root
				.Elements(page.Namespace + "PageSettings")
				.FirstOrDefault();

			// this SHOULD be impossible
			if (element == null)
			{
				logger.WriteLine("PageColor failed because PageSettings not found!");
				return false;
			}

			var changed = false;

			var attribute = element.Attribute("color");
			if (attribute == null)
			{
				element.Add(new XAttribute("color", color));
				changed = true;
			}
			else if (attribute.Value != color)
			{
				attribute.Value = color;
				changed = true;
			}

			if (changed)
			{
				logger.Verbose($"page color set to {color} for {page.Title}");
			}
			else
			{
				logger.Verbose($"page color unchanged on {page.Title}");
			}

			return changed;
		}
	}
}
