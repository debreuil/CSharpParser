using System.Text;

namespace DDW
{
	public interface ISourceCode
	{
		/// <summary>
		/// Returns the source code representation of the node.
		/// </summary>
		/// <returns>Returns the source code representation of the node.</returns>
		void ToSource(StringBuilder sb);


        object AcceptVisitor(AbstractVisitor visitor, object data);
	}
}
