using System.Text;

namespace DDW
{
	public class DoStatement : StatementNode
	{
	  private BlockStatement statements;
	  private ExpressionNode test;

	  public DoStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	    statements = new BlockStatement(relatedToken);
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
            sb.Append("do ");
            statements.ToSource(sb);
            sb.Append("while (");
            test.ToSource(sb);
            sb.Append(");");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDoLoopStatement(this, data);
        }

	}
}
