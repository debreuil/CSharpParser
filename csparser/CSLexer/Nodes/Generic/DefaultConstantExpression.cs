using System.Text;

namespace DDW
{
    /// <summary>
    /// To handle the default assignment of a type parameter in generic declaration
    /// i.e. : object x = default(T)
	/// But also for any other type, e.g. default(int)
    /// </summary>
	public class DefaultConstantExpression : PrimaryExpression
	{
		private TypeNode type;

      public DefaultConstantExpression(TypeNode type)
            : base(type.RelatedToken)
		{
            this.type = type;
		}

      public TypeNode Type
      {
        get
        {
          return type;
        }
        set
        {
          type = value;
        }
      }

      public override void ToSource(StringBuilder sb)
		{
            sb.Append("default(");
            type.ToSource(sb);
            sb.Append(")");
        }
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDefaultValueExpression(this, data);
        }
	}
}
