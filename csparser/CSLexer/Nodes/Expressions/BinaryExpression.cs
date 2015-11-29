using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
 
	public class BinaryExpression : ExpressionNode
	{
		private static readonly Dictionary<TokenID, string> stringMap;
	  private ExpressionNode left;
	  private TokenID op;
	  private ExpressionNode right;

	  static BinaryExpression()
	  {
	    stringMap = new Dictionary<TokenID, string>();
	    stringMap.Add(TokenID.Not, @"!");
	    stringMap.Add(TokenID.Percent, @"%");
	    stringMap.Add(TokenID.BAnd, @"&");
	    stringMap.Add(TokenID.BOr, @"|");
	    stringMap.Add(TokenID.BXor, @"^");
	    stringMap.Add(TokenID.Star, @"*");
	    stringMap.Add(TokenID.Plus, @"+");
	    stringMap.Add(TokenID.Minus, @"-");
	    stringMap.Add(TokenID.Slash, @"/");
	    stringMap.Add(TokenID.Less, @"<");
	    stringMap.Add(TokenID.Greater, @">");

	    stringMap.Add(TokenID.PlusPlus, @"++");
	    stringMap.Add(TokenID.MinusMinus, @"--");
	    stringMap.Add(TokenID.And, @"&&");
	    stringMap.Add(TokenID.Or, @"||");
	    stringMap.Add(TokenID.EqualEqual, @"==");
	    stringMap.Add(TokenID.NotEqual, @"!=");
	    stringMap.Add(TokenID.LessEqual, @"<=");
	    stringMap.Add(TokenID.GreaterEqual, @">=");
	    stringMap.Add(TokenID.ShiftLeft, @"<<");
	    stringMap.Add(TokenID.ShiftRight, @">>");

	    stringMap.Add(TokenID.Is, @"is");
	    stringMap.Add(TokenID.As, @"as");

	    stringMap.Add(TokenID.MinusGreater, @"->");

	    stringMap.Add(TokenID.Equal, "=");
	    stringMap.Add(TokenID.PlusEqual, "+=");
	    stringMap.Add(TokenID.MinusEqual, "-=");
	    stringMap.Add(TokenID.StarEqual, "*=");
	    stringMap.Add(TokenID.SlashEqual, "/=");
	    stringMap.Add(TokenID.PercentEqual, "%=");
	    stringMap.Add(TokenID.BAndEqual, "&=");
	    stringMap.Add(TokenID.BOrEqual, "|=");
	    stringMap.Add(TokenID.BXorEqual, "^=");
	    stringMap.Add(TokenID.ShiftLeftEqual, "<<=");
	    stringMap.Add(TokenID.ShiftRightEqual, ">>=");

	    stringMap.Add(TokenID.QuestionQuestion, "??");
	  }

	  public BinaryExpression(Token relatedtoken)
            : base(relatedtoken)
		{
		}

        public BinaryExpression(TokenID op, Token relatedtoken)
            : base(relatedtoken)
		{
			this.op = op;
		}
        public BinaryExpression(TokenID op, ExpressionNode left)
            : base(left.RelatedToken)
		{
			this.op = op;
			this.left = left;
		}
        public BinaryExpression(TokenID op, ExpressionNode left, ExpressionNode right)
            : base(left.RelatedToken)
		{
			this.op = op;
			this.left = left;
			this.right = right; // right must be 'type'
		}

	  public TokenID Op
		{
			get { return op; }
			set 
			{
				if (!stringMap.ContainsKey(op))
				{
					throw new ArgumentException("The TokenID " + op + " does not represent a valid binary operator.");
				}
				op = value; 
			}
		}

	  public ExpressionNode Left
		{
			get { return left; }
			set { left = value; }
		}

	  public ExpressionNode Right
		{
			get { return right; }
			set { right = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
//			sb.Append('(');
			left.ToSource(sb);
//			sb.Append(')');
			sb.Append(" " + stringMap[op] + " ");
//			sb.Append('(');
			right.ToSource(sb);
//			sb.Append(')');
		}

	  public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBinaryOperatorExpression(this, data);
        }

	}
}
