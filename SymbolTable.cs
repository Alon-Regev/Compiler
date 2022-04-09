using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum SymbolType
	{
		LOCAL_VAR
	}

	class SymbolTable
	{
		public SymbolTable ParentTable { get; set; }
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
		// input: declaration node
		// return: symbol's entry
		public SymbolTableEntry GetEntry(VariableDeclaration decl)
		{
			// check if entry exists
			if (EntryExists(decl.Identifier))
				return _table[decl.Identifier];
			// try to find entry in parent
			if(ParentTable == null)
				throw new UnknownNameError(decl);

			return ParentTable.GetEntry(decl);
		}

		// Method return the entry of a specific symbol from the table
		// input: variable reference node
		// return: symbol's entry
		public SymbolTableEntry GetEntry(Variable variable)
		{
			// check if entry exists
			if (EntryExists(variable.Identifier))
				return _table[variable.Identifier];
			// try to find entry in parent
			if (ParentTable == null)
				throw new UnknownNameError(variable);

			return ParentTable.GetEntry(variable);
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

		// Method offsets addresses of all local variables
		// input: amount of bytes to offset by
		// return: none
		public void OffsetAddresses(int offset)
		{
			foreach(string key in _table.Keys)
			{
				// if local var
				if (_table[key].SymbolType == SymbolType.LOCAL_VAR)
				{
					// offset address
					_table[key].Address += offset;
				}
			}
		}
	}
}
