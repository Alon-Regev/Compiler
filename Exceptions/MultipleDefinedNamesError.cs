using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class MultipleDefinedNamesError : CompilerError
	{
		public MultipleDefinedNamesError(string identifier, int line)
			: base("MultipleDefinedNamesError", line, "Symbol \"" + identifier + "\" was defined more than once")
		{
		}
	}
}
