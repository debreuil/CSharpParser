using System.Text;

namespace DDW
{
	public class WhileStatement : StatementNode
	{
	  private BlockStatement statements;
	  private ExpressionNode test;

	  public WhileStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	    statements = new BlockStatement(RelatedToken);
	  }

	  public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

	  public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("while (");
            test.ToSource( sb );
            sb.Append(")");
			NewLine(sb);
            statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitWhileStatement(this, data);
        }
	}
}
