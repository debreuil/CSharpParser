using System;
using System.Diagnostics;
using System.Text;

namespace DDW
{
	public class UsingDirectiveNode : BaseNode
	{
	  private IdentifierExpression aliasName;
	  private PrimaryExpression target;

	  public UsingDirectiveNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public PrimaryExpression Target
		{
            [DebuggerStepThrough]
            get { return target; }
            [DebuggerStepThrough]
            set { target = value; }
		}

	  public IdentifierExpression AliasName
		{
            [DebuggerStepThrough]
            get { return aliasName; }
            [DebuggerStepThrough]
            set { aliasName = value; }
		}

		public bool IsAlias
		{
            [DebuggerStepThrough]
            get { return (aliasName != null); }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("using ");

            if (IsAlias)
            {
                aliasName.ToSource(sb);
                sb.Append(" = ");
            }

			// target
			target.ToSource(sb);

			sb.Append(";");
			NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (IsAlias)
            {
                throw new NotSupportedException();
            }
            else
            {
                resolver.Context.AddUsingDirective(this);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUsingDirective(this, data);
        }

	}
}
