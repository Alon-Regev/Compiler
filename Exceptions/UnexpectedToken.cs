using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnexpectedToken : CompilerError
	{
		public UnexpectedToken(string expected, Token received)
			: base("UnexpectedToken", received.Line, "Expected " + expected + ", instead got " + received.Value + " at position " + received.Pos)
		{
		}
	}
}
