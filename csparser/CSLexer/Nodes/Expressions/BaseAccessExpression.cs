using System.Text;

namespace DDW
{
	public class BaseAccessExpression : PrimaryExpression
	{
        ExpressionNode expression;

	  public BaseAccessExpression(Token relatedToken) :base(relatedToken)
		{
		}

        public BaseAccessExpression(ExpressionNode expression) :base(expression.RelatedToken)
        {
            this.expression = expression;
        }

	  public ExpressionNode Expression
	  {
	    get
	    {
	      return expression;
	    }
	    set
	    {
	      expression = value;
	    }
	  }

	  public override void ToSource(StringBuilder sb)
		{
			sb.Append("base");
			if (expression != null)
			{
				sb.Append(".");
				expression.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBaseReferenceExpression(this, data);
        }

	}
}
