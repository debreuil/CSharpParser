using System.Text;

namespace DDW
{
    public class OperatorNode : MemberNode, IIterator
	{
      private readonly bool couldBeIterator;
      private bool isExplicit;
      private bool isImplicit;
      private TokenID op;
      private ParamDeclNode param1;
      private ParamDeclNode param2;
      private BlockStatement statements ;

      public OperatorNode(Token relatedtoken)
        : base(relatedtoken)
      {
        statements = new BlockStatement(relatedtoken);
      }

      public OperatorNode(bool couldBeIterator, Token relatedtoken) :base(relatedtoken)
      {
        this.couldBeIterator = couldBeIterator;
        statements = new BlockStatement(relatedtoken);
      }

      public TokenID Operator
		{
			get { return op; }
			set { op = value; }
		}

      public bool IsExplicit
		{
			get { return isExplicit; }
			set { isExplicit = value; }
		}

      public bool IsImplicit
		{
			get { return isImplicit; }
			set { isImplicit = value; }
		}

      public ParamDeclNode Param1
		{
			get { return param1; }
			set { param1 = value; }
		}

      public ParamDeclNode Param2
		{
			get { return param2; }
			set { param2 = value; }
		}

      public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
		}

      #region IIterator Members

      public bool CouldBeIterator
        {
            get
            {
                return couldBeIterator;
            }
        }

      public bool IsIterator { get; set; }

      #endregion

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

			TraceModifiers(Modifiers, sb);

			if (isExplicit)
			{
				sb.Append("explicit operator ");
				type.ToSource(sb);
			}
			else if (isImplicit)
			{
				sb.Append("implicit operator ");
				type.ToSource(sb);
			}
			else
			{
				type.ToSource(sb);
				sb.Append(" operator ");
				switch(op)
				{
					case TokenID.Plus:
					case TokenID.Minus:
					case TokenID.Not:
					case TokenID.Tilde:
					case TokenID.Star:
					case TokenID.Slash:
					case TokenID.Percent:
					case TokenID.BAnd:
					case TokenID.BOr:
					case TokenID.BXor:
					case TokenID.Greater:
					case TokenID.Less:
						sb.Append((char) op);
						break;
					case TokenID.PlusPlus:
						sb.Append("++");
						break;
					case TokenID.MinusMinus:
						sb.Append("--");
						break;
					case TokenID.True:
						sb.Append("true");
						break;
					case TokenID.False:
						sb.Append("false");
						break;
					case TokenID.ShiftLeft:
						sb.Append("<<");
						break;
					case TokenID.ShiftRight:
						sb.Append(">>");
						break;
					case TokenID.EqualEqual:
						sb.Append("==");
						break;
					case TokenID.NotEqual:
						sb.Append("!=");
						break;
					case TokenID.GreaterEqual:
						sb.Append(">=");
						break;
					case TokenID.LessEqual:
						sb.Append("<=");
						break;
					default:
						sb.Append("<ILLEGAL OPERATOR: " + op + ">");
						break;
				}
			}

			sb.Append("(");
			if (param1 != null)
			{
				param1.ToSource(sb);
			}
			if (param2 != null)
			{
				sb.Append(", ");
				param2.ToSource(sb);
			}
			sb.Append(")");

            if((Modifiers & Modifier.Extern) != 0)
            {
                sb.Append(';');
                NewLine(sb);
            }
            else
            {
                NewLine(sb);
                statements.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitOperatorDeclaration(this, data);
        }
	}
}
