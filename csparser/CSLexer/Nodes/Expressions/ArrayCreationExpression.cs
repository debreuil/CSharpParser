using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ArrayCreationExpression : PrimaryExpression
	{
	  private List<int> additionalRankSpecifiers = new List<int>();
	  private ArrayInitializerExpression initializer;
	  private ExpressionList rankSpecifier;
	  private IType type;

	  public ArrayCreationExpression(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public IType Type
		{
			get { return type; }
			set { type = value; }
		}

	  public ExpressionList RankSpecifier
		{
			get { return rankSpecifier; }
			set { rankSpecifier = value; }
		}

	  public List<int> AdditionalRankSpecifiers
		{
			get { return additionalRankSpecifiers; }
			set { additionalRankSpecifiers = value; }
		}

	  public ArrayInitializerExpression Initializer
		{
			get { return initializer; }
			set { initializer = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("new ");
			type.ToSource(sb);

			if (rankSpecifier != null && rankSpecifier.Count > 0)
			{
                sb.Append("[");
				rankSpecifier.ToSource(sb);
                sb.Append("]");
			}			

			if (additionalRankSpecifiers != null)
			{
				for (int i = 0; i < additionalRankSpecifiers.Count; i++)
				{					
					sb.Append("[");
					for (int j = 0; j < additionalRankSpecifiers[i]; j++)
					{						
						sb.Append(",");
					}
					sb.Append("]");
				}
			}
			if (initializer != null)
			{
				initializer.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArrayCreateExpression(this, data);
        }

	}
}
