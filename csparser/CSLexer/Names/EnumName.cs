using DDW.Enums;

namespace DDW.Names
{
    public class EnumName : TypeName
    {
        /// <summary>
        /// Initializes a new instance of the EnumName class.
        /// </summary>
        public EnumName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
        }
    }
}
