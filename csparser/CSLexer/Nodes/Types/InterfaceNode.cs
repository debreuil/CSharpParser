using System.Text;
using DDW.Collections;

namespace DDW
{
    public class InterfaceNode : ConstructedTypeNode
	{
		private TypeCollection baseClasses;
      private NodeCollection<InterfaceEventNode> events;
      private NodeCollection<InterfaceIndexerNode> indexers;

      private NodeCollection<InterfaceMethodNode> methods;

      private NodeCollection<InterfacePropertyNode> properties;

      public InterfaceNode(Token relatedToken)
        : base(relatedToken)
      {
        kind = KindEnum.Interface;
      }

      public TypeCollection BaseClasses
      {
        get { if (baseClasses == null) baseClasses = new TypeCollection(); return baseClasses; }
      }

      public NodeCollection<InterfaceMethodNode> Methods
      {
        get { if (methods == null) methods = new NodeCollection<InterfaceMethodNode>(); return methods; }
        set { methods = value; }
      }

      public NodeCollection<InterfacePropertyNode> Properties
		{
			get { if (properties == null) properties = new NodeCollection<InterfacePropertyNode>(); return properties; }
			set { properties = value; }
		}

      public NodeCollection<InterfaceIndexerNode> Indexers
		{
			get { if (indexers == null) indexers = new NodeCollection<InterfaceIndexerNode>(); return indexers; }
			set { indexers = value; }
		}

      public NodeCollection<InterfaceEventNode> Events
		{
			get { if (events == null) events = new NodeCollection<InterfaceEventNode>(); return events; }
			set { events = value; }
		}

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);
			TraceModifiers(modifiers, sb);

            if (IsPartial)
            {
                sb.Append("partial ");
            }

			sb.Append("interface ");
			name.ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			if (baseClasses != null && baseClasses.Count > 0)
			{
				sb.Append(" : ");
                baseClasses.ToSource(sb);
			}

            if (IsGeneric)
                Generic.ConstraintsToSource(sb);

			NewLine(sb);
			sb.Append("{");
			indent++;
			NewLine(sb);

			if (properties != null)
			{
				properties.ToSource(sb);
			}
			if (methods != null)
			{
				methods.ToSource(sb);
			}
			if (indexers != null)
			{
				indexers.ToSource(sb);
			}
			if (events != null)
			{
				events.ToSource(sb);
			}

			indent--;
			NewLine(sb);
			sb.Append("}");
			NewLine(sb);

		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceDeclaration(this, data);
        }
	}
}