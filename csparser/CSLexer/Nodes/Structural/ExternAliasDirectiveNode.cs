using System;
using System.Diagnostics;
using System.Text;

namespace DDW
{
	public class ExternAliasDirectiveNode : BaseNode
	{
	  private ExpressionNode externaAliasName;

	  public ExternAliasDirectiveNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	  }

	  public ExpressionNode ExternAliasName
		{
            [DebuggerStepThrough]
            get { return externaAliasName; }
            [DebuggerStepThrough]
            set { externaAliasName = value; }
		}
        public override void ToSource(StringBuilder sb)
        {
			sb.Append("extern alias ");

            if (externaAliasName != null)
            {
                externaAliasName.ToSource(sb);
            }

			sb.Append(";");
			NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            throw new NotSupportedException();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitExternAliasDirectiveNode(this, data);
        }

	}
}
