using DDW.Enums;

namespace DDW.Names
{
    public class DelegateName : TypeName
    {
        public DelegateName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
        }

        public DelegateName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, genericParameters, context)
        {
        }
    }
}
