namespace DDW
{
	public abstract class PrimaryExpression : ExpressionNode
	{
        public PrimaryExpression(Token relatedToken)
            : base(relatedToken)
        {
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPrimaryExpression(this, data);
        }
	}
}
