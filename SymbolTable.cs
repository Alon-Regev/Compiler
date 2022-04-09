using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum SymbolType
	{
		LOCAL_VAR
	}

	struct SymbolTableEntry
	{
		public SymbolType Type;
		public int Address;
	}

	class SymbolTable
	{
		private Dictionary<string, SymbolTableEntry> _table;
		private int _addressCounter = 0;

		// Constructor
		// input: none
		public SymbolTable()
		{
			_table = new Dictionary<string, SymbolTableEntry>();
		}

		// Method adds an entry to the symbol table
		// input: declaration object, data (entry)
		// return: none
		public void AddEntry(VariableDeclaration decl, SymbolTableEntry entry)
		{
			// check if insertion is possible
			if (EntryExists(decl.Identifier))
				throw new MultipleDefinedNamesError(decl);
			_addressCounter += 4;
			entry.Address = _addressCounter;
			// set address
			// insert new entry
			_table.Add(decl.Identifier, entry);
		}

		// Method return the entry of a specific symbol from the table
		// input: symbol name
		// return: symbol's entry
		public SymbolTableEntry GetEntry(VariableDeclaration decl)
		{
			// check if entry exists
			if (!EntryExists(decl.Identifier))
				throw new UnknownNameError(decl);
			return _table[decl.Identifier];
		}

		// Method checks if a symbol is already defined
		// input: symbol name to check
		// return: whether it's already in the table or not
		public bool EntryExists(string symbol)
		{
			return _table.ContainsKey(symbol);
		}

		// Method returns the amount of bytes needed for the local variables of this block
		// input: none
		// return: number of bytes needed to be allocated on the stack
		public int VariableBytes()
		{
			return _addressCounter;
		}
	}
}
