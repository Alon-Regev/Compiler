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

			// values
			new KeyValuePair<TokenCode, string>(TokenCode.NUMBER, @"\d+" ),

			// default unknown
			new KeyValuePair<TokenCode, string>(TokenCode.UNKNOWN, @".+" ),
		};

		// Method converts a program string to a list of tokens
		// program: program string to convert
		// return: array of Token instances
		static public Token[] Tokenize(string program)
		{
			List<Token> tokens = new List<Token>();

			// variables to check where we are in the program
			string programLeft = program;
			int line = 1, pos = 0;
			int increment = 0;

			// go over program
			while(programLeft.Length != 0)
			{
				// check whitespace to skip
				if (programLeft[0] == ' ' || programLeft[0] == '\t' || programLeft[0] == '\r')
				{
					programLeft = programLeft.Substring(1);
					pos += 1;
					continue;
				}
				else if(programLeft[0] == '\n')
				{
					programLeft = programLeft.Substring(1);
					line++;
					pos = 0;
					continue;
				}

				increment = 0;
				// try each token to check if it's fitting
				foreach(KeyValuePair<TokenCode, string> tokenRegex in TokenRegexExpressions)
				{
					// match regex to program
					Match match = Regex.Match(programLeft, tokenRegex.Value);
					// check if first match is at the beginning
					if (match.Index != 0 || !match.Success)
						continue;
					// if index is at start, found token
					tokens.Add(new Token(tokenRegex.Key, line, pos, match.Value));
					increment = match.Value.Length;
					break;
				}

				// increment position
				programLeft = programLeft.Substring(increment);
				pos += increment;
			}

			return tokens.ToArray();
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

			// print tokens line by line
			foreach(Token token in tokens)
			{
				Console.WriteLine(token);
			}
		}
	}
}
