using System.Text;

namespace DDW
{
	public class UncheckedExpression : PrimaryExpression
	{
	  private ExpressionNode expression;

	  public UncheckedExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public UncheckedExpression(ExpressionNode expression)
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
			sb.Append("unchecked(");
			expression.ToSource(sb);
			sb.Append(")");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUncheckedExpression(this, data);
        }

	}
}
