//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System;
	using System.Globalization;
	using System.Xml.Linq;


	/// <summary>
	/// Wraps a one:Tag element on an OE paragraph. A Tag references a TagDef by index and
	/// carries optional action-item state (completed, disabled, dates).
	/// </summary>
	internal sealed class TagNode : OneNoteNode
	{
		internal TagNode(XElement el) : base(el) { }


		/// <summary>Creates a new Tag element referencing the given TagDef index.</summary>
		public static TagNode Create(int tagDefIndex)
			=> new TagNode(E("Tag", new XAttribute("index", tagDefIndex)));


		/// <summary>Index into the page's TagDef list.</summary>
		public int Index => AttrInt("index") ?? 0;


		/// <summary>Whether this action-item tag has been checked off.</summary>
		public bool Completed
		{
			get => AttrBool("completed") == true;
			set => AttrBool("completed", value);
		}


		/// <summary>
		/// Whether this tag is disabled (greyed out in the UI). Disabled tags are not
		/// included in the To-Do summary page.
		/// </summary>
		public bool Disabled
		{
			get => AttrBool("disabled") == true;
			set => AttrBool("disabled", value);
		}


		/// <summary>When the tag was created. Null if the attribute is absent.</summary>
		public DateTime? CreationDate
		{
			get => ParseDate("creationDate");
			set => Attr("creationDate", FormatDate(value));
		}


		/// <summary>When the tag was marked complete. Null if not yet completed.</summary>
		public DateTime? CompletionDate
		{
			get => ParseDate("completionDate");
			set => Attr("completionDate", FormatDate(value));
		}


		private DateTime? ParseDate(string name)
		{
			var v = Attr(name);
			return v is not null && DateTime.TryParse(v,
				CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind, out var dt)
				? dt : (DateTime?)null;
		}


		private static string FormatDate(DateTime? dt)
			=> dt?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
	}
}
