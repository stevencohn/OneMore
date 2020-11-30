//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System.Xml;
	using System.Xml.Linq;


	internal static class AtomicFactory
	{

		/// <summary>
		/// Create an appropriate IAtom wrapper of the given node.
		/// </summary>
		/// <param name="node">An XElement or XText node</param>
		/// <returns></returns>
		public static IAtom MakeAtom(XNode node)
		{
			return node.NodeType == XmlNodeType.Text
				? new TextAtom(node) as IAtom
				: new ElementAtom(node);
		}
	}
}
