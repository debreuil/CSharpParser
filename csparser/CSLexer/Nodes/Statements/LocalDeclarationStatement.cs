using System.Text;
using DDW.Collections;

namespace DDW
{
	public class Declarator : ISourceCode
	{
	  public IdentifierExpression Identifier;
		public ExpressionNode Initializer;

	  public Declarator(IdentifierExpression identifier, ExpressionNode initializer)
	  {
	    Identifier = identifier;
	    Initializer = initializer;
	  }

	  #region ISourceCode Members

	  public void ToSource(StringBuilder sb)
		{
			Identifier.ToSource(sb);
			if(Initializer != null)
			{
				sb.Append(" = ");
				Initializer.ToSource(sb);
			}
		}

		public object AcceptVisitor(AbstractVisitor visitor, object data)
		{
			return visitor.VisitDeclarator(this, data);
		}

	  #endregion
	}

	public class LocalDeclaration : ExpressionNode
	{
	  NodeCollection<Declarator> declarators = new NodeCollection<Declarator>();
	  private bool isConstant;
	  IType type;

	  public LocalDeclaration(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		public LocalDeclaration(IType type, IdentifierExpression identifier, ExpressionNode rightSide)
            : base(identifier.RelatedToken)
		{
			this.type = type;
			AddDeclarator(identifier, rightSide);
		}

	  public IType Type
		{
			get { return type; }
			set { type = value; }
		}

	  public NodeCollection<Declarator> Declarators
		{
			get { return declarators; }
			set { declarators = value; }
		}

	  public bool IsConstant
		{
			get { return isConstant; }
			set { isConstant = value; }
		}

	  public void AddDeclarator(IdentifierExpression identifier, ExpressionNode initializer)
	  {
	    declarators.Add(new Declarator(identifier, initializer));
	  }

	  public override void ToSource(StringBuilder sb)
		{
			if (isConstant)
			{
				sb.Append("const ");
			}
			type.ToSource(sb);
			sb.Append(' ');
			declarators.ToSource(sb, ", ");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLocalDeclaration(this, data);
        }
	}

	public class LocalDeclarationStatement : StatementNode
	{
		private readonly LocalDeclaration decl;

		public LocalDeclarationStatement(Token relatedtoken)
			: base(relatedtoken)
		{
			decl = new LocalDeclaration(relatedtoken);
		}

		public LocalDeclarationStatement(IType type, IdentifierExpression identifier, ExpressionNode rightSide)
			: base(identifier.RelatedToken)
		{
			decl = new LocalDeclaration(type, identifier, rightSide);
		}

		public LocalDeclarationStatement(LocalDeclaration decl)
			: base(decl.RelatedToken)
		{
			this.decl = decl;
		}

		public IType Type
		{
			get { return decl.Type; }
			set { decl.Type = value; }
		}

		public NodeCollection<Declarator> Declarators
		{
			get { return decl.Declarators; }
			set { decl.Declarators = value; }
		}

	  public bool IsConstant
		{
			get { return decl.IsConstant; }
			set { decl.IsConstant = value; }
		}

	  public void AddDeclarator(IdentifierExpression identifier, ExpressionNode initializer)
	  {
	    decl.Declarators.Add(new Declarator(identifier, initializer));
	  }

	  public override void ToSource(StringBuilder sb)
		{
			decl.ToSource(sb);
			sb.Append(';');
			NewLine(sb);
		}

		public override object AcceptVisitor(AbstractVisitor visitor, object data)
		{
			return visitor.VisitLocalDeclarationStatement(this, data);
		}
	}
}
