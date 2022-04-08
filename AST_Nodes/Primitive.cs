using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Primitive<T> : Expression, IPrimitive
	{
		public T Value { get; private set; }

		// constructors

		// line: line number of the node
		// value: value to assign to the primitive
		public Primitive(int line, T value) : base(line)
		{
			Value = value;
		}

		// line: line number of the node
		// value: value to assign as string
		public Primitive(int line, string value) : base(line)
		{
			FromString(value);
		}

		// token: Token to generate primitive from
		public Primitive(Token token) : base(token.Line)
		{
			FromString(token.Value);
		}

		// Helper to fill the primitive from a string
		// value: the value of the primitive from a string.
		// return: none
		private void FromString(string str)
		{
			try
			{
				// try to convert
				Value = (T)Convert.ChangeType(str, typeof(T));
			}
			catch
			{
				// on error throw a value exception
				throw new ValueError(Line, str, typeof(T).Name);
			}
		}

		// ToString override prints primitive value
		public override string ToString(int indent)
		{
			string baseline = "Primitive " + Value.GetType().Name + ": " + Value;
			return baseline + base.ToString(indent);
		}
	}
}
