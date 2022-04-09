using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class SymbolTableEntry
	{
		public SymbolType Type { get; private set; }
		public int Address { get; set; }

		// Constructor
		public SymbolTableEntry(SymbolType type)
		{
			Type = type;
		}
	}
}
