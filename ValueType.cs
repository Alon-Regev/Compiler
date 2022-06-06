﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	enum TypeCode
	{
		UNKNOWN,
		INT,
		FLOAT,
		BOOL,
		VOID,
	}

	class ValueType
	{
		public TypeCode TypeCode;
		public int Pointer = 0;
		public int Line = 0;

		public ValueType(Token keyword, int pointer)
		{
			TypeCode = ToTypeCode(keyword);
			Pointer = pointer;
			Line = keyword.Line;
		}
		public ValueType(TypeCode type, int pointer, int line)
		{
			TypeCode = type;
			Pointer = pointer;
			Line = line;
		}
		public ValueType(TypeCode type)
		{
			TypeCode = type;
			Pointer = 0;
		}

		// converts type token code to type code
		public static TypeCode ToTypeCode(TokenCode token, int line)
		{
			return token switch
			{
				TokenCode.INT_KEYWORD => TypeCode.INT,
				TokenCode.FLOAT_KEYWORD => TypeCode.FLOAT,
				TokenCode.BOOL_KEYWORD => TypeCode.BOOL,
				TokenCode.VOID => TypeCode.VOID,
				_ => throw new UnexpectedToken("type", token, line)
			};
		}
		public static TypeCode ToTypeCode(Token keyword)
		{
			return ToTypeCode(keyword.Code, keyword.Line);
		}

		// creates an unknown value types instance
		public static ValueType Unknown(int line)
		{
			return new ValueType(TypeCode.UNKNOWN, 0, line);
		}

		// sets attributes on type based on another type
		public void Set(TypeCode other)
		{
			TypeCode = other;
		}
		public void Set(ValueType other)
		{
			TypeCode = other.TypeCode;
			Pointer = other.Pointer;
		}

		// operator overloading
		public static bool operator ==(ValueType a, ValueType b)
		{
			return a.TypeCode == b.TypeCode && a.Pointer == b.Pointer;
		}
		public static bool operator !=(ValueType a, ValueType b)
		{
			return !(a == b);
		}
		public static bool operator ==(ValueType a, TypeCode b)
		{
			return a.TypeCode == b && a.Pointer == 0;
		}
		public static bool operator !=(ValueType a, TypeCode b)
		{
			return !(a == b);
		}
	}
}
