using System;

namespace Compiler
{
	class Program
	{
		private enum menuOptions
		{
			COMPILE_FILE=1,
			COMPILE_INPUT=2,
			HELP=3,
		};

		static void Main(string[] args)
		{
			if(args.Length == 0)
			{
				// main menu
				Console.Write(
					"Options:\n" +
					"1. Compile File\n" +
					"2. Compile from Console input\n" +
					"3. Print help\n\n" +
					"Enter option: "
				);
				// get option input
				int option = 0;
				bool res = int.TryParse(Console.ReadLine(), out option);
				Console.WriteLine();
				if (!res)
				{
					Console.WriteLine("Invalid input");
					return;
				}
				switch(option)
				{
					case (int)menuOptions.COMPILE_FILE:
						break;

					case (int)menuOptions.COMPILE_INPUT:
						Compile(ConsoleProgramInput());
						break;

					case (int)menuOptions.HELP:
						PrintHelp();
						break;

					default:
						Console.WriteLine("Invalid option");
						break;
				}
				
			}
			else if(args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
			{
				// print help message
				PrintHelp();
			}
			else if(args.Length >= 1 && (args[0] == "-i" || args[0] == "--input"))
			{
				string program = "";

				if(args.Length == 1)	// input from console
					program = ConsoleProgramInput();
				else	// input from command line
					program = args[1];

				Compile(program);
			}
			else
			{
				// compile file
			}
		}

		// Method prints list of options for help command.
		// input: none
		// return: none
		private static void PrintHelp()
		{
			Console.WriteLine(
				"List of commands:\n" +
				"--help / -h  :	prints this list of commands\n" +
				"--input / -i :	receives input from the console to compile\n" +
				"-i <code>    :	compiles code from the command line\n" +
				"<file path>  :	compiles the input file\n"
			);
		}

		// Method gets program input from console
		// input: none
		// return: program string
		private static string ConsoleProgramInput()
		{
			Console.WriteLine("Enter program: ");
			string program = Console.ReadLine();
			return program;
		}

		// Method compiles string.
		// program: string code to compile
		// return: none
		private static void Compile(string program)
		{
			Token[] tokens = Scanner.Tokenize(program);
			Console.WriteLine("\nTokens:\n");
			Scanner.PrintTokens(tokens);
		}
	}
}
