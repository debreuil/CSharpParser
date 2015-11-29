using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class ConstraintExpressionCollection : List<ConstraintExpressionNode>, ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
			bool isFirst = true;
            foreach (ConstraintExpressionNode ce in this)
            {
				if(isFirst) isFirst = false;
				else sb.Append(", ");
                ce.ToSource(sb);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (ConstraintExpressionNode node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        }

      #endregion
	}
}
