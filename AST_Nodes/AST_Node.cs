using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class AST_Node
	{
		private List<AST_Node> Children = new List<AST_Node>();

		// Method adds a child to this node
		// child: child node to add
		// return: none
		public void AddChild(AST_Node child)
		{
			Children.Add(child);
		}

		// Method returns the ith child of this node.
		// i: child's index
		// return: child's AST_Node
		public AST_Node GetChild(int i)
		{
			// check range
			if (i < Children.Count)
				return Children[i];
			else
				return null;
		}
	}
}
