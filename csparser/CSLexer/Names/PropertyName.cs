using System.Diagnostics;
using DDW.Enums;

namespace DDW.Names
{
    public class PropertyName : TypeMemberName
    {
        private readonly PropertyAccessors accessors;
        private readonly NameVisibilityRestriction setVisibility;

        /// <summary>
        /// Initializes a new instance of the PropertyName class.
        /// </summary>
        public PropertyName(string name, 
            NameVisibilityRestriction getVisibility, NameVisibilityRestriction setVisibility, 
            Scope scope, PropertyAccessors accessors, Context context)
            : base(name, getVisibility, scope, context)
        {
            this.accessors = accessors;
            this.setVisibility = setVisibility;
        }

        public PropertyAccessors Accessors
        {
            [DebuggerStepThrough]
            get
            {
                return accessors;
            }
        }

        public NameVisibilityRestriction SetVisibility
        {
            [DebuggerStepThrough]
            get
            {
                return setVisibility;
            }
        }
   }
}
