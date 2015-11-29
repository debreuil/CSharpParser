#pragma warning disable 659

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DDW
{
    [DebuggerDisplay("Identifier = {GenericIdentifier}")]
    public class TypeNode : PrimaryExpression, IType, IGeneric, IEquatable<TypeNode>, IPointer, INullableType
	{
      GenericNode generic;
      private QualifiedIdentifierExpression identifier;
      bool isNullableType;
      private List<int> rankSpecifiers = new List<int>();

      public TypeNode(Token relatedToken)
        : base(relatedToken)
      {
        identifier = new QualifiedIdentifierExpression(relatedToken);
      }


      public TypeNode(QualifiedIdentifierExpression identifier)
            : base(identifier.RelatedToken)
		{
			this.identifier = identifier;
		}

        public TypeNode(IdentifierExpression identifier)
            : base(identifier.RelatedToken)
        {
            this.identifier = new QualifiedIdentifierExpression(RelatedToken);
            this.identifier.Expressions.Add( identifier );
        }

        public TypeNode(ExpressionNode expression)
            : base(expression.RelatedToken)
        {
            if (expression is QualifiedIdentifierExpression)
            {
                identifier = (QualifiedIdentifierExpression)expression;
            }
            else
            {
                identifier = new QualifiedIdentifierExpression(RelatedToken);
                identifier.Expressions.Add(expression);
            }
        }

      public QualifiedIdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

      #region IEquatable<TypeNode> Members

      public bool Equals(TypeNode other)
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
                    if (identifier == null && other.identifier == null
                        || identifier != null && identifier.Equals(other.identifier))
                    {
                        if (generic == null && other.generic == null
                            || generic != null && generic.Equals(other.generic))
                        {
                            if (rankSpecifiers == null && other.rankSpecifiers == null
                                || rankSpecifiers != null && other.rankSpecifiers != null)
                            {
                                if (rankSpecifiers.Count == other.rankSpecifiers.Count)
                                {
                                    ret = true;
                                    for (int i = 0; i < rankSpecifiers.Count; ++i)
                                    {
                                        if (rankSpecifiers[i] != other.rankSpecifiers[i])
                                        {
                                            ret = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

      #endregion

      #region IGeneric Members

      public virtual GenericNode Generic
      {
        get
        {
          return generic;
        }
        set
        {
          generic = value;
        }
      }

      public virtual bool IsGeneric
      {
        get
        {
          return Generic != null;
        }
      }

      public virtual string GenericIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (identifier.IsGeneric)
                {
                    sb.Append(identifier.GenericIdentifier);
                }
                else
                {
                    identifier.ToSource(sb);
                }

                if (IsGeneric)
                {
                    sb.Append("<");

                    foreach ( TypeParameterNode  item in generic.TypeParameters)
                    {
                        item.ToSource(sb);
                        sb.Append(",");
                    }
                    sb.Remove(sb.Length - 1, 1);

                    sb.Append(">");
                }

                if (isNullableType)
                {
                    sb.Append("? ");
                }

                if (rankSpecifiers.Count > 0)
                {
                    foreach (int val in rankSpecifiers)
                    {
                        sb.Append("[");
                        for (int i = 0; i < val; i++)
                        {
                            sb.Append(",");
                        }
                        sb.Append("]");
                    }
                }

                return sb.ToString();
            }
        }

        public string GenericIndependentIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (identifier.IsGeneric)
                {
                    sb.Append(identifier.GenericIndependentIdentifier);
                }
                else
                {
                    identifier.ToSource(sb);
                }

                if (IsGeneric)
                {
                    sb.Append("<");

                    sb.Append(',', generic.TypeParameters.Count - 1);

                    sb.Append(">");
                }

                if (isNullableType)
                {
                    sb.Append("? ");
                }

                if (rankSpecifiers.Count > 0)
                {
                    foreach (int val in rankSpecifiers)
                    {
                        sb.Append("[");
                        for (int i = 0; i < val; i++)
                        {
                            sb.Append(",");
                        }
                        sb.Append("]");
                    }
                }

                return sb.ToString();
            }
        }

      #endregion

      #region INullableType Members

      public virtual bool IsNullableType
      {
        get
        {
          return isNullableType;
        }
        set
        {
          isNullableType = true;
        }
      }

      #endregion

      #region IType Members

      public List<int> RankSpecifiers
      {
        get { return rankSpecifiers; }
        set { rankSpecifiers = value; }
      }

      public override void ToSource(StringBuilder sb)
		{
			identifier.ToSource(sb);

            if (IsGeneric)
            {
                generic.ToSource(sb);
            }

            if (isNullableType)
            {
                sb.Append("?");
            }

			if (rankSpecifiers.Count > 0)
			{
				foreach (int val in rankSpecifiers)
				{
					sb.Append("[");
					for (int i = 0; i < val; i++)
					{
						sb.Append(",");
					}
					sb.Append("]");					
				}
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypeReference(this, data);
        }

      #endregion

      public override bool Equals(object obj)
      {
        bool ret = false;

        if (obj is TypeNode)
        {
          ret = Equals(obj as TypeNode);
        }
        else
        {
          ret = base.Equals(obj);
        }

        return ret;
      }
	}
}

#pragma warning restore 659