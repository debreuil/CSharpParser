using System;
using System.Text;

namespace DDW
{
	public class CharPrimitive : LiteralNode
	{
	  /// <summary>
	  /// this property is not a <c>char</c> but a <c>string</c> to be able to parse  
	  /// any characters starting with a '\'.
	  /// </summary>
	  private readonly string value;

	  public CharPrimitive(char value, Token relatedToken) : base(relatedToken)
		{
			this.value = value.ToString();
		}
        public CharPrimitive(string value, Token relatedToken)
            : base(relatedToken)
		{
			if (value.Length == 1
                || value.Length == 2 && value.StartsWith("\\") )
			{
				this.value = value;
			}
			else
			{
				throw new FormatException("char primitive value is not a char");
			}
		}

	  public string Value
		{
			get { return value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("'" + value + "'");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCharPrimitive(this, data);
        }

	}
}
