namespace DDW
{
    /// <summary>
    /// Interface for node which might unsafe ( block, member, types ... )
    /// </summary>  
	public interface IUnsafe
	{
        bool IsUnsafe
        {
            get;
        }

        bool IsUnsafeDeclared
        {
            get;
        }
	}
}
