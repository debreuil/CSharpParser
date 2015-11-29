using System.Text;
using DDW.Collections;

namespace DDW
{
	public class TryStatement : StatementNode
	{
	  private NodeCollection<CatchNode> catchBlocks = new NodeCollection<CatchNode>();
	  private FinallyNode finallyBlock;
	  private BlockStatement tryBlock;

	  public TryStatement(Token relatedToken)
	    : base(relatedToken)
	  {
	    tryBlock = new BlockStatement(RelatedToken);
	  }

	  public BlockStatement TryBlock
		{
			get { return tryBlock; }
			set { tryBlock = value; }
		}

	  public NodeCollection<CatchNode> CatchBlocks
		{
			get { return catchBlocks; }
			set { catchBlocks = value; }
		}

	  public FinallyNode FinallyBlock
		{
			get { return finallyBlock; }
			set { finallyBlock = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("try");
			NewLine(sb);
			tryBlock.ToSource(sb);
			foreach (CatchNode cb in catchBlocks)
			{				
				cb.ToSource(sb);
			}
			
			if (finallyBlock != null)
			{
				finallyBlock.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTryStatement(this, data);
        }
	}
}
