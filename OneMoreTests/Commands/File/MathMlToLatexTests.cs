//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.File
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using River.OneMoreAddIn.Models;
	using River.OneMoreAddIn.Tests.Builders;
	using System.Threading.Tasks;
	using System.Xml.Linq;

	[TestClass]
	public class MathMlToLatexTests : TestBase
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";

		private const string Mml = "xmlns=\"http://www.w3.org/1998/Math/MathML\"";


		private static string Convert(string body, out bool block, string attrs = "")
		{
			var ok = MathMlToLatex.TryConvert(
				$"<math {Mml} {attrs}>{body}</math>", out var latex, out block);

			Assert.IsTrue(ok);
			return latex;
		}


		[TestMethod]
		public void Superscript_AreaOfCircle()
		{
			// sample from a real OneNote page; &#120587; is math-italic pi
			var latex = Convert(
				"<mi>A</mi><mo>=</mo><mi>&#120587;</mi><msup><mi>r</mi><mn>2</mn></msup>",
				out var block);

			Assert.AreEqual(@"A=\pi r^{2}", latex);
			Assert.IsFalse(block);
		}


		[TestMethod]
		public void BlockDisplayIsDetected()
		{
			Convert("<mi>x</mi>", out var block, "display=\"block\"");
			Assert.IsTrue(block);
		}


		[TestMethod]
		public void Fraction()
		{
			var latex = Convert(
				"<mfrac><mi>a</mi><mrow><mi>b</mi><mo>+</mo><mn>1</mn></mrow></mfrac>", out _);

			Assert.AreEqual(@"\frac{a}{b+1}", latex);
		}


		[TestMethod]
		public void SquareRootAndNthRoot()
		{
			Assert.AreEqual(@"\sqrt{x}",
				Convert("<msqrt><mi>x</mi></msqrt>", out _));

			Assert.AreEqual(@"\sqrt[3]{x}",
				Convert("<mroot><mi>x</mi><mn>3</mn></mroot>", out _));
		}


		[TestMethod]
		public void SummationWithLimits()
		{
			var latex = Convert(
				"<munderover><mo>&#8721;</mo>" +
				"<mrow><mi>i</mi><mo>=</mo><mn>1</mn></mrow><mi>n</mi></munderover>" +
				"<msub><mi>x</mi><mi>i</mi></msub>", out _);

			Assert.AreEqual(@"\sum_{i=1}^{n}x_{i}", latex);
		}


		[TestMethod]
		public void FencedGroupUsesLeftRight()
		{
			var latex = Convert(
				"<mfenced><mrow><mi>a</mi><mo>+</mo><mi>b</mi></mrow></mfenced>", out _);

			Assert.AreEqual(@"\left( a+b \right)", latex);
		}


		[TestMethod]
		public void BracketedTableBecomesMatrix()
		{
			var latex = Convert(
				"<mfenced open=\"[\" close=\"]\"><mtable>" +
				"<mtr><mtd><mn>1</mn></mtd><mtd><mn>2</mn></mtd></mtr>" +
				"<mtr><mtd><mn>3</mn></mtd><mtd><mn>4</mn></mtd></mtr>" +
				"</mtable></mfenced>", out _);

			Assert.AreEqual(@"\begin{bmatrix} 1 & 2 \\ 3 & 4 \end{bmatrix}", latex);
		}


		[TestMethod]
		public void FunctionNamesAndSymbols()
		{
			var latex = Convert(
				"<mi>sin</mi><mi>&#952;</mi><mo>&#8804;</mo><mi>x</mi>", out _);

			Assert.AreEqual(@"\sin\theta\leq x", latex);
		}


		// the following use the shapes OneNote itself returned after re-saving the page

		[TestMethod]
		public void OneNote_LimitWithSplitFunctionName()
		{
			var latex = Convert(
				"<munder><mrow><mi>l</mi><mi>i</mi><mi>m</mi></mrow>" +
				"<mrow><mi>x</mi><mo>&#8594;</mo><mn>0</mn></mrow></munder>" +
				"<mfrac><mrow><mi>s</mi><mi>i</mi><mi>n</mi><mi>x</mi></mrow><mi>x</mi></mfrac>" +
				"<mo>=</mo><mn>1</mn>", out _);

			Assert.AreEqual(@"\lim_{x\to0}\frac{\sin x}{x}=1", latex);
		}


		[TestMethod]
		public void OneNote_NamedEntitySpaceAndIntegralLimits()
		{
			var latex = Convert(
				"<mrow><msubsup><mo>&#8747;</mo><mn>0</mn><mn>1</mn></msubsup><mi>f</mi></mrow>" +
				"<mo>&nbsp;</mo><mi>d</mi><mi>x</mi>", out _);

			Assert.AreEqual(@"\int_{0}^{1}f\,dx", latex);
		}


		[TestMethod]
		public void OneNote_BarAccentsAndParenthesizedBase()
		{
			Assert.AreEqual(@"\bar{y}",
				Convert("<mover accent=\"true\"><mi>y</mi><mo>-</mo></mover>", out _));

			Assert.AreEqual(@"\overline{AB}",
				Convert("<mover accent=\"true\"><mrow><mi>A</mi><mi>B</mi></mrow><mo>-</mo></mover>", out _));

			Assert.AreEqual(@"(\frac{a}{b})^{2}",
				Convert("<msup><mrow><mrow><mo>(</mo><mfrac><mi>a</mi><mi>b</mi></mfrac>" +
				"<mo>)</mo></mrow></mrow><mn>2</mn></msup>", out _));
		}


		[TestMethod]
		public void InvalidMarkupFails()
		{
			Assert.IsFalse(MathMlToLatex.TryConvert("<math><mi>", out _, out _));
		}


		private static Page BuildPage(string cdata)
		{
			var element = new PageBuilder("page-1", "Math Test")
				.WithElement(new XElement(Ns + "OE",
					new XElement(Ns + "T", new XCData(cdata))))
				.BuildElement();

			return new Page(XElement.Parse(element.ToString(SaveOptions.OmitDuplicateNamespaces)));
		}


		[TestMethod]
		public async Task Render_InlineEquation_BecomesDollarLatex()
		{
			var page = BuildPage(
				"<span style='font-family:Calibri' lang=en-US>Lorem ipsum. </span>" +
				$"<!--[if mathML]><math {Mml}><mi>A</mi><mo>=</mo><mi>&#120587;</mi>" +
				"<msup><mi>r</mi><mn>2</mn></msup></math><![endif]-->" +
				"<span style='font-family:Calibri' lang=en-US>  Duis aute.</span>");

			var content = page.Root.Element(Ns + "Outline").Element(Ns + "OEChildren");
			var markdown = await new MarkdownWriter(page, false).Render(content, includeTitle: false);

			StringAssert.Contains(markdown, @"Lorem ipsum. $A=\pi r^{2}$  Duis aute.");
		}


		[TestMethod]
		public async Task Render_BlockEquation_BecomesDoubleDollar()
		{
			var page = BuildPage(
				$"<!--[if mathML]><math {Mml} display=\"block\">" +
				"<msup><mi>x</mi><mn>2</mn></msup></math><![endif]-->");

			var content = page.Root.Element(Ns + "Outline").Element(Ns + "OEChildren");
			var markdown = await new MarkdownWriter(page, false).Render(content, includeTitle: false);

			StringAssert.Contains(markdown, "$$x^{2}$$");
		}
	}
}
