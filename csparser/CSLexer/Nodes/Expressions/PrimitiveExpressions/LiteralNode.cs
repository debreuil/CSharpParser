namespace DDW
{
	public abstract class LiteralNode : PrimaryExpression
	{
        public LiteralNode(Token relatedToken)
            : base(relatedToken)
        {
        }
	}
}
