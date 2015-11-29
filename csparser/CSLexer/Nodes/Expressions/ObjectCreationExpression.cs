using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ObjectCreationExpression : PrimaryExpression
	{
	  private NodeCollection<ArgumentNode> argumentList;
	  private IType type;

	  public ObjectCreationExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public ObjectCreationExpression(IType type, Token relatedToken)
            : base(relatedToken)
		{
			this.type = type;
		}
        public ObjectCreationExpression(IType type, NodeCollection<ArgumentNode> argumentList, Token relatedToken)
            : base(relatedToken)
		{
			this.type = type;
			this.argumentList = argumentList;
		}

	  public IType Type
		{
			get { return type; }
			set { type = value; }
		}

	  public NodeCollection<ArgumentNode> ArgumentList
		{
			get { return argumentList; }
			set { argumentList = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("new ");
			type.ToSource(sb);
			sb.Append("(");
			argumentList.ToSource(sb, ", ");
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitObjectCreateExpression(this, data);
        }
	}
}
