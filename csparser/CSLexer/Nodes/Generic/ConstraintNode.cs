using System;
using System.Text;
using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// type-parameter-constraints-clause:
    ///     where   type-parameter   :   type-parameter-constraints
    /// type-parameter-constraints:
    ///     primary-constraint
    ///     secondary-constraints
    ///     constructor-constraint
    ///     primary-constraint   ,   secondary-constraints
    ///     primary-constraint   ,   constructor-constraint
    ///     secondary-constraints   ,   constructor-constraint
    ///     primary-constraint   ,   secondary-constraints   ,   constructor-constraint
    /// </summary>
    /// <remarks>
    /// Actually we can say if the primary constraint is really a class-type ( and not an interface-type or a type-parameter).
    /// </remarks>
    public sealed class Constraint : ISourceCode, IEquatable<Constraint>
	{
      readonly TypeParameterNode                       typeParameter;
      ConstraintExpressionCollection constraintExpressions = new ConstraintExpressionCollection();
      ConstructorConstraint               constructorConstraint;

      public Constraint(TypeParameterNode typeParameter)
      {
        this.typeParameter = typeParameter;
      }

      public TypeParameterNode TypeParameter
        {
            get
            {
                return typeParameter;
            }
        }

        public ConstructorConstraint ConstructorConstraint
        {
            get
            {
                return constructorConstraint;
            }
            set
            {
                constructorConstraint = value;
            }
        }

        public ConstraintExpressionCollection ConstraintExpressions
        {
            get
            {
                return constraintExpressions;
            }
            set
            {
                constraintExpressions = value;
            }
        }

      #region IEquatable<Constraint> Members

      public bool Equals(Constraint other)
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
                    if (typeParameter == null && other.typeParameter == null
                        || typeParameter != null && typeParameter.Equals(other.typeParameter)
                        )
                    {
                        if (constructorConstraint == null && other.constructorConstraint == null
                            || constructorConstraint != null && other.constructorConstraint != null
                        )
                        {
                            if (constraintExpressions.Count == other.constraintExpressions.Count)
                            {
                                ret = true;

                                for (int i = 0; i < constraintExpressions.Count; ++i)
                                {
                                    if (!constraintExpressions[i].Equals(other.constraintExpressions[i]))
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

            return ret;
        }

      #endregion

      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
            sb.Append("where ");

            typeParameter.ToSource(sb);

            sb.Append( " : " );


            constraintExpressions.ToSource(sb);


            if (constructorConstraint != null)
            {
                if (constraintExpressions.Count  > 0)
                {
                    sb.Append(", ");
                }

                constructorConstraint.ToSource(sb);
            }
		}

        public object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitGenericConstraint (this, data);
        }

      #endregion

      public override bool Equals(object obj)
      {
        bool ret = false;

        if (obj is Constraint)
        {
          ret = Equals(obj as Constraint);
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