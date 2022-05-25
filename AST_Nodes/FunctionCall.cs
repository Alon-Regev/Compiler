using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class FunctionCall : Expression
	{
		// constructor
		// variable: function variable
		public FunctionCall(Variable function, List<Expression> arguments) : base(function.Line)
		{
			AddChild(function);
			// add arguments
			foreach(Expression arg in arguments)
				AddChild(arg);
		}

		public Variable Function()
		{
			return GetChild(0) as Variable;
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Function call (" + Function().Identifier + ")";
			return baseline + base.ToString(indent);
		}
	}
}
