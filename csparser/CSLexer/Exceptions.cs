using System;
using System.Diagnostics;

namespace DDW
{
    [Serializable]
    public class UnresolvableIdentifierException : Exception
    {
        private readonly string identifier;

        public UnresolvableIdentifierException(string identifier)
        {
            this.identifier = identifier;
        }

        public string Identifier
        {
            [DebuggerStepThrough]
            get
            {
                return identifier;
            }
        }
    }
}
