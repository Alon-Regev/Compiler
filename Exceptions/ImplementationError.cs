using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ImplementationError : Exception
	{
		public ImplementationError(string s)
			: base(s)
		{
		}
	}
}
