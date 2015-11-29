using System.Text;

namespace DDW
{
	public class PPDefineNode : PPNode
	{
	  private IdentifierExpression identifier;

	  public PPDefineNode(Token relatedToken) : base(relatedToken)
		{
		}
        public PPDefineNode(IdentifierExpression identifier)
            : base(identifier.RelatedToken)
		{
			this.identifier = identifier;
		}

	  public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("#define ");
            identifier.ToSource(sb);
            NewLine(sb);
        }
	}
}
