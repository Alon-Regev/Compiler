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

		// functions compile assembly code to executable file
		// input: assembly file path or assembly code string
		// output: none
		public void FromString(string assembly)
		{
			File.WriteAllText("temp.asm", assembly);
			FromFile("temp.asm");
		}

		public void FromFile(string filePath)
		{
			Process.Start("nasm", "-fwin32 -o temp.obj " + filePath);
			Process.Start("gcc", "temp.obj -o " + _outputPath);
		}
	}
}
