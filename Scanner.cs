using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler
{
	class Scanner
	{
		// defines regexes for different tokens
		static private readonly KeyValuePair<TokenCode, string>[] TokenRegexExpressions =
		{
			// operators
			new KeyValuePair<TokenCode, string>(TokenCode.ADD_OP, @"\+" ),
			new KeyValuePair<TokenCode, string>(TokenCode.SUB_OP, "-" ),
			new KeyValuePair<TokenCode, string>(TokenCode.MUL_OP, @"\*" ),
			new KeyValuePair<TokenCode, string>(TokenCode.DIV_OP, "/" ),

			// expression symbols
			new KeyValuePair<TokenCode, string>(TokenCode.LEFT_PARENTHESIS, @"\(" ),
			new KeyValuePair<TokenCode, string>(TokenCode.RIGHT_PARENTHESIS, @"\)" ),

			// values
			new KeyValuePair<TokenCode, string>(TokenCode.NUMBER, @"\d+" ),

			// default unknown and EOF
			new KeyValuePair<TokenCode, string>(TokenCode.UNKNOWN, @".+" ),
			new KeyValuePair<TokenCode, string>(TokenCode.EOF, @""),
		};

		// fields to check where we are in the program
		private string _programLeft;
		private int _line = 1, _pos = 0;
		//private int increment = 0;

		// Constructor
		// program: program to tokenize
		public Scanner(string program)
		{
			_programLeft = program;
		}

		// Method converts a program string to a list of tokens
		// input: none
		// return: array of Token instances
		public Token[] Tokenize()
		{
			List<Token> tokens = new List<Token>();

			// go over program
			while(_programLeft.Length != 0)
			{
				// add token
				Token next = Next();
				tokens.Add(next);
			}

			return tokens.ToArray();
		}

		// Method returns the next token in the program and moves to next.
		// input: none
		// return: the next token
		public Token Next()
		{
			Token token = Peek();
			// move to next
			int increment = token.Value.Length;
			_programLeft = _programLeft.Substring(increment);
			_pos += increment;

			return token;
		}

		// Method returns the next token in the program without moving to next token. only skips whitespace
		// input: none
		// return: the next Token
		public Token Peek()
		{
			SkipWhitespace();

			Token result = null;
			// try each token to check if it fits
			foreach (KeyValuePair<TokenCode, string> tokenRegex in TokenRegexExpressions)
			{
				// match regex to program
				Match match = Regex.Match(_programLeft, tokenRegex.Value);
				// check if first match is at the beginning
				if (match.Index != 0 || !match.Success)
					continue;
				// if index is at start, found token
				result = new Token(tokenRegex.Key, _line, _pos, match.Value);
				break;
			}
			// check if valid and return
			if (result.Code == TokenCode.UNKNOWN)
				throw new UnknownToken(result);

			return result;
		}

		// Method skips whitespace in the program.
		// input: none
		// return: none
		private void SkipWhitespace()
		{
			// break on non-whitespace
			while (_programLeft.Length > 0)
			{
				if (_programLeft[0] == ' ' || _programLeft[0] == '\t' || _programLeft[0] == '\r')
				{
					_programLeft = _programLeft.Substring(1);
					_pos += 1;
				}
				else if (_programLeft[0] == '\n')
				{
					_programLeft = _programLeft.Substring(1);
					_line++;
					_pos = 0;
				}
				else
					break;
			}
		}

		// Method prints a token array
		// tokens: token array to print
		// return: none
		static public void PrintTokens(Token[] tokens)
		{
			// print table headers
			Console.WriteLine(
				"Type".PadRight(Token.PRINT_WIDTH) +
				"Value".PadRight(Token.PRINT_WIDTH) +
				"Position".PadRight(Token.PRINT_WIDTH)
			);

			// print tokens _line by _line
			foreach(Token token in tokens)
			{
				Console.WriteLine(token);
			}
		}
	}
}
