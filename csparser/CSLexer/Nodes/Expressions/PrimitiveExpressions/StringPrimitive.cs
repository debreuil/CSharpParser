using System.Text;

namespace DDW
{
	public class StringPrimitive : LiteralNode
	{
	  private readonly string value;
	  private bool isVerbatim; // strings are always lexed as non-verbatim for now

	  public StringPrimitive(string value, Token relatedToken)
	    : base(relatedToken)
	  {
	    this.value = value;
	  }

	  public string Value
	  {
	    get { return value; }
	  }

	  public bool IsVerbatim
		{
			get { return isVerbatim; }
			set { isVerbatim = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if (value != null)
			{
				if (isVerbatim)
				{
					sb.Append("@");
				}
				sb.Append("\"" + value + "\"");
			}
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStringPrimitive(this, data);
        }

	}    
}
