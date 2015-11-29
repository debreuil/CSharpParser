using System.Text;
using DDW.Collections;

namespace DDW
{
    public class DelegateNode : ConstructedTypeNode
	{
      private NodeCollection<ParamDeclNode> parameters;
      private IType type;

      public DelegateNode(Token relatedToken)
        : base(relatedToken)
      {
        kind = KindEnum.Delegate;
      }

      public IType Type
		{
			get { return type; }
			set { type = value; }
		}

      public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

      public override string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Type.ToSource(sb);

                sb.Append(" ");

                sb.Append(base.GenericIdentifier);

                return sb.ToString();
            }
        }


		public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(modifiers, sb);

			sb.Append("delegate ");
			type.ToSource(sb);
			sb.Append(" ");
			name.ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			sb.Append("(");
			if (parameters != null)
				parameters.ToSource(sb, ", ");
			sb.Append(")");

            if (IsGeneric)
            {
                NewLine(sb);
                Generic.ConstraintsToSource(sb);
            }

            sb.Append(";");

			NewLine(sb);

		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDelegateDeclaration(this, data);
        }

	}
}