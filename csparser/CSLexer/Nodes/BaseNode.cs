using System;
using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public abstract class BaseNode : ISourceCode
	{
      private const string tabChar = "    ";

		protected static int indent;
		protected NodeCollection<AttributeNode> attributes = new NodeCollection<AttributeNode>();
      private string comment = string.Empty;
      private string docComment = string.Empty;
      public  Token RelatedToken;

      private BaseNode()
        {
        }

        public BaseNode(Token relatedToken)
        {
            RelatedToken = relatedToken;
        }

      public string DocComment
      {
        get
        {
          return docComment;
        }
        set
        {
          docComment = value;
        }
      }

      public string Comment
      {
        get
        {
          return comment;
        }
        set
        {
          comment = value;
        }
      }

      public NodeCollection<AttributeNode> Attributes
      {
        get { return attributes; }
        set { attributes = value; }
      }

      public BaseNode Parent { [DebuggerStepThrough]
      get; [DebuggerStepThrough]
      internal set; }

      #region ISourceCode Members

      /// <summary>
		/// Returns the source code representation of the node.
		/// </summary>
		/// <returns>Returns the source code representation of the node.</returns>
        public abstract void ToSource(StringBuilder sb);

      public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
      {
        throw new NotImplementedException("please implement the ISourceCode.AcceptVisitor member");
      }

      #endregion

      public static void NewLine(StringBuilder sb)
		{
			sb.Append(Environment.NewLine);
			for(int i = 0; i < indent; i++)
			{
				sb.Append(tabChar);
			}
		}

        protected string IdentNewLine(string str)
        {
            StringBuilder indentation = new StringBuilder();

            indentation.Append(Environment.NewLine);

            for (int i = 0; i < indent; i++)
            {
                indentation.Append(tabChar);
            }

            return str.Replace(Environment.NewLine, indentation.ToString());
        }

		protected void AddTab(StringBuilder sb)
		{
			sb.Append(tabChar);
		}

		protected void TraceDottedIdent(string[] target, StringBuilder sb)
		{
			bool isFirst = true;
			foreach (string s in target)
			{
				if(isFirst) isFirst = false;
				else sb.Append('.');
				sb.Append(s);
			}
		}

		protected void TraceModifiers(Modifier modifiers, StringBuilder sb)
		{
			if ((modifiers & Modifier.New) == Modifier.New)
			{
				sb.Append("new ");
			}
			if ((modifiers & Modifier.Public) == Modifier.Public)
			{
				sb.Append("public ");
			}
			if ((modifiers & Modifier.Protected) == Modifier.Protected)
			{
				sb.Append("protected ");
			}
			if ((modifiers & Modifier.Internal) == Modifier.Internal)
			{
				sb.Append("internal ");
			}
			if ((modifiers & Modifier.Private) == Modifier.Private)
			{
				sb.Append("private ");
			}
			if ((modifiers & Modifier.Abstract) == Modifier.Abstract)
			{
				sb.Append("abstract ");
			}
			if ((modifiers & Modifier.Sealed) == Modifier.Sealed)
			{
				sb.Append("sealed ");
			}
			if ((modifiers & Modifier.Static) == Modifier.Static)
			{
				sb.Append("static ");
			}
			if ((modifiers & Modifier.Virtual) == Modifier.Virtual)
			{
				sb.Append("virtual ");
			}
			if ((modifiers & Modifier.Override) == Modifier.Override)
			{
				sb.Append("override ");
			}
			if ((modifiers & Modifier.Extern) == Modifier.Extern)
			{
				sb.Append("extern ");
			}
			if ((modifiers & Modifier.Readonly) == Modifier.Readonly)
			{
				sb.Append("readonly ");
			}
			if ((modifiers & Modifier.Volatile) == Modifier.Volatile)
			{
				sb.Append("volatile ");
			}
			if ((modifiers & Modifier.Ref) == Modifier.Ref)
			{
				sb.Append("ref ");
			}
			if ((modifiers & Modifier.Out) == Modifier.Out)
			{
				sb.Append("out ");
			}
			if ((modifiers & Modifier.Params) == Modifier.Params)
			{
				sb.Append("params ");
			}
			if ((modifiers & Modifier.Assembly) == Modifier.Assembly)
			{
				sb.Append("assembly ");
			}
			if ((modifiers & Modifier.Field) == Modifier.Field)
			{
				sb.Append("field ");
			}
			if ((modifiers & Modifier.Event) == Modifier.Event)
			{
				sb.Append("event ");
			}
			if ((modifiers & Modifier.Method) == Modifier.Method)
			{
				sb.Append("method ");
			}
			if ((modifiers & Modifier.Param) == Modifier.Param)
			{
				sb.Append("param ");
			}
			if ((modifiers & Modifier.Property) == Modifier.Property)
			{
				sb.Append("property ");
			}
			if ((modifiers & Modifier.Return) == Modifier.Return)
			{
				sb.Append("return ");
			}
			if ((modifiers & Modifier.Type) == Modifier.Type)
			{
				sb.Append("type ");
			}
			if ((modifiers & Modifier.Module) == Modifier.Module)
			{
				sb.Append("module ");
			}

            if ((modifiers & Modifier.Partial) == Modifier.Partial)
            {
                sb.Append("partial ");
            }

            if ((modifiers & Modifier.Unsafe) == Modifier.Unsafe)
            {
                sb.Append("unsafe ");
            }

            if ((modifiers & Modifier.Fixed) == Modifier.Fixed)
            {
                sb.Append("fixed ");
            }
		}

      protected internal void Resolve(IResolver resolver)
        {
            Resolve(resolver, true);
        }

        protected internal virtual void Resolve(IResolver resolver, bool canEnterContext)
        {
            foreach (AttributeNode attributeNode in Attributes)
            {
                attributeNode.Parent = this;
                attributeNode.Resolve(resolver);
            }
        }
	}
}
