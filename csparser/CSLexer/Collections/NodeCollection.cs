using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
  public class NodeCollection<T> : List<T> where T : ISourceCode
  {
    /// <summary>
    /// Adds the given item to the end of the NodeCollection, if it is not null.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public new void Add(T item)
    {
      if (item != null)
        base.Add(item);
    }

    public virtual void ToSource(StringBuilder sb)
    {
      foreach (T node in this)
      {
        node.ToSource(sb);
      }
    }

    public virtual void ToSource(StringBuilder sb, String delimiter)
    {
      bool isFirst = true;
      foreach (T node in this)
      {
        if (isFirst) isFirst = false;
        else sb.Append(delimiter);
        node.ToSource(sb);
      }
    }

    public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
    {
      foreach (T node in this)
      {
        node.AcceptVisitor(visitor, data);
      }

      return null;
    }
  }
}
