using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using DDW.Collections;
using Microsoft.CSharp;

namespace DDW
{
    public class Cmc
    {
        public static bool ERRINFO; // const

        // 58 - unsafe code
        private static int[] skip = { };

        [STAThread]
        static void Main(string[] args)
        {
            WhatToDo whatToDo = WhatToDo.OnlyParse;
            List<String> files = new List<String>();
            List<Parser.Error> errors = new List<Parser.Error>();
            String skipToFile = null;
            bool useMono = false;
            bool showUsage = false;

            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];
                if (arg == "-O") whatToDo = WhatToDo.PrintToScreen;
                else if (arg == "-F") whatToDo = WhatToDo.WriteToFiles;
                else if (arg == "-C") whatToDo = WhatToDo.Compile;
                else if (arg == "--skipto")
                {
                    if (i + 1 >= args.Length)
                    {
                        Console.WriteLine("--skipto option is missing the file argument!");
                        return;
                    }
                    skipToFile = args[++i];
                }
                else if (arg == "--mono")
                    useMono = true;
                else if (arg == "--help")
                {
                    showUsage = true;
                    break;
                }
                else if (File.Exists(arg))
                {
                    files.Add(arg);
                }
                else if (Directory.Exists(arg))
                {
                    files.AddRange(Directory.GetFiles(arg, "*.cs", SearchOption.AllDirectories));
                }
                else
                {
                    Console.WriteLine(arg + " is neither a file nor a directory!");
                    return;
                }
            }

            if (files.Count == 0 || showUsage)
            {
                if(!showUsage)
                    Console.WriteLine("No files specified!\n");
                Console.WriteLine("Usage: cmc [options] [files]\n"
                    + "Where files is a list of files and/or directories.\n"
                    + "Options:\n"
                    + " -O                 Print result to screen\n"
                    + " -F                 Write results to files\n"
                    + " -C                 Compile result\n"
                    + " --skipto <file>    Skips parsing until the specified file\n"
                    + " --mono             Executes the tests with Mono\n"
                    + "                    (mono must be in the path)\n");
                return;
            }

            Console.WriteLine("Found " + files.Count + " files.");

            Stopwatch sw = new Stopwatch();
            sw.Reset();

            bool skip = skipToFile != null;
            foreach (string fileName in files)
            {
                if (skip)
                {
                    if (fileName.EndsWith(skipToFile))
                    {
                        if (fileName.Length > skipToFile.Length)
                        {
                            char prevChar = fileName[fileName.Length - skipToFile.Length - 1];
                            if (prevChar == Path.DirectorySeparatorChar || prevChar == Path.AltDirectorySeparatorChar)
                                skip = false;
                            else
                                continue;
                        }
                        else
                            skip = false;
                    }
                    else continue;
                }
                sw.Start();
                //Console.WriteLine(fileName);
                ParseFile(fileName, errors, whatToDo, useMono); // multi-thread this? :)
                sw.Stop();
            }

            Console.WriteLine("\n\nTime parsing : " + sw.Elapsed + ", for " + files.Count + " file(s).\n");

