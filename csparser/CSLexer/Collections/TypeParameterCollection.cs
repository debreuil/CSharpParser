using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class TypeParameterCollection : List<TypeParameterNode>, ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
			bool isFirst = true;
			foreach(TypeParameterNode tp in this)
			{
				if(isFirst) isFirst = false;
				else sb.Append(", ");
                tp.ToSource(sb);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (TypeParameterNode tp in this)
            {
                tp.AcceptVisitor(visitor, data);
            }

            return null;
        }

      #endregion
	}
}
