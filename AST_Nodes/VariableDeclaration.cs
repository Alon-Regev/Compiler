using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class VariableDeclaration : Statement
	{
		public ValueType Type { private set; get; }
		public List<string> Identifiers { private set; get; }
		
		// Constructor
		// input: var type keyword token (int, float...), identifier token
		public VariableDeclaration(ValueType type) : base(type.Line)
		{
			Type = type;
			Identifiers = new List<string>();
		}

		// Method returns variable type code
		// input: none
		// return: type code of the declared variable
		public TypeCode GetTypeCode()
		{
			return Type.TypeCode;
		}

		// Method returns list of identifiers as string
		// input: none
		// return: identifiers as string
		public string IdentifiersString()
		{
			return String.Join(", ", Identifiers);
		}

		// ToString override specifies the variable declaration
		public override string ToString(int indent)
		{
			return "Variable Declaration (" + Type + " " + IdentifiersString() + ")" + base.ToString(indent);
		}
	}
}
