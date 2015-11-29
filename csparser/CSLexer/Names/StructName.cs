using DDW.Enums;

namespace DDW.Names
{
    public class StructName : TypeName
    {
        /// <summary>
        /// Initializes a new instance of the StructName class.
        /// </summary>
        public StructName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, genericParameters, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the StructName class.
        /// </summary>
        public StructName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
        }
    }
}
