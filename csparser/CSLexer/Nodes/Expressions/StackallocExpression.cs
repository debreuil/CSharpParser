using System.Text;

namespace DDW
{
	public class StackallocExpression : PrimaryExpression
	{
	  private ExpressionNode expression;
	  private IType type;

	  public StackallocExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public StackallocExpression(IType type, ExpressionNode expression, Token relatedToken)
            : base(relatedToken)
		{
            this.type = type;
			this.expression = expression;
		}

	  public IType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

	  public ExpressionNode Expression
		{
			get { return expression; }
			set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("stackalloc ");
            type.ToSource(sb);

            sb.Append("[");

            expression.ToSource(sb);

            sb.Append("]");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStackAllocExpression(this, data);
        }
	}
}
