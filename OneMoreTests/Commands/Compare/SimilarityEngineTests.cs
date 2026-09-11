//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Compare
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn;
	using River.OneMoreAddIn.Commands.Compare;
	using River.OneMoreAddIn.Models;
	using System.Linq;
	using System.Xml.Linq;
	using Resx = River.OneMoreAddIn.Properties.Resources;

	/*
	 * Test Protocol - SimilarityEngine
	 *
	 * This is a pure, local-only scorer exercised entirely through hand-built page XML, so
	 * these unit tests are the primary verification for the rubric math; there is no
	 * separate manual protocol. Structural (heading) matching is exercised only indirectly
	 * through Compare(), since GetHeadings() depends on live theme/style state that isn't
	 * meaningful to hand-construct here; SimilarityEngine already falls back to "no
	 * headings" if that lookup fails, so these pages intentionally carry no heading markup.
	 */

	[TestClass]
	public class SimilarityEngineTests
	{
		private static readonly XNamespace ns = "http://schemas.microsoft.com/office/onenote/2013/onenote";


		private static Page BuildPage(string id, string title, params string[] paragraphs)
		{
			var oeChildren = new XElement(ns + "OEChildren",
				paragraphs.Select(p => new XElement(ns + "OE", new XElement(ns + "T", new XCData(p)))));

			var root = new XElement(ns + "Page",
				new XAttribute(XNamespace.Xmlns + "one", ns.NamespaceName),
				new XAttribute("ID", id),
				new XAttribute("name", title),
				new XElement(ns + "Outline", oeChildren));

			return new Page(root);
		}


		private static Page BuildPageWithImage(string id, string title, string imageData, params string[] paragraphs)
		{
			var oeChildren = new XElement(ns + "OEChildren",
				paragraphs.Select(p => new XElement(ns + "OE", new XElement(ns + "T", new XCData(p)))));

			oeChildren.Add(new XElement(ns + "OE",
				new XElement(ns + "Image", new XElement(ns + "Data", imageData))));

			var root = new XElement(ns + "Page",
				new XAttribute(XNamespace.Xmlns + "one", ns.NamespaceName),
				new XAttribute("ID", id),
				new XAttribute("name", title),
				new XElement(ns + "Outline", oeChildren));

			return new Page(root);
		}


		[TestMethod]
		public void Compare_IdenticalPages_ScoresNearPerfect()
		{
			var left = BuildPage("L1", "Design Notes",
				"The synchronization engine reconciles the local cache with the remote API.",
				"Conflicts are resolved using a last-writer-wins strategy.");

			var right = BuildPage("R1", "Design Notes",
				"The synchronization engine reconciles the local cache with the remote API.",
				"Conflicts are resolved using a last-writer-wins strategy.");

			var result = SimilarityEngine.Compare(left, right, null);

			Assert.IsTrue(result.Overall > 0.9, $"expected near-perfect overall, got {result.Overall}");
			Assert.AreEqual(5, result.Rubrics.Count);
			Assert.IsTrue(result.Rubrics.All(r => r.Score > 0.9),
				string.Join(", ", result.Rubrics.Select(r => $"{r.Name}={r.Score:F2}")));
		}


		[TestMethod]
		public void Compare_UnrelatedVocabulary_ScoresLowerThanIdentical()
		{
			var left = BuildPage("L1", "API Notes",
				"The synchronization engine reconciles the local cache with the remote API.");

			var right = BuildPage("R1", "Garden Notes",
				"Tomato seedlings need full sun and consistent watering through the summer.");

			var identical = SimilarityEngine.Compare(left, BuildPage("L2", "API Notes",
				"The synchronization engine reconciles the local cache with the remote API."), null);

			var unrelated = SimilarityEngine.Compare(left, right, null);

			Assert.IsTrue(unrelated.Overall < identical.Overall,
				$"unrelated ({unrelated.Overall:F2}) should score below identical ({identical.Overall:F2})");

			var lexical = unrelated.Rubrics.Single(r => r.Name == Resx.Similarity_rubricLexical);
			Assert.IsTrue(lexical.Score < 0.3, $"expected low lexical overlap, got {lexical.Score}");
		}


		[TestMethod]
		public void Compare_BothPagesEmpty_ReturnsPerfectScore()
		{
			var left = BuildPage("L1", "Empty");
			var right = BuildPage("R1", "Empty");

			var result = SimilarityEngine.Compare(left, right, null);

			Assert.AreEqual(1.0, result.Overall, 0.0001);
			Assert.IsTrue(result.Rubrics.All(r => r.Score == 1.0));
		}


		[TestMethod]
		public void Compare_OneSideEmpty_ReturnsZeroTfIdfAndLexicalScores()
		{
			var left = BuildPage("L1", "Has Text", "Some meaningful content lives here.");
			var right = BuildPage("R1", "Empty");

			var result = SimilarityEngine.Compare(left, right, null);

			var tfidf = result.Rubrics.Single(r => r.Name == Resx.Similarity_rubricTfIdf);
			var lexical = result.Rubrics.Single(r => r.Name == Resx.Similarity_rubricLexical);

			Assert.AreEqual(0.0, tfidf.Score, 0.0001);
			Assert.AreEqual(0.0, lexical.Score, 0.0001);
		}


		[TestMethod]
		public void Compare_RubricWeights_SumToOne()
		{
			var left = BuildPage("L1", "A", "Some content.");
			var right = BuildPage("R1", "B", "Other content.");

			var result = SimilarityEngine.Compare(left, right, null);

			var totalWeight = result.Rubrics.Sum(r => r.Weight);
			Assert.AreEqual(1.0, totalWeight, 0.0001);
		}


		[TestMethod]
		public void Compare_SharedTechnicalIdentifiers_IncreaseEntityScore()
		{
			var left = BuildPage("L1", "A",
				"Call OneNote.GetPage to fetch the PageId before invoking UpdateHierarchy.");

			var right = BuildPage("R1", "B",
				"The UpdateHierarchy call requires a valid PageId returned by OneNote.GetPage.");

			var unrelatedRight = BuildPage("R2", "C",
				"Tomato seedlings need full sun and consistent watering through the summer.");

			var withSharedEntities = SimilarityEngine.Compare(left, right, null);
			var withoutSharedEntities = SimilarityEngine.Compare(left, unrelatedRight, null);

			var sharedScore = withSharedEntities.Rubrics.Single(r => r.Name == Resx.Similarity_rubricEntity).Score;
			var unrelatedScore = withoutSharedEntities.Rubrics.Single(r => r.Name == Resx.Similarity_rubricEntity).Score;

			Assert.IsTrue(sharedScore > unrelatedScore,
				$"shared-entity score ({sharedScore:F2}) should exceed unrelated score ({unrelatedScore:F2})");
		}


		[TestMethod]
		public void BuildProfile_ExtractsImageHashes()
		{
			var page = BuildPageWithImage("L1", "Has Image", "AAAABBBB", "Some text.");
			var profile = SimilarityEngine.BuildProfile(page, null);

			Assert.AreEqual(1, profile.ImageHashes.Count);
		}


		[TestMethod]
		public void Compare_SameTextDifferentImages_MediaRubricScoresZero()
		{
			var left = BuildPageWithImage("L1", "A", "AAAA", "Identical paragraph text here.");
			var right = BuildPageWithImage("R1", "B", "ZZZZ", "Identical paragraph text here.");

			var profileA = SimilarityEngine.BuildProfile(left, null);
			var profileB = SimilarityEngine.BuildProfile(right, null);

			var result = SimilarityEngine.Compare(profileA, profileB, new SimilarityOptions(Rubric.Media));
			var media = result.Rubrics.Single(r => r.Name == Resx.Similarity_rubricMedia);

			Assert.AreEqual(0.0, media.Score, 0.0001);
		}


		[TestMethod]
		public void Compare_SharedImages_MediaRubricScoresPerfect()
		{
			var left = BuildPageWithImage("L1", "A", "SAMEDATA", "Text.");
			var right = BuildPageWithImage("R1", "B", "SAMEDATA", "Text.");

			var profileA = SimilarityEngine.BuildProfile(left, null);
			var profileB = SimilarityEngine.BuildProfile(right, null);

			var result = SimilarityEngine.Compare(profileA, profileB, new SimilarityOptions(Rubric.Media));
			var media = result.Rubrics.Single(r => r.Name == Resx.Similarity_rubricMedia);

			Assert.AreEqual(1.0, media.Score, 0.0001);
		}


		[TestMethod]
		public void Compare_DefaultOptions_NeverIncludesMediaRubric()
		{
			var left = BuildPageWithImage("L1", "A", "AAAA", "Some text.");
			var right = BuildPageWithImage("R1", "B", "ZZZZ", "Some text.");

			var result = SimilarityEngine.Compare(left, right, null);

			Assert.IsFalse(result.Rubrics.Any(r => r.Name == Resx.Similarity_rubricMedia),
				"Compare Hierarchy's default options must never score Media");
		}


		[TestMethod]
		public void Compare_SubsetOfRubrics_WeightsRenormalizeToOne()
		{
			var left = BuildPage("L1", "A", "Some content.");
			var right = BuildPage("R1", "B", "Other content.");

			var profileA = SimilarityEngine.BuildProfile(left, null);
			var profileB = SimilarityEngine.BuildProfile(right, null);

			var options = new SimilarityOptions(Rubric.TfIdf | Rubric.Lexical);
			var result = SimilarityEngine.Compare(profileA, profileB, options);

			Assert.AreEqual(2, result.Rubrics.Count);
			Assert.AreEqual(1.0, result.Rubrics.Sum(r => r.Weight), 0.0001);
		}
	}
}
