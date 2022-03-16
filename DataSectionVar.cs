using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum DataSize
	{
		BYTE = 'b',
		WORD = 'w',
		DWORD = 'd',
		QWORD = 'q'
	}

	class DataSectionVar
	{
		public string Name;
		public string Value;
		public char Size;

		private static int floatConstantIndex = 0;

		// Constructor
		// input: variable's name
		//		  variable's size in bytes
		//		  variable's initial value
		public DataSectionVar(string name, DataSize size, string value)
		{
			Name = name;
			Value = value;
			Size = (char)size;
		}

		// Method return variable as assembly code for it's declaration
		// input: none
		// return: none
		public string ToAssembly()
		{
			return String.Format("{0}: d{1} {2}\n", Name, Size, Value);
		}

		// Method creates a data section variable for a float with incrementing index
		// input: value
		// return: none
		public static DataSectionVar FloatConstant(float value)
		{
			return new DataSectionVar("__f" + floatConstantIndex++, DataSize.DWORD, value.ToString());
		}

		// Method creates a variable for a const string
		public static DataSectionVar StringConstant(string name, string value, bool newline = true)
		{
			return new DataSectionVar(name, DataSize.BYTE,
				String.Format("\"{0}\"{1}, 0", value, newline ? ", 0xa" : "")
			);
		}
	}
}
