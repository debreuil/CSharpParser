using System;
using System.Diagnostics;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class IdentifierName
    {
      private readonly NameVisibilityRestriction visibility;
      private string[] fullyQualifiedName;

      /// <summary>
        /// Initializes a new instance of the Identifier class.
        /// </summary>
        protected IdentifierName(string name, NameVisibilityRestriction visibility, Context context)
        {
            string[] curContext = context.GetContext();

            fullyQualifiedName = new string[curContext.Length + 1];
            fullyQualifiedName[0] = name;
            Array.Copy(curContext, 0, fullyQualifiedName, 1, curContext.Length);

            this.visibility = visibility;
        }

        /// <summary>
        /// Fully qualified name, in reverse order.
        /// e.g.: DDW.Names.Identifier will be saved as {"Identifier", "Names", "DDW"}
        /// </summary>
        public string[] FullyQualifiedName
        {
            [DebuggerStepThrough]
            get
            {
                return fullyQualifiedName;
            }
            [DebuggerStepThrough]
            protected internal set
            {
                fullyQualifiedName = value;
            }
        }

        /// <summary>
        /// Who is allowed to see this identifier?
        /// </summary>
        public NameVisibilityRestriction Visibility
        {
            [DebuggerStepThrough]
            get
            {
                return visibility;
            }
        }

      public IdentifierName Parent { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      set; }
    }
}
