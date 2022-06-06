using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ExternStatement : Statement
	{
		public string Identifier { private set; get; }
		public List<KeyValuePair<string, ValueType>> Parameters { private set; get; }
		public ValueType ReturnType { private set; get; }

		public ExternStatement(Token identifier, ValueType returnType, List<KeyValuePair<string, ValueType>> parameters) : base(identifier.Line)
		{
			Identifier = identifier.Value;
			ReturnType = returnType.Code;
			Parameters = parameters;
		}

		public override string ToString(int indent)
		{
			return "Extern Statement" + base.ToString(indent);
		}
	}
}
