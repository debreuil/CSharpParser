namespace DDW
{
	public abstract class ExpressionNode : BaseNode
	{
        public ExpressionNode(Token relatedToken)
            : base(relatedToken)
        {
        }
    }
}
