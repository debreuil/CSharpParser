namespace DDW.GraphVisitor
{
  using Builders;

  /// <summary>
  /// C# visitor implementation.
  /// </summary>
  public sealed class CSharpVisitor : AbstractVisitor
  { 

    private readonly CSharpBuilder builder = new CSharpBuilder("\t");

    private string CompiledResult
    {
      get
      {
        return builder.ToString();
      }
    }

    public override object VisitCompilationUnit(CompilationUnitNode compilationUnit, object data)
    {
      base.VisitCompilationUnit(compilationUnit, data);
      return CompiledResult;
    }

    public override object VisitNamespaceDeclaration(NamespaceNode namespaceDeclaration, object data)
    {
      if (namespaceDeclaration.Name == null)
        return null;

      builder.AppendLine("namespace " + namespaceDeclaration.Name.GenericIdentifier);
      builder.BeginScope();
      {
        
      }
      builder.EndScope();
      return base.VisitNamespaceDeclaration(namespaceDeclaration, data);
    }

    public override object VisitCharPrimitive(CharPrimitive charPrimitive, object data)
    {
      builder.Append(charPrimitive.Value);
      return null;
    }

    public override object VisitIfStatement(IfStatement ifStatement, object data)
    {
      builder.Append("if (");
      {
        ifStatement.Test.AcceptVisitor(this, data);
      }
      builder.BeginScope(")");
      ifStatement.Statements.AcceptVisitor(this, data);
      builder.EndScope();

      ifStatement.ElseStatements.AcceptVisitor(this, data);

      return null;

    }
  }
}