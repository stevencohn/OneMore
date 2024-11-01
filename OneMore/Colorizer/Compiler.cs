//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
//************************************************************************************************                

namespace River.OneMoreAddIn.Colorizer
{
	using System.Collections.Generic;
	using System.Text;
	using System.Text.RegularExpressions;


	internal static class Compiler
	{
		private static readonly Regex capturePattern =
			new(@"(?x)(?<!(\\|(?!\\)\(\?))\((?!\?)", RegexOptions.Compiled);

		private static readonly Regex namedPattern =
			new(@"(?<!(\\|(?!\\)\(\?))\((\?<\w+>)", RegexOptions.Compiled);


		public static ICompiledLanguage Compile(ILanguage language)
		{
			var builder = new StringBuilder();

			// ignore pattern whitespace (?x)
			builder.Append("(?x)");
			// include end-of-line capture ($)
			builder.Append("(?-xis)(?m)($)(?x)");
			builder.AppendLine();

			var scopes = new List<string>
			{
				"*",  // 0th capture is always the entire string
				"$"   // 1st capture is always the end-of-line
			};

			for (int i = 0; i < language.Rules.Count; i++)
			{
				var rule = language.Rules[i];
				ValidateRule(language, rule, i);

				// add a visually significant separator between rules to ease debugging
				builder.AppendLine();
				builder.AppendLine("|");
				builder.AppendLine();

				// ?-xis = enables pattern whitespace, case sensitivity, multi line
				// ?m = enables multi line
				builder.Append("(?-xis)(?m)(?:");
				builder.Append(rule.Pattern);
				// ?x ignores pattern whitespace again
				builder.AppendLine(")(?x)");

				// collect named captures as a list of scopes
				scopes.AddRange(rule.Captures);
			}

			//Logger.Current.Verbose(builder.ToString());

			var compiled = (Language)language;
			compiled.Regex = new Regex(builder.ToString());
			compiled.Scopes = scopes;

			return compiled;
		}


		private static void ValidateRule(ILanguage language, IRule rule, int ruleNum)
		{
			if (string.IsNullOrWhiteSpace(rule.Pattern))
			{
				throw new LanguageException(
					$"{language.Name} rule {ruleNum} has an empty pattern\n{rule.Pattern}",
					language.Name, ruleNum);
			}

			if (rule.Captures == null || rule.Captures.Count == 0)
			{
				throw new LanguageException(
					$"{language.Name} rule {ruleNum} does not have defined captures\n{rule.Pattern}",
					language.Name, ruleNum);
			}

			var count = capturePattern.Matches(rule.Pattern).Count;
			if (count != rule.Captures.Count)
			{
				throw new LanguageException(
					$"{language.Name} rule {ruleNum} has misaligned captures\n{rule.Pattern}",
					language.Name, ruleNum);
			}

			if (namedPattern.Match(rule.Pattern).Success)
			{
				throw new LanguageException(
					$"{language.Name} rule {ruleNum} cannot contain a named group\n{rule.Pattern}",
					language.Name, ruleNum);
			}
		}
	}
}
