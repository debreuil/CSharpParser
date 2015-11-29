using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
	public class TokenCollection : LinkedList<Token>
	{
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

            LinkedListNode<Token> tk = First;
            int i = 0;

            while (tk != null)
            {
                sb.Append(i + ": " + tk.Value.ID + "\n");
                i++;
                tk = tk.Next;
            }
			return sb.ToString();
		}
	}
}
