using System;
using System.Text;

namespace DDW
{
	public class UnaryExpression : ExpressionNode
	{
	  protected ExpressionNode child;
	  private TokenID op;

	  public UnaryExpression(Token relatedtoken)
            : base(relatedtoken)
		{
		}

        public UnaryExpression(TokenID op, Token relatedtoken) : base(relatedtoken)
		{
			this.op = op;
		}

        public UnaryExpression(TokenID op, ExpressionNode child, Token relatedtoken)
            : base(relatedtoken)
		{
			this.op = op;
			this.child = child;
		}

	  public TokenID Op
		{
			get { return op; }
			set { op = value; }
		}

	  public ExpressionNode Child
		{
			get { return child; }
			set { child = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			String opStr;
			switch(op)
			{
				case TokenID.Plus:       opStr = "+";  break;
				case TokenID.Minus:      opStr = "-";  break;
				case TokenID.Not:        opStr = "!";  break;
				case TokenID.Tilde:      opStr = "~";  break;
				case TokenID.PlusPlus:   opStr = "++"; break;
				case TokenID.MinusMinus: opStr = "--"; break;
				case TokenID.BAnd:       opStr = "&";  break;
				case TokenID.Star:       opStr = "*";  break;
				default:
					throw new Exception("Unexpected unary operator: " + op);
			}
			sb.Append(opStr);
			child.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUnaryExpression(this, data);
        }
	}
}
