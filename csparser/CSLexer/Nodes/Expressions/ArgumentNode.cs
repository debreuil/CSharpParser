using System.Text;

namespace DDW
{
	public class ArgumentNode : BaseNode
	{
	  private ExpressionNode expression;
	  private bool isRef;

	  public ArgumentNode(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public bool IsRef
		{
			get { return isRef; }
			set { isRef = value; }
		}

	  public bool IsOut { get; set; }

	  public ExpressionNode Expression
		{
			get { return expression; }
			set { expression = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			if (isRef)
			{
				sb.Append("ref ");
			}
			else if (IsOut)
			{
				sb.Append("out ");
			}

			expression.ToSource(sb);
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArgumentNode(this, data);
        }

	}
}
