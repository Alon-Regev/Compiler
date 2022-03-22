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
		// input: none
		// return: expression tree
		private Expression ParseExpression()
		{
			// Term | Term [+-] Term
			Expression node = ParseTerm();

			TokenCode nextToken = scanner.Peek().Code;
			while(nextToken == TokenCode.ADD_OP || nextToken == TokenCode.SUB_OP)
			{
				Token op = scanner.Next();
				Expression nextTerm = ParseTerm();
				// add term to node
				node = new BinaryOperator(nextToken, node, nextTerm);
				// check next code
				nextToken = scanner.Peek().Code;
			}

			return node;
		}

		// Method parses a mathematical expression, defined as a product of factors.
		// input: none
		// return: term tree
		private Expression ParseTerm()
		{
			// Factor | Factor [*/] Factor
			Expression node = ParseFactor();

			TokenCode nextToken = scanner.Peek().Code;
			while (nextToken == TokenCode.MUL_OP || nextToken == TokenCode.DIV_OP)
			{
				Token op = scanner.Next();
				Expression nextFactor = ParseFactor();
				// add term to node
				node = new BinaryOperator(nextToken, node, nextFactor);
				// check next code
				nextToken = scanner.Peek().Code;
			}

			return node;
		}

		// Method parses a mathematical factor, defined as an integer or as an expression in parentheses.
		// input: none
		// return: factor tree
		private Expression ParseFactor()
		{
			// int | (expression)
			Token token = scanner.Next();
			// check integer
			if (token.Code == TokenCode.INTEGER)
			{
				return new Primitive<int>(token);
			}
			// check float
			else if(token.Code == TokenCode.DECIMAL)
			{
				return new Primitive<float>(token);
			}
			// check expression
			else if(token.Code == TokenCode.LEFT_PARENTHESIS)
			{
				Expression node = ParseExpression();
				// check closing parenthesis
				Token closingParenthesis = scanner.Next();
				if(closingParenthesis.Code != TokenCode.RIGHT_PARENTHESIS)
				{
					throw new MissingParenthesis(token);
				}

				return node;
			}
			// castings
			else if(token.Code == TokenCode.INT_CAST)
			{
				Expression toCast = ParseTerm();
				return new Cast(toCast, TypeCode.INT);
			}
			else if (token.Code == TokenCode.FLOAT_CAST)
			{
				Expression toCast = ParseTerm();
				return new Cast(toCast, TypeCode.FLOAT);
			}
			else
			{
				throw new UnexpectedToken("expression", token);
			}
		}
	}
}
