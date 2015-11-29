using System.Text;

namespace DDW
{
	public class CommentStatement : StatementNode
	{
	  private readonly string comment = string.Empty;
	  private readonly bool multiLine;

	  public CommentStatement(Token relatedtoken, string comment, bool multiLine)
            : base(relatedtoken)
		{
            this.multiLine = multiLine;
            this.comment = comment;
		}

	  public bool MultiLine
	  {
	    get
	    {
	      return multiLine;
	    }
	  }


	  public override void ToSource(StringBuilder sb)
        {
            sb.Append(IdentNewLine(comment));
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCommentStatement(this, data);
        }

	}
}
