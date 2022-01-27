using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ValueError : CompilerError
	{
		public ValueError(int line, string value, string type) 
			: base("ValueError", line, "can't convert \"" + value + "\" to type " + type)
		{
		}
	}
}
