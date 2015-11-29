using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ElementAccessExpression : PrimaryExpression
	{
	  private ExpressionList expressions;
	  private ExpressionNode leftSide;

	  public ElementAccessExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public ElementAccessExpression(ExpressionNode leftSide)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
		}
		public ElementAccessExpression(ExpressionNode leftSide, ExpressionList expressions)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
			this.expressions = expressions;
		}


	  public ExpressionNode LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; }
		}

	  public ExpressionList Expressions
		{
			get { return expressions; }
			set { expressions = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			leftSide.ToSource(sb);

			sb.Append("[");
			expressions.ToSource(sb);
			sb.Append("]");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitElementAccessExpression(this, data);
        }

	}
}
