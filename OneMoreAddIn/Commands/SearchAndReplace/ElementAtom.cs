//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn
{
	using System;
	using System.Xml.Linq;


	/// <summary>
	/// An IAtom wrapper of an XElement node
	/// </summary>
	internal class ElementAtom : IAtom
	{
		private readonly XElement element;


		public ElementAtom(XNode content)
		{
			element = content as XElement;
		}


		public int Length => element.Value.Length;


		public bool Empty() => string.IsNullOrEmpty(element.Value);


		public void Append(string s)
		{
			element.Value += s;
		}


		public string Extract(int index, int length)
		{
			if (Empty()) { return string.Empty; }
			var l = Math.Min(length, element.Value.Length - index);
			var s = element.Value.Substring(index, l);
			element.Value = element.Value.Remove(index, l);
			return s;
		}


		public void Replace(int index, int length, string replacement)
		{
			element.Value = element.Value.Remove(index, length).Insert(index, replacement);
		}


		public void Remove()
		{
			element.Remove();
		}
	}
}
