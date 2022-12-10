//************************************************************************************************
// Copyright © 2022 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	internal class PageReader
	{
		#region Types

		private sealed class DeepNode
		{
			public XElement Element;
			public int Depth;
		}

		#endregion Types

		private Page page;
		private XNamespace ns;


		/// <summary>
		/// Initialize a new editor for the given page
		/// </summary>
		/// <param name="page"></param>
		public PageReader(Page page)
		{
			this.page = page;
			ns = page.Namespace;
		}


		/// <summary>
		/// Removes the selected content on the current page, wraps it inside a new OEChildren
		/// container, and returns the container.
		/// </summary>
		/// <returns>A new OEChildren element containing the extracted content</returns>
		/// <remarks>
		/// Normalizes the content to include selected items in a partially selected bullet or
		/// number list and excludes the unselected items. Similarlly, this applies to indented
		/// paragraphs where not all sibilings or even child thereof are selected.
		/// </remarks>
		public XElement ExtractSelectedContent()
		{
			var content = new XElement(ns + "OEChildren");

			var selections = page.Root.Elements(ns+ "Outline")
				.Descendants(ns+ "T")
				.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
				.Select(e => new DeepNode
				{
					Element = e,
					Depth = e.Ancestors().Count()
				})
				.ToList();

			if (!selections.Any())
			{
				// no T runs selected but check for selected images
				selections = page.Root.Elements(ns + "Outline")
					.Descendants(ns + "Image")
					.Where(e => e.Attributes().Any(a => a.Name == "selected" && a.Value == "all"))
					.Select(e => new DeepNode
					{
						Element = e,
						Depth = e.Ancestors().Count()
					})
					.ToList();
			}

			if (!selections.Any())
			{
				return content;
			}

			var minDepth = selections.Min(e => e.Depth);

			return content;
		}
	}
}
