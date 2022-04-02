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
	}
}
