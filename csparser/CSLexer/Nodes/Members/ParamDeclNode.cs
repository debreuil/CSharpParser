using System.Text;

namespace DDW
{
	public class ParamDeclNode : BaseNode
	{
	  private Modifier modifiers;

	  private string name;

	  private IType type;

	  public ParamDeclNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public Modifier Modifiers
	  {
	    get { return modifiers; }
	    set { modifiers = value; }
	  }

	  public string Name
	  {
	    get { return name; }
	    set { name = value; }
	  }

	  public IType Type
		{
			get { return type; }
			set { type = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

			if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");

			sb.Append(name);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitParameterDeclarationExpression(this, data);
        }
	}
}
