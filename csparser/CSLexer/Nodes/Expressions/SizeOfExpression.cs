using System.Text;

namespace DDW
{
	public class SizeOfExpression : PrimaryExpression
	{
	  private ExpressionNode expression;

	  public SizeOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public SizeOfExpression(ExpressionNode expression)
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
			sb.Append("sizeof(");
			expression.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitSizeOfExpression(this, data);
        }
	}
}
