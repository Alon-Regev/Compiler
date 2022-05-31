using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ExternStatement : Statement
	{
		public string Identifier { private set; get; }

		public ExternStatement(Token identifier) : base(identifier.Line)
		{
			Identifier = identifier.Value;
		}

		public override string ToString(int indent)
		{
			return "Extern Statement" + base.ToString(indent);
		}
	}
}
