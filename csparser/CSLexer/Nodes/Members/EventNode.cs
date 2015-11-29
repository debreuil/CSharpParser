using System.Text;

namespace DDW
{
	public class EventNode : MemberNode
	{
        public EventNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        //private BlockStatement addBlock = null;
        //public BlockStatement AddBlock
        //{
        //    get { return addBlock; }
        //    set { addBlock = value; }
        //}
        //private BlockStatement removeBlock = null;
        //public BlockStatement RemoveBlock
        //{
        //    get { return removeBlock; }
        //    set { removeBlock = value; }
        //}

	  public AccessorNode AddBlock { get; set; }

	  public AccessorNode RemoveBlock { get; set; }

	  public override void ToSource(StringBuilder sb)
        {
            sb.Append(IdentNewLine(DocComment));

            // todo: eventnode to source

            if (attributes != null)
				attributes.ToSource(sb);

            TraceModifiers(Modifiers, sb);

            sb.Append("event ");

            Type.ToSource(sb);

            sb.Append(" ");

			Names.ToSource(sb, ", ");

            if (AddBlock != null
                || RemoveBlock != null)
            {
				NewLine(sb);

                sb.Append("{");
				indent++;
				NewLine(sb);

                if (AddBlock != null)
                {
                    AddBlock.ToSource(sb);
                }

                if (RemoveBlock != null)
                {
                    RemoveBlock.ToSource(sb);
                }

				indent--;
				NewLine(sb);
                sb.Append("}");
            }
            else
            {
                if (Value != null)
                {
                    sb.Append(" = ");
                    Value.ToSource(sb);
                }

                sb.Append(";");
            }
			NewLine(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitEventDeclaration(this, data);
        }

	}
}
