//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Globalization;
	using System.Xml.Linq;


	/// <summary>
	/// Abstract base for all PageModels wrapper nodes. Holds the underlying XElement and
	/// provides typed attribute accessors. All mutations write directly to the element so the
	/// tree can be serialized at any time without a separate conversion step.
	/// </summary>
	internal abstract class OneNoteNode
	{
		internal static readonly XNamespace NS =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		protected readonly XElement el;


		protected OneNoteNode(XElement element) => el = element;


		/// <summary>The underlying XElement for direct XML access.</summary>
		public XElement Element => el;


		/// <summary>Creates a new element in the OneNote namespace.</summary>
		protected static XElement E(string localName, params object[] content)
			=> new XElement(NS + localName, content);


		protected string Attr(string name) => el.Attribute(name)?.Value;


		protected void Attr(string name, string value)
		{
			var attr = el.Attribute(name);
			if (value is null)
				attr?.Remove();
			else if (attr is null)
				el.Add(new XAttribute(name, value));
			else
				attr.Value = value;
		}


		protected bool? AttrBool(string name)
		{
			var v = Attr(name);
			if (v is null) return null;
			return bool.TryParse(v, out var b) ? b : null;
		}


		protected void AttrBool(string name, bool? value)
		{
			if (value is null) Attr(name, null);
			else Attr(name, value.Value ? "true" : "false");
		}


		protected int? AttrInt(string name)
		{
			var v = Attr(name);
			return int.TryParse(v, out var n) ? (int?)n : null;
		}


		protected void AttrInt(string name, int? value)
			=> Attr(name, value?.ToString());


		protected double? AttrDouble(string name)
		{
			var v = Attr(name);
			return double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
				? d : (double?)null;
		}


		protected void AttrDouble(string name, double? value)
			=> Attr(name, value?.ToString("F1", CultureInfo.InvariantCulture));


		// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
		// PageObject attributes — present on most content-bearing nodes

		/// <summary>OneNote object identifier. Unique within the page.</summary>
		public string ObjectId
		{
			get => Attr("objectID");
			set => Attr("objectID", value);
		}


		// EditedByAttributes — author/timestamp metadata present on OE, Table, Row, Cell

		public string Author
		{
			get => Attr("author");
			set => Attr("author", value);
		}


		public string AuthorInitials
		{
			get => Attr("authorInitials");
			set => Attr("authorInitials", value);
		}


		public string LastModifiedBy
		{
			get => Attr("lastModifiedBy");
			set => Attr("lastModifiedBy", value);
		}


		public string LastModifiedByInitials
		{
			get => Attr("lastModifiedByInitials");
			set => Attr("lastModifiedByInitials", value);
		}


		/// <summary>ISO 8601 timestamp string as stored by OneNote.</summary>
		public string LastModifiedTime
		{
			get => Attr("lastModifiedTime");
			set => Attr("lastModifiedTime", value);
		}


		/// <summary>ISO 8601 timestamp string as stored by OneNote.</summary>
		public string CreationTime
		{
			get => Attr("creationTime");
			set => Attr("creationTime", value);
		}
	}
}
