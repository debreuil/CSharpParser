using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class MemberAccessExpression : PrimaryExpression, IType
	{
	  private ExpressionNode left;
	  private TokenID qualifierKind = TokenID.Invalid;
	  private List<int> rankSpecifiers = new List<int>();
	  private ExpressionNode right;

	  public MemberAccessExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public MemberAccessExpression(ExpressionNode left, ExpressionNode right, TokenID qualifierKind)
            : base(left.RelatedToken)
		{
			this.left = left;
			this.right = right;
			this.qualifierKind = qualifierKind;
		}

	  public TokenID QualifierKind
        {
            get
            {
				return qualifierKind;
            }
            set
            {
				qualifierKind = value;
            }
        }

	  public ExpressionNode Right
		{
            get { return right; }
            set { right = value; }
		}

	  public ExpressionNode Left
		{
			get { return left; }
			set { left = value; }
		}

	  #region IType Members

	  public List<int> RankSpecifiers
		{
			get { return rankSpecifiers; }
			set { rankSpecifiers = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			left.ToSource(sb);

            switch(qualifierKind)
            {
				case TokenID.Dot:          sb.Append('.');  break;
				case TokenID.MinusGreater: sb.Append("->"); break;
				case TokenID.ColonColon:   sb.Append("::"); break;
				default:
					throw new Exception("MemberAccessExpression has invalid QualifierKind!");
            }

			right.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitMemberAccessExpression(this, data);
        }

	  #endregion
	}
}
