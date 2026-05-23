//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;


	internal static class OneNoteExtensions
	{
		/// <summary>Calls action on each element in the sequence.</summary>
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
				action(item);
		}


		/// <summary>Filters OE paragraphs by quick style index.</summary>
		public static IEnumerable<OENode> WithQuickStyle(
			this IEnumerable<OENode> oes, int index)
			=> oes.Where(oe => oe.QuickStyleIndex == index);


		/// <summary>Filters OE paragraphs whose plain text matches the given regex pattern.</summary>
		public static IEnumerable<OENode> WithText(
			this IEnumerable<OENode> oes, string pattern)
			=> oes.Where(oe => Regex.IsMatch(oe.PlainText, pattern));


		/// <summary>Filters OE paragraphs that contain a table.</summary>
		public static IEnumerable<OENode> WithTable(this IEnumerable<OENode> oes)
			=> oes.Where(oe => oe.Table is not null);


		/// <summary>Filters OE paragraphs that contain an image.</summary>
		public static IEnumerable<OENode> WithImage(this IEnumerable<OENode> oes)
			=> oes.Where(oe => oe.Image is not null);


		/// <summary>Extracts the TableNode from each OE that contains one.</summary>
		public static IEnumerable<TableNode> SelectTables(this IEnumerable<OENode> oes)
			=> oes.Where(oe => oe.Table is not null).Select(oe => oe.Table);


		/// <summary>Extracts the ImageNode from each OE that contains one.</summary>
		public static IEnumerable<ImageNode> SelectImages(this IEnumerable<OENode> oes)
			=> oes.Where(oe => oe.Image is not null).Select(oe => oe.Image);
	}
}
