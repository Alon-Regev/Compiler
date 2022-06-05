using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ExternStatement : Statement
	{
		public string Identifier { private set; get; }
		public List<KeyValuePair<string, TypeCode>> Parameters { private set; get; }
		public TokenCode ReturnType { private set; get; }

		public ExternStatement(Token identifier, Token returnType, List<KeyValuePair<string, TypeCode>> parameters) : base(identifier.Line)
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
