using System.Diagnostics;
using System.Text;

namespace DDW
{
	public class AttributeArgumentNode : BaseNode
	{
	  private IdentifierExpression argumentName;

	  private ExpressionNode expression;

	  public AttributeArgumentNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public IdentifierExpression ArgumentName
	  {
	    [DebuggerStepThrough]
	    get { return argumentName; }
	    [DebuggerStepThrough]
	    set { argumentName = value; }
	  }

	  public ExpressionNode Expression
		{
            [DebuggerStepThrough]
            get { return expression; }
            [DebuggerStepThrough]
            set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if (argumentName != null)
			{
				argumentName.ToSource(sb);
				sb.Append(" = ");
			}
			expression.ToSource(sb);
		}

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            if (expression != null)
            {
                expression.Parent = this;
                expression.Resolve(resolver);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAttributeArgumentNode(this, data);
        }

	}
}
