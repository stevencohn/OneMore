﻿//************************************************************************************************
// Copyright © 2020 Steven M Cohn.  All rights reserved.
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

		private int indexOffset;


		/// <summary>
		/// Initialize a new instance
		/// </summary>
		public Calculator()
		{
		}


		/// <summary>
		/// Evaluate the given expression and returns the result
		/// </summary>
		/// <param name="expression">The expression to evaluate</param>
		/// <param name="indexOffset">
		/// The positional index of the expression in table rows. This is a specialized value
		/// used for OneMore table formulas where the user asked for a row-offset calculation to
		/// accomodate a dynamically growing table of rows without having to update the formula.
		/// </param>
		/// <returns></returns>
		public double Execute(string expression, int indexOffset)
		{
			this.indexOffset = indexOffset;

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
			return ExecuteTokens(tokenized);
		}


		/// <summary>
		/// Converts a standard infix expression to list of tokens in
		/// postfix order.
		/// </summary>
		/// <param name="expression">Expression to evaluate</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug",
			"S2583:Conditionally executed code should be reachable", Justification = "<Pending>")]
		private List<string> TokenizeExpression(string expression)
		{
			var tokens = new List<string>();
			var stack = new Stack<string>();
			var state = State.None;
			int parenDepth = 0;
			string temp;

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

					// Allow additional unary operators after "("
					if (state == State.UnaryOperator)
						state = State.Operator;

					// push opening parenthesis onto stack
					stack.Push(parser.Peek().ToString());
					// track number of parentheses
					parenDepth++;
				}
				else if (parser.Peek() == ')')
				{
					// Must follow operand
					if (state != State.Operand)
						throw new FormulaException(ErrOperandExpected, parser.Position);
					// Must have matching open parenthesis
					if (parenDepth == 0)
						throw new FormulaException(ErrUnmatchedClosingParen, parser.Position);
					// Pop all operators until matching "(" found
					temp = stack.Pop();
					while (temp != "(")
					{
						tokens.Add(temp);
						temp = stack.Pop();
					}
					// Track number of parentheses
					parenDepth--;
				}
				else if ("+-*/^".Contains(parser.Peek()))
				{
					// Need a bit of extra code to support unary operators
					if (state == State.Operand)
					{
						// Pop operators with precedence >= current operator
						int currPrecedence = GetPrecedence(parser.Peek().ToString());
						while (stack.Count > 0 && GetPrecedence(stack.Peek()) >= currPrecedence)
							tokens.Add(stack.Pop());
						stack.Push(parser.Peek().ToString());
						state = State.Operator;
					}
					else if (state == State.UnaryOperator)
					{
						// Don't allow two unary operators together
						throw new FormulaException(ErrOperandExpected, parser.Position);
					}
					else
					{
						// Test for unary operator
						if (parser.Peek() == '-')
						{
							// Push unary minus
							stack.Push(UnaryMinus);
							state = State.UnaryOperator;
						}
						else if (parser.Peek() == '+')
						{
							// Just ignore unary plus
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
					if (state == State.Operand)
					{
						// Cannot follow other operand
						throw new FormulaException(ErrOperatorExpected, parser.Position);
					}
					// Parse number
					temp = ParseNumberToken(parser);
					tokens.Add(temp);
					state = State.Operand;
					continue;
				}
				else
				{
					// Parse symbols and functions
					if (state == State.Operand)
					{
						// Symbol or function cannot follow other operand
						throw new FormulaException(ErrOperatorExpected, parser.Position);
					}

					var c = parser.Peek();
					if (!(char.IsLetter(c) || c == '<' || c == '>' || c == '!' || c == '_'))
					{
						// Invalid character
						temp = string.Format(ErrUnexpectedCharacter, parser.Peek());
						throw new FormulaException(temp, parser.Position);
					}

					// save start of symbol for error reporting
					int symbolPos = parser.Position;
					// parse this symbol
					temp = ParseSymbolToken(parser);
					// skip whitespace
					parser.MovePastWhitespace();
					// check for parameter list
					FormulaValue result;
					if (parser.Peek() == '(')
					{
						// found parameter list, evaluate function
						result = EvaluateFunction(parser, temp, symbolPos);
					}
					else
					{
						// no parameter list, evaluate symbol (variable)
						result = EvaluateSymbol(temp, symbolPos);
					}

					// handle negative result
					if (result.Type == FormulaValueType.Double && result.DoubleValue < 0)
					{
						stack.Push(UnaryMinus);
						result = new FormulaValue(Math.Abs(result.DoubleValue));
					}

					tokens.Add(result.ToString());
					state = State.Operand;
					continue;
				}
				parser.MoveAhead();
			}
			// Expression cannot end with operator
			if (state == State.Operator || state == State.UnaryOperator)
				throw new FormulaException(ErrOperandExpected, parser.Position);
			// Check for balanced parentheses
			if (parenDepth > 0)
				throw new FormulaException(ErrClosingParenExpected, parser.Position);
			// Retrieve remaining operators from stack
			while (stack.Count > 0)
				tokens.Add(stack.Pop());
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
			while (char.IsLetterOrDigit(c) || c == '<' || c == '>' || c == '!' || c == '_' ||
				(c == '-' && indexOffset > 0))
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

			var Fn = MathFunctions.Find(name);
			if (Fn != null)
			{
				return new FormulaValue(Fn(parameters));
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
				if (next == ':')
				{
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

			if (int.Parse(match.Groups["r"].Value) == 0)
			{
				// do not include result cell (or off-table) in calculations
				throw new FormulaException("row offset cannot be zero");
			}

			var col1 = match.Groups["c"].Value;
			var row1 = match.Groups["r"].Value;

			// cell2...

			match = Regex.Match(cell2, Processor.OffsetPattern);
			if (!match.Success)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, cell2), p2);

			if (int.Parse(match.Groups["r"].Value) == 0)
			{
				// do not include result cell (or off-table) in calculations
				throw new FormulaException("row offset cannot be zero");
			}

			var col2 = match.Groups["c"].Value;
			var row2 = match.Groups["o"].Success && indexOffset > 0
				? $"{indexOffset - int.Parse(match.Groups["r"].Value)}"
				: match.Groups["r"].Value;

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
				var args = new SymbolEventArgs(name, indexOffset);

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
		/// Evaluates the given list of tokens and returns the result.
		/// Tokens must appear in postfix order.
		/// </summary>
		/// <param name="tokens">List of tokens to evaluate.</param>
		/// <returns></returns>
		private static FormulaValue ExecuteTokens(List<string> tokens)
		{
			var stack = new Stack<FormulaValue>();

			foreach (string token in tokens)
			{
				// TryParse is more performance and complete than regex
				if (double.TryParse(token, out var d)) // Culture-specific user input?!
				{
					stack.Push(new FormulaValue(d));
				}
				else if (token == "+")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(v1.DoubleValue + v2.DoubleValue));
					else
						stack.Push(new FormulaValue(v1.ToString() + v2.ToString()));
				}
				else if (token == "-")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(v1.DoubleValue - v2.DoubleValue));
				}
				else if (token == "*")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(v1.DoubleValue * v2.DoubleValue));
				}
				else if (token == "/")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(v1.DoubleValue / v2.DoubleValue));
				}
				else if (token == "^")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double && v2.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(Math.Pow(v1.DoubleValue, v2.DoubleValue)));
				}
				else if (token == UnaryMinus)
				{
					var v1 = stack.Pop();
					if (v1.Type == FormulaValueType.Double)
						stack.Push(new FormulaValue(-v1.DoubleValue));
				}
				else if (bool.TryParse(token, out var b))
				{
					stack.Push(new FormulaValue(b));
				}
				else
				{
					stack.Push(new FormulaValue(token));
				}
			}

			// remaining item on stack contains result
			return stack.Count > 0 ? stack.Pop() : new FormulaValue(0);
		}
	}
}
