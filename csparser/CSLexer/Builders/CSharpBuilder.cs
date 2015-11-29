/* Copyright (C) Dmitri Nesteruk */

namespace DDW.Builders
{
  /// <summary>
  /// This is a <c>StringBuilder</c>-based class which builds C# source code.
  /// Unlike CodeDOM, it has no problems with keywords such as <c>readonly</c>.
  /// </summary>
  public sealed class CSharpBuilder : CodeBuilder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpBuilder"/> class.
    /// </summary>
    /// <param name="indentString">The indent string.</param>
    public CSharpBuilder(string indentString) : base(indentString)
    {
    }

    /// <summary>
    /// Begins the scope.
    /// </summary>
    /// <returns></returns>
    public CSharpBuilder BeginScope()
    {
      AppendLine("{").Indent();
      return this;
    }

    /// <summary>
    /// Ends the scope.
    /// </summary>
    /// <returns></returns>
    public CSharpBuilder EndScope()
    {
      Unindent().AppendLine("}");
      return this;
    }

    /// <summary>
    /// Begins the scope.
    /// </summary>
    /// <param name="openClause">The opening clause.</param>
    /// <returns></returns>
    public CSharpBuilder BeginScope(string openClause)
    {
      Append(openClause);
      return BeginScope();
    }

    /// <summary>
    /// Returns the accumulated code.
    /// </summary>
    /// <returns>The resulting string. Whitespace at the end of the
    /// accumulated code is trimmed.</returns>
    public override string ToString()
    {
      return base.ToString().TrimEnd();
    }
  }
}