using DDW.Enums;

namespace DDW.Names
{
    public class EventName : TypeMemberName
    {
        /// <summary>
        /// Initializes a new instance of the EventName class.
        /// </summary>
        public EventName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, scope, context)
        {
        }
    }
}
