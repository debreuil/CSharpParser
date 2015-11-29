using System;
using System.Text;

namespace DDW
{
    /// <summary>
    /// primary-constraint:
    ///     class-type
    ///     class
    ///     struct
    /// secondary-constraints:
    ///     interface-type
    ///     type-parameter
    ///     secondary-constraints   ,   interface-type
    ///     secondary-constraints   ,   type-parameter
    /// 
    /// Because the parser is not actually able to distinguish primary contraint and secondary constraint
    /// it parses all constraint in an unique class.
    /// </summary>
    public sealed class ConstraintExpressionNode : ISourceCode, IEquatable<ConstraintExpressionNode>
	{
        /// <summary>
        ///  the expression can be a TypeNode or an IdentifierNode
        /// </summary>
        readonly PrimaryExpression expression;

        Token token = new Token( TokenID.Invalid );


        public ConstraintExpressionNode(PrimaryExpression expression)
        {
            this.expression = expression;
        }
        public ConstraintExpressionNode(Token token)
        {
            this.token = token;
        }

      public PrimaryExpression Expression
        {
            get
            {
                return expression;
            }
        }

        public Token Token
        {
            get
            {
                return token;
            }
        }

      #region IEquatable<ConstraintExpressionNode> Members

      public bool Equals(ConstraintExpressionNode other)
      {
        bool ret = false;

        if (other == this)
        {
          ret = true;
        }
        else
        {
          if (other != null)
          {
            if (token.ID == other.token.ID
                && token.Data == other.token.Data)
            {
              if (expression == null && other.expression == null
                  || expression != null && expression.GetType().Equals(other.expression.GetType()))
              {
                if (expression is TypeNode)
                {
                  ret = ((TypeNode)expression).Equals((TypeNode)other.expression);
                }
                else
                {
                  if (expression is IdentifierExpression)
                  {
                    ret = ((IdentifierExpression)expression).Equals((IdentifierExpression)other.expression);
                  }
                }
              }
            }
          }
        }

        return ret;
      }

      #endregion

      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
      {
        if (token.ID == TokenID.Invalid)
        {
          expression.ToSource(sb);
        }
        else
        {
          sb.Append( token.ID.ToString().ToLower() );
        }
      }


      public object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstraintExpression(this, data);
        }

      #endregion

      public override bool Equals(object obj)
      {
        bool ret = false;

        if (obj is ConstraintExpressionNode)
        {
          ret = Equals(obj as ConstraintExpressionNode);
        }
        else
        {
          ret = base.Equals(obj);
        }

        return ret;
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }
	}
}
