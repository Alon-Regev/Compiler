using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class SymbolTableEntry
	{
		public SymbolType SymbolType { get; private set; }
		public ValueType ValueType { get; set; }
		public int Address { get; set; }
		public Statement Declaration { get; set; }

		// Constructor
		public SymbolTableEntry(SymbolType type, ValueType valueType, Statement declaration)
		{
			SymbolType = type;
			ValueType = valueType;
			Declaration = declaration;
		}
	}
}
