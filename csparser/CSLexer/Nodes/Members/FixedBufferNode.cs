using System.Text;
using DDW.Collections;

namespace DDW
{
	public class FixedBufferNode : MemberNode
	{
	  private NodeCollection<ConstantExpression> fixedBufferConstants = new NodeCollection<ConstantExpression>();

	  public FixedBufferNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public NodeCollection<ConstantExpression> FixedBufferConstants
        {
            get
            {
                return fixedBufferConstants;
            }
            set
            {
                fixedBufferConstants = value;
            }
        }

        public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");

			bool isFirst = true;
			for (int i = 0; i < Names.Count; i++)
			{
				if(isFirst) isFirst = false;
				else sb.Append(", ");

				Names[i].ToSource(sb);
                sb.Append("[");
                FixedBufferConstants[i].ToSource(sb);
                sb.Append("]");
			}
			sb.Append(";");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedBufferNode(this, data);
        }

	}

}
