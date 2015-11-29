using System.Text;

namespace DDW
{
	public class LabeledStatement : StatementNode
	{
	  private IdentifierExpression name;

	  private StatementNode statement;

	  public LabeledStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public IdentifierExpression Name
	  {
	    get { return name; }
	    set { name = value; }
	  }

	  public StatementNode Statement
		{
			get { return statement; }
			set { statement = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			name.ToSource(sb);
			sb.Append(": ");
			statement.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLabelStatement(this, data);
        }
	}
}
