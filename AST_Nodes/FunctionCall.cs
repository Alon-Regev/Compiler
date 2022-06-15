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

		// returns function variable node
		public Variable Function()
		{
			return GetChild(0) as Variable;
		}

		// returns number of arguments in function call
		public int ArgumentCount()
		{
			return Children.Count - 1;
		}

		// gets one of the function call arguments by index
		public Expression GetArgument(int index)
		{
			return GetChild(index + 1) as Expression;
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Function call (" + Function().Identifier + ")";
			return baseline + base.ToString(indent);
		}
	}
}
