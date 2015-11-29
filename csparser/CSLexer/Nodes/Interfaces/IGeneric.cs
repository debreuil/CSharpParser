namespace DDW
{
    /// <summary>
    /// this interface is used by all constructed type.
    /// If a node inherits from this interface, it does not means that 
    /// the type is a Generic type. It means that the type could be generic.
    /// To test if the type is generic, lookat the property <see cref="IsGeneric"/>
    /// </summary>
	public interface IGeneric
	{
        GenericNode Generic
        {
            get;
            set;
        }

        bool IsGeneric
        {
            get;
        }

        /// <summary>
        /// in generic type, the type name is not enough to identify a type.
        /// i.e. : 
        /// a generic class A
        ///     A<x>
        ///  and A<x,y> are differents 
        /// so the GenericIdentifier will generate the name string with type parameter
        /// </summary>
        string GenericIdentifier
        {
            get;
        }

        /// <summary>
        /// Same as <see cref="GenericIdentifier"/> but type parameter name are not 
        /// serialized -> genric wil lbe output like A<,,,>
        /// 
        /// Used to check that two generic type declaration are intrasically different : 
        /// 
        /// x<T,V> is same as x<B,U>.
        /// </summary>
        string GenericIndependentIdentifier
        {
            get;
        }
	}
}
