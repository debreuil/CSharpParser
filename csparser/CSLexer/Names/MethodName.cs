using System.Diagnostics;
using DDW.Enums;

namespace DDW.Names
{
    public class MethodName : TypeMemberName
    {
        private readonly string[] genericParameters;

        /// <summary>
        /// Initializes a new instance of the MethodName class.
        /// </summary>
        public MethodName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Scope scope, Context context)
            : base(name, visibility, scope, context)
        {
            this.genericParameters = genericParameters;
        }

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        public MethodName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, scope, context)
        {
            genericParameters = new string[0];
        }

        public string[] GenericParameters
        {
            [DebuggerStepThrough]
            get
            {
                return genericParameters;
            }
        }
    }
}
