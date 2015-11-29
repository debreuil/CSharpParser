using DDW.Enums;

namespace DDW.Names
{
    public class InterfaceName : TypeName
    {
        /// <summary>
        /// Initializes a new instance of the InterfaceName class.
        /// </summary>
        public InterfaceName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, genericParameters, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InterfaceName class.
        /// </summary>
        public InterfaceName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
        }
    }
}
