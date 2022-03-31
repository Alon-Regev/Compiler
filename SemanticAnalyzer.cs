using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum TypeCode
	{
		UNKNOWN,
		INT,
		FLOAT,
	}

	class SemanticAnalyzer
	{
		private AST_Node _tree;
		private static Dictionary<TokenCode, HashSet<TypeCode>> _binOpAllowedTypes = new Dictionary<TokenCode, HashSet<TypeCode>>
		{
			// --- Arithmetic
			{ TokenCode.ADD_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.SUB_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.MUL_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.DIV_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.MOD_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.POW_OP, new HashSet<TypeCode>{ TypeCode.FLOAT} },

			// --- Bitwise
			{ TokenCode.BIT_AND_OP, new HashSet<TypeCode>{ TypeCode.INT } },
			{ TokenCode.BIT_OR_OP, new HashSet<TypeCode>{ TypeCode.INT } },
			{ TokenCode.BIT_XOR_OP, new HashSet<TypeCode>{ TypeCode.INT } },
		};

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
				case IPrimitive p:
					AnalyzePrimitive(p);
					break;
				case Cast c:
					AnalyzeCast(c);
					break;
				default:
					break;
			}
		}

		// Method does a semantic analysis for a BinaryOperator subtree
		// input: binary operator to check
		//		  what type the result needs to be
		// return: none
		private void AnalyzeBinaryOperator(BinaryOperator op)
		{
			// analyze operands
			AnalyzeSubtree(op.Operand(0));
			AnalyzeSubtree(op.Operand(1));
			// check types
			if(op.Operand(0).Type != op.Operand(1).Type)
				throw new TypeError(op);
			// set type
			op.Type = op.Operand(0).Type;
			// check if operation is allowed
			if (!_binOpAllowedTypes[op.Operator].Contains(op.Type))
				throw new TypeError(op);
		}

		// Method does a semantic analysis for a primitive
		// input: primitive
		// return: none
		private void AnalyzePrimitive(IPrimitive primitive)
		{
			switch(primitive)
			{
				case Primitive<int> p:
					p.Type = TypeCode.INT;
					break;
				case Primitive<float> p:
					p.Type = TypeCode.FLOAT;
					break;
				default:
					break;
			}
		}

		// does an analysis on a cast
		private void AnalyzeCast(Cast cast)
		{
			AnalyzeSubtree(cast.GetChild(0));
			cast.FromType = cast.Child().Type;
		}
	}
}
