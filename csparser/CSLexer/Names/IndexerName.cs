using DDW.Enums;

namespace DDW.Names
{
    // TODO: Omer - the default name is Items, but System.Runtime.CompilerServices.IndexerNameAttribute can change that.

    public class IndexerName : PropertyName
    {
        public IndexerName(NameVisibilityRestriction getVisibility, NameVisibilityRestriction setVisibility,
                           Scope scope, PropertyAccessors accessors, Context context)
            : base("Items", getVisibility, setVisibility, scope, accessors, context)
        {
        }
    }
}