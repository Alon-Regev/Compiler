using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class AST_Node
	{
		public List<AST_Node> Children { get; private set; }
		public int Line { get; private set; }

		// constructor
		// line: Line number of the node
		protected AST_Node(int line)
		{
			Children = new List<AST_Node>();
			Line = line;
		}

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

		// Method sets the ith child of this node.
		// i: child's index
		// node: node to replace child with
		// return: none
		public void SetChild(int i, AST_Node node)
		{
			// check range
			if (i < Children.Count)
				Children[i] = node;
		}

		// base node's TpString prints children
		public override string ToString()
		{
			return ToString(1);
		}

		virtual public string ToString(int indent = 0)
		{
			string result = "\n";
			string indentStr = "";
			for (int i = 0; i < indent; i++) indentStr += '\t';
			// add children
			foreach(AST_Node child in Children)
			{
				result += indentStr + child.ToString(indent + 1);
			}
			// return all children
			return result;
		}
	}
}
