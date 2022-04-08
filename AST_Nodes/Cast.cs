using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Cast : Expression
	{
		public TypeCode FromType { get; set; }

		// Constructor
		// from: which Expression to cast
		// to: what type to cast into
		public Cast(Expression from, TypeCode toType) : base(from.Line)
		{
			FromType = from.Type;
			Type = toType;

			AddChild(from);
		}
		
		// Getter for single child
		public Expression Child()
		{
			return (Expression)GetChild(0);
		}

		// ToString override prints cast
		public override string ToString(int indent)
		{
			string baseLine = "Cast " + FromType + " -> " + Type;
			return baseLine + base.ToString(indent);
		}
	}
}
