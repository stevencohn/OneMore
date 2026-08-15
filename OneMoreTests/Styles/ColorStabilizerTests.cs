//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Styles
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Styles;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;

	[TestClass]
	public class ColorStabilizerTests
	{
		private const string Xmlns =
			"xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\"";

		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement BuildRoot(string quickStyleColor, string localStyle)
		{
			return XElement.Parse(
				$"<one:Page {Xmlns}>" +
				$"<one:QuickStyleDef index=\"1\" name=\"p\" fontColor=\"{quickStyleColor}\" " +
				"font=\"Segoe UI\" fontSize=\"11.0\" />" +
				"<one:Outline><one:OEChildren>" +
				$"<one:OE quickStyleIndex=\"1\" style=\"{localStyle}\">" +
				"<one:T><![CDATA[bullet text]]></one:T>" +
				"</one:OE>" +
				"</one:OEChildren></one:Outline>" +
				"</one:Page>");
		}


		[TestMethod]
		public void Stabilize_LowContrastQuickStyleColor_InjectsContrastingColor()
		{
			var root = BuildRoot("#FFFFFF", "font-family:'Segoe UI';font-size:11.0pt;");

			var patched = ColorStabilizer.Stabilize(root, Ns, Color.White, Color.Black);

			Assert.AreEqual(1, patched);
			var style = (string)root.Descendants(Ns + "OE").First().Attribute("style");
			StringAssert.Contains(style, "color:#000000");
		}


		[TestMethod]
		public void Stabilize_AutomaticQuickStyleColor_DoesNotPatch()
		{
			var root = BuildRoot("automatic", "font-family:'Segoe UI';font-size:11.0pt;");

			var patched = ColorStabilizer.Stabilize(root, Ns, Color.White, Color.Black);

			Assert.AreEqual(0, patched);
		}


		[TestMethod]
		public void Stabilize_LocalStyleAlreadyHasColor_DoesNotPatch()
		{
			var root = BuildRoot("#FFFFFF",
				"font-family:'Segoe UI';font-size:11.0pt;color:#123456;");

			var patched = ColorStabilizer.Stabilize(root, Ns, Color.White, Color.Black);

			Assert.AreEqual(0, patched);
			var style = (string)root.Descendants(Ns + "OE").First().Attribute("style");
			StringAssert.Contains(style, "color:#123456");
		}


		[TestMethod]
		public void Stabilize_NoLocalStyle_DoesNotPatch()
		{
			var root = XElement.Parse(
				$"<one:Page {Xmlns}>" +
				"<one:QuickStyleDef index=\"1\" name=\"p\" fontColor=\"#FFFFFF\" " +
				"font=\"Segoe UI\" fontSize=\"11.0\" />" +
				"<one:Outline><one:OEChildren>" +
				"<one:OE quickStyleIndex=\"1\">" +
				"<one:T><![CDATA[bullet text]]></one:T>" +
				"</one:OE>" +
				"</one:OEChildren></one:Outline>" +
				"</one:Page>");

			var patched = ColorStabilizer.Stabilize(root, Ns, Color.White, Color.Black);

			Assert.AreEqual(0, patched);
		}


		[TestMethod]
		public void Stabilize_HighContrastQuickStyleColor_DoesNotPatch()
		{
			// black quick-style text on a white page is already visible
			var root = BuildRoot("#000000", "font-family:'Segoe UI';font-size:11.0pt;");

			var patched = ColorStabilizer.Stabilize(root, Ns, Color.White, Color.Black);

			Assert.AreEqual(0, patched);
		}
	}
}
