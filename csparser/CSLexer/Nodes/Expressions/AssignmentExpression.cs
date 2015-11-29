using System.Text;

namespace DDW
{
	public class AssignmentExpression : ExpressionNode
	{
	  TokenID op;
	  private ExpressionNode rightSide;
	  private ExpressionNode variable;

	  public AssignmentExpression(Token relatedtoken)
            : base(relatedtoken)
        {
        }

        public AssignmentExpression(TokenID op, ExpressionNode variable, ExpressionNode rightSide): base(variable.RelatedToken)
		{
			this.op = op;
			this.variable = variable;
			this.rightSide = rightSide;
		}

	  public TokenID Operator
		{
			get { return op; }
			set { op = value; }
		}

	  public ExpressionNode Variable
		{
			get { return variable; }
			set { variable = value; }
		}

	  public ExpressionNode RightSide
		{
			get { return rightSide; }
			set { rightSide = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			variable.ToSource(sb);
			sb.Append(" ");
			switch (op)
			{
				case TokenID.Equal:
					sb.Append("=");
					break;
				case TokenID.PlusEqual:
					sb.Append("+=");
					break;
				case TokenID.MinusEqual:
					sb.Append("-=");
					break;
				case TokenID.StarEqual:
					sb.Append("*=");
					break;
				case TokenID.SlashEqual:
					sb.Append("/=");
					break;
				case TokenID.PercentEqual:
					sb.Append("%=");
					break;
				case TokenID.BAndEqual:
					sb.Append("&=");
					break;
				case TokenID.BOrEqual:
					sb.Append("|=");
					break;
				case TokenID.BXorEqual:
					sb.Append("^=");
					break;
				case TokenID.ShiftLeftEqual:
					sb.Append("<<=");
					break;
				case TokenID.ShiftRightEqual:
					sb.Append(">>=");
					break;
			}
			sb.Append(" ");
			rightSide.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAssignmentExpression(this, data);
        }

	}
}
