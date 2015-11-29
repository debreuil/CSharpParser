using System;
using System.Text;
using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// this interface defienes generic type ( class, interface, struct, delegate, method )
    /// C# 2.0 specification
    /// 
    /// attributesopt   class-modifiersopt   class   identifier   type-parameter-listopt   class-baseopt
    ///	        type-parameter-constraints-clausesopt   class-body   ;opt
    /// 
    /// This class is used as a Property/Field type by <see cref="ClassNode"/>, <see cref="InterfaceNode"/>,
    ///     <see cref="StructNode"/>, <see cref="DelegateNode"/>, <see cref="TypeNode"/>, <see cref="MethodNode"/>,
    /// </summary>
	public sealed class GenericNode : BaseNode, IEquatable<GenericNode>
	{
      /// <summary>
      /// type-parameter-constraints-clauses:
      ///     type-parameter-constraints-clause
      ///     type-parameter-constraints-clauses   type-parameter-constraints-clause
      /// </sunmmary>
      private ConstraintCollection _constraints = new ConstraintCollection();

      private TypeParameterCollection typeParameters = new TypeParameterCollection();

      public GenericNode(Token relatedtoken)
        : base(relatedtoken)
      {
      }

      public TypeParameterCollection TypeParameters
        {
            get
            {
                return typeParameters;
            }
            set
            {
                typeParameters = value;
            }
        }

      public ConstraintCollection Constraints
        {
            get
            {
                return _constraints;
            }
            set
            {
                _constraints = value;
            }
        }

      #region IEquatable<GenericNode> Members

      public bool Equals(GenericNode other)
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
            if (typeParameters.Count == other.typeParameters.Count)
            {
              ret = true;

              foreach ( TypeParameterNode item in TypeParameters)
              {
                if ( !other.TypeParameters.Contains(item) )
                {
                  ret = false;
                  break;
                }
              }

              if (ret)
              {
                if (_constraints.Count == other._constraints.Count)
                {
                  for (int i = 0; i < _constraints.Count; ++i)
                  {
                    if (_constraints[i] != other._constraints[i])
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

      public void TypeParametersToSource(StringBuilder sb)
        {
            sb.Append("<");

            TypeParameters.ToSource(sb);

            sb.Append(">");
        }

        public void ConstraintsToSource(StringBuilder sb)
        {
			indent++;
            _constraints.ToSource(sb);
			indent--;
        }

      public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj is GenericNode)
            {
                ret = Equals(obj as GenericNode);
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

        public override void ToSource(StringBuilder sb)
        {
            // by default we call the TypeParametersToSource method.
            // -> constraint is serialized only for specific type nodes ( ClassNode, StructNode ... )
            TypeParametersToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitGenericDefinition(this, data);
        }
	}
}
