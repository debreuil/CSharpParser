namespace DDW
{
    /// <summary>
    /// This interface is used by method, operator and accessor to notify the system 
    /// they are iterator body
    /// 
    /// A class which inherits from the IITerator interface could be iterator ad it could be not iterator.
    /// It depends on it return type.
    /// </summary>    /// </summary>
	public interface IIterator
	{
        /// <summary>
        /// This property means that the Node could be an iterator : 
        /// 
        /// If his return type is nor not IEnumerator nor IEnumerable, it can not be an iterator.
        ///     -> <c>IsCouldBeIterator</c> = <c>false</c>
        ///     -> each yield in his block will raise an error
        /// 
        /// If his return type is IEnumerator or IEnumerable, it can be an iterator.
        ///     -> <c>IsCouldBeIterator</c> = <c>true</c>
        ///     -> yield are allowed in his block.
        ///     -> if it has at least one yield, it is an iterator
        ///     -> if it has no yield in his block, it is not an iterator
        /// </summary>
        bool CouldBeIterator
        {
            get;
        }

        /// <summary>
        /// this property return <c>true</c> when the node has at least one yield field in his block <c>AND</c>
        /// his property <see cref="IsCouldBeIterator"/> is set to <c>true</c>.
        /// if <see cref="IsCouldBeIterator"/> is set to <c>false</c>, this property will always returns <c>false</c>
        /// </summary>
        bool IsIterator
        {
            get;
            set;
        }
	}
}
