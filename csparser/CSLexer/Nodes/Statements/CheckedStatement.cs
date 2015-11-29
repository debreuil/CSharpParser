using System.Text;

namespace DDW
{
	public class CheckedStatement : StatementNode
	{
	  private BlockStatement checkedBlock;
	  private ExpressionNode checkedExpression;

	  public CheckedStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public ExpressionNode CheckedExpression
        {
            get
            {
                return checkedExpression;
            }
            set
            {
                checkedExpression = value;
            }
        }

	  public BlockStatement CheckedBlock
		{
			get { return checkedBlock; }
			set { checkedBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("checked ");

            if (checkedExpression != null)
            {
                sb.Append("(");
                checkedExpression.ToSource(sb);
                sb.Append(")");
                sb.Append(";");
            }
            else
            {
                checkedBlock.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCheckedStatement(this, data);
        }
	}
}
