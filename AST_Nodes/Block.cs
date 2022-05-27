using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Block : Statement
	{
		public SymbolTable SymbolTable { get; private set; }
		public SymbolTable OuterTable { get; set; }

		public Block(int line) : base(line)
		{
			SymbolTable = new SymbolTable();
			OuterTable = null;
		}

		// Method adds a statement to the block
		// input: statement to add
		// return: none
		public void AddStatement(Statement statement)
		{
			AddChild(statement);
		}

		// Method gets a statement from the block based on the index
		// input: index
		// return: statement
		public Statement GetStatement(int i)
		{
			return (Statement)GetChild(i);
		}

		// Method offsets addresses of variables in this block and in nested blocks
		// input: number of bytes to offset
		// return: none
		public void OffsetAddresses(int offset)
		{
			SymbolTable.OffsetAddresses(offset);
			foreach (AST_Node stmt in Children)
				if (stmt is Block)
					(stmt as Block).OffsetAddresses(offset);
		}

		// ToString override shows block and it's content
		public override string ToString(int indent)
		{
			return "Block" + base.ToString(indent);
		}
	}
}
