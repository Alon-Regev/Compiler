using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Parser
	{
		Scanner scanner;

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
			return ParseExpression();
		}

		// Method parses a mathematical expression, defined as a sum of terms.
		// input:   order - expression's max operator order
		//			default: includes all operators
		// return:	expression tree
		private Expression ParseExpression(int order = 7)
		{
			// subexp(0) := factor | factor unary_op
			if (order == 0)
			{
				Expression factor = ParseFactor();
				if (IsUnaryPostfixOperator(scanner.Peek()))
					return new UnaryOperator(scanner.Next().Code, factor, false);
				else
					return factor;
			}

			// subexpression(n) is defined as
			// subexp(n-1) | subexp(n-1) op(n) subexp(n-1)

			Expression node = ParseExpression(order - 1);
			// while peeked token's order is n
			while(GetOperatorOrder(scanner.Peek()) == order)
			{
				Token op = scanner.Next();
				Expression nextSubexp = ParseExpression(order - 1);
				// add binary operator node
				node = new BinaryOperator(op.Code, node, nextSubexp);
			}

			return node;
		}

		// Method returns operator's order (in order of operations)
		// higher value means it's computed later
		// input: operator's token
		// return: operator's order value
		private int GetOperatorOrder(Token t)
		{
			switch(t.Code)
			{
				// bitwise
				case TokenCode.BIT_OR_OP:
					return 7;
				case TokenCode.BIT_XOR_OP:
					return 6;
				case TokenCode.BIT_AND_OP:
					return 5;
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
				case TokenCode.EOF:
					return -1;
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
			// Factor := <primitive> | (<expression>) | <cast><factor> | <unary_op><factor> | <factor><unary_op>
			Token token = scanner.Next();
			AST_Node result;
			
			switch(token.Code)
			{
				// --- Primitives
				case TokenCode.INTEGER:
					return new Primitive<int>(token);
				case TokenCode.DECIMAL:
					return new Primitive<float>(token);

				// --- Parentheses Expression
				case TokenCode.LEFT_PARENTHESIS:
					Expression node = ParseExpression();
					// check closing parenthesis
					if (scanner.Next().Code != TokenCode.RIGHT_PARENTHESIS)
						throw new MissingParenthesis(token);
					return node;

				// --- Castings
				case TokenCode.INT_CAST:
					return new Cast(ParseFactor(), TypeCode.INT);
				case TokenCode.FLOAT_CAST:
					return new Cast(ParseFactor(), TypeCode.FLOAT);

				// --- Unary Prefix Operators
				case TokenCode.BIT_NOT_OP:
					return new UnaryOperator(token.Code, ParseFactor(), true);
				case TokenCode.SUB_OP:  // negation
					return new UnaryOperator(token.Code, ParseFactor(), true);

				// --- Unexpected
				default:
					throw new UnexpectedToken("expression", token);
			}
		}
	}
}
