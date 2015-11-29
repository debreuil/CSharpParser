namespace DDW
{
  using Builders;
  using NUnit.Framework;

  /// <summary>
  /// Unit tests for the <c>CSharpBuilder</c>.
  /// </summary>
  [TestFixture]
  public sealed class CSharpBuilderTests
  {
    private CSharpBuilder builder;

    [SetUp]
    public void Run_this_before_each_test()
    {
      // two spaces as indent
      builder = new CSharpBuilder("  ");
    }

    [Test]
    public void Default_builder_yields_empty_string()
    {
      Assert.IsEmpty(builder.ToString());
    }

    [Test]
    public void Empty_scope_is_handled_correctly()
    {
      builder.BeginScope();
      builder.EndScope();
      Assert.AreEqual(
        @"{
}", builder.ToString());
    }

    [Test]
    public void If_statement_scope_is_handled_correctly()
    {
      builder.BeginScope("if (x == 0) ");
      builder.EndScope();
      Assert.AreEqual(@"if (x == 0) {
}", builder.ToString());
    }

    [Test]
    public void Double_indentation_works_as_expected()
    {
      builder.AppendLine("a();").Indent().Indent().Append("b();");
      Assert.AreEqual(@"a();
    b();", builder.ToString());
    }
  }
}