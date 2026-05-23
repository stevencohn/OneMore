//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.PageModels
{
	using System.Globalization;
	using System.Xml.Linq;


	/// <summary>Wraps the one:Bullet element within a one:List on an OE paragraph.</summary>
	internal sealed class BulletDef : OneNoteNode
	{
		internal BulletDef(XElement el) : base(el) { }


		/// <summary>Creates a new Bullet element with the given character code and font size.</summary>
		public static BulletDef Create(string bulletId = "2", double fontSize = 11.0)
			=> new BulletDef(E("Bullet",
				new XAttribute("bullet", bulletId),
				new XAttribute("fontSize", fontSize.ToString("F1", CultureInfo.InvariantCulture))));


		/// <summary>
		/// OneNote bullet character code. Common values: 2 = •, 3 = ◦, 4 = ▪,
		/// 5 = –, 6 = >, 7 = ☐, 8 = ★, 26 = ➔
		/// </summary>
		public string BulletId
		{
			get => Attr("bullet");
			set => Attr("bullet", value);
		}


		public double? FontSize
		{
			get => AttrDouble("fontSize");
			set => AttrDouble("fontSize", value);
		}


		/// <summary>Bullet character color (#RRGGBB or "automatic").</summary>
		public string FontColor
		{
			get { var v = Attr("fontColor"); return v == "automatic" ? null : v; }
			set => Attr("fontColor", value ?? "automatic");
		}
	}
}
