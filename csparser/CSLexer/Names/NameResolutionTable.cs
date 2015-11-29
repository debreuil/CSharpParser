using System.Collections;
using System.Collections.Generic;

namespace DDW.Names
{
    public class NameResolutionTable : IEnumerable<IdentifierName>
    {
        private readonly Dictionary<string, List<IdentifierName>> names =
            new Dictionary<string, List<IdentifierName>>();

      #region IEnumerable<IdentifierName> Members

      IEnumerator<IdentifierName> IEnumerable<IdentifierName>.GetEnumerator()
        {
            foreach (KeyValuePair<string, List<IdentifierName>> pair in names)
            {
                foreach (IdentifierName identifierName in pair.Value)
                {
                    yield return identifierName;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IdentifierName>)this).GetEnumerator();
        }

      #endregion

      public void AddIdentifier(IdentifierName item)
      {
        if (!names.ContainsKey(item.FullyQualifiedName[0]))
        {
          names.Add(item.FullyQualifiedName[0], new List<IdentifierName>());
        }

        names[item.FullyQualifiedName[0]].Add(item);
      }

      public IEnumerable<T> GetMatches<T>(string name) where T : IdentifierName
      {
        if (names.ContainsKey(name))
        {
          foreach (IdentifierName identifierName in names[name])
          {
            if (identifierName is T)
            {
              yield return (T)identifierName;
            }
          }
        }
      }
    }
}
