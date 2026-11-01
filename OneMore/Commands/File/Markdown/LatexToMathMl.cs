//************************************************************************************************
// Copyright © 2026 Steven M Cohn. All rights reserved.
//************************************************************************************************

namespace River.OneMoreAddIn.Commands
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Xml.Linq;


	/// <summary>
	/// Converts a LaTeX math expression into the presentation MathML that OneNote stores for
	/// native equations. Supports the common subset of LaTeX math; anything unrecognized
	/// causes the conversion to fail so the caller can leave the original text untouched.
	/// </summary>
	internal static class LatexToMathMl
	{
		private static readonly XNamespace M = "http://www.w3.org/1998/Math/MathML";

		private static readonly string[] GreekLower =
		{
			"alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta", "iota",
			"kappa", "lambda", "mu", "nu", "xi", "omicron", "pi", "rho", "varsigma", "sigma",
			"tau", "upsilon", "phi", "chi", "psi", "omega"
		};

		private static readonly Dictionary<string, string> GreekUpper = new()
		{
			{ "Gamma", "Γ" }, { "Delta", "Δ" }, { "Theta", "Θ" }, { "Lambda", "Λ" },
			{ "Xi", "Ξ" }, { "Pi", "Π" }, { "Sigma", "Σ" }, { "Upsilon", "Υ" },
			{ "Phi", "Φ" }, { "Psi", "Ψ" }, { "Omega", "Ω" }
		};

		// symbols that are variables/constants rather than operators
		private static readonly Dictionary<string, string> Identifiers = new()
		{
			{ "infty", "∞" }, { "partial", "∂" }, { "nabla", "∇" }, { "emptyset", "∅" },
			{ "ell", "ℓ" }, { "hbar", "ℏ" }, { "aleph", "ℵ" },
			{ "varepsilon", "ε" }, { "vartheta", "ϑ" }, { "varphi", "ϕ" }
		};

		private static readonly Dictionary<string, string> Operators = new()
		{
			{ "pm", "±" }, { "mp", "∓" }, { "times", "×" }, { "div", "÷" }, { "cdot", "⋅" },
			{ "ast", "∗" }, { "circ", "∘" }, { "leq", "≤" }, { "le", "≤" }, { "geq", "≥" },
			{ "ge", "≥" }, { "neq", "≠" }, { "ne", "≠" }, { "approx", "≈" }, { "cong", "≅" },
			{ "equiv", "≡" }, { "sim", "∼" }, { "simeq", "≃" }, { "propto", "∝" },
			{ "ll", "≪" }, { "gg", "≫" }, { "in", "∈" }, { "notin", "∉" }, { "ni", "∋" },
			{ "subset", "⊂" }, { "supset", "⊃" }, { "subseteq", "⊆" }, { "supseteq", "⊇" },
			{ "cup", "∪" }, { "cap", "∩" }, { "wedge", "∧" }, { "land", "∧" }, { "vee", "∨" },
			{ "lor", "∨" }, { "oplus", "⊕" }, { "otimes", "⊗" }, { "perp", "⊥" },
			{ "parallel", "∥" }, { "to", "→" }, { "rightarrow", "→" }, { "leftarrow", "←" },
			{ "leftrightarrow", "↔" }, { "Rightarrow", "⇒" }, { "Leftarrow", "⇐" },
			{ "Leftrightarrow", "⇔" }, { "iff", "⇔" }, { "implies", "⇒" }, { "mapsto", "↦" },
			{ "uparrow", "↑" }, { "downarrow", "↓" }, { "forall", "∀" }, { "exists", "∃" },
			{ "neg", "¬" }, { "lnot", "¬" }, { "angle", "∠" }, { "therefore", "∴" },
			{ "because", "∵" }, { "prime", "′" }, { "ldots", "…" }, { "dots", "…" },
			{ "cdots", "⋯" }, { "vdots", "⋮" }, { "ddots", "⋱" },
			{ "sum", "∑" }, { "prod", "∏" }, { "coprod", "∐" }, { "int", "∫" },
			{ "iint", "∬" }, { "iiint", "∭" }, { "oint", "∮" },
			{ "bigcup", "⋃" }, { "bigcap", "⋂" },
			{ "lbrace", "{" }, { "rbrace", "}" }, { "langle", "⟨" }, { "rangle", "⟩" },
			{ "lceil", "⌈" }, { "rceil", "⌉" }, { "lfloor", "⌊" }, { "rfloor", "⌋" },
			{ "vert", "|" }, { "lvert", "|" }, { "rvert", "|" }, { "Vert", "‖" },
			{ "backslash", "\\" }
		};

		// operators whose scripts are placed above and below rather than beside
		private static readonly HashSet<string> LimitOperators = new() { "∑", "∏", "∐", "⋃", "⋂" };

		private static readonly HashSet<string> LimitFunctions = new()
		{
			"lim", "max", "min", "sup", "inf", "liminf", "limsup"
		};

		private static readonly HashSet<string> Functions = new()
		{
			"sin", "cos", "tan", "cot", "sec", "csc", "sinh", "cosh", "tanh", "coth",
			"arcsin", "arccos", "arctan", "log", "ln", "lg", "exp", "lim", "liminf", "limsup",
			"max", "min", "sup", "inf", "det", "dim", "gcd", "arg", "deg", "ker", "Pr", "hom"
		};

		private static readonly Dictionary<string, string> OverAccents = new()
		{
			{ "hat", "^" }, { "widehat", "^" }, { "bar", "¯" }, { "overline", "¯" },
			{ "tilde", "~" }, { "widetilde", "~" }, { "vec", "→" }, { "overrightarrow", "→" },
			{ "dot", "˙" }, { "ddot", "¨" }, { "overbrace", "⏞" }
		};

		private static readonly Dictionary<string, string> UnderAccents = new()
		{
			{ "underline", "_" }, { "underbrace", "⏟" }
		};

		private static readonly Dictionary<string, string> MatrixFences = new()
		{
			{ "matrix", null }, { "pmatrix", "()" }, { "bmatrix", "[]" }, { "Bmatrix", "{}" },
			{ "vmatrix", "||" }, { "Vmatrix", "‖‖" }, { "cases", "{" },
			{ "array", null }, { "aligned", null }, { "align", null }, { "split", null }
		};


		/// <summary>
		/// Converts LaTeX to a MathML string suitable for a OneNote equation
		/// </summary>
		/// <param name="latex">LaTeX without surrounding delimiters</param>
		/// <param name="block">True to mark the equation display="block"</param>
		/// <param name="mathml">The resulting markup, with non-ASCII as numeric entities</param>
		/// <returns>True if the LaTeX was fully understood and converted</returns>
		public static bool TryConvert(string latex, bool block, out string mathml)
		{
			mathml = null;
			if (string.IsNullOrWhiteSpace(latex))
			{
				return false;
			}

			try
			{
				var items = new Parser(latex).ParseAll();
				if (items.Count == 0)
				{
					return false;
				}

				var math = new XElement(M + "math");
				if (block)
				{
					math.SetAttributeValue("display", "block");
				}

				math.Add(items);

				mathml = EncodeNonAscii(math.ToString(SaveOptions.DisableFormatting));
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}


		private static string EncodeNonAscii(string text)
		{
			var sb = new StringBuilder(text.Length);
			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];
				if (c < 128)
				{
					sb.Append(c);
				}
				else if (char.IsHighSurrogate(c) && i + 1 < text.Length)
				{
					sb.Append("&#").Append(char.ConvertToUtf32(c, text[++i])).Append(';');
				}
				else
				{
					sb.Append("&#").Append((int)c).Append(';');
				}
			}

			return sb.ToString();
		}


		private sealed class Parser
		{
			private readonly string s;
			private int pos;

			public Parser(string text)
			{
				s = text;
			}


			public List<XElement> ParseAll()
			{
				var items = ParseSequence();
				if (pos < s.Length)
				{
					// stray }, &, \\, \end or \right
					throw new FormatException();
				}

				return items;
			}


			private static XElement El(string name, string value = null)
			{
				return value is null ? new XElement(M + name) : new XElement(M + name, value);
			}


			private static XElement Row(IEnumerable<XElement> items)
			{
				var list = items.ToList();
				return list.Count == 1 ? list[0] : new XElement(M + "mrow", list);
			}


			private void SkipWhitespace()
			{
				while (pos < s.Length && char.IsWhiteSpace(s[pos]))
				{
					pos++;
				}
			}


			private bool AtStop()
			{
				SkipWhitespace();
				if (pos >= s.Length)
				{
					return true;
				}

				if (s[pos] == '}' || s[pos] == '&')
				{
					return true;
				}

				return s[pos] == '\\' && (
					(pos + 1 < s.Length && s[pos + 1] == '\\') ||
					StartsWithCommand("end") ||
					StartsWithCommand("right"));
			}


			private bool StartsWithCommand(string name)
			{
				var end = pos + 1 + name.Length;
				return string.CompareOrdinal(s, pos + 1, name, 0, name.Length) == 0 &&
					(end >= s.Length || !char.IsLetter(s[end]));
			}


			private List<XElement> ParseSequence()
			{
				var list = new List<XElement>();
				while (!AtStop())
				{
					var atom = ParseAtom(false);
					if (atom is null)
					{
						continue;
					}

					list.Add(ParseScripts(atom));
				}

				return list;
			}


			private XElement ParseGroup()
			{
				// positioned at '{'
				pos++;
				var items = ParseSequence();
				if (pos >= s.Length || s[pos] != '}')
				{
					throw new FormatException();
				}

				pos++;
				return items.Count == 0 ? El("mrow") : Row(items);
			}


			// text between balanced braces, without parsing it
			private string RawGroup()
			{
				SkipWhitespace();
				if (pos >= s.Length || s[pos] != '{')
				{
					throw new FormatException();
				}

				var depth = 0;
				var start = pos + 1;
				for (; pos < s.Length; pos++)
				{
					if (s[pos] == '{')
					{
						depth++;
					}
					else if (s[pos] == '}' && --depth == 0)
					{
						var text = s.Substring(start, pos - start);
						pos++;
						return text;
					}
				}

				throw new FormatException();
			}


			private XElement ParseArg()
			{
				SkipWhitespace();
				if (pos >= s.Length)
				{
					throw new FormatException();
				}

				if (s[pos] == '{')
				{
					return ParseGroup();
				}

				if (s[pos] == '}' || s[pos] == '&')
				{
					throw new FormatException();
				}

				return ParseAtom(true) ?? throw new FormatException();
			}


			private XElement ParseScripts(XElement basis)
			{
				XElement sub = null;
				XElement sup = null;

				while (true)
				{
					SkipWhitespace();
					if (pos >= s.Length)
					{
						break;
					}

					var c = s[pos];
					if (c == '_' && sub is null)
					{
						pos++;
						sub = ParseArg();
					}
					else if (c == '^' && sup is null)
					{
						pos++;
						sup = ParseArg();
					}
					else if (c == '\'' && sup is null)
					{
						pos++;
						sup = El("mo", "′");
					}
					else
					{
						break;
					}
				}

				if (sub is null && sup is null)
				{
					return basis;
				}

				var limits = basis.Name == M + "mo" && LimitOperators.Contains(basis.Value) ||
					basis.Name == M + "mi" && LimitFunctions.Contains(basis.Value);

				if (sub is not null && sup is not null)
				{
					return new XElement(M + (limits ? "munderover" : "msubsup"), basis, sub, sup);
				}

				if (sub is not null)
				{
					return new XElement(M + (limits ? "munder" : "msub"), basis, sub);
				}

				return new XElement(M + (limits ? "mover" : "msup"), basis, sup);
			}


			// single determines whether a digit run is consumed whole or one digit only
			private XElement ParseAtom(bool single)
			{
				SkipWhitespace();
				var c = s[pos];

				if (c == '{')
				{
					return ParseGroup();
				}

				if (c == '\\')
				{
					pos++;
					return ParseCommand();
				}

				if (char.IsDigit(c))
				{
					var start = pos++;
					while (!single && pos < s.Length &&
						(char.IsDigit(s[pos]) ||
						(s[pos] == '.' && pos + 1 < s.Length && char.IsDigit(s[pos + 1]))))
					{
						pos++;
					}

					return El("mn", s.Substring(start, pos - start));
				}

				pos++;

				if (char.IsLetter(c))
				{
					return El("mi", c.ToString());
				}

				switch (c)
				{
					case '~': return El("mspace");
					case '-': return El("mo", "−");
					case '*': return El("mo", "∗");
					case '^':
					case '_':
					case '$':
						throw new FormatException();
					default:
						return El("mo", c.ToString());
				}
			}


			private XElement ParseCommand()
			{
				if (pos >= s.Length)
				{
					throw new FormatException();
				}

				string name;
				if (char.IsLetter(s[pos]))
				{
					var start = pos;
					while (pos < s.Length && char.IsLetter(s[pos]))
					{
						pos++;
					}

					name = s.Substring(start, pos - start);
				}
				else
				{
					name = s[pos++].ToString();
				}

				switch (name)
				{
					case "frac":
					case "dfrac":
					case "tfrac":
						return new XElement(M + "mfrac", ParseArg(), ParseArg());

					case "binom":
						{
							var frac = new XElement(M + "mfrac", ParseArg(), ParseArg());
							frac.SetAttributeValue("linethickness", "0");
							return new XElement(M + "mrow", El("mo", "("), frac, El("mo", ")"));
						}

					case "sqrt":
						return ParseSqrt();

					case "text":
					case "textrm":
					case "textbf":
					case "textit":
					case "mbox":
						return El("mtext", RawGroup());

					case "mathrm":
					case "operatorname":
					case "mathbf":
					case "mathit":
					case "mathsf":
					case "mathtt":
					case "mathcal":
					case "mathbb":
						return ParseStyled(name);

					case "left":
						return ParseFenced();

					case "begin":
						return ParseEnvironment();

					case "end":
					case "right":
						throw new FormatException();

					case ",": return Space("0.17em");
					case ":": return Space("0.22em");
					case ";": return Space("0.28em");
					case "!": return Space("-0.17em");
					case " ": return Space("0.28em");
					case "quad": return Space("1em");
					case "qquad": return Space("2em");

					case "displaystyle":
					case "textstyle":
					case "scriptstyle":
					case "limits":
					case "nolimits":
						return null;

					case "{":
					case "}":
					case "%":
					case "$":
					case "&":
					case "#":
					case "_":
						return El("mo", name);

					case "|":
						return El("mo", "‖");
				}

				if (OverAccents.TryGetValue(name, out var over))
				{
					var accent = new XElement(M + "mo", over);
					var element = new XElement(M + "mover", ParseArg(), accent);
					element.SetAttributeValue("accent", "true");
					return element;
				}

				if (UnderAccents.TryGetValue(name, out var under))
				{
					return new XElement(M + "munder", ParseArg(), El("mo", under));
				}

				if (Functions.Contains(name))
				{
					return El("mi", name);
				}

				var greek = Array.IndexOf(GreekLower, name);
				if (greek >= 0)
				{
					// OneNote renders lowercase greek from the math-italic block
					return El("mi", char.ConvertFromUtf32(0x1D6FC + greek));
				}

				if (GreekUpper.TryGetValue(name, out var upper))
				{
					return El("mi", upper);
				}

				if (Identifiers.TryGetValue(name, out var identifier))
				{
					return El("mi", identifier);
				}

				if (Operators.TryGetValue(name, out var op))
				{
					return El("mo", op);
				}

				throw new FormatException();
			}


			private static XElement Space(string width)
			{
				var space = El("mspace");
				space.SetAttributeValue("width", width);
				return space;
			}


			private XElement ParseSqrt()
			{
				SkipWhitespace();
				if (pos < s.Length && s[pos] == '[')
				{
					var close = s.IndexOf(']', pos);
					if (close < 0)
					{
						throw new FormatException();
					}

					var index = new Parser(s.Substring(pos + 1, close - pos - 1)).ParseAll();
					pos = close + 1;
					return new XElement(M + "mroot", ParseArg(), Row(index));
				}

				return new XElement(M + "msqrt", ParseArg());
			}


			private XElement ParseStyled(string command)
			{
				var raw = RawGroup();

				switch (command)
				{
					case "mathbb":
						return El("mi", string.Concat(raw.Select(Blackboard)));

					case "operatorname":
					case "mathrm":
						{
							var mi = El("mi", raw);
							if (command == "mathrm")
							{
								mi.SetAttributeValue("mathvariant", "normal");
							}

							return mi;
						}

					case "mathbf":
						{
							var mi = El("mi", raw);
							mi.SetAttributeValue("mathvariant", "bold");
							return mi;
						}

					default:
						{
							var items = new Parser(raw).ParseAll();
							return items.Count == 0 ? El("mrow") : Row(items);
						}
				}
			}


			private static string Blackboard(char c)
			{
				switch (c)
				{
					case 'R': return "ℝ";
					case 'N': return "ℕ";
					case 'Z': return "ℤ";
					case 'Q': return "ℚ";
					case 'C': return "ℂ";
					case 'H': return "ℍ";
					case 'P': return "ℙ";
					default:
						return c >= 'A' && c <= 'Z'
							? char.ConvertFromUtf32(0x1D538 + c - 'A')
							: c.ToString();
				}
			}


			private string ReadDelimiter()
			{
				SkipWhitespace();
				if (pos >= s.Length)
				{
					throw new FormatException();
				}

				var c = s[pos++];
				if (c == '.')
				{
					return string.Empty;
				}

				if (c != '\\')
				{
					return c.ToString();
				}

				if (pos >= s.Length)
				{
					throw new FormatException();
				}

				if (!char.IsLetter(s[pos]))
				{
					var symbol = s[pos++];
					return symbol == '|' ? "‖" : symbol.ToString();
				}

				var start = pos;
				while (pos < s.Length && char.IsLetter(s[pos]))
				{
					pos++;
				}

				return Operators.TryGetValue(s.Substring(start, pos - start), out var op)
					? op
					: throw new FormatException();
			}


			private XElement ParseFenced()
			{
				var open = ReadDelimiter();
				var items = ParseSequence();

				if (!StartsWithCommand("right"))
				{
					throw new FormatException();
				}

				pos += "\\right".Length;
				var close = ReadDelimiter();

				var row = new XElement(M + "mrow");
				if (open.Length > 0)
				{
					row.Add(El("mo", open));
				}

				row.Add(items);

				if (close.Length > 0)
				{
					row.Add(El("mo", close));
				}

				return row;
			}


			private XElement ParseEnvironment()
			{
				var env = RawGroup().Trim();
				if (!MatrixFences.TryGetValue(env, out var fences))
				{
					throw new FormatException();
				}

				// array takes a column spec argument, e.g. \begin{array}{cc}
				SkipWhitespace();
				if (env == "array" && pos < s.Length && s[pos] == '{')
				{
					RawGroup();
				}

				var rows = new List<List<XElement>>();
				var cells = new List<XElement>();

				while (true)
				{
					var items = ParseSequence();
					cells.Add(new XElement(M + "mtd", items.Count == 0 ? El("mrow") : Row(items)));

					if (pos >= s.Length)
					{
						throw new FormatException();
					}

					if (s[pos] == '&')
					{
						pos++;
					}
					else if (s[pos] == '\\' && s[pos + 1] == '\\')
					{
						pos += 2;
						rows.Add(cells);
						cells = new List<XElement>();
					}
					else if (StartsWithCommand("end"))
					{
						pos += "\\end".Length;
						if (RawGroup().Trim() != env)
						{
							throw new FormatException();
						}

						rows.Add(cells);
						break;
					}
					else
					{
						throw new FormatException();
					}
				}

				// a trailing \\ leaves a final row holding only an empty cell
				if (rows.Count > 1 && rows[rows.Count - 1].Count == 1 &&
					string.IsNullOrEmpty(rows[rows.Count - 1][0].Value))
				{
					rows.RemoveAt(rows.Count - 1);
				}

				var table = new XElement(M + "mtable",
					rows.Select(r => new XElement(M + "mtr", r)));

				if (fences is null)
				{
					return table;
				}

				var result = new XElement(M + "mrow");
				if (fences == "{")
				{
					result.Add(El("mo", "{"), table);
				}
				else
				{
					result.Add(El("mo", fences.Substring(0, fences.Length / 2)), table,
						El("mo", fences.Substring(fences.Length / 2)));
				}

				return result;
			}
		}
	}
}
