namespace DDW
{
	public class StructNode : ClassNode
	{
        public StructNode(Token relatedToken)
            : base(relatedToken)
        {
            kind = KindEnum.Struct;
        }


        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStructNode(this, data);
        }

	}
}
