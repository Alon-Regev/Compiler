using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnknownNameError : CompilerError
	{
		public UnknownNameError(VariableDeclaration decl)
			: base("UnkownName", decl.Line, "Use of undefined symbol \"" + decl.Identifier + "\"")
		{
		}
	}
}
