using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class MultipleDefinedNamesError : CompilerError
	{
		public MultipleDefinedNamesError(Token symbolToken)
			: base("MultipleDefinedNamesError", symbolToken.Line, "Symbol \"" + symbolToken.Value + "\" was defined more than once")
		{
		}
	}
}
