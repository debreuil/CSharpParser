using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ForStatement : StatementNode
	{
	  private NodeCollection<ExpressionNode> inc;
	  private NodeCollection<ExpressionNode> init;
	  private BlockStatement statements;

	  private ExpressionNode test;

	  public ForStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statements = new BlockStatement(relatedtoken);
	  }

	  public NodeCollection<ExpressionNode> Init
	  {
	    get { return init; }
	    set { init = value; }
	  }

	  public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

	  public NodeCollection<ExpressionNode> Inc
		{
			get { return inc; }
			set { inc = value; }
		}

	  public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("for(");

			if(init != null)
				init.ToSource(sb, ", ");
			sb.Append("; ");
			
			if(test != null)
				test.ToSource(sb);
			sb.Append("; ");

			if(inc != null)
				inc.ToSource(sb, ", ");

			sb.Append(")");
			NewLine(sb);
			statements.ToSource(sb);            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitForStatement(this, data);
        }
	}
}
