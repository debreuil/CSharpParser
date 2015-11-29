using System.Text;

namespace DDW
{
	public class DecimalPrimitive : LiteralNode
	{
	  private readonly decimal value;

	  public DecimalPrimitive(decimal value, Token relatedToken)
	    : base(relatedToken)
	  {
	    this.value = value;
	  }

	  public decimal Value
		{
			get { return value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(value + " ");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDecimalPrimitive(this, data);
        }

	}
}
