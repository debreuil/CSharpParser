using System.Text;

namespace DDW
{
	public class PostDecrementExpression : PrimaryExpression
	{
	  private ExpressionNode expression;

	  public PostDecrementExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public PostDecrementExpression(ExpressionNode expression)
            : base(expression.RelatedToken)
		{
			this.expression = expression;
		}

	  public ExpressionNode Expression
		{
			get { return expression; }
			set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			expression.ToSource(sb);
			sb.Append("--");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPostDecrementExpression(this, data);
        }

	}
}
