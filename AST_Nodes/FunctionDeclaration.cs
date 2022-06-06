using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class FunctionDeclaration : Statement
	{
		public ValueType ReturnType { private set; get; }
		public string Identifier { private set; get; }
		public List<KeyValuePair<string, ValueType>> Parameters { private set; get; }
		
		// Constructor
		// input: var type keyword token (int, float...), identifier token
		public FunctionDeclaration(ValueType retType, string identifier, Block block, List<KeyValuePair<string, ValueType>> parameters, bool builtin = false) : base(retType.Line)
		{
			// add parameters to symbol table
			foreach (KeyValuePair<string, ValueType> param in parameters)
			{
				block?.SymbolTable?.AddEntry(param.Key, Line,
					new SymbolTableEntry(SymbolType.PARAMETER, param.Value, null)
				);
			}
			// add pebp entry
			block?.SymbolTable?.AddEntry("pebp", -1,
					new SymbolTableEntry(SymbolType.PARAMETER, ValueType.Unknown(-1), null)
			);

			ReturnType = retType;
			Identifier = identifier;
			AddChild(block);
			Parameters = parameters;
		}

		// Method returns function's implementation block
		public Block GetBlock()
		{
			return GetChild(0) as Block;
		}

		// Method returns variable type code
		// input: none
		// return: type code of the declared variable
		public TypeCode GetTypeCode()
		{
			return SemanticAnalyzer.ToTypeCode(ReturnType, Line);
		}

		// ToString override specifies the variable declaration
		public override string ToString(int indent)
		{
			string parameterTypes = Parameters.Count == 0 ? "Void" : "(";
			foreach(KeyValuePair<string, TypeCode> param in Parameters)
			{
				parameterTypes += param.Value + ", ";
			}
			if(Parameters.Count != 0)
			{
				parameterTypes = parameterTypes.Substring(0, parameterTypes.Length - 2) + ")";
			}
			return "Function Declaration (" + Identifier + " " + parameterTypes + " -> " + GetTypeCode() + ")" + base.ToString(indent);
		}
	}
}
