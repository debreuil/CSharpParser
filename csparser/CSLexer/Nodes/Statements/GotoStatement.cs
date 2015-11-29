using System.Text;

namespace DDW
{
	public class GotoStatement : StatementNode
	{
	  private bool isCase;
	  private ExpressionNode target;

	  public GotoStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public bool IsCase
		{
			get { return isCase; }
			set { isCase = value; }
		}

	  public bool IsDefaultCase { get; set; }

	  public ExpressionNode Target
		{
			get { return target; }
			set { target = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("goto ");

            if (IsDefaultCase)
            {
                sb.Append("default");
            }
            else
            {
                if (isCase)
                {
                    sb.Append("case ");
                }

                target.ToSource(sb);
            }

            sb.Append(";");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitGotoStatement(this, data);
        }

	}
}
