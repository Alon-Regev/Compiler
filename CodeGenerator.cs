using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
	class CodeGenerator
	{
		private AST_Node _tree;
		private Block _currentBlock;
		private string _functionDefinitions = "";
		private List<string> _externFunctions = new List<string> { "_printf", "_malloc", "_free" };

		private int _labelCounter = 0;

		List<DataSectionVar> _varsToDeclare = new List<DataSectionVar>
		{
			new DataSectionVar("__temp", DataSize.DWORD, "0"),
			DataSectionVar.StringConstant("format", "%?", true),
			DataSectionVar.StringConstant("formatin", "%?", false),
			DataSectionVar.StringConstant("true_string", "true", false),
			DataSectionVar.StringConstant("false_string", "false", false),
		};

		private HashSet<string> _helperFunctionsUsed = new HashSet<string>();
		private HashSet<string> _macrosUsed = new HashSet<string>();

		// Constructor
		// program: program's source code as string
		public CodeGenerator(AST_Node tree)
		{
			_tree = tree;
		}

		// Method generates assembly program from input program
		// input: none
		// return: none
		public string GenerateAssembly()
		{
			string programAssembly = ToAssembly(_tree);
			string data = DataSectionAssembly();
			return
				"global _main\n" +
				ExternFunctions() + 
				"\n" +
				MacrosAssembly() +
				"\n" +
				"section .data\n" +
				Indent(data) +
				"\n" +

				"section .text\n" +
				"_main:\n" +
				Indent(
					"push ebp\n" +
					"mov ebp, esp\n\n" +
					programAssembly +
					"\n" +
					"mov esp, ebp\n" +
					"pop ebp\n" +
					"mov eax, 0\n" +
					"ret"
				) + "\n\n" +
				_functionDefinitions +
				HelperFunctionsAssembly();
		}

		// Methods generate assembly code from subtrees
		// tree: subtree to turn to assembly code
		// return: assembly as string
		private string ToAssembly(AST_Node tree)
		{
			switch (tree)
			{
				// --- Expressions
				case BinaryOperator op when op.Operator == TokenCode.ASSIGN_OP:
					return AssignmentAssembly(op);
				case BinaryOperator op:
					return ToAssembly(op);
				case UnaryOperator op:
					return ToAssembly(op);
				case TernaryOperator op:
					return ToAssembly(op);
				case IPrimitive p:
					return ToAssembly(p);
				case Variable v:
					return ToAssembly(v);
				case Cast c:
					return ToAssembly(c);
				case FunctionCall call:
					return ToAssembly(call);
				case ArrayIndex arrayIndex:
					return ToAssembly(arrayIndex);
				// --- Statements
				case ForLoop stmt:
					return ToAssembly(stmt);
				case Block block:
					return ToAssembly(block);
				case ExpressionStatement stmt:
					return ToAssembly(stmt.GetExpression());
				case PrintStatement stmt:
					return ToAssembly(stmt);
				case ReturnStatement stmt:
					return ToAssembly(stmt);
				case VariableDeclaration decl:
					return ToAssembly(decl);
				case FunctionDeclaration decl:
					string toAdd = ToAssembly(decl);
					_functionDefinitions += toAdd;
					return "";
				case ExternStatement stmt:
					_externFunctions.Add(stmt.Identifier);
					return "";
				case IfStatement stmt:
					return ToAssembly(stmt);
				case WhileLoop stmt:
					return ToAssembly(stmt);
				case SwitchCase stmt:
					return ToAssembly(stmt);
				case NewExpression expr:
					return ToAssembly(expr);
				case DeleteStatement stmt:
					return ToAssembly(stmt);
				default:
					return "";
			}
		}

		// Methods generate assembly code for a block of statements
		// tree: Block to turn into ASM
		// return: assembly as string
		private string ToAssembly(Block block)
		{
			// change current block
			Block prevBlock = _currentBlock;
			_currentBlock = block;

			string result = "";
			// allocate memory for local variables
			int stackOffset = block.SymbolTable.VariableBytes();
			if (stackOffset != 0)
				result += "sub esp, " + stackOffset + "\n";
			// add assembly code for all statements
			foreach (Statement stmt in block.Children)
			{
				result += ToAssembly(stmt);
			}
			// deallocate memory from the stack
			if (stackOffset != 0)
				result += "add esp, " + stackOffset + "\n";

			// return to previous block and return
			_currentBlock = prevBlock;

			return result;
		}

		// binary operator assembly rules:
		// result	ax
		// operands ax, bx
		public static string DEFAULT_OPERATOR_BINARY = "Invalid binary operation passed semantic analysis";
		public static string DEFAULT_TYPE_BINARY = "Binary operator has no type (or invalid type) after semantic analysis";
		private string ToAssembly(BinaryOperator op)
		{
			string operandsASM = "";
			if (op.Operand(0).Type != TypeCode.BOOL)    // bool logical operators use operands differently (short-circuit)
			{
				// get operand2 on stack
				operandsASM += ToAssembly(op.Operand(1));
				operandsASM += "push eax\n";
				// get operand1 in eax
				operandsASM += ToAssembly(op.Operand(0));
				// pop operand2 to ebx
				operandsASM += "pop ebx\n";
			}

			// check ptr operations
			if (op.Operand(0).Type.Pointer != 0)
				return operandsASM + PointerOperatorAssembly(op.Operand(0), "eax", op.Operand(1), "ebx", op);
			else if (op.Operand(1).Type.Pointer != 0)
				return operandsASM + PointerOperatorAssembly(op.Operand(1), "ebx", op.Operand(0), "eax", op);

			// calculate based on input type
			switch (op.Operand(0).Type)
			{
				case ValueType t when t == new ValueType(TypeCode.INT):
					return operandsASM +
						op.Operator switch
						{
							// --- Arithmetic
							TokenCode.ADD_OP => "add eax, ebx\n",
							TokenCode.SUB_OP => "sub eax, ebx\n",
							TokenCode.MUL_OP => "mul ebx\n",
							TokenCode.DIV_OP => "xor edx, edx\ndiv ebx\n",
							TokenCode.MOD_OP => "xor edx, edx\ndiv ebx\n" +
												"mov eax, edx\n",
							// --- Bitwise
							TokenCode.BIT_OR_OP => "or eax, ebx\n",
							TokenCode.BIT_XOR_OP => "xor eax, ebx\n",
							TokenCode.BIT_AND_OP => "and eax, ebx\n",
							TokenCode.LEFT_SHIFT => "mov cl, bl\n" +
													"shl eax, cl\n",
							TokenCode.RIGHT_SHIFT => "mov cl, bl\n" +
													"shr eax, cl\n",
							// --- Relational
							TokenCode.LESS_OP => "cmp eax, ebx\nmov eax, 0\nsetl al\n",
							TokenCode.LESS_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetle al\n",
							TokenCode.GREATER_OP => "cmp eax, ebx\nmov eax, 0\nsetg al\n",
							TokenCode.GREATER_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetge al\n",
							TokenCode.EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsete al\n",
							TokenCode.NOT_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetne al\n",
							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						};

				case ValueType t when t == new ValueType(TypeCode.FLOAT):
					return operandsASM +
						// load eax and ebx to fpu
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						"mov [__temp], ebx\n" +
						"fld dword [__temp]\n" +
						// calculate operation
						op.Operator switch
						{
							// --- Arithmetic
							TokenCode.ADD_OP => "faddp\n",
							TokenCode.SUB_OP => "fsubp\n",
							TokenCode.MUL_OP => "fmulp\n",
							TokenCode.DIV_OP => "fdivp\n",
							TokenCode.POW_OP => HelperCall("pow"),
							// --- Relational
							TokenCode.LESS_OP => Macro("float_comparison", "0000000000000000b"),    // not condition flags
							TokenCode.LESS_EQUAL_OP => Macro("float_comparison_inverse", "0000000100000000b"),  // not greater,
							TokenCode.GREATER_OP => Macro("float_comparison", "0000000100000000b"), // carry flag
							TokenCode.GREATER_EQUAL_OP => Macro("float_comparison_inverse", "0000000000000000b"),   // not less
							TokenCode.EQUAL_OP => Macro("float_comparison", "0100000000000000b"),   // zero flag
							TokenCode.NOT_EQUAL_OP => Macro("float_comparison_inverse", "0100000000000000b"),   // not equal
							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						} +
						// mov result from fpu to eax if type is float
						(op.Type == TypeCode.FLOAT ?
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n" : "");

				case ValueType t when t == new ValueType(TypeCode.BOOL):
					string label = GetLabel();
					return operandsASM +
						op.Operator switch
						{
							TokenCode.LOGIC_AND_OP => ToAssembly(op.Operand(0)) +
														"cmp eax, 0\n" +
														"je " + label + "\n" +
														ToAssembly(op.Operand(1)) +
														label + ":\n",

							TokenCode.LOGIC_OR_OP => ToAssembly(op.Operand(0)) +
														"cmp eax, 1\n" +
														"je " + label + "\n" +
														ToAssembly(op.Operand(1)) +
														label + ":\n",

							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						};

				default:
					throw new ImplementationError(DEFAULT_TYPE_BINARY);
			}
		}
		public string PointerOperatorAssembly(Expression ptr, string ptrReg, Expression other, string otherReg, BinaryOperator op)
		{
			switch (op.Operator)
			{
				case TokenCode.ADD_OP:
					// add integer
					return "lea eax, [" + ptrReg + " + 4 * " + otherReg + "]\n";
				case TokenCode.SUB_OP:
					if(other.Type.Pointer != 0)
					{
						// pointer subtraction
						return "sub " + ptrReg + ", " + otherReg + "\n" +
							(ptrReg == "eax" ? "" : "mov " + ptrReg + ", eax\n") +
							"xor edx, edx\n" +
							"mov ebx, 4\n" +
							"div ebx\n";
					}
					else
					{
						// sub integer
						return "neg " + otherReg + "\n" +  
							"lea eax, [" + ptrReg + " + 4 * " + otherReg + "]\n";
					}
				default:
					return "";
			}
		}

		// generates assembly for assignment
		// rules: value to assign at eax, moves into memory
		private string AssignmentAssembly(BinaryOperator op)
		{
			switch (op.Operand(0))
			{
				case Variable v:
					Tuple<string, string> addressASM = VariableAddress(v);

					return ToAssembly(op.Operand(1)) +
						addressASM.Item1 +
						"mov [" + addressASM.Item2 + "], eax\n";

				case UnaryOperator valueAt when valueAt.Operator == TokenCode.MUL_OP:
					return ToAssembly(valueAt.Operand()) +  // address
						"push eax\n" +
						ToAssembly(op.Operand(1)) + // new value at eax
						"pop ebx\n" +   // address at ebx
						"mov [ebx], eax\n";

				case ArrayIndex arrayIndex:
					return ToAssembly(arrayIndex.Index()) +
						"push eax\n" + // index in stack
						ToAssembly(arrayIndex.Array()) +
						"push eax\n" + // base in stack
						ToAssembly(op.Operand(1)) + // new value in eax
						"pop ebx\n" +   // base in ebx
						"pop ecx\n" +  // index in ecx
						"mov [ebx + 4 * ecx], eax\n";

				default:
					throw new ImplementationError("Invalid assignment passed semantic analysis");
			}
		}

		// unary operator assembly rules:
		// operand -> result: ax -> ax
		public static string DEFAULT_OPERATOR_UNARY = "Invalid unary operator passed semantic analysis";
		public static string DEFAULT_TYPE_UNARY = "Unary operator has no type (or invalid type) after semantic analysis";
		private string ToAssembly(UnaryOperator op)
		{
			string operandASM = ToAssembly(op.Operand());
			// calculate result of op
			switch (op.Type)
			{
				case ValueType t when op.Operator == TokenCode.BIT_AND_OP:
					Tuple<string, string> addressASM = VariableAddress(op.Operand() as Variable);
					return addressASM.Item1 +
						"lea eax, [" + addressASM.Item2 + "]\n";

				case ValueType t when op.Operator == TokenCode.MUL_OP:
					return operandASM + 
						"mov eax, [eax]\n";

				case ValueType t when t == new ValueType(TypeCode.INT):
					return operandASM +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.BIT_NOT_OP, true) => "not eax\n",
							(TokenCode.SUB_OP, true) => "neg eax\n",    // negation
							(TokenCode.EXCLAMATION_MARK, false) => HelperCall("factorial"),
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						};

				case ValueType t when t == new ValueType(TypeCode.FLOAT):
					return operandASM +
						// load operand to fpu
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.SUB_OP, true) => "fchs\n",    // negation
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						} +
						// move result back into eax
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n";

				case ValueType t when t == new ValueType(TypeCode.BOOL):
					return operandASM +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.EXCLAMATION_MARK, true) => "cmp eax, 0\n" +
																	"mov eax, 0\n" +
																	"sete al\n",    // logical not
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						};

				default:
					throw new ImplementationError(DEFAULT_TYPE_UNARY);
			}
		}

		// ternary operator assembly:
		// condition operand: eax
		// operands: ebx, ecx
		// result: eax
		private string ToAssembly(TernaryOperator op)
		{
			string operandsASM = "";
			// get operand3 on stack
			operandsASM += ToAssembly(op.Operand(2));
			operandsASM += "push eax\n";
			// get operand2 on the stack
			operandsASM += ToAssembly(op.Operand(1));
			operandsASM += "push eax\n";
			// put all operands in place
			operandsASM += ToAssembly(op.Operand(0));
			operandsASM += "pop ebx\npop ecx\n";

			// add code for ternary op
			return operandsASM +
				"cmp eax, 0\n" +
				"cmove eax, ecx\n" +
				"cmovne eax, ebx\n";
		}

		// primitive
		// place at eax
		private string ToAssembly(IPrimitive primitive)
		{
			switch (primitive)
			{
				case Primitive<int> p:
					return "mov eax, " + p.Value + "\n";
				case Primitive<float> p:
					DataSectionVar floatConst = DataSectionVar.FloatConstant(p.Value);
					_varsToDeclare.Add(floatConst);
					return "mov eax, [" + floatConst.Name + "]\n";
				case Primitive<bool> p:
					return "mov eax, " + (p.Value ? 1 : 0) + "\n";
				default:
					return "";
			}
		}

		// generate assembly for casting
		// operand at eax
		// result at eax
		private string ToAssembly(Cast cast)
		{
			string result = ToAssembly(cast.Child());
			// only float cast changes data
			ValueType floatType = new ValueType(TypeCode.FLOAT);
			if (cast.FromType == floatType && cast.Type != floatType)
			{
				// load eax to fpu
				result += "mov [__temp], eax\n" +
					"fld dword [__temp]\n";
				// store in eax as integer
				result += "fistp dword [__temp]\n" +
					"mov eax, [__temp]\n";
			}
			if(cast.Type == floatType && cast.FromType != floatType)
			{
				// load eax to fpu as integer
				// load eax to fpu
				result += "mov [__temp], eax\n" +
					"fild dword [__temp]\n";
				// store in eax
				result += "fstp dword [__temp]\n" +
					"mov eax, [__temp]\n";
			}
			return result;
		}

		// function call assembly
		private string ToAssembly(FunctionCall call)
		{
			string result = "";
			// push pebp
			// find function call entry
			Tuple<string, string> functionScopeInfo = VariableAddress(call.Function());
			SymbolTableEntry entry = _currentBlock.SymbolTable.GetOuterEntry(call.Function()).Item1;
			if (entry.SymbolType != SymbolType.BUILTIN_FUNCTION)
			{
				if (functionScopeInfo.Item1 != "")
				{
					// push pebp
					result += functionScopeInfo.Item1;
					result += "push ebx\n";
				}
				else
					result += "push ebp\n";
			}
			// push arguments
			for(int i = call.ArgumentCount() - 1; i >= 0 ; i--)
			{
				result += ToAssembly(call.GetArgument(i)) +
					"push eax\n";
			}
			if (entry.SymbolType == SymbolType.BUILTIN_FUNCTION)
				result += HelperCall(call.Function().Identifier);
			else
				result += "call " + call.Function().Identifier + "\n";
			// pop arguments
			int argCount = call.ArgumentCount();
			if (argCount != 0)
				result += "add esp, " + argCount * 4 + "\n";
			return result;
		}

		// array index assembly
		private string ToAssembly(ArrayIndex arrayIndex)
		{
			return
				// index expression in stack
				ToAssembly(arrayIndex.Index()) +
				"push eax\n" +
				// array in eax, index in ebx
				ToAssembly(arrayIndex.Array()) +
				"pop ebx\n" +
				// access element
				"mov eax, [eax + ebx * 4]\n";
		}

		// generate assembly for variable reference
		// load local var from memory to eax
		private string ToAssembly(Variable variable)
		{
			Tuple<string, string> addressASM = VariableAddress(variable);
			return addressASM.Item1 +
				"mov eax, [" + addressASM.Item2 + "]\n";
		}

		// method gets assembly address of variable
		// input: variable
		// return: assembly for address, tuple (code before, address)
		private Tuple<string, string> VariableAddress(Variable variable)
		{
			Tuple<SymbolTableEntry, SymbolTable> entry = _currentBlock.SymbolTable.GetOuterEntry(variable);

			if (entry.Item2 == _currentBlock.SymbolTable)
				return Tuple.Create("", "ebp" + (-entry.Item1.Address).ToString(" + #; - #;"));
			else
			{
				string asm = "";
				string baseRegister = "ebp";
				SymbolTable outer = _currentBlock.SymbolTable;
				while (entry.Item2 != outer)
				{
					outer = outer.GetOuterTable();
					// get address of last parameter (pebp)
					SymbolTableEntry pebp = _currentBlock.SymbolTable.GetEntry("pebp", variable.Line);
					// get address from outer table
					entry = outer.GetOuterEntry(variable);
					asm += "mov ebx, [" + baseRegister + (-pebp.Address).ToString(" + #; - #;") + "]\n";
					baseRegister = "ebx";
				}
				return Tuple.Create(asm, "ebx" + (-entry.Item1.Address).ToString(" + #; - #;"));
			}
		}

		// generate assembly for variable declaration
		// load local var from memory to eax
		private string ToAssembly(VariableDeclaration variableDeclaration)
		{
			string result = "";
			// add assignments ASM
			foreach (AST_Node child in variableDeclaration.Children)
			{
				result += ToAssembly(child);
			}
			return result;
		}

		// generate assembly for function definition
		private string ToAssembly(FunctionDeclaration function)
		{
			return function.Identifier + ":\n" +
				Indent(
					"push ebp\n" +
					"mov ebp, esp\n\n" +
					ToAssembly(function.GetChild(0)) +
					"\nmov esp, ebp\n" +
					"pop ebp\n" +
					"ret"
				) +
				"\n\n";
		}

		// Method generates assembly for a print statement
		private string ToAssembly(PrintStatement statement)
		{
			return ToAssembly(statement.GetExpression()) +
				statement.GetExpression().Type.TypeCode switch
				{
					TypeCode.INT => HelperCall("print_int"),
					TypeCode.FLOAT => HelperCall("print_float"),
					TypeCode.BOOL => HelperCall("print_bool"),
					_ => ""
				};
		}

		// Method generates assembly for a return statement
		// return value in eax
		private string ToAssembly(ReturnStatement stmt)
		{
			return
				// calculate return value in eax
				ToAssembly(stmt.GetExpression()) +
				"mov esp, ebp\n" +
				"pop ebp\n" +
				"ret\n";
		}

		// Method generates assembly for an if statement
		private string ToAssembly(IfStatement stmt)
		{
			string elseLabel = GetLabel();
			string finalLabel = stmt.HasElse() ? GetLabel() : "";
			return
				// condition in eax
				ToAssembly(stmt.GetCondition()) +
				// cmp and jump
				"cmp eax, 0\n" +
				"je " + elseLabel + "\n" +
				// code for if block
				ToAssembly(stmt.GetTrueBlock()) +
				(stmt.HasElse() ? "jmp " + finalLabel + "\n" : "") +
				// end of if block, start else
				elseLabel + ":\n" +
				(stmt.HasElse() ? ToAssembly(stmt.GetFalseBlock()) : "") +
				(stmt.HasElse() ? finalLabel + ":\n" : "");
		}

		// Method generates assembly for an if statement
		private string ToAssembly(WhileLoop stmt)
		{
			if (stmt.IsDoWhile)
			{
				string loopStartLabel = GetLabel();
				return
					loopStartLabel + ":\n" +
					ToAssembly(stmt.GetBlock()) +
					ToAssembly(stmt.GetCondition()) +
					"cmp eax, 0\n" +
					"jne " + loopStartLabel + "\n";
			}
			else
			{
				string loopStartLabel = GetLabel();
				string loopEndLabel = GetLabel();
				return
					loopStartLabel + ":\n" +
					ToAssembly(stmt.GetCondition()) +
					"cmp eax, 0\n" +    // if false end loop
					"je " + loopEndLabel + "\n" +
					ToAssembly(stmt.GetBlock()) +
					"jmp " + loopStartLabel + "\n" +
					loopEndLabel + ":\n";
			}
		}

		// Method generates assembly for an if statement
		private string ToAssembly(ForLoop stmt)
		{
			// change current block
			Block prevBlock = _currentBlock;
			_currentBlock = stmt;

			string loopStartLabel = GetLabel();
			string loopEndLabel = GetLabel();
			string result = "";
			// start block
			int stackOffset = stmt.SymbolTable.VariableBytes();
			if (stackOffset != 0)
				result += "sub esp, " + stackOffset + "\n";
			// loop
			result +=
				// initialization
				ToAssembly(stmt.GetChild(ForLoop.INIT_INDEX)) +
				loopStartLabel + ":\n" +
				// check condition
				ToAssembly(stmt.GetChild(ForLoop.CONDITION_INDEX)) +
				"cmp eax, 0\n" +
				"je " + loopEndLabel + "\n" +
				// body
				ToAssembly(stmt.GetChild(ForLoop.BODY_INDEX)) +
				// loop end
				ToAssembly(stmt.GetChild(ForLoop.ACTION_INDEX)) +
				"jmp " + loopStartLabel + "\n" +
				loopEndLabel + ":\n";
			// end block and return
			if (stackOffset != 0)
				result += "add esp, " + stackOffset + "\n";
			_currentBlock = prevBlock;
			return result;
		}

		// Method generates assembly for a switch case statement
		private string ToAssembly(SwitchCase stmt)
		{
			string result = "";
			// generate labels
			List<string> caseLabels = new List<string>();
			int caseCount = (stmt.Children.Count - 1) / 2;
			for (int i = 0; i < caseCount; i++)
			{
				caseLabels.Add(GetLabel());
			}
			string endLabel = GetLabel();
			int caseStartIndex = stmt.HasDefault ? 2 : 1;
			// get switch value on the stack
			result += ToAssembly(stmt.GetChild(0)) +
				"push eax\n";
			// case jumps
			for(int i = 0; i < caseCount; i++)
			{
				Expression caseExpression = stmt.GetChild(caseStartIndex + 2 * i) as Expression;
				result +=
					ToAssembly(caseExpression) +
					"cmp [esp], eax\n" +
					"je " + caseLabels[i] + "\n";
			}
			// default
			if(stmt.HasDefault)
			{
				result += ToAssembly(stmt.GetChild(1)) + 
					"jmp " + endLabel + "\n";
			}
			// case blocks
			for(int i = 0; i < caseCount; i++)
			{
				Statement caseStatement = stmt.GetChild(caseStartIndex + 2 * i + 1) as Statement;
				result +=
					caseLabels[i] + ":\n" +
					ToAssembly(caseStatement);
				if(i != caseCount - 1)
					result += "jmp " + endLabel + "\n";
			}
			// switch case exit
			result += endLabel + ":\n" + 
				"sub esp, 4\n\n";	// pop switch value
			return result;
		}

		// Method adds necessary global variables after turning program to assembly
		// input: none
		// return: assembly code for data section
		private string DataSectionAssembly()
		{
			string result = "";
			foreach(DataSectionVar v in _varsToDeclare)
			{
				result += v.ToAssembly();
			}
			return result;
		}

		// Method adds indent before every line
		// input: string to indent
		// return: indented string
		private string Indent(string str)
		{
			return "\t" + str.Replace("\n", "\n\t");
		}

		// Method returns an assembly call to a helper function and makes sure it's added to the final assembly.
		// input: name of helper function
		// return: function call assembly as string
		private string HelperCall(string functionName)
		{
			_helperFunctionsUsed.Add(functionName);
			return "call " + functionName + "\n";
		}

		// Method returns an assembly macro use and makes sure it's added to the final assembly
		// input: name of macro, parameters
		// return: macro call
		private string Macro(string macroName, string parameters)
		{
			_macrosUsed.Add(macroName);
			return macroName + " " + parameters + "\n";
		}

		// Method returns assembly code for used helper functions and macros
		// input: none
		// return: assembly code for definitions of used functions
		private string HelperFunctionsAssembly()
		{
			string[] functions = Properties.Resources.Functions.Split(";FUNCTION;\r\n", StringSplitOptions.RemoveEmptyEntries);
			string result = "";
			// add used functions to result
			foreach(string function in functions)
			{
				string name = function.Split(":")[0];
				// if this function was used, add its definition
				if (_helperFunctionsUsed.Contains(name))
					result += function;
			}
			return result;
		}
		private string MacrosAssembly()
		{
			string result = "";
			string[] macros = Properties.Resources.Macros.Split("%macro ", StringSplitOptions.RemoveEmptyEntries);
			// add used macros to result
			foreach (string macro in macros)
			{
				string name = macro.Split()[0];
				// if this function was used, add its definition
				if (_macrosUsed.Contains(name))
					result += "%macro " + macro + "\n";
			}
			return result;
		}

		// Method returns a unique label name every call
		// input: none
		// return: unique label name as string
		private string GetLabel()
		{
			return "__" + _labelCounter++;
		}

		// Method reuturns asm for defining extern functions
		private string ExternFunctions()
		{
			string result = "";
			foreach(string func in _externFunctions)
			{
				result += "extern " + func + "\n";
			}
			return result;
		}

		private string ToAssembly(NewExpression expr)
		{
			return ToAssembly(expr.Size()) +
				"mov ebx, 4\n" +
				"mul ebx\n" +
				"push eax\n" +
				"call _malloc\n" +
				"add esp, 4\n";
		}

		private string ToAssembly(DeleteStatement stmt)
		{
			return ToAssembly(stmt.GetExpression()) +
				"push eax\n" +
				"call _free\n" +
				"add esp, 4\n";
		}
	}
}
