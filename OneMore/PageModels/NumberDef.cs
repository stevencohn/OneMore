//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Globalization;
	using System.Xml.Linq;


	/// <summary>Wraps the one:Number element within a one:List on an OE paragraph.</summary>
	internal sealed class NumberDef : OneNoteNode
	{
		internal NumberDef(XElement el) : base(el) { }


		/// <summary>Creates a new Number element.</summary>
		public static NumberDef Create(
			int sequence = 0,
			string format = "##.",
			string font = "Calibri",
			double fontSize = 11.0)
			=> new NumberDef(E("Number",
				new XAttribute("numberSequence", sequence),
				new XAttribute("numberFormat", format),
				new XAttribute("font", font),
				new XAttribute("fontSize", fontSize.ToString("F1", CultureInfo.InvariantCulture))));


		/// <summary>
		/// Zero-based position of this item within the sequence. OneNote uses this to render
		/// the visible number (1, 2, 3…) and to restart numbering mid-list.
		/// </summary>
		public int NumberSequence
		{
			get => AttrInt("numberSequence") ?? 0;
			set => AttrInt("numberSequence", value);
		}


		/// <summary>
		/// Format string for the rendered number, e.g. "##." → "1.", "##)" → "1)",
		/// "##.##." → "1.1.", "(##)" → "(1)".
		/// </summary>
		public string NumberFormat
		{
			get => Attr("numberFormat");
			set => Attr("numberFormat", value);
		}


		/// <summary>
		/// When set, this item restarts the count from this value instead of continuing
		/// the sequence from the previous item.
		/// </summary>
		public int? RestartAt
		{
			get => AttrInt("restartNumberingAt");
			set => AttrInt("restartNumberingAt", value);
		}


		public string Font
		{
			get => Attr("font");
			set => Attr("font", value);
		}


		public double? FontSize
		{
			get => AttrDouble("fontSize");
			set => AttrDouble("fontSize", value);
		}


		public string FontColor
		{
			get { var v = Attr("fontColor"); return v == "automatic" ? null : v; }
			set => Attr("fontColor", value ?? "automatic");
		}


		public bool? Bold
		{
			get => AttrBool("bold");
			set => AttrBool("bold", value);
		}


		public bool? Italic
		{
			get => AttrBool("italic");
			set => AttrBool("italic", value);
		}


		/// <summary>BCP 47 language tag for number rendering, e.g. "en-US".</summary>
		public string Language
		{
			get => Attr("language");
			set => Attr("language", value);
		}


		/// <summary>Optional literal text appended after the number character.</summary>
		public string Text
		{
			get => Attr("text");
			set => Attr("text", value);
		}
	}
}
