using System.Text;

namespace DDW
{
	public class RefNode : ExpressionNode
	{
	  private ExpressionNode variableReference;

	  public RefNode(Token relatedToken)
            : base(relatedToken)
        {
        }

        public RefNode(ExpressionNode variableReference) : base(variableReference.RelatedToken)
		{
			this.variableReference = variableReference;
		}

	  public ExpressionNode VariableReference
		{
			get { return variableReference; }
			set { variableReference = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("ref ");
			variableReference.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitRefNode(this, data);
        }

	}
}
