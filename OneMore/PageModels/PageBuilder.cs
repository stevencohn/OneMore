//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Collections.Generic;
	using System.Xml.Linq;


	/// <summary>
	/// Fluent builder for constructing OneNote pages programmatically.
	///
	/// Usage:
	///   var page = PageBuilder.New("My Title")
	///       .Heading1("Introduction")
	///       .Paragraph("Normal text.")
	///       .BulletList(b => b.Item("First").Item("Second"))
	///       .Table(2, 3, (t, r, c) => t.Cell(r, c).SetText($"R{r}C{c}"))
	///       .Build();
	/// </summary>
	internal sealed class PageBuilder
	{
		private readonly PageNode page;
		private readonly OutlineNode outline;
		private readonly OEChildrenNode root;

		// scope stack for nested lists
		private readonly Stack<OEChildrenNode> scopeStack = new Stack<OEChildrenNode>();


		private PageBuilder(string title)
		{
			var rootEl = new XElement(OneNoteNode.NS + "Page",
				new XAttribute("xmlns:one", OneNoteNode.NS.NamespaceName));
			page = PageNode.FromElement(rootEl);
			page.Title = title;
			outline = page.AddOutline();
			root = outline.Root;
			scopeStack.Push(root);
		}


		private OEChildrenNode CurrentScope => scopeStack.Peek();


		/// <summary>Creates a new builder with the given page title.</summary>
		public static PageBuilder New(string title = "") => new PageBuilder(title);


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Content appenders

		/// <summary>Appends a paragraph with a quick style name (e.g. "h1").</summary>
		public PageBuilder Styled(string text, string quickStyleName)
		{
			var idx = page.QuickStyles.FindByName(quickStyleName);
			var oe = CurrentScope.AppendItem(text);
			if (idx >= 0) oe.QuickStyleIndex = idx;
			return this;
		}


		public PageBuilder Heading1(string text) => Styled(text, "h1");
		public PageBuilder Heading2(string text) => Styled(text, "h2");
		public PageBuilder Heading3(string text) => Styled(text, "h3");
		public PageBuilder Paragraph(string text) => Styled(text, "p");


		/// <summary>Appends a paragraph with a custom style.</summary>
		public PageBuilder Paragraph(string text, Action<StyleString> configure)
		{
			var oe = CurrentScope.AppendItem(text);
			if (configure is not null)
			{
				var style = oe.Style;
				configure(style);
				oe.Style = style;
			}
			return this;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Lists

		/// <summary>Appends a bullet list. Use the configure action to add items.</summary>
		public PageBuilder BulletList(Action<ListBuilder> configure)
		{
			var listRoot = CurrentScope.AppendItem().EnsureChildren();
			var builder = new ListBuilder(listRoot, isBullet: true);
			configure(builder);
			return this;
		}


		/// <summary>Appends a numbered list. Use the configure action to add items.</summary>
		public PageBuilder NumberedList(Action<ListBuilder> configure)
		{
			var listRoot = CurrentScope.AppendItem().EnsureChildren();
			var builder = new ListBuilder(listRoot, isBullet: false);
			configure(builder);
			return this;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Table

		/// <summary>
		/// Appends a table. The configure action receives the TableNode, row index, and
		/// column index for each cell in row-major order.
		/// </summary>
		public PageBuilder Table(int rows, int columns,
			Action<TableNode, int, int> configure = null,
			double columnWidthPx = 120)
		{
			var oe = new XElement(OneNoteNode.NS + "OE");
			CurrentScope.Element.Add(oe);
			var oeNode = new OENode(oe);
			var table = oeNode.InsertTable(rows, columns, columnWidthPx);

			if (configure is not null)
			{
				for (int r = 0; r < rows; r++)
					for (int c = 0; c < columns; c++)
						configure(table, r, c);
			}

			return this;
		}


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Output

		public PageNode Build() => page;


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// Inner list builder

		/// <summary>Fluent helper for building list items within a BulletList or NumberedList.</summary>
		internal sealed class ListBuilder
		{
			private readonly OEChildrenNode container;
			private readonly bool isBullet;
			private int seq;


			internal ListBuilder(OEChildrenNode container, bool isBullet)
			{
				this.container = container;
				this.isBullet = isBullet;
			}


			/// <summary>Appends a list item. Optionally adds sub-items via the configure action.</summary>
			public ListBuilder Item(string text, Action<ListBuilder> configure = null)
			{
				var oe = container.AppendItem(text);
				if (isBullet)
					oe.SetBullet("2", "11.0");
				else
					oe.SetNumber(seq++, "##.", "Calibri");

				if (configure is not null)
				{
					var sub = oe.EnsureChildren();
					configure(new ListBuilder(sub, isBullet));
				}

				return this;
			}
		}
	}
}
