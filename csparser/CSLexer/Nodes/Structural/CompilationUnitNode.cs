using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DDW.Collections;
using DDW.Names;

namespace DDW
{
  public class CompilationUnitNode : BaseNode
  {
    private readonly NamespaceNode defaultNamespace;
    private readonly Dictionary<string, PreprocessorID> ppDefs = new Dictionary<string, PreprocessorID>();
    private NodeCollection<NamespaceNode> namespaces;

    public CompilationUnitNode()
      : base(new Token(TokenID.Invalid, 0, 0))
    {
      namespaces = new NodeCollection<NamespaceNode>();

      defaultNamespace = new NamespaceNode(RelatedToken);
    }

    public Dictionary<string, PreprocessorID> PPDefs
    {
      [DebuggerStepThrough]
      get { return ppDefs; }
    }

    public NodeCollection<NamespaceNode> Namespaces
    {
      [DebuggerStepThrough]
      get { return namespaces; }
      [DebuggerStepThrough]
      set { namespaces = value; }
    }

    public NamespaceNode DefaultNamespace
    {
      [DebuggerStepThrough]
      get { return defaultNamespace; }
    }


    public NameResolutionTable NameTable
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      set;
    }

    public override void ToSource(StringBuilder sb)
    {
      foreach (String def in ppDefs.Keys)
      {
        sb.Append("#define ");
        sb.Append(def);
        sb.Append(Environment.NewLine);
      }

      if (attributes != null)
        attributes.ToSource(sb);

      defaultNamespace.ToSource(sb);
      namespaces.ToSource(sb);
    }

    protected internal override void Resolve(IResolver resolver, bool canEnterContext)
    {
      base.Resolve(resolver, false);

      DefaultNamespace.Parent = this;
      DefaultNamespace.Resolve(resolver);

      foreach (NamespaceNode namespaceNode in Namespaces)
      {
        namespaceNode.Parent = this;
        namespaceNode.Resolve(resolver);
      }
    }

    public override object AcceptVisitor(AbstractVisitor visitor, object data)
    {
      return visitor.VisitCompilationUnit(this, data);
    }

  }
}
