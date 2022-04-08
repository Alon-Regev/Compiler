using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnknownNameError : CompilerError
	{
		public UnknownNameError(Token symbolToken)
			: base("UnkownName", symbolToken.Line, "Use of undefined symbol \"" + symbolToken.Value + "\"")
		{
		}
	}
}
