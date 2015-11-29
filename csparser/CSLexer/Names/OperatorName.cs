using DDW.Enums;

namespace DDW.Names
{
    public class OperatorName : TypeMemberName
    {
        public OperatorName(TokenID token, Context context)
            : base(token.ToString(), NameVisibilityRestriction.Everyone, Scope.Static, context)
        {
        }
    }
}