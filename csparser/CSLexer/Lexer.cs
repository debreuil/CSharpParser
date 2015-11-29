using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DDW.Collections;

// two optimizations are done by the lexer : 
//
// generics : 
//
// the lexer keeps a stack of all '<' saw by him self.
// The lexer pop the last '<' when it reads a '>' or when 
// it is clear that a '<' can not be a generic delcaration
//
// to determine if the '<' begins a generic, the lexer will check each character read until it 
// found a '>'. if the character is not in ['ident', '>', '<', ',', 'white space', '.'], the lexer
// invalidate the '<' and it is removed from the stack  
//
// In parallel, the lexer keeps a trace of all '[' and ']' found between '<' and '>'. -> because
// type parameter can have attributes, so while the position is between a '[' and a ']', all characters are valid, 
// and the '<' is never invalidated.
//
//
// nullable types.
//
// A similar mechanism is used for '?'. Valid characters are different.
// The lexer register the number of opened '[' and '(' after the '?'. 
// when it reachs a ']' or a ')', if this number is equals to zero, this a '?' for 
// a nullable type, else it have to continue the analysis.
// 
// if it reads a ':' after a '?', the lexer invalidates the '?' which can not be a nullable type declaration.

namespace DDW
{
    public class Lexer
    {
        private static readonly Dictionary<string, TokenID> keywords = new Dictionary<string, TokenID>();
        private readonly TextReader src;
        private int c;
        private int curCol = 1;
        private int curLine = 1;
        private bool isHashLine; // when a '#' is read this field is set to <c>true<c> until curLine != '#'.Line
        private bool prefixedWithAt;
        private List<string> strings;
        private TokenCollection tokens;

        public Lexer(TextReader source)
        {
            src = source;
        }
        public List<string> StringLiterals
        {
            get
            {
                return strings;
            }
        }

        public Token GetLastTokenIDExceptBlanks()
        {
            LinkedListNode<Token> node = tokens.Last;
            Token ret = new Token(TokenID.Invalid);

            while (node != null
                   && (node.Value.ID == TokenID.Whitespace || node.Value.ID == TokenID.Newline))
            {
                node = node.Previous;
            }

            if (node != null)
            {
                ret = node.Value;
            }

            return ret;
        }

        public static bool IsKeyWord(TokenID tokId)
        {
            return keywords.ContainsKey(tokId.ToString().ToLower());
        }

