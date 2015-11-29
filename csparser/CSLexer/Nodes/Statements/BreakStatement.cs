using System.Text;

namespace DDW
{
	public class BreakStatement : StatementNode
	{
        public BreakStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        public override void ToSource(StringBuilder sb)
        {
			sb.Append("break;");
			NewLine(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBreakStatement(this, data);
        }

	}
}
