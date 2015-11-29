using System.Text;
using DDW.Collections;

namespace DDW
{
	public class InterfaceIndexerNode : MemberNode
	{
	  protected NodeCollection<AttributeNode> getterAttributes = new NodeCollection<AttributeNode>();
	  private bool hasGetter;
	  private bool hasSetter;
	  private NodeCollection<ParamDeclNode> parameters;
	  protected NodeCollection<AttributeNode> setterAttributes = new NodeCollection<AttributeNode>();

	  public InterfaceIndexerNode(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

	  public bool HasGetter
		{
			get { return hasGetter; }
			set { hasGetter = value; }
		}

	  public NodeCollection<AttributeNode> GetterAttributes
        {
            get { return getterAttributes; }
            set { getterAttributes = value; }
        }

	  public bool HasSetter
		{
			get { return hasSetter; }
			set { hasSetter = value; }
		}

	  public NodeCollection<AttributeNode> SetterAttributes
        {
            get { return setterAttributes; }
            set { setterAttributes = value; }
        }

		public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			type.ToSource(sb);
			sb.Append(" this[");
			if (parameters != null)
			{
				parameters.ToSource(sb, ", ");
			}
			sb.Append("] { ");
			if (hasGetter)
			{
                getterAttributes.ToSource(sb);
				sb.Append("get; ");
			}
			if (hasSetter)
			{
                setterAttributes.ToSource(sb);
                sb.Append("set; ");
			}
			sb.Append("}");			
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceIndexerNode(this, data);
        }

	}
}
