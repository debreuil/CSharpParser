using System.Diagnostics;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class TypeMemberName : IdentifierName
    {
        private readonly Scope scope;

        /// <summary>
        /// Initializes a new instance of the TypeMemberName class.
        /// </summary>
        public TypeMemberName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, context)
        {
            this.scope = scope;
        }

        public Scope Scope
        {
            [DebuggerStepThrough]
            get
            {
                return scope;
            }
        }
    }
}
