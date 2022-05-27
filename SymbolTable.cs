using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum SymbolType
	{
		LOCAL_VAR,
		FUNCTION,
		PARAMETER,
		OUTER_VAR,
	}

	class SymbolTable
	{
		public SymbolTable ParentTable { get; set; }
		private Dictionary<string, SymbolTableEntry> _table;
		private int _addressCounter = 0;
		private int _paramAddressCounter = -4;

		// Constructor
		// input: none
		public SymbolTable()
		{
			_table = new Dictionary<string, SymbolTableEntry>();
		}

		// Method adds an entry to the symbol table
		// input: declaration object, data (entry)
		// return: none
		public void AddEntry(string identifier, int line, SymbolTableEntry entry, int address = 0)
		{
			// check if insertion is possible
			if (EntryExists(identifier))
				throw new MultipleDefinedNamesError(identifier, line);
			if (entry.SymbolType == SymbolType.LOCAL_VAR)
			{
				_addressCounter += 4;
				entry.Address = _addressCounter;
			}
			else if(entry.SymbolType == SymbolType.PARAMETER)
			{
				_paramAddressCounter -= 4;
				entry.Address = _paramAddressCounter;
			}	
			else
				entry.Address = address;
			// set address
			// insert new entry
			_table.Add(identifier, entry);
		}

		// Method return the entry of a specific symbol from the table
		// input: declaration node
		// return: symbol's entry
		public SymbolTableEntry GetEntry(string identifier, int line)
		{
			// check if entry exists
			if (EntryExists(identifier))
				return _table[identifier];
			// try to find entry in parent
			if(ParentTable == null)
				throw new UnknownNameError(identifier, line);

			return ParentTable.GetEntry(identifier, line);
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
