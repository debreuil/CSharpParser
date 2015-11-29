using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DDW
{
	public class PredefinedTypeNode : TypeNode
	{
	    private static readonly Dictionary<TokenID, Type> types
	        = new Dictionary<TokenID, Type>();

	  private readonly TokenID type;

	  static PredefinedTypeNode()
	  {
	    types.Add(TokenID.Bool, typeof(bool));
	    types.Add(TokenID.Byte, typeof(byte));
	    types.Add(TokenID.Char, typeof(char));
	    types.Add(TokenID.Decimal, typeof(decimal));
	    types.Add(TokenID.Double, typeof(double));
	    types.Add(TokenID.Float, typeof(float));
	    types.Add(TokenID.Int, typeof(int));
	    types.Add(TokenID.Long, typeof(long));
	    types.Add(TokenID.Object, typeof(object));
	    types.Add(TokenID.SByte, typeof(sbyte));
	    types.Add(TokenID.Short, typeof(short));
	    types.Add(TokenID.String, typeof(string));
	    types.Add(TokenID.UInt, typeof(uint));
	    types.Add(TokenID.ULong, typeof(ulong));
	    types.Add(TokenID.UShort, typeof(ushort));
	    types.Add(TokenID.Void, typeof(void));
	  }

	  public PredefinedTypeNode(Token relatedToken)
            : base(relatedToken)
        {
        }
	    public PredefinedTypeNode(TokenID type, Token relatedToken) 
            : base( new IdentifierExpression(type.ToString().ToLower(), relatedToken) )
		{
	        this.type = type;
		}

        public TokenID Type
		{
            [DebuggerStepThrough]
			get
            {
                return type;
            }
		}

        public override GenericNode Generic
        {
            [DebuggerStepThrough]
            get
            {
                return null;
            }
            [DebuggerStepThrough]
            set
            {
            }
        }

	    public override bool IsGeneric
	    {
            [DebuggerStepThrough]
            get
	        {
	            return false;
	        }
	    }

        public Type GetRealType()
        {
            return types[type];
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPredefinedTypeReference(this, data);
        }
	}
}
