﻿using System;
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
		};

		struct Option
		{
			public string shortName;
			public string longName;
			public string description;
			public Action<ArgumentParser> toRun;
		}

		private static readonly List<Option> options = new List<Option>()
		{
			new Option{ shortName="h", longName="help", description="prints list of commands.", toRun=PrintHelp },
			new Option{ shortName="i", longName="input", description="gets input from console or parameter.", toRun=ProgramInput },
			new Option{ shortName="f", longName="file", description="compiles the specified file.", toRun=CompileFile },
			new Option{ shortName="o", longName="output", description="specifies output file to save executable at", toRun=null },
		};


		static void Main(string[] args)
		{
			ArgumentParser arguments = new ArgumentParser(args);
			// check arguments
			foreach(Option option in options)	// merge both versions of options
				arguments.JoinOptions(option.longName, option.shortName);

			// run specified option
			foreach (Option option in options)
			{
				// run if option is specified and not a flag
				if (option.toRun != null && arguments.HasOption(option.longName))
				{
					option.toRun(arguments);
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

				default:
					Console.WriteLine("Invalid option");
					break;
			}
		}

		// Method prints list of options for help command.
		// input: none
		// return: none
		private static void PrintHelp(ArgumentParser ap = null)
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

		// Method recevies input for a program and compiles it.
		// input: argument parser
		// return: none
		private static void ProgramInput(ArgumentParser ap = null)
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
		private static void CompileFile(ArgumentParser ap)
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
