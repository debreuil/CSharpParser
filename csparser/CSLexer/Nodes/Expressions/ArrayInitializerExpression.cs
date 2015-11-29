using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ArrayInitializerExpression : ExpressionNode
	{
	  private ExpressionList expressions = new ExpressionList();

	  public ArrayInitializerExpression(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public ExpressionList Expressions
		{
			get { return expressions; }
			set { expressions = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("{");
			expressions.ToSource(sb);
			sb.Append("}");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArrayInitializerExpression(this, data);
        }

	}
}
