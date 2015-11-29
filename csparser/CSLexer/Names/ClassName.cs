using DDW.Enums;

namespace DDW.Names
{
    public class ClassName : TypeName
    {
        /// <summary>
        /// Initializes a new instance of the ClassName class.
        /// </summary>
        public ClassName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, genericParameters, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ClassName class.
        /// </summary>
        public ClassName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
        }
    }
}
