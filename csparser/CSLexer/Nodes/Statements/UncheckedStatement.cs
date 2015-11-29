using System.Text;

namespace DDW
{
	public class UncheckedStatement : StatementNode
    {
	  private BlockStatement uncheckedBlock;
	  private ExpressionNode uncheckedExpression;

	  public UncheckedStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public ExpressionNode UncheckedExpression
        {
            get
            {
                return uncheckedExpression;
            }
            set
            {
                uncheckedExpression = value;
            }
        }

	  public BlockStatement UncheckedBlock
		{
			get { return uncheckedBlock; }
			set { uncheckedBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("unchecked ");

            if (uncheckedExpression != null)
            {
                sb.Append("(");
                uncheckedExpression.ToSource(sb);
                sb.Append(")");
                sb.Append(";");
            }
            else
            {
                uncheckedBlock.ToSource(sb);
            }            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUncheckedStatement(this, data);
        }

	}
}
