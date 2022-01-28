using System;
using System.IO;

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
			Console.WriteLine(new ArgumentParser(args));
			return;

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
						Console.Write("Enter program's path: ");
						string path = Console.ReadLine();
						CompileFile(path);
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
				CompileFile(args[0]);
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
			Console.WriteLine("Enter program. Empty line to end.");
			string program = "";
			string input = Console.ReadLine();
			// repeat until empty line
			while (input != "")
			{
				program += input + "\r\n";
				input = Console.ReadLine();
			}
			
			return program;
		}


		// Method checks if a file at a certain path exists.
		// path: file path to check
		// return: whether or not the path is valid
		private static bool FileExists(string path)
		{
			return File.Exists(path);
		}

		// Method reads content of file.
		// path: file's path to read
		// return: file's content
		private static string ReadFile(string path)
		{
			if (FileExists(path))
				return File.ReadAllText(path);
			else
				return "";
		}

		// Method compiles a code file.
		// path: program's path
		// return: none
		private static void CompileFile(string path)
		{
			// check if file exists
			if(!FileExists(path))
			{
				Console.WriteLine("Error: File not found");
				return;
			}

			string program = ReadFile(path);
			Compile(program);
		}

		// Method compiles string.
		// program: string code to compile
		// return: none
		private static void Compile(string program)
		{
			try
			{
				Parser parser = new Parser(program);
				Console.WriteLine(parser.Parse());
			}
			catch(CompilerError e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
