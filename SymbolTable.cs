using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum SymbolType
	{

	}

	struct SymbolTableEntry
	{
		readonly SymbolType type;
		readonly int address;
	}

	class SymbolTable
	{
		private Dictionary<string, SymbolTableEntry> _table;

		// Constructor
		// input: none
		public SymbolTable()
		{
			_table = new Dictionary<string, SymbolTableEntry>();
		}

		// Method adds an entry to the symbol table
		// input: symbol name, data (entry)
		// return: none
		public void AddEntry(Token symbolToken, SymbolTableEntry entry)
		{
			// check if insertion is possible
			if (EntryExists(symbolToken.Value))
				throw new MultipleDefinedNamesError(symbolToken);
			// insert new entry
			_table.Add(symbolToken.Value, entry);
		}

		// Method return the entry of a specific symbol from the table
		// input: symbol name
		// return: symbol's entry
		public SymbolTableEntry GetEntry(Token symbolToken)
		{
			// check if entry exists
			if (!EntryExists(symbolToken.Value))
				throw new UnknownNameError(symbolToken);
			return _table[symbolToken.Value];
		}

		// Method checks if a symbol is already defined
		// input: symbol name to check
		// return: whether it's already in the table or not
		public bool EntryExists(string symbol)
		{
			return _table.ContainsKey(symbol);
		}
	}
}
