//************************************************************************************************
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

namespace River.OneMoreAddIn.Commands.Formulas
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Resx = River.OneMoreAddIn.Properties.Resources;


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

		// To distinguish it from a minus operator, use a character unlikely to appear
		// in expressions to signify a unary negative
		private const string UnaryMinus = "\x80";


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
		/// <returns></returns>
		public double Execute(string expression)
		{
			return ExecuteTokens(TokenizeExpression(expression));
		}


		/// <summary>
		/// Converts a standard infix expression to list of tokens in
		/// postfix order.
		/// </summary>
		/// <param name="expression">Expression to evaluate</param>
		/// <returns></returns>
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
					double result;

					// Parse symbols and functions
					if (state == State.Operand)
					{
						// Symbol or function cannot follow other operand
						throw new FormulaException(ErrOperatorExpected, parser.Position);
					}
					if (!(char.IsLetter(parser.Peek()) || parser.Peek() == '_'))
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
					if (result < 0)
					{
						stack.Push(UnaryMinus);
						result = Math.Abs(result);
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
		/// Parses and extracts a symbol at the current position
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <returns></returns>
		private static string ParseSymbolToken(TextParser parser)
		{
			int start = parser.Position;
			while (char.IsLetterOrDigit(parser.Peek()) || parser.Peek() == '_')
				parser.MoveAhead();
			return parser.Extract(start, parser.Position);
		}


		/// <summary>
		/// Evaluates a function and returns its value. It is assumed the current
		/// position is at the opening parenthesis of the argument list.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <param name="name">Name of function</param>
		/// <param name="pos">Position at start of function</param>
		/// <returns></returns>
		private double EvaluateFunction(TextParser parser, string name, int pos)
		{
			// parse function parameters
			var parameters = ParseParameters(parser);

			var Fn = MathFunctions.Find(name);
			if (Fn != null)
			{
				return Fn(parameters.ToArray());
			}

			double result = default;

			// ask consumer to evaluate function
			var status = FunctionStatus.UndefinedFunction;
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

				result = args.Result;
				status = args.Status;
			}

			if (status == FunctionStatus.UndefinedFunction)
				throw new FormulaException(string.Format(ErrUndefinedFunction, name), pos);

			if (status == FunctionStatus.WrongParameterCount)
				throw new FormulaException(ErrWrongParamCount, pos);

			return result;
		}


		/// <summary>
		/// Evaluates each parameter of a function's parameter list and returns
		/// a list of those values. An empty list is returned if no parameters
		/// were found. It is assumed the current position is at the opening
		/// parenthesis of the argument list.
		/// </summary>
		/// <param name="parser">TextParser object</param>
		/// <returns></returns>
		private List<double> ParseParameters(TextParser parser)
		{
			// Move past open parenthesis
			parser.MoveAhead();

			// Look for function parameters
			var parameters = new List<double>();
			parser.MovePastWhitespace();
			if (parser.Peek() != ')')
			{
				// Parse function parameter list
				int paramStart = parser.Position;
				int pardepth = 1;

				while (!parser.EndOfText)
				{
					if (parser.Peek() == ':')
					{
						// assume current token and next token are cell references
						var p1 = parser.Position;
						var cell1 = parser.Extract(paramStart, parser.Position);
						parser.MoveAhead();
						var p2 = parser.Position;
						var cell2 = ParseSymbolToken(parser);
						paramStart = parser.Position;
						parameters.AddRange(EvaluateCellReferences(cell1, cell2, p1, p2));
					}
					else if (parser.Peek() == ',')
					{
						// Note: Ignore commas inside parentheses. They could be
						// from a parameter list for a function inside the parameters
						if (pardepth == 1)
						{
							parameters.Add(EvaluateParameter(parser, paramStart));
							paramStart = parser.Position + 1;
						}
					}

					if (parser.Peek() == ')')
					{
						pardepth--;
						if (pardepth == 0)
						{
							if (paramStart < parser.Position)
							{
								parameters.Add(EvaluateParameter(parser, paramStart));
							}
							break;
						}
					}
					else if (parser.Peek() == '(')
					{
						pardepth++;
					}
					parser.MoveAhead();
				}
			}
			// Make sure we found a closing parenthesis
			if (parser.Peek() != ')')
				throw new FormulaException(ErrClosingParenExpected, parser.Position);
			// Move past closing parenthesis
			parser.MoveAhead();
			// Return parameter list
			return parameters;
		}


		private List<double> EvaluateCellReferences(string cell1, string cell2, int p1, int p2)
		{
			var pattern = @"^([a-zA-Z]{1,3})(\d{1,3})$";

			var match = Regex.Match(cell1, pattern);
			if (!match.Success)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, cell1), p1);

			var col1 = match.Groups[1].Value;
			var row1 = match.Groups[2].Value;

			match = Regex.Match(cell2, pattern);
			if (!match.Success)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, cell2), p2);

			var col2 = match.Groups[1].Value;
			var row2 = match.Groups[2].Value;

			var values = new List<double>();
			if (col1 == col2)
			{
				// iterate rows in column
				for (var row = int.Parse(row1); row <= int.Parse(row2); row++)
				{
					var value = EvaluateSymbol($"{col1}{row}", p1);
					if (!double.IsNaN(value))
					{
						values.Add(value);
					}
				}
			}
			else if (row1 == row2)
			{
				// iterate columns in row
				for (var col = CellLettersToIndex(col1); col <= CellLettersToIndex(col2); col++)
				{
					values.Add(EvaluateSymbol($"{CellIndexToLetters(col)}{row1}", p1));
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
				letters = (char)(65 + mod) + letters;
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
		private double EvaluateParameter(TextParser parser, int paramStart)
		{
			try
			{
				// Extract expression and evaluate it
				string expression = parser.Extract(paramStart, parser.Position);
				return Execute(expression);
			}
			catch (FormulaException ex)
			{
				// Adjust column and rethrow exception
				ex.Column += paramStart;
				throw;
			}
		}

		/// <summary>
		/// This method evaluates a symbol name and returns its value.
		/// </summary>
		/// <param name="name">Name of symbol</param>
		/// <param name="pos">Position at start of symbol</param>
		/// <returns></returns>
		private double EvaluateSymbol(string name, int pos)
		{
			// built-in symbols

			if (string.Compare(name, "pi", true) == 0)
			{
				return Math.PI;
			}
			else if (string.Compare(name, "e", true) == 0)
			{
				return Math.E;
			}

			double result = default;

			// ask consumer to resolve symbol reference
			var status = SymbolStatus.UndefinedSymbol;
			if (ProcessSymbol != null)
			{
				var args = new SymbolEventArgs
				{
					Name = name,
					Result = result,
					Status = SymbolStatus.OK
				};

				ProcessSymbol(this, args);

				result = args.Result;
				status = args.Status;
			}

			if (status == SymbolStatus.UndefinedSymbol)
				throw new FormulaException(string.Format(ErrUndefinedSymbol, name), pos);

			if (status == SymbolStatus.None)
				result = 0;

			return result;
		}

		/// <summary>
		/// Evaluates the given list of tokens and returns the result.
		/// Tokens must appear in postfix order.
		/// </summary>
		/// <param name="tokens">List of tokens to evaluate.</param>
		/// <returns></returns>
		private static double ExecuteTokens(List<string> tokens)
		{
			var stack = new Stack<double>();

			foreach (string token in tokens)
			{
				// Is this a value token?
				int count = token.Count(c => char.IsDigit(c) || c == '.');
				if (count == token.Length)
				{
					stack.Push(double.Parse(token));
				}
				else if (token == "+")
				{
					stack.Push(stack.Pop() + stack.Pop());
				}
				else if (token == "-")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					stack.Push(v1 - v2);
				}
				else if (token == "*")
				{
					stack.Push(stack.Pop() * stack.Pop());
				}
				else if (token == "/")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					stack.Push(v1 / v2);
				}
				else if (token == "^")
				{
					var v2 = stack.Pop();
					var v1 = stack.Pop();
					stack.Push(Math.Pow(v1, v2));
				}
				else if (token == UnaryMinus)
				{
					stack.Push(-stack.Pop());
				}
			}

			// remaining item on stack contains result
			return stack.Count > 0 ? stack.Pop() : 0.0;
		}

		/// <summary>
		/// Returns a value that indicates the relative precedence of
		/// the specified operator
		/// </summary>
		/// <param name="s">Operator to be tested</param>
		/// <returns></returns>
		private static int GetPrecedence(string s)
		{
			switch (s)
			{
				case ":":
					return 1;

				case "+":
				case "-":
					return 2;

				case "*":
				case "/":
					return 3;

				case "^":
					return 4;

				case UnaryMinus:
					return 10;
			}

			return 0;
		}
	}
}
