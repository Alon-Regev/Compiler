using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class VariableDeclaration : Statement
	{
		public TokenCode Type {private set; get;}
		public string Identifier { private set; get; }
		
		// Constructor
		// input: var type keyword token (int, float...), identifier token
		public VariableDeclaration(Token type, Token identifier) : base(type.Line)
		{
			Type = type.Code;
			Identifier = identifier.Value;
		}

		// ToString override specifies the variable declaration
		public override string ToString(int indent)
		{
			return "Variable Declaration (" + Type + " " + Identifier + ")" + base.ToString(indent);
		}
	}
}
