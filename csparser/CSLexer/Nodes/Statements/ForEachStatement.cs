using System.Text;

namespace DDW
{
	public class ForEachStatement : StatementNode
	{
	  private ExpressionNode collection;
	  private ParamDeclNode iterator;
	  private BlockStatement statements;

	  public ForEachStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statements = new BlockStatement(relatedtoken);
	  }

	  public ParamDeclNode Iterator
		{
			get { return iterator; }
			set { iterator = value; }
		}

	  public ExpressionNode Collection
		{
			get { return collection; }
			set { collection = value; }
		}

	  public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("foreach (");

            iterator.ToSource(sb);

            sb.Append(" in ");

            collection.ToSource(sb);

            sb.Append( ")" );

            NewLine(sb);

            statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitForeachStatement(this, data);
        }

	}
}
