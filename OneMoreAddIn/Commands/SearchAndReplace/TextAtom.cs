//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System.Xml.Linq;


	/// <summary>
	/// An IAtom wrapper of an XText node
	/// </summary>
	internal class TextAtom : IAtom
	{
		private readonly XText text;


		public TextAtom(XNode content)
		{
			text = content as XText;
		}


		public int Length => text.Value.Length;


		public bool Empty() => string.IsNullOrEmpty(text.Value);


		public void Append(string s)
		{
			text.Value += s;
		}


		public string Extract(int index, int length)
		{
			if (Empty()) { return string.Empty; }
			var s = text.Value.Substring(index, length);
			text.Value = text.Value.Remove(index, length);
			return s;
		}


		public void Replace(int index, int length, string replacement)
		{
			text.Value = text.Value.Remove(index, length).Insert(index, replacement);
		}
	}
}
