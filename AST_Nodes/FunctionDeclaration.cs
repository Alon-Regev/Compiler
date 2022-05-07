using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class FunctionDeclaration : Statement
	{
		public TokenCode ReturnType { private set; get; }
		public string Identifier { private set; get; }
		
		// Constructor
		// input: var type keyword token (int, float...), identifier token
		public FunctionDeclaration(Token retType, string identifier, Block block) : base(retType.Line)
		{
			ReturnType = retType.Code;
			Identifier = identifier;
			AddChild(block);
		}

		// Method returns variable type code
		// input: none
		// return: type code of the declared variable
		public TypeCode GetTypeCode()
		{
			return ReturnType switch 
			{ 
				TokenCode.INT_KEYWORD => TypeCode.INT,
				TokenCode.FLOAT_KEYWORD => TypeCode.FLOAT,
				TokenCode.BOOL_KEYWORD => TypeCode.BOOL,
				TokenCode.VOID => TypeCode.VOID,
			};
		}

		// ToString override specifies the variable declaration
		public override string ToString(int indent)
		{
			return "Function Declaration (" + Identifier + " VOID -> " + GetTypeCode() + ")" + base.ToString(indent);
		}
	}
}
