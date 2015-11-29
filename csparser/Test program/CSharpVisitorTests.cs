/* Copyright (C) Dmitri Nesteruk */

using DDW.GraphVisitor;

namespace DDW
{
  using NUnit.Framework;
  using System.IO;
  using Collections;

  /// <summary>
  /// CSharpVisitorTests.
  /// </summary>
  [TestFixture]
  public class CSharpVisitorTests
  {
    private CSharpVisitor visitor;
    private Parser parser;
    private Lexer lexer;
    private CompilationUnitNode rootNode;

    [SetUp]
    public void Run_this_before_each_test()
    {
      visitor = new CSharpVisitor();
    }

    public string ParseContent(string content)
    {
      using (StringReader sr = new StringReader(content))
      {
        lexer = new Lexer(sr);
        TokenCollection tc = lexer.Lex();
        parser = new Parser();
        rootNode = parser.Parse(tc, lexer.StringLiterals);
        return rootNode.AcceptVisitor(visitor, null) as string;
      }
    }

    [Test]
    public void Empty_source_should_yields_empty_result()
    {
      Assert.IsEmpty(ParseContent(string.Empty));
    }

    [Test]
    public void Empty_namespace_should_yield_itself()
    {
      const string self = 
@"namespace N
{
}";
      string s = ParseContent(self);
      Assert.AreEqual(self, s);
    }
  }
}