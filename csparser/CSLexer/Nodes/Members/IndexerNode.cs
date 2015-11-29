using System.Text;
using DDW.Collections;

namespace DDW
{
	public class IndexerNode : MemberNode
	{
	  private AccessorNode getter;
	  private TypeNode interfaceType;

	  private NodeCollection<ParamDeclNode> parameters;
	  private AccessorNode setter;

	  public IndexerNode(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public TypeNode InterfaceType
	  {
	    get { return interfaceType; }
	    set { interfaceType = value; }
	  }

	  public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
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

			TraceModifiers(modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");
			if (interfaceType != null)
			{
				interfaceType.ToSource(sb);
				sb.Append(".");
			}
			sb.Append("this[");
			if (parameters != null)
			{
				parameters.ToSource(sb, ", ");
			}
			sb.Append("]");

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
            return visitor.VisitIndexerDeclaration(this, data);
        }

	}

}
