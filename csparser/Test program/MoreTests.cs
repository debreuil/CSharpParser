using System;
using System.IO;
using System.Text;
using DDW.Collections;

namespace DDW
{
  class MoreTests
  {
    private static void Main()
    {
      try
      {
        testCode("namespace N { class C { } }");

        testCode(@"
class C {
  private int N() {
    int [] t = new int[10];
    foreach (int n in t)
      t.ToString();
  }
}
");

        testCode(@"
class C {
  private int N() {
    WaitHandle.WaitAll(new ThreadStart[]{a, b}.Select(o => i.BeginInvoke(null,null).AsyncWaitHandle).ToArray()));
  }
  private void a() {}
  private void b() {}
}
");
      } 
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

#if DEBUG
      Console.ReadKey();
#endif
    }

    static void testCode(string code)
    {
      Lexer lex = new Lexer(new StreamReader(new MemoryStream(
                                               Encoding.UTF8.GetBytes(code))));
      TokenCollection tc = lex.Lex();
      Parser p = new Parser(string.Empty);
      CompilationUnitNode n = p.Parse(tc, lex.StringLiterals);

      StringBuilder sb = new StringBuilder();
      n.ToSource(sb);
      Console.WriteLine(sb.ToString());
    }
  }
}
