using System;

class X
{
	static void Main ()
	{
		bool? a = true;
		// According to ECMA-334 13.7.2 the cast from bool? to bool
		// must be explicit. But I am not completely sure about the
		// following sentence of 14.13: when there is no implicit
		// conversion to bool "the operator true defined by the type
		// of b is invoked to produce a bool value."
		// The next line does not compile on .NET without the explicit
		// cast to bool.
		int? b = (bool)a ? 3 : 4;
		Console.WriteLine (b);
	}
}
