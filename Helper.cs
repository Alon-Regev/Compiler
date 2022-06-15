using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Helper
	{
		// Method changes a string to a constant width.
		// baseString: string to change
		// charLimit: max amount of characters to include
		// width: constant number of chars in the result. Padding with spaces.
		// return: formatted string
		public static string StringFormat(string baseString, int width, int charLimit = -1)
		{
			if (charLimit == -1)
				charLimit = width;
			string result = baseString;
			// limit chars
			if (baseString.Length > charLimit - 3)
				result = baseString.Substring(0, charLimit - 3) + "...";

			// return padded string
			return result.PadRight(width);
		}
	}
}
