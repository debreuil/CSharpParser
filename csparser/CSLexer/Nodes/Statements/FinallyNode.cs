using System.Text;

namespace DDW
{
	public class FinallyNode : StatementNode
	{
	  private BlockStatement finallyBlock;

	  public FinallyNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    finallyBlock = new BlockStatement(relatedtoken);
	  }

	  public BlockStatement FinallyBlock
		{
			get { return finallyBlock; }
			set { finallyBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("finally");
			NewLine(sb);
			finallyBlock.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFinallyStatement(this, data);
        }
	}
}
