using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum TypeCode
	{
		UNKNOWN,
		INT
	}

	class SemanticAnalyzer
	{
		private AST_Node _tree;

		// Constructor
		// tree: AST to check
		public SemanticAnalyzer(AST_Node tree)
		{
			_tree = tree;
		}

		// Method does a semantic analysis on the AST and adds necessary operations
		// input: none
		// return: none
		public void Analyze()
		{
			AnalyzeSubtree(_tree);
		}

		// Method does a semantic analysis on a part of the AST
		// input: subtree to check
		// return: none
		private void AnalyzeSubtree(AST_Node tree)
		{
			switch(tree)
			{
				case BinaryOperator op:
					AnalyzeBinaryOperator(op);
					break;
				default:
					break;
			}
		}

		// Method does a semantic analysis for a BinaryOperator subtree
		// input: binary operator to check
		//		  what type the result needs to be
		// return: none
		private void AnalyzeBinaryOperator(BinaryOperator op, TypeCode neededType = TypeCode.UNKNOWN)
		{
			// analyze operands
			AnalyzeSubtree(op.GetChild(0));
			AnalyzeSubtree(op.GetChild(1));
			// final type
			TypeCode type = neededType;
			if (type == TypeCode.UNKNOWN)
			{
				// default to type of first
				type = op.Operand(0).Type;
			}
			// add type to bin op
			op.Type = type;

			// add casting as needed
			if(op.Operand(0).Type != type)
			{
				Cast cast = new Cast(op.Operand(0), type);
				op.SetChild(0, cast);
			}
			if(op.Operand(1).Type != type)
			{
				Cast cast = new Cast(op.Operand(1), type);
				op.SetChild(1, cast);
			}
		}
	}
}
