//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
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


		public string Value => text.Value;


		public void Append(string s)
		{
			text.Value += s;
		}


		public void Remove(int index, int length)
		{
			if (!Empty())
			{
				var len = Math.Min(length, text.Value.Length - index);
				text.Value = text.Value.Remove(index, len);
			}
		}


		public void Replace(int index, int length, string replacement)
		{
			text.Value = text.Value.Remove(index, length).Insert(index, replacement);
		}
	}
}
