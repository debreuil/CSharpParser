using System.Text;

namespace DDW
{
	public class ParenthesizedExpression : PrimaryExpression
	{
	  ExpressionNode expression;

	  public ParenthesizedExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public ParenthesizedExpression(ExpressionNode expression)
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
			sb.Append("(");
			expression.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitParenthesizedExpression(this, data);
        }
	}
}
