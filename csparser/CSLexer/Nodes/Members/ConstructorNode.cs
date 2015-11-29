using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ConstructorNode : MemberNode
	{
	  private bool hasBase;
	  private bool hasThis;
	  private bool isStaticConstructor;
	  private NodeCollection<ParamDeclNode> parameters;
	  private BlockStatement statementBlock;
	  private NodeCollection<ArgumentNode> thisBaseArgs;

	  public ConstructorNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statementBlock = new BlockStatement(relatedtoken);
	  }

	  public bool HasThis
		{
			get { return hasThis; }
			set { hasThis = value; }
		}

	  public bool HasBase
		{
			get { return hasBase; }
			set { hasBase = value; }
		}

	  public NodeCollection<ArgumentNode> ThisBaseArgs
		{
			get { return thisBaseArgs; }
			set { thisBaseArgs = value; }
		}

	  public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
		}

	  public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
		}

	  public bool IsStaticConstructor
		{
			get { return isStaticConstructor; }
			set { isStaticConstructor = value; }
		}

        public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			if (isStaticConstructor)
			{
				sb.Append("static ");
			}

			Names[0].ToSource(sb);
			sb.Append("(");
			if (parameters != null)
				parameters.ToSource(sb, ", ");
			sb.Append(")");

			// possible :this or :base
			if(hasBase || hasThis)
			{
				if(hasBase)
					sb.Append(" : base(");
				else
					sb.Append(" : this(");

				if (thisBaseArgs != null)
					thisBaseArgs.ToSource(sb, ", ");

				sb.Append(")");
			}

            if((Modifiers & Modifier.Extern) != 0)
            {
                sb.Append(';');
                NewLine(sb);
            }
            else
            {
                NewLine(sb);
                statementBlock.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstructorDeclaration(this, data);
        }
	}
}
