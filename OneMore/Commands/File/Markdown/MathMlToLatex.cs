//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;


	/// <summary>
	/// Converts the presentation MathML that OneNote stores for native equations into LaTeX.
	/// Supports the subset of MathML that OneNote/Office produces; unrecognized elements
	/// degrade to the concatenation of their children rather than failing.
	/// </summary>
	internal static class MathMlToLatex
	{
		private static readonly Regex CommandTail = new(@"\\[a-zA-Z]+$", RegexOptions.Compiled);
		private static readonly Regex SingleToken = new(@"^(\\[a-zA-Z]+|.)$", RegexOptions.Compiled);

		private static readonly HashSet<string> Functions = new()
		{
			"sin", "cos", "tan", "cot", "sec", "csc", "sinh", "cosh", "tanh", "coth",
			"arcsin", "arccos", "arctan", "log", "ln", "lg", "exp", "lim", "liminf", "limsup",
			"max", "min", "sup", "inf", "det", "dim", "gcd", "arg", "deg", "ker", "Pr", "hom"
		};

		// longest first so "arcsin" wins over "sin"; OneNote stores function names as single
		// letters, e.g. sin as three consecutive mi elements
		private static readonly string[] FunctionNames = Functions
			.OrderByDescending(f => f.Length)
			.ToArray();

		// named entities that OneNote may emit but which XML does not define
		private static readonly Dictionary<string, int> Entities = new()
		{
			{ "nbsp", 160 }, { "ensp", 8194 }, { "emsp", 8195 }, { "thinsp", 8201 },
			{ "ThinSpace", 8201 }, { "hairsp", 8202 }, { "MediumSpace", 8287 },
			{ "ZeroWidthSpace", 8203 }, { "InvisibleTimes", 8290 }, { "ApplyFunction", 8289 },
			{ "InvisibleComma", 8291 }, { "hellip", 8230 }, { "minus", 8722 }, { "times", 215 },
			{ "middot", 183 }, { "deg", 176 }, { "plusmn", 177 }
		};

		private static readonly HashSet<string> LimitOperators = new()
		{
			@"\sum", @"\prod", @"\coprod", @"\int", @"\iint", @"\iiint", @"\oint",
			@"\bigcup", @"\bigcap", @"\bigoplus", @"\bigotimes", @"\bigvee", @"\bigwedge"
		};

		private static readonly Dictionary<string, string> Symbols = new()
		{
			// escapes
			{ "\\", @"\backslash" }, { "{", @"\{" }, { "}", @"\}" }, { "%", @"\%" },
			{ "#", @"\#" }, { "&", @"\&" }, { "$", @"\$" }, { "_", @"\_" },

			// lowercase greek
			{ "α", @"\alpha" }, { "β", @"\beta" }, { "γ", @"\gamma" }, { "δ", @"\delta" },
			{ "ε", @"\epsilon" }, { "ϵ", @"\epsilon" }, { "ζ", @"\zeta" }, { "η", @"\eta" },
			{ "θ", @"\theta" }, { "ϑ", @"\vartheta" }, { "ι", @"\iota" }, { "κ", @"\kappa" },
			{ "λ", @"\lambda" }, { "μ", @"\mu" }, { "ν", @"\nu" }, { "ξ", @"\xi" },
			{ "π", @"\pi" }, { "ρ", @"\rho" }, { "ς", @"\varsigma" }, { "σ", @"\sigma" },
			{ "τ", @"\tau" }, { "υ", @"\upsilon" }, { "φ", @"\phi" }, { "ϕ", @"\varphi" },
			{ "χ", @"\chi" }, { "ψ", @"\psi" }, { "ω", @"\omega" },

			// uppercase greek (those identical to latin letters map to the latin letter)
			{ "Α", "A" }, { "Β", "B" }, { "Γ", @"\Gamma" }, { "Δ", @"\Delta" }, { "Ε", "E" },
			{ "Ζ", "Z" }, { "Η", "H" }, { "Θ", @"\Theta" }, { "ϴ", @"\Theta" }, { "Ι", "I" },
			{ "Κ", "K" }, { "Λ", @"\Lambda" }, { "Μ", "M" }, { "Ν", "N" }, { "Ξ", @"\Xi" },
			{ "Ο", "O" }, { "Π", @"\Pi" }, { "Ρ", "P" }, { "Σ", @"\Sigma" }, { "Τ", "T" },
			{ "Υ", @"\Upsilon" }, { "Φ", @"\Phi" }, { "Χ", "X" }, { "Ψ", @"\Psi" },
			{ "Ω", @"\Omega" },

			// sets and misc
			{ "ℝ", @"\mathbb{R}" }, { "ℕ", @"\mathbb{N}" }, { "ℤ", @"\mathbb{Z}" },
			{ "ℚ", @"\mathbb{Q}" }, { "ℂ", @"\mathbb{C}" }, { "ℓ", @"\ell" }, { "ℏ", @"\hbar" },
			{ "∞", @"\infty" }, { "∂", @"\partial" }, { "∇", @"\nabla" }, { "∅", @"\emptyset" },
			{ "∀", @"\forall" }, { "∃", @"\exists" }, { "¬", @"\neg" }, { "′", "'" },
			{ "″", "''" }, { "…", @"\ldots" }, { "⋯", @"\cdots" }, { "⋮", @"\vdots" },
			{ "⋱", @"\ddots" }, { "°", @"^\circ" }, { "ℵ", @"\aleph" },

			// operators and relations
			{ "±", @"\pm" }, { "∓", @"\mp" }, { "×", @"\times" }, { "÷", @"\div" },
			{ "·", @"\cdot" }, { "⋅", @"\cdot" }, { "∗", "*" }, { "∘", @"\circ" },
			{ "−", "-" }, { "–", "-" }, { "≤", @"\leq" }, { "≥", @"\geq" }, { "≠", @"\neq" },
			{ "≈", @"\approx" }, { "≅", @"\cong" }, { "≡", @"\equiv" }, { "∼", @"\sim" },
			{ "≃", @"\simeq" }, { "∝", @"\propto" }, { "≪", @"\ll" }, { "≫", @"\gg" },
			{ "∈", @"\in" }, { "∉", @"\notin" }, { "∋", @"\ni" }, { "⊂", @"\subset" },
			{ "⊃", @"\supset" }, { "⊆", @"\subseteq" }, { "⊇", @"\supseteq" },
			{ "∪", @"\cup" }, { "∩", @"\cap" }, { "∧", @"\wedge" }, { "∨", @"\vee" },
			{ "⊕", @"\oplus" }, { "⊗", @"\otimes" }, { "⊥", @"\perp" }, { "∥", @"\parallel" },
			{ "∠", @"\angle" }, { "∴", @"\therefore" }, { "∵", @"\because" },
			{ "→", @"\to" }, { "←", @"\leftarrow" }, { "↔", @"\leftrightarrow" },
			{ "⇒", @"\Rightarrow" }, { "⇐", @"\Leftarrow" }, { "⇔", @"\Leftrightarrow" },
			{ "↦", @"\mapsto" }, { "↑", @"\uparrow" }, { "↓", @"\downarrow" },

			// large operators
			{ "∑", @"\sum" }, { "∏", @"\prod" }, { "∐", @"\coprod" }, { "∫", @"\int" },
			{ "∬", @"\iint" }, { "∭", @"\iiint" }, { "∮", @"\oint" }, { "⋃", @"\bigcup" },
			{ "⋂", @"\bigcap" }, { "√", @"\surd" },

			// delimiters
			{ "‖", @"\|" }, { "⟨", @"\langle" }, { "⟩", @"\rangle" }, { "〈", @"\langle" },
			{ "〉", @"\rangle" }, { "⌈", @"\lceil" }, { "⌉", @"\rceil" }, { "⌊", @"\lfloor" },
			{ "⌋", @"\rfloor" }
		};

		// accent characters: base accent command when placed over a base
		private static readonly Dictionary<string, string> OverAccents = new()
		{
			{ "^", @"\hat" }, { "ˆ", @"\hat" }, { "̂", @"\hat" },
			{ "¯", @"\bar" }, { "‾", @"\bar" }, { "―", @"\bar" }, { "̄", @"\bar" },
			{ "→", @"\vec" }, { "⃗", @"\vec" }, { "-", @"\bar" }, { "−", @"\bar" },
			{ "~", @"\tilde" }, { "˜", @"\tilde" }, { "̃", @"\tilde" },
			{ ".", @"\dot" }, { "˙", @"\dot" }, { "̇", @"\dot" },
			{ "¨", @"\ddot" }, { "̈", @"\ddot" },
			{ "⏞", @"\overbrace" }, { "←", @"\overleftarrow" }
		};

		private static readonly Dictionary<string, string> UnderAccents = new()
		{
			{ "_", @"\underline" }, { "¯", @"\underline" }, { "⏟", @"\underbrace" }
		};


		/// <summary>
		/// Converts the given MathML to LaTeX
		/// </summary>
		/// <param name="mathml">A MathML document string whose root is a math element</param>
		/// <param name="latex">The resulting LaTeX without $ delimiters</param>
		/// <param name="block">True if the math element is display="block"</param>
		/// <returns>True if the markup was parsed and converted</returns>
		public static bool TryConvert(string mathml, out string latex, out bool block)
		{
			latex = null;
			block = false;

			if (string.IsNullOrWhiteSpace(mathml))
			{
				return false;
			}

			try
			{
				mathml = Regex.Replace(mathml, @"&(?<n>[A-Za-z][A-Za-z0-9]*);", m =>
					Entities.TryGetValue(m.Groups["n"].Value, out var code) ? $"&#{code};" : m.Value);

				var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };
				using var reader = XmlReader.Create(new System.IO.StringReader(mathml), settings);
				var root = XElement.Load(reader);

				block = string.Equals(
					(string)root.Attribute("display"), "block", StringComparison.OrdinalIgnoreCase);

				latex = Row(root.Elements().ToList()).Trim();
				return latex.Length > 0;
			}
			catch (XmlException)
			{
				return false;
			}
		}


		private static string Walk(XElement e)
		{
			switch (e.Name.LocalName)
			{
				case "mi": return Identifier(e);
				case "mn":
				case "mo": return Token(e);
				case "mtext": return Text(e);
				case "ms": return $@"\text{{{e.Value}}}";
				case "mspace": return @"\,";
				case "mfrac": return Fraction(e);
				case "msqrt": return $@"\sqrt{{{Row(e.Elements().ToList())}}}";
				case "mroot": return Root(e);
				case "msub": return Scripts(e, true, false);
				case "msup": return Scripts(e, false, true);
				case "msubsup": return Scripts(e, true, true);
				case "munder": return Limits(e, true, false);
				case "mover": return Limits(e, false, true);
				case "munderover": return Limits(e, true, true);
				case "mfenced": return Fenced(e);
				case "mtable": return Table(e, "matrix");
				case "menclose": return $@"\boxed{{{Row(e.Elements().ToList())}}}";
				case "mphantom": return string.Empty;
				case "semantics":
					{
						var first = e.Elements().FirstOrDefault();
						return first is null ? string.Empty : Walk(first);
					}
				case "annotation":
				case "annotation-xml":
					return string.Empty;
				default:
					// math, mrow, mstyle, mpadded, maction, merror and anything unknown
					return Row(e.Elements().ToList());
			}
		}


		// concatenates sibling elements, recognizing fence+table patterns such as matrices
		private static string Row(IList<XElement> kids)
		{
			if (kids.Count >= 2 && kids[1].Name.LocalName == "mtable" && kids[0].Name.LocalName == "mo")
			{
				var open = kids[0].Value.Trim();
				if (kids.Count == 3 && kids[2].Name.LocalName == "mo" &&
					MatrixEnvironments.TryGetValue(open, out var env))
				{
					return Table(kids[1], env);
				}

				if (kids.Count == 2 && open == "{")
				{
					return Table(kids[1], "cases");
				}
			}

			var sb = new StringBuilder();
			for (var i = 0; i < kids.Count; i++)
			{
				if (!IsLetter(kids[i]))
				{
					Append(sb, Walk(kids[i]));
					continue;
				}

				var letters = new StringBuilder();
				while (i < kids.Count && IsLetter(kids[i]))
				{
					letters.Append(kids[i].Value.Trim());
					i++;
				}

				i--;
				AppendLetters(sb, letters.ToString());
			}

			return sb.ToString();
		}


		private static bool IsLetter(XElement e)
		{
			if (e.Name.LocalName != "mi")
			{
				return false;
			}

			var text = e.Value.Trim();
			return text.Length == 1 && text[0] < 128 && char.IsLetter(text[0]);
		}


		// a run of single-letter identifiers may spell a function name such as sin or lim
		private static void AppendLetters(StringBuilder sb, string letters)
		{
			var pos = 0;
			while (pos < letters.Length)
			{
				var name = FunctionNames.FirstOrDefault(f =>
					string.CompareOrdinal(letters, pos, f, 0, f.Length) == 0);

				if (name is null)
				{
					Append(sb, letters[pos].ToString());
					pos++;
				}
				else
				{
					Append(sb, @"\" + name);
					pos += name.Length;
				}
			}
		}


		// mo and mn; whitespace-only tokens (e.g. &nbsp;) are spacing, invisible operators vanish
		private static string Token(XElement e)
		{
			var raw = e.Value;
			var text = raw.Trim();
			if (text.Length == 0)
			{
				return raw.Length > 0 ? @"\," : string.Empty;
			}

			text = text.Replace("⁡", string.Empty).Replace("⁢", string.Empty)
				.Replace("⁣", string.Empty).Replace("⁤", string.Empty)
				.Replace("​", string.Empty);

			return Normalize(text);
		}


		private static readonly Dictionary<string, string> MatrixEnvironments = new()
		{
			{ "(", "pmatrix" }, { "[", "bmatrix" }, { "{", "Bmatrix" },
			{ "|", "vmatrix" }, { "‖", "Vmatrix" }
		};


		// appends text, inserting a space where a LaTeX command would otherwise absorb a letter
		private static void Append(StringBuilder sb, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			if (sb.Length > 0 && char.IsLetter(text[0]) && CommandTail.IsMatch(sb.ToString()))
			{
				sb.Append(' ');
			}

			sb.Append(text);
		}


		private static string Identifier(XElement e)
		{
			var text = e.Value.Trim();
			if (Functions.Contains(text))
			{
				return @"\" + text;
			}

			var normal = Normalize(text);
			if (text.Length > 1 && text.All(char.IsLetter) && normal.All(char.IsLetter))
			{
				return $@"\mathrm{{{normal}}}";
			}

			return normal;
		}


		private static string Text(XElement e)
		{
			var text = e.Value;
			if (text.Trim().Length == 0)
			{
				return @"\ ";
			}

			return $@"\text{{{text}}}";
		}


		private static string Fraction(XElement e)
		{
			var kids = e.Elements().ToList();
			if (kids.Count < 2)
			{
				return Row(kids);
			}

			var numerator = Walk(kids[0]);
			var denominator = Walk(kids[1]);

			return (string)e.Attribute("linethickness") == "0"
				? $@"\binom{{{numerator}}}{{{denominator}}}"
				: $@"\frac{{{numerator}}}{{{denominator}}}";
		}


		private static string Root(XElement e)
		{
			var kids = e.Elements().ToList();
			if (kids.Count < 2)
			{
				return $@"\sqrt{{{Row(kids)}}}";
			}

			return $@"\sqrt[{Walk(kids[1])}]{{{Walk(kids[0])}}}";
		}


		// wraps multi-token bases so a following _ or ^ binds to the whole base
		private static string Base(XElement e)
		{
			var text = Walk(e);
			if (text.Length == 0)
			{
				return "{}";
			}

			if (SingleToken.IsMatch(text) || IsFenceGroup(text))
			{
				return text;
			}

			return $"{{{text}}}";
		}


		// "(...)" or "[...]" whose opening fence matches the final character, so a following
		// script binds to the closing fence token exactly as it would to the whole group
		private static bool IsFenceGroup(string text)
		{
			if (text.Length < 2 || text.StartsWith(@"\left") ||
				!((text[0] == '(' && text[text.Length - 1] == ')') ||
				(text[0] == '[' && text[text.Length - 1] == ']')))
			{
				return false;
			}

			var depth = 0;
			for (var i = 0; i < text.Length - 1; i++)
			{
				depth += text[i] is '(' or '[' ? 1 : text[i] is ')' or ']' ? -1 : 0;
				if (depth == 0)
				{
					return false;
				}
			}

			return true;
		}


		private static string Scripts(XElement e, bool sub, bool sup)
		{
			var kids = e.Elements().ToList();
			if (kids.Count < 2 + (sub && sup ? 1 : 0))
			{
				return Row(kids);
			}

			var sb = new StringBuilder(Base(kids[0]));

			if (sub)
			{
				sb.Append("_{").Append(Walk(kids[1])).Append('}');
			}

			if (sup)
			{
				sb.Append("^{").Append(Walk(kids[sub ? 2 : 1])).Append('}');
			}

			return sb.ToString();
		}


		private static string Limits(XElement e, bool under, bool over)
		{
			var kids = e.Elements().ToList();
			if (kids.Count < 2 + (under && over ? 1 : 0))
			{
				return Row(kids);
			}

			var basis = Walk(kids[0]);
			var bottom = under ? kids[1] : null;
			var top = over ? kids[under ? 2 : 1] : null;

			if (LimitOperators.Contains(basis) ||
				(basis.StartsWith(@"\") && Functions.Contains(basis.Substring(1))))
			{
				var sb = new StringBuilder(basis);
				if (bottom is not null)
				{
					sb.Append("_{").Append(Walk(bottom)).Append('}');
				}

				if (top is not null)
				{
					sb.Append("^{").Append(Walk(top)).Append('}');
				}

				return sb.ToString();
			}

			if (under && !over && UnderAccents.TryGetValue(bottom.Value.Trim(), out var under1))
			{
				return $"{under1}{{{basis}}}";
			}

			if (over && !under && OverAccents.TryGetValue(top.Value.Trim(), out var over1))
			{
				// \bar covers a single token; a longer base needs \overline
				if (over1 == @"\bar" && !SingleToken.IsMatch(basis))
				{
					over1 = @"\overline";
				}

				return $"{over1}{{{basis}}}";
			}

			var result = basis;
			if (bottom is not null)
			{
				result = $@"\underset{{{Walk(bottom)}}}{{{result}}}";
			}

			if (top is not null)
			{
				result = $@"\overset{{{Walk(top)}}}{{{result}}}";
			}

			return result;
		}


		private static string Fenced(XElement e)
		{
			var open = (string)e.Attribute("open") ?? "(";
			var close = (string)e.Attribute("close") ?? ")";
			var separators = (string)e.Attribute("separators") ?? ",";
			var kids = e.Elements().ToList();

			if (kids.Count == 1 && kids[0].Name.LocalName == "mtable" &&
				MatrixEnvironments.TryGetValue(open.Trim(), out var env))
			{
				return Table(kids[0], env);
			}

			var sb = new StringBuilder();
			for (var i = 0; i < kids.Count; i++)
			{
				if (i > 0 && separators.Length > 0)
				{
					var s = separators[Math.Min(i - 1, separators.Length - 1)];
					sb.Append(s == ' ' ? string.Empty : Normalize(s.ToString()));
				}

				Append(sb, Walk(kids[i]));
			}

			return $@"\left{Delimiter(open)} {sb} \right{Delimiter(close)}";
		}


		private static string Delimiter(string text)
		{
			text = text.Trim();
			if (text.Length == 0)
			{
				return ".";
			}

			if (text == "{" || text == "}")
			{
				return @"\" + text;
			}

			return Symbols.TryGetValue(text, out var symbol) ? symbol : text;
		}


		private static string Table(XElement e, string environment)
		{
			var rows = e.Elements()
				.Where(r => r.Name.LocalName == "mtr" || r.Name.LocalName == "mlabeledtr")
				.Select(r => string.Join(" & ", r.Elements()
					.Where(c => c.Name.LocalName == "mtd")
					.Select(c => Row(c.Elements().ToList()))))
				.ToList();

			return $@"\begin{{{environment}}} {string.Join(@" \\ ", rows)} \end{{{environment}}}";
		}


		// maps math alphanumeric code points to plain letters and known symbols to commands
		private static string Normalize(string text)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < text.Length; i++)
			{
				var code = char.IsHighSurrogate(text[i]) && i + 1 < text.Length
					? char.ConvertToUtf32(text[i], text[++i])
					: text[i];

				string piece;
				if (code >= 0x1D434 && code <= 0x1D44D)
				{
					piece = ((char)('A' + code - 0x1D434)).ToString();
				}
				else if (code >= 0x1D44E && code <= 0x1D467)
				{
					piece = code == 0x1D455 ? "h" : ((char)('a' + code - 0x1D44E)).ToString();
				}
				else if (code >= 0x1D6FC && code <= 0x1D714)
				{
					piece = ((char)(0x3B1 + code - 0x1D6FC)).ToString();
				}
				else if (code >= 0x1D6E2 && code <= 0x1D6FA)
				{
					piece = code == 0x1D6E2 + 17
						? "ϴ"
						: ((char)(0x391 + code - 0x1D6E2)).ToString();
				}
				else if (code >= 0x1D538 && code <= 0x1D551)
				{
					piece = $@"\mathbb{{{(char)('A' + code - 0x1D538)}}}";
				}
				else
				{
					piece = char.ConvertFromUtf32(code);
				}

				if (Symbols.TryGetValue(piece, out var symbol))
				{
					piece = symbol;
				}

				Append(sb, piece);
			}

			return sb.ToString();
		}
	}
}
