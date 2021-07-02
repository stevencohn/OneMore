//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
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


		public string Value => element.Value;


		public void Append(string s)
		{
			element.Value += s;
		}


		public void Remove(int index, int length)
		{
			if (!Empty())
			{
				var len = Math.Min(length, element.Value.Length - index);
				element.Value = element.Value.Remove(index, len);
			}
		}


		public void Replace(int index, int length, string replacement)
		{
			element.Value = element.Value.Remove(index, length).Insert(index, replacement);
		}


		public void Replace(int index, int length, XElement replacement)
		{
			var first = element.FirstNode;
			if (first.NodeType == System.Xml.XmlNodeType.Text)
			{
				new TextAtom(first).Replace(index, length, replacement);
			}
			else
			{
				/*
				 * TODO: This is not correct!
				 * 
				 * given <a> aaa <i>some</i><b>text</b> zzz </a>
				 * when replacing "sometext"
				 * then then will be <a>replacement</a>
				 * instead of <a> aaa replacement zzz </a>
				 * 
				 * need to recurse?
				 * 
				 */
				first.ReplaceWith(replacement);
			}
		}
	}
}
