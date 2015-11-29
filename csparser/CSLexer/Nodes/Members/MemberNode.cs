using DDW.Collections;

namespace DDW
{
    public abstract class MemberNode : BaseNode, IUnsafe
	{
      protected Modifier modifiers;

      protected NodeCollection<QualifiedIdentifierExpression> names = new NodeCollection<QualifiedIdentifierExpression>();

      protected IType type;
      protected ExpressionNode val;

      public MemberNode(Token relatedToken)
        : base(relatedToken)
      {
      }

      public Modifier Modifiers
      {
        get { return modifiers; }
        set { modifiers = value; }
      }

      public NodeCollection<QualifiedIdentifierExpression> Names
      {
        get { return names; }
        set { names = value; }
      }

      public IType Type
		{
			get { return type; }
			set { type = value; }
		}

      public ExpressionNode Value
      {
        get { return val; }
        set { val = value; }
      }

      #region IUnsafe Members

      public bool IsUnsafe { get; set; }

      public bool IsUnsafeDeclared { get; set; }

      #endregion
	}
}
