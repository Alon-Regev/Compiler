using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum SymbolType
	{
		LOCAL_VAR,
		FUNCTION,
		BUILTIN_FUNCTION,
		PARAMETER,
	}

	class SymbolTable
	{
		private static int GlobalTableNumber = 0;
		private int TableNumber;

		public SymbolTable ParentTable { get; set; }
		public SymbolTable OuterTable { get; set; }
		private Dictionary<string, SymbolTableEntry> _table;
		private int _addressCounter = 0;
		private int _paramAddressCounter = -4;

		// Constructor
		// input: none
		public SymbolTable()
		{
			_table = new Dictionary<string, SymbolTableEntry>();
			TableNumber = GlobalTableNumber++;
		}

		// Method rounds type sizes to multiples of four to put in the stack.
		// input: original size
		// return: size on the stack
		private int RoundByFours(int original)
		{
			return original + Mod(4 - original, 4);
		}
		// mov implementation which works with negative numbers
		// input: a, b
		// return: a mod b
		private int Mod(int a, int b)
		{
			return a - b * (int)Math.Floor((double)a / b);
		}

		// Method adds an entry to the symbol table
		// input: declaration object, data (entry), address and size
		// return: none
		public void AddEntry(string identifier, int line, SymbolTableEntry entry, int address = 0, int size = 4)
		{
			// check if insertion is possible
			if (EntryExists(identifier))
				throw new MultipleDefinedNamesError(identifier, line);
			if (entry.SymbolType == SymbolType.LOCAL_VAR)
			{
				_addressCounter += RoundByFours(size);
				entry.Address = _addressCounter;
			}
			else if(entry.SymbolType == SymbolType.PARAMETER)
			{
				_paramAddressCounter -= RoundByFours(size);
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
			{
				SymbolTableEntry entry = _table[identifier].Copy();
				if(entry.SymbolType == SymbolType.LOCAL_VAR)
					entry.Address += GetOffset();
				return _table[identifier];
			}
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
			return GetEntry(variable.Identifier, variable.Line);
		}

		// Method checks if a symbol is already defined
		// input: symbol name to check
		// return: whether it's already in the table or not
		public bool EntryExists(string symbol)
		{
			return _table.ContainsKey(symbol);
		}
		public bool EntryExistsRecursive(string symbol)
		{
			return EntryExists(symbol) ||
				ParentTable != null && ParentTable.EntryExists(symbol);
		}

		// Method returns the amount of bytes needed for the local variables of this block
		// input: none
		// return: number of bytes needed to be allocated on the stack
		public int VariableBytes()
		{
			return _addressCounter;
		}

		// Method returns offset in stack frame.
		// input: none
		// return: offset of variables in stack frame.
		public int GetOffset()
		{
			if (ParentTable is null)
				return _addressCounter;
			else
				return _addressCounter + ParentTable.GetOffset();
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

		// Method finds closest outer table
		// input: none
		// return: outer table
		public SymbolTable GetOuterTable()
		{
			if (OuterTable != null)
				return OuterTable;
			else
				return ParentTable?.GetOuterTable();
		}

		// Method finds entry in this or in outer tables
		// input: variable to find
		// return: tuple. Entry and Symbol Table it was found in (or null)
		public Tuple<SymbolTableEntry, SymbolTable> GetOuterEntry(Variable var)
		{
			if (EntryExistsRecursive(var.Identifier))
				return Tuple.Create(GetEntry(var), this);
			else
			{
				SymbolTable outer = GetOuterTable();
				if (outer != null)
					return outer.GetOuterEntry(var);
				else
					return Tuple.Create(GetEntry(var), (SymbolTable)null);
			}
		}

		// To String overload to print content of symbol table
		public string ToString(int indent)
		{
			string indentStr = "";
			for (; indent > 0; indent--)
				indentStr += '\t';

			string result = indentStr + "___SymbolTable" + TableNumber + "___\n" +
				indentStr + Helper.StringFormat("Name", 20) + " | " + Helper.StringFormat("Type", 15) + " | " + Helper.StringFormat(
				"Value", 12) + " | " + Helper.StringFormat("Address", 12) + "\n";
			
			foreach((string name, SymbolTableEntry entry) in _table)
			{
				result += indentStr + Helper.StringFormat(name, 20) + " | " + Helper.StringFormat(entry.SymbolType.ToString(), 15) + " | " +
					Helper.StringFormat(entry.ValueType.ToString(), 12) + " | " + Helper.StringFormat(entry.Address.ToString(), 12) + "\n";
			}

			// add relations
			result += indentStr + "Parent: " + ParentTable?.TableNumber + ", Outer: " + OuterTable?.TableNumber + "\n";

			return result;
		}
	}
}
