using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Compiler
{
	class ExecutableCreator
	{
		private string _outputPath;

		// Constructor
		// outputFile: resulting executable
		public ExecutableCreator(string outputPath)
		{
			_outputPath = outputPath;
		}

		// Methods compile assembly code to executable file
		// input: assembly file path or assembly code string
		// output: none
		public void FromString(string assembly)
		{
			File.WriteAllText("temp.asm", assembly);
			FromFile("temp.asm");
		}

		public void FromFile(string filePath)
		{
			Process nasm = Process.Start("nasm", "-fwin32 -o temp.obj " + filePath);
			nasm.WaitForExit();
			if (nasm.ExitCode != 0)
				throw new ImplementationError("Error in assembly!");
			Process gcc = Process.Start("gcc", "temp.obj -o " + _outputPath);
			gcc.WaitForExit();
			if (gcc.ExitCode != 0)
				throw new ImplementationError("Error in linker!");
		}

		// Method runs resulting executable
		// input: none
		// return: none
		public void Run()
		{
			Process program = Process.Start(_outputPath);
			program.WaitForExit();
			if (program.ExitCode != 0)
				throw new ImplementationError("Error in executable!");
		}
	}
}
