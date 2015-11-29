using System.Text;

namespace DDW
{
	public class NullPrimitive : LiteralNode
	{
        public NullPrimitive(Token relatedToken)
            : base(relatedToken)
        {
        }
		public override void ToSource(StringBuilder sb)
		{
			sb.Append("null");
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitNullPrimitive(this, data);
        }

	}
}
