using System;
using System.Collections.Generic;

namespace Compiler
{
	class SemanticAnalyzer
	{
		private AST_Node _tree;
		private Block _currentBlock;
		private FunctionDeclaration _currentFunction = null;

		private HashSet<string> _declaredSymbols = new HashSet<string>();

		private static Dictionary<TokenCode, HashSet<ValueType>> _binOpAllowedTypes = new Dictionary<TokenCode, HashSet<ValueType>>
		{
			// --- Arithmetic
			{ TokenCode.ADD_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.SUB_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.MUL_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.DIV_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.MOD_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.POW_OP, new HashSet<ValueType>{ new ValueType(TypeCode.FLOAT)} },

			// --- Bitwise
			{ TokenCode.BIT_AND_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },
			{ TokenCode.BIT_OR_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },
			{ TokenCode.BIT_XOR_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },
			{ TokenCode.LEFT_SHIFT, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },
			{ TokenCode.RIGHT_SHIFT, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },

			// --- Logical
			{ TokenCode.LOGIC_AND_OP, new HashSet<ValueType>{ new ValueType(TypeCode.BOOL) } },
			{ TokenCode.LOGIC_OR_OP, new HashSet<ValueType>{ new ValueType(TypeCode.BOOL) } },

			// --- Relational
			{ TokenCode.LESS_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.LESS_EQUAL_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.GREATER_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.GREATER_EQUAL_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.EQUAL_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },
			{ TokenCode.NOT_EQUAL_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },

			// --- Assignment
			{ TokenCode.ASSIGN_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT), new ValueType(TypeCode.BOOL) } },
		};

		private static Dictionary<TokenCode, HashSet<ValueType>> _unaryPrefixOpAllowedTypes = new Dictionary<TokenCode, HashSet<ValueType>>
		{
			// --- Arithmetic
			{ TokenCode.BIT_NOT_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },
			{ TokenCode.SUB_OP, new HashSet<ValueType>{ new ValueType(TypeCode.INT), new ValueType(TypeCode.FLOAT) } },	// negation
			{ TokenCode.EXCLAMATION_MARK, new HashSet<ValueType>{ new ValueType(TypeCode.BOOL) } },			// logic not
		};

		private static Dictionary<TokenCode, HashSet<ValueType>> _unaryPostfixOpAllowedTypes = new Dictionary<TokenCode, HashSet<ValueType>>
		{
			// --- Arithmetic
			{ TokenCode.EXCLAMATION_MARK, new HashSet<ValueType>{ new ValueType(TypeCode.INT) } },	// factorial
		};

		private static HashSet<TokenCode> _pointerOperators = new HashSet<TokenCode>
		{
			TokenCode.ASSIGN_OP,
		};

		// built in function list
		public struct BuiltInFunctionData
		{
			public string Identifier;
			public ValueType ReturnType;
			public List<KeyValuePair<string, ValueType>> Parameters;
		}

		public List<BuiltInFunctionData> builtInFunctions = new List<BuiltInFunctionData>
		{
			new BuiltInFunctionData{
				Identifier = "input_int",
				ReturnType = new ValueType(TypeCode.INT, 0, -1),
				Parameters = new List<KeyValuePair<string, ValueType>>()
			}
		};

		// Constructor
		// tree: AST to check
		public SemanticAnalyzer(AST_Node tree)
		{
			_tree = tree;
			_currentBlock = tree as Block;
			AddBuiltInFunctions();
		}

		// Method adds built in functions to base symbol table.
		// input: none
		// return: none
		public void AddBuiltInFunctions()
		{
			if (!(_tree is Block))
				return;
			foreach (BuiltInFunctionData func in builtInFunctions)
			{
				AddBuiltInFunction(func);
			}
		}

		public void AddBuiltInFunction(BuiltInFunctionData func)
		{
			_currentBlock.SymbolTable.AddEntry(func.Identifier, -1,
				new SymbolTableEntry(SymbolType.BUILTIN_FUNCTION, func.ReturnType,
				new FunctionDeclaration(func.ReturnType, func.Identifier, null, func.Parameters)
			));
			_declaredSymbols.Add(func.Identifier);
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
				case ExternStatement stmt:
					AnalyzeExtern(stmt);
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
			ValueType type = op.Operand(0).Type;
			// set type
			if (IsRelationalOp(op))
				op.Type.Set(TypeCode.BOOL);
			else
				op.Type.Set(type);
			// check if operation is allowed
			if (!_binOpAllowedTypes[op.Operator].Contains(type) &&
				// pointer op
				!(type.Pointer != 0 && _pointerOperators.Contains(op.Operator)))

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
			Dictionary<TokenCode, HashSet<ValueType>> allowedTypes = op.Prefix ? _unaryPrefixOpAllowedTypes : _unaryPostfixOpAllowedTypes;
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
					p.Type.Set(TypeCode.INT);
					break;
				case Primitive<float> p:
					p.Type.Set(TypeCode.FLOAT);
					break;
				case Primitive<bool> p:
					p.Type.Set(TypeCode.BOOL);
					break;
				default:
					break;
			}
		}

		// does an analysis on a cast
		private void AnalyzeCast(Cast cast)
		{
			AnalyzeSubtree(cast.GetChild(0));
			cast.FromType.Set(cast.Child().Type);
		}

		// does a semantic analysis on a variable
		private void AnalyzeVariable(Variable variable)
		{
			SymbolTableEntry entry = _currentBlock.SymbolTable.GetOuterEntry(variable).Item1;

			// get type from current block's symbol table
			variable.Type = entry.ValueType;

			// check if already passed declaration
			if (entry.SymbolType != SymbolType.PARAMETER && !_declaredSymbols.Contains(variable.Identifier))
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
			ValueType type = (stmt.GetChild(0) as Expression).Type;
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

		// analyzes extern statement and adds it to declared symbols
		private void AnalyzeExtern(ExternStatement stmt)
		{
			AddBuiltInFunction(new BuiltInFunctionData
			{
				Identifier = stmt.Identifier,
				ReturnType = stmt.ReturnType,
				Parameters = stmt.Parameters
			});
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
			AnalyzeVariable(call.Function());
			// get type from symbol table
			Tuple<SymbolTableEntry, SymbolTable> entryInfo = _currentBlock.SymbolTable.GetOuterEntry(call.Function());
			SymbolTableEntry entry = entryInfo.Item1;
			if (entry.SymbolType != SymbolType.FUNCTION && entry.SymbolType != SymbolType.BUILTIN_FUNCTION)
				throw new TypeError("Calling variable \"" + call.Function().Identifier + "\" which is not a function", call.Line);
			call.Type = entry.ValueType;
			// check arguments
			FunctionDeclaration decl = entry.Declaration as FunctionDeclaration;
			if (decl.Parameters.Count != call.ArgumentCount())
				throw new TypeError(
					"Wrong number of arguments when calling function \"" + call.Function().Identifier + "\"" +
					" (" + call.ArgumentCount() + " given instead of " + decl.Parameters.Count + ")", call.Line);
			// check arguments types
			for(int i = 0; i < call.ArgumentCount(); i++)
			{
				// analyze argument
				AnalyzeSubtree(call.GetArgument(i));
				if (call.GetArgument(i).Type != decl.Parameters[i].Value)
					throw new TypeError("Argument " + i + " of function \"" + call.Function().Identifier + 
						"\" is type " + call.GetArgument(i).Type + " but " + decl.Parameters[i].Value + " was expected", call.Line);
			}
		}
	}
}
