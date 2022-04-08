using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class CompilerError : Exception
	{
		protected CompilerError(string name, int line, string info)
			: base("Error at line " + line + "\n" + name + ": " + info)
		{
		}
	}
}
