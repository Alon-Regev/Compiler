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
		BOOLEAN,
		CHAR,
		STRING_LITERAL,

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
		BIT_NOT_OP,
		LEFT_SHIFT,
		RIGHT_SHIFT,
		// logical
		LOGIC_AND_OP,
		LOGIC_OR_OP,
		// relational
		LESS_OP,
		LESS_EQUAL_OP,
		GREATER_OP,
		GREATER_EQUAL_OP,
		EQUAL_OP,
		NOT_EQUAL_OP,
		// ternary
		QUESTION_MARK,
		COLON,
		// assignment
		ASSIGN_OP,
		// other
		EXCLAMATION_MARK,
		COMMA,
		DOT,
		TRIPLE_DOT,

		// --- Expressions
		LEFT_PARENTHESIS,
		RIGHT_PARENTHESIS,

		LEFT_SQUARE_BRACKET,
		RIGHT_SQUARE_BRACKET,

		// --- Statements
		SEMI_COLON,
		// variable declaration
		INT_KEYWORD,
		FLOAT_KEYWORD,
		BOOL_KEYWORD,
		CHAR_KEYWORD,
		// function declaration
		VOID,
		RETURN,
		// blocks
		OPEN_BRACE,
		CLOSE_BRACE,
		// if else
		IF,
		ELSE,
		// loops
		WHILE,
		DO,
		FOR,
		// switch case
		SWITCH,
		CASE,
		DEFAULT,
		// memory allocation
		NEW,
		DELETE,
		// other
		EXTERN,
		PRINT_KEYWORD,

		// --- Other
		IDENTIFIER,
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
			res += Helper.StringFormat(Code.ToString(), PRINT_WIDTH, PRINT_WIDTH - 1);
			res += Helper.StringFormat(Value.ToString(), PRINT_WIDTH, PRINT_WIDTH - 1);
			res += Helper.StringFormat(Line + ":" + Pos, PRINT_WIDTH, PRINT_WIDTH - 1);

			return res;
		}
	}
}
