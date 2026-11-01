//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.File
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Linq;
	using System.Xml.Linq;

	[TestClass]
	public class LatexToMathMlTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		[TestMethod]
		public void AreaOfCircle_MatchesOneNoteShape()
		{
			Assert.IsTrue(LatexToMathMl.TryConvert(@"A=\pi r^2", false, out var mathml));

			// same structure OneNote itself produced for this equation
			Assert.AreEqual(
				"<math xmlns=\"http://www.w3.org/1998/Math/MathML\">" +
				"<mi>A</mi><mo>=</mo><mi>&#120587;</mi>" +
				"<msup><mi>r</mi><mn>2</mn></msup></math>",
				mathml);
		}


		[TestMethod]
		public void BlockSetsDisplayAttribute()
		{
			Assert.IsTrue(LatexToMathMl.TryConvert("x", true, out var mathml));
			StringAssert.Contains(mathml, "display=\"block\"");
		}


		[TestMethod]
		public void UnknownCommandFails()
		{
			Assert.IsFalse(LatexToMathMl.TryConvert(@"\notacommand{x}", false, out _));
			Assert.IsFalse(LatexToMathMl.TryConvert(@"\frac{a}", false, out _));
			Assert.IsFalse(LatexToMathMl.TryConvert(@"{x", false, out _));
		}


		// LaTeX -> MathML -> LaTeX should return a normalized form of the input
		[DataTestMethod]
		[DataRow(@"x^2", @"x^{2}")]
		[DataRow(@"\frac{a}{b+1}", @"\frac{a}{b+1}")]
		[DataRow(@"\sqrt{x}", @"\sqrt{x}")]
		[DataRow(@"\sqrt[3]{x}", @"\sqrt[3]{x}")]
		[DataRow(@"\sum_{i=1}^{n} x_i", @"\sum_{i=1}^{n}x_{i}")]
		[DataRow(@"a \leq b", @"a\leq b")]
		[DataRow(@"\sin\theta", @"\sin\theta")]
		[DataRow(@"\left( a+b \right)", @"(a+b)")]
		[DataRow(@"\begin{bmatrix} 1 & 2 \\ 3 & 4 \end{bmatrix}",
			@"\begin{bmatrix} 1 & 2 \\ 3 & 4 \end{bmatrix}")]
		[DataRow(@"\hat{x}", @"\hat{x}")]
		[DataRow(@"\mathbb{R}", @"\mathbb{R}")]
		public void RoundTrip(string latex, string expected)
		{
			Assert.IsTrue(LatexToMathMl.TryConvert(latex, false, out var mathml));
			Assert.IsTrue(MathMlToLatex.TryConvert(mathml, out var result, out _));
			Assert.AreEqual(expected, result);
		}


		private static Page BuildPage(string cdata)
		{
			var element = new PageBuilder("page-1", "Math Test")
				.WithElement(new XElement(Ns + "OE",
					new XElement(Ns + "T", new XCData(cdata))))
				.BuildElement();

			return new Page(XElement.Parse(element.ToString(SaveOptions.OmitDuplicateNamespaces)));
		}


		// PageBuilder also emits a Title OE, so look only inside the Outline
		private static XElement ContentOe(Page page)
		{
			return page.Root.Element(Ns + "Outline").Descendants(Ns + "OE").First();
		}


		[TestMethod]
		public void RewriteMath_InlineInsideStyledSpan()
		{
			var page = BuildPage(
				@"<span style='font-family:Calibri'>Lorem \(A=\pi r^2\) Duis.</span>");

			var oe = ContentOe(page);
			new MarkdownConverter(page).RewriteMath(new[] { oe });

			var value = oe.Element(Ns + "T").GetCData().Value;

			StringAssert.StartsWith(value, "<span style='font-family:Calibri'>Lorem </span><!--[if mathML]>");
			StringAssert.Contains(value, "<msup><mi>r</mi><mn>2</mn></msup>");
			StringAssert.EndsWith(value, "<![endif]--><span style='font-family:Calibri'> Duis.</span>");
		}


		[TestMethod]
		public void RewriteMath_BlockWhenSoleContent()
		{
			var page = BuildPage(@"<span style='font-family:Calibri'>\[x^2\]</span>");

			var oe = ContentOe(page);
			new MarkdownConverter(page).RewriteMath(new[] { oe });

			StringAssert.Contains(oe.Element(Ns + "T").GetCData().Value, "display=\"block\"");
		}


		[TestMethod]
		public void RewriteMath_HtmlEscapedLessThanAndAmpersandInCases()
		{
			// Markdig escapes & and < in the HTML it renders
			var page = BuildPage(
				@"\(f(x) = \begin{cases} x^2 &amp; x \geq 0 \\ -x &amp; x &lt; 0 \end{cases}\)");

			var oe = ContentOe(page);
			new MarkdownConverter(page).RewriteMath(new[] { oe });

			var value = oe.Element(Ns + "T").GetCData().Value;
			StringAssert.StartsWith(value, "<!--[if mathML]>");
			StringAssert.Contains(value, "<mtable>");
		}


		[TestMethod]
		public void RewriteMath_UnconvertibleLeftAsText()
		{
			var page = BuildPage(@"Keep \(\notacommand\) as is");

			var oe = ContentOe(page);
			new MarkdownConverter(page).RewriteMath(new[] { oe });

			Assert.AreEqual(@"Keep \(\notacommand\) as is", oe.Element(Ns + "T").GetCData().Value);
		}
	}
}
