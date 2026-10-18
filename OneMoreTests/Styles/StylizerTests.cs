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


		private const string MathML =
			"<!--[if mathML]><math><mi>x</mi><mo>--</mo><mi>y</mi></math><![endif]-->";


		[TestMethod]
		public void TryGetWrapper_ValidComment_KeepsSpanAndComment()
		{
			var cdata = MakeCData("<span style='font-weight:bold'>x</span><!-- note -->");

			var wrapper = Stylizer.TryGetWrapper(cdata);

			Assert.IsNotNull(wrapper);
			Assert.AreEqual(1, wrapper.Elements("span").Count());
			Assert.AreEqual(1, wrapper.Nodes().OfType<XComment>().Count());
		}


		[TestMethod]
		public void TryGetWrapper_MathML_ReturnsNull()
		{
			var cdata = MakeCData(MathML);

			Assert.IsNull(Stylizer.TryGetWrapper(cdata));
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

			// unparsable content is left untouched
			Assert.IsTrue(cdata.Value.Contains("<!-- a -- b -->"));
		}


		[TestMethod]
		public void Clear_ValidComment_KeepsCommentMarkers()
		{
			var cdata = MakeCData("<span style='color:#FF0000'>x</span><!-- note -->");
			var element = (XElement)cdata.Parent;

			var stylizer = new Stylizer(new Style());
			stylizer.Clear(element, Stylizer.Clearing.All);

			Assert.IsTrue(cdata.Value.Contains("<!-- note -->"));
		}


		[TestMethod]
		public void Clear_MathML_IsUnchanged()
		{
			// the word "span" forces Clear to inspect the CDATA
			var content = MathML.Replace("<mi>x</mi>", "<mi>span</mi>");
			var cdata = MakeCData(content);
			var element = (XElement)cdata.Parent;

			var stylizer = new Stylizer(new Style());
			stylizer.Clear(element, Stylizer.Clearing.All);

			Assert.AreEqual(content, cdata.Value);
		}


		[TestMethod]
		public void ApplyStyle_MathML_IsUnchanged()
		{
			var cdata = MakeCData(MathML);
			var element = (XElement)cdata.Parent;

			var stylizer = new Stylizer(new Style { Color = "#FF0000" });
			stylizer.ApplyStyle(element);

			Assert.AreEqual(MathML, cdata.Value);
		}
	}
}
