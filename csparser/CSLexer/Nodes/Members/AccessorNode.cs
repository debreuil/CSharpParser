using System.Text;

namespace DDW
{
    public class AccessorNode : MemberNode, IIterator
	{
      private readonly bool couldBeIterator;

      public AccessorNode(bool couldBeIterator, Token relatedtoken): base(relatedtoken)
      {
        this.couldBeIterator = couldBeIterator;
        StatementBlock = new BlockStatement(relatedtoken);
      }

      public string Kind { get; set; }

      public bool IsAbstractOrInterface { get; set; }

      public BlockStatement StatementBlock { get; set; }

      #region IIterator Members

      public bool CouldBeIterator
        {
            get
            {
                return couldBeIterator;
            }
        }

      public bool IsIterator { get; set; }

      #endregion

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
				attributes.ToSource(sb);

            TraceModifiers(modifiers, sb);

			sb.Append(Kind);
			if (IsAbstractOrInterface)
			{
				sb.Append(";");
			}
			else
			{
				NewLine(sb);
				// statements
				StatementBlock.ToSource(sb);
			}
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAccessorNode(this, data);
        }
	}
}
