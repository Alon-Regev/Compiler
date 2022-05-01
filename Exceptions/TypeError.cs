using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class TypeError : CompilerError
	{
		public TypeError(BinaryOperator op)
			: base("TypeError", op.Line, "Operator " + op.Operator + " on types " + op.Operand(0).Type + " and " + op.Operand(1).Type)
		{
		}

		public TypeError(UnaryOperator op)
			: base("TypeError", op.Line, "Operator " + op.Operator + " on type " + op.Operand().Type)
		{
		}

		public TypeError(Cast cast)
			: base("TypeError", cast.Line, "Casting " + cast.FromType + " to " + cast.Type)
		{
		}

		public TypeError(TernaryOperator op, string msg)
			: base("TypeError", op.Line, msg)
		{
		}

		public TypeError(Statement stmt, string msg)
			: base("TypeError", stmt.Line, msg)
		{
		}
	}
}
