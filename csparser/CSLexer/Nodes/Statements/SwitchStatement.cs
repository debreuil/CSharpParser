using System.Text;
using DDW.Collections;

namespace DDW
{
	public class SwitchStatement : StatementNode
	{
	  private NodeCollection<CaseNode> cases = new NodeCollection<CaseNode>();
	  private ExpressionNode test;

	  public SwitchStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

	  public NodeCollection<CaseNode> Cases
		{
			get { return cases; }
			set { cases = value; }
		}

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("switch(");
			test.ToSource(sb);
			sb.Append(")");

			NewLine(sb);
			sb.Append("{");
			indent++;

			for (int i = 0; i < cases.Count; i++)
			{
				cases[i].ToSource(sb);
			}
			indent--;
			NewLine(sb);
			sb.Append("}");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitSwitchStatement(this, data);
        }
        
	}
}
