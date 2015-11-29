using System.Text;

namespace DDW
{
	public class BooleanPrimitive : LiteralNode
	{
	  private readonly bool value;

	  public BooleanPrimitive(bool value, Token relatedToken) : base(relatedToken)
	  {
	    this.value = value;
	  }

	  public bool Value
		{
			get { return value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(value.ToString().ToLower());
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBooleanPrimitive(this, data);
        }

	}
}
