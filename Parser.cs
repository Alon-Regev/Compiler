using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Parser
	{
		Scanner scanner;

		private const int TERNARY_ORDER = 12;

		// constructor
		// program: program string to parse
		public Parser(string program)
		{
			scanner = new Scanner(program);
		}

		// Method parses the program into an AST
		// input: none
		// return: AST as AST_Node
		public AST_Node Parse()
		{
			// return a block of statements
			return ParseBlock();
		}

		// Method parses a statement
		// input: none
		// return: statement tree
		public Statement ParseStatement()
		{
			Statement statement;
			// check statement type
			switch (scanner.Peek().Code)
			{
				case TokenCode.INT_KEYWORD:
				case TokenCode.FLOAT_KEYWORD:
				case TokenCode.BOOL_KEYWORD:
					statement = ParseVariableDeclaration();
					break;
				case TokenCode.OPEN_BRACE:
					return ParseBlock();
				case TokenCode.PRINT_KEYWORD:
					scanner.Next();
					statement = new PrintStatement(ParseExpression());
					break;
				case TokenCode.IF:  // return (don't check semicolon)
					return ParseIfStatement();
				case TokenCode.WHILE:
				case TokenCode.DO:
					return ParseWhileLoop();
				default:	// expression
					statement = new ExpressionStatement(ParseExpression());
					break;
			}
			// check semicolon
			Token stmtEnd = scanner.Next();
			if (stmtEnd.Code != TokenCode.SEMI_COLON)
				throw new UnexpectedToken("Semicolon", stmtEnd);
			return statement;
		}

		// Method parses a local variable declaration
		// input: none
		// return: VariableDeclaration Node
		public VariableDeclaration ParseVariableDeclaration()
		{
			// varDecl := <type> { <identifier> [ = <value>], }

			Token type = scanner.Next();
			VariableDeclaration declaration = new VariableDeclaration(type);

			// add identifiers
			do
			{
				Token identifier = scanner.Next();
				if (identifier.Code != TokenCode.IDENTIFIER)
					throw new UnexpectedToken("identifier", identifier);
				// add to declaration
				declaration.Identifiers.Add(identifier.Value);

				// check optional assignment
				if (scanner.Peek().Code == TokenCode.ASSIGN_OP)
				{
					scanner.Next();
					declaration.AddChild(
						new BinaryOperator(TokenCode.ASSIGN_OP,
							new Variable(identifier),
							ParseExpression()
						)
					);
				}
				// check if comma (and eat it)
			} while (scanner.Peek().Code == TokenCode.COMMA && scanner.Next().Code == TokenCode.COMMA);

			return declaration;
		}

		// Method parses an if-else statement
		// input: none
		// return: parsed if-else statement
		private IfStatement ParseIfStatement()
		{
			// skip if keyword
			scanner.Next(); 
			// get condition and true-block
			Expression condition = ParseExpression();
			Statement trueBlock = ParseStatement();
			// get else-block
			Statement? elseBlock = null;
			if(scanner.Peek().Code == TokenCode.ELSE)
			{
				// skip else keyword and get block
				scanner.Next();
				elseBlock = ParseStatement();
			}
			return new IfStatement(condition, trueBlock, elseBlock);
		}

		// Method parses a while loop
		// input: none
		// return: parsed while loop statement
		private WhileLoop ParseWhileLoop()
		{
			// skip while keyword
			Token startToken = scanner.Next();
			// regular while
			if (startToken.Code == TokenCode.WHILE)
			{
				// get condition and block
				return new WhileLoop(ParseExpression(), ParseStatement());
			}
			// do while
			else
			{
				Statement block = ParseStatement();
				// get keyword
				scanner.Require(TokenCode.WHILE);
				// get condition and return
				Expression condition = ParseExpression();
				scanner.Require(TokenCode.SEMI_COLON);
				return new WhileLoop(condition, block, true);
			}
		}

		// Method parses a block of statements
		// input: none
		// return: Block node
		private Block ParseBlock()
		{
			// block: {<statements>}
			// check open brace
			if (scanner.Next().Code != TokenCode.OPEN_BRACE)
				throw new UnexpectedToken("block open brace {", scanner.Last);

			Block block = new Block(scanner.Last.Line);
			// gather statements
			while(scanner.Peek().Code != TokenCode.CLOSE_BRACE && scanner.Peek().Code != TokenCode.EOF)
			{
				// add statement
				Statement newStatement = ParseStatement();
				block.AddStatement(newStatement);
				// add symbols to symbol table
				if (newStatement is VariableDeclaration)
				{
					VariableDeclaration declaration = newStatement as VariableDeclaration;
					foreach (string identifier in declaration.Identifiers)
					{
						block.SymbolTable.AddEntry(
							identifier,
							declaration.Line,
							new SymbolTableEntry(SymbolType.LOCAL_VAR, declaration.GetTypeCode())
						);
					}
				}
			}

			// check close brace
			if (scanner.Next().Code != TokenCode.CLOSE_BRACE)
				throw new UnexpectedToken("block open brace }", scanner.Last);

			// offset addresses of sub-blocks
			foreach (Statement stmt in block.Children)
			{
				switch(stmt)
				{
					case IfStatement ifStatement:
						Statement?[] statements = new Statement?[] { ifStatement.GetTrueBlock(), ifStatement.GetFalseBlock() };
						foreach (Statement? statement in statements)
						{
							if (statement != null && statement is Block)
							{
								(statement as Block).SymbolTable.ParentTable = block.SymbolTable;
								(statement as Block).OffsetAddresses(block.SymbolTable.VariableBytes());
							}
						}
						break;
					case WhileLoop whileLoop:
						if (whileLoop.GetBlock() is Block)
						{
							(whileLoop.GetBlock() as Block).SymbolTable.ParentTable = block.SymbolTable;
							(whileLoop.GetBlock() as Block).OffsetAddresses(block.SymbolTable.VariableBytes());
						}
						break;
					case Block subBlock:
						// set parent and address offset
						subBlock.SymbolTable.ParentTable = block.SymbolTable;
						subBlock.OffsetAddresses(block.SymbolTable.VariableBytes());
						break;
					default:
						break;

				}
			}

			return block;
		}

		// Method parses a mathematical expression, defined as a sum of terms.
		// input:   order - expression's max operator order
		//			default: includes all operators
		// return:	expression tree
		private Expression ParseExpression(int order = 13)
		{
			// subexp(0) := factor | factor unary_op
			if (order == 0)
				return ParseFactor();

			// subexpression(n) is defined as
			// subexp(n-1) | subexp(n-1) op(n) subexp(n-1)

			Expression node = ParseExpression(order - 1);
			// check ternary
			if (order == TERNARY_ORDER)
				return ParseTernary(node);
			// while peeked token's order is n
			while (GetOperatorOrder(scanner.Peek()) == order)
			{
				Token op = scanner.Next();
				Expression nextSubexp = ParseExpression(order - 1);
				// add binary operator node
				if (RTL_Evaluated(op.Code) &&
					node is BinaryOperator && RTL_Evaluated((node as BinaryOperator).Operator))
				{
					// evaluate right expression under operand 1
					node.SetChild(1, new BinaryOperator(op.Code, (node as BinaryOperator).Operand(1), nextSubexp));
				}
				else
					node = new BinaryOperator(op.Code, node, nextSubexp);
			}

			return node;
		}

		// Method checks if an operator is evaluated Right To Left or not
		// input: operator token
		// return: true if RTL, false otherwise
		private bool RTL_Evaluated(TokenCode op)
		{
			return op switch
			{
				TokenCode.ASSIGN_OP => true,
				_ => false
			};
		}

		// Method parses a ternary operator
		// input: possible first operand (Expression)
		// return: Ternary Expression (or input if there isn't one)
		private Expression ParseTernary(Expression node)
		{
			if (scanner.Peek().Code != TokenCode.QUESTION_MARK)
				return node;
			scanner.Next();
			// parse second operand
			Expression operand2 = ParseExpression(TERNARY_ORDER - 1);
			// check colon
			Token colon = scanner.Next();
			if (colon.Code != TokenCode.COLON)
				throw new UnexpectedToken("Ternary colon token (:)", colon);
			Expression operand3 = ParseExpression(TERNARY_ORDER - 1);
			// return operator
			return new TernaryOperator(node, operand2, operand3);
		}

		// Method returns operator's order (in order of operations)
		// higher value means it's computed later
		// input: operator's token
		// return: operator's order value
		private int GetOperatorOrder(Token t)
		{
			switch(t.Code)
			{
				// assignment
				case TokenCode.ASSIGN_OP:
					return 13;
				// ternary
				case TokenCode.QUESTION_MARK:
					return TERNARY_ORDER;
				// logical
				case TokenCode.LOGIC_OR_OP:
					return 11;
				case TokenCode.LOGIC_AND_OP:
					return 10;
				// bitwise
				case TokenCode.BIT_OR_OP:
					return 9;
				case TokenCode.BIT_XOR_OP:
					return 8;
				case TokenCode.BIT_AND_OP:
					return 7;
				// relational
				case TokenCode.EQUAL_OP:
				case TokenCode.NOT_EQUAL_OP:
					return 6;
				case TokenCode.LESS_OP:
				case TokenCode.LESS_EQUAL_OP:
				case TokenCode.GREATER_OP:
				case TokenCode.GREATER_EQUAL_OP:
					return 5;
				// bitwise shift
				case TokenCode.LEFT_SHIFT:
				case TokenCode.RIGHT_SHIFT:
					return 4;
				// arithmetic
				case TokenCode.ADD_OP:
				case TokenCode.SUB_OP:
					return 3;
				case TokenCode.MUL_OP:
				case TokenCode.DIV_OP:
				case TokenCode.MOD_OP:
					return 2;
				case TokenCode.POW_OP:
					return 1;
				// end of expression
				case TokenCode.EOF:
				case TokenCode.RIGHT_PARENTHESIS:
				case TokenCode.OPEN_BRACE:
				case TokenCode.COLON:
				case TokenCode.SEMI_COLON:
				case TokenCode.COMMA:
					return -1;
				// invalid
				default:
					throw new UnexpectedToken("Operator", t);
			}	
		}

		// Method checks if a token is of an unary postfix operator
		// input: token to check
		// return: true if unary postfix operator, else false
		private bool IsUnaryPostfixOperator(Token t)
		{
			return t.Code == TokenCode.EXCLAMATION_MARK;
		}

		// Method parses a mathematical factor, defined as an integer or as an expression in parentheses.
		// input: none
		// return: factor tree
		private Expression ParseFactor()
		{
			// the most compact part of an expression
			// Factor := <primitive> | (<expression>) | <cast><factor> | <unary_op><factor> | <factor><unary_op> | <variable>
			Token token = scanner.Next();
			Expression result;

			switch (token.Code)
			{
				// --- Primitives
				case TokenCode.INTEGER:
					result = new Primitive<int>(token);
					break;
				case TokenCode.DECIMAL:
					result = new Primitive<float>(token);
					break;
				case TokenCode.BOOLEAN:
					result = new Primitive<bool>(token);
					break;

				// --- Parentheses Expression
				case TokenCode.LEFT_PARENTHESIS:
					Expression node = ParseExpression();
					// check closing parenthesis
					if (scanner.Next().Code != TokenCode.RIGHT_PARENTHESIS)
						throw new MissingParenthesis(token);
					result = node;
					break;

				// --- Castings
				case TokenCode.INT_CAST:
					result = new Cast(ParseFactor(), TypeCode.INT);
					break;
				case TokenCode.FLOAT_CAST:
					result = new Cast(ParseFactor(), TypeCode.FLOAT);
					break;
				case TokenCode.BOOL_CAST:
					result = new Cast(ParseFactor(), TypeCode.BOOL);
					break;

				// --- Unary Prefix Operators
				case TokenCode.BIT_NOT_OP:
				case TokenCode.SUB_OP:          // negation
				case TokenCode.EXCLAMATION_MARK:    // logical not
					result = new UnaryOperator(token.Code, ParseFactor(), true);
					break;

				// --- Identifier
				case TokenCode.IDENTIFIER:
					result = new Variable(token);
					break;

				// --- Unexpected
				default:
					throw new UnexpectedToken("expression", token);
			}

			// check postfix unary operator
			if (IsUnaryPostfixOperator(scanner.Peek()))
				return new UnaryOperator(scanner.Next().Code, result, false);
			else
				return result;
		}
	}
}
