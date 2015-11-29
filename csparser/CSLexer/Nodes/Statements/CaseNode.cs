using System.Text;
using DDW.Collections;

namespace DDW
{
	public class CaseNode : StatementNode
	{
	  private NodeCollection<ExpressionNode> ranges = new NodeCollection<ExpressionNode>();
	  private NodeCollection<StatementNode> statements = new NodeCollection<StatementNode>();

	  public CaseNode(Token relatedToken)
            : base(relatedToken)
        {
        }

	  public bool IsDefaultCase { get; set; }

	  public NodeCollection<ExpressionNode> Ranges
		{
			get { return ranges; }
			set { ranges = value; }
		}

	  public NodeCollection<StatementNode> Statements
		{
			get { return statements; }
			set { statements = value; }
        }

		public override void ToSource(StringBuilder sb)
        {
			for (int i = 0; i < ranges.Count; i++)
			{
				NewLine(sb);
				sb.Append("case ");
				ranges[i].ToSource(sb);
				sb.Append(":");
			}
			if (IsDefaultCase)
			{
				NewLine(sb);
				sb.Append("default:");
			}

			// do not indent, if the only statement is a block statement
			if(Statements.Count != 1 || !(Statements[0] is BlockStatement))
				indent++;
			NewLine(sb);

			statements.ToSource(sb);

			// unindent, if indented previously
			if(Statements.Count != 1 || !(Statements[0] is BlockStatement))
				indent--;
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCaseLabel(this, data);
        }
	}
}
