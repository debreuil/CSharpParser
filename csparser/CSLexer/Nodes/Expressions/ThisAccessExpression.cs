using System.Text;

namespace DDW
{
	public class ThisAccessExpression : IdentifierExpression
	{
        
		public ThisAccessExpression(Token relatedToken): base(relatedToken)
		{
            identifier = "this";
		}
		public override void ToSource(StringBuilder sb)
		{
			sb.Append("this");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitThisReferenceExpression(this, data);
        }
	}
}
