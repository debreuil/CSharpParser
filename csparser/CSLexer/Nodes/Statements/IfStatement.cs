using System.Text;

namespace DDW
{
	public class IfStatement : StatementNode
	{
	  private readonly BlockStatement statements;
	  private BlockStatement elseStatements;
	  private ExpressionNode test;

	  public IfStatement(Token relatedToken)
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
		}

	  public BlockStatement ElseStatements
		{
            get { if (elseStatements == null) elseStatements = new BlockStatement(RelatedToken); return elseStatements; }
       }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("if(");
			if (test != null)
			{
				test.ToSource(sb);
			}
			sb.Append(")");
			NewLine(sb);
			statements.ToSource(sb);

			if (elseStatements != null)
			{
				sb.Append("else");
				NewLine(sb);
				elseStatements.ToSource(sb);
			}
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitIfStatement(this, data);
        }

	}
}
