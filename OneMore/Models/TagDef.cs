//************************************************************************************************
// Copyright © 2021 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Models
{
	using System.Xml.Linq;


	internal class TagDef : XElement
	{
		private int indexValue;


		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="original">Original TagDef to copy</param>
		public TagDef(XElement original)
			: base(original.GetNamespaceOfPrefix(OneNote.Prefix) + "TagDef",
				  original.Attributes())
		{
			IndexValue = int.Parse(original.Attribute("index").Value);
		}


		public string Index => Attribute("index").Value;


		public int IndexValue
		{
			get => indexValue;

			set
			{
				indexValue = value;
				Attribute("index").Value = value.ToString();
			}
		}


		public string Symbol => Attribute("symbol").Value;


		public override bool Equals(object obj)
		{
			if (obj is XElement other)
			{
				// this is all we care about; OneNote allows exactly one of each symbol, no more
				return Attribute("symbol").Value.Equals(other.Attribute("symbol").Value);
			}

			return false;
		}


		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
