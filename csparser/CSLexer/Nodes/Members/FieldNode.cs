using System.Text;

namespace DDW
{
	public class FieldNode : MemberNode
	{
        public FieldNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");

			Names.ToSource(sb, ", ");

			if (Value != null)
			{
				sb.Append(" = ");
				Value.ToSource(sb);
			}

			sb.Append(";");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFieldDeclaration(this, data);
        }

	}

}
