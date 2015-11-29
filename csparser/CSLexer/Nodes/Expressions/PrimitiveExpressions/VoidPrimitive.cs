using System.Text;

namespace DDW
{
    public class VoidPrimitive : LiteralNode, IPointer
	{
        public VoidPrimitive(Token relatedToken)
            : base(relatedToken)
        {
        }
		public override void ToSource(StringBuilder sb)
		{
			sb.Append("void");
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitVoidPrimitive(this, data);
        }

	}
}
