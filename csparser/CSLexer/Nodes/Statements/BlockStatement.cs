using System.Text;
using DDW.Collections;

namespace DDW
{
    public class BlockStatement : StatementNode, IUnsafe
	{
		private bool hasBraces = true;

      private NodeCollection<StatementNode> statements = new NodeCollection<StatementNode>();

      public BlockStatement(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        public BlockStatement(bool isUnsafe, Token relatedtoken) : base(relatedtoken)
        {
            this.IsUnsafe = isUnsafe;
        }

      public bool HasBraces
      {
        get { return hasBraces; }
        set { hasBraces = value; }
      }

      public NodeCollection<StatementNode> Statements
      {
        get { return statements; }
        set { statements = value; }
      }

      #region IUnsafe Members

      public bool IsUnsafe { get; set; }

      public bool IsUnsafeDeclared { get; set; }

      #endregion

      public override void ToSource(StringBuilder sb)
        {
            if (IsUnsafeDeclared)
            {
                sb.Append("unsafe ");
            }

			if (hasBraces)
			{
				sb.Append("{");
				indent++;
				NewLine(sb);
			}
			else if(statements.Count == 1)
			{
				// only a case stmt can have more than one stmt without braces, and it special cases this
				AddTab(sb);
			}

			if (statements != null)
			{
				statements.ToSource(sb);
			}

			if (hasBraces)
			{
				indent--;
				NewLine(sb);
				sb.Append("}");
			}
			NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBlockStatement(this, data);
        }

	}
	
}
