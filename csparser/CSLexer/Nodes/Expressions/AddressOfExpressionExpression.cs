using System.Text;

namespace DDW
{
	public class AddressOfExpression : PrimaryExpression
	{
	  private ExpressionNode expression;

	  public AddressOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public AddressOfExpression(ExpressionNode expression) : base(expression.RelatedToken)
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
			sb.Append("&");
            expression.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAddressOfExpression(this, data);
        }

	}
}