        public TokenCollection Lex()
        {
            tokens = new TokenCollection();
            strings = new List<string>();
            StringBuilder sb = new StringBuilder();
            int loc = 0;

            // this is a stack to keep all characters '<' in memory.
            // this field is used in the evaluation optimization of the field <see cref="Token.GenericStart"/>.
            Stack<LinkedListNode<Token>> lessers = new Stack<LinkedListNode<Token>>();

            Stack<LinkedListNode<Token>> lessersTRW = new Stack<LinkedListNode<Token>>();

            // this is the last previous token '<' that the lexer has read.
            // This field is used in the evaluation optimization of the field <see cref="Token.GenericStart"/>.
            LinkedListNode<Token> lastLesser = null;

            bool previousWasGreater = false;

            // this integer is used to track attribute declaration
            // see the end of the while loop for his use.
            int possibleAttribute = 0;
            Stack<int> possibleAttributes = new Stack<int>();

            // conditional variables : they help to dertermine if 
            // a '?' is a conditional test or a nullable type declaration
            LinkedListNode<Token> lastQuestion = null;
            // this declaration allow to handle this case :
            // expr ? a<name?>.StaticMethod() : other<name?>.StaticMethod2()
            Stack<LinkedListNode<Token>> oldQuestions = new Stack<LinkedListNode<Token>>();
            Stack<int> oldOpenedBracket = new Stack<int>();
            Stack<int> oldOpenedParent = new Stack<int>();

            // to handle the case  long? method ( int? param )
            // if the number of '(' is equals to zero when reading the ')', this close the '?'
            // same for the ']'
            int openedBracket = 0;
            int openedParent = 0;

            c = src.Read();
        readLoop:
            while (c != -1)
            {
                // this variable is set to true when the lexer reads an ident, comma or any possible attributes.
                bool possibleGeneric = false;


                #pragma warning disable
                bool possibleNullableType = false;
                // this boolean is set to true when the lexer reads the characters ; 
                // '<', '>', '[', ']', '(', ')', '.', ';'
                bool closePossibleNullableType = false;
                #pragma warning restore

                bool tokenCreated = true;

                switch (c)
                {
                    #region EOS
                    case -1:
                        {
                            goto readLoop; // eos 
                        }
                    #endregion

                    #region WHITESPACE
                    case '\t':
                        {
                            //dont add whitespace tokens
                            while (c == '\t')
                            {
                                c = src.Read();
                                curCol += 4;//tabluation are 4 characters ... is the right default value ? 
                            } // check for dups of \t

                            possibleGeneric = true;
                            possibleNullableType = true;
                            tokenCreated = false;
                            break;
                        }
                    case ' ':
                        {
                            //dont add tokens whitespace
                            while (c == ' ')
                            {
                                c = src.Read();
                                curCol++;
                            }// check for dups of ' '
                            possibleGeneric = true;
                            possibleNullableType = true;
                            tokenCreated = false;
                            break;
                        }
                    case '\r':
                        c = src.Read();
                        if (c == '\n')
                            c = src.Read();
                        goto addNewLine;

                    case '\n':
                        c = src.Read();
                    addNewLine:
                        tokens.AddLast(new Token(TokenID.Newline, curLine, curCol));
                    curLine++;
                    curCol = 1;
                    isHashLine = false;
                    possibleGeneric = true;
                    possibleNullableType = true;
                    break;
                    #endregion

                    #region	STRINGS
                    case '@':
                    case '\'':
                    case '"':
                    {
                        if (isHashLine)
                        {
                            goto default;
                        }

                        int startCol = curCol;
                        bool isVerbatim = false;
                        if (c == '@')
                        {
                            isVerbatim = true;
                            c = src.Read(); // skip to follow quote
                            curCol++;

                            if (c != '\'' && c != '"')
                            {
                                //this is not a string, but this is an identifier usig a keyword id
                                prefixedWithAt = true;
                                goto default;
                            }
                        }
                        sb.Length = 0;
                        int quote = c;
                        bool isSingleQuote = (c == '\'');
                        c = src.Read();
                        curCol++;
                        while (c != -1)
                        {
                            if (c == '\\')
                            {
                                if (isVerbatim)
                                {
                                    sb.Append("\\\\");
                                    c = src.Read();
                                    curCol++;
                                }
                                else                    // normal escaped chars
                                {
                                    c = src.Read();
                                    curCol++;
                                    switch (c)
                                    {
                                        //'"\0abfnrtv
                                        case -1:
                                            {
                                                goto readLoop;
                                            }
                                        case '0':
                                            {
                                                sb.Append("\\0");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'a':
                                            {
                                                sb.Append("\\a");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'b':
                                            {
                                                sb.Append("\\b");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'f':
                                            {
                                                sb.Append("\\f");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'n':
                                            {
                                                sb.Append("\\n");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'r':
                                            {
                                                sb.Append("\\r");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 't':
                                            {
                                                sb.Append("\\t");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case 'v':
                                            {
                                                sb.Append("\\v");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case '\\':
                                            {
                                                sb.Append("\\\\");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case '\'':
                                            {
                                                sb.Append("\\\'");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        case '\"':
                                            {
                                                sb.Append("\\\"");
                                                c = src.Read();
                                                curCol++;
                                                break;
                                            }
                                        default:
                                            {
                                                sb.Append((char)c);
                                                break;
                                            }
                                    }
                                }
                            }
                            else if (c == '\"')
                            {
                                c = src.Read();
                                curCol++;
                                // two double quotes are escapes for quotes in verbatim mode
                                if (c == '\"' && isVerbatim)// verbatim escape
                                {
                                    sb.Append("\\\"");
                                    c = src.Read();
                                    curCol++;
                                }
                                else if (isSingleQuote)
                                {
                                    sb.Append('\"');
                                    curCol++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else if (c == '\r')
                            {
                                sb.Append("\\r");
                                c = src.Read();
                                if (c == '\n')
                                {
                                    sb.Append("\\n");
                                    c = src.Read();
                                    curLine++;
                                    curCol = 1;
                                    isHashLine = false;
                                }
                                if (!isVerbatim)
                                {
                                    tokens.AddLast(new Token(TokenID.Invalid, loc, curLine, startCol));
                                    continue;
                                }
                            }
                            else if (c == '\n')
                            {
                                sb.Append("\\n");
                                c = src.Read();
                                curLine++;
                                curCol = 1;
                                isHashLine = false;
                                if (!isVerbatim)
                                {
                                    tokens.AddLast(new Token(TokenID.Invalid, loc, curLine, startCol));
                                    continue;
                                }
                            }
                            else // non escaped
                            {
                                if (c == quote)
                                {
                                    break;
                                }

                                sb.Append((char)c);
                                c = src.Read();
                                curCol++;
                            }

                        }
                        if (c != -1)
                        {
                            if (c == quote)
                            {
                                c = src.Read(); // skip last quote
                                curCol++;
                            }

                            loc = strings.Count;
                            strings.Add(sb.ToString());
                            if (quote == '"')
                                tokens.AddLast(new Token(TokenID.StringLiteral, loc, curLine, startCol));
                            else
                                tokens.AddLast(new Token(TokenID.CharLiteral, loc, curLine, startCol));
                        }
                        break;
                    }
                    #endregion

                    #region PUNCTUATION
                    case '!':
                    {
                        c = src.Read();
                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.NotEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;

                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Not, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '#':
                    {
                        // preprocessor
                        tokens.AddLast(new Token(TokenID.Hash, curLine, curCol));
                        isHashLine = true;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '$':
                    {
                        tokens.AddLast(new Token(TokenID.Dollar, curLine, curCol)); // this is error in C#
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '%':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.PercentEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Percent, curLine, curCol));
                        }
                        curCol++;
                        break;
                    }
                    case '&':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.BAndEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '&')
                        {
                            tokens.AddLast(new Token(TokenID.And, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.BAnd, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '(':
                    {
                        if (lastQuestion != null)
                        {
                            openedParent++;
                        }

                        tokens.AddLast(new Token(TokenID.LParen, curLine, curCol));
                        possibleNullableType = true;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case ')':
                    {
                        if (lastQuestion != null)
                        {
                            if (openedParent == 0)
                            {
                                closePossibleNullableType = true;
                            }
                            else
                            {
                                openedParent--;
                            }
                        }

                        tokens.AddLast(new Token(TokenID.RParen, curLine, curCol));
                        possibleNullableType = true;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '*':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.StarEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Star, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '+':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.PlusEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '+')
                        {
                            tokens.AddLast(new Token(TokenID.PlusPlus, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Plus, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case ',':
                    {
                        if (tokens.Last == lastQuestion)
                        {
                            closePossibleNullableType = true;
                        }

                        tokens.AddLast(new Token(TokenID.Comma, curLine, curCol));
                        // comma is authorized in generics
                        possibleGeneric = true;
                        possibleNullableType = true;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '-':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.MinusEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '-')
                        {
                            tokens.AddLast(new Token(TokenID.MinusMinus, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '>')
                        {
                            tokens.AddLast(new Token(TokenID.MinusGreater, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Minus, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '/':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.SlashEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '/')
                        {
                            bool IsDocComment = false;
                            c = src.Read();
                            int startCol = curCol;
                            curCol++;

                            sb.Length = 0;


                            if (c == '/')
                            {
                                c = src.Read();
                                curCol++;
                                sb.Append("///");
                                IsDocComment = true;
                            }
                            else
                            {
                                sb.Append("//");
                            }

                            while (c != '\n' && c != '\r' && c != -1)
                            {
                                sb.Append((char)c);
                                c = src.Read();
                                curCol++;
                            }

                            int index = strings.Count;

                            sb.Append(Environment.NewLine);

                            if (IsDocComment)
                            {
                                Token last = GetLastTokenIDExceptBlanks();

                                if (last.ID == TokenID.DocComment)
                                {
                                    index = last.Data;

                                    sb.Insert(0, strings[index]);

                                    strings[index] = sb.ToString();
                                }
                                else
                                {
                                    strings.Add(sb.ToString());

                                    tokens.AddLast(new Token(TokenID.DocComment, index, curLine, startCol));
                                }
                            }
                            else
                            {
                                strings.Add(sb.ToString());

                                tokens.AddLast(new Token(TokenID.SingleComment, index, curLine, startCol));
                            }

                            possibleGeneric = true;
                            possibleNullableType = true;
                        }
                        else if (c == '*')
                        {
                            c = src.Read();
                            int startCol = curCol;
                            curCol++;
                            sb.Length = 0;
                            sb.Append("/*");
                            for (bool exit = false; !exit; )
                            {
                                switch (c)
                                {
                                    case '\r':
                                        {
                                            c = src.Read();
                                            curCol++;
                                            if (c == '\n')
                                                c = src.Read();
                                            sb.Append(Environment.NewLine);
                                            curLine++;
                                            isHashLine = false;
                                            curCol = 1;
                                            break;
                                        }
                                    case '\n':
                                        {
                                            c = src.Read();
                                            curCol++;
                                            curLine++;
                                            isHashLine = false;
                                            curCol = 1;
                                            break;
                                        }
                                    case '*':
                                        {
                                            c = src.Read();
                                            curCol++;
                                            if (c == -1 || c == '/')
                                            {
                                                c = src.Read();
                                                curCol++;
                                                exit = true;
                                            }
                                            else
                                            {
                                                sb.Append('*');
                                            }
                                            break;
                                        }
                                    case -1:
                                        {
                                            exit = true;
                                            break;
                                        }
                                    default:
                                        {
                                            sb.Append((char)c);
                                            c = src.Read();
                                            curCol++;
                                            break;
                                        }

                                }
                            }

                            sb.Append("*/");

                            int index = strings.Count;
                            strings.Add(sb.ToString());
                            tokens.AddLast(new Token(TokenID.MultiComment, index, curLine, startCol));
                            possibleGeneric = true;
                            possibleNullableType = true;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Slash, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }

                    case ':':
                    {
                        c = src.Read();

                        if (c == ':')
                        {
                            tokens.AddLast(new Token(TokenID.ColonColon, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Colon, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case ';':
                    {
                        tokens.AddLast(new Token(TokenID.Semi, curLine, curCol));
                        possibleNullableType = true;
                        closePossibleNullableType = true;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '<':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.LessEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '<')
                        {
                            c = src.Read();

                            if (c == '=')
                            {
                                tokens.AddLast(new Token(TokenID.ShiftLeftEqual, curLine, curCol));
                                c = src.Read();
                                curCol++;
                            }
                            else
                            {
                                tokens.AddLast(new Token(TokenID.ShiftLeft, curLine, curCol));
                            }

                            curCol++;
                        }
                        else
                        {
                            // it is probably a generic open token
                            // if another generic was opened
                            // it pushes the previous '<' on the stack.
                            if (lastLesser != null)
                            {
                                lessers.Push(lastLesser);
                            }
                            possibleAttributes.Push(possibleAttribute);
                            possibleAttribute = 0;
                            //                        }

                            tokens.AddLast(new Token(TokenID.Less, curLine, curCol));

                            lastLesser = tokens.Last;
                            possibleGeneric = true;

                            lessersTRW.Push(tokens.Last);
                        }

                        curCol++;
                        break;
                    }
                    case '=':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.EqualEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Equal, curLine, curCol));
                            possibleNullableType = true;
                            closePossibleNullableType = true;
                        }
                        curCol++;
                        break;
                    }
                    case '>':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.GreaterEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '>')
                        {
                            c = src.Read();

                            if (c == '=')
                            {
                                //                            tokens.AddLast(new Token(TokenID.ShiftRightEqual, curLine, curCol));
                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));
                                Token tok = new Token(TokenID.GreaterEqual, curLine, curCol);
                                tok.LastCharWasGreater = true;
                                tokens.AddLast(tok);

                                c = src.Read();
                                curCol++;
                            }
                            else
                            {
#if !BLUB
                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));
                                Token tok = new Token(TokenID.Greater, curLine, curCol);
                                tok.LastCharWasGreater = true;
                                tokens.AddLast(tok);

                                possibleGeneric = true;
                                previousWasGreater = false;     // we handle the current greater here

                                if (lessersTRW.Count > 0)
                                {
                                    lessersTRW.Pop();
                                    possibleAttribute = possibleAttributes.Pop();
                                }
#else
                            bool convertIt = false;
                            // case of the shift right operator
                            // it might be a double generic closure 
                            // so we must analyze it to convert it in the right token

                            // if there is a previous '<'
                            // and this previous '<' has not been set as a "no generic opening" token
                            // and if there is a "previous-1" '<' and this "previous-1" '<' has not been set as a "no generic opening" token
                            // then this shift right is a double generic closure
                            if (lastLesser != null)
                            {
                                if (lastLesser.Value.GenericStart != false)
                                {
                                    if (lessers.Count > 0 && lessers.Peek().Value.GenericStart != false)
                                    {
                                        convertIt = true;
                                    }
                                }
                            }

                            if (!convertIt)
                            {
                                tokens.AddLast(new Token(TokenID.ShiftRight, curLine, curCol));
                            }
                            else
                            {
                                if (tokens.Last == lastQuestion)
                                {
                                    closePossibleNullableType = true;
                                }

                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol++));
                                tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));

                                // pop the previous-1 '<'.
                                lastLesser = lessers.Pop();
                                possibleAttribute = possibleAttributes.Pop();
								if(lessersTRW > 0) lessersTRW.Pop();

                                // the previous-1 '<' has been validated by the '>>', so back to the previous-2 '<' ( if exists )
                                if (lessers.Count > 0)
                                {
                                    lastLesser = lessers.Pop();
                                    possibleAttribute = possibleAttributes.Pop();
                                }
                                else
                                {
                                    lastLesser = null;
                                    possibleAttribute = 0;
                                }
                            }

                            possibleGeneric = convertIt;
                            possibleNullableType = convertIt;
#endif
                            }

                            curCol++;
                        }
                        else
                        {
                            if (tokens.Last == lastQuestion)
                            {
                                closePossibleNullableType = true;
                            }

                            tokens.AddLast(new Token(TokenID.Greater, curLine, curCol));
                            // greater is authorized in generics
                            possibleGeneric = true;
                            possibleNullableType = true;

                            // if the previous '<' has not been set to false
                            // this is a generic declaration

                            // still '<' on the stack ? 
                            if (lessers.Count > 0)
                            {
                                lastLesser = lessers.Pop();
                                //                            possibleAttribute = possibleAttributes.Pop();
                            }
                            else
                            {
                                lastLesser = null;
                                //                            possibleAttribute = 0;
                            }
                        }

                        curCol++;
                        break;
                    }
                    case '?':
                    {
                        c = src.Read();

                        if (c == '?')
                        {
                            tokens.AddLast(new Token(TokenID.QuestionQuestion, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Question, curLine, curCol));
                            possibleNullableType = true;
                            if (lastQuestion != null)
                            {
                                oldQuestions.Push(lastQuestion);
                                oldOpenedBracket.Push(openedBracket);
                                oldOpenedParent.Push(openedParent);
                            }
                            openedBracket = 0;
                            openedParent = 0;
                            lastQuestion = tokens.Last;

                            possibleGeneric = true;
                        }

                        curCol++;
                        break;
                    }

                    case '[':
                    {
                        if (lastQuestion != null)
                        {
                            openedBracket++;
                        }

                        tokens.AddLast(new Token(TokenID.LBracket, curLine, curCol));
                        possibleNullableType = true;
                        possibleAttribute++;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '\\':
                    {
                        tokens.AddLast(new Token(TokenID.BSlash, curLine, curCol));
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case ']':
                    {
                        if (lastQuestion != null)
                        {
                            if (openedBracket == 0)
                            {
                                closePossibleNullableType = true;
                            }
                            else
                            {
                                openedBracket--;
                                possibleNullableType = true;
                            }
                        }

                        tokens.AddLast(new Token(TokenID.RBracket, curLine, curCol));
                        possibleGeneric = true;
                        possibleAttribute--;
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '^':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.BXorEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.BXor, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '`':
                    {
                        tokens.AddLast(new Token(TokenID.BSQuote, curLine, curCol));
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '{':
                    {
                        tokens.AddLast(new Token(TokenID.LCurly, curLine, curCol));
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '|':
                    {
                        c = src.Read();

                        if (c == '=')
                        {
                            tokens.AddLast(new Token(TokenID.BOrEqual, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else if (c == '|')
                        {
                            tokens.AddLast(new Token(TokenID.Or, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.BOr, curLine, curCol));
                        }

                        curCol++;
                        break;
                    }
                    case '}':
                    {
                        tokens.AddLast(new Token(TokenID.RCurly, curLine, curCol));
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    case '~':
                    {
                        tokens.AddLast(new Token(TokenID.Tilde, curLine, curCol));
                        c = src.Read();
                        curCol++;
                        break;
                    }
                    #endregion

                    #region NUMBERS
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                    {
                        int startCol = curCol;
                        sb.Length = 0;
                        TokenID numKind = TokenID.IntLiteral; // default
                        bool isReal = false;

                        // special case dot
                        if (c == '.')
                        {
                            c = src.Read();
                            curCol++;

                            if (c < '0' || c > '9')
                            {
                                if (tokens.Last == lastQuestion)
                                {
                                    closePossibleNullableType = true;
                                }

                                tokens.AddLast(new Token(TokenID.Dot, curLine, curCol));
                                possibleNullableType = true;
                                possibleGeneric = true;
                                break;
                            }
                            else
                            {
                                sb.Append('.');
                                numKind = TokenID.RealLiteral;
                                isReal = true;
                            }
                        }
                        bool isNum = true;
                        if (c == '0')
                        {
                            sb.Append((char)c);
                            c = src.Read();
                            curCol++;
                            if (c == 'x' || c == 'X')
                            {
                                sb.Append((char)c);
                                isNum = true;
                                while (isNum && c != -1)
                                {
                                    c = src.Read();
                                    curCol++;
                                    switch (c)
                                    {
                                        case '0':
                                        case '1':
                                        case '2':
                                        case '3':
                                        case '4':
                                        case '5':
                                        case '6':
                                        case '7':
                                        case '8':
                                        case '9':
                                        case 'A':
                                        case 'B':
                                        case 'C':
                                        case 'D':
                                        case 'E':
                                        case 'F':
                                        case 'a':
                                        case 'b':
                                        case 'c':
                                        case 'd':
                                        case 'e':
                                        case 'f':
                                            {
                                                sb.Append((char)c);
                                                break;
                                            }
                                        default:
                                            {
                                                isNum = false;
                                                break;
                                            }
                                    }
                                }
                                // find possible U and Ls
                                if (c == 'l' || c == 'L')
                                {
                                    sb.Append((char)c);
                                    c = src.Read();
                                    curCol++;
                                    numKind = TokenID.LongLiteral;
                                    if (c == 'u' || c == 'U')
                                    {
                                        sb.Append((char)c);
                                        numKind = TokenID.ULongLiteral;
                                        c = src.Read();
                                        curCol++;
                                    }
                                }
                                else if (c == 'u' || c == 'U')
                                {
                                    sb.Append((char)c);
                                    numKind = TokenID.UIntLiteral;
                                    c = src.Read();
                                    curCol++;
                                    if (c == 'l' || c == 'L')
                                    {
                                        sb.Append((char)c);
                                        numKind = TokenID.ULongLiteral;
                                        c = src.Read();
                                        curCol++;
                                    }
                                }
                                //numKind = TokenID.HexLiteral;
                                loc = strings.Count;
                                strings.Add(sb.ToString());
                                tokens.AddLast(new Token(numKind, loc, curLine, startCol));
                                break; // done number, exits
                            }
                        }

                        // if we get here, it is non hex, but it might be just zero

                        // read number part
                        isNum = true;
                        while (isNum && c != -1)
                        {
                            switch (c)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    {
                                        sb.Append((char)c);
                                        c = src.Read();
                                        curCol++;
                                        break;
                                    }
                                case '.':
                                    {
                                        if (isReal) // only one dot allowed in numbers
                                        {
                                            numKind = TokenID.RealLiteral;
                                            loc = strings.Count;
                                            strings.Add(sb.ToString());
                                            tokens.AddLast(new Token(numKind, loc, curLine, startCol));
                                            goto readLoop;
                                        }

                                        // might have 77.toString() construct
                                        c = src.Read();
                                        curCol++;
                                        if (c < '0' || c > '9')
                                        {
                                            loc = strings.Count;
                                            strings.Add(sb.ToString());
                                            tokens.AddLast(new Token(numKind, loc, curLine, startCol));

                                            if (tokens.Last == lastQuestion)
                                            {
                                                closePossibleNullableType = true;
                                            }

                                            tokens.AddLast(new Token(TokenID.Dot, curLine, curCol));
                                            possibleNullableType = true;
                                            possibleGeneric = true;
                                            goto readLoop;
                                        }
                                        else
                                        {
                                            sb.Append('.');
                                            sb.Append((char)c);
                                            numKind = TokenID.RealLiteral;
                                            isReal = true;
                                        }
                                        c = src.Read();
                                        curCol++;
                                        break;
                                    }
                                default:
                                    {
                                        isNum = false;
                                        break;
                                    }
                            }
                        }
                        // now test for letter endings

                        // first exponent
                        if (c == 'e' || c == 'E')
                        {
                            numKind = TokenID.RealLiteral;
                            sb.Append((char)c);
                            c = src.Read();
                            curCol++;
                            if (c == '+' || c == '-')
                            {
                                sb.Append((char)c);
                                c = src.Read();
                                curCol++;
                            }

                            isNum = true;
                            while (isNum && c != -1)
                            {
                                switch (c)
                                {
                                    case '0':
                                    case '1':
                                    case '2':
                                    case '3':
                                    case '4':
                                    case '5':
                                    case '6':
                                    case '7':
                                    case '8':
                                    case '9':
                                        {
                                            sb.Append((char)c);
                                            c = src.Read();
                                            curCol++;
                                            break;
                                        }
                                    default:
                                        {
                                            isNum = false;
                                            break;
                                        }
                                }
                            }
                        }
                        else if (c == 'd' || c == 'D' ||
                                    c == 'f' || c == 'F' ||
                                    c == 'm' || c == 'M')
                        {
                            numKind = TokenID.RealLiteral;
                            sb.Append((char)c);
                            c = src.Read();
                            curCol++;
                        }
                        // or find possible U and Ls
                        else if (c == 'l' || c == 'L')
                        {
                            sb.Append((char)c);
                            numKind = TokenID.LongLiteral;
                            c = src.Read();
                            curCol++;
                            if (c == 'u' || c == 'U')
                            {
                                sb.Append((char)c);
                                numKind = TokenID.ULongLiteral;
                                c = src.Read();
                                curCol++;
                            }
                        }
                        else if (c == 'u' || c == 'U')
                        {
                            sb.Append((char)c);
                            numKind = TokenID.UIntLiteral;
                            c = src.Read();
                            curCol++;
                            if (c == 'l' || c == 'L')
                            {
                                sb.Append((char)c);
                                numKind = TokenID.ULongLiteral;
                                c = src.Read();
                                curCol++;
                            }
                        }

                        loc = strings.Count;
                        strings.Add(sb.ToString());
                        tokens.AddLast(new Token(numKind, loc, curLine, startCol));
                        isNum = false;
                        break;
                    }
                    #endregion

                    #region IDENTIFIERS/KEYWORDS
                    default:
                    {
                        int startCol = curCol;

                        // todo: deal with unicode chars
                        // check if this is an identifier char

                        char convertedChar = (char)c;
                        //do not use code like "if c =='a'" because it might be not latin character
                        if (isHashLine
                                || Char.IsLetter(convertedChar)
                                || convertedChar == '_')
                        {
                            if (c == '\n')
                            {
                                goto case '\n';
                            }
                            else
                            {
                                if (c == '\r')
                                {
                                    goto case '\r';
                                }
                            }
                            sb.Length = 0;
                            if (prefixedWithAt)
                            {
                                sb.Append("@");
                                prefixedWithAt = false;
                            }
                            sb.Append((char)c);
                            c = src.Read();
                            curCol++;
                            bool endIdent = false;
                            bool possibleKeyword = true;

                            while (c != -1 && !endIdent)
                            {
                                convertedChar = (char)c;

                                if (!Char.IsLetterOrDigit(convertedChar)
                                        && convertedChar != '_'
                                        || (c == '\n' || c == '\r'))
                                {
                                    endIdent = true;
                                }
                                else
                                {
                                    //do not use code like "if c =='a'" because it might be not latin caharacter
                                    if (!Char.IsLetter(convertedChar) || Char.IsUpper(convertedChar))
                                    {
                                        possibleKeyword = false;
                                    }

                                    sb.Append(convertedChar);
                                    c = src.Read();
                                    curCol++;
                                }
                            }

                            string identText = sb.ToString();
                            bool isKeyword = possibleKeyword ? keywords.ContainsKey(identText) : false;
                            Token tk;
                            if (isKeyword)
                            {
                                tk = new Token(keywords[identText], curLine, startCol);
                            }
                            else
                            {
                                loc = strings.Count;
                                strings.Add(identText);
                                tk = new Token(TokenID.Ident, loc, curLine, startCol);
                            }


                            if (tk.ID == TokenID.Ident
                                    || tk.ID >= TokenID.Byte && tk.ID <= TokenID.UInt)
                            {
                                possibleGeneric = true;
                                possibleNullableType = true;
                            }

                            tokens.AddLast(tk);
                        }
                        else
                        {
                            tokens.AddLast(new Token(TokenID.Invalid, curLine, curCol));
                            c = src.Read();
                            curCol++;
                        }
                        break;
                    }
                    #endregion
                }

                // the lexer encountered an invalid character for a generic declaration
                // and this is not a possible attribute declaration for a type parameter
                // so the previous '<' was not a generic open declaration
                /*                if (lastLesser != null)
                                {
                                    if (!possibleGeneric 
                                        && possibleAttribute == 0 
                                        && ( lastQuestion == null || !possibleNullableType))
                                    {
                                        Token tk = lastLesser.Value;
                                        tk.GenericStart = false;
                                        lastLesser.Value = tk;
                                        possibleAttribute = 0;

                                        // if this is not a generic, it invalidate all previous '<'
                                        while (lessers.Count > 0)
                                        {
                                            lastLesser = lessers.Pop();
                                            tk = lastLesser.Value;
                                            tk.GenericStart = false;
                                            lastLesser.Value = tk;
                                            // pop possible attributes stack                             
                                            possibleAttribute = possibleAttributes.Pop();
                                        }

                                        lastLesser = null;
                                        possibleAttribute = 0;
                                    }
                                }*/

                /*                if (lastQuestion != null)
                                {
                                    if (!possibleNullableType)
                                    {
                                        if (oldQuestions.Count > 0 )
                                        {
                                            lastQuestion = oldQuestions.Pop();
                                            openedBracket = oldOpenedBracket.Pop();
                                            openedParent = oldOpenedParent.Pop();
                                        }

                                        Token tk = lastQuestion.Value;
                                        tk.NullableDeclaration = false;
                                        lastQuestion.Value = tk;
                                        lastQuestion = null;
                                    }

                                    if (closePossibleNullableType)
                                    {
                                        lastQuestion = null;

                                        if (oldQuestions.Count > 0)
                                        {
                                            lastQuestion = oldQuestions.Pop();
                                            openedBracket = oldOpenedBracket.Pop();
                                            openedParent = oldOpenedParent.Pop();
                                        }
                                    }
                                }*/

                if (lastQuestion != null && tokens.Last != lastQuestion)
                {
                    switch (tokens.Last.Value.ID)
                    {
                        case TokenID.LParen:
                        case TokenID.Plus:
                        case TokenID.Minus:
                        case TokenID.Star:
                        case TokenID.BAnd:
                        case TokenID.Tilde:
                        case TokenID.Not:

                        case TokenID.Ident:
                        case TokenID.CharLiteral:
                        case TokenID.DecimalLiteral:
                        case TokenID.HexLiteral:
                        case TokenID.IntLiteral:
                        case TokenID.LongLiteral:
                        case TokenID.RealLiteral:
                        case TokenID.StringLiteral:
                        case TokenID.UIntLiteral:
                        case TokenID.ULongLiteral:
                        case TokenID.True:
                        case TokenID.False:
                        case TokenID.Null:

                        case TokenID.SByte:
                        case TokenID.Byte:
                        case TokenID.Short:
                        case TokenID.UShort:
                        case TokenID.Int:
                        case TokenID.UInt:
                        case TokenID.Long:
                        case TokenID.ULong:
                        case TokenID.Char:
                        case TokenID.Float:
                        case TokenID.Double:
                        case TokenID.Decimal:
                        case TokenID.Bool:
                        case TokenID.Object:
                        case TokenID.String:
                            Token tk = lastQuestion.Value;
                            tk.NullableDeclaration = false;
                            lastQuestion.Value = tk;
                            lastQuestion = null;
                            break;

                        case TokenID.Newline:
                        case TokenID.DocComment:
                        case TokenID.SingleComment:
                        case TokenID.MultiComment:
                            break;

                        // TODO: Move preprocessor into lexer!!

                        default:
                            lastQuestion = null;
                            break;
                    }
                }

                if (!tokenCreated) continue;

                if (lessersTRW.Count > 0)
                {
                    // ECMA 9.2.3
                    switch (tokens.Last.Value.ID)
                    {
                        case TokenID.Greater:
                            if (lessersTRW.Count > 1)
                                goto case TokenID.NotEqual;     // could be a generic
                            else
                                goto default;                   // if previousWasGreater is true, it's not a generic

                        case TokenID.LParen:
                        case TokenID.RParen:
                        case TokenID.RBracket:
                        case TokenID.Colon:
                        case TokenID.Semi:
                        case TokenID.Comma:
                        case TokenID.Dot:
                        case TokenID.Question:
                        case TokenID.EqualEqual:
                        case TokenID.NotEqual:
                            if (previousWasGreater)
                            {
                                LinkedListNode<Token> node = lessersTRW.Pop();
                                Token tk = node.Value;
                                tk.GenericStart = true;
                                node.Value = tk;
                                possibleAttribute = possibleAttributes.Pop();
                            }
                            previousWasGreater = tokens.Last.Value.ID == TokenID.Greater;
                            break;

                        case TokenID.Newline:
                        case TokenID.DocComment:
                        case TokenID.SingleComment:
                        case TokenID.MultiComment:
                            break;

                        // TODO: Move preprocessor into lexer!!

                        default:
                            if (previousWasGreater)
                            {
                                // not a generic, invalidate all previous '<'
                                do
                                {
                                    lessersTRW.Pop();
                                    // pop possible attributes stack
                                    possibleAttributes.Pop();
                                }
                                while (lessersTRW.Count > 0);
                                possibleAttribute = 0;
                            }
                            previousWasGreater = tokens.Last.Value.ID == TokenID.Greater;
                            break;
                    }

                    // found illegal token for generic?
                    if (lessersTRW.Count > 0 && !possibleGeneric && possibleAttribute == 0)
                    {
                        // yes, invalidate all previous '<'
                        do
                        {
                            LinkedListNode<Token> node = lessersTRW.Pop();
                            Token tk = node.Value;
                            tk.GenericStart = false;
                            node.Value = tk;
                            // pop possible attributes stack
                            possibleAttributes.Pop();
                        }
                        while (lessersTRW.Count > 0);
                        possibleAttribute = 0;
                    }
                }
                else previousWasGreater = false;
            }
            return tokens;
        }

        #region STATIC CTOR
        static Lexer()
        {
            keywords.Add("abstract", TokenID.Abstract);
            keywords.Add("as", TokenID.As);
            keywords.Add("base", TokenID.Base);
            keywords.Add("bool", TokenID.Bool);
            keywords.Add("break", TokenID.Break);
            keywords.Add("byte", TokenID.Byte);
            keywords.Add("case", TokenID.Case);
            keywords.Add("catch", TokenID.Catch);
            keywords.Add("char", TokenID.Char);
            keywords.Add("checked", TokenID.Checked);
            keywords.Add("class", TokenID.Class);
            keywords.Add("const", TokenID.Const);
            keywords.Add("continue", TokenID.Continue);
            keywords.Add("decimal", TokenID.Decimal);
            keywords.Add("default", TokenID.Default);
            keywords.Add("delegate", TokenID.Delegate);
            keywords.Add("do", TokenID.Do);
            keywords.Add("double", TokenID.Double);
            keywords.Add("else", TokenID.Else);
            keywords.Add("enum", TokenID.Enum);
            keywords.Add("event", TokenID.Event);
            keywords.Add("explicit", TokenID.Explicit);
            keywords.Add("extern", TokenID.Extern);
            keywords.Add("false", TokenID.False);
            keywords.Add("finally", TokenID.Finally);
            keywords.Add("fixed", TokenID.Fixed);
            keywords.Add("float", TokenID.Float);
            keywords.Add("for", TokenID.For);
            keywords.Add("foreach", TokenID.Foreach);
            keywords.Add("goto", TokenID.Goto);
            keywords.Add("if", TokenID.If);
            keywords.Add("implicit", TokenID.Implicit);
            keywords.Add("in", TokenID.In);
            keywords.Add("int", TokenID.Int);
            keywords.Add("interface", TokenID.Interface);
            keywords.Add("internal", TokenID.Internal);
            keywords.Add("is", TokenID.Is);
            keywords.Add("lock", TokenID.Lock);
            keywords.Add("long", TokenID.Long);
            keywords.Add("namespace", TokenID.Namespace);
            keywords.Add("new", TokenID.New);
            keywords.Add("null", TokenID.Null);
            keywords.Add("object", TokenID.Object);
            keywords.Add("operator", TokenID.Operator);
            keywords.Add("out", TokenID.Out);
            keywords.Add("override", TokenID.Override);
            keywords.Add("params", TokenID.Params);
            keywords.Add("private", TokenID.Private);
            keywords.Add("protected", TokenID.Protected);
            keywords.Add("public", TokenID.Public);
            keywords.Add("readonly", TokenID.Readonly);
            keywords.Add("ref", TokenID.Ref);
            keywords.Add("return", TokenID.Return);
            keywords.Add("sbyte", TokenID.SByte);
            keywords.Add("sealed", TokenID.Sealed);
            keywords.Add("short", TokenID.Short);
            keywords.Add("sizeof", TokenID.Sizeof);
            keywords.Add("stackalloc", TokenID.Stackalloc);
            keywords.Add("static", TokenID.Static);
            keywords.Add("string", TokenID.String);
            keywords.Add("struct", TokenID.Struct);
            keywords.Add("switch", TokenID.Switch);
            keywords.Add("this", TokenID.This);
            keywords.Add("throw", TokenID.Throw);
            keywords.Add("true", TokenID.True);
            keywords.Add("try", TokenID.Try);
            keywords.Add("typeof", TokenID.Typeof);
            keywords.Add("uint", TokenID.UInt);
            keywords.Add("ulong", TokenID.ULong);
            keywords.Add("unchecked", TokenID.Unchecked);
            keywords.Add("unsafe", TokenID.Unsafe);
            keywords.Add("ushort", TokenID.UShort);
            keywords.Add("using", TokenID.Using);
            keywords.Add("virtual", TokenID.Virtual);
            keywords.Add("void", TokenID.Void);
            keywords.Add("volatile", TokenID.Volatile);
            keywords.Add("while", TokenID.While);
        }
        #endregion
    }

}
