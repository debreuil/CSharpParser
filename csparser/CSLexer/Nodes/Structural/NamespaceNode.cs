using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class NamespaceNode : BaseNode
	{
	  private NodeCollection<ClassNode> classes = new NodeCollection<ClassNode>();
	  private NodeCollection<DelegateNode> delegates = new NodeCollection<DelegateNode>();
	  private NodeCollection<EnumNode> enums = new NodeCollection<EnumNode>();
	  private NodeCollection<ExternAliasDirectiveNode> externAliases = new NodeCollection<ExternAliasDirectiveNode>();
	  private NodeCollection<InterfaceNode> interfaces = new NodeCollection<InterfaceNode>();
	  private QualifiedIdentifierExpression name;
	  private NodeCollection<NamespaceNode> namespaces;
	  private NodeCollection<StructNode> structs = new NodeCollection<StructNode>();

	  private NodeCollection<UsingDirectiveNode> usingDirectives = new NodeCollection<UsingDirectiveNode>();

	  public NamespaceNode(Token relatedToken) : base(relatedToken)
	  {
	  }

	  public NodeCollection<ExternAliasDirectiveNode> ExternAliases
	  {
	    [DebuggerStepThrough]
	    get { return externAliases; }
	    [DebuggerStepThrough]
	    set { externAliases = value; }
	  }

	  public NodeCollection<UsingDirectiveNode> UsingDirectives
        {
            get { return usingDirectives; }
            set { usingDirectives = value; }
        }

	  public NodeCollection<NamespaceNode> Namespaces
		{
			get { if (namespaces == null) namespaces = new NodeCollection<NamespaceNode>(); return namespaces; }
            [DebuggerStepThrough]
            set { namespaces = value; }
		}

	  public NodeCollection<ClassNode> Classes
		{
            [DebuggerStepThrough]
            get { return classes; }
            [DebuggerStepThrough]
            set { classes = value; }
		}

	  public NodeCollection<EnumNode> Enums
		{
            [DebuggerStepThrough]
            get { return enums; }
            [DebuggerStepThrough]
            set { enums = value; }
		}

	  public NodeCollection<DelegateNode> Delegates
		{
            [DebuggerStepThrough]
            get { return delegates; }
            [DebuggerStepThrough]
            set { delegates = value; }
		}

	  public NodeCollection<InterfaceNode> Interfaces
		{
            [DebuggerStepThrough]
            get { return interfaces; }
            [DebuggerStepThrough]
            set { interfaces = value; }
		}

	  public NodeCollection<StructNode> Structs
		{
            [DebuggerStepThrough]
            get { return structs; }
            [DebuggerStepThrough]
            set { structs = value; }
		}


	  public QualifiedIdentifierExpression Name
		{
            [DebuggerStepThrough]
            get { return name; }
            [DebuggerStepThrough]
            set { name = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

			if (name != null)
			{
				if(attributes != null)
					attributes.ToSource(sb);

				sb.Append("namespace ");
				name.ToSource(sb);
				NewLine(sb);
				sb.Append("{");
				indent++;
				NewLine(sb);
			}

            if (ExternAliases.Count > 0)
            {
				ExternAliases.ToSource(sb);
                NewLine(sb);
            }

            if (UsingDirectives.Count > 0)
            {
				UsingDirectives.ToSource(sb);
				NewLine(sb);
			}

			if(name == null)
			{
				if(attributes != null)
					attributes.ToSource(sb);
			}

			if (namespaces != null)
				namespaces.ToSource(sb);

			if(interfaces != null)
				interfaces.ToSource(sb);

			if (classes != null)
				classes.ToSource(sb);

			if (structs != null)
				structs.ToSource(sb);

			if (delegates != null)
				delegates.ToSource(sb);

			if (enums != null)
				enums.ToSource(sb);

			if (name != null)
			{
				indent--;
				NewLine(sb);
				sb.Append("}");
				NewLine(sb);
			}
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (canEnterContext && name != null)
                resolver.Context.Enter(name.QualifiedIdentifier, true);

            foreach (UsingDirectiveNode node in usingDirectives)
            {
                node.Parent = this;
                resolver.Context.AddUsingDirective(node);
            }

            foreach (ExternAliasDirectiveNode aliasDirectiveNode in ExternAliases)
            {
                // TODO: Collect
                aliasDirectiveNode.Parent = this;
                aliasDirectiveNode.Resolve(resolver, false);
            }

            foreach (NamespaceNode namespaceNode in Namespaces)
            {
                namespaceNode.Parent = this;
                namespaceNode.Resolve(resolver);
            }

            foreach (ClassNode classNode in Classes)
            {
                classNode.Parent = this;
                classNode.Resolve(resolver);
            }

            foreach (DelegateNode delegateNode in Delegates)
            {
                delegateNode.Parent = this;
                delegateNode.Resolve(resolver);
            }

            foreach (EnumNode enumNode in Enums)
            {
                enumNode.Parent = this;
                enumNode.Resolve(resolver);
            }

            foreach (InterfaceNode interfaceNode in Interfaces)
            {
                interfaceNode.Parent = this;
                interfaceNode.Resolve(resolver);
            }

            foreach (StructNode structNode in Structs)
            {
                structNode.Parent = this;
                structNode.Resolve(resolver);
            }

            if (canEnterContext && name != null)
                resolver.Context.Leave();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitNamespaceDeclaration(this, data);
        }
    }
}
