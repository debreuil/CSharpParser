using System.Text;

namespace DDW
{
	public class ContinueStatement : StatementNode
	{
        public ContinueStatement(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        public override void ToSource(StringBuilder sb)
        {
            sb.Append("continue;");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitContinueStatement(this, data);
        }

	}
}
