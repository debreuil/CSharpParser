using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// this interface is used by all constructed type.
    /// If a node inherits from this class, it does not means that 
    /// the type is a Generic type. It means that the type could be generic.
    /// To test if the type is generic, lookat the property <see cref="IsGeneric"/>
    /// </summary>
    public abstract class ConstructedTypeNode : BaseNode, IGeneric, IPartial, IUnsafe
	{
      // Omer - Shouldn't there be an 'Enum' option here as well?

      #region KindEnum enum

      public enum KindEnum
        {
            Class,
            Struct, 
            Interface,
            Delegate
        }

      #endregion

      protected GenericNode generic;
      protected KindEnum kind;

      protected Modifier modifiers;

      protected IdentifierExpression name;

      public ConstructedTypeNode(Token relatedtoken)
        : base(relatedtoken)
      {
      }

      public Modifier Modifiers
      {
        [DebuggerStepThrough]
        get { return modifiers; }
        [DebuggerStepThrough]
        set { modifiers = value; }
      }

      public IdentifierExpression Name
        {
            [DebuggerStepThrough]
            get { return name; }
            [DebuggerStepThrough]
            set { name = value; }
        }

      public KindEnum Kind
      {
        [DebuggerStepThrough]
        get
        {
          return kind;
        }
      }

      public bool IsStatic
      {
        get
        {
          return ((modifiers & Modifier.Static) != Modifier.Empty);
        }
      }

      #region IGeneric Members

      public GenericNode Generic
        {
            [DebuggerStepThrough]
            get
            {
                return generic;
            }
            [DebuggerStepThrough]
            set
            {
                generic = value;
            }
        }

        public bool IsGeneric
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

          sb.Append(kind.ToString().ToLower());
          sb.Append(" ");

          if ( name != null ) name.ToSource(sb);

          if (IsGeneric)
          {
            sb.Append("<");

            foreach ( TypeParameterNode item in generic.TypeParameters)
            {
              item.ToSource(sb);
              sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);


            sb.Remove( sb.Length-1 ,1);
            sb.Append(">");
          }

          return sb.ToString();
        }
      }

      public virtual string GenericIndependentIdentifier
      {
        get
        {
          StringBuilder sb = new StringBuilder();

          sb.Append(kind.ToString().ToLower());
          sb.Append(" ");

          name.ToSource(sb);

          if (IsGeneric)
          {
            sb.Append("<");

            if (generic.TypeParameters.Count > 1)
            {
              sb.Append(',', generic.TypeParameters.Count - 1);
            }

            sb.Append(">");
          }

          return sb.ToString();
        }
      }

      #endregion

      #region IPartial Members

      public PartialCollection Partials { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      set; }

      public bool IsPartial { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      set; }

      #endregion

      #region IUnsafe Members

      public bool IsUnsafe { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      set; }

      public bool IsUnsafeDeclared { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      set; }

      #endregion

      protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (IsGeneric)
            {
                Generic.Parent = this;
                Generic.Resolve(resolver, false);
            }
        }

    }
}
