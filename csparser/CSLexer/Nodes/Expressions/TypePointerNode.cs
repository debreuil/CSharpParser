using System;
using System.Text;

namespace DDW
{
    public class TypePointerNode : TypeNode, IEquatable<TypePointerNode>
	{
        private readonly ExpressionNode expression;

      public TypePointerNode(Token relatedToken)
            : base(relatedToken)
        {
        }
        public TypePointerNode(ExpressionNode expression): base(expression.RelatedToken)
		{
            this.expression = expression;
		}

      public ExpressionNode Expression
      {
        get
        {
          return expression;
        }
      }

      #region IEquatable<TypePointerNode> Members

      public bool Equals(TypePointerNode other)
        {
            bool ret = false;

            if (this == other)
            {
                ret = true;
            }
            else
            {
                if (other != null)
                {
                    if (expression == null && other.expression == null
                        || expression != null && expression.Equals(other.expression))
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

      #endregion

      public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj is TypePointerNode)
            {
                ret = Equals(obj as TypePointerNode);
            }
            else
            {
                ret = base.Equals(obj);
            }

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public override void ToSource(StringBuilder sb)
		{
            expression.ToSource(sb);

             sb.Append("*");

             if (IsNullableType)
             {
                 sb.Append("?");
             }

             if (RankSpecifiers.Count > 0)
             {
                 foreach (int val in RankSpecifiers)
                 {
                     sb.Append("[");
                     for (int i = 0; i < val; i++)
                     {
                         sb.Append(",");
                     }
                     sb.Append("]");
                 }
             }
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypePointerReference(this, data);
        }
	}
}
