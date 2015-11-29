using System.Text;
using DDW.Collections;

namespace DDW
{
    public class InterfaceMethodNode : MemberNode, IGeneric
	{
      GenericNode generic;
      private NodeCollection<ParamDeclNode> parameters;

      public InterfaceMethodNode(Token relatedtoken)
        : base(relatedtoken)
      {
      }

      public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

      #region IGeneric Members

      public GenericNode Generic
        {
            get
            {
                return generic;
            }
            set
            {
                generic = value;
            }
        }

        public bool IsGeneric
        {
            get
            {
                return Generic != null;
            }
        }

        public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Type.ToSource(sb);

                sb.Append(" ");

                names[0].ToSource(sb);

                if (IsGeneric)
                {
                    sb.Append("<");

                    foreach ( TypeParameterNode item in generic.TypeParameters)
                    {
                        item.ToSource(sb);
                        sb.Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);

                    sb.Append(">");
                }

                return sb.ToString();
            }
        }

        public virtual string GenericIndependentIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Type.ToSource(sb);

                sb.Append(" ");

                names[0].ToSource(sb);

                if (IsGeneric)
                {
                    sb.Append("<");

                    if (generic.TypeParameters.Count > 1)
                    {
                        sb.Append(',', generic.TypeParameters.Count - 1);
                    }

                    sb.Append(">");
                }

                return sb.ToString();
            }
        }

      #endregion

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));


            if (attributes != null)
				attributes.ToSource(sb);
			TraceModifiers(Modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");

			names[0].ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			sb.Append("(");
			if (parameters != null)
				parameters.ToSource(sb, ", ");
			sb.Append(")");

            if (IsGeneric)
                Generic.ConstraintsToSource(sb);

            sb.Append(";");
			NewLine(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceMethodNode(this, data);
        }

	}
}
