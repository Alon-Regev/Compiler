using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ForLoop : Block
	{
		public static readonly int INIT_INDEX = 0;
		public static readonly int CONDITION_INDEX = 1;
		public static readonly int ACTION_INDEX = 2;
		public static readonly int BODY_INDEX = 3;

		public ForLoop(VariableDeclaration initialization, Expression condition, Expression action, Statement body) : base(condition.Line)
		{
			AddChild(initialization);
			AddChild(condition);
			AddChild(action);
			AddChild(body);
			// add initialization to symbol table
			foreach (string identifier in initialization.Identifiers)
			{
				SymbolTable.AddEntry(
					identifier,
					initialization.Line,
					new SymbolTableEntry(SymbolType.LOCAL_VAR, initialization.Type, initialization)
				);
			}
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "For Loop" + base.ToString(indent);
		}
	}
}
