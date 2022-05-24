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
		BOOL,
		VOID,
	}

	class SemanticAnalyzer
	{
		private AST_Node _tree;
		private Block _currentBlock;
		private FunctionDeclaration _currentFunction = null;

		private HashSet<string> _declaredSymbols = new HashSet<string>();

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
			{ TokenCode.LEFT_SHIFT, new HashSet<TypeCode>{ TypeCode.INT } },
			{ TokenCode.RIGHT_SHIFT, new HashSet<TypeCode>{ TypeCode.INT } },

			// --- Logical
			{ TokenCode.LOGIC_AND_OP, new HashSet<TypeCode>{ TypeCode.BOOL } },
			{ TokenCode.LOGIC_OR_OP, new HashSet<TypeCode>{ TypeCode.BOOL } },

			// --- Relational
			{ TokenCode.LESS_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.LESS_EQUAL_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.GREATER_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.GREATER_EQUAL_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.EQUAL_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },
			{ TokenCode.NOT_EQUAL_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },

			// --- Assignment
			{ TokenCode.ASSIGN_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT, TypeCode.BOOL } },
		};

		private static Dictionary<TokenCode, HashSet<TypeCode>> _unaryPrefixOpAllowedTypes = new Dictionary<TokenCode, HashSet<TypeCode>>
		{
			// --- Arithmetic
			{ TokenCode.BIT_NOT_OP, new HashSet<TypeCode>{ TypeCode.INT } },
			{ TokenCode.SUB_OP, new HashSet<TypeCode>{ TypeCode.INT, TypeCode.FLOAT } },	// negation
			{ TokenCode.EXCLAMATION_MARK, new HashSet<TypeCode>{ TypeCode.BOOL } },			// logic not
		};

		private static Dictionary<TokenCode, HashSet<TypeCode>> _unaryPostfixOpAllowedTypes = new Dictionary<TokenCode, HashSet<TypeCode>>
		{
			// --- Arithmetic
			{ TokenCode.EXCLAMATION_MARK, new HashSet<TypeCode>{ TypeCode.INT } },	// factorial
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
				// --- Expressions
				case TernaryOperator op:
					AnalyzeTernaryOperator(op);
					break;
				case BinaryOperator op:
					AnalyzeBinaryOperator(op);
					break;
				case UnaryOperator op:
					AnalyzeUnaryOperator(op);
					break;
				case IPrimitive p:
					AnalyzePrimitive(p);
					break;
				case Cast c:
					AnalyzeCast(c);
					break;
				case Variable v:
					AnalyzeVariable(v);
					break;
				case FunctionCall call:
					AnalyzeFunctionCall(call);
					break;
				// --- Statements
				case ForLoop stmt:
					AnalyzeForLoop(stmt);
					break;
				case Block block:
					AnalyzeBlock(block);
					break;
				case ExpressionStatement stmt:
					AnalyzeSubtree(stmt.GetExpression());
					break;
				case PrintStatement stmt:
					AnalyzeSubtree(stmt.GetExpression());
					break;
				case ReturnStatement stmt:
					AnalyzeReturn(stmt);
					break;
				case VariableDeclaration decl:
					AnalyzeVariableDeclaration(decl);
					break;
				case FunctionDeclaration decl:
					AnalyzeFunctionDeclaration(decl);
					break;
				case IfStatement stmt:
					AnalyzeIfStatement(stmt);
					break;
				case WhileLoop stmt:
					AnalyzeWhileLoop(stmt);
					break;
				case SwitchCase stmt:
					AnalyzeSwitchCase(stmt);
					break;
				default:
					break;
			}
		}

		// Method does a semantic analysis for a block
		// input: block to check
		// return: none
		private void AnalyzeBlock(Block block)
		{
			// set current block
			Block prevBlock = _currentBlock;
			_currentBlock = block;

			// analyze all statements in block
			foreach(Statement stmt in block.Children)
			{
				AnalyzeSubtree(stmt);
			}

			// return to previous block
			_currentBlock = prevBlock;
		}

		// Method does a semantic analysis for a BinaryOperator subtree
		// input: binary operator to check
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
			if (IsRelationalOp(op))
				op.Type = TypeCode.BOOL;
			else
				op.Type = op.Operand(0).Type;
			// check if operation is allowed
			if (!_binOpAllowedTypes[op.Operator].Contains(op.Operand(0).Type))
				throw new TypeError(op);
			
			// additional checks
			if(op.Operator == TokenCode.ASSIGN_OP)
			{
				// check if assigning to a variable
				if (!(op.Operand(0) is Variable))
					throw new AssignmentError(op.Line);
			}
		}

		// Method checks if a binary operator is a relational operator
		// input: BinaryOperator to check
		// return: true if it's relational operator (<, <=, >, >=, ==, !=)
		private bool IsRelationalOp(BinaryOperator op)
		{
			return op.Operator == TokenCode.LESS_OP || op.Operator == TokenCode.LESS_EQUAL_OP ||
				op.Operator == TokenCode.GREATER_OP || op.Operator == TokenCode.GREATER_EQUAL_OP ||
				op.Operator == TokenCode.EQUAL_OP || op.Operator == TokenCode.NOT_EQUAL_OP;
		}

		// Method does a semantic analysis for a UnaryOperator subtree
		// input: binary operator to check
		//		  what type the result needs to be
		// return: none
		private void AnalyzeUnaryOperator(UnaryOperator op)
		{
			// analyze operands
			AnalyzeSubtree(op.Operand());
			// set type
			op.Type = op.Operand().Type;
			// check if operation is allowed
			Dictionary<TokenCode, HashSet<TypeCode>> allowedTypes = op.Prefix ? _unaryPrefixOpAllowedTypes : _unaryPostfixOpAllowedTypes;
			if (!allowedTypes[op.Operator].Contains(op.Type))
				throw new TypeError(op);
		}

		// Method does a semantic analysis on a ternary operator
		// input: ternary operator
		// return: none
		private void AnalyzeTernaryOperator(TernaryOperator op)
		{
			// analyze all operands
			for(int i = 0; i < 3; i++)
				AnalyzeSubtree(op.Operand(i));
			// check types
			if (op.Operand(0).Type != TypeCode.BOOL)
				throw new TypeError(op, "Expected BOOL for first operand, instead got " + op.Operand(0).Type);
			if (op.Operand(1).Type != op.Operand(2).Type)
				throw new TypeError(op, "Inconsistent return types (" + op.Operand(1).Type + ", " + op.Operand(2).Type + ")");
			op.Type = op.Operand(1).Type;
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
				case Primitive<bool> p:
					p.Type = TypeCode.BOOL;
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

		// does a semantic analysis on a variable
		private void AnalyzeVariable(Variable variable)
		{
			// get type from current block's symbol table
			variable.Type = _currentBlock.SymbolTable.GetEntry(variable).ValueType;

			// check if already passed declaration
			if (!_declaredSymbols.Contains(variable.Identifier))
				throw new ReferenceBeforeDeclarationError(variable);
		}

		// if statement analysis
		private void AnalyzeIfStatement(IfStatement stmt)
		{
			// analyze condition
			AnalyzeSubtree(stmt.GetCondition());
			if(stmt.GetCondition().Type != TypeCode.BOOL)
				throw new TypeError(stmt, "Expected BOOL for condition, instead got " + stmt.GetCondition().Type);
			// analyze substatements
			AnalyzeSubtree(stmt.GetTrueBlock());
			if(stmt.HasElse())
				AnalyzeSubtree(stmt.GetFalseBlock());
		}

		// while loop analysis
		private void AnalyzeWhileLoop(WhileLoop stmt)
		{
			// analyze condition
			AnalyzeSubtree(stmt.GetCondition());
			if (stmt.GetCondition().Type != TypeCode.BOOL)
				throw new TypeError(stmt, "Expected BOOL for condition, instead got " + stmt.GetCondition().Type);
			// analyze substatements
			AnalyzeSubtree(stmt.GetBlock());
		}

		// for loop analysis
		private void AnalyzeForLoop(ForLoop stmt)
		{
			Block previousBlock = _currentBlock;
			_currentBlock = stmt;
			// analyze condition
			AnalyzeSubtree(stmt.GetChild(ForLoop.INIT_INDEX));
			AnalyzeSubtree(stmt.GetChild(ForLoop.CONDITION_INDEX));
			AnalyzeSubtree(stmt.GetChild(ForLoop.ACTION_INDEX));
			// check condition type
			Expression condition = stmt.GetChild(ForLoop.CONDITION_INDEX) as Expression;
			if (condition?.Type != TypeCode.BOOL)
				throw new TypeError(stmt, "Expected BOOL for condition, instead got " + condition.Type);

			// analyze block
			AnalyzeSubtree(stmt.GetChild(ForLoop.BODY_INDEX));

			// return to previous block
			_currentBlock = previousBlock;
		}

		// switch case analysis
		private void AnalyzeSwitchCase(SwitchCase stmt)
		{
			// get switch type
			AnalyzeSubtree(stmt.GetChild(0));
			TypeCode type = (stmt.GetChild(0) as Expression).Type;
			// analyze cases
			int startIndex = 1;
			if(stmt.HasDefault)
			{
				AnalyzeSubtree(stmt.GetChild(1));
				startIndex = 2;
			}

			for(int i = startIndex; i < stmt.Children.Count; i += 2)
			{
				AnalyzeSubtree(stmt.GetChild(i));
				AnalyzeSubtree(stmt.GetChild(i+1));
				// check case type
				if ((stmt.GetChild(i) as Expression).Type != type)
				{
					throw new TypeError(stmt,
						"Switch case on type " + type +
						" has a case with type " + (stmt.GetChild(i) as Expression).Type
					);
				}
			}
		}

		// analayzes variable declaration node
		private void AnalyzeVariableDeclaration(VariableDeclaration decl)
		{
			_declaredSymbols.UnionWith(decl.Identifiers);
			AnalyzeSubtree(decl.GetChild(0));
		}

		// analayzes function declaration node
		private void AnalyzeFunctionDeclaration(FunctionDeclaration decl)
		{
			FunctionDeclaration prev = _currentFunction;
			_currentFunction = decl;
			// analyze
			_declaredSymbols.Add(decl.Identifier);
			AnalyzeSubtree(decl.GetChild(0));
			// return to prev
			_currentFunction = prev;
		}

		// analyzes type of return statement
		private void AnalyzeReturn(ReturnStatement ret)
		{
			AnalyzeSubtree(ret.GetExpression());
			// check type
			if(_currentFunction != null && ret.GetExpression().Type != _currentFunction.GetTypeCode())
				throw new TypeError(ret, _currentFunction.GetTypeCode());
		}

		// analyzes function call node
		private void AnalyzeFunctionCall(FunctionCall call)
		{
			AnalyzeVariable(call.Function);
			// get type from symbol table
			call.Type = _currentBlock.SymbolTable.GetEntry(call.Function.Identifier, call.Line).ValueType;
		}
	}
}
