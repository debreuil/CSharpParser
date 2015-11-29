using System.Text;

namespace DDW
{
	public class PPIfNode : PPNode
	{
	  private ExpressionNode expression;

	  public PPIfNode(Token relatedToken)
            : base(relatedToken)
		{
		}
        public PPIfNode(ExpressionNode expression)
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
            sb.Append("#if ");
            expression.ToSource(sb);
            NewLine(sb);
        }
	}
}
