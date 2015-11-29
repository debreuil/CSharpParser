using System;

namespace DDW.Enums
{
    [Flags]
    public enum PropertyAccessors
    {
        Get  = 0x01,
        Set  = 0x02,
        Both = Get | Set
    }
}
