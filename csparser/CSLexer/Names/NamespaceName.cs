using DDW.Enums;

namespace DDW.Names
{
    public class NamespaceName : IdentifierName
    {
        /// <summary>
        /// Initializes a new instance of the NamespaceName class.
        /// </summary>
        public NamespaceName(string name, Context context)
            : base(name, NameVisibilityRestriction.Everyone, context)
        {
        }
    }
}
