using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

using DDW.Collections;

// Contributed by Thomas (tw codeplex id) -- thanks!

/**************************************************************************************************
 * 
 * IMPORTANT NOTE
 * --------------
 * 
 * This class requires a minimal change to the lexer class (file: Lexer.cs) to compile:
 * 
 * - C'tor was changed to take a 'TextReader' argument instead of 'StreamReader'.
 * - Type of 'src' field was changed accordingly. 
 * - No other changes required.
 * 
 *************************************************************************************************/

namespace DDW
{
    /// <summary>
    /// Runs the MicroParser in a separate thread that times out, thus preventing the calling
    /// method to hang if parser itself hangs (because it was given some 'strange' source 
    /// code to parse or sth. else went wrong ;-)).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Parsing can be done either by file (<see cref="ParseFile"/>) or code snippet (<see cref="ParseCode"/>).
    /// </para>
    /// <para>
    /// Timeout value can be set either explicitely by using the 'Timeout' property or
    /// implicitly by invoking the respective method taking a timeout parameter. In the
    /// latter case, the objects 'Timeout' property will remain unchanged and the supplied
    /// timeout value is valid only for the current method call.
    /// </para>
    /// </remarks>
    /// <author>Thomas Weller</author>
    public sealed class ParserRunner
    {
        #region Constants

        /// <summary>
        /// Used in parser's error descriptions if no filename is available.
        /// </summary>
        public const string CODE = "<code snippet>";

        /// <summary>
        /// Default timeout for parser in ms.
        /// </summary>
        public const int DEFAULTTIMEOUT = 500;
        /// <summary>
        /// Minimum settable timeout for parser in ms.
        /// </summary>
        public const int MINTIMEOUT = 20;
        /// <summary>
        /// Maximum settable timeout for parser in ms.
        /// </summary>
        public const int MAXTIMEOUT = 5000;

        #endregion // Constants

        #region Fields

        /// <summary>
        /// Timeout value (in ms) used when invoking parser.
        /// </summary>
        static int _timeout = DEFAULTTIMEOUT;

        #endregion // Fields

        #region Public Properties ('Class Interface')

        /// <summary>
        /// Gets or sets the timeout value to use for parser invocation.
        /// </summary>
        /// <value>
        /// The timeout (in ms). Setting this value to <c>null</c> means setting it to <see cref="DEFAULTTIMEOUT"/>.
        /// </value>
        /// <remarks>
        /// Timeout values may vary according to size/complexity of the parsed code
        /// and the performance of the machine that is running the parser code.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the supplied value is outside the range defined by the <see cref="MINTIMEOUT"/> 
        /// and <see cref="MAXTIMEOUT"/> constants.
        /// </exception>
        public static int? Timeout
        {
            get { return ParserRunner._timeout; }
            set
            {
                if (value == null)
                {
                    _timeout = DEFAULTTIMEOUT;
                }
                else if (value > MAXTIMEOUT || value < MINTIMEOUT)
                {
                    throw new ArgumentException("Timeout value must be in the range of " +
                                                MINTIMEOUT + " to " + MAXTIMEOUT + ".");
                }
                else
                {
                    _timeout = value.Value;
                }
            }
        }

        #endregion // Public Properties

        #region Public Methods ('Class Interface')

        /// <summary>
        /// Parses the specified code snippet using an explicit timeout value.
        /// </summary>
        /// <param name="code">The text to parse.</param>
        /// <param name="errors">Out param. List of errors from the
        /// parser or <c>null</c> if there were none.</param>
        /// <param name="timeout">
        /// The timeout in ms - used only during this method call, the <see cref="Timeout"/>
        /// property value will not be changed.</param>
        /// <returns>The parsing result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the supplied timeout value is outside the range defined by the
        /// <see cref="MINTIMEOUT"/>  and <see cref="MAXTIMEOUT"/> constants.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="code"/> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either the parser itself threw an exception or a timeout occured. Refer
        /// to the inner exception(s) to check.
        /// </exception>
        public static CompilationUnitNode ParseCode(string code, out List<Parser.Error> errors, int timeout)
        {
            int? oldval = Timeout;
            Timeout = timeout;

            try
            {
                return ParseCode(code, out errors);
            }
            finally
            {
                Timeout = oldval;
            }
        }

        /// <summary>
        /// Parses the specified file using an explicit timeout value.
        /// </summary>
        /// <param name="file">The file to parse.</param>
        /// <param name="errors">Out param. List of errors from the
        /// parser or <c>null</c> if there were none.</param>
        /// <param name="timeout">
        /// The timeout in ms - used only during this method call, the <see cref="Timeout"/>
        /// property value will not be changed.</param>
        /// <returns>The parsing result.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the supplied timeout value is outside the range defined by the
        /// <see cref="MINTIMEOUT"/>  and <see cref="MAXTIMEOUT"/> constants.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the file specified by the <paramref name="file"/> parameter could not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either the parser itself threw an exception or a timeout occured. Refer
        /// to the inner exception(s) to check.
        /// </exception>
        public static CompilationUnitNode ParseFile(string file, out List<Parser.Error> errors, int timeout)
        {
            int? oldval = Timeout;
            Timeout = timeout;

            try
            {
                return ParseFile(file, out errors);
            }
            finally
            {
                Timeout = oldval;
            }
        }

