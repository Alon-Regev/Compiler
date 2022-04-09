using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class VariableDeclaration : Statement
	{
		public TokenCode Type { private set; get; }
		public string Identifier { private set; get; }
		
		// Constructor
		// input: var type keyword token (int, float...), identifier token
		public VariableDeclaration(Token type, Token identifier) : base(type.Line)
		{
			Type = type.Code;
			Identifier = identifier.Value;
		}

		// Method returns variable type code
		// input: none
		// return: type code of the declared variable
		public TypeCode GetTypeCode()
		{
			return Type switch 
			{ 
				TokenCode.INT_KEYWORD => TypeCode.INT,
				TokenCode.FLOAT_KEYWORD => TypeCode.FLOAT,
				TokenCode.BOOL_KEYWORD => TypeCode.BOOL,
			};
		}

		// ToString override specifies the variable declaration
		public override string ToString(int indent)
		{
			return "Variable Declaration (" + Type + " " + Identifier + ")" + base.ToString(indent);
		}
	}
}
