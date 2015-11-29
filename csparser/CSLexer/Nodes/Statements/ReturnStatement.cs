using System.Text;

namespace DDW
{
	public class ReturnStatement : StatementNode
	{
	  private ExpressionNode returnValue;

	  public ReturnStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public ExpressionNode ReturnValue
		{
			get { return returnValue; }
			set { returnValue = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("return");
			if (returnValue != null)
			{
				sb.Append(" ");
				returnValue.ToSource(sb);
			}
			sb.Append(";");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitReturnStatement(this, data);
        }
	}
}
