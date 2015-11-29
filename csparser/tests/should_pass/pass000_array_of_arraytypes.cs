using System;

class test
{
	static int CheckParam(Type[] types)
	{
		return types.Length - 2;
	}

	static int Main(String[] args)
	{
		return CheckParam(
			new Type[2]
			{
				typeof(String[]),
				typeof(int[])
			}
		);
	}
}
