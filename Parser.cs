﻿using System;
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
			return ParseBlock(false);
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
				case TokenCode.VOID:
					ValueType type = ParseType();
					Token identifier = scanner.Require(TokenCode.IDENTIFIER);
					// check if variable or function declaration
					if (scanner.Peek().Code == TokenCode.LEFT_PARENTHESIS)
						return ParseFunctionDeclaration(type, identifier);
					else
						statement = ParseVariableDeclaration(type, identifier);
					break;
				case TokenCode.OPEN_BRACE:
					return ParseBlock();
				case TokenCode.PRINT_KEYWORD:
					scanner.Next();
					statement = new PrintStatement(ParseExpression());
					break;
				case TokenCode.EXTERN:
					statement = ParseExtern();
					break;
				case TokenCode.RETURN:
					scanner.Next();
					statement = new ReturnStatement(ParseExpression());
					break;
				case TokenCode.IF:  // return (don't check semicolon)
					return ParseIfStatement();
				case TokenCode.WHILE:
				case TokenCode.DO:
					return ParseWhileLoop();
				case TokenCode.FOR:
					return ParseForLoop();
				case TokenCode.SWITCH:
					return ParseSwitchCase();
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

		// Method parses a value type
		// input: none
		// return: Value type
		public ValueType ParseType()
		{
			Token baseType = scanner.Next();
			// get pointer count
			int pointer = 0;
			while (scanner.NextIf(TokenCode.MUL_OP))
				pointer++;

			return new ValueType(baseType, pointer);
		}

		// Method parses a local variable declaration
		// input: none
		// return: VariableDeclaration Node
		public VariableDeclaration ParseVariableDeclaration(ValueType type=null, Token identifier=null)
		{
			// varDecl := <type> { <identifier> [ = <value>], }

			if (type == null)
				type = ParseType();

			VariableDeclaration declaration = new VariableDeclaration(type);

			bool first = identifier != null;
			// add identifiers
			do
			{
				// get identifier (first was already taken)
				if (!first)
					identifier = scanner.Require(TokenCode.IDENTIFIER);
				else
					first = false;
				// add to declaration
				declaration.Identifiers.Add(identifier.Value);

				// check optional assignment
				if (scanner.NextIf(TokenCode.ASSIGN_OP))
				{
					declaration.AddChild(
						new BinaryOperator(TokenCode.ASSIGN_OP,
							new Variable(identifier),
							ParseExpression()
						)
					);
				}
				// check if comma (and eat it)
			} while (scanner.NextIf(TokenCode.COMMA));

			return declaration;
		}

		// Method parses a function declaration
		// input: none
		// return: VariableDeclaration Node
		public FunctionDeclaration ParseFunctionDeclaration(ValueType retType, Token identifier)
		{
			List<KeyValuePair<string, ValueType>> parameters = ParseParameters();
			Block implementation = ParseBlock();

			return new FunctionDeclaration(retType, identifier.Value, implementation, parameters);
		}

		// Method parses function parameters
		// input: none
		// return: list of (param name, param type)
		public List<KeyValuePair<string, ValueType>> ParseParameters()
		{
			List<KeyValuePair<string, ValueType>> parameters = new List<KeyValuePair<string, ValueType>>();
			scanner.Require(TokenCode.LEFT_PARENTHESIS);
			if (scanner.Peek().Code != TokenCode.RIGHT_PARENTHESIS)
			{
				do
				{
					// parse parameter declarations
					ValueType type = ParseType();
					Token name = scanner.Require(TokenCode.IDENTIFIER);
					parameters.Add(
						new KeyValuePair<string, ValueType>(name.Value, type)
					);
				} while (scanner.NextIf(TokenCode.COMMA));
			}
			scanner.Require(TokenCode.RIGHT_PARENTHESIS);
			return parameters;
		}

		// Method parses an extern function declaration
		// input: none
		// return: ExternStatement
		public ExternStatement ParseExtern()
		{
			scanner.Next();
			// get return type
			ValueType returnType = ParseType();
			Token identifier = scanner.Require(TokenCode.IDENTIFIER);
			// parse parameters
			List<KeyValuePair<string, ValueType>> parameters = ParseParameters();


			return new ExternStatement(identifier, returnType, parameters);
		}

		// Method parses an if-else statement
		// input: none
		// return: parsed if-else statement
		private IfStatement ParseIfStatement()
		{
			// skip if keyword
			scanner.Next();
			// get condition and true-block
			scanner.Require(TokenCode.LEFT_PARENTHESIS);
			Expression condition = ParseExpression();
			scanner.Require(TokenCode.RIGHT_PARENTHESIS);
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
				scanner.Require(TokenCode.LEFT_PARENTHESIS);
				Expression expr = ParseExpression();
				scanner.Require(TokenCode.RIGHT_PARENTHESIS);
				return new WhileLoop(expr, ParseStatement());
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

		// Method parses a for loop
		// input: none
		// return: parsed for loop statement
		private ForLoop ParseForLoop()
		{
			// skip for keyword
			Token startToken = scanner.Next();
			scanner.Require(TokenCode.LEFT_PARENTHESIS);
			// regular while
			VariableDeclaration initialization = ParseVariableDeclaration();
			scanner.Require(TokenCode.SEMI_COLON);
			Expression condition = ParseExpression();
			scanner.Require(TokenCode.SEMI_COLON);
			Expression action = ParseExpression();
			scanner.Require(TokenCode.RIGHT_PARENTHESIS);
			Statement body = ParseStatement();

			return new ForLoop(initialization, condition, action, body);
		}

		// Method parses a switch case statement
		// input: none
		// return: parsed switch case statement
		private SwitchCase ParseSwitchCase()
		{
			Token switchKeyword = scanner.Require(TokenCode.SWITCH);
			Expression switchValue = ParseExpression();
			scanner.Require(TokenCode.OPEN_BRACE);

			SwitchCase switchCase = new SwitchCase(switchValue);
			// parse cases
			while (scanner.Peek().Code == TokenCode.CASE)
			{
				scanner.Require(TokenCode.CASE);
				Expression caseExpression = ParseExpression();
				scanner.Require(TokenCode.COLON);
				// read statements
				switchCase.AddCase(caseExpression, ParseBlock());
			}
			// check default
			if(scanner.Peek().Code == TokenCode.DEFAULT)
			{
				scanner.Next();
				scanner.Require(TokenCode.COLON);
				switchCase.AddDefault(ParseBlock());
			}

			scanner.Require(TokenCode.CLOSE_BRACE);

			return switchCase;
		}

		// Method parses a block of statements
		// input: whether to check brackets or not (default check)
		// return: Block node
		private Block ParseBlock(bool checkBraces=true)
		{
			// block: {<statements>}
			// check open brace
			if(checkBraces)
				scanner.Require(TokenCode.OPEN_BRACE);

			Block block = new Block(scanner.Peek().Line);
			// gather statements
			while(scanner.Peek().Code != TokenCode.CLOSE_BRACE && scanner.Peek().Code != TokenCode.EOF)
			{
				// add statement and add to symbol table if needed
				Statement newStatement = ParseStatement();
				block.AddStatement(newStatement);
			}

			// check close brace
			if(checkBraces)
				scanner.Require(TokenCode.CLOSE_BRACE);

			// handle sub-blocks
			foreach (Statement stmt in block.Children)
			{
				foreach (AST_Node node in stmt.Children)
				{
					if (!(node is Block))
						continue;
					else if (stmt is FunctionDeclaration)
					{
						// no parent, new outer table
						(node as Block).SymbolTable.OuterTable = block.SymbolTable;
						(node as Block).SymbolTable.OuterTable.ParentTable = block.SymbolTable.OuterTable;
					}
					else
					{
						// block is parent, same outer table
						(node as Block).SymbolTable.ParentTable = block.SymbolTable;
						(node as Block).OffsetAddresses(block.SymbolTable.VariableBytes());
					}
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
			// check function call
			else if (result is Variable && scanner.Peek().Code == TokenCode.LEFT_PARENTHESIS)
				return ParseFunctionCall(result as Variable);
			else
				return result;
		}

		FunctionCall ParseFunctionCall(Variable function)
		{
			scanner.Require(TokenCode.LEFT_PARENTHESIS);
			List<Expression> arguments = new List<Expression>();
			// parse arguments
			while(scanner.NextIf(TokenCode.COMMA) || !scanner.NextIf(TokenCode.RIGHT_PARENTHESIS))
			{
				arguments.Add(ParseExpression());
			}
			return new FunctionCall(function, arguments);
		}
	}
}
