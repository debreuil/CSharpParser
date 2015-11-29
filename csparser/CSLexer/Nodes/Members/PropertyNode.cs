using System.Text;

namespace DDW
{
    public class PropertyNode : MemberNode
    {
        private AccessorNode getter;

        private AccessorNode setter;

        public PropertyNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }

        public AccessorNode Getter
        {
            get { return getter; }
            set { getter = value; }
        }

        public AccessorNode Setter
        {
            get { return setter; }
            set { setter = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
                attributes.ToSource(sb);

            TraceModifiers(Modifiers, sb);

            type.ToSource(sb);
            sb.Append(" ");

            names[0].ToSource(sb);

            // start block
            NewLine(sb);
            sb.Append("{");
            indent++;
            NewLine(sb);

            if (getter != null)
            {
                getter.ToSource(sb);
            }
            if (setter != null)
            {
                setter.ToSource(sb);
            }

            indent--;
            NewLine(sb);
            sb.Append("}");
            NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPropertyDeclaration(this, data);
        }
    }
}
