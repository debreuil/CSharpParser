using System;
using System.Diagnostics;

namespace DDW
{
    public class IdentifiedLocalTypeNode : IdentifiedTypeNode
    {
        private readonly string fullyQualifiedName;

        /// <summary>
        /// Initializes a new instance of the IdentifiedLocalTypeNode class.
        /// </summary>

        public IdentifiedLocalTypeNode(string fullyQualifiedName, Token relatedToken): base(relatedToken)
        {
            this.fullyQualifiedName = fullyQualifiedName;
        }

        public string FullyQualifiedName
        {
            [DebuggerStepThrough]
            get
            {
                return fullyQualifiedName;
            }
        }
    }

    public class IdentifiedExternalTypeNode : IdentifiedTypeNode
    {
        private readonly Type type;

        /// <summary>
        /// Initializes a new instance of the IdentifiedExternalTypeNode class.
        /// </summary>
        public IdentifiedExternalTypeNode(Type type, Token relatedToken): base(relatedToken)
        {
            this.type = type;
        }

        public Type Type
        {
            [DebuggerStepThrough]
            get
            {
                return type;
            }
        }
    }

    public abstract class IdentifiedTypeNode : TypeNode
    {
        public IdentifiedTypeNode(Token relatedToken)
            : base(relatedToken)
        {
        }
    }
}
