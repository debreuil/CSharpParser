using DDW.Collections;

namespace DDW
{
    /// <summary>
    /// this is used by partial type.
    /// keep in mind that only the first declarated partial part
    /// see by the parser handles his partial list ( all other
    /// partial have a <see cref="Partials"/> set to null );
    /// </summary>
	public interface IPartial
	{
        /// <summary>
        /// This property stores all other partial class declaration
        /// </summary>
        PartialCollection Partials
        {
            get;
            set;
        }

        bool IsPartial
        {
            get;
        }

        object AcceptVisitor(AbstractVisitor visitor, object data);
	}
}
