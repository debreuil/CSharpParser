using System.Text;
using DDW.Collections;

namespace DDW
{
	public class InvocationExpression : PrimaryExpression
	{
	  private NodeCollection<ArgumentNode> argumentList;
	  private ExpressionNode leftSide;

	  public InvocationExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
		public InvocationExpression(ExpressionNode leftSide, NodeCollection<ArgumentNode> argumentList)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
			this.argumentList = argumentList;
		}

	  public ExpressionNode LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; }
		}

	  public NodeCollection<ArgumentNode> ArgumentList
		{
			get { return argumentList; }
			set { argumentList = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			leftSide.ToSource(sb);
			sb.Append("(");
			argumentList.ToSource(sb, ", ");
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInvocationExpression(this, data);
        }
	}
}
