using System;

namespace Compiler
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Enter program: ");
			string program = Console.ReadLine();

			Token[] tokens = Scanner.Tokenize(program);
			Console.WriteLine("\nTokens:\n");
			Scanner.PrintTokens(tokens);
		}
	}
}
