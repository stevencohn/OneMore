//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using River.OneMoreAddIn.UI;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using Resx = Properties.Resources;


	/// <summary>
	/// A small popup showing the hierarchy breadcrumb path of the current page. Each segment
	/// is a dropdown listing its siblings (other notebooks, section groups, sections, or pages
	/// at that same level); picking one navigates the active OneNote window to that object and
	/// refreshes the breadcrumb in place without closing this popup.
	/// </summary>
	internal partial class WhereAmIWindow : MoreForm
	{
		private const string RightArrow = "→";
		private const string Caret = " ▼";
		private const string Ellipsis = "…";
		private const int MinWidth = 400;
		private const int HorizontalPadding = 40; // Padding.Left + Padding.Right

		private readonly int maxDesktopWidth;
		private List<OneNote.HierarchyNode> segments;


		public WhereAmIWindow(List<OneNote.HierarchyNode> segments, int maxDesktopWidth)
		{
			InitializeComponent();

			if (NeedsLocalizing())
			{
				Text = Resx.WhereAmIWindow_Text;
				Localize(new[] { "cancelButton=word_Cancel", "copyButton=word_Copy" });
			}

			this.maxDesktopWidth = maxDesktopWidth;

			// CancelButton only wires up Escape to click this button; for a modeless window
			// (Show(), not ShowDialog()) setting DialogResult alone does not close the form,
			// so the actual close must happen explicitly here
			cancelButton.Click += (s, e) => Close();

			copyButton.Click += async (s, e) => await CopyBreadcrumb();

			// keep this popup above the ONENOTE window at all times
			TopMost = true;

			RebuildBreadcrumb(segments);
		}


		protected override void OnActivated(EventArgs e)
		{
			// MoreForm.OnActivated calls Elevate(false) for modeless forms, which resets
			// TopMost to false at the end; reassert it so this popup stays on top
			base.OnActivated(e);
			TopMost = true;
		}


		private void RebuildBreadcrumb(List<OneNote.HierarchyNode> newSegments)
		{
			segments = newSegments;

			var stale = flowPanel.Controls.Cast<Control>().ToList();
			flowPanel.SuspendLayout();
			flowPanel.Controls.Clear();
			foreach (var control in stale)
			{
				control.Dispose();
			}

			var font = Font;
			var caretWidth = TextRenderer.MeasureText(Caret, font).Width;
			var separatorText = $" {RightArrow} ";
			var separatorWidth = TextRenderer.MeasureText(separatorText, font).Width;

			var maxContentWidth = Math.Max(MinWidth, maxDesktopWidth) - HorizontalPadding;

			// measure every segment up front so only the final (page) segment absorbs truncation

			var names = new List<string>(segments.Count);
			var fixedWidth = 0;

			for (var i = 0; i < segments.Count; i++)
			{
				names.Add(segments[i].Name);

				if (i < segments.Count - 1)
				{
					fixedWidth += TextRenderer.MeasureText(segments[i].Name, font).Width +
						caretWidth + separatorWidth;
				}
			}

			var lastIndex = segments.Count - 1;
			var lastWidth = TextRenderer.MeasureText(names[lastIndex], font).Width + caretWidth;

			if (fixedWidth + lastWidth > maxContentWidth)
			{
				var available = Math.Max(0, maxContentWidth - fixedWidth - caretWidth);
				names[lastIndex] = TruncateToWidth(names[lastIndex], font, available);
				lastWidth = TextRenderer.MeasureText(names[lastIndex], font).Width + caretWidth;
			}

			for (var i = 0; i < segments.Count; i++)
			{
				var index = i; // capture for closures below

				var link = new MoreLinkLabel
				{
					AutoSize = true,
					Text = names[i] + Caret,
					Margin = new Padding(0, 4, 0, 4)
				};

				link.LinkClicked += (s, e) => ShowDropdown(index, link);
				flowPanel.Controls.Add(link);

				if (i < lastIndex)
				{
					var arrow = new MoreLabel
					{
						AutoSize = true,
						Text = separatorText,
						Margin = new Padding(0, 4, 0, 4)
					};

					flowPanel.Controls.Add(arrow);
				}
			}

			flowPanel.ResumeLayout();

			var contentWidth = fixedWidth + lastWidth;
			var clientWidth = Math.Min(
				Math.Max(contentWidth + HorizontalPadding, MinWidth),
				Math.Max(maxDesktopWidth, MinWidth));

			ClientSize = new Size(clientWidth, ClientSize.Height);

			// changing ClientSize slides the Bottom|Right-anchored cancelButton to its new X;
			// that anchor reposition can leave a stale paint of it at the old location (the
			// owner-drawn MoreButton clears only its own clip rect in OnPaint, so the vacated
			// area doesn't get erased on its own) - force the whole form to repaint cleanly
			Invalidate(true);
		}


		private static string TruncateToWidth(string text, Font font, int maxWidth)
		{
			if (TextRenderer.MeasureText(text, font).Width <= maxWidth)
			{
				return text;
			}

			var truncated = text;
			while (truncated.Length > 1 &&
				TextRenderer.MeasureText(truncated + Ellipsis, font).Width > maxWidth)
			{
				truncated = truncated.Substring(0, truncated.Length - 1);
			}

			return truncated.Length <= 1 ? Ellipsis : truncated + Ellipsis;
		}


		private async void ShowDropdown(int index, MoreLinkLabel link)
		{
			List<OneNote.HierarchyNode> siblings;

			await using (var one = new OneNote())
			{
				siblings = await GetSiblings(one, index);
			}

			if (siblings.Count == 0)
			{
				return;
			}

			var menu = new MoreContextMenuStrip();

			foreach (var node in siblings.OrderBy(n => n.Name, StringComparer.CurrentCultureIgnoreCase))
			{
				var item = new MoreMenuItem(node.Name, null, async (s, e) => await SelectSibling(index, node))
				{
					Checked = node.Id == segments[index].Id
				};

				menu.Items.Add(item);
			}

			menu.Show(link, new Point(0, link.Height));
		}


		private async Task<List<OneNote.HierarchyNode>> GetSiblings(OneNote one, int index)
		{
			var node = segments[index];

			switch (node.NodeType)
			{
				case OneNote.NodeType.Notebook:
					return await one.GetScopedNodes(string.Empty, OneNote.Scope.Notebooks);

				// "current scope" for sections is the whole enclosing notebook, so the user can
				// jump directly to any section regardless of which section group nests it
				case OneNote.NodeType.Section:
					return await one.GetScopedNodes(segments[0].Id, OneNote.Scope.Sections);

				case OneNote.NodeType.Page:
					return await one.GetScopedNodes(segments[index - 1].Id, OneNote.Scope.Pages);

				default: // SectionGroup: sibling section groups under its own parent container
					var children = await one.GetScopedNodes(segments[index - 1].Id, OneNote.Scope.Children);
					return children.Where(n => n.NodeType == OneNote.NodeType.SectionGroup).ToList();
			}
		}


		private async Task SelectSibling(int index, OneNote.HierarchyNode node)
		{
			List<OneNote.HierarchyNode> breadcrumb;

			await using (var one = new OneNote())
			{
				// navigate by raw ID rather than a constructed onenote: hyperlink; the
				// hyperlink path (GetHyperlinkToObject + NavigateToUrl) can fail to resolve
				// local, non-cloud-hosted notebooks, while the ID-based overload works
				// uniformly for local and cloud notebooks alike
				await one.NavigateTo(node.Id, string.Empty);

				// NavigateTo needs a moment to settle before the resulting current page
				// (e.g. the last-visited page in a newly selected notebook/section) is queryable
				await Task.Delay(150);

				breadcrumb = await one.GetPageBreadcrumb();
			}

			// trust the re-derived breadcrumb only if it actually descends from the picked node;
			// otherwise NavigateTo left the previous, unrelated page current (e.g. an empty
			// notebook/section group with nothing to navigate into) and re-deriving from it
			// would silently discard the user's selection
			if (breadcrumb is null || !breadcrumb.Any(s => s.Id == node.Id))
			{
				breadcrumb = segments.Take(index).ToList();
				breadcrumb.Add(node);
			}

			RebuildBreadcrumb(breadcrumb);

			// NavigateTo can bring the ONENOTE window to the foreground, burying this popup;
			// keepTop=true re-asserts TopMost since this call happens outside of OnActivated
			Elevate(true);
		}


		private async Task CopyBreadcrumb()
		{
			// build from segment data, not the MoreLinkLabel controls' Text, since their
			// visible text includes the display-only " ▼" dropdown caret
			var html = string.Join(" → ",
				segments.Select(s => $"<a href=\"{s.Link}\">{s.Name}</a>"));

			var text = string.Join(" → ", segments.Select(s => s.Name));

			var board = new ClipboardProvider();
			board.Stash(System.Windows.TextDataFormat.Html,
				ClipboardProvider.WrapWithHtmlPreamble(html));
			board.Stash(System.Windows.TextDataFormat.Text, text);
			board.Stash(System.Windows.TextDataFormat.UnicodeText, text);

			var success = await board.RestoreState();
			if (!success)
			{
				MoreMessageBox.ShowError(this, Resx.Clipboard_locked);
			}
		}
	}
}
