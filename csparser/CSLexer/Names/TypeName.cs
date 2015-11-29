using System.Diagnostics;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class TypeName : IdentifierName
    {
        private readonly string[] genericParameters;

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        protected TypeName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, context)
        {
            this.genericParameters = genericParameters;
        }

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        protected TypeName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
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
