using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	// token code specifies the type of the token
	enum TokenCode
	{
		// --- Values
		INTEGER,
		DECIMAL,

		// --- Castings
		INT_CAST,
		FLOAT_CAST,
		
		// --- Operators
		// arithmetic
		ADD_OP,
		SUB_OP,
		MUL_OP,
		DIV_OP,
		MOD_OP,
		POW_OP,
		// bitwise
		BIT_AND_OP,
		BIT_OR_OP,
		BIT_XOR_OP,
		LEFT_SHIFT,
		RIGHT_SHIFT,

		// --- Expressions
		LEFT_PARENTHESIS,
		RIGHT_PARENTHESIS,

		// --- Other
		UNKNOWN,
		EOF,
	}

	class Token
	{
		static public readonly int PRINT_WIDTH = 16;

		public TokenCode Code { get; private set; }
		public string Value { get; private set; }
		public int Line { get; private set; }
		public int Pos { get; private set; }

		// constructor
		// code: token code from enum
		// line: line of code the token appeared at
		// pos: index in line the token appeared at
		// value: the string of the token as it was found in the program
		public Token(TokenCode code, int line, int pos, string value)
		{
			Code = code;
			Line = line;
			Pos = pos;
			Value = value;
		}

		// ToString override, returns token's information
		public override string ToString()
		{
			string res = "";

			// add fields
			res += StringFormat(Code.ToString(), PRINT_WIDTH - 1, PRINT_WIDTH);
			res += StringFormat(Value.ToString(), PRINT_WIDTH - 1, PRINT_WIDTH);
			res += StringFormat(Line + ":" + Pos, PRINT_WIDTH - 1, PRINT_WIDTH);

			return res;
		}

		// Method changes a string to a constant width.
		// baseString: string to change
		// charLimit: max amount of characters to include
		// width: constant number of chars in the result. Padding with spaces.
		// return: formatted string
		private static string StringFormat(string baseString, int charLimit, int width)
		{
			string result = baseString;
			// limit chars
			if (baseString.Length > charLimit - 3)
				result = baseString.Substring(0, charLimit - 3) + "...";

			// return padded string
			return result.PadRight(width);
		}
	}
}
