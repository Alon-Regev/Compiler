using System;
using System.IO;
using System.Collections.Generic;

namespace Compiler
{
	class Program
	{
		private enum menuOptions
		{
			COMPILE_FILE=1,
			COMPILE_INPUT=2,
			HELP=3,
			QUIT=4,
		};

		struct Option
		{
			public string shortName;
			public string longName;
			public string description;
			public string parameterDescription;
			public Action toRun;
		}

		private static readonly List<Option> options = new List<Option>()
		{
			new Option{ shortName="h", longName="help", description="prints list of commands.", toRun=PrintHelp },
			new Option{ shortName="i", longName="input", description="gets input from console or parameter.", toRun=ProgramInput, parameterDescription="<?code>" },
			new Option{ shortName="f", longName="file", description="compiles the specified file.", toRun=CompileFile, parameterDescription="<path>" },
			new Option{ shortName="o", longName="output", description="specifies output file to save executable at.", toRun=null, parameterDescription="<path>" },
			new Option{ shortName="D", longName="detailed", description="detailed compilation output which shows all steps.", toRun=null },
		};

		private static ArgumentParser ap;

		static void Main(string[] args)
		{
			ap = new ArgumentParser(args);
			// check arguments
			foreach(Option option in options)	// merge both versions of options
				ap.JoinOptions(option.longName, option.shortName);

			// run specified option
			foreach (Option option in options)
			{
				// run if option is specified and not a flag
				if (option.toRun != null && ap.HasOption(option.longName))
				{
					option.toRun();
					return;
				}
			}
			// default run menu
			Menu();
		}

		// Method runs menu and receives action to do from user.
		// input: none
		// return: none
		private static void Menu()
		{
			Console.Write(
				"Options:\n" +
				"1. Compile File\n" +
				"2. Compile from Console input\n" +
				"3. Print help\n" +
				"4. Quit\n\n" + 
				"Enter option: "
			);
			// get option input
			int option;
			bool res = int.TryParse(Console.ReadLine(), out option);
			Console.WriteLine();
			if (!res)
			{
				Console.WriteLine("Invalid input");
				return;
			}
			switch (option)
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

				case (int)menuOptions.QUIT:
					break;

				default:
					Console.WriteLine("Invalid option");
					break;
			}
		}

		// Method prints list of options for help command.
		// input: none
		// return: none
		private static void PrintHelp()
		{
			// print usage
			Console.Write("\nUsage: Compiler");
			foreach(Option option in options)
			{
				Console.Write(" [-{0} {1}] ", option.shortName, option.parameterDescription ?? "\b");
			}
			// print options
			Console.WriteLine("\n\nOptions:");
			foreach(Option option in options)
			{
				Console.WriteLine(
					(("\t-" + option.shortName + " | --" + option.longName).PadRight(16) + option.parameterDescription).PadRight(24)
					+ ":  " + option.description
				);
			}
			// additional info
			Console.WriteLine("\nNo parameters: Main Menu");
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

		// Method recevies input for a program and compiles it.
		// input: argument parser
		// return: none
		private static void ProgramInput()
		{
			string program = "";
			// check parameters
			List<string> parameters = ap?.GetParameters("input");
			if (parameters?.Count > 0)	// input from parameters
				program = string.Join("", parameters);
			else	// input from console
				program = ConsoleProgramInput();

			Compile(program);
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
		// ap: argument parser
		// return: none
		private static void CompileFile()
		{
			// get path
			List<string> parameters = ap.GetParameters("file");
			if(parameters.Count == 0)
			{   // no path specified
				Console.WriteLine("Error: No file specified");
				return;
			}
			CompileFile(parameters[0]);
		}

		// Method compiles a code file.
		// path: program's path
		// return: none
		private static void CompileFile(string path)
		{
			// check if file exists
			if (!FileExists(path))
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
				if (ap.HasOption("detailed"))
				{
					// print each step
					Console.WriteLine("Lexer Tokens: ");
					Scanner.PrintTokens(new Scanner(program).Tokenize());

					Console.WriteLine("\nParser AST: ");
					AST_Node AST = new Parser(program).Parse();
					Console.WriteLine(AST);

					Console.Write("\nSemantic analysis result: ");
					new SemanticAnalyzer(AST).Analyze();
					Console.WriteLine("Good!");
					Console.WriteLine(AST);

					Console.WriteLine("\nGenerated Assembly: ");
					string assembly = new CodeGenerator(program).GenerateAssembly();
					Console.WriteLine(assembly);

					Console.WriteLine("\nTurning Assembly to executable...");
					ExecutableCreator ec = new ExecutableCreator("temp.exe");
					ec.FromString(assembly);
					Console.WriteLine("\nRunning Program: ");
					ec.Run();
				}
				else
				{
					// compiler regularly
					Parser parser = new Parser(program);
					Console.WriteLine(parser.Parse());
				}
			}
			catch(CompilerError e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
