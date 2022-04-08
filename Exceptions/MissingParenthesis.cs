using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class MissingParenthesis : CompilerError
	{
		public MissingParenthesis(Token openingParenthesis)
			: base("MissingParenthesis", openingParenthesis.Line, "opening parenthesis at position " + openingParenthesis.Pos + " without matching closing parenthesis")
		{

		}
	}
}
