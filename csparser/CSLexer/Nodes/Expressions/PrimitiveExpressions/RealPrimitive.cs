using System.Globalization;
using System.Text;

namespace DDW
{
	// todo: need to add ValueAsDecimal, ValueAsDouble, and ValueAsFloat, then store a string and 'kind'.
	public class RealPrimitive : LiteralNode
	{
	  private readonly double val;
	  private bool isDecimal;
	  private bool isDouble;
	  private bool isFloat;
	  private string originalString;
        public RealPrimitive(string value, Token relatedToken)
            : base(relatedToken)
		{
			originalString = value;
			char c = value[value.Length - 1];
			switch(c)
			{
				case 'f':
				case 'F':
					isFloat = true;
					value = value.Substring(0, value.Length - 1);
					val = float.Parse(value, CultureInfo.InvariantCulture);
					break;
				case 'd':
				case 'D':
					isDouble = true;
					value = value.Substring(0, value.Length - 1);
					val = double.Parse(value, CultureInfo.InvariantCulture);
					break;
				case 'm':
				case 'M':
					isDecimal = true;
					value = value.Substring(0, value.Length - 1);
					val = (double)decimal.Parse(value, CultureInfo.InvariantCulture);
					break;
				default:
					val = double.Parse(value, CultureInfo.InvariantCulture);
					break;
			}
		}
        public RealPrimitive(double value, Token relatedToken)
            : base(relatedToken)
		{
			isDouble = true;
			val = value;
		}

	  public double Value
		{
			get { return val; }
		}

	  public bool IsFloat
		{
			get { return isFloat; }
			set { isFloat = value; }
		}

	  public bool IsDouble
		{
			get { return isDouble; }
			set { isDouble = value; }
		}

	  public bool IsDecimal
		{
			get { return isDecimal; }
			set { isDecimal = value; }
		}
	

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(val.ToString( CultureInfo.InvariantCulture) );
			if (isFloat)
			{
				sb.Append("f");
			}
			else if (isDouble)
			{
				sb.Append("d");
			}
			else if (isDecimal)
			{
				sb.Append("m");
			}
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitRealPrimitive(this, data);
        }

	}
}
