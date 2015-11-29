using DDW.Enums;

namespace DDW.Names
{
    public class FieldName : TypeMemberName
    {
        /// <summary>
        /// Initializes a new instance of the FieldName class.
        /// </summary>
        public FieldName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, scope, context)
        {
        }
    }
}
