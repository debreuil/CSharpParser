using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class PartialCollection : List<IPartial>, ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
			bool isFirst = true;
            foreach (ISourceCode p in this)
            {
				if(isFirst) isFirst = false;
				else sb.Append(", ");
                p.ToSource(sb);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (IPartial node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        }

      #endregion
	}
}
