using System.Text;
using DDW.Collections;

namespace DDW
{
	public class FixedDeclarationsStatement : ExpressionNode
	{
	  NodeCollection<Declarator> declarators = new NodeCollection<Declarator>();
	  IType type;

	  public FixedDeclarationsStatement(Token relatedtoken)
	    : base(relatedtoken)
	  {
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

		public void AddDeclarator(IdentifierExpression identifier, ExpressionNode initializer)
		{
			declarators.Add(new Declarator(identifier, initializer));
		}

		public override void ToSource(StringBuilder sb)
		{
			type.ToSource(sb);
			sb.Append(' ');

			declarators.ToSource(sb, ", ");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedDeclarationStatement(this, data);
        }

	}
}
