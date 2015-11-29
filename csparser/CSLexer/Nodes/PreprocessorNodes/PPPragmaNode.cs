using System.Text;
using DDW.Collections;

namespace DDW
{
    public enum PragmaAction { disable, restore }
	public class PPPragmaNode : PPNode
	{
	  private IdentifierExpression identifier;
	  private NodeCollection<ConstantExpression> value = new NodeCollection<ConstantExpression>();

	  public PPPragmaNode(Token relatedToken)
            : base(relatedToken)
		{
		}
        public PPPragmaNode(IdentifierExpression identifier, NodeCollection<ConstantExpression> value, PragmaAction action)
            : base(identifier.RelatedToken)
		{
			this.identifier = identifier;
            this.value = value;
            this.Action = action;
		}

	  public PragmaAction Action { get; set; }

	  public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

	  public NodeCollection<ConstantExpression> Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("#pragma ");
            if (identifier!= null ) identifier.ToSource(sb);
            sb.Append(" ");
            if ( value != null && value.Count > 0) value.ToSource(sb);
            NewLine(sb);
        }
	}
}
