using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Cast : Expression
	{
		public TypeCode FromType { get; private set; }

		// Constructor
		// from: which Expression to cast
		// to: what type to cast into
		public Cast(Expression from, TypeCode toType) : base(from.Line)
		{
			FromType = from.Type;
			Type = toType;

			AddChild(from);
		}

		// ToString override prints cast
		public override string ToString(int indent)
		{
			string baseLine = "Cast " + FromType + " -> " + Type;
			return baseLine = base.ToString();
		}
	}
}
