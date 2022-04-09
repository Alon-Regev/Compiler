using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class AssignmentError : CompilerError
	{
		public AssignmentError(int line) 
			: base("AssignmentError", line, "assigning to a non-variable value")
		{
		}
	}
}
