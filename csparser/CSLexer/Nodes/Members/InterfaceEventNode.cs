using System.Text;

namespace DDW
{
	public class InterfaceEventNode : MemberNode
	{
        public InterfaceEventNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			sb.Append("event ");
			type.ToSource(sb);

			sb.Append(" ");
			names[0].ToSource(sb);

			sb.Append(";");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceEventNode(this, data);
        }

	}
}
