using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public interface IType
	{
		List<int> RankSpecifiers
		{
			get;
			set;
		}
		void ToSource(StringBuilder sb);

        object AcceptVisitor(AbstractVisitor visitor, object data);
	}
}
