using System.Text;

namespace DDW
{
	public class UsingStatement : StatementNode
	{
	  private ExpressionNode resource;
	  private BlockStatement statements;

	  public UsingStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	    statements = new BlockStatement(RelatedToken);
	  }

	  public ExpressionNode Resource
	  {
	    get { return resource; }
	    set { resource = value; }
	  }

	  public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("using(");
			resource.ToSource(sb);
			sb.Append(")");
            NewLine(sb);
			statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUsingStatement(this, data);
        }

	}
}
