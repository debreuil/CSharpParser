using System.Text;

namespace DDW
{
	public class CatchNode : StatementNode
	{
	  private BlockStatement catchBlock;
	  private IType classType;

	  private IdentifierExpression identifier;

	  public CatchNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    catchBlock = new BlockStatement(relatedtoken);
	  }

	  public IType ClassType
	  {
	    get { return classType; }
	    set { classType = value; }
	  }

	  public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

	  public BlockStatement CatchBlock
		{
			get { return catchBlock; }
			set { catchBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("catch");
			if (classType != null)
			{
				sb.Append("(");
				classType.ToSource(sb);
				if(identifier != null)
				{
					sb.Append(' ');
					identifier.ToSource(sb);
				}
				sb.Append(")");
			}
			NewLine(sb);
			catchBlock.ToSource(sb);            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCatchClause(this, data);
        }

	}
}
