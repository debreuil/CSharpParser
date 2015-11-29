using System.Text;

namespace DDW
{
	public class DestructorNode : MemberNode
	{
	  private BlockStatement statementBlock;

	  public DestructorNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statementBlock = new BlockStatement(relatedtoken);
	  }

	  public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			sb.Append("~");
			Names[0].ToSource(sb);
			sb.Append("()");

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
            return visitor.VisitDestructorDeclaration(this, data);
        }

	}
}
