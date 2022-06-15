using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler
{
	class Scanner
	{
		static private readonly string KEYWORD_REGEX = "(?=[^_a-zA-Z0-9])";

		// defines regexes for different tokens
		static private readonly KeyValuePair<TokenCode, string>[] TokenRegexExpressions =
		{
			// --- Operators
			// arithmetic
			new KeyValuePair<TokenCode, string>(TokenCode.ADD_OP, @"\+" ),
			new KeyValuePair<TokenCode, string>(TokenCode.SUB_OP, "-" ),
			new KeyValuePair<TokenCode, string>(TokenCode.POW_OP, @"\*\*" ),
			new KeyValuePair<TokenCode, string>(TokenCode.MUL_OP, @"\*" ),
			new KeyValuePair<TokenCode, string>(TokenCode.DIV_OP, "/" ),
			new KeyValuePair<TokenCode, string>(TokenCode.MOD_OP, "%" ),
			// logical
			new KeyValuePair<TokenCode, string>(TokenCode.LOGIC_AND_OP, "&&" ),
			new KeyValuePair<TokenCode, string>(TokenCode.LOGIC_OR_OP, @"\|\|" ),
			// bitwise
			new KeyValuePair<TokenCode, string>(TokenCode.BIT_AND_OP, "&" ),
			new KeyValuePair<TokenCode, string>(TokenCode.BIT_OR_OP, @"\|" ),
			new KeyValuePair<TokenCode, string>(TokenCode.BIT_XOR_OP, @"\^" ),
			new KeyValuePair<TokenCode, string>(TokenCode.BIT_NOT_OP, @"~" ),
			new KeyValuePair<TokenCode, string>(TokenCode.LEFT_SHIFT, "<<" ),
			new KeyValuePair<TokenCode, string>(TokenCode.RIGHT_SHIFT, ">>" ),
			// relational
			new KeyValuePair<TokenCode, string>(TokenCode.LESS_EQUAL_OP, "<=" ),
			new KeyValuePair<TokenCode, string>(TokenCode.GREATER_EQUAL_OP, ">=" ),
			new KeyValuePair<TokenCode, string>(TokenCode.LESS_OP, "<" ),
			new KeyValuePair<TokenCode, string>(TokenCode.GREATER_OP, ">" ),
			new KeyValuePair<TokenCode, string>(TokenCode.EQUAL_OP, "==" ),
			new KeyValuePair<TokenCode, string>(TokenCode.NOT_EQUAL_OP, "!=" ),
			// ternary
			new KeyValuePair<TokenCode, string>(TokenCode.QUESTION_MARK, @"\?" ),
			new KeyValuePair<TokenCode, string>(TokenCode.COLON, ":" ),
			// assignment
			new KeyValuePair<TokenCode, string>(TokenCode.ASSIGN_OP, "=" ),
			// other
			new KeyValuePair<TokenCode, string>(TokenCode.EXCLAMATION_MARK, "!" ),
			new KeyValuePair<TokenCode, string>(TokenCode.COMMA, "," ),
			new KeyValuePair<TokenCode, string>(TokenCode.TRIPLE_DOT, @"\.{3}" ),
			new KeyValuePair<TokenCode, string>(TokenCode.DOT, @"\." ),

			// --- Expression symbols
			new KeyValuePair<TokenCode, string>(TokenCode.LEFT_PARENTHESIS, @"\(" ),
			new KeyValuePair<TokenCode, string>(TokenCode.RIGHT_PARENTHESIS, @"\)" ),

			new KeyValuePair<TokenCode, string>(TokenCode.LEFT_SQUARE_BRACKET, @"\[" ),
			new KeyValuePair<TokenCode, string>(TokenCode.RIGHT_SQUARE_BRACKET, @"\]" ),

			// --- Values
			new KeyValuePair<TokenCode, string>(TokenCode.DECIMAL, @"\d+\.\d+" ),
			new KeyValuePair<TokenCode, string>(TokenCode.INTEGER, @"\d+" ),
			new KeyValuePair<TokenCode, string>(TokenCode.BOOLEAN, @"(true|false)" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.CHAR, @"'(.|\\.)'" ),
			new KeyValuePair<TokenCode, string>(TokenCode.STRING_LITERAL, "\".*?[^\\\\]\"" ),

			// --- Statements
			new KeyValuePair<TokenCode, string>(TokenCode.SEMI_COLON, ";" ),
			// variable declaration
			new KeyValuePair<TokenCode, string>(TokenCode.INT_KEYWORD, "int" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.FLOAT_KEYWORD, "float" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.BOOL_KEYWORD, "bool" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.CHAR_KEYWORD, "char" + KEYWORD_REGEX ),
			// function declaration
			new KeyValuePair<TokenCode, string>(TokenCode.VOID, "void" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.RETURN, "return" + KEYWORD_REGEX ),
			// blocks
			new KeyValuePair<TokenCode, string>(TokenCode.OPEN_BRACE, "{" ),
			new KeyValuePair<TokenCode, string>(TokenCode.CLOSE_BRACE, "}" ),
			// if else
			new KeyValuePair<TokenCode, string>(TokenCode.IF, "if" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.ELSE, "else" + KEYWORD_REGEX ),
			// loops
			new KeyValuePair<TokenCode, string>(TokenCode.WHILE, "while" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.DO, "do" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.FOR, "for" + KEYWORD_REGEX ),
			// switch case
			new KeyValuePair<TokenCode, string>(TokenCode.SWITCH, "switch" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.CASE, "case" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.DEFAULT, "default" + KEYWORD_REGEX ),
			// memory allocation
			new KeyValuePair<TokenCode, string>(TokenCode.NEW, "new" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.DELETE, "delete" + KEYWORD_REGEX ),
			// other
			new KeyValuePair<TokenCode, string>(TokenCode.PRINT_KEYWORD, "print" + KEYWORD_REGEX ),
			new KeyValuePair<TokenCode, string>(TokenCode.EXTERN, "extern" + KEYWORD_REGEX ),

			// --- Other
			new KeyValuePair<TokenCode, string>(TokenCode.IDENTIFIER, @"[_a-zA-Z][_a-zA-Z0-9]*" ),
			new KeyValuePair<TokenCode, string>(TokenCode.UNKNOWN, @".+?" ),
			new KeyValuePair<TokenCode, string>(TokenCode.EOF, @""),
		};

		// fields to check where we are in the program
		private string _programLeft;
		private int _line = 1, _pos = 0;
		
		public Token Last { private set; get; }

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

			Last = token;

			return token;
		}

		// Method moves to the next token if the current token is a certain value
		// input: required current token to move
		// return: true if moved to next
		public bool NextIf(TokenCode required)
		{
			if (Peek().Code == required)
			{
				Next();
				return true;
			}
			return false;
		}

		// Method returns the next token in the program without moving to next token. only skips whitespace
		// input: none
		// return: the next Token
		public Token Peek()
		{
			SkipWhitespace();
			SkipComments();

			Token result = null;
			// try each token to check if it fits
			foreach (KeyValuePair<TokenCode, string> tokenRegex in TokenRegexExpressions)
			{
				// match regex to program
				Match match = Regex.Match(_programLeft, tokenRegex.Value, RegexOptions.Singleline);
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

		// Method skips over required token. Throws exception if the required token is missing.
		// input: requiredToken: token required in the syntax (example: keywords)
		// return: token
		public Token Require(TokenCode requiredToken)
		{
			Token next = Next();
			if (next.Code != requiredToken)
				throw new UnexpectedToken(requiredToken.ToString(), next);
			return next;
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

		// Method skips comments in the program.
		// input: none
		// return: none
		private void SkipComments()
		{
			while (_programLeft.StartsWith("//") || _programLeft.StartsWith("/*"))
			{
				if (_programLeft.StartsWith("//"))
				{
					// skip until newline
					do
					{
						_programLeft = _programLeft.Substring(1);
					} while (_programLeft[0] != '\n');
					_programLeft = _programLeft.Substring(1);
					SkipWhitespace();
					_line++;
				}
				else if (_programLeft.StartsWith("/*"))
				{
					// skip until newline
					do
					{
						if (_programLeft[0] == '\n')
							_line++;
						_programLeft = _programLeft.Substring(1);
					} while (_programLeft.Substring(0, 2) != "*/");
					_programLeft = _programLeft.Substring(2);
					SkipWhitespace();
				}
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
