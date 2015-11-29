using System.Text;

namespace DDW
{
	public class FixedStatementStatement : StatementNode
	{
	  readonly FixedDeclarationsStatement localPointers;

	  private readonly BlockStatement statements;

	  public FixedStatementStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    localPointers = new FixedDeclarationsStatement(relatedtoken);
	    statements = new BlockStatement(relatedtoken);
	  }

	  public FixedDeclarationsStatement LocalPointers
	  {
	    get
	    {
	      return localPointers;
	    }
	  }

	  public BlockStatement Statements
		{
			get { return statements; }
		}

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("fixed(");

            localPointers.ToSource(sb);

			sb.Append(")");
			NewLine(sb);
			statements.ToSource(sb);            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedStatement(this, data);
        }

	}
}
