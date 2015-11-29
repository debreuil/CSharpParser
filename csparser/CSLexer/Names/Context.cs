using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DDW.Names
{
    public class Context
    {
      #region Internal implementation

      #region Nested type: Level

      private abstract class Level
      {
        private readonly string name;

        protected Level(string name)
        {
          this.name = name;
        }

        public string Name
        {
          [DebuggerStepThrough]
          get
          {
            return name;
          }
        }
      }

      #endregion

      #region Nested type: NamespaceLevel

      private class NamespaceLevel : Level
      {
        public static readonly NamespaceLevel Root = new NamespaceLevel(string.Empty);

        private List<UsingDirectiveNode> usingDirectives;

        public NamespaceLevel(string name)
          : base(name)
        {
        }

        public List<UsingDirectiveNode> UsingDirectives
        {
          [DebuggerStepThrough]
          get
          {
            if (usingDirectives == null)
            {
              usingDirectives = new List<UsingDirectiveNode>();
            }

            return usingDirectives;
          }
        }
      }

      #endregion

      #region Nested type: TypeLevel

      private class TypeLevel : Level
      {
        public TypeLevel(string name)
          : base(name)
        {
        }
      }

      #endregion

      #endregion

      private readonly Dictionary<String, object> couldBeAlias = new Dictionary<string, object>();
      private readonly Stack<Level> levels = new Stack<Level>();

      /// <summary>
        /// Initializes a new instance of the Context class.
        /// </summary>
        public Context()
        {
            levels.Push(NamespaceLevel.Root);
        }

        public void Enter(string name, bool isNamespace)
        {
            if (isNamespace)
            {
                levels.Push(new NamespaceLevel(name));
            }
            else
            {
                levels.Push(new TypeLevel(name));
            }
        }

        /// <summary>
        /// Adds a using directive for the current context.
        /// </summary>
        public void AddUsingDirective(UsingDirectiveNode node)
        {
            NamespaceLevel currentLevel = levels.Peek() as NamespaceLevel;

            if (currentLevel == null)
                throw new InvalidOperationException("Can not add a using directive in a type.");

            currentLevel.UsingDirectives.Add(node);
            if(node.IsAlias)
                couldBeAlias[node.AliasName.Identifier] = null;
        }

        public void Leave()
        {
            levels.Pop();
        }

        public string[] GetContext()
        {
            return GetContext(false);
        }

        public string[] GetContext(bool ignoreTypes)
        {
            Level[] levelsArray = levels.ToArray();
            string[] names = null;

            if (ignoreTypes)
            {
                for (int i = levelsArray.Length - 1; i >= 0; i--)
                {
                    if (levelsArray[i] is NamespaceLevel)
                    {
                        names = new string[i + 1];
                        break;
                    }
                }

                if (names == null)
                    names = new string[0];
            }
            else
            {
                names = new string[levelsArray.Length - 1];
            }

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = levelsArray[i].Name;
            }

            return names;
        }

        public IEnumerable<UsingDirectiveNode> GetAllUsingDirectives()
        {
            foreach(Level currentLevel in levels)
            {
                if (currentLevel is NamespaceLevel)
                {
                    foreach (UsingDirectiveNode node in ((NamespaceLevel)(currentLevel)).UsingDirectives)
                    {
                        yield return node;
                    }
                }
            }
        }

		/// <summary>
		/// Gets the target of a given alias name.
		/// </summary>
		/// <param name="alias">The name of the alias</param>
		/// <returns>The target of the alias, or null, if such an alias does not exist.</returns>
		public PrimaryExpression GetAliasTarget(String alias)
		{
			if(!couldBeAlias.ContainsKey(alias)) return null;

			// Could be an alias, so check, if it really is one in the current context.
			foreach(UsingDirectiveNode usingDir in GetAllUsingDirectives())
			{
				if(usingDir.IsAlias && usingDir.AliasName.Identifier == alias)
					return usingDir.Target;
			}
			return null;
		}
    }
}
