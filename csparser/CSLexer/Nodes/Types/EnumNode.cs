using System.Text;
using DDW.Collections;

namespace DDW
{
    public class EnumNode : ConstructedTypeNode
	{
        public EnumNode(Token relatedToken)
            : base(relatedToken)
        {
        }

      public IType BaseClass { get; set; }

      public object Value { get; set; }

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

            if (Value is NodeCollection<EnumNode>)
            {
                TraceModifiers(Modifiers, sb);

                sb.Append("enum ");

                Name.ToSource(sb);

                sb.Append('{');
				indent++;
				NewLine(sb);

                NodeCollection<EnumNode> coll = (NodeCollection<EnumNode>)Value;

				bool isFirst = true;
                foreach (EnumNode expr in coll)
                {
					if(isFirst) isFirst = false;
					else
					{
						sb.Append(',');
						NewLine(sb);
					}
                    expr.ToSource(sb);
                }
				indent--;
				NewLine(sb);
				sb.Append("};");
				NewLine(sb);
			}
            else
            {
                Name.ToSource(sb);
                if (Value != null)
                {
                    sb.Append(" = ");
                    ((ExpressionNode)Value).ToSource(sb);
                }
            }
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitEnumDeclaration(this, data);
        }
	}
}
