using System.Text;

namespace DDW
{
	public class ThrowNode : StatementNode
	{
	  private ExpressionNode throwExpression;

	  public ThrowNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public ExpressionNode ThrowExpression
		{
			get { return throwExpression; }
			set { throwExpression = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			if (throwExpression != null)
			{
				sb.Append("throw ");
				throwExpression.ToSource(sb);
				sb.Append(";");
			}
			else sb.Append("throw;");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitThrowStatement(this, data);
        }
	}
}
