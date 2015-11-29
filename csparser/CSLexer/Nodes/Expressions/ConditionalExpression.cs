using System.Text;

namespace DDW
{
	public class ConditionalExpression : ExpressionNode
	{
	  protected ExpressionNode left;
	  protected ExpressionNode right;
	  private ExpressionNode test;

	  public ConditionalExpression(Token relatedtoken) : base (relatedtoken)
		{
		}

        public ConditionalExpression(ExpressionNode test, ExpressionNode left, ExpressionNode right)
            : base(test.RelatedToken)
		{
			this.test = test;
			this.left = left;
			this.right = right;
		}

	  public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

	  public ExpressionNode Left
		{
			get { return left; }
			set { left = value; }
		}

	  public ExpressionNode Right
		{
			get { return right; }
			set { right = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			test.ToSource(sb);
			sb.Append(" ? ");
			left.ToSource(sb);
			sb.Append(" : ");
			right.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConditionalExpression(this, data);
        }
	}
}
