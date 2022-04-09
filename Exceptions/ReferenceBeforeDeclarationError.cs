using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ReferenceBeforeDeclarationError : CompilerError
	{
		public ReferenceBeforeDeclarationError(Variable variable)
			: base("ReferenceBeforeDeclaration", variable.Line, "Use of variable \"" + variable.Identifier + "\" before it's declaration")
		{
		}
	}
}
