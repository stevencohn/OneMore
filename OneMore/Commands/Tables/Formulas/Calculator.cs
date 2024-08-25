//************************************************************************************************
// Copyright © 2020 Steven M Cohn. All rights reserved.
//************************************************************************************************
/*
 * Based on the C# Expression Evaluator by Jonathan Wood, 2010
 *
 * This source code and all associated files and resources are copyrighted by
 * the author(s). This source code and all associated files and resources may
 * be used as long as they are used according to the terms and conditions set
 * forth in The Code Project Open License (CPOL), which may be viewed at
 * http://www.blackbeltcoder.com/Legal/Licenses/CPOL.
 * Copyright (c) 2010 Jonathan Wood
 */

#pragma warning disable S1066 // Collapsible "if" statements should be merged

namespace River.OneMoreAddIn.Commands.Tables.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Resx = Properties.Resources;


	/// <summary>
	/// Expression evaluator
	/// </summary>
	internal class Calculator
	{
		// event handers
		public delegate void ProcessSymbolHandler(object sender, SymbolEventArgs e);
		public delegate void ProcessFunctionHandler(object sender, FunctionEventArgs e);

		public event ProcessSymbolHandler ProcessSymbol;
		public event ProcessFunctionHandler ProcessFunction;

		// token types
		private enum State
		{
			None = 0,
			Operand = 1,
			Operator = 2,
			UnaryOperator = 3
		}

		// error messages
		private readonly string ErrInvalidOperand = Resx.Calculator_ErrInvalidOperand;
		private readonly string ErrOperandExpected = Resx.Calculator_ErrOperandExpected;
		private readonly string ErrOperatorExpected = Resx.Calculator_ErrOperatorExpected;
		private readonly string ErrUnmatchedClosingParen = Resx.Calculator_ErrUnmatchedClosingParen;
		private readonly string ErrMultipleDecimalPoints = Resx.Calculator_ErrMultipleDecimalPoints;
		private readonly string ErrUnexpectedCharacter = Resx.Calculator_ErrUnexpectedCharacter;
		private readonly string ErrUndefinedSymbol = Resx.Calculator_ErrUndefinedSymbol;
		private readonly string ErrUndefinedFunction = Resx.Calculator_ErrUndefinedFunction;
		private readonly string ErrClosingParenExpected = Resx.Calculator_ErrClosingParenExpected;
		private readonly string ErrWrongParamCount = Resx.Calculator_ErrWrongParamCount;
		private readonly string ErrInvalidCellRange = Resx.Calculator_ErrInvalidCellRange;

		// To distinguish it from minus operator, use char unlikely to appear in expressions
		// to signify a unary negative; 0x80 follows DEL/0x7F just out of ASCII range
		private const string UnaryMinus = "\x80";

		private readonly Models.Table table;
		private Coordinates coordinates;
		private FunctionFactory factory;


		/// <summary>
		/// Initialize a new instance
		/// </summary>
		public Calculator(Models.Table table)
		{
			this.table = table;
		}


		/// <summary>
		/// Evaluate the given expression and returns the result
		/// </summary>
		/// <param name="expression">The expression to evaluate</param>
		/// <param name="colNumber"></param>
		/// <param name="rowNumber"></param>
		/// <returns></returns>
		public double Execute(string expression, int colNumber, int rowNumber)
		{
			coordinates = new Coordinates(colNumber, rowNumber);

			var result = ExecuteInternal(expression);
			if (result.Type == FormulaValueType.Double)
			{
				return result.DoubleValue;
			}
			else if (result.Type == FormulaValueType.Boolean)
			{
				return (bool)result.Value ? 1 : 0;
			}

			throw new FormulaException($"expression cannot result in a string value");
		}


		private FormulaValue ExecuteInternal(string expression)
		{
			var tokenized = TokenizeExpression(expression);

			foreach (var token in tokenized)
			{
				Logger.Current.Verbose($"tokenized [{token}]");
			}


			return ExecuteTokens(tokenized);
		}


		/// <summary>
		/// Converts a standard infix expression to list of tokens in
		/// postfix order.
		/// </summary>
		/// <param name="expression">Expression to evaluate</param>
		/// <returns></returns>s
		private List<string> TokenizeExpression(string expression)
		{
			var tokens = new List<string>();
			var stack = new Stack<string>();
			var state = State.None;
			int parenDepth = 0;

			var parser = new TextParser(expression);

			while (!parser.EndOfText)
			{
				if (char.IsWhiteSpace(parser.Peek()))
				{
					// ignore spaces, tabs, etc.
				}
				else if (parser.Peek() == '(')
				{
					// cannot follow operand
					if (state == State.Operand)
						throw new FormulaException(ErrOperatorExpected, parser.Position);

					// allow additional unary operators after "("
					if (state == State.UnaryOperator)
					{
						state = State.Operator;
					}

					// push opening parenthesis onto stack
					stack.Push(parser.Peek().ToString());
					// track number of parentheses
					parenDepth++;
				}
				else if (parser.Peek() == ')')
				{
					// must follow operand
					if (state != State.Operand)
						throw new FormulaException(ErrOperandExpected, parser.Position);

					// must have matching open parenthesis
					if (parenDepth == 0)
						throw new FormulaException(ErrUnmatchedClosingParen, parser.Position);

					// pop all operators until matching "(" found
					var token = stack.Pop();
					while (token != "(")
					{
						tokens.Add(token);
						token = stack.Pop();
					}

					// track number of parentheses
					parenDepth--;
				}
				else if ("+-*/^".Contains(parser.Peek()))
				{
					// need a bit of extra code to support unary operators
					if (state == State.Operand)
					{
						var precedence = GetPrecedence(parser.Peek().ToString());

						// pop operators with precedence >= current operator
						while (stack.Count > 0 && GetPrecedence(stack.Peek()) >= precedence)
						{
							tokens.Add(stack.Pop());
						}

						stack.Push(parser.Peek().ToString());
						state = State.Operator;
					}
					else if (state == State.UnaryOperator)
					{
						// don't allow two unary operators together
						throw new FormulaException(ErrOperandExpected, parser.Position);
					}
					else
					{
						// test for unary operator
						if (parser.Peek() == '-')
						{
							// push unary minus
							stack.Push(UnaryMinus);
							state = State.UnaryOperator;
						}
						else if (parser.Peek() == '+')
						{
							// just ignore unary plus
							state = State.UnaryOperator;
						}
						else
						{
							throw new FormulaException(ErrOperandExpected, parser.Position);
						}
					}
				}
				else if (char.IsDigit(parser.Peek()) || parser.Peek() == '.')
				{
					// cannot follow other operand
					if (state == State.Operand)
						throw new FormulaException(ErrOperatorExpected, parser.Position);

					// parse number
					var token = ParseNumberToken(parser);
					tokens.Add(token);
					state = State.Operand;

					continue;
				}
				else
				{
					// parse symbols and functions...

					// symbol or function cannot follow other operand
					if (state == State.Operand)
						throw new FormulaException(ErrOperatorExpected, parser.Position);

					var c = parser.Peek();

					// the chars [<>!] support countif second parameter
					if (!(char.IsLetter(c) || c == '<' || c == '>' || c == '!'))
					{
						// Invalid character
						var msg = string.Format(ErrUnexpectedCharacter, parser.Peek());
						throw new FormulaException(msg, parser.Position);
					}

					// save start of symbol for error reporting
					var symbolPos = parser.Position;

					// parse this symbol
					var token = ParseSymbolToken(parser);
					// skip whitespace
					parser.MovePastWhitespace();

					// check for parameter list
					FormulaValue value;
					if (parser.Peek() == '(')
					{
						// found parameter list, evaluate function
						value = EvaluateFunction(parser, token, symbolPos);
					}
					else
					{
						// no parameter list, evaluate symbol (variable)
						value = EvaluateSymbol(token, symbolPos);
					}

					// handle negative result
					if (value.Type == FormulaValueType.Double && value.DoubleValue < 0)
					{
						stack.Push(UnaryMinus);
						value = new FormulaValue(Math.Abs(value.DoubleValue));
					}

					tokens.Add(value.ToString());
					state = State.Operand;

					continue;
				}

				parser.MoveAhead();
			}

			// expression cannot end with operator
			if (state == State.Operator || state == State.UnaryOperator)
				throw new FormulaException(ErrOperandExpected, parser.Position);

			// check for balanced parentheses
			if (parenDepth > 0)
				throw new FormulaException(ErrClosingParenExpected, parser.Position);

			// retrieve remaining operators from stack
			while (stack.Count > 0)
			{
				tokens.Add(stack.Pop());
			}

			return tokens;
		}


		/// <summary>
		/// Parses and extracts a numeric value at the current position
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <returns></returns>
		private string ParseNumberToken(TextParser parser)
		{
			bool hasDecimal = false;
			int start = parser.Position;
			while (char.IsDigit(parser.Peek()) || parser.Peek() == '.')
			{
				if (parser.Peek() == '.')
				{
					if (hasDecimal)
						throw new FormulaException(ErrMultipleDecimalPoints, parser.Position);
					hasDecimal = true;
				}
				parser.MoveAhead();
			}
			// Extract token
			string token = parser.Extract(start, parser.Position);
			if (token == ".")
				throw new FormulaException(ErrInvalidOperand, parser.Position - 1);
			return token;
		}

		/// <summary>
		/// Parses and extracts a symbol at the current position comprised of valid name
		/// characters.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <returns></returns>
		private string ParseSymbolToken(TextParser parser)
		{
			int start = parser.Position;
			var c = parser.Peek();

			// the chars [<>!] support countif second parameter
			while (char.IsLetterOrDigit(c) || c == '<' || c == '>' || c == '!')
			{
				parser.MoveAhead();
				c = parser.Peek();
			}

			var token = parser.Extract(start, parser.Position);
			return token;
		}


		/// <summary>
		/// Evaluates a function and returns its value. It is assumed the current
		/// position is at the opening parenthesis of the argument list.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <param name="name">Name of function</param>
		/// <param name="pos">Position at start of function</param>
		/// <returns></returns>
		private FormulaValue EvaluateFunction(TextParser parser, string name, int pos)
		{
			// parse function parameters
			var parameters = ParseParameters(parser);

			if (factory is null)
			{
				factory = new FunctionFactory();
			}

			var function = factory.Find(name);
			if (function is not null)
			{
				if (function.Spacial)
				{
					parameters.Add(new FormulaValue(table));
					parameters.Add(coordinates.ColNumber);
					parameters.Add(coordinates.RowNumber);
				}

				return new FormulaValue(function.Fn(parameters));
			}

			double result = default;

			// ask consumer to evaluate function
			if (ProcessFunction != null)
			{
				var args = new FunctionEventArgs
				{
					Name = name,
					Parameters = parameters,
					Result = result,
					Status = FunctionStatus.OK
				};

				ProcessFunction(this, args);
				if (args.Status == FunctionStatus.OK)
				{
					return new FormulaValue(args.Result);
				}

				if (args.Status == FunctionStatus.WrongParameterCount)
				{
					throw new FormulaException(ErrWrongParamCount, pos);
				}
			}

			throw new FormulaException(string.Format(ErrUndefinedFunction, name), pos);
		}


		/// <summary>
		/// Evaluates each parameter of a function's parameter list and returns
		/// a list of those values. An empty list is returned if no parameters
		/// were found. It is assumed the current position is at the opening
		/// parenthesis of the argument list.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <returns></returns>
		private FormulaValues ParseParameters(TextParser parser)
		{
			var parameters = new FormulaValues();

			// move past open parenthesis
			parser.MoveAhead();
			parser.MovePastWhitespace();

			// empty param list
			if (parser.Peek() == ')')
			{
				parser.MoveAhead();
				return parameters;
			}

			// collect parameters...


			// Parse function parameter list
			int start = parser.Position;
			int depth = 1;

			while (!parser.EndOfText)
			{
				var next = parser.Peek();
				Logger.Current.Verbose($"char [{next}]");
				if (next == ':')
				{
					parameters.Add(EvaluateParameter(parser, start));
					start = parser.Position + 1;

					Logger.Current.Verbose($"add :parameter [{parameters[parameters.Count - 1]}]");

					/*
					// assume current token and next token are cell references
					var p1 = parser.Position;
					var cell1 = parser.Extract(start, parser.Position);
					parser.MoveAhead();
					var p2 = parser.Position;
					var cell2 = ParseSymbolToken(parser);
					start = parser.Position;

					var values = EvaluateCellReferences(cell1, cell2, p1, p2).ToArray();
					for (int i = 0; i < values.Length; i++)
					{
						parameters.Add(values[i]);
					}
					*/
				}
				else if (next == ',')
				{
					// Note: Ignore commas inside parentheses. They could be
					// from a parameter list for a function inside the parameters
					if (depth == 1)
					{
						// evaluate the string prior to the comma
						parameters.Add(EvaluateParameter(parser, start));
						start = parser.Position + 1;

						Logger.Current.Verbose($"add ,parameter [{parameters[parameters.Count - 1]}]");
					}
				}

				next = parser.Peek();
				if (next == ')')
				{
					depth--;
					if (depth == 0)
					{
						if (start < parser.Position)
						{
							if (parser.PeekAt(start) == ',')
							{
								start++;
							}

							parameters.Add(EvaluateParameter(parser, start));

							Logger.Current.Verbose($"add )parameter [{parameters[parameters.Count - 1]}]");
						}
						break;
					}
				}
				else if (next == '(')
				{
					depth++;
				}

				parser.MoveAhead();
			}

			// make sure we found a closing parenthesis
			if (depth > 0)
				throw new FormulaException(ErrClosingParenExpected, parser.Position);

			// move past closing parenthesis
			parser.MoveAhead();
			return parameters;
		}


		private FormulaValues EvaluateCellReferences(string cell1, string cell2, int p1, int p2)
		{
			// cell1...

			var match = Regex.Match(cell1, Processor.AddressPattern);
			if (!match.Success)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, cell1), p1);

			var col1 = match.Groups[1].Value;
			var row1 = match.Groups[2].Value;

			match = Regex.Match(cell2, Processor.AddressPattern);
			if (!match.Success)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, cell2), p2);

			var col2 = match.Groups[1].Value;
			var row2 = match.Groups[2].Value;

			// validate...

			var values = new FormulaValues();
			if (col1 == col2)
			{
				// iterate rows in column
				for (var row = int.Parse(row1); row <= int.Parse(row2); row++)
				{
					values.Add(EvaluateSymbol($"{col1}{row}", p1));
				}
			}
			else if (row1 == row2)
			{
				// iterate columns in row
				for (var col = CellLettersToIndex(col1); col <= CellLettersToIndex(col2); col++)
				{
					var v = EvaluateSymbol($"{CellIndexToLetters(col)}{row1}", p1);
					if (v.Type == FormulaValueType.Unknown)
					{
						throw new FormulaException($"invalid parameter at cell {CellIndexToLetters(col)}{row1}");
					}

					values.Add(v);
				}
			}
			else
				throw new FormatException(ErrInvalidCellRange);

			return values;
		}

		private static string CellIndexToLetters(int index)
		{
			int div = index;
			string letters = string.Empty;
			int mod;

			while (div > 0)
			{
				mod = (div - 1) % 26;
				letters = $"{(char)(65 + mod)}{letters}";
				div = ((div - mod) / 26);
			}
			return letters;
		}

		private static int CellLettersToIndex(string letters)
		{
			letters = letters.ToUpper();
			int sum = 0;

			for (int i = 0; i < letters.Length; i++)
			{
				sum *= 26;
				sum += (letters[i] - 'A' + 1);
			}
			return sum;
		}



		/// <summary>
		/// Extracts and evaluates a function parameter and returns its value. If an
		/// exception occurs, it is caught and the column is adjusted to reflect the
		/// position in original string, and the exception is rethrown.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <param name="paramStart">Column where this parameter started</param>
		/// <returns></returns>
		private FormulaValue EvaluateParameter(TextParser parser, int paramStart)
		{
			string expression = string.Empty;
			try
			{
				// Extract expression and evaluate it
				expression = parser.Extract(paramStart, parser.Position).Trim();
				return ExecuteInternal(expression);
			}
			catch (FormulaException ex)
			{
				// Adjust column and rethrow exception
				throw new FormulaException(
					$"{ex.PlainMessage}\n\"{expression}\"\n",
					ex.Column + paramStart);
			}
		}

		/// <summary>
		/// This method evaluates a symbol name and returns its value.
		/// </summary>
		/// <param name="name">Name of symbol</param>
		/// <param name="pos">Position at start of symbol</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style",
			"IDE0060:Remove unused parameter", Justification = "Future")]
		private FormulaValue EvaluateSymbol(string name, int pos)
		{
			// built-in symbols

			if (string.Compare(name, "pi", true) == 0)
			{
				return new FormulaValue(Math.PI);
			}
			else if (string.Compare(name, "e", true) == 0)
			{
				return new FormulaValue(Math.E);
			}

			// ask consumer to resolve symbol reference
			if (ProcessSymbol != null)
			{
				var args = new SymbolEventArgs(name, coordinates);

				ProcessSymbol(this, args);
				if (args.Status == SymbolStatus.OK)
				{
					if (args.Type == FormulaValueType.Double)
						return new FormulaValue(args.DoubleResult);

					if (args.Type == FormulaValueType.Boolean)
						return new FormulaValue((bool)args.Result);

					if (args.Type == FormulaValueType.String)
						return new FormulaValue((string)args.Result);
				}
			}

			return new FormulaValue(name);
		}


		/// <summary>
		/// Returns a value that indicates the relative precedence of
		/// the specified operator
		/// </summary>
		/// <param name="s">Operator to be tested</param>
		/// <returns></returns>
		private static int GetPrecedence(string s)
		{
			return s switch
			{
				":" => 1,
				"+" or "-" => 2,
				"*" or "/" => 3,
				"^" => 4,
				UnaryMinus => 10,
				_ => 0,
			};
		}


		/// <summary>
		/// Evaluates basic arithmetic with the given list of tokens and returns the result.
		/// Tokens must appear in postfix order.
		/// </summary>
		/// <param name="tokens">List of tokens to evaluate.</param>
		/// <returns></returns>
		private static FormulaValue ExecuteTokens(List<string> tokens)
		{
			var stack = new Stack<FormulaValue>();

			foreach (string token in tokens)
			{
				// TryParse is more performant and complete than regex
				if (double.TryParse(token, out var d)) // culture-specific user input?!
				{
					stack.Push(new FormulaValue(d));
				}
				else if (token == "+")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
					{
						// add
						stack.Push(new FormulaValue(v1.DoubleValue + v2.DoubleValue));
					}
					else
					{
						// concat strings
						stack.Push(new FormulaValue(v1.ToString() + v2.ToString()));
					}
				}
				else if (token == "-")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
					{
						// substract
						stack.Push(new FormulaValue(v1.DoubleValue - v2.DoubleValue));
					}
				}
				else if (token == "*")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
					{
						// multiply
						stack.Push(new FormulaValue(v1.DoubleValue * v2.DoubleValue));
					}
				}
				else if (token == "/")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
					{
						// divide
						stack.Push(new FormulaValue(v1.DoubleValue / v2.DoubleValue));
					}
				}
				else if (token == "^")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
					{
						// exponent
						stack.Push(new FormulaValue(Math.Pow(v1.DoubleValue, v2.DoubleValue)));
					}
				}
				else if (token == UnaryMinus)
				{
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double)
					{
						// negate
						stack.Push(new FormulaValue(-v1.DoubleValue));
					}
				}
				else if (bool.TryParse(token, out var b))
				{
					// bool
					stack.Push(new FormulaValue(b));
				}
				else
				{
					// string
					stack.Push(new FormulaValue(token));
				}
			}

			// remaining item on stack contains result
			return stack.Count > 0 ? stack.Pop() : new FormulaValue(0);
		}
	}
}
