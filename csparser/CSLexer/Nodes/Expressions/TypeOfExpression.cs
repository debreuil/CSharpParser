using System.Text;

namespace DDW
{
	public class TypeOfExpression : PrimaryExpression
	{
	  private ExpressionNode expression;

	  public TypeOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public TypeOfExpression(ExpressionNode expression)
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
			sb.Append("typeof(");
			expression.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypeOfExpression(this, data);
        }
	}
}
