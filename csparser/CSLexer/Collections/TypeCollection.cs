using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class TypeCollection : List<IType>, ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
			bool isFirst = true;
            foreach (ISourceCode t in this)
            {
				if(isFirst) isFirst = false;
				else sb.Append(", ");
                t.ToSource(sb);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (IType node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        }

      #endregion
	}
}
