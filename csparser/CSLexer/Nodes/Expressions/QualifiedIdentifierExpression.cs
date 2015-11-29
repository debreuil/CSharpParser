using System;
using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
    [DebuggerDisplay("Identifier = {GenericIdentifier}")]
    public class QualifiedIdentifierExpression : PrimaryExpression, IEquatable<QualifiedIdentifierExpression>, IGeneric
	{
      readonly ExpressionList expressions = new ExpressionList();
      private bool isNamespaceAliasQualifier;

      public QualifiedIdentifierExpression(Token relatedToken)
        : base(relatedToken)
      {
      }

      public bool IsNamespaceAliasQualifier
        {
            get
            {
                return isNamespaceAliasQualifier;
            }
            set
            {
                isNamespaceAliasQualifier = value;
            }
        }

      public ExpressionList Expressions
        {
            get
            {
                return expressions;
            }
        }

      public string QualifiedIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                bool isFirst = true;
                for(int i = 0; i < expressions.Count; ++i)
                {
                    if(isFirst) isFirst = false;
                    else if(i == 1 && isNamespaceAliasQualifier)
                        sb.Append("::");
                    else
                        sb.Append('.');

                    expressions[i].ToSource(sb);
                }

                return sb.ToString();
            }
        }

      /// <summary>
      /// only check that the last expression is a TypeNode
      /// </summary>
      public bool IsType
      {
        get
        {
          int lastidx = expressions.Count-1;

          return lastidx > 0 && expressions[lastidx] is IType;
        }
      }

      #region IEquatable<QualifiedIdentifierExpression> Members

      public bool Equals(QualifiedIdentifierExpression other)
        {
            bool ret = false;

            if (this == other)
            {
                ret = true;
            }
            else
            {
                if (other != null)
                {
                    // does not use QualifiedIdentifier ( like QualifiedIdentifier == other.QualifiedIdentifier ) 
                    // because the following form may be faster : it can stop before reaching the last identifier
                    if (expressions.Count != other.expressions.Count)
                    {
                        ret = true;

                        for (int i = 0 ; i < expressions.Count; ++i)
                        {
                            if (!expressions[i].Equals(other.Expressions[i]))
                            {
                                ret = false;
                                break;
                            }
                        }
                    }
                }
            }

            return ret;
        }

      #endregion

      #region IGeneric Members

      public GenericNode Generic
        {
            get
            {
				ExpressionNode last = expressions.Last;

                if (last != null && last is IGeneric)
                {
                    return ((IGeneric)last).Generic;
                }

                return null;
            }
            set
            {
				ExpressionNode last = expressions.Last;

                if (last != null && last is IGeneric)
                {
                    ((IGeneric)last).Generic = value;
                }
            }
        }

        public bool IsGeneric
        {
            get
            {
				ExpressionNode last = expressions.Last;

                if (last != null && last is IGeneric)
                {
                    return ((IGeneric)last).Generic != null;
                }

                return false;
            }
        }


        public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                bool isFirst = true;
                for(int i = 0; i < expressions.Count; ++i)
                {
                    if(isFirst) isFirst = false;
                    else if(i == 1 && isNamespaceAliasQualifier)
                        sb.Append("::");
                    else
                        sb.Append('.');

                    if(expressions[i] is IGeneric)
                        sb.Append(((IGeneric) expressions[i]).GenericIdentifier);
                    else
                        expressions[i].ToSource(sb);
                }

                return sb.ToString();
            }
        }

        public string GenericIndependentIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                bool isFirst = true;
                for(int i = 0; i < expressions.Count; ++i)
                {
                    if(isFirst) isFirst = false;
                    else if(i == 1 && isNamespaceAliasQualifier)
                        sb.Append("::");
                    else
                        sb.Append('.');

                    if(expressions[i] is IGeneric)
                        sb.Append(((IGeneric) expressions[i]).GenericIndependentIdentifier);
                    else
                        expressions[i].ToSource(sb);
                }

                return sb.ToString();
            }
        }

      #endregion

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(QualifiedIdentifier);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitQualifiedIdentifierExpression(this, data);
        }
    }
}
