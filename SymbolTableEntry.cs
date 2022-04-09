using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class SymbolTableEntry
	{
		public SymbolType SymbolType { get; private set; }
		public TypeCode ValueType { get; set; }
		public int Address { get; set; }

		// Constructor
		public SymbolTableEntry(SymbolType type, TypeCode valueType)
		{
			SymbolType = type;
			ValueType = valueType;
		}
	}
}
