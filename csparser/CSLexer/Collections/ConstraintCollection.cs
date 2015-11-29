using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class ConstraintCollection : List<Constraint>, ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
            foreach (Constraint constraint in this)
            {
				BaseNode.NewLine(sb);
				constraint.ToSource(sb);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (Constraint node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        }

      #endregion
	}
}
