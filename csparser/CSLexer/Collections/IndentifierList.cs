using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
	public class IdentifierList : ExpressionNode, IEquatable<IdentifierList>
	{
        public IdentifierList()
		{
		}

        private LinkedList<IdentifierExpression> identifiers = new LinkedList<IdentifierExpression>();
        public LinkedList<IdentifierExpression> Identifiers
		{
            get { return identifiers; }
		}

        public bool Equals(IdentifierList other)
        {
            bool ret = false;

            if (this == other)
            {
                ret = true;
            }
            else
            {
                if (other != null)
                {
                    if (this.identifiers == null && other.identifiers == null
                        || identifiers != null && other.identifiers != null)
                    {
                        if (identifiers != null)
                        {
                            if (QualifiedIdentifierString == other.QualifiedIdentifierString)
                            {
                                ret = false;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public string QualifiedIdentifierString
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                LinkedListNode<IdentifierExpression> node = identifiers.First;

                while (node != null)
                {
                    node.Value.ToSource(sb);
                    sb.Append(".");
                    node = node.Next;
                }

                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }

        }

        public string IndependentUniqueIdentifier
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                LinkedListNode<IdentifierExpression> node = identifiers.First;

                while (node != null)
                {
                    sb.Append(node.Value.IndependentQualifiedIdentifier);
                    sb.Append(".");
                    node = node.Next;
                }

                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }

        }

		public override void ToSource(StringBuilder sb)
		{
            sb.Append(QualifiedIdentifierString);   
		}

	}
}
