//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Search
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml.Linq;


	[TestClass]
	public class FirstMatchSelectorTests
	{
		private static readonly XNamespace Ns =
			"http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static XElement MakeParagraph(string id, params string[] runs)
		{
			return new XElement(Ns + "OE",
				new XAttribute("objectID", id),
				runs.Select(r => new XElement(Ns + "T", new XCData(r))));
		}


		// mimics SearchAndReplaceEditor.FindAllMatches over the given paragraphs
		private static List<MatchInfo> FindAll(string pattern, params XElement[] paragraphs)
		{
			var regex = new Regex(pattern);
			var matches = new List<MatchInfo>();

			foreach (var oe in paragraphs)
			{
				foreach (var t in oe.Elements(Ns + "T"))
				{
					var text = t.GetCData().GetWrapper().Value;
					foreach (Match m in regex.Matches(text))
					{
						matches.Add(new MatchInfo
						{
							TElement = t,
							StartIndex = m.Index,
							Length = m.Length,
							MatchedText = m.Value,
							ParagraphObjectId = oe.Attribute("objectID").Value
						});
					}
				}
			}

			return matches;
		}


		[TestMethod]
		public void Select_SecondMatchInParagraph_IsIgnored()
		{
			var oe = MakeParagraph("{OE1}", "foo bar foo");

			var selected = FirstMatchSelector.Select(FindAll("foo", oe));

			Assert.AreEqual(1, selected.Count);

			var runs = oe.Elements(Ns + "T").ToList();
			Assert.AreEqual(2, runs.Count);

			Assert.AreEqual("all", runs[0].Attribute("selected")?.Value);
			Assert.AreEqual("foo", runs[0].GetCData().Value);
			Assert.IsNull(runs[1].Attribute("selected"));
			Assert.AreEqual(" bar foo", runs[1].GetCData().Value);
		}


		[TestMethod]
		public void Select_MatchesInSeveralParagraphs_SelectsOnePerParagraph()
		{
			var p1 = MakeParagraph("{OE1}", "a foo b foo");
			var p2 = MakeParagraph("{OE2}", "nothing here");
			var p3 = MakeParagraph("{OE3}", "plain", "second foo run foo");

			var selected = FirstMatchSelector.Select(FindAll("foo", p1, p2, p3));

			Assert.AreEqual(2, selected.Count);

			var all = new[] { p1, p2, p3 }
				.SelectMany(p => p.Elements(Ns + "T"))
				.Where(t => t.Attribute("selected")?.Value == "all")
				.ToList();

			Assert.AreEqual(2, all.Count);
			Assert.IsTrue(all.All(t => t.GetCData().Value == "foo"));
			Assert.AreEqual(3, p1.Elements(Ns + "T").Count());
			Assert.AreEqual(0, p2.Elements(Ns + "T").Count(t => t.Attribute("selected") is not null));
		}


		[TestMethod]
		public void Select_MatchWithinStyledSpan_KeepsStyleOnSelectedRun()
		{
			var oe = MakeParagraph("{OE1}", "x <span style='font-weight:bold'>bold foo</span> y");

			var selected = FirstMatchSelector.Select(FindAll("foo", oe));

			Assert.AreEqual(1, selected.Count);

			var run = oe.Elements(Ns + "T")
				.First(t => t.Attribute("selected")?.Value == "all");

			StringAssert.Contains(run.GetCData().Value, "font-weight:bold");
			StringAssert.Contains(run.GetCData().Value, "foo");
		}


		[TestMethod]
		public void Select_NoMatches_ReturnsEmpty()
		{
			var oe = MakeParagraph("{OE1}", "abc");

			var selected = FirstMatchSelector.Select(FindAll("zzz", oe));

			Assert.AreEqual(0, selected.Count);
			Assert.AreEqual(1, oe.Elements(Ns + "T").Count());
		}
	}
}
