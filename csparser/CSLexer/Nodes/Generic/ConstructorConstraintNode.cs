using System.Text;

namespace DDW
{
    /// <summary>
    /// constructor-constraint:
    ///     new   (   )
    /// </summary>
	public sealed class ConstructorConstraint : ISourceCode
	{
      #region ISourceCode Members

      public void ToSource(StringBuilder sb)
		{
            sb.Append("new() ");
		}

        public object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstructorConstraint(this, data);
        }

      #endregion
	}
}
