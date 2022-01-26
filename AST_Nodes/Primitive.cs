using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Primitive<T> : Expression
	{
		public T Value { get; private set; }

		// constructor
		// line: line number of the node
		// value: value to assign to the primitive
		public Primitive(int line, T value) : base(line)
		{
			Value = value;
		}

		// ToString override prints primitive value
		public override string ToString(int indent)
		{
			string baseline = "Primitive " + Value.GetType().Name + ": " + Value;
			return baseline + base.ToString(indent);
		}
	}
}
