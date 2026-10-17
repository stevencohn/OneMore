//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Styles
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Styles;
	using System.Linq;
	using System.Xml.Linq;

	[TestClass]
	public class StylizerTests
	{
		// issue 2769: a '--' inside an HTML comment in a CDATA is invalid XML and made
		// Stylizer.Clear throw XmlException from XElement.Parse

		private static XCData MakeCData(string content)
		{
			var cdata = new XCData(content);
			_ = new XElement("T", cdata);
			return cdata;
		}


		[TestMethod]
		public void TryGetWrapper_CommentWithDoubleDash_KeepsSpan()
		{
			var cdata = MakeCData("<span style='font-weight:bold'>x</span><!-- a -- b -->");

			var wrapper = Stylizer.TryGetWrapper(cdata);

			Assert.IsNotNull(wrapper);
			Assert.AreEqual(1, wrapper.Elements("span").Count());
			Assert.IsTrue(wrapper.Value.Contains("a -- b"));
		}


		[TestMethod]
		public void TryGetWrapper_Malformed_ReturnsNull()
		{
			var cdata = MakeCData("<span>x</div>");

			Assert.IsNull(Stylizer.TryGetWrapper(cdata));
		}


		[TestMethod]
		public void Clear_CommentWithDoubleDash_DoesNotThrow()
		{
			var cdata = MakeCData("<span style='color:#FF0000'>x</span><!-- a -- b -->");
			var element = (XElement)cdata.Parent;

			var stylizer = new Stylizer(new Style());
			stylizer.Clear(element, Stylizer.Clearing.All);
		}
	}
}
