using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DDW.Collections;
using System.Diagnostics;

namespace DDW
{
    public class Cmc
    {
        public static bool ERRINFO = false; // const

        // 58 - unsafe code
        private static int[] skip = { };

        [STAThread]
        static void Main(string[] args)
        {
            bool testFiles = false;
            string testDirectory = string.Empty;
            string[] files = null;
            List<Parser.Error> errors = new List<Parser.Error>();

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "--test")
                {
                    if (++i < args.Length)
                    {
                        testFiles = true;
                        testDirectory = args[i];
                    }
                }
            }

            if (testFiles)
            {
                files = System.IO.Directory.GetFiles(testDirectory, "*.cs", SearchOption.AllDirectories);
                Console.WriteLine("found " + files.Length + " files.");
                Console.WriteLine(string.Empty);
            }
            else
            {
                files = new string[] { @"..\..\..\ddw_tests\scratch.cs" };
            }

            // if you need to test a particular test file ( because the test phase was wrong with this file )
            // uncomment the folling comment and rewrite the right file path
            // the system will parse only this file
            //const string the_file = @"D:\Dev\CSParser\csparser\Class2.cs";
            //files = new string[] { the_file };

            Stopwatch sw = new Stopwatch();
            sw.Reset();

            foreach (string fileName in files)
            {
                sw.Start();
                ParseFile(fileName, errors);
                sw.Stop();
            }

            //foreach (Parser.Error error in errors)
            //{
            //    Console.WriteLine(string.Empty);
            //    Console.WriteLine(error.Message + " in token " + error.Token.ID);
            //    Console.WriteLine("line: " + error.Line + ", column: " + error.Column);
            //    Console.WriteLine("in file: " + error.FileName);
            //    Console.WriteLine(" ");
            //}

            Console.WriteLine("Time parsing : " + sw.Elapsed + ", for " + files.Length + " file(s).");
            Console.ReadKey();
        }

        private static void ParseFile(string fileName, List<Parser.Error> errors)
        {
            //Console.WriteLine("Parsing " + fileName);
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, true);
            Lexer l = new Lexer(sr);
            TokenCollection toks = l.Lex();

            Parser p = null;
            CompilationUnitNode cu = null;

            p = new Parser(fileName);
            cu = p.Parse(toks, l.StringLiterals);



            errors.AddRange(p.Errors);

            //Console.WriteLine(toks + "\n\n");
            //Console.WriteLine(p.CurrentState + "\n\n");

            //StringBuilder sb = new StringBuilder();
            //cu.ToSource(sb);
            //Console.WriteLine(sb.ToString());
        }
    }
}

