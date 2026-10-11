//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Tests.Commands.Search
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using River.OneMoreAddIn.Commands;


	[TestClass]
	public class TextMatchBuilderTests
	{
		private static bool Matches(string query, string candidate, bool caseSensitive = false)
		{
			var regex = new TextMatchBuilder(asRegex: false, caseSensitive).BuildRegex(query);
			return regex.IsMatch(candidate);
		}


		[TestMethod]
		public void BareWords_MatchRegardlessOfOrder()
		{
			// issue #2410 - unordered words should default to implicit AND
			Assert.IsTrue(Matches("foo bar", "bar fox foo"));
			Assert.IsTrue(Matches("foo bar", "foo bar"));
			Assert.IsTrue(Matches("foo bar", "bar foo"));
		}


		[TestMethod]
		public void BareWords_RequireAllWordsPresent()
		{
			Assert.IsFalse(Matches("foo bar", "foo only"));
			Assert.IsFalse(Matches("foo bar", "bar only"));
		}


		[TestMethod]
		public void QuotedPhrase_RequiresExactOrder()
		{
			Assert.IsTrue(Matches("\"foo bar\"", "well foo bar indeed"));
			Assert.IsFalse(Matches("\"foo bar\"", "bar foo"));
		}


		[TestMethod]
		public void ExplicitAnd_MatchesBothTermsAnywhere()
		{
			Assert.IsTrue(Matches("power AND boston", "A Boston resident won the PowerBall lottery"));
		}


		[TestMethod]
		public void BareWordsBeforeAnd_DoNotDropEarlierTerm()
		{
			// left-operand collapse bug: "foo bar AND baz" must require all three,
			// not just "bar" and "baz"
			Assert.IsTrue(Matches("foo bar AND baz", "baz bar foo"));
			Assert.IsFalse(Matches("foo bar AND baz", "bar baz"));
		}


		[TestMethod]
		public void ExplicitOr_MatchesEitherTerm()
		{
			Assert.IsTrue(Matches("foo OR baz", "only foo here"));
			Assert.IsTrue(Matches("foo OR baz", "only baz here"));
			Assert.IsFalse(Matches("foo OR baz", "neither"));
		}


		[TestMethod]
		public void NotFollowedByOneWord_ExcludesThatWord()
		{
			Assert.IsFalse(Matches("NOT foo", "has foo"));
			Assert.IsTrue(Matches("NOT foo", "has bar"));
		}


		[TestMethod]
		public void NotFollowedByMultipleWords_ExcludesAnyOfThem()
		{
			// confirmed semantics: "NOT foo bar" == NOT(foo) AND NOT(bar)
			Assert.IsFalse(Matches("NOT foo bar", "foo bar"));
			Assert.IsFalse(Matches("NOT foo bar", "has foo only"));
			Assert.IsFalse(Matches("NOT foo bar", "has bar only"));
			Assert.IsTrue(Matches("NOT foo bar", "baz qux"));
		}


		[TestMethod]
		public void NotFollowedByQuotedPhrase_ExcludesExactPhraseOnly()
		{
			Assert.IsFalse(Matches("NOT \"foo bar\"", "foo bar"));
			Assert.IsTrue(Matches("NOT \"foo bar\"", "bar foo"));
		}


		[TestMethod]
		public void GroupingAndWildcard_MatchClassDocExample()
		{
			var query = "(error* OR fail*) AND NOT warning*";
			Assert.IsTrue(Matches(query, "Critical failure occurred in error handler"));
			Assert.IsFalse(Matches(query, "error and a warning too"));
			Assert.IsFalse(Matches(query, "nothing relevant"));
		}


		[TestMethod]
		public void CaseSensitiveFlag_IsHonored()
		{
			Assert.IsTrue(Matches("foo", "FOO", caseSensitive: false));
			Assert.IsFalse(Matches("foo", "FOO", caseSensitive: true));
			Assert.IsTrue(Matches("foo", "foo", caseSensitive: true));
		}


		[TestMethod]
		public void Term_OnALaterLineOfTheParagraph_IsFound()
		{
			// OneNote text with a line break: <br /> followed by a newline
			var text = "OneMore/UI/Scaling.cs (lines 101-102).<br />\nReplace System.Windows with";

			Assert.IsTrue(Matches("Replace", text));
			Assert.IsTrue(Matches("lines AND Replace", text));
			Assert.IsTrue(Matches("\"Replace System\"", text));
			Assert.IsFalse(Matches("Replace AND NOT lines", text));
			Assert.IsFalse(Matches("NOT Replace", text));
			Assert.IsTrue(Matches("NOT missing", text));
		}


		private static string IndexQuery(string query, bool asRegex = false)
		{
			return new TextMatchBuilder(asRegex, caseSensitive: false).BuildIndexQuery(query);
		}


		[TestMethod]
		public void IndexQuery_SingleTerm_IsQuoted()
		{
			Assert.AreEqual("\"error\"", IndexQuery("error"));
		}


		[TestMethod]
		public void IndexQuery_BareWords_AreAnded()
		{
			// the parser collects bare words from a stack, so they come back in reverse; the
			// order of an AND does not matter
			Assert.AreEqual("(\"bar\" AND \"foo\")", IndexQuery("foo bar"));
		}


		[TestMethod]
		public void IndexQuery_Or_IsKept()
		{
			Assert.AreEqual("(\"error\" OR \"fail\")", IndexQuery("error* OR fail*"));
		}


		[TestMethod]
		public void IndexQuery_QuotedPhrase_IsOneSubstring()
		{
			Assert.AreEqual("\"page identity\"", IndexQuery("\"page identity\""));
		}


		[TestMethod]
		public void IndexQuery_WildcardPieces_AreEachLookedUp()
		{
			Assert.AreEqual("(\"bcd\" AND \"efg\")", IndexQuery("a*bcd*efg"));
		}


		[TestMethod]
		public void IndexQuery_QuoteInTerm_IsDoubled()
		{
			Assert.AreEqual("\"say\"\"hi\"\"\"", IndexQuery("say\"hi\""));
		}


		[TestMethod]
		public void IndexQuery_TermsUnderThreeCharacters_CannotNarrow()
		{
			Assert.IsNull(IndexQuery("ab"));
			Assert.IsNull(IndexQuery("*ab*"));

			// the short term is left to the regex; the other still narrows
			Assert.AreEqual("\"foo\"", IndexQuery("ab foo"));
		}


		[TestMethod]
		public void IndexQuery_Not_IsLeftToTheRegex()
		{
			Assert.IsNull(IndexQuery("NOT foo"));
			Assert.AreEqual("\"foo\"", IndexQuery("foo AND NOT bar"));
		}


		[TestMethod]
		public void IndexQuery_OrWithAnAlternativeThatCannotNarrow_CannotNarrow()
		{
			Assert.IsNull(IndexQuery("foo OR NOT bar"));
			Assert.IsNull(IndexQuery("foo OR ab"));
		}


		[TestMethod]
		public void IndexQuery_RegexMode_CannotNarrow()
		{
			Assert.IsNull(IndexQuery("^foo.*bar$", asRegex: true));
		}


		[TestMethod]
		public void IndexQuery_EmptyQuery_CannotNarrow()
		{
			Assert.IsNull(IndexQuery("   "));
		}


		[TestMethod]
		public void RegexMode_BypassesQueryParsing()
		{
			var regex = new TextMatchBuilder(asRegex: true, caseSensitive: false).BuildRegex("^foo.*bar$");
			Assert.IsTrue(regex.IsMatch("foo baz bar"));
			Assert.IsFalse(regex.IsMatch("bar baz foo"));
		}
	}
}
