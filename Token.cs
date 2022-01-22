using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	// token code specifies the type of the token
	enum TokenCode
	{
		// Values
		NUMBER,

		// Operators
		ADD_OP,
		SUB_OP,
		MUL_OP,
		DIV_OP,

		// other
		UNKNOWN,
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
			res += Code.ToString().PadRight(PRINT_WIDTH);
			res += Value.ToString().PadRight(PRINT_WIDTH);
			res += (Line + ":" + Pos).PadRight(PRINT_WIDTH);

			return res;
		}
	}
}
