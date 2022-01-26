using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Primitive<T> : Expression
	{
		public T Value { get; private set; }

		// constructor
		// value: value to assign to the primitive
		public Primitive(T value)
		{
			Value = value;
		}
	}
}
