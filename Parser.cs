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

		public AST_Node ParseExpression()
		{
			// Term | Term [+-] Term
			AST_Node node = ParseTerm();

			TokenCode nextToken = scanner.Peek().Code;
			while(nextToken == TokenCode.ADD_OP || nextToken == TokenCode.SUB_OP)
			{
				Token op = scanner.Next();
				AST_Node nextTerm = ParseTerm();
				// add term to node
				node = new BinaryOperator(node.Line, nextToken, node, nextTerm);
				// check next code
				nextToken = scanner.Peek().Code;
			}

			return node;
		}

		public AST_Node ParseTerm()
		{
			// Factor | Factor [*/] Factor
			AST_Node node = ParseFactor();

			TokenCode nextToken = scanner.Peek().Code;
			while (nextToken == TokenCode.MUL_OP || nextToken == TokenCode.DIV_OP)
			{
				Token op = scanner.Next();
				AST_Node nextFactor = ParseFactor();
				// add term to node
				node = new BinaryOperator(node.Line, nextToken, node, nextFactor);
				// check next code
				nextToken = scanner.Peek().Code;
			}

			return node;
		}

		public AST_Node ParseFactor()
		{
			// int | (expression)
			Token t = scanner.Next();
			if (t.Code != TokenCode.NUMBER)
				throw new Exception("Invalid syntax");
			else
				return new Primitive<int>(t.Line, int.Parse(t.Value));
		}
	}
}
