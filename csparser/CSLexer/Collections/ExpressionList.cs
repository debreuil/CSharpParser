using System;
using System.Text;

namespace DDW.Collections
{
    public class ExpressionList : NodeCollection<ExpressionNode>
    {
        public ExpressionNode Last
        {
            get
            {
                return Count > 0 ? base[Count - 1] : null;
            }
        }

        public override void ToSource(StringBuilder sb)
        {
            base.ToSource(sb, ", ");
        }
    }
}
