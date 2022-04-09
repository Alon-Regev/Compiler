using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnknownNameError : CompilerError
	{
		public UnknownNameError(string identifier, int line)
			: base("UnkownName", line, "Use of undefined symbol \"" + identifier + "\"")
		{
		}

		public UnknownNameError(Variable variable)
			: base("UnkownName", variable.Line, "Use of undefined symbol \"" + variable.Identifier + "\"")
		{
		}
	}
}