            if (errors.Count == 0)
                Console.WriteLine("No errors.");
            else
            {
                Console.WriteLine("The following errors occurred:");
                PrintErrors(errors);
            }
#if DEBUG
            Console.ReadKey();
#endif
        }

        private static void PrintErrors(IEnumerable<Parser.Error> errors)
        {
            foreach (Parser.Error error in errors)
            {
                if (error.Token.ID == TokenID.Eof && error.Line == -1)
                {
                    Console.WriteLine(error.Message + "\nFile: " + error.FileName + "\n");
                }
                else
                {
                    Console.WriteLine(error.Message + " in token " + error.Token.ID
                        + "\nline: " + error.Line + ", column: " + error.Column
                        + "\nin file: " + error.FileName + "\n");
                }
            }
        }

        private static CompilationUnitNode ParseUnit(string fileName, List<Parser.Error> errors)
        {
            Console.Write("\nParsing " + fileName);
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, true);
            Lexer l = new Lexer(sr);
            TokenCollection toks = l.Lex();

            Parser p = null;
            CompilationUnitNode cu = null;

            p = new Parser(fileName);
            cu = p.Parse(toks, l.StringLiterals);

            if (p.Errors.Count != 0)
            {
                Console.WriteLine();
                PrintErrors(p.Errors);
                errors.AddRange(p.Errors);
                return null;
            }
            return cu;
        }

        private static void AddError(List<Parser.Error> errors, String fileName, string errorStr, Token tok, int line, int column)
        {
            Console.WriteLine("\n" + errorStr);
            errors.Add(new Parser.Error(errorStr, tok, line, column, fileName));
        }

        private static void AddError(List<Parser.Error> errors, String fileName, string errorStr)
        {
            AddError(errors, fileName, errorStr, Parser.EOF, -1, -1);
        }

        private static void ParseFile(string fileName, List<Parser.Error> errors, WhatToDo whatToDo, bool useMono)
        {
            bool createExe = true;
            String compilerOptionsStr = "";
            String basePath = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar;
            if (basePath.Length == 1) basePath = "";
            String outAssembly = Path.GetFileNameWithoutExtension(fileName) + ".exe";

            List<String> files = new List<string>();
            files.Add(fileName);

            String[] compilerOptions, dependencies;
            if (!GetExtraOptions(fileName, out compilerOptions, out dependencies))
            {
                Console.WriteLine("GetExtraOptions failed!");
            }
            if (compilerOptions != null)
            {
                foreach (String compOpt in compilerOptions)
                {
                    if (compOpt.Length == 0) continue;
                    if (compOpt[0] != '-')
                        files.Add(basePath + compOpt);
                    else if (compOpt == "-t:library")
                    {
                        createExe = false;
                        outAssembly = Path.GetFileNameWithoutExtension(fileName) + ".dll";
                    }
                    else if (compOpt == "-t:module")
                    {
                        createExe = false;
                        compilerOptionsStr += "/t:module ";
                        outAssembly = Path.GetFileNameWithoutExtension(fileName) + ".netmodule";
                    }
                    else if (compOpt == "-unsafe")
                        compilerOptionsStr += "/unsafe ";
                    else if (compOpt.StartsWith("-r:"))
                        compilerOptionsStr += "/" + compOpt.Substring(1) + " ";
                    else
                    {
                        int colonPos = compOpt.IndexOf(':');
                        if (colonPos == -1) continue;

                        String optionName = compOpt.Substring(1, colonPos - 1);
                        if (optionName != "res" && optionName != "linkresource" && optionName != "addmodule"
                                && optionName != "keyfile")
                            continue;

                        String optFileName;
                        String rest = compOpt.Substring(optionName.Length + 2);
                        int commaPos = rest.IndexOf(',');
                        if (commaPos == -1)
                        {
                            optFileName = rest;
                            rest = "";
                        }
                        else
                        {
                            optFileName = rest.Substring(0, commaPos);
                            rest = rest.Substring(commaPos);
                        }
                        compilerOptionsStr += "/" + optionName + ":\"" + basePath + optFileName + "\"" + rest + " ";
                    }
                }
            }

            String[] unparsedFiles = new String[files.Count];
            for (int i = 0; i < files.Count; i++)
            {
                CompilationUnitNode cu = ParseUnit(files[i], errors);
                if (cu == null) return;

                StringBuilder sb = new StringBuilder();
                cu.ToSource(sb);
                unparsedFiles[i] = sb.ToString();
            }

            //Console.WriteLine(toks + "\n\n");
            //Console.WriteLine(p.CurrentState + "\n\n");

            if (whatToDo == WhatToDo.PrintToScreen)
            {
                Console.WriteLine();
                foreach (String unparsedFile in unparsedFiles)
                    Console.WriteLine(unparsedFile);
            }
            else if (whatToDo == WhatToDo.Compile)
            {
                CSharpCodeProvider compiler = new CSharpCodeProvider();
                CompilerParameters compParams = new CompilerParameters();
                compParams.ReferencedAssemblies.Add("System.dll");
                compParams.ReferencedAssemblies.Add("System.XML.dll");
                compParams.ReferencedAssemblies.Add("System.Data.dll");
                if (createExe)
                    compParams.GenerateExecutable = true;

                if (basePath.Length > 0)
                    compilerOptionsStr += "/lib:\"" + basePath.Substring(0, basePath.Length - 1) + "\" ";
                compParams.OutputAssembly = basePath + outAssembly;
                compParams.CompilerOptions = compilerOptionsStr;
                CompilerResults res = null;
                try
                {
                    res = compiler.CompileAssemblyFromSource(compParams, unparsedFiles);
                }
                catch (BadImageFormatException ex)
                {
                    if (compilerOptionsStr.Contains("/t:module"))
                        AddError(errors, fileName, "\nMono tried to load a module as an assembly\n"
                            + "(see https://bugzilla.novell.com/show_bug.cgi?id=353536)");
                    else
                        AddError(errors, fileName, ex.Message);
                    return;
                }
                catch (Exception ex)
                {
                    AddError(errors, fileName, ex.Message);
                    return;
                }
                if (res.Errors.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\nIllegal C# source code generated: ");
                    sb.Append(res.Errors.Count.ToString());
                    sb.Append(" Errors:\n");
                    foreach (CompilerError error in res.Errors)
                    {
                        sb.Append("Line: ");
                        sb.Append(error.Line.ToString());
                        sb.Append(" - ");
                        sb.Append(error.ErrorText);
                        sb.Append('\n');
                    }
                    AddError(errors, fileName, sb.ToString());
                    return;
                }
                Console.Write(". Compiled.");

                if (compParams.GenerateExecutable)
                {
                    Console.Write(" Testing");
                    ProcessStartInfo psi;
                    if (useMono)
                        psi = new ProcessStartInfo("mono", outAssembly);
                    else
                        psi = new ProcessStartInfo(basePath + outAssembly);

                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.WorkingDirectory = basePath;
                    using (Process proc = Process.Start(psi))
                    {
                        for (int i = 0; i < 5 && !proc.HasExited; i++)
                        {
                            Console.Write('.');
                            proc.WaitForExit(1000);
                        }
                        if (!proc.HasExited)
                        {
                            proc.Kill();
                            AddError(errors, fileName, "Test seems to hang!");
                        }
                        else if (proc.ExitCode != 0)
                        {
                            AddError(errors, fileName, "Test failed! Exit code = " + proc.ExitCode);
                        }
                        else Console.WriteLine(" Succeeded!");
                    }
                }
            }
            else if (whatToDo == WhatToDo.WriteToFiles)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    String path = files[i].Substring(9);
                    Console.WriteLine(". Writing " + path);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (StreamWriter writer = new StreamWriter(path))
                        writer.WriteLine(unparsedFiles[i]);
                }
            }

            /*            TestVisitorVisitor visitor = new TestVisitorVisitor();

                        // if you forget to override the method 'public override object AcceptVisitor(AbstractVisitor visitor, object data)'
                        // it will throw an exception
                        cu.AcceptVisitor(visitor, null);*/
        }

        #region From the Mono compiler-tester

        private static bool GetExtraOptions(string file, out string[] compiler_options,
                                            out string[] dependencies)
        {
            int row = 0;
            compiler_options = null;
            dependencies = null;
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    String line;
                    while (row++ < 3 && (line = sr.ReadLine()) != null)
                    {
                        if (!AnalyzeTestFile(ref row, line, ref compiler_options,
                                            ref dependencies))
                            return false;
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static bool AnalyzeTestFile(ref int row, string line,
                                            ref string[] compiler_options,
                                            ref string[] dependencies)
        {
            const string options = "// Compiler options:";
            const string depends = "// Dependencies:";

            if (row == 1)
            {
                compiler_options = null;
                dependencies = null;
            }

            int index = line.IndexOf(options);
            if (index != -1)
            {
                compiler_options = line.Substring(index + options.Length).Trim().Split(' ');
                for (int i = 0; i < compiler_options.Length; i++)
                {
                    compiler_options[i] = compiler_options[i].TrimStart();
                    if (compiler_options[i].Length > 0 && compiler_options[i][0] == '/')
                        compiler_options[i] = '-' + compiler_options[i].Substring(1);
                }
            }
            index = line.IndexOf(depends);
            if (index != -1)
            {
                dependencies = line.Substring(index + depends.Length).Trim().Split(' ');
                for (int i = 0; i < dependencies.Length; i++)
                    dependencies[i] = dependencies[i].TrimStart();
            }

            return true;
        }
        #endregion From the Mono compiler-tester

        #region Nested type: WhatToDo

        enum WhatToDo { OnlyParse, WriteToFiles, PrintToScreen, Compile };

        #endregion
    }
}

