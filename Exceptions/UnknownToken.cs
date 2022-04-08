using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnknownToken : CompilerError
	{
		public UnknownToken(Token t)
			: base("UnknownToken", t.Line, '"' + t.Value + "\" at position " + t.Pos)
		{
		}
	}
}
