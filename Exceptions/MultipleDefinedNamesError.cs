using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class MultipleDefinedNamesError : CompilerError
	{
		public MultipleDefinedNamesError(VariableDeclaration decl)
			: base("MultipleDefinedNamesError", decl.Line, "Symbol \"" + decl.Identifier + "\" was defined more than once")
		{
		}
	}
}
