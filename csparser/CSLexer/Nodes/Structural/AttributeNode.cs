using System;
using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class AttributeNode : BaseNode
    {
	  private NodeCollection<AttributeArgumentNode> arguments;
	  private Modifier modifiers;

	  private QualifiedIdentifierExpression name;

	  public AttributeNode(Token relatedToken)
	    : base(relatedToken)
	  {
	  }

	  public Modifier Modifiers
	  {
	    [DebuggerStepThrough]
	    get { return modifiers; }
	    [DebuggerStepThrough]
	    set { modifiers = value; }
	  }

	  public QualifiedIdentifierExpression Name
		{
            [DebuggerStepThrough]
            get { return name; }
            [DebuggerStepThrough]
            set { name = value; }
		}

	  public NodeCollection<AttributeArgumentNode> Arguments
		{
			get { if (arguments == null) arguments = new NodeCollection<AttributeArgumentNode>(); return arguments; }
		}

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("[");
			if(modifiers != Modifier.Empty)
			{
				TraceModifiers(Modifiers, sb);
				sb.Append(": ");
			}
			name.ToSource(sb);

			if (arguments != null)
			{
				sb.Append("(");
				arguments.ToSource(sb, ", ");
				sb.Append(")");
			}
			sb.Append("]");
			NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            throw new NotSupportedException();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAttribute(this, data);
        }

    }
}
