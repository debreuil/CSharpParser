using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
	public class ParseStateCollection : List<ParseState>
	{
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < Count; i++)
			{
				sb.Append(i + "\t" + this[i] + "\n");
			}
			return sb.ToString();
		}
	}
}
