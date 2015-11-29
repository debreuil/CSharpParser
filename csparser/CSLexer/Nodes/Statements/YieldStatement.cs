using System.Text;

namespace DDW
{
	public class YieldStatement : StatementNode
	{
	  readonly bool isBreak;
	  readonly bool isReturn;
	  private ExpressionNode returnValue;

	  public YieldStatement(bool isBreak, bool isReturn, Token relatedtoken) : base(relatedtoken)
	  {
	    this.isBreak = isBreak;
	    this.isReturn = isReturn;
	  }

	  public ExpressionNode ReturnValue
	  {
	    get { return returnValue; }
	    set { returnValue = value; }
	  }

	  public bool IsBreak
        {
            get
            {
                return isBreak;
            }
        }

        public bool IsReturn
        {
            get
            {
                return isReturn;
            }
        }

	  public override void ToSource(StringBuilder sb)
		{
			sb.Append("yield ");

            if (IsBreak)
            {
                sb.Append("break");
            }

            if (IsReturn)
            {
                sb.Append("return ");
            }

			if (returnValue != null)
			{
				returnValue.ToSource(sb);
			}

			sb.Append(";");
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitYieldStatement(this, data);
        }

	}
}
