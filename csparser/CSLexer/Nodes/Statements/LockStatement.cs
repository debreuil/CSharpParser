using System.Text;

namespace DDW
{
	public class LockStatement : StatementNode
	{
	  private BlockStatement statements;
	  private ExpressionNode target;

	  public LockStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statements = new BlockStatement(relatedtoken);
	  }

	  public ExpressionNode Target
		{
			get { return target; }
			set { target = value; }
		}

	  public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("lock(");
			target.ToSource(sb);
			sb.Append(")");
			statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLockStatement(this, data);
        }
	}
}