        /// <summary>
        /// Parses the specified code snippet.
        /// </summary>
        /// <param name="code">The text to parse.</param>
        /// <param name="errors">Out param. List of errors from the
        /// parser or <c>null</c> if there were none.</param>
        /// <returns>The parsing result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="code"/> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either the parser itself threw an exception or a timeout occured. Refer
        /// to the inner exception(s) to check.
        /// </exception>
        public static CompilationUnitNode ParseCode(string code, out List<Parser.Error> errors)
        {
            if (code == null)
                throw new ArgumentNullException("code");

            try
            {
                StringReader sr = new StringReader(code);
                return InvokeParse(sr, CODE, out errors);
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException("Error while parsing. See inner exception for details.", exc);
            }
        }

        /// <summary>
        /// Parses the specified file.
        /// </summary>
        /// <param name="file">The file to parse.</param>
        /// <param name="errors">Out param. List of errors from the
        /// parser or <c>null</c> if there were none.</param>
        /// <returns>The parsing result.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> parameter is <c>null</c>.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when the file specified by the <paramref name="file"/> parameter could not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either the parser itself threw an exception or a timeout occured. Refer
        /// to the inner exception(s) to check.
        /// </exception>
        public static CompilationUnitNode ParseFile(string file, out List<Parser.Error> errors)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, true);
                return InvokeParse(sr, file, out errors);
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException("Error while parsing. See inner exception for details.", exc);
            }

        }

        #endregion // Public Methods

        #region Implementation

        /// <summary>
        /// Invokes the parser using a separate thread.
        /// </summary>
        /// <remarks>
        /// Uses the <see cref="ParserWorkitem"/> helper class to execute parsing. This
        /// ensures that parser invocations always return, even if the parser itself hangs.
        /// </remarks>
        /// <param name="rd">A <see cref="System.IO.TextReader"/> with the code to parse.</param>
        /// <param name="filename">The filename or the value of <see cref="CODE"/> constant.</param>
        /// <param name="errors">The error list returned by the parser.</param>
        /// <returns></returns>
        static CompilationUnitNode InvokeParse(TextReader rd, string filename, out List<Parser.Error> errors)
        {
            errors = null;

            EventWaitHandle wait = new EventWaitHandle(false, EventResetMode.AutoReset);
            ParserWorkitem workitem = new ParserWorkitem(rd, filename, wait);
            Thread workerThread = new Thread(workitem.Parse);
            workerThread.Priority = ThreadPriority.AboveNormal;

            workerThread.Start();

            if (wait.WaitOne(_timeout, false))
            {
                // Were there any exceptions in parser itself? If so, rethrow these.
                if (workitem.ExceptionFromParser != null)
                    throw workitem.ExceptionFromParser;

                // Parser finished regularly. -> Return results.
                errors = workitem.Errors;
                return workitem.Result;
            }

            // If we arrive here, the timeout has elapsed -> 
            // abort parser thread, then throw TimeoutException.
            // *************************************************

            workerThread.Abort();
            workerThread.Join(); // waits until the thread really is canceled

            throw new TimeoutException("Parser invocation timed out, timeout value (in ms): " + _timeout + ".");
        }

        #endregion // Implementation

    } // class ParserRunner

    /// <summary>
    /// Helper class to invoke the MicroParser in a separate thread
    /// (Handles thread abortion and exceptions from the parser).
    /// </summary>
    /// <remarks>
    /// This class mimics the 'ParserRunner.InvokeParse()' method signature.
    /// </remarks>
    /// <author>Thomas Weller</author>
    class ParserWorkitem
    {
        #region 'Method arguments' (set by c'tor)

        readonly TextReader rd;
        readonly string filename;
        readonly EventWaitHandle wait;

        #endregion // 'Method arguments' (set by c'tor)

        #region 'Method return values'

        CompilationUnitNode _result = null;
        List<Parser.Error> _errors = null;
        Exception _exc = null;

        public Exception ExceptionFromParser
        {
            get { return _exc; }
        }

        public List<Parser.Error> Errors
        {
            get { return _errors; }
        }

        public CompilationUnitNode Result
        {
            get { return _result; }
        }

        #endregion // 'Method return values'

        #region Construction (Taking 'method arguments' and a wait handle)

        public ParserWorkitem(TextReader rd, string filename, EventWaitHandle wait)
        {
            this.rd = rd;
            this.filename = filename;
            this.wait = wait;
        }

        #endregion // Construction

        #region 'Worker method' (referenced by a ThreadStart' delegate from the calling thread)

        /// <summary>
        /// Invokes the parser. Signals the wait event supplied to the c'tor if parser has finished.
        /// </summary>
        /// <remarks>
        /// This method assumes that it will be executed in a separate thread that may be aborted
        /// by the calling thread. Takes care of the two different error conditions: Parser itself
        /// may throw an exception or thread is being aborted.
        /// </remarks>
        public void Parse()
        {
            try
            {
                try
                {
                    Lexer l = new Lexer(rd);
                    TokenCollection toks = l.Lex();
                    Parser p = new Parser(filename);

                    _result = p.Parse(toks, l.StringLiterals);
                    _errors = p.Errors;
                }
                catch (Exception exc)       // Exception thrown by parser -> save
                {                           // it (calling thread will rethrow this).
                    _exc = exc;
                }
                finally                     // Ensure signalling wait event, we are done (no
                {                           // matter whether errors/exceptions occured or not).
                    wait.Set();
                }
            }
            catch (ThreadAbortException)    // Thread that this method is running in was aborted
            {                               // by calling thread (because of a timeout) -> prevent
                Thread.ResetAbort();        // ThreadAbortException from being rethrown (see MSDN).
            }
        }

        #endregion // 'Worker method'

    } // class ParserWorkitem

} // namespace DDW
