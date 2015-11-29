using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DDW.Collections;
using DDW.Enums;
using DDW.Names;

namespace DDW
{
    public class Parser
    {
        public static readonly Token EOF = new Token(TokenID.Eof);
        private static readonly Modifier[] modMap;
        private static readonly byte[] precedence;
        private static readonly Dictionary<string, PreprocessorID> preprocessor;
        public static int ASSOC_LEFT = 1;
        public static int ASSOC_RIGHT;
        public static int no_name_index;
        public static byte PRECEDENCE_MULTIPLICATIVE = 0x7f;

        public static byte PRECEDENCE_PRIMARY = 0x90;
        public static byte PRECEDENCE_UNARY = 0x80;
        public static char[] QualifiedIdentifierDelimiters = new char[] { '.' };
        private readonly string currentFileName = string.Empty;

        /// <summary>
        /// all parsed errors are stored in this list
        /// </summary>
        public readonly List<Error> Errors = new List<Error>();

        /// <summary>
        /// accessed by <see cref="ParseLocalDeclaration"/>
        /// </summary>
        private Stack<BlockStatement> blockStack;

        /// <summary>
        /// This dictionary stores all parsed constructed types.
        /// The key is the full qualified namespace plus the unique identifier of the type
        /// ( <see cref="ConstructedTypeNode.GenericIdentifier"/> ).
        /// I.e. : namespace1.namespace2.type
        /// 
        /// This field has two objectives :
        ///     1. control the unicity of each type between all parsed files.
        ///     2. grabs together partial class defined in different files.
        /// </summary>
        private Dictionary<string, ConstructedTypeNode> constructedTypes;

        private CompilationUnitNode cu;
        private NodeCollection<AttributeNode> curAttributes;
        private InterfaceNode curInterface;

        /// <summary>
        /// used by ParseYieldStatement to check that an iterator is really iterator : 
        /// A class which inherits from the IIterator interface could be iterator and it could be not iterator.
        /// It depends on its return type.
        /// </summary>
        private IIterator curIterator;

        /// <summary>
        /// many syntax checks need to access method parameter list
        /// </summary>
        private MethodNode curMethod;

        private Modifier curmods;
        private OperatorNode curOperator;
        private string currentDocComment = string.Empty;

        private ParseStateCollection curState;
        private Token curtok;

        private Dictionary<string, Constraint> curTypeParameterConstraints;

        /// <summary>
        /// can not be Dictionary<string, TypeParameterNode> because in the case
        /// class test : i<X,X>, the keys 'X' will be duplicated
        /// </summary>
        private List<TypeParameterNode> curTypeParameters;

        private Stack<ExpressionNode> exprStack;

        /// <summary>
        /// stores all parsed generic type
        /// </summary>
        private List<IGeneric> genericList;

        /// <summary>
        /// to travel information from the try to catch to notify it that the try has a 'yield return'
        /// </summary>
        private bool hasYieldReturnInTry;

        private bool inPPDirective;
        /// <summary>
        /// this flag is set when we enter in an anonymous delegate declaration
        /// this is an integer because we can declare anonymous delegate in anonymous delegate ... 
        /// </summary>
        private int isAnonymous;

        private int isCatch;
        private int isFinally;
        private bool isLocalConst;
        private bool isNewStatement;

        /// <summary>
        /// this flag is set when we enter in an try, catch or finally block
        /// this is an integer because we can declare try another try ... 
        /// </summary>
        private int isTry;

        /// <summary>
        /// this flag is set when we enter in an unsafe block
        /// this is an integer because we can declare unsafe block in unsafe block ... 
        /// 
        /// It is usefull to travel the unsafe state in child block : 
        /// for example, a block declared in an unsafe block, i unsafe, so 
        /// it inherits the unsafe state of his parent block
        /// </summary>
        private int isUnsafe;

        //iterator constant
        /// <summary>
        /// this field is used to keep a list of interface that made a method an iterator
        /// </summary>
        private StringCollection iteratorsClass;

        private int lineCount = 1;
        #pragma warning disable
        private bool mayBeLocalDecl;
        #pragma warning restore
        private Stack<NamespaceNode> namespaceStack;
        private bool nextIsPartial;

        /// <summary>
        /// This is always the following token of the curtok.
        /// Don't use it for lookahead though, as it may be a whitespace, newline, comment or hash token!!!
        /// </summary>
        private LinkedListNode<Token> nextTokNode;

        private bool ppCondition;
        private List<string> strings;
        private TokenCollection tokens;
        private Stack<ClassNode> typeStack;

        static Parser()
        {
            // all default to zero
            modMap = new Modifier[0xFF];

            modMap[(int)TokenID.New] = Modifier.New;
            modMap[(int)TokenID.Public] = Modifier.Public;
            modMap[(int)TokenID.Protected] = Modifier.Protected;
            modMap[(int)TokenID.Internal] = Modifier.Internal;
            modMap[(int)TokenID.Private] = Modifier.Private;
            modMap[(int)TokenID.Abstract] = Modifier.Abstract;
            modMap[(int)TokenID.Sealed] = Modifier.Sealed;
            modMap[(int)TokenID.Static] = Modifier.Static;
            modMap[(int)TokenID.Virtual] = Modifier.Virtual;
            modMap[(int)TokenID.Override] = Modifier.Override;
            modMap[(int)TokenID.Extern] = Modifier.Extern;
            modMap[(int)TokenID.Readonly] = Modifier.Readonly;
            modMap[(int)TokenID.Volatile] = Modifier.Volatile;
            modMap[(int)TokenID.Ref] = Modifier.Ref;
            modMap[(int)TokenID.Out] = Modifier.Out;
            //			modMap[(int)TokenID.Assembly] = Modifier.Assembly;
            //			modMap[(int)TokenID.Field] = Modifier.Field;
            modMap[(int)TokenID.Event] = Modifier.Event;
            //			modMap[(int)TokenID.Method] = Modifier.Method;
            //			modMap[(int)TokenID.Param] = Modifier.Param;
            //			modMap[(int)TokenID.Property] = Modifier.Property;
            modMap[(int)TokenID.Return] = Modifier.Return;
            //			modMap[(int)TokenID.Type] = Modifier.Type;
            //            modMap[(int)TokenID.Partial] = Modifier.Partial;
            modMap[(int)TokenID.Unsafe] = Modifier.Unsafe;
            modMap[(int)TokenID.Fixed] = Modifier.Fixed;

            // all default to zero
            precedence = new byte[0xFF];

            // these start at 80 for no particular reason
            precedence[(int)TokenID.LBracket] = PRECEDENCE_PRIMARY;
            precedence[(int)TokenID.Dot] = PRECEDENCE_PRIMARY;
            precedence[(int)TokenID.MinusGreater] = PRECEDENCE_PRIMARY;


            precedence[(int)TokenID.LParen] = PRECEDENCE_UNARY;		// 0x80
            precedence[(int)TokenID.Not] = PRECEDENCE_UNARY;
            precedence[(int)TokenID.Tilde] = PRECEDENCE_UNARY;
            precedence[(int)TokenID.PlusPlus] = PRECEDENCE_UNARY;
            precedence[(int)TokenID.MinusMinus] = PRECEDENCE_UNARY;
            precedence[(int)TokenID.Star] = PRECEDENCE_MULTIPLICATIVE; // 0x7f
            precedence[(int)TokenID.Slash] = PRECEDENCE_MULTIPLICATIVE;
            precedence[(int)TokenID.Percent] = PRECEDENCE_MULTIPLICATIVE;
            precedence[(int)TokenID.Plus] = 0x7E;
            precedence[(int)TokenID.Minus] = 0x7E;
            precedence[(int)TokenID.ShiftLeft] = 0x7D;
            precedence[(int)TokenID.ShiftRight] = 0x7D;
            precedence[(int)TokenID.Less] = 0x7C;
            precedence[(int)TokenID.Greater] = 0x7C;
            precedence[(int)TokenID.LessEqual] = 0x7C;
            precedence[(int)TokenID.GreaterEqual] = 0x7C;
            precedence[(int)TokenID.Is] = 0x7C;
            precedence[(int)TokenID.As] = 0x7C;
            precedence[(int)TokenID.EqualEqual] = 0x7B;
            precedence[(int)TokenID.NotEqual] = 0x7B;
            precedence[(int)TokenID.BAnd] = 0x7A;
            precedence[(int)TokenID.BXor] = 0x79;
            precedence[(int)TokenID.BOr] = 0x78;
            precedence[(int)TokenID.And] = 0x77;
            precedence[(int)TokenID.Or] = 0x76;
            precedence[(int)TokenID.QuestionQuestion] = 0x75;
            precedence[(int)TokenID.Question] = 0x74;

            precedence[(int)TokenID.Equal] = 0x73;
            precedence[(int)TokenID.PlusEqual] = 0x73;
            precedence[(int)TokenID.MinusEqual] = 0x73;
            precedence[(int)TokenID.StarEqual] = 0x73;
            precedence[(int)TokenID.SlashEqual] = 0x73;
            precedence[(int)TokenID.PercentEqual] = 0x73;
            precedence[(int)TokenID.BAndEqual] = 0x73;
            precedence[(int)TokenID.BOrEqual] = 0x73;
            precedence[(int)TokenID.BXorEqual] = 0x73;
            precedence[(int)TokenID.ShiftLeftEqual] = 0x73;
            precedence[(int)TokenID.ShiftRightEqual] = 0x73;


            preprocessor = new Dictionary<string, PreprocessorID>();

            preprocessor.Add("define", PreprocessorID.Define);
            preprocessor.Add("undef", PreprocessorID.Undef);
            preprocessor.Add("if", PreprocessorID.If);
            preprocessor.Add("elif", PreprocessorID.Elif);
            preprocessor.Add("else", PreprocessorID.Else);
            preprocessor.Add("endif", PreprocessorID.Endif);
            preprocessor.Add("line", PreprocessorID.Line);
            preprocessor.Add("error", PreprocessorID.Error);
            preprocessor.Add("warning", PreprocessorID.Warning);
            preprocessor.Add("region", PreprocessorID.Region);
            preprocessor.Add("endregion", PreprocessorID.Endregion);
            preprocessor.Add("pragma", PreprocessorID.Pragma);

        }

        public Parser() : this(string.Empty) {}

        public Parser(string currentFileName)
        {
            this.currentFileName = currentFileName;
        }

        #region Name Resolution

        private readonly Context currentContext = new Context();
        private readonly NameResolutionTable nameTable = new NameResolutionTable();

        private void EnterNamespace(string qualifiedName)
        {
            string[] nameParts = qualifiedName.Split(Type.Delimiter);

            for (int i = 0; i < nameParts.Length; i++)
            {
                nameTable.AddIdentifier(new NamespaceName(nameParts[i], currentContext));
                currentContext.Enter(nameParts[i], true);
            }
        }

        private void LeaveNamespace(string qualifiedName)
        {
            int index = -1;

            while ((index = qualifiedName.IndexOf(Type.Delimiter, index + 1)) != -1)
            {
                currentContext.Leave();
            }

            currentContext.Leave();
        }

        private static NameVisibilityRestriction ToVisibilityRestriction(Modifier modifier)
        {
            Modifier relevantMods = modifier & Modifier.Accessibility;
            NameVisibilityRestriction restriction = NameVisibilityRestriction.Self;

            if ((relevantMods & Modifier.Protected) != Modifier.Empty)
            {
                restriction = NameVisibilityRestriction.Family;
            }

            if (((relevantMods & Modifier.Internal) != Modifier.Empty) ||
                ((relevantMods & Modifier.Public) != Modifier.Empty))
            {
                restriction = NameVisibilityRestriction.Everyone;
            }

            return restriction;
        }
        #endregion

        public ParseStateCollection CurrentState
        {
            get { return curState; }
        }


        public CompilationUnitNode Parse(TokenCollection tokens, List<string> strings)
        {
            this.tokens = tokens;
            this.strings = strings;
            curmods = Modifier.Empty;
            curAttributes = new NodeCollection<AttributeNode>();
            curTypeParameters = new List<TypeParameterNode>();
            curTypeParameterConstraints = new Dictionary<string, Constraint>();

            blockStack = new Stack<BlockStatement>();

            curState = new ParseStateCollection();

            cu = new CompilationUnitNode();
            namespaceStack = new Stack<NamespaceNode>();
            namespaceStack.Push(cu.DefaultNamespace);
            typeStack = new Stack<ClassNode>();
            genericList = new List<IGeneric>();
            constructedTypes = new Dictionary<string, ConstructedTypeNode>();

            iteratorsClass = new StringCollection();

            iteratorsClass.Add("IEnumerator");
            iteratorsClass.Add("IEnumerable");
            iteratorsClass.Add("Collections.IEnumerator");
            iteratorsClass.Add("Collections.IEnumerable");
            iteratorsClass.Add("System.Collections.IEnumerator");
            iteratorsClass.Add("System.Collections.IEnumerable");

            iteratorsClass.Add("IEnumerator<>");
            iteratorsClass.Add("IEnumerable<>");
            iteratorsClass.Add("Generic.IEnumerator<>");
            iteratorsClass.Add("Generic.IEnumerable<>");
            iteratorsClass.Add("Collections.Generic.IEnumerator<>");
            iteratorsClass.Add("Collections.Generic.IEnumerable<>");
            iteratorsClass.Add("System.Collections.Generic.IEnumerator<>");
            iteratorsClass.Add("System.Collections.Generic.IEnumerable<>");

            exprStack = new Stack<ExpressionNode>();

            // begin parse
            nextTokNode = tokens.First;
            Advance();
            try
            {
                ParseNamespaceOrTypes();
            }
            catch
            {
                Console.WriteLine("Crashed for token: " + curtok + " at line " + curtok.Line + " column " + curtok.Col);
                throw;
            }

            cu.NameTable = nameTable;

            return cu;
        }

        private void ParseNamespaceOrTypes()
        {
            while (!curtok.Equals(EOF))
            {
                // todo: account for assembly attributes
                ParsePossibleAttributes(true);
                if (curAttributes.Count > 0)
                {
                    for (int i = 0; i < curAttributes.Count; ++i)
                    {
                        AttributeNode an = curAttributes[i];
                        if ((an.Modifiers & Modifier.Assembly) == Modifier.Assembly
                            || (an.Modifiers & Modifier.Module) == Modifier.Module)
                        {
                            cu.DefaultNamespace.Attributes.Add(an);
                            curAttributes.RemoveAt(i);
                            i--;
                        }
                    }
                    //curAttributes.Clear();
                }

                // can be usingDirectives, globalAttribs, or NamespaceMembersDecls
                // NamespaceMembersDecls include namespaces, class, struct, interface, enum, delegate
                switch (curtok.ID)
                {
                    case TokenID.Using:
                        // using directive
                        ParseUsingDirectives();
                        break;

                    case TokenID.New:
                    case TokenID.Public:
                    case TokenID.Protected:
                    case TokenID.Internal:
                    case TokenID.Private:
                    case TokenID.Abstract:
                    case TokenID.Sealed:
                    case TokenID.Static:
                    case TokenID.Unsafe:
                        //parseTypeModifier();
                        curmods |= modMap[(int)curtok.ID];
                        Advance();
                        break;

                    case TokenID.Namespace:
                        ParseNamespace();
                        break;

                    case TokenID.Class:
                        ParseClass();
                        break;

                    case TokenID.Struct:
                        ParseStruct();
                        break;

                    case TokenID.Interface:
                        ParseInterface();
                        break;

                    case TokenID.Enum:
                        ParseEnum();
                        break;

                    case TokenID.Delegate:
                        ParseDelegate();
                        break;

                    case TokenID.Semi:
                        Advance();
                        break;
                    case TokenID.Extern:
                        ParseExternAlias();
                        break;

                    case TokenID.Ident:
                        if (strings[curtok.Data] == "partial")
                        {
                            Advance();

                            if (curtok.ID != TokenID.Class
                                    && curtok.ID != TokenID.Interface
                                    && curtok.ID != TokenID.Struct)
                            {
                                RecoverFromError("Only class, struct and interface may be declared partial.",
                                    TokenID.Class, TokenID.Struct, TokenID.Interface);
                            }
                            else
                            {
                                nextIsPartial = true;
                            }
                            break;
                        }
                        goto default;

                    case TokenID.RCurly:
                        return;

                    case TokenID.Eof:
                        if (namespaceStack.Count != 1)
                            ReportError("Unexpected EOF", curtok);
                        return;

                    default:
                        ReportError("Unexpected token", curtok);
                        return;
                }
            }
        }

        private void ParseExternAlias()
        {
            Advance();	// over extern
            if (curtok.ID != TokenID.Ident || strings[curtok.Data] != "alias")
                ReportError("Expected 'alias', but found: " + curtok.ID, curtok);
            Advance();	// over alias

            ExternAliasDirectiveNode node = new ExternAliasDirectiveNode(curtok);
            node.ExternAliasName = ParseIdentifierOrKeyword(false, false, false, false, false);

            namespaceStack.Peek().ExternAliases.Add(node);
        }

        private void ParseUsingDirectives()
        {
            do
            {
                Advance();
                UsingDirectiveNode node = new UsingDirectiveNode(curtok);

                QualifiedIdentifierExpression nameOrAlias = ParseQualifiedIdentifier(true, false, false);
                if (curtok.ID == TokenID.Equal)
                {
                    Advance();

                    if (nameOrAlias.Expressions.Count > 1)
                        ReportError("An alias name may not be qualified", nameOrAlias.RelatedToken);

                    // an alias could be written like 
                    // using alias = path.identifier
                    //
                    // or like
                    // using alias = path.identifier<object>
                    //
                    // it is not possible to know before if this a type or an qualified identifier
                    // so we always parse it as a Qualified expression, and if the result is not a type
                    // we keep only the identifier part

                    QualifiedIdentifierExpression target = ParseQualifiedIdentifier(true, false, false);

                    if (target.IsType)
                    {
                        node.Target = new TypeNode(target);
                    }
                    else
                    {
                        // it does not mean that this is not a type.
                        // it only means that actually we can not resolve the type
                        // but in a next stage, we will probably resolve it as a type.
                        node.Target = target;
                    }

                    node.AliasName = nameOrAlias.Expressions[0] as IdentifierExpression;
                    if (node.AliasName == null)
                    {
                        ReportError("An alias name must be an identifier", nameOrAlias.RelatedToken);
                        node.AliasName = IdentifierExpression.GetErrorIdentifier(nameOrAlias.RelatedToken);
                    }
                }
                else
                {
                    node.Target = nameOrAlias;
                }

                AssertAndAdvance(TokenID.Semi);

                namespaceStack.Peek().UsingDirectives.Add(node);

                currentContext.AddUsingDirective(node);

            } while (curtok.ID == TokenID.Using);
        }

        private bool EvalPPExpression(ExpressionNode expr)
        {
            BinaryExpression binExpr = expr as BinaryExpression;
            if (binExpr != null)
            {
                switch (binExpr.Op)
                {
                    case TokenID.Or:
                        return EvalPPExpression(binExpr.Left) || EvalPPExpression(binExpr.Right);
                    case TokenID.And:
                        return EvalPPExpression(binExpr.Left) && EvalPPExpression(binExpr.Right);
                    case TokenID.EqualEqual:
                        return EvalPPExpression(binExpr.Left) == EvalPPExpression(binExpr.Right);
                    case TokenID.NotEqual:
                        return EvalPPExpression(binExpr.Left) != EvalPPExpression(binExpr.Right);
                    default:
                        ReportError("Illegal binary preprocessor expression", binExpr.RelatedToken);
                        return false;
                }
            }

            UnaryExpression unExpr = expr as UnaryExpression;
            if (unExpr != null)
            {
                if (unExpr.Op == TokenID.Not)
                    return !EvalPPExpression(unExpr.Child);
                ReportError("Illegal unary preprocessor expression", binExpr.RelatedToken);
                return false;
            }

            IdentifierExpression idExpr = expr as IdentifierExpression;
            if (idExpr != null)
                return cu.PPDefs.ContainsKey(idExpr.Identifier);

            BooleanPrimitive boolPrim = expr as BooleanPrimitive;
            if (boolPrim != null)
                return boolPrim.Value;

            ParenthesizedExpression parenExpr = expr as ParenthesizedExpression;
            if (parenExpr != null)
                return EvalPPExpression(parenExpr.Expression);

            ReportError("Illegal preprocessor expression", expr.RelatedToken);
            return false;
        }

        private bool SameLine(int oldLine)
        {
            if (curtok.Line != oldLine)
            {
                ReportError("Unexpected end of line", curtok);
                return false;
            }
            return true;
        }

        private void ParsePreprocessorExpressionSegment(int oldLine)
        {
            int startStackCount = exprStack.Count;
            TokenID startToken = curtok.ID;
            switch (curtok.ID)
            {
                case TokenID.True:
                    exprStack.Push(new BooleanPrimitive(true, curtok));
                    Advance();
                    break;

                case TokenID.False:
                    exprStack.Push(new BooleanPrimitive(false, curtok));
                    Advance();
                    break;

                case TokenID.And:
                case TokenID.Or:
                    Advance();
                    if (!SameLine(oldLine)) return;

                    BinaryExpression bNode = new BinaryExpression(startToken, curtok);
                    bNode.Left = exprStack.Pop();
                    exprStack.Push(bNode); // push node
                    ParsePreprocessorExpressionSegment(oldLine); // right side
                    // consume now or let next op consume?
                    while (precedence[(int)curtok.ID] > precedence[(int)startToken] && curtok.Line == oldLine)
                    {
                        ParsePreprocessorExpressionSegment(oldLine);
                    }

                    bNode.Right = exprStack.Pop();
                    break;

                case TokenID.Not:
                    Advance();
                    do
                    {
                        if (!SameLine(oldLine)) return;
                        ParsePreprocessorExpressionSegment(oldLine);
                    }
                    while (precedence[(int)curtok.ID] >= precedence[(int)TokenID.LParen]);      // LParen for precedence of unary operator

                    ExpressionNode node = exprStack.Pop();
                    UnaryExpression uNode = new UnaryExpression(startToken, node.RelatedToken);
                    uNode.Child = node;
                    exprStack.Push(uNode);
                    break;

                case TokenID.LParen:
                    Advance();
                    if (!SameLine(oldLine)) return;
                    exprStack.Push(new ParenthesizedExpression(ParsePreprocessorExpression()));
                    if (!SameLine(oldLine)) return;
                    AssertAndAdvance(TokenID.RParen);
                    break;


                case TokenID.Ident:
                    exprStack.Push(ParseIdentifierOrKeyword(false, false, false, false, false));
                    break;

                default:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        goto case TokenID.Ident;
                    }
                    else
                    {
                        ReportError("Unexpected token", curtok);
                    }
                    break;
            }
        }

        private ExpressionNode ParsePreprocessorExpression()
        {
            TokenID id = curtok.ID;
            int oldLine = curtok.Line;
            bool lineChanged = false;

            while (!lineChanged && id != TokenID.Eof &&
                    id != TokenID.Semi && id != TokenID.RParen &&
                    id != TokenID.Comma && id != TokenID.Colon)
            {
                ParsePreprocessorExpressionSegment(oldLine);
                id = curtok.ID;
                if (curtok.Line != oldLine)
                {
                    lineChanged = true;
                }
            }
            return exprStack.Pop();
        }

        private PPNode ParsePreprocessorDirective()
        {
            PPNode result = null;
            int startLine = lineCount;

            inPPDirective = true;
            Advance(); // over hash

            IdentifierExpression ie = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
            string ppKind = ie.Identifier;

            PreprocessorID id = PreprocessorID.Empty;
            if (preprocessor.ContainsKey(ppKind))
            {
                id = preprocessor[ppKind];
            }
            else
            {
                ReportError("Preprocessor directive must be valid identifier, rather than \"" + ppKind + "\".");
            }

            switch (id)
            {
                case PreprocessorID.Define:
                    // conditional-symbol pp-newline
                    if (curtok.ID == TokenID.True || curtok.ID == TokenID.False)
                    {
                        ReportError("Conditional symbol may neither be true nor false");
                        Advance();
                        break;
                    }
                    IdentifierExpression def = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
                    if (!cu.PPDefs.ContainsKey(def.Identifier))
                    {
                        cu.PPDefs.Add(def.Identifier, PreprocessorID.Empty);
                    }
                    result = new PPDefineNode(def);
                    break;
                case PreprocessorID.Undef:
                    // conditional-symbol pp-newline
                    IdentifierExpression undef = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
                    if (cu.PPDefs.ContainsKey(undef.Identifier))
                    {
                        cu.PPDefs.Remove(undef.Identifier);
                    }
                    //					result = new PPDefineNode(undef );
                    break;
                case PreprocessorID.If:
                    // pp-expression pp-newline conditional-section(opt)
                    ppCondition = EvalPPExpression(ParsePreprocessorExpression());

                    //result = new PPIfNode(ParseExpressionToNewline());
                    if (!ppCondition)
                    {
                        // skip this block
                        SkipToElseOrEndIf();
                    }
                    break;
                case PreprocessorID.Elif:
                    // pp-expression pp-newline conditional-section(opt)
                    if (ppCondition)   // a previous part was already true?
                    {
                        SkipToElseOrEndIf();
                        break;
                    }

                    ppCondition = EvalPPExpression(ParsePreprocessorExpression());
                    if (!ppCondition)
                    {
                        // skip this block
                        SkipToElseOrEndIf();
                    }
                    break;
                case PreprocessorID.Else:
                    // pp-newline conditional-section(opt)
                    if (ppCondition)
                    {
                        // skip this block
                        SkipToElseOrEndIf();
                    }
                    break;
                case PreprocessorID.Endif:
                    // pp-newline
                    result = new PPEndIfNode(curtok);
                    ppCondition = false;
                    break;
                case PreprocessorID.Line:
                    // line-indicator pp-newline
                    SkipToEOL(startLine);
                    break;
                case PreprocessorID.Error:
                    // pp-message
                    SkipToEOL(startLine);
                    break;
                case PreprocessorID.Warning:
                    // pp-message
                    SkipToEOL(startLine);
                    break;
                case PreprocessorID.Region:
                    // pp-message
                    SkipToEOL(startLine);
                    break;
                case PreprocessorID.Endregion:
                    // pp-message
                    SkipToEOL(startLine);
                    break;
                case PreprocessorID.Pragma:
                    {
                        // pp-message
                        //pragma-warning-body:
                        //  warning   whitespace   warning-action
                        //  warning   whitespace   warning-action   whitespace   warning-list
                        int start_line = curtok.Line;

                        PPPragmaNode pppnode = new PPPragmaNode(curtok);
                        result = pppnode;

                        if (curtok.Line == start_line)
                        {
                            pppnode.Identifier = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
                        }

                        if (curtok.Line == start_line)
                        {
                            string paction = ((IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false)).Identifier;
                            pppnode.Action = (PragmaAction)Enum.Parse(typeof(PragmaAction), paction);
                        }

                        while (curtok.Line == start_line)
                        {
                            if (curtok.ID != TokenID.Comma)
                            {
                                if (curtok.ID == TokenID.IntLiteral)
                                {
                                    pppnode.Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.Int, curtok)));
                                }
                                else
                                {
                                    if (curtok.ID == TokenID.UIntLiteral)
                                    {
                                        pppnode.Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.UInt, curtok)));
                                    }
                                    else
                                    {
                                        if (curtok.ID == TokenID.LongLiteral)
                                        {
                                            pppnode.Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.Long, curtok)));
                                        }
                                        else
                                        {
                                            if (curtok.ID == TokenID.ULongLiteral)
                                            {
                                                pppnode.Value.Add(new ConstantExpression(new IntegralPrimitive(strings[curtok.Data], IntegralType.ULong, curtok)));
                                            }
                                            else
                                            {
                                                RecoverFromError(TokenID.IntLiteral);
                                            }
                                        }
                                    }
                                }
                            }

                            Advance();
                        }
                        break;
                    }
                default:
                    break;
            }
            inPPDirective = false;
            return result;
        }

        private void ParsePossibleDocComment()
        {
            if (curtok.ID == TokenID.DocComment
                || curtok.ID == TokenID.SingleComment
                || curtok.ID == TokenID.MultiComment)
            {
                currentDocComment += strings[curtok.Data];
            }
        }

        private void ParsePossibleAttributes(bool isGlobal)
        {
            while (curtok.ID == TokenID.LBracket)
            {
                // it is neccessary to save current typeparameter
                // because an attribute can be declared for a type parameter
                // so if we do not do that, the typeparameter will be considered
                // as the type parameter of his attribute declaration ...
                // i.e.: 
                // class cons <[GenPar] A, [GenPar] B>{}
                //
                // without backup, A is considered as the type parameter of GenPar, 
                // and the parser will generate the wrong output:  class cons <[GenPar<A>]...

                List<TypeParameterNode> backupTypeParameters = curTypeParameters;
                curTypeParameters = new List<TypeParameterNode>();
                Dictionary<string, Constraint> backupConstraints = curTypeParameterConstraints;
                curTypeParameterConstraints = new Dictionary<string, Constraint>();

                Advance(); // advance over LBracket token

                if (curmods != Modifier.Empty)
                    ReportError("Internal compiler error: Modifiers unused!", curtok);

                Modifier curAttribMods = ParseAttributeModifiers();

                if (isGlobal && curAttribMods == Modifier.GlobalAttributeMods)
                {
                    // nothing to check, globally positioned attributes can still apply to namespaces etc
                }
                else
                {
                    if ((curAttribMods & ~Modifier.AttributeMods) != 0)
                        ReportError("Attribute contains illegal modifiers.");
                }

                if (curAttribMods != Modifier.Empty)
                    AssertAndAdvance(TokenID.Colon);

                while (curtok.ID != TokenID.RBracket && curtok.ID != TokenID.Eof)
                {
                    AttributeNode node = new AttributeNode(curtok);
                    curAttributes.Add(node);
                    node.Modifiers = curAttribMods;
                    node.Name = ParseQualifiedIdentifier(true, false, false);

                    if (curtok.ID == TokenID.LParen)
                    {
                        // named argument
                        //gtest-286, line 16
                        // [Test(typeof(C<string>))]
                        // public static void Foo()
                        // {
                        // }
                        //
                        // the attribute is applied to the type parameter, so we back up it.
                        NodeCollection<AttributeNode> backupAttributes = curAttributes;
                        curAttributes = new NodeCollection<AttributeNode>();

                        // has attribute arguments
                        Advance(); // over lparen

                        // positional args are just expr
                        bool hadAssign = false;
                        while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.RParen)
                        {
                            AttributeArgumentNode aNode = new AttributeArgumentNode(curtok);
                            node.Arguments.Add(aNode);
                            ExpressionNode expr = ParseExpression();
                            AssignmentExpression assignExpr = expr as AssignmentExpression;
                            if (assignExpr != null)
                            {
                                IdentifierExpression idExpr = assignExpr.Variable as IdentifierExpression;
                                if (idExpr == null)
                                    ReportError("Identifier expected", assignExpr.Variable.RelatedToken);
                                aNode.ArgumentName = idExpr;
                                aNode.Expression = assignExpr.RightSide;
                                hadAssign = true;
                            }
                            else
                                aNode.Expression = expr;

                            if (curtok.ID != TokenID.Comma)
                                goto endofattrargs;
                            Advance();  // over comma

                            if (curtok.ID == TokenID.RParen)
                                ReportError("Unexpected token ')'", curtok);

                            if (hadAssign) break;
                        }

                        // named args are ident = expr
                        while (curtok.ID == TokenID.Ident)
                        {
                            AttributeArgumentNode aNode = new AttributeArgumentNode(curtok);
                            aNode.ArgumentName = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
                            AssertAndAdvance(TokenID.Equal);
                            aNode.Expression = ParseExpression();
                            node.Arguments.Add(aNode);

                            if (curtok.ID != TokenID.Comma) break;
                            Advance(); // over comma

                            if (curtok.ID == TokenID.RParen)
                                ReportError("Unexpected token ')'", curtok);
                        }
                    endofattrargs:
                        AssertAndAdvance(TokenID.RParen);  // over rparen

                        // restore the backup
                        curAttributes = backupAttributes;
                    }

                    if (curtok.ID != TokenID.Comma) break;
                    Advance(); // over comma
                }
                AssertAndAdvance(TokenID.RBracket); // over rbracket

                curTypeParameters = backupTypeParameters;
                curTypeParameterConstraints = backupConstraints;
            }
        }
        private void ParseNamespace()
        {
            Advance(); // advance over Namespace token

            NamespaceNode node = new NamespaceNode(curtok);

            ApplyAttributes(node);
            ApplyDocComment(node);

            if (curmods != Modifier.Empty)
                ReportError("Namespace can not contain modifiers");

            if (namespaceStack.Count == 1 /*one -> default namespace only*/)
            {
                cu.Namespaces.Add(node);
            }
            else
            {
                namespaceStack.Peek().Namespaces.Add(node);
            }

            namespaceStack.Push(node);


            node.Name = ParseQualifiedIdentifier(false, false, false);

            AssertAndAdvance(TokenID.LCurly);

            EnterNamespace(node.Name.QualifiedIdentifier);

            ParseNamespaceOrTypes();

            LeaveNamespace(node.Name.QualifiedIdentifier);

            AssertAndAdvance(TokenID.RCurly);
            namespaceStack.Pop();
        }

        /// <summary>
        /// it returns the current qualified name space plus classes path 
        /// </summary>
        /// <returns></returns>
        private string GetQualifiedNameSpace()
        {
            string ret = string.Empty;

            // first parse namespaces
            foreach (NamespaceNode ns in namespaceStack)
            {
                if (ns.Name != null)
                {
                    ret += ns.Name.QualifiedIdentifier;
                    ret += ".";
                }
            }

            // now parse the "type path"
            foreach (ClassNode cn in typeStack)
            {
                ret += cn.GenericIdentifier;
                ret += ".";
            }


            return ret.TrimEnd('.');
        }


        // types
        /// <summary>
        /// this method is used to check that a type name is not already 
        /// declared in the same namespace. 
        /// It also do the link between partials types
        /// </summary>
        /// <param name="node"></param>
        private void CheckTypeUnicityAndAdd(ConstructedTypeNode node)
        {
            string key = GetQualifiedNameSpace() + "." + node.GenericIdentifier;

            //checks that this type does not exist in this namespace ( from this file or another parsed file )
            if (constructedTypes.ContainsKey(key))
            {
                ConstructedTypeNode otherType = constructedTypes[key];

                if (otherType.Kind == node.Kind
                    || otherType.IsPartial
                    || node.IsPartial)
                {
                    if (otherType.IsPartial && !node.IsPartial
                        || node.IsPartial && !otherType.IsPartial
                        )
                    {
                        //one of the two types is declared as partial.
                        //so maybe is missing the partial modifier for one of these types
                        ReportError("The " + node.Kind.ToString().ToLower() + " "
                                            + node.GenericIdentifier
                                            + " is already declared in the namespace. Is missing partial modifier?");
                    }
                    else
                    {
                        //the first declared partial type become the partial container
                        if (otherType.Partials == null)
                        {
                            otherType.Partials = new PartialCollection();
                        }

                        //all partial classes must have the same modifiers set ( public, protected, internal and private )
                        if (otherType.Modifiers != Modifier.Empty && node.Modifiers != Modifier.Empty
                                && (otherType.Modifiers & Modifier.Accessibility) != (node.Modifiers & Modifier.Accessibility))
                        {
                            ReportError("All partial types declarations must have the same modifiers set");
                        }

                        if (otherType.IsGeneric)
                        {
                            //all type parameter must have the same name
                            foreach (TypeParameterNode item in otherType.Generic.TypeParameters)
                            {
                                if (!node.Generic.TypeParameters.Contains(item))
                                {
                                    ReportError("Type parameter must have the same name in all partial declarations.");
                                    break;
                                }
                            }

                            // partial declaration with constrainst must have the same constraint declarations.
                            // a partial declaration can have no constraint declaration.

                            ConstructedTypeNode typeWithConstraint = otherType;

                            //if the main partial container does not have any constraint, 
                            //we look for another partial declaration with constraint
                            foreach (IPartial p in typeWithConstraint.Partials)
                            {
                                if (((IGeneric)p).Generic.Constraints.Count > 0)
                                {
                                    typeWithConstraint = (ConstructedTypeNode)p;
                                    break;
                                }
                            }

                            if (typeWithConstraint != null
                                && typeWithConstraint.Generic.Constraints.Count > 0)
                            {
                                if (typeWithConstraint.Generic.Constraints.Count == node.Generic.Constraints.Count)
                                {
                                    // commented until we can resolve any type ( file gtest-129.cs generate a wrong error )

                                    //foreach (Constraint cst in typeWithConstraint.Generic.Constraints)
                                    //{
                                    //    //TODO : to resolve : see mono file gtest-129.cs.
                                    //    // it should not generate errors ... 
                                    //    if (!node.Generic.Constraints.Contains(cst))
                                    //    {
                                    //        ReportError("All partial declarations must declare the same constraint declarations.");
                                    //        break;
                                    //    }
                                    //}
                                }
                                else
                                {
                                    if (typeWithConstraint.Generic.Constraints.Count != 0
                                            && node.Generic.Constraints.Count != 0)
                                    {
                                        ReportError("All partial declarations must declare the same constraint declarations.");
                                    }
                                }
                            }

                            //same base class


                        }

                        otherType.Partials.Add(node);
                    }
                }
                else
                {
                    // one another type has the same name.
                    ReportError("The identifier " + node.GenericIdentifier + " is already declared in the namespace.");
                }
            }
            else
            {
                constructedTypes.Add(key, node);
            }
        }

        private BaseNode ParseClass()
        {
            uint classMask = ~(uint)Modifier.ClassMods;
            if (((uint)curmods & classMask) != (uint)Modifier.Empty)
                ReportError("Class contains illegal modifiers.");

            ClassNode node = new ClassNode(curtok);

            ApplyAttributes(node);
            ApplyDocComment(node);

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek(); //retrieve the parent class if existing;

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the class is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            if (cl != null)
            {
                CheckStaticClass(cl, node.Modifiers, false);
            }

            if (node.IsStatic)
            {
                if ((node.Modifiers & (Modifier.Sealed | Modifier.Abstract)) != Modifier.Empty)
                {
                    ReportError("A class delared as 'static' can not be declared nor 'sealed', nor 'abstract'.");
                }
            }

            Advance(); // advance over Class token

            if (curtok.ID != TokenID.Ident)
            {
                string msg = "Error: Expected " + TokenID.Ident + " found: " + curtok.ID;

                ReportError(msg);

                //to stay coherent with the rest of the parser
                //it generate a random class name 

                node.Name = new IdentifierExpression("no_name_" + (no_name_index++), node.RelatedToken);

            }
            else
            {
                node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true, false);
            }

            ParsePossibleTypeParameterNode(true, true, false, true);
            ApplyTypeParameters(node);

            if (curtok.ID == TokenID.Colon) // for base members
            {
                if (node.IsStatic)
                {
                    ReportError("Static class can not have nor base classes nor base interface.");
                }

                do
                {
                    Advance();
                    node.BaseClasses.Add(ParseType(false));
                }
                while (curtok.ID == TokenID.Comma);
            }

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                nameTable.AddIdentifier(new ClassName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    currentContext));
            }
            else
            {
                nameTable.AddIdentifier(new ClassName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    currentContext));
            }

            typeStack.Push(node);

            if (cl == null)
            {
                namespaceStack.Peek().Classes.Add(node);
            }
            else
            {
                cl.Classes.Add(node);
            }

            AssertAndAdvance(TokenID.LCurly);

            currentContext.Enter(node.Name.Identifier, false);

            while (curtok.ID != TokenID.RCurly && curtok.ID != TokenID.Eof) // guard for empty
            {
                ParseClassMember();
            }

            AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                isUnsafe--;
            }

            currentContext.Leave();
            typeStack.Pop();

            // case  :
            // class cl {
            // /// doc comment
            // }
            curAttributes = new NodeCollection<AttributeNode>();
            currentDocComment = string.Empty;

            return cl;
        }

        private BaseNode ParseInterface()
        {
            InterfaceNode node = new InterfaceNode(curtok);
            curInterface = node;

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

            uint interfaceMask = ~(uint)Modifier.InterfaceMods;
            if (((uint)curmods & interfaceMask) != (uint)Modifier.Empty)
                ReportError("Interface contains illegal modifiers");

            ApplyAttributes(node);
            ApplyDocComment(node);


            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
            //the interface is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            Advance(); // advance over Interface token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true, false);

            ParsePossibleTypeParameterNode(true, true, false, true);
            ApplyTypeParameters(node);

            if (curtok.ID == TokenID.Colon) // for base members
            {
                do
                {
                    Advance();
                    node.BaseClasses.Add(ParseType(false));
                }
                while (curtok.ID == TokenID.Comma);
            }

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                nameTable.AddIdentifier(new InterfaceName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    currentContext));
            }
            else
            {
                nameTable.AddIdentifier(new InterfaceName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    currentContext));
            }

            currentContext.Enter(node.Name.Identifier, false);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Interfaces.Add(node);
            }
            else
            {
                cl.Interfaces.Add(node);
            }

            AssertAndAdvance(TokenID.LCurly);

            while (curtok.ID != TokenID.RCurly) // guard for empty
            {
                ParseInterfaceMember();
            }

            AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            currentContext.Leave();

            curInterface = null;

            // case  :
            // class cl {
            // /// doc comment
            // }
            curAttributes = new NodeCollection<AttributeNode>();
            currentDocComment = string.Empty;

            return node;

        }
        private BaseNode ParseStruct()
        {
            StructNode node = new StructNode(curtok);

            node.IsPartial = nextIsPartial;
            nextIsPartial = false;

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            typeStack.Push(node);

            uint structMask = ~(uint)Modifier.StructMods;
            if (((uint)curmods & structMask) != (uint)Modifier.Empty)
                ReportError("Struct contains illegal modifiers");

            ApplyAttributes(node);
            ApplyDocComment(node);


            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
            //the struct is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            Advance(); // advance over Struct token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true, false);

            ParsePossibleTypeParameterNode(true, true, false, true);
            ApplyTypeParameters(node);

            if (curtok.ID == TokenID.Colon) // for base members
            {
                do
                {
                    Advance();
                    node.BaseClasses.Add(ParseType(false));
                }
                while (curtok.ID == TokenID.Comma);
            }

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            CheckTypeUnicityAndAdd(node);

            if (node.IsGeneric)
            {
                nameTable.AddIdentifier(new StructName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    currentContext));
            }
            else
            {
                nameTable.AddIdentifier(new StructName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    currentContext));
            }

            currentContext.Enter(node.Name.Identifier, false);

            if (cl == null)
            {
                namespaceStack.Peek().Structs.Add(node);
            }
            else
            {
                cl.Structs.Add(node);
            }

            AssertAndAdvance(TokenID.LCurly);

            while (curtok.ID != TokenID.RCurly) // guard for empty
            {
                ParseClassMember();
            }

            AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            currentContext.Leave();

            typeStack.Pop();

            // case  :
            // class cl {
            // /// doc comment
            // }
            curAttributes = new NodeCollection<AttributeNode>();
            currentDocComment = string.Empty;

            return node;
        }
        private BaseNode ParseEnum()
        {
            EnumNode node = new EnumNode(curtok);
            // todo: this needs to have any nested class info, or go in potential container class

            ApplyAttributes(node);
            ApplyDocComment(node);


            uint enumMask = ~(uint)Modifier.EnumMods;
            if (((uint)curmods & enumMask) != (uint)Modifier.Empty)
                ReportError("Enum contains illegal modifiers");

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            Advance(); // advance over Enum token
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true, false);

            if (curtok.ID == TokenID.Colon) // for base type
            {
                Advance();
                node.BaseClass = ParseType(false);
            }

            CheckTypeUnicityAndAdd(node);

            nameTable.AddIdentifier(new EnumName(node.Name.Identifier,
                ToVisibilityRestriction(node.Modifiers),
                currentContext));

            currentContext.Enter(node.Name.Identifier, false);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Enums.Add(node);
            }
            else
            {
                cl.Enums.Add(node);
            }

            AssertAndAdvance(TokenID.LCurly);

            NodeCollection<EnumNode> list = new NodeCollection<EnumNode>();
            node.Value = list;

            while (curtok.ID != TokenID.RCurly) // guard for empty
            {
                list.Add(ParseEnumMember());
                if (curtok.ID != TokenID.Comma) break;
                Advance();
            }

            AssertAndAdvance(TokenID.RCurly);

            if (curtok.ID == TokenID.Semi)
            {
                Advance();
            }

            currentContext.Leave();

            return node;
        }
        private BaseNode ParseDelegate()
        {
            DelegateNode node = new DelegateNode(curtok);

            ApplyAttributes(node);
            ApplyDocComment(node);


            uint delegateMask = ~(uint)Modifier.DelegateMods;
            if (((uint)curmods & delegateMask) != (uint)Modifier.Empty)
                ReportError("Delegate contains illegal modifiers");

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }
            //the delegate is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            Advance(); // advance over delegate token
            node.Type = ParseType(false);
            node.Name = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, true, false);

            ParsePossibleTypeParameterNode(true, true, false, true);
            ApplyTypeParameters(node);

            CheckTypeUnicityAndAdd(node);


            node.Params = ParseParamList();

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();

            if (cl == null)
            {
                namespaceStack.Peek().Delegates.Add(node);
            }
            else
            {
                cl.Delegates.Add(node);
            }

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            AssertAndAdvance(TokenID.Semi);

            if (node.IsGeneric)
            {
                nameTable.AddIdentifier(new DelegateName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    node.Generic.TypeParameters.ConvertAll(delegate(TypeParameterNode input) { return input.Identifier.Identifier; }).ToArray(),
                    currentContext));
            }
            else
            {
                nameTable.AddIdentifier(new DelegateName(node.Name.Identifier,
                    ToVisibilityRestriction(node.Modifiers),
                    currentContext));
            }

            return node;
        }

        private void CheckStaticClass(ClassNode cl, Modifier nodeModifier, bool checkStaticModifier)
        {
            if (cl.IsStatic)
            {
                if (checkStaticModifier
                    && ((nodeModifier & Modifier.Static) == Modifier.Empty))
                {
                    ReportError("Static class accepts only static members.");
                }

                if (((nodeModifier & (Modifier.Protected | Modifier.Internal)) == (Modifier.Protected | Modifier.Internal)))
                {
                    ReportError("Static class does not accept protected internal member.");
                }
                else
                {
                    //do it in the else condition of the previous if or the message
                    //will be duplicate
                    if (((nodeModifier & Modifier.Protected) == Modifier.Protected))
                    {
                        ReportError("Static class does not accept protected member.");
                    }
                }

            }

        }

        // members
        private BaseNode ParseClassMember()
        {
            BaseNode ret = null;

            // const field method property event indexer operator ctor ~ctor cctor typedecl
            ParsePossibleAttributes(false);
            ParseModifiers();
            switch (curtok.ID)
            {
                case TokenID.Class:
                    ret = ParseClass();
                    break;

                case TokenID.Interface:
                    ret = ParseInterface();
                    break;

                case TokenID.Struct:
                    ret = ParseStruct();
                    break;

                case TokenID.Enum:
                    ret = ParseEnum();
                    break;

                case TokenID.Delegate:
                    ret = ParseDelegate();
                    break;

                case TokenID.Const:
                    ret = ParseConst();
                    break;

                case TokenID.Event:
                    ret = ParseEvent();
                    break;

                case TokenID.Tilde:
                    ret = ParseDestructor();
                    break;

                case TokenID.Explicit:
                case TokenID.Implicit:
                    ret = ParseOperatorDecl(null);
                    break;
                case TokenID.RCurly:
                    break;
                default:
                    IType type = ParseType(false);
                    if (type == null)
                    {
                        ReportError("Expected type or ident in member definition");
                    }
                    switch (curtok.ID)
                    {
                        case TokenID.Operator:
                            ret = ParseOperatorDecl(type);
                            break;
                        case TokenID.LParen:
                            ret = ParseCtor((TypeNode)type);
                            break;
                        case TokenID.This: // can be iface.this too, see below
                            ret = ParseIndexer(type, null);
                            break;
                        default:
                            // this is qualified name because it could be
                            // an explicit interface member declaration
                            //HACK : ParseQualifiedIdentifier call PArseTypeParameter
                            //and PArseTypeParameter consume modifiers ... so we backup modifiers

                            Modifier backupMod = curmods;
                            curmods = Modifier.Empty;

                            QualifiedIdentifierExpression name2 = ParseQualifiedIdentifier(true, true, true);

                            if (name2 == null || name2.Expressions.Count == 0)
                            {
                                ReportError("Expected name or ident in member definition");
                            }

                            curmods = backupMod;

                            switch (curtok.ID)
                            {
                                case TokenID.This:
                                    ret = ParseIndexer(type, name2);
                                    break;
                                case TokenID.Comma:
                                case TokenID.Equal:
                                case TokenID.Semi:
                                    ret = ParseField(type, name2);
                                    break;
                                case TokenID.LBracket:
                                    ret = ParseFixedBuffer(type, name2);
                                    break;
                                case TokenID.Less:
                                case TokenID.LParen:
                                    ret = ParseMethod(type, name2);
                                    break;
                                case TokenID.LCurly:
                                    ret = ParseProperty(type, name2);
                                    break;
                                default:
                                    ReportError("Invalid member syntax");
                                    break;
                            }
                            break;
                    }
                    break;
            }

            return ret;
        }
        private BaseNode ParseInterfaceMember()
        {
            BaseNode ret = null;

            ParsePossibleAttributes(false);
            ParseModifiers();

            switch (curtok.ID)
            {
                case TokenID.RCurly:
                    break;
                case TokenID.Event:
                    // event
                    InterfaceEventNode node = new InterfaceEventNode(curtok);
                    curInterface.Events.Add(node);

                    ApplyAttributes(node);
                    ApplyDocComment(node);

                    node.Modifiers = curmods;
                    curmods = Modifier.Empty;
                    AssertAndAdvance(TokenID.Event);
                    node.Type = ParseType(false);
                    node.Names.Add(ParseQualifiedIdentifier(false, true, true));
                    AssertAndAdvance(TokenID.Semi);
                    ret = node;
                    break;
                default:
                    IType type = ParseType(false);
                    if (type == null)
                    {
                        ReportError("Expected type or ident in interface member definition.");
                    }
                    switch (curtok.ID)
                    {
                        case TokenID.This:
                            {
                                // interface indexer
                                InterfaceIndexerNode iiNode = new InterfaceIndexerNode(curtok);
                                curInterface.Indexers.Add(iiNode);
                                ApplyAttributes(iiNode);
                                ApplyDocComment(iiNode);

                                iiNode.Type = type;
                                Advance(); // over 'this'
                                iiNode.Params = ParseParamList(TokenID.LBracket, TokenID.RBracket);

                                bool hasGetter, hasSetter;
                                NodeCollection<AttributeNode> getAttrs, setAttrs;
                                ParseInterfaceAccessors(out hasGetter, out getAttrs,
                                    out hasSetter, out setAttrs);
                                iiNode.HasGetter = hasGetter;
                                if (getAttrs != null) iiNode.GetterAttributes = getAttrs;
                                iiNode.HasSetter = hasSetter;
                                if (setAttrs != null) iiNode.SetterAttributes = setAttrs;

                                ret = iiNode;
                                break;
                            }

                        default:
                            QualifiedIdentifierExpression name = ParseQualifiedIdentifier(false, true, true);
                            if (name == null || name.Expressions.Count == 0)
                            {
                                ReportError("Expected name or ident in member definition.");
                            }
                            switch (curtok.ID)
                            {
                                case TokenID.Less:
                                case TokenID.LParen:
                                    // method
                                    InterfaceMethodNode mnode = new InterfaceMethodNode(curtok);
                                    curInterface.Methods.Add(mnode);

                                    ApplyAttributes(mnode);
                                    ApplyDocComment(mnode);


                                    mnode.Modifiers = curmods;
                                    curmods = Modifier.Empty;

                                    mnode.Names.Add(name);
                                    mnode.Type = type;

                                    ParsePossibleTypeParameterNode(true, true, false, true);
                                    //if generic applies type parameter collection to the node
                                    ApplyTypeParameters(mnode);

                                    // starts at LParen

                                    mnode.Params = ParseParamList();

                                    ParsePossibleTypeParameterConstraintNode(mnode);
                                    ApplyTypeParameterConstraints(mnode);

                                    ret = mnode;

                                    AssertAndAdvance(TokenID.Semi);
                                    break;

                                case TokenID.LCurly:
                                    {
                                        // property
                                        InterfacePropertyNode pnode = new InterfacePropertyNode(curtok);
                                        curInterface.Properties.Add(pnode);

                                        ApplyAttributes(pnode);
                                        ApplyDocComment(pnode);


                                        pnode.Modifiers = curmods;
                                        curmods = Modifier.Empty;

                                        pnode.Names.Add(name);
                                        pnode.Type = type;

                                        bool hasGetter, hasSetter;
                                        NodeCollection<AttributeNode> getAttrs, setAttrs;
                                        ParseInterfaceAccessors(out hasGetter, out getAttrs,
                                            out hasSetter, out setAttrs);
                                        pnode.HasGetter = hasGetter;
                                        if (getAttrs != null) pnode.GetterAttributes = getAttrs;
                                        pnode.HasSetter = hasSetter;
                                        if (setAttrs != null) pnode.SetterAttributes = setAttrs;

                                        ret = pnode;

                                        if (curtok.ID == TokenID.Semi)
                                        {
                                            AssertAndAdvance(TokenID.Semi);
                                        }
                                        break;
                                    }

                                default:
                                    ReportError("Invalid interface member syntax.");
                                    break;
                            }
                            break;
                    }
                    break;
            }

            return ret;
        }

        private BaseNode ParseCtor(TypeNode type)
        {
            ConstructorNode node = new ConstructorNode(curtok);

            ApplyAttributes(node);
            ApplyDocComment(node);


            if ((curmods & Modifier.Static) != Modifier.Empty)
            {
                node.IsStaticConstructor = true;
                curmods = curmods & ~Modifier.Static;
            }
            uint mask = ~(uint)Modifier.ConstructorMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("constructor declaration contains illegal modifiers");

            ClassNode cl = typeStack.Peek();
            cl.Constructors.Add(node);

            //node.Attributes.Add(curAttributes);
            //curAttributes.Clear();
            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the constructor is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            if (node.IsStaticConstructor)
            {
                CheckStaticClass(cl, node.Modifiers | Modifier.Static, true);
            }
            else
            {
                CheckStaticClass(cl, node.Modifiers, true);
            }

            node.Type = type;
            QualifiedIdentifierExpression nameCtor = new QualifiedIdentifierExpression(node.RelatedToken);
            nameCtor.Expressions.Add(typeStack.Peek().Name);
            node.Names.Add(nameCtor);

            // starts at LParen
            node.Params = ParseParamList();

            if (curtok.ID == TokenID.Colon)
            {
                Advance();
                if (curtok.ID == TokenID.Base)
                {
                    Advance();
                    node.HasBase = true;
                    node.ThisBaseArgs = ParseArgs();
                }
                else if (curtok.ID == TokenID.This)
                {
                    Advance();
                    node.HasThis = true;
                    node.ThisBaseArgs = ParseArgs();
                }
                else
                {
                    RecoverFromError("constructor requires this or base calls after colon", TokenID.Base);
                }
            }
            ParseBlock(node.StatementBlock);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            return node;
        }
        private BaseNode ParseDestructor()
        {
            Advance(); // over tilde

            DestructorNode node = new DestructorNode(curtok);

            ApplyAttributes(node);
            ApplyDocComment(node);

            uint mask = ~(uint)Modifier.DestructorMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("destructor declaration contains illegal modifiers");

            ClassNode cl = typeStack.Peek();
            cl.Destructors.Add(node);

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the destructor is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true);

            if (curtok.ID == TokenID.Ident)
            {
                node.Names.Add(ParseQualifiedIdentifier(false, false, true));
            }
            else
            {
                ReportError("Destructor requires identifier as name.");
            }
            // no args in dtor
            AssertAndAdvance(TokenID.LParen);
            AssertAndAdvance(TokenID.RParen);

            ParseBlock(node.StatementBlock);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            return node;
        }
        private BaseNode ParseOperatorDecl(IType type)
        {
            OperatorNode node;

            //we check the return type of the method.
            // -> it determines if the method is an iterator.

            if (type != null //implicit/explicit operator 
                && IsIteratorClass(type))
            {
                node = new OperatorNode(true, curtok);
                curIterator = node;
            }
            else
            {
                node = new OperatorNode(false, curtok);
            }

            curOperator = node;

            ApplyAttributes(node);
            ApplyDocComment(node);


            uint mask = ~(uint)Modifier.OperatorMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("operator declaration contains illegal modifiers");

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the operator is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            ClassNode cl = typeStack.Peek();
            cl.Operators.Add(node);

            CheckStaticClass(cl, node.Modifiers, true);

            if (type == null && curtok.ID == TokenID.Explicit)
            {
                Advance();
                node.IsExplicit = true;
                AssertAndAdvance(TokenID.Operator);
                type = ParseType(false);
            }
            else if (type == null && curtok.ID == TokenID.Implicit)
            {
                Advance();
                node.IsImplicit = true;
                AssertAndAdvance(TokenID.Operator);
                type = ParseType(false);
            }
            else
            {
                AssertAndAdvance(TokenID.Operator);
                node.Operator = curtok.ID;
                Advance();
            }
            node.Type = type;

            NodeCollection<ParamDeclNode> paramList = ParseParamList();
            if (paramList.Count == 0 || paramList.Count > 2)
            {
                ReportError("Operator declarations must only have one or two parameters.");
            }
            node.Param1 = paramList[0];
            if (paramList.Count == 2)
            {
                node.Param2 = paramList[1];
            }
            ParseBlock(node.Statements);

            if (node.IsIterator)
            {
                if ((node.Param1.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                {
                    ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                }

                if ((node.Param2.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                {
                    ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                }
            }

            curIterator = null;
            curOperator = null;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            // Doubt this will be in use.
            if (node.IsImplicit)
                nameTable.AddIdentifier(new OperatorName(TokenID.Implicit, currentContext));
            else if (node.IsExplicit)
                nameTable.AddIdentifier(new OperatorName(TokenID.Explicit, currentContext));
            else
                nameTable.AddIdentifier(new OperatorName(node.Operator, currentContext));

            return node;
        }

        private BaseNode ParseIndexer(IType type, QualifiedIdentifierExpression interfaceType)
        {
            IndexerNode node = new IndexerNode(curtok);
            ClassNode cl = typeStack.Peek();
            cl.Indexers.Add(node);

            ApplyAttributes(node);
            ApplyDocComment(node);


            uint mask = ~(uint)Modifier.IndexerMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("indexer declaration contains illegal modifiers");


            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the indexer is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

            node.Type = type;
            if (interfaceType != null)
            {
                node.InterfaceType = new TypeNode(interfaceType);
            }

            AssertAndAdvance(TokenID.This);
            node.Params = ParseParamList(TokenID.LBracket, TokenID.RBracket);

            // parse accessor part
            AssertAndAdvance(TokenID.LCurly);

            ParsePossibleAttributes(false);
            ParseModifiers();

            if (curtok.ID != TokenID.Ident)
            {
                RecoverFromError("At least one get or set required in accessor", TokenID.Ident);
            }
            bool parsedGet = false;
            if (strings[curtok.Data] == "get")
            {
                node.Getter = ParseAccessor(type);
                parsedGet = true;
            }

            ParsePossibleAttributes(false);
            ParseModifiers();

            if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
            {
                node.Setter = ParseAccessor(type);
            }

            ParsePossibleAttributes(false);
            ParseModifiers();
            // get might follow set
            if (!parsedGet && curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
            {
                node.Getter = ParseAccessor(type);
            }

            if (node.Setter != null
                && node.Getter != null)
            {
                if (node.Getter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Modifiers is permitted only for one of the acessors.");
                }
            }
            else
            {
                if (node.Setter == null && node.Getter.Modifiers != Modifier.Empty
                    || node.Getter == null && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Accessor modifier is authorized only if the 'get' and the 'set' are declared.");
                }

            }

            switch (node.Modifiers)
            {
                case Modifier.Public:
                    break;
                case (Modifier.Protected | Modifier.Internal):
                    if (node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty
                        && (node.Getter.Modifiers != Modifier.Protected
                                || node.Getter.Modifiers != Modifier.Private
                                || node.Getter.Modifiers != Modifier.Internal
                            )
                        )
                    {
                        ReportError("The property is protected internal, so the accessor can be only protected, private or internal.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty
                        && (node.Setter.Modifiers != Modifier.Protected
                                || node.Setter.Modifiers != Modifier.Private
                                || node.Setter.Modifiers != Modifier.Internal
                            )
                        )
                    {
                        ReportError("The property is protected internal, so the accessor can be only protected, private or internal.");
                    }
                    break;
                case Modifier.Protected:
                case Modifier.Internal:
                    if (node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty
                        && node.Getter.Modifiers != Modifier.Private)
                    {
                        ReportError("The property is protected or internal, so the accessor can be only private.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Private)
                    {
                        ReportError("The property is protected or internal, so the accessor can be only private.");
                    }
                    break;
                case Modifier.Private:
                    if (node.Getter != null
                        && node.Getter.Modifiers != Modifier.Empty)
                    {
                        ReportError("The property is private or internal, so the accessor can not have any modifier.");
                    }

                    if (node.Setter != null
                        && node.Setter.Modifiers != Modifier.Empty)
                    {
                        ReportError("The property is private or internal, so the accessor can not have any modifier.");
                    }

                    break;
            }

            AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            #region Save name in the name table
            if (node.Getter != null)
            {
                if (node.Setter != null)
                {
                    if (node.Getter.Modifiers != Modifier.Empty)
                    {
                        nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                    else if (node.Setter.Modifiers != Modifier.Empty)
                    {
                        nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                    else
                    {
                        nameTable.AddIdentifier(new IndexerName(
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                }
                else
                {
                    nameTable.AddIdentifier(new IndexerName(
                        ToVisibilityRestriction(node.Modifiers),
                        NameVisibilityRestriction.Self,
                        ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                        PropertyAccessors.Get,
                        currentContext));
                }
            }
            else
            {
                nameTable.AddIdentifier(new IndexerName(
                    NameVisibilityRestriction.Self,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                    PropertyAccessors.Set,
                    currentContext));
            }
            #endregion

            return node;
        }

        private void ApplyDocComment(BaseNode node)
        {
            if (currentDocComment != string.Empty)
            {
                node.DocComment = currentDocComment;
                currentDocComment = string.Empty;
            }
        }

        private void ApplyAttributes(BaseNode node)
        {
            if (curAttributes.Count > 0)
            {
                node.Attributes = curAttributes;
                curAttributes = new NodeCollection<AttributeNode>();
            }
        }

        private void ApplyTypeParameters(IGeneric node)
        {
            if (curTypeParameters.Count > 0)
            {
                if (node.Generic == null)
                {
                    node.Generic = new GenericNode(curtok);
                    genericList.Add(node);
                }

                node.Generic.TypeParameters.AddRange(curTypeParameters);
                curTypeParameters = new List<TypeParameterNode>();
            }
        }

        private void ApplyTypeParameterConstraints(IGeneric node)
        {
            if (curTypeParameterConstraints.Count > 0)
            {
                if (node.IsGeneric)
                {
                    node.Generic.Constraints.AddRange(curTypeParameterConstraints.Values);
                    curTypeParameterConstraints = new Dictionary<string, Constraint>();
                }
                else
                {
                    ReportError("Type parameter constraint applies only on generic type or generic method");
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowAttributes"> Attributes are authorized in type parameter declaration. 
        /// In closed type and in constraints, attributes are not authorized </param>
        /// <param name="inDeclaration"> in generic type declaration, type parameter are only identifier, and type name are not authorized.
        /// So it is used to reject string like "object", "string", etc ... </param>
        /// <param name="allowEmpty"> In the case of the expression typeof(), empty type parameter are allowed </param>
        private void ParseTypeParameterNode(bool allowAttributes, bool inDeclaration, bool allowEmpty)
        {
            if (curmods != Modifier.Empty)
                ReportError("Type parameter can not contain modifiers");

            ParsePossibleAttributes(false);

            if (!allowAttributes
                && curAttributes.Count > 0)
            {
                RecoverFromError("Attributes not allowed for the type Parameter", TokenID.Ident);
            }

            TypeParameterNode typeParameter = null;

            // inDeclaration is only used to force the type parameter to be an identifier
            // but, due to developer write errors, it might be a TypeNode ... 
            if (inDeclaration)
            {
                //parse ident only
                // parses all type of type parameter to handle wrong user's syntax 
                // i.e. : the user add a type name in a method generic type parameters declaration.
                // -> it avoid crash
                // one another case is :
                //
                // class kv <k,v> {}
                //
                // class m <k,v> : c <k,v>,
                // {
                //         void a <kv <k,v>>.x () {} // a<t>.x ()
                // }
                // 
                ExpressionNode name = ParseIdentifierOrKeyword(true, true, true, false, true);
                bool empty = true;

                if (name != null)
                {
                    if (name is IdentifierExpression)
                    {
                        IdentifierExpression id = name as IdentifierExpression;
                        empty = id.Identifier == string.Empty; // TODO: ?
                        typeParameter = new TypeParameterNode(id);

                        empty = false;
                    }
                    else
                    {
                        if (name is TypeNode)
                        {
                            TypeNode ty = name as TypeNode;
                            empty = ty.Identifier.QualifiedIdentifier == string.Empty;
                            typeParameter = new TypeParameterNode(ty);

                            empty = false;
                        }
                    }
                }

                if (empty)
                {
                    ReportError("type parameter must be simple name");
                    typeParameter = new TypeParameterNode(name.RelatedToken);
                }
            }
            else
            {
                //ParseType ...WARNING : it could be a type parameter

                switch (curtok.ID)
                {
                    case TokenID.Comma:
                    case TokenID.Greater:
                    case TokenID.RParen:
                        // we are in the case of empty parameter
                        typeParameter = new TypeParameterNode(curtok);
                        break;
                    default:
                        TypeNode tn = ParseType(false);
                        CheckRankSpecifier(tn); // T<string[]> is allowed, so check ranks
                        if (tn is TypePointerNode)
                        {
                            ReportError("pointer type are not authorized in generic's type parameters.");
                        }
                        // if t is TypePointerNode, it creates an empty type parameter
                        typeParameter = new TypeParameterNode(tn);
                        break;
                }

            }

            ApplyAttributes(typeParameter);

            if (!allowEmpty
                && typeParameter.IsEmpty)
            {
                ReportError("Empty type parameter not allowed");
            }

            if (!typeParameter.IsEmpty
                    && curTypeParameters.Contains(typeParameter))
            {
                ReportError("duplicated type parameter name");
            }
            else
            {
                curTypeParameters.Add(typeParameter);
            }
        }

        /*        private void ParseGenericTypeNodeExpression()
                {
                    if (ParsePossibleTypeParameterNode(false, false, true))
                    {
                        IdentifierExpression leftSide = (IdentifierExpression)exprStack.Pop();
                        TypeNode node = new TypeNode(leftSide );

                        ApplyTypeParameters(node);

                        ParsePossibleTypeParameterConstraintNode(node); // over lparen

                        if (curTypeParameterConstraints.Count > 0)
                        {
                            ReportError("Constraints are not allowed in generic closed type declaration.");
                        }

                        //set constraint only to preserve file source 
                        // when ToSource is called
                        ApplyTypeParameterConstraints(node);

                        exprStack.Push(node);
                    }
                }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowAttributes"> Attributes are authorized in type parameter declaration. 
        /// In closed type and in constraints, attributes are not authorized </param>
        /// <param name="inDeclaration"> in generic type declaration, type parameter are only identifier, and type name are not authorized.
        /// So it is used to reject string like "object", "string", etc ... </param>
        /// <param name="allowEmpty"> In the case of the expression typeof(), empty type parameter are allowed </param>
        /// <returns> <c>true</c> if it parsed a type parameter</returns>
        private bool ParsePossibleTypeParameterNode(bool allowAttributes, bool inDeclaration, bool allowEmpty, bool alwaysGeneric)
        {
            // if the current token is "<", the source code is starting 
            // a type parameter list
            if (curtok.ID == TokenID.Less && (alwaysGeneric || curtok.GenericStart))
            {
                List<TypeParameterNode> backupTypeParameters;
                // use of AssertAndAdvance to valid the previous state restore
                AssertAndAdvance(TokenID.Less); // advance over Less token

                //countOpenedGeneric++;

                //parse it at least one time out of the next while loop
                // to be sure that at least one type parameter is defined 
                //( for example -> "< >" is invalid ( except in typeof expression )

                //Hack: in a generic type we have to back up attributes otherwise it will applied to the type parameter and not to current field or property
                NodeCollection<AttributeNode> backupAttrs = this.curAttributes;
                this.curAttributes = new NodeCollection<AttributeNode>();

                ParseTypeParameterNode(allowAttributes, inDeclaration, allowEmpty);

                // back up the previous curTypeParameters or following type parameter are parsed as type node
                // i.e. : 
                //
                // class TOTO<X,Y> is parsed like class TOTO<Y<X>>
                backupTypeParameters = curTypeParameters;

                while (curtok.ID != TokenID.Greater
                    //&& curtok.ID != TokenID.ShiftRight//we handle the case X<Y<object>> which ends with >>
                        && curtok.ID != TokenID.Eof)
                {
                    curTypeParameters = new List<TypeParameterNode>();

                    //we need a comma
                    AssertAndAdvance(TokenID.Comma);

                    ParseTypeParameterNode(allowAttributes, inDeclaration, allowEmpty);

                    //test if type parameter already exists ? 
                    backupTypeParameters.AddRange(curTypeParameters);
                }

                //restor curtype parameter
                curTypeParameters = backupTypeParameters;

                //check if the end of file is reached
                AssertAndAdvance(TokenID.Greater); // over ">"

                //restore the previous backed up attributes 
                this.curAttributes = backupAttrs;

                return true;
            }

            return false;
        }

        private void ParseConstraintNode(Constraint node)
        {
            switch (curtok.ID)
            {
                //type constraint
                case TokenID.Object:
                case TokenID.String:
                case TokenID.Ident:
                    TypeNode tn = ParseType(false);
                    CheckRankSpecifier(tn);
                    node.ConstraintExpressions.Add(new ConstraintExpressionNode(tn)); // allow type[,,]
                    break;
                case TokenID.Class:
                case TokenID.Struct:
                    node.ConstraintExpressions.Add(new ConstraintExpressionNode(curtok));
                    Advance();
                    break;

                //constructor constrains?
                case TokenID.New:
                    if (node.ConstructorConstraint != null)
                    {
                        ReportError("Only one constructor constraint allowed by constraint");
                    }

                    node.ConstructorConstraint = new ConstructorConstraint();

                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    AssertAndAdvance(TokenID.RParen);
                    break;

                default:
                    ReportError("Invalid constraint definition. " + TokenID.Ident.ToString().ToLower() + " expected");
                    break;
            }
        }

        private void ParseTypeParameterConstraintNode(IGeneric genericType)
        {
            if (curmods != Modifier.Empty)
                ReportError("Type parameter constraint can not contain modifiers");

            ParseTypeParameterNode(false, true, false);

            Constraint node = new Constraint(curTypeParameters[0]);

            curTypeParameters = new List<TypeParameterNode>();

            AssertAndAdvance(TokenID.Colon);

            //parse the first constraint
            ParseConstraintNode(node);

            while (curtok.ID == TokenID.Comma && curtok.ID != TokenID.Eof)
            {
                Advance();
                ParseConstraintNode(node);
            }

            if (!node.TypeParameter.IsEmpty)
            {
                if (!genericType.Generic.TypeParameters.Contains(node.TypeParameter))
                {
                    ReportError("unknow type parameter : " + node.TypeParameter.UniqueIdentifier);
                }
            }

            string key = node.TypeParameter.Identifier.Identifier;

            if (curTypeParameterConstraints.ContainsKey(key))
            {
                ReportError("duplicated constraint for the type parameter '" + key + "'.");
            }

            curTypeParameterConstraints.Add(key, node);
        }

        private void ParsePossibleTypeParameterConstraintNode(IGeneric node)
        {
            // if the current token is "where", the source code is starting 
            // a type parameter constraint
            while (curtok.ID == TokenID.Ident && strings[curtok.Data] == "where")
            {
                Advance(); // advance over Where token
                ParseTypeParameterConstraintNode(node);
            }
        }

        private bool IsIteratorClass(IType type)
        {
            TypeNode typeNode = type as TypeNode;
            if (typeNode == null) return false;
            String name = typeNode.GenericIndependentIdentifier;
            while (true)
            {
                String[] parts = name.Split(QualifiedIdentifierDelimiters, 2);      // returns at least one element
                PrimaryExpression aliasTarget = currentContext.GetAliasTarget(parts[0]);
                if (aliasTarget != null)
                {
                    TypeNode tgtType = aliasTarget as TypeNode;
                    if (tgtType != null)
                        name = tgtType.GenericIndependentIdentifier;
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        aliasTarget.ToSource(sb);
                        name = sb.ToString();
                    }
                    if (parts.Length > 1) name += '.' + parts[1];
                }
                else break;
            }
            return iteratorsClass.Contains(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the method</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
        private BaseNode ParseMethod(IType type, QualifiedIdentifierExpression name)
        {
            uint mask = ~(uint)Modifier.MethodMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("method declaration contains illegal modifiers");

            MethodNode node;

            //we check the return type of the method.
            // -> it dertimes if the method is an iterator.

            if (IsIteratorClass(type))
            {
                node = new MethodNode(true, curtok);
                curIterator = node;
            }
            else
            {
                node = new MethodNode(false, curtok);
            }

            curMethod = node;

            ClassNode cl = typeStack.Peek();
            cl.Methods.Add(node);

            ApplyAttributes(node);
            ApplyDocComment(node);


            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the method is declared in an unsafe context ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

            node.Type = type;
            QualifiedIdentifierExpression methName = new QualifiedIdentifierExpression(name.RelatedToken);
            methName.Expressions.Add(name);
            node.Names.Add(methName);

            if (methName.IsGeneric)
            {
                //move generic from identifier to method
                node.Generic = methName.Generic;
                methName.Generic = null;
            }

            ParsePossibleTypeParameterNode(true, true, false, true);
            //if generic applies type parameter collection to the node
            ApplyTypeParameters(node);

            // starts at LParen
            node.Params = ParseParamList();

            ParsePossibleTypeParameterConstraintNode(node);
            ApplyTypeParameterConstraints(node);

            ParseBlock(node.StatementBlock);

            //we parse all parameter, if only one is ref or out, we raise an exception
            if (node.IsIterator)
            {
                foreach (ParamDeclNode param in node.Params)
                {
                    if ((param.Modifiers & (Modifier.Ref | Modifier.Out)) != Modifier.Empty)
                    {
                        ReportError("Iterators can not have nor 'ref' nor 'out' parameter.");
                        break;
                    }
                }
            }

            curMethod = null;
            curIterator = null;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            nameTable.AddIdentifier(new MethodName(name.QualifiedIdentifier,
                ToVisibilityRestriction(node.Modifiers),
                ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                currentContext));

            return node;
        }

        private void ParseFixedBuffer(StructNode st, FixedBufferNode node)
        {
            if (st == null)
            {
                ReportError("fixed buffer authorized only in structure declaration.");
            }

            if (!node.IsUnsafe)
            {
                ReportError("fixed buffer authorized only in unsafe context.");
            }

            if ((node.Modifiers & Modifier.Static) != Modifier.Empty)
            {
                ReportError("fixed buffer can not be declared as static.");
            }

            if (node.Type is TypePointerNode)
            {
                ReportError("fixed buffer can not be pointer.");
            }
            else
            {
                StringCollection strColl = new StringCollection();
                string type_str = ((TypeNode)node.Type).Identifier.QualifiedIdentifier.ToLower();

                strColl.AddRange(new String[] { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "bool" });

                if (!strColl.Contains(type_str))
                {
                    ReportError("fixed buffer element type must be of type sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double or bool.");
                }
            }
            AssertAndAdvance(TokenID.LBracket);

            ConstantExpression expr = new ConstantExpression(curtok);
            expr.Value = ParseExpression(TokenID.RBracket);
            node.FixedBufferConstants.Add(expr);

            AssertAndAdvance(TokenID.RBracket);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the field</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
        private BaseNode ParseFixedBuffer(IType type, QualifiedIdentifierExpression name)
        {
            uint mask = ~(uint)Modifier.FxedBufferdMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("field declaration contains illegal modifiers");

            FixedBufferNode node = new FixedBufferNode(curtok);
            StructNode st = (StructNode)typeStack.Peek();
            st.FixedBuffers.Add(node);

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(st, node.Modifiers, true);

            ApplyAttributes(node);
            ApplyDocComment(node);


            node.Type = type;
            QualifiedIdentifierExpression fieldName = new QualifiedIdentifierExpression(name.RelatedToken);
            fieldName.Expressions.Add(name);
            node.Names.Add(fieldName);

            //fixed buffer
            // if the current type is not a structure, it is invalid
            ParseFixedBuffer(st, node);

            while (curtok.ID == TokenID.Comma)
            {
                Advance(); // over comma
                QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(false, false, true);
                node.Names.Add(ident);

                ParseFixedBuffer(st, node);
            }

            if (curtok.ID == TokenID.Semi)
            {
                Advance();
            }

            return node;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the field</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
        private BaseNode ParseField(IType type, QualifiedIdentifierExpression name)
        {
            uint mask = ~(uint)Modifier.FieldMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("field declaration contains illegal modifiers");

            FieldNode node = new FieldNode(curtok);
            ClassNode cl = typeStack.Peek();
            cl.Fields.Add(node);

            Modifier fieldMods = curmods;
            curmods = Modifier.Empty;

            node.Modifiers = fieldMods;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true);

            ApplyAttributes(node);
            ApplyDocComment(node);

            node.Type = type;
            QualifiedIdentifierExpression fieldName = new QualifiedIdentifierExpression(name.RelatedToken);
            fieldName.Expressions.Add(name);
            node.Names.Add(fieldName);

            while (true)
            {
                nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                    currentContext));

                //eg: int ok = 0, error, xx = 0;
                if (curtok.ID == TokenID.Equal)
                {
                    Advance();

                    if (curtok.ID == TokenID.LCurly)
                        node.Value = ParseArrayInitializer();
                    else
                        node.Value = ParseExpression();

                    if (curtok.ID == TokenID.Comma)
                    {
                        node = new FieldNode(curtok);
                        typeStack.Peek().Fields.Add(node);
                        node.Modifiers = fieldMods;
                        node.Type = type;
                    }
                }
                if (curtok.ID != TokenID.Comma) break;

                Advance(); // over comma
                QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(false, false, true);
                node.Names.Add(ident);
            }

            AssertAndAdvance(TokenID.Semi);

            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> type of the property</param>
        /// <param name="name"> name in qualified form ( if it is an explicit interface declaration) </param>
        private BaseNode ParseProperty(IType type, QualifiedIdentifierExpression name)
        {
            uint mask = ~(uint)Modifier.PropertyMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("field declaration contains illegal modifiers");

            PropertyNode node = new PropertyNode(curtok);
            ClassNode cl = typeStack.Peek();
            cl.Properties.Add(node);

            ApplyAttributes(node);
            ApplyDocComment(node);

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the property is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

            node.Type = type;
            QualifiedIdentifierExpression propName = new QualifiedIdentifierExpression(name.RelatedToken);
            propName.Expressions.Add(name);
            node.Names.Add(propName);

            // opens on lcurly
            AssertAndAdvance(TokenID.LCurly);

            // todo: AddNode attributes to get and setters
            ParsePossibleAttributes(false);

            ParseModifiers();

            if (curtok.ID != TokenID.Ident)
            {
                RecoverFromError("At least one get or set required in accessor", TokenID.Ident);
            }

            bool parsedGet = false;
            if (strings[curtok.Data] == "get")
            {
                node.Getter = ParseAccessor(type);
                parsedGet = true;
            }

            // todo: AddNode attributes to get and setters
            ParsePossibleAttributes(false);

            ParseModifiers();

            if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
            {
                node.Setter = ParseAccessor(type);
            }

            if (!parsedGet)
            {
                // todo: AddNode attributes to get and setters
                ParsePossibleAttributes(false);

                ParseModifiers();

                // get might follow set
                if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
                {
                    node.Getter = ParseAccessor(type);
                }
            }

            if ((node.Modifiers & Modifier.Override) == 0)
            {
                if (node.Setter != null
                    && node.Getter != null)
                {
                    if (node.Getter.Modifiers != Modifier.Empty
                        && node.Setter.Modifiers != Modifier.Empty)
                    {
                        ReportError("Modifier is permitted only for one of the accessors.");
                    }
                }
                else if (node.Getter != null && node.Getter.Modifiers != Modifier.Empty
                        || node.Setter != null && node.Setter.Modifiers != Modifier.Empty)
                {
                    ReportError("Accessor modifier is authorized only if the 'get' and the 'set' are declared.");
                }
            }

            AssertAndAdvance(TokenID.RCurly);

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            #region Save name in the name table
            if (node.Getter != null)
            {
                if (node.Setter != null)
                {
                    if (node.Getter.Modifiers != Modifier.Empty)
                    {
                        nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                    else if (node.Setter.Modifiers != Modifier.Empty)
                    {
                        nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Setter.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                    else
                    {
                        nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                            ToVisibilityRestriction(node.Modifiers),
                            ToVisibilityRestriction(node.Modifiers),
                            ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                            PropertyAccessors.Both,
                            currentContext));
                    }
                }
                else
                {
                    nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                        ToVisibilityRestriction(node.Modifiers),
                        NameVisibilityRestriction.Self,
                        ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                        PropertyAccessors.Get,
                        currentContext));
                }
            }
            else
            {
                nameTable.AddIdentifier(new PropertyName(name.QualifiedIdentifier,
                    NameVisibilityRestriction.Self,
                    ToVisibilityRestriction(node.Modifiers),
                    ((node.Modifiers & Modifier.Static) != Modifier.Static ? Scope.Instance : Scope.Static),
                    PropertyAccessors.Set,
                    currentContext));
            }
            #endregion

            return node;
        }

        private BaseNode ParseEvent()
        {
            uint mask = ~(uint)Modifier.EventMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("Event contains illegal modifiers");

            EventNode node = new EventNode(curtok);

            ClassNode cl = typeStack.Peek();

            cl.Events.Add(node);

            ApplyAttributes(node);
            ApplyDocComment(node);


            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe++;
                node.IsUnsafeDeclared = true;
            }

            //the event is declared in an unsafe type ?
            node.IsUnsafe = isUnsafe > 0;

            CheckStaticClass(cl, node.Modifiers, true); ;

            Advance(); // advance over event keyword

            node.Type = ParseType(false);

            if (curtok.ID != TokenID.Ident)
                ReportError("Expected event member name.");

            while (curtok.ID == TokenID.Ident)
            {
                node.Names.Add(ParseQualifiedIdentifier(true, false, true));
            }

            //TODO: Omer - Add all of the events in that line to the name table once Robin fixes the code.

            switch (curtok.ID)
            {
                case TokenID.Semi:
                    AssertAndAdvance(TokenID.Semi);
                    break;
                case TokenID.Equal:
                    Advance();
                    node.Value = ParseExpression();
                    AssertAndAdvance(TokenID.Semi);
                    break;
                case TokenID.LCurly:
                    Advance(); // over lcurly

                    ParsePossibleAttributes(false);

                    if (curtok.ID != TokenID.Ident)
                    {
                        ReportError("Event accessor requires add or remove clause.");
                    }

                    string curAccessor = strings[curtok.Data];
                    Advance(); // over ident
                    if (curAccessor == "add")
                    {
                        node.AddBlock = new AccessorNode(false, curtok);
                        node.AddBlock.Kind = "add";
                        node.AddBlock.IsUnsafe = isUnsafe > 0;
                        ApplyAttributes(node.AddBlock);
                        ParseBlock(node.AddBlock.StatementBlock);

                        ParsePossibleAttributes(false);
                        if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "remove")
                        {
                            node.RemoveBlock = new AccessorNode(false, curtok);
                            node.RemoveBlock.IsUnsafe = isUnsafe > 0;
                            node.RemoveBlock.Kind = "remove";
                            ApplyAttributes(node.RemoveBlock);
                            Advance(); // over ident
                            ParseBlock(node.RemoveBlock.StatementBlock);
                        }
                        else
                        {
                            ReportError("Event accessor expected remove clause.");
                        }
                    }
                    else if (curAccessor == "remove")
                    {
                        node.RemoveBlock = new AccessorNode(false, curtok);
                        node.RemoveBlock.IsUnsafe = isUnsafe > 0;
                        node.RemoveBlock.Kind = "remove";
                        ApplyAttributes(node.RemoveBlock);
                        ParseBlock(node.RemoveBlock.StatementBlock);

                        ParsePossibleAttributes(false);
                        if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "add")
                        {
                            node.AddBlock = new AccessorNode(false, curtok);
                            node.AddBlock.IsUnsafe = isUnsafe > 0;
                            node.AddBlock.Kind = "remove";
                            Advance(); // over ident
                            ApplyAttributes(node.AddBlock);
                            ParseBlock(node.AddBlock.StatementBlock);
                        }
                        else
                        {
                            ReportError("Event accessor expected add clause.");
                        }
                    }
                    else
                    {
                        ReportError("Event accessor requires add or remove clause.");
                    }

                    AssertAndAdvance(TokenID.RCurly);

                    break;
            }

            if ((node.Modifiers & Modifier.Unsafe) != Modifier.Empty)
            {
                //unsafe modifier -> unsafe type.
                isUnsafe--;
            }

            return node;
        }
        private BaseNode ParseConst()
        {
            uint mask = ~(uint)Modifier.ConstantMods;
            if (((uint)curmods & mask) != (uint)Modifier.Empty)
                ReportError("const declaration contains illegal modifiers");

            ConstantNode node = new ConstantNode(curtok);

            ClassNode cl = typeStack.Count == 0 ? null : typeStack.Peek();
            cl.Constants.Add(node); // TODO: If cl is null, then this will throw an exception... Better throw a specialized one.

            ApplyAttributes(node);
            ApplyDocComment(node);

            node.Modifiers = curmods;
            curmods = Modifier.Empty;

            if (cl != null)
            {
                CheckStaticClass(cl, node.Modifiers, false);
            }

            Advance(); // advance over const keyword

            TypeNode tn = ParseType(false);
            CheckRankSpecifier(tn);
            node.Type = tn;

            bool hasEqual = false;
            QualifiedIdentifierExpression name = ParseQualifiedIdentifier(false, false, true);
            node.Names.Add(name);

            if (curtok.ID == TokenID.Equal)			// TODO: THIS IS SHIT, SIR!!!!
            {
                Advance();
                hasEqual = true;
            }

            nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                ToVisibilityRestriction(node.Modifiers),
                Scope.Static,
                currentContext));

            while (curtok.ID == TokenID.Comma)
            {
                Advance();

                name = ParseQualifiedIdentifier(false, false, true);
                node.Names.Add(name);

                if (curtok.ID == TokenID.Equal)
                {
                    Advance();
                    hasEqual = true;
                }
                else
                {
                    hasEqual = false;
                }

                // TODO: Robin - make sure when you fix the constants that this is still relevant.
                nameTable.AddIdentifier(new FieldName(name.QualifiedIdentifier,
                    ToVisibilityRestriction(node.Modifiers),
                    Scope.Static,
                    currentContext));
            }

            if (hasEqual)
            {
                node.Value = ParseExpression();
            }

            AssertAndAdvance(TokenID.Semi);

            return node;
        }

        private EnumNode ParseEnumMember()
        {
            EnumNode result = new EnumNode(curtok);

            ParsePossibleAttributes(false);

            ApplyDocComment(result);
            ApplyAttributes(result);

            if (curtok.ID != TokenID.Ident)
            {
                ReportError("Enum members must be legal identifiers.");
            }

            result.Name = new IdentifierExpression(strings[curtok.Data], curtok);
            Advance();

            if (curtok.ID == TokenID.Equal)
            {
                Advance();
                result.Value = ParseExpression();
            }

            nameTable.AddIdentifier(new FieldName(result.Name.Identifier,
                NameVisibilityRestriction.Everyone,
                Scope.Static,
                currentContext));

            return result;
        }

        // member helpers
        private NodeCollection<ParamDeclNode> ParseParamList()
        {
            // default is parens, however things like indexers use square brackets
            return ParseParamList(TokenID.LParen, TokenID.RParen);
        }

        private NodeCollection<ParamDeclNode> ParseParamList(TokenID openToken, TokenID closeToken)
        {
            AssertAndAdvance(openToken);
            if (curtok.ID == closeToken)
            {
                Advance();
                return null;
            }
            NodeCollection<ParamDeclNode> result = new NodeCollection<ParamDeclNode>();
            bool isParams;
            bool hasComma;
            do
            {
                ParamDeclNode node = new ParamDeclNode(curtok);
                result.Add(node);
                isParams = false;

                ParsePossibleAttributes(false);
                ApplyAttributes(node);

                if (isAnonymous > 0
                    && curAttributes.Count > 0)
                {
                    ReportError("Attributes are not allowed for anonymous delegate's parameters.");
                }

                if (curtok.ID == TokenID.Ref)
                {
                    node.Modifiers |= Modifier.Ref;
                    Advance();
                }
                else if (curtok.ID == TokenID.Out)
                {
                    node.Modifiers |= Modifier.Out;
                    Advance();
                }
                else if (curtok.ID == TokenID.Params)
                {
                    if (isAnonymous > 0)
                    {
                        ReportError("Params parameter are not allowed for anonymous delegate.");
                    }

                    isParams = true;
                    node.Modifiers |= Modifier.Params;
                    Advance();
                }

                TypeNode tn = ParseType(false);
                CheckRankSpecifier(tn);
                node.Type = tn;

                if (isParams)
                {
                    // ensure is array type
                }

                if (curtok.ID == TokenID.Ident)	// TODO: This should not be optional, should it?!?
                {
                    node.Name = ((IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false)).Identifier;//strings[curtok.Data];
                }

                hasComma = false;
                if (curtok.ID == TokenID.Comma)
                {
                    Advance();
                    hasComma = true;
                }
            }
            while (!isParams && hasComma);

            AssertAndAdvance(closeToken);

            return result;
        }
        private ParamDeclNode ParseParamDecl()
        {

            ParamDeclNode node = new ParamDeclNode(curtok);

            ParsePossibleAttributes(false);

            if (curAttributes.Count > 0)
            {
                node.Attributes = curAttributes;
                curAttributes = new NodeCollection<AttributeNode>();
            }

            /*			ExpressionNode type = ParseExpressionOrType(true);
                        if(!(type is TypeNode))
                        {
                            ReportError("Type expected", ((BaseNode) node.Type).RelatedToken);
                            return node;
                        }
                        node.Type = (IType) type;*/

            TypeNode tn = ParseType(false);
            CheckRankSpecifier(tn);
            node.Type = tn;

            if (curtok.ID == TokenID.Ident)
            {
                node.Name = strings[curtok.Data];
                Advance();
            }
            else
            {
                RecoverFromError("Expected arg name.", TokenID.Ident);
            }
            return node;
        }
        private NodeCollection<ArgumentNode> ParseArgs()
        {
            AssertAndAdvance(TokenID.LParen);
            if (curtok.ID == TokenID.RParen)
            {
                Advance();
                return null;
            }
            bool hasComma;
            NodeCollection<ArgumentNode> result = new NodeCollection<ArgumentNode>();
            do
            {
                ArgumentNode node = new ArgumentNode(curtok);
                result.Add(node);

                if (curtok.ID == TokenID.Ref)
                {
                    node.IsRef = true;
                    Advance();
                }
                else if (curtok.ID == TokenID.Out)
                {
                    node.IsOut = true;
                    Advance();
                }
                node.Expression = ParseExpression();

                hasComma = false;
                if (curtok.ID == TokenID.Comma)
                {
                    Advance();
                    hasComma = true;
                }
            }
            while (hasComma);

            AssertAndAdvance(TokenID.RParen);

            return result;
        }

        private AccessorNode ParseAccessor(IType type)
        {
            AccessorNode result;

            if (IsIteratorClass(type))
            {
                result = new AccessorNode(true, curtok);
                curIterator = result;
            }
            else
            {
                result = new AccessorNode(false, curtok);
            }

            result.Modifiers = curmods;
            curmods = Modifier.Empty;

            ApplyAttributes(result);
            ApplyDocComment(result);

            string kind = "";
            if (curtok.ID == TokenID.Ident)
            {
                kind = strings[curtok.Data];
            }
            else
            {
                RecoverFromError("Must specify accessor kind in accessor.", TokenID.Ident);
            }

            if (result.Modifiers != Modifier.Empty)
            {
                if (curInterface != null)
                {
                    ReportError("Property accessor modifiers are not authorized in interface declaration");
                }

                Modifier id = (result.Modifiers & (Modifier.Private | Modifier.Protected | Modifier.Internal));

                switch (id)
                {
                    case Modifier.Private:
                    case Modifier.Protected:
                    case Modifier.Internal:
                    case (Modifier.Internal | Modifier.Protected):
                        break;
                    default:
                        ReportError("Invalid modifiers set for the accessor " + kind + ".");
                        break;
                }
            }


            result.Kind = kind;
            Advance();
            if (curtok.ID == TokenID.Semi)
            {
                result.IsAbstractOrInterface = true;
                Advance(); // over semi
            }
            else
            {
                ParseBlock(result.StatementBlock);
            }

            curIterator = null;
            return result;
        }
        private ConstantExpression ParseConstExpr()
        {
            ConstantExpression node = new ConstantExpression(curtok);
            node.Value = ParseExpression();

            return node;
        }
        private void ParseModifiers()
        {
            while (!curtok.Equals(EOF))
            {
                switch (curtok.ID)
                {
                    case TokenID.New:
                    case TokenID.Public:
                    case TokenID.Protected:
                    case TokenID.Internal:
                    case TokenID.Private:
                    case TokenID.Abstract:
                    case TokenID.Sealed:
                    case TokenID.Static:
                    case TokenID.Virtual:
                    case TokenID.Override:
                    case TokenID.Extern:
                    case TokenID.Readonly:
                    case TokenID.Volatile:
                    case TokenID.Unsafe:
                    case TokenID.Fixed:
                    case TokenID.Ref:
                    case TokenID.Out:

                        //case TokenID.Assembly:
                        //case TokenID.Field:
                        //case TokenID.Event:
                        //case TokenID.Method:
                        //case TokenID.Param:
                        //case TokenID.Property:
                        //case TokenID.Return:
                        //case TokenID.Type:

                        uint mod = (uint)modMap[(int)curtok.ID];
                        if (((uint)curmods & mod) > 0)
                        {
                            ReportError("Duplicate modifier.");
                        }
                        curmods |= (Modifier)mod;
                        Advance();
                        break;

                    case TokenID.Ident:
                        if (strings[curtok.Data] == "partial")
                        {
                            if ((curmods & Modifier.Partial) != 0)
                            {
                                ReportError("Duplicate partial.");
                            }
                            curmods |= Modifier.Partial;
                            Advance();
                            break;
                        }
                        goto default;

                    default:
                        return;
                }
            }
        }
        private Modifier ParseAttributeModifiers()
        {
            Modifier result = Modifier.Empty;
            bool isMod = true;

            while (isMod)
            {
                switch (curtok.ID)
                {
                    case TokenID.Ident:
                        string curIdent;
                        curIdent = strings[curtok.Data];
                        switch (curIdent)
                        {
                            case "field":
                                result |= Modifier.Field;
                                Advance();
                                break;
                            case "method":
                                result |= Modifier.Method;
                                Advance();
                                break;
                            case "param":
                                result |= Modifier.Param;
                                Advance();
                                break;
                            case "property":
                                result |= Modifier.Property;
                                Advance();
                                break;
                            case "type":
                                result |= Modifier.Type;
                                Advance();
                                break;
                            case "module":
                                result |= Modifier.Module;
                                Advance();
                                break;
                            case "assembly":
                                result |= Modifier.Assembly;
                                Advance();
                                break;
                            default:
                                isMod = false;
                                break;
                        }
                        break;

                    case TokenID.Return:
                        result |= Modifier.Return;
                        Advance();
                        break;

                    case TokenID.Event:
                        result |= Modifier.Event;
                        Advance();
                        break;

                    default:
                        isMod = false;
                        break;

                }
            }
            return result;
        }

        /*		private TypeNode ParseTypeDeclarator(TypeNode type)
                {
                    if(curtok.ID == TokenID.Question && curtok.NullableDeclaration)
                    {
                        Advance();
                        type.IsNullableType = true;
                    }
                    while(curtok.ID == TokenID.Star)
                    {
                        Advance();
                        type = new TypePointerNode(type);
                    }
                    // TODO: Parse rank specifiers
                    return type;
                }*/

        // note: this does NOT check for rank specifiers: MyType[,,]
        // to also do this use CheckRankSpecifier(TypeNode);
        private TypeNode ParseType(bool considerTrinary)
        {
            //backup modifiers because ParseQualifiedIdentifier will consume ParseTypeParameter
            // and ParseTypeParameter consume modifiers ... 
            Modifier backupModifiers = curmods;
            curmods = Modifier.Empty;
            TypeNode result = null;

            QualifiedIdentifierExpression ident = ParseQualifiedIdentifier(true, false, false);
           
            // if the ParseQualifiedIdentifier can resolve the identifier to a type, 
            // the parser does not need to wrap the type in another type node ... 
            if (ident.Expressions.Last is PredefinedTypeNode)
            {
                result = (TypeNode)ident.Expressions.Last;
            }
            else
            {
                result = new TypeNode(curtok);
                result.Identifier = ident;

                // Edit Robin: I don't think ParseQualifiedIdentifier will ever have rank specifiers..
                //// move the rank specifier
                //if (result.Identifier.IsType)
                //{
                //    result.RankSpecifiers = ((IType)result.Identifier.Expressions.Last).RankSpecifiers;
                //    ((IType)result.Identifier.Expressions.Last).RankSpecifiers = new List<int>();
                //}
            }

            if (curtok.ID == TokenID.Question && (!considerTrinary || curtok.NullableDeclaration))
            {
                Advance();
                result.IsNullableType = true;
            }
            while (curtok.ID == TokenID.Star)
            {
                Advance();
                result = new TypePointerNode(result);
            }

            curmods = backupModifiers;

            return result;
        }

        /// <summary>
        /// it parse expressions like
        /// 
        /// A.B.type1<int>.type2
        /// 
        /// </summary>
        /// <returns></returns>
        private QualifiedIdentifierExpression ParseQualifiedIdentifier(bool consumeTypeParameter, bool allowTypeParameterAttributes, bool inDeclaration)
        {
            QualifiedIdentifierExpression result = new QualifiedIdentifierExpression(curtok);

            result.Expressions.Add(ParseIdentifierOrKeyword(true, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration, true));
            while (curtok.ID == TokenID.Dot || curtok.ID == TokenID.ColonColon)
            {
                if (curtok.ID == TokenID.ColonColon)
                {
                    // 'global' is not a kw so it is treated as an identifier
                    if (result.IsNamespaceAliasQualifier)
                    {
                        ReportError("Qualified name can not have more than one qualificator alias name.");
                    }
                    result.IsNamespaceAliasQualifier = true;
                }

                Advance();
                if (curtok.ID == TokenID.This)
                {
                    // this is an indexer with a prepended interface, do nothing (but consume dot)		// WTF0001
                }
                else
                {
                    result.Expressions.Add(ParseIdentifierOrKeyword(true, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration, true));
                }
            }

            return result;
        }

        private void CheckRankSpecifier(ExpressionNode node)
        {
            // now any 'rank only' specifiers (without size decls)
            while (curtok.ID == TokenID.LBracket)
            {
                Token nextToken = PeekNextNonWhitespace();
                if (nextToken.ID != TokenID.RBracket &&
                    nextToken.ID != TokenID.Comma)
                {
                    // anything with size or accessor decls has own node type
                    break;
                }

                if (!(node is IType))
                {
                    node = new TypeNode(node);			// TODO: This will NOT change the given node to a TypeNode!!!
                }

                Advance(); // over lbracket
                int commaCount = 0;
                while (curtok.ID == TokenID.Comma)
                {
                    commaCount++;
                    Advance();
                }
                ((IType)node).RankSpecifiers.Add(commaCount);
                AssertAndAdvance(TokenID.RBracket);
            }
        }

        private ExpressionNode CheckIdentifierIsType(ExpressionNode expr, bool consumeTypeParameter,
            bool allowTypeParameterAttributes, bool inDeclaration, bool alwaysGeneric)
        {
            ExpressionNode result = expr;

            if (consumeTypeParameter)
            {
                //check if it is a generic type
                ParsePossibleTypeParameterNode(allowTypeParameterAttributes, inDeclaration, true, alwaysGeneric);
                if (curTypeParameters != null
                        && curTypeParameters.Count != 0
                    //&& curtok.ID != TokenID.LParen
                    )
                {
                    TypeNode type = result as TypeNode;

                    if (type == null)
                    {
                        type = new TypeNode(result.RelatedToken);
                        type.Identifier.Expressions.Add(result);
                        result = type;
                    }

                    ApplyTypeParameters(type);
                }
            }

            switch (curtok.ID)
            {
                case TokenID.LBracket:
                    if (!isNewStatement)
                    {
                        CheckRankSpecifier(result);
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// this method parse the next token.
        /// </summary>
        /// <param name="checkIsType">
        /// if set to <c>true</c>, it will check that the identifier is a type. If it is a type, it converts it
        /// to a TypeNode.
        /// if set to <c>false</c>, it will not check that the identifier is a type.
        /// 
        /// Set it to <c>false</c> in the case of a type declaration while your are parsing the type's name
        /// 
        /// </param>
        /// <returns>
        /// Generally it returns identifier expression
        /// But in some cases ,it is possible to resolve the kind the expression, 
        /// so it might returns a TypeNode object.
        /// 
        /// For exemple in this case : System.Collections.Generic.List&lt;int&gt;
        /// 
        /// The first identifiers are really identifier.
        /// And the last identifier is a type, and now that because of the generic declaration
        /// 
        /// in the next case : 
        /// 
        /// System.Collections.ArrayList
        /// 
        /// All are returned as Identifier, because in the first stage of the parser, 
        /// we are not able to pronostic what is it the kind of the expression. The ArrayList type will
        /// be resolved in another parser stage
        /// </returns>
        private ExpressionNode ParseIdentifierOrKeyword(bool checkIsType, bool consumeTypeParameter,
            bool allowTypeParameterAttributes, bool inDeclaration, bool alwaysGeneric)
        {
            ExpressionNode result = null;
            switch (curtok.ID)
            {
                case TokenID.Ident:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        result = new IdentifierExpression(curtok.ID.ToString().ToLower(), curtok);
                    }
                    else
                    {
                        result = new IdentifierExpression(strings[curtok.Data], curtok);
                    }
                    Advance();

                    while (curtok.ID == TokenID.BSlash)
                    {
                        Advance();
                        string c = char.ConvertFromUtf32(int.Parse(strings[curtok.Data].TrimStart('\\').TrimStart('u'), NumberStyles.HexNumber));
                        ((IdentifierExpression)result).Identifier += c;
                        Advance();
                    }

                    break;


                case TokenID.Bool:
                case TokenID.Byte:
                case TokenID.Char:
                case TokenID.Decimal:
                case TokenID.Double:
                case TokenID.Float:
                case TokenID.Int:
                case TokenID.Long:
                case TokenID.Object:
                case TokenID.SByte:
                case TokenID.Short:
                case TokenID.String:
                case TokenID.UInt:
                case TokenID.ULong:
                case TokenID.UShort:
                case TokenID.Void:
                    result = new PredefinedTypeNode(curtok.ID, curtok);
                    Advance();
                    break;
                //                case TokenID.If:		// TODO: ???
                //                case TokenID.Else:		// TODO: ???
                case TokenID.This:
                case TokenID.Base:
                    string predef = Enum.GetName(TokenID.Invalid.GetType(), curtok.ID).ToLower();
                    result = new IdentifierExpression(predef, curtok);
                    ((IdentifierExpression)result).StartingPredefinedType = curtok.ID;
                    Advance();
                    break;

                default:
                    if (Lexer.IsKeyWord(curtok.ID))
                    {
                        // in this case a key word is used like a variable name.
                        goto case TokenID.Ident;
                    }
                    else
                    {
                        RecoverFromError(TokenID.Ident);
                        result = IdentifierExpression.GetErrorIdentifier(curtok);
                    }
                    break;
            }

            if (checkIsType)
            {
                result = CheckIdentifierIsType(result, consumeTypeParameter, allowTypeParameterAttributes, inDeclaration, alwaysGeneric);
            }

            //check if it is an array access
            // TODO

            return result;
        }

        private ExpressionNode ParseMember(bool alwaysGeneric)
        {
            return ParseIdentifierOrKeyword(true, true, false, false, alwaysGeneric);
        }

        private ExpressionNode ParseMemberAccess(ExpressionNode left)
        {
            TokenID qualifierKind = curtok.ID;
            Debug.Assert(qualifierKind == TokenID.Dot || qualifierKind == TokenID.MinusGreater, "Invalid qualifier kind: " + curtok.ToLongString());

            Advance(); // over dot or minusgreater
            if (curtok.ID != TokenID.Ident)
            {
                ReportError("Right side of member access must be identifier", curtok);
            }

            return new MemberAccessExpression(left, ParseMember(false), qualifierKind);
        }

        private void ParseInterfaceAccessors(out bool hasGetter, out NodeCollection<AttributeNode> getAttrs,
            out bool hasSetter, out NodeCollection<AttributeNode> setAttrs)
        {
            hasGetter = false;
            hasSetter = false;
            getAttrs = null;
            setAttrs = null;

            AssertAndAdvance(TokenID.LCurly); // LCurly

            ParsePossibleAttributes(false);

            if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "get")
            {
                if (curAttributes.Count > 0)
                {
                    getAttrs = curAttributes;
                    curAttributes = new NodeCollection<AttributeNode>();
                }

                hasGetter = true;
                Advance();
                AssertAndAdvance(TokenID.Semi);

                ParsePossibleAttributes(false);

                if (curtok.ID == TokenID.Ident)
                {
                    if (strings[curtok.Data] == "set")
                    {
                        if (curAttributes.Count > 0)
                        {
                            setAttrs = curAttributes;
                            curAttributes = new NodeCollection<AttributeNode>();
                        }

                        hasSetter = true;
                        Advance();
                        AssertAndAdvance(TokenID.Semi);
                    }
                    else
                    {
                        RecoverFromError("Expected set in interface property def.");
                    }
                }
            }
            else if (curtok.ID == TokenID.Ident && strings[curtok.Data] == "set")
            {
                if (curAttributes.Count > 0)
                {
                    setAttrs = curAttributes;
                    curAttributes = new NodeCollection<AttributeNode>();
                }

                hasSetter = true;
                Advance();
                AssertAndAdvance(TokenID.Semi);

                ParsePossibleAttributes(false);

                if (curtok.ID == TokenID.Ident)
                {
                    if (strings[curtok.Data] == "get")
                    {
                        if (curAttributes.Count > 0)
                        {
                            getAttrs = curAttributes;
                            curAttributes = new NodeCollection<AttributeNode>();
                        }

                        hasGetter = true;
                        Advance();
                        AssertAndAdvance(TokenID.Semi);
                    }
                    else
                    {
                        RecoverFromError("Expected get in interface property def.");
                    }
                }
            }
            else
            {
                RecoverFromError("Expected get or set in interface property def.");
            }

            AssertAndAdvance(TokenID.RCurly);
        }

        // statements		
        private StatementNode ParseStatement()
        {
            // label		ident	: colon
            // localDecl	type	: ident
            // block		LCurly
            // empty		Semi
            // expression	
            //	-invoke		pexpr	: LParen
            //	-objCre		new		: type
            //	-assign		uexpr	: assignOp
            //	-postInc	pexpr	: ++
            //	-postDec	pexpr	: --
            //	-preInc		++		: uexpr
            //	-preDec		--		: uexpr
            //
            // selection	if		: LParen
            //				switch	: LParen
            //
            // iteration	while	: LParen
            //				do		: LParen
            //				for		: LParen
            //				foreach	: LParen
            //
            // jump			break	: Semi
            //				continue: Semi
            //				goto	: ident | case | default
            //				return	: expr
            //				throw	: expr
            //
            // try			try		: block
            // checked		checked	: block
            // unchecked	unchecked : block
            // lock			lock	: LParen
            // using		using	: LParen

            StatementNode node;
            mayBeLocalDecl = false;
            switch (curtok.ID)
            {
                case TokenID.LCurly:	// block
                    BlockStatement newBlock = new BlockStatement(isUnsafe > 0, curtok);
                    newBlock.IsUnsafe = isUnsafe > 0;
                    node = newBlock;
                    ParseBlock(newBlock);
                    break;
                case TokenID.Semi:		// empty statement
                    node = new StatementNode(curtok);
                    Advance(); // over semi
                    break;
                case TokenID.If:		// If statement
                    node = ParseIf();
                    break;
                case TokenID.Switch:	// Switch statement
                    node = ParseSwitch();
                    break;
                case TokenID.While:		// While statement
                    node = ParseWhile();
                    break;
                case TokenID.Do:		// Do statement
                    node = ParseDo();
                    break;
                case TokenID.For:		// For statement
                    node = ParseFor();
                    break;
                case TokenID.Foreach:	// Foreach statement
                    node = ParseForEach();
                    break;
                case TokenID.Break:		// Break statement
                    node = ParseBreak();
                    break;
                case TokenID.Continue:	// Continue statement
                    node = ParseContinue();
                    break;
                case TokenID.Goto:		// Goto statement
                    node = ParseGoto();
                    break;
                case TokenID.Return:	// Return statement
                    node = ParseReturn();
                    break;
                case TokenID.Throw:		// Throw statement
                    node = ParseThrow();
                    break;
                case TokenID.Try:		// Try statement
                    node = ParseTry();
                    break;
                case TokenID.Checked:	// Checked statement
                    node = ParseChecked();
                    break;
                case TokenID.Unchecked:	// Unchecked statement
                    node = ParseUnchecked();
                    break;
                case TokenID.Lock:		// Lock statement
                    node = ParseLock();
                    break;
                case TokenID.Using:		// Using statement
                    node = ParseUsing();
                    break;

                case TokenID.Const:
                    node = null;
                    isLocalConst = true;
                    Advance(); // over const
                    break;

                case TokenID.StringLiteral:
                case TokenID.HexLiteral:
                case TokenID.IntLiteral:
                case TokenID.UIntLiteral:
                case TokenID.LongLiteral:
                case TokenID.ULongLiteral:
                case TokenID.True:
                case TokenID.False:
                case TokenID.Null:
                case TokenID.LParen:
                case TokenID.DecimalLiteral:
                case TokenID.RealLiteral:
                case TokenID.CharLiteral:
                case TokenID.PlusPlus:	// PreInc statement
                case TokenID.MinusMinus:// PreDec statement
                case TokenID.This:
                case TokenID.Base:
                case TokenID.New:		// creation statement
                    node = new ExpressionStatement(ParseExpression());
                    AssertAndAdvance(TokenID.Semi);
                    break;

                case TokenID.Void:			// TODO: Special case void
                case TokenID.Bool:
                case TokenID.Byte:
                case TokenID.Char:
                case TokenID.Decimal:
                case TokenID.Double:
                case TokenID.Float:
                case TokenID.Int:
                case TokenID.Long:
                case TokenID.Object:
                case TokenID.SByte:
                case TokenID.Short:
                case TokenID.String:
                case TokenID.UInt:
                case TokenID.ULong:
                case TokenID.UShort:
                    {
                        TypeNode type = new PredefinedTypeNode(curtok.ID, curtok);
                        Advance();
                        bool mustBeDecl = false;
                        if (curtok.ID == TokenID.Question)
                        {
                            Advance();
                            type.IsNullableType = true;
                            mustBeDecl = true;
                        }
                        else if (curtok.ID == TokenID.Star)
                        {
                            do
                            {
                                Advance();
                                type = new TypePointerNode(type);
                            }
                            while (curtok.ID == TokenID.Star);
                            mustBeDecl = true;
                        }
                        if (curtok.ID == TokenID.LBracket)
                        {
                            do
                            {
                                Advance();   // over lbracket
                                int commaCount = 0;
                                while (curtok.ID == TokenID.Comma)
                                {
                                    Advance();
                                    commaCount++;
                                }
                                type.RankSpecifiers.Add(commaCount);
                                AssertAndAdvance(TokenID.RBracket);
                            }
                            while (curtok.ID == TokenID.LBracket);
                            mustBeDecl = true;
                        }

                        if (curtok.ID == TokenID.Ident)
                        {
                            node = ParseLocalDeclarationStatement(type);
                        }
                        else if (mustBeDecl)
                        {
                            RecoverFromError(TokenID.Ident);
                            node = new StatementNode(curtok);
                        }
                        else
                        {
                            ExpressionNode expr = ParseSubexpression(1, type);
                            if (!(expr is InvocationExpression))
                            {
                                ReportError("Statement must be an invocation", expr.RelatedToken);
                            }
                            node = new ExpressionStatement(expr);
                        }
                        AssertAndAdvance(TokenID.Semi);
                        break;
                    }

                case TokenID.Ident:
                    {
                        if (strings[curtok.Data] == "yield")
                        {
                            node = ParseYieldStatement();
                            break;
                        }

                        ExpressionNode expr = ParseExpressionOrType(true);
                        if (expr is IdentifierExpression && curtok.ID == TokenID.Colon)
                        {
                            Advance(); // advance over colon
                            LabeledStatement lsnode = new LabeledStatement(curtok);
                            lsnode.Name = (IdentifierExpression)expr;
                            lsnode.Statement = ParseStatement();
                            node = lsnode;
                        }
                        else
                        {
                            if (expr is TypeNode)
                                node = ParseLocalDeclarationStatement((IType)expr);
                            else
                                node = new ExpressionStatement(expr);
                            AssertAndAdvance(TokenID.Semi);
                        }

                        /*					ExpressionNode expr = ParseIdentifierOrKeyword(false, true, false, false);
                                            IdentifierExpression idExpr = expr as IdentifierExpression;
                                            if(idExpr != null && curtok.ID == TokenID.Colon)
                                            {
                                                Advance(); // advance over colon
                                                LabeledStatement lsnode = new LabeledStatement(curtok);
                                                lsnode.Name = idExpr;
                                                lsnode.Statement = ParseStatement();
                                                node = lsnode;
                                            }
                                            else
                                            {
                                                mayBeLocalDecl = true;

                                                if(curtok.ID == TokenID.ColonColon)
                                                {
                                                    Advance();
                                                    if(curtok.ID != TokenID.Ident)
                                                    {
                                                        RecoverFromError(TokenID.Ident);
                                                        node = null;
                                                        break;
                                                    }
                                                    expr = new MemberAccessExpression(expr, new IdentifierExpression(strings[curtok.Data], curtok), TokenID.ColonColon);
                                                    Advance(); // over ident
                                                }

                                                while(curtok.ID == TokenID.Dot)
                                                {
                                                    Advance();
                                                    if(curtok.ID != TokenID.Ident)
                                                    {
                                                        RecoverFromError(TokenID.Ident);
                                                        node = null;
                                                        break;
                                                    }
                                                    expr = new MemberAccessExpression(expr, new IdentifierExpression(strings[curtok.Data], curtok), TokenID.Dot);
                                                    Advance(); // over ident
                                                }

                                                if(ParsePossibleTypeParameterNode(false, false, false))
                                                {
                                                    expr = new TypeNode(expr);
                                                    ApplyTypeParameters((TypeNode) expr);
                                                }

                                                if(curtok.ID == TokenID.LBracket)
                                                {
                                                    Advance();
                                                    if(curtok.ID == TokenID.Comma || curtok.ID == TokenID.RBracket)
                                                    {
                                                        TypeNode typeNode = expr as TypeNode;
                                                        if(typeNode == null)
                                                            expr = typeNode = new TypeNode(expr);
                                                        while(true)
                                                        {
                                                            int numCommas = 0;
                                                            while(curtok.ID == TokenID.Comma)
                                                            {
                                                                Advance();
                                                                numCommas++;
                                                            }
                                                            AssertAndAdvance(TokenID.RBracket);
                                                            typeNode.RankSpecifiers.Add(numCommas);

                                                            if(curtok.ID != TokenID.LBracket) break;
                                                            Advance();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ExpressionList exprList = ParseExpressionList();
                                                        expr = new ElementAccessExpression(expr, exprList);
                                                        AssertAndAdvance(TokenID.RBracket);
                                                    }
                                                }

                                                if(curtok.ID == TokenID.Question)
                                                {
                                                    Advance();
                                                    TypeNode typeNode = expr as TypeNode;
                                                    if(typeNode == null)
                                                        expr = typeNode = new TypeNode(expr);
                                                    typeNode.IsNullableType = true;
                                                }
                                                else
                                                {
                                                    while(curtok.ID == TokenID.Star)
                                                    {
                                                        Advance();
                                                        expr = new TypePointerNode(expr);
                                                    }
                                                }*/


                        //						expr = ParseSubexpression(PRECEDENCE_PRIMARY, expr);
                        /*						exprStack.Push(expr);
                                                ParseContinuingPrimary();
                                                expr = exprStack.Pop();*/
                        /*						if((expr is IdentifierExpression || expr is MemberAccessExpression || expr is TypeNode)
                                                        && (curtok.ID == TokenID.Ident || curtok.ID == TokenID.Star || curtok.ID == TokenID.Question))
                                                {
                                                    node = ParseLocalDeclarationStatement((IType) expr);
                                                }
                                                else
                                                    node = new ExpressionStatement(ParseSubexpression(1, expr));
                                                AssertAndAdvance(TokenID.Semi);
                                            }*/
                        break;
                    }

                case TokenID.Unsafe:
                    // preprocessor directives
                    node = ParseUnsafeCode();
                    break;

                case TokenID.Fixed:
                    // preprocessor directives
                    node = ParseFixedStatement();
                    break;


                case TokenID.Star:
                    // dereference variable 
                    // introduced because of the mono file test-406.cs
                    //private static uint DoOp2 (uint *u) {
                    //    *(u) += 100;
                    //    return *u;
                    //}
                    node = new ExpressionStatement(ParseExpression());
                    AssertAndAdvance(TokenID.Semi);
                    break;

                case TokenID.SingleComment:
                case TokenID.MultiComment:
                case TokenID.DocComment:
                    node = new CommentStatement(curtok, strings[curtok.Data], curtok.ID != TokenID.SingleComment);
                    Advance(); // over comment token
                    break;
                default:
                    {
                        Console.WriteLine("Unhandled case in statement parsing: \"" + curtok.ID + "\" in line: " + lineCount);
                        // this is almost always an expression
                        ExpressionStatement dnode = new ExpressionStatement(ParseExpression());
                        node = dnode;
                        if (curtok.ID == TokenID.Semi)
                        {
                            Advance();
                        }
                        break;
                    }
            }
            return node;
        }

        private void ParseBlock(BlockStatement node)
        {
            ParseBlock(node, false);
        }

        private void ParseBlock(BlockStatement node, bool isCase)
        {
            blockStack.Push(node);

            node.IsUnsafe = isUnsafe > 0;

            if (curtok.ID == TokenID.LCurly)
            {
                Advance(); // over lcurly
                while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.RCurly)
                {
                    node.Statements.Add(ParseStatement());
                }
                AssertAndAdvance(TokenID.RCurly);
            }
            else if (isCase)
            {
                // case stmts can have multiple lines without curlies, ugh
                // break can be omitted if it is unreachable code, double ugh
                // this becomes impossible to trace without code analysis of course, so look for 'case' or '}'

                while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.Case && curtok.ID != TokenID.Default && curtok.ID != TokenID.RCurly)
                {
                    node.Statements.Add(ParseStatement());
                }
                //bool endsOnReturn = false;
                //while (curtok.ID != TokenID.Eof && !endsOnReturn)
                //{
                //    TokenID startTok = curtok.ID;
                //    if (startTok == TokenID.Return	|| 
                //        startTok == TokenID.Goto	|| 
                //        startTok == TokenID.Throw	|| 
                //        startTok == TokenID.Break)
                //    {
                //        endsOnReturn = true;
                //    }

                //    ParseStatement(node.Statements);

                //    // doesn't have to end on return or goto
                //    if (endsOnReturn && (startTok == TokenID.Return	|| startTok == TokenID.Goto	|| startTok == TokenID.Throw))
                //    {
                //        if (curtok.ID == TokenID.Break)
                //        {
                //            ParseStatement(node.Statements);
                //        }
                //    }
                //}
            }
            else
            {
                node.Statements.Add(ParseStatement());
            }

            blockStack.Pop();
        }

        private IfStatement ParseIf()
        {
            IfStatement node = new IfStatement(curtok);
            Advance(); // advance over IF

            AssertAndAdvance(TokenID.LParen);
            node.Test = ParseExpression();
            AssertAndAdvance(TokenID.RParen);

            ParseBlock(node.Statements);

            if (curtok.ID == TokenID.Else)
            {
                Advance(); // advance of else
                ParseBlock(node.ElseStatements);
            }
            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private SwitchStatement ParseSwitch()
        {
            SwitchStatement node = new SwitchStatement(curtok);
            Advance(); // advance over SWITCH

            AssertAndAdvance(TokenID.LParen);
            node.Test = ParseExpression();
            AssertAndAdvance(TokenID.RParen);

            AssertAndAdvance(TokenID.LCurly);
            while (curtok.ID == TokenID.Case || curtok.ID == TokenID.Default)
            {
                node.Cases.Add(ParseCase());
            }

            AssertAndAdvance(TokenID.RCurly);

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private CaseNode ParseCase()
        {
            CaseNode node = new CaseNode(curtok);
            bool isDefault = (curtok.ID == TokenID.Default);
            Advance(); // advance over CASE or DEFAULT

            if (!isDefault)
            {
                node.Ranges.Add(ParseExpression());
            }
            else
            {
                node.IsDefaultCase = true;
            }
            AssertAndAdvance(TokenID.Colon);

            // may be multiple cases, but must be at least one
            while (curtok.ID == TokenID.Case || curtok.ID == TokenID.Default)
            {
                isDefault = (curtok.ID == TokenID.Default);
                Advance(); // advance over CASE or DEFAULT
                if (!isDefault)
                {
                    node.Ranges.Add(ParseExpression());
                }
                else
                {
                    node.IsDefaultCase = true;
                }
                AssertAndAdvance(TokenID.Colon);
            }

            do
            {
                node.Statements.Add(ParseStatement());
            }
            while (curtok.ID != TokenID.Eof && curtok.ID != TokenID.Case && curtok.ID != TokenID.Default && curtok.ID != TokenID.RCurly);

            return node;
        }
        private WhileStatement ParseWhile()
        {
            WhileStatement node = new WhileStatement(curtok);
            Advance(); // advance over While

            AssertAndAdvance(TokenID.LParen);
            node.Test = ParseExpression();
            AssertAndAdvance(TokenID.RParen);

            ParseBlock(node.Statements);

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private DoStatement ParseDo()
        {
            DoStatement node = new DoStatement(curtok);
            Advance(); // advance over DO

            ParseBlock(node.Statements);

            AssertAndAdvance(TokenID.While); // advance over While

            AssertAndAdvance(TokenID.LParen);
            node.Test = ParseExpression();
            AssertAndAdvance(TokenID.RParen);

            AssertAndAdvance(TokenID.Semi); // not optional on DO

            return node;
        }
        private ForStatement ParseFor()
        {
            ForStatement node = new ForStatement(curtok);
            Advance(); // advance over FOR

            AssertAndAdvance(TokenID.LParen);

            if (curtok.ID != TokenID.Semi)
            {
                ExpressionNode expr = ParseLocalDeclarationOrExpression();
                if (expr is LocalDeclaration)
                {
                    node.Init = new NodeCollection<ExpressionNode>();
                    node.Init.Add(expr);
                }
                else
                {
                    node.Init = new ExpressionList();
                    node.Init.Add(expr);
                    while (curtok.ID == TokenID.Comma)
                    {
                        Advance();
                        node.Init.Add(ParseExpression());
                    }
                }
            }
            AssertAndAdvance(TokenID.Semi);

            if (curtok.ID != TokenID.Semi)
                node.Test = ParseExpression();

            AssertAndAdvance(TokenID.Semi);

            if (curtok.ID != TokenID.RParen)
            {
                node.Inc = ParseExpressionList();
            }
            AssertAndAdvance(TokenID.RParen);
            ParseBlock(node.Statements);

            if (curtok.ID == TokenID.Semi)
            {
                Advance();
            }
            return node;
        }
        private ForEachStatement ParseForEach()
        {
            ForEachStatement node = new ForEachStatement(curtok);
            Advance(); // advance over FOREACH

            AssertAndAdvance(TokenID.LParen);
            node.Iterator = ParseParamDecl();
            AssertAndAdvance(TokenID.In);
            node.Collection = ParseExpression();
            AssertAndAdvance(TokenID.RParen);

            ParseBlock(node.Statements);

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private BreakStatement ParseBreak()
        {
            BreakStatement node = new BreakStatement(curtok);
            Advance(); // advance over BREAK

            if (curtok.ID == TokenID.Semi)
                Advance();

            return node;
        }
        private ContinueStatement ParseContinue()
        {
            ContinueStatement node = new ContinueStatement(curtok);
            Advance(); // advance over Continue

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private GotoStatement ParseGoto()
        {
            Advance();
            GotoStatement gn = new GotoStatement(curtok);
            if (curtok.ID == TokenID.Case)
            {
                Advance();
                gn.IsCase = true;
            }
            else if (curtok.ID == TokenID.Default)
            {
                Advance();
                gn.IsDefaultCase = true;
            }
            if (!gn.IsDefaultCase)
            {
                gn.Target = ParseExpression();
            }
            AssertAndAdvance(TokenID.Semi);
            return gn;
        }
        private ReturnStatement ParseReturn()
        {
            ReturnStatement node = new ReturnStatement(curtok);
            Advance(); // advance over Return

            if (curIterator != null
                    && curIterator.IsIterator)
            {
                ReportError("return unauthorized in iterator.");
            }

            if (curtok.ID == TokenID.Semi)
            {
                Advance();
            }
            else
            {
                node.ReturnValue = ParseExpression();
                AssertAndAdvance(TokenID.Semi);
            }
            return node;
        }
        private ThrowNode ParseThrow()
        {
            ThrowNode node = new ThrowNode(curtok);
            Advance(); // advance over Throw

            if (curtok.ID != TokenID.Semi)
            {
                node.ThrowExpression = ParseExpression();
            }

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private TryStatement ParseTry()
        {
            TryStatement node = new TryStatement(curtok);
            Advance(); // advance over Try

            isTry++;

            ParseBlock(node.TryBlock);

            isTry--;

            while (curtok.ID == TokenID.Catch)
            {
                isCatch++;

                CatchNode cn = new CatchNode(curtok);
                node.CatchBlocks.Add(cn);

                Advance(); // over catch
                if (curtok.ID == TokenID.LParen)
                {
                    Advance(); // over lparen

                    TypeNode tn = ParseType(false);
                    CheckRankSpecifier(tn);
                    cn.ClassType = tn;

                    if (curtok.ID == TokenID.Ident)
                    {
                        cn.Identifier = new IdentifierExpression(strings[curtok.Data], curtok);
                        Advance();
                    }
                    AssertAndAdvance(TokenID.RParen);

                    ParseBlock(cn.CatchBlock);
                }
                else
                {
                    ParseBlock(cn.CatchBlock);
                    isCatch--;
                    break; // must be last catch block if not a specific catch clause
                }

                isCatch--;
            }
            if (curtok.ID == TokenID.Finally)
            {
                Advance(); // over finally
                FinallyNode fn = new FinallyNode(curtok);
                node.FinallyBlock = fn;

                isFinally++;

                ParseBlock(fn.FinallyBlock);

                isFinally--;
            }

            if (curtok.ID == TokenID.Semi)
            {
                Advance();
            }

            hasYieldReturnInTry = false;

            return node;
        }
        private CheckedStatement ParseChecked()
        {
            CheckedStatement node = new CheckedStatement(curtok);
            AssertAndAdvance(TokenID.Checked); // advance over Checked

            if (curtok.ID == TokenID.LParen)
            {
                Advance();
                node.CheckedExpression = ParseExpression();
                AssertAndAdvance(TokenID.RParen);
            }
            else
            {
                node.CheckedBlock = new BlockStatement(curtok);
                ParseBlock(node.CheckedBlock);
            }

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private UncheckedStatement ParseUnchecked()
        {
            UncheckedStatement node = new UncheckedStatement(curtok);
            AssertAndAdvance(TokenID.Unchecked);  // advance over Unchecked

            if (curtok.ID == TokenID.LParen)
            {
                Advance();
                node.UncheckedExpression = ParseExpression();
                AssertAndAdvance(TokenID.RParen);
            }
            else
            {
                node.UncheckedBlock = new BlockStatement(curtok);
                ParseBlock(node.UncheckedBlock);
            }

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }
        private LockStatement ParseLock()
        {
            LockStatement node = new LockStatement(curtok);
            Advance(); // advance over Lock

            AssertAndAdvance(TokenID.LParen);
            node.Target = ParseExpression();
            AssertAndAdvance(TokenID.RParen);
            ParseBlock(node.Statements);

            if (curtok.ID == TokenID.Semi)
                Advance();
            return node;
        }

        private ExpressionNode ParseExpressionOrType(bool inStatementContext)
        {
            // Can the interior be a type? (A.2.2)
            switch (curtok.ID)
            {
                // type-name
                case TokenID.Ident:

                // simple-type
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

                // class-type
                case TokenID.Object:
                case TokenID.String:
                    // yes, it can
                    break;

                default:
                    // no, it can not
                    return ParseExpression();
            }

            ExpressionNode expr = ParseIdentifierOrKeyword(false, false, false, false, false);
            if (curtok.ID == TokenID.ColonColon)
            {
                // id :: id type-parameter-list_opt

                QualifiedIdentifierExpression qualID = new QualifiedIdentifierExpression(curtok);	// TODO: Replace by QualifiedAliasMember instance
                qualID.IsNamespaceAliasQualifier = true;
                qualID.Expressions.Add(expr);

                Advance(); // over ColonColon
                qualID.Expressions.Add(ParseMember(inStatementContext));

                expr = qualID;
            }
            else if (ParsePossibleTypeParameterNode(false, false, false, inStatementContext))
            {
                expr = new TypeNode(expr);
                ApplyTypeParameters((TypeNode)expr);
            }

            while (curtok.ID == TokenID.Dot)
            {
                Advance(); // over Dot
                if (curtok.ID != TokenID.Ident)
                {
                    RecoverFromError(TokenID.Ident);
                    return expr;
                }
                expr = new MemberAccessExpression(expr, ParseMember(inStatementContext), TokenID.Dot);
            }

            bool hasQuestionMark = false;
            int starCount = 0;
            bool hasBracket = false;

            if (curtok.ID == TokenID.Question)
            {
                Advance();
                hasQuestionMark = true;
            }

            while (curtok.ID == TokenID.Star)
            {
                Advance();
                starCount++;
            }

            // return (a * b);
            // return (a ? b : c);
            // a* b = c;
            // a? b = c;
            // a b = c;

            //if(curtok.ID == TokenID.Ident || (hasQuestionMark || starCount > 0) && curtok.ID == TokenID.RParen) goto foundType;

            if (curtok.ID == TokenID.Ident)
            {
                if (inStatementContext) goto foundType;
                else if (!hasQuestionMark && starCount == 0) goto foundType;	// it is a parse error
            }
            else if (curtok.ID == TokenID.RParen)
            {
                if (hasQuestionMark || starCount > 0) goto foundType;
            }
            else if (curtok.ID == TokenID.LBracket)
            {
                Advance();
                hasBracket = true;
                if (hasQuestionMark || starCount > 0 || curtok.ID == TokenID.Comma || curtok.ID == TokenID.RBracket) goto foundType;
            }

            //
            // treat as expression
            //
            if (hasQuestionMark)
            {
                // hasBracket is false
                ExpressionNode trueExpr = ParseSubexpression(starCount > 0 ? (int)PRECEDENCE_UNARY : 1);
                while (starCount-- > 0)
                    trueExpr = new DereferenceExpression(trueExpr);
                AssertAndAdvance(TokenID.Colon);
                return new ConditionalExpression(expr, trueExpr, ParseExpression());
            }
            else if (starCount > 0)
            {
                // hasBracket is false
                starCount--;
                ExpressionNode right = ParseSubexpression(starCount > 0 ? PRECEDENCE_UNARY : PRECEDENCE_MULTIPLICATIVE + ASSOC_LEFT);
                while (starCount-- > 0)
                    right = new DereferenceExpression(right);
                expr = new BinaryExpression(TokenID.Star, expr, right);
            }
            else if (hasBracket)
            {
                expr = new ElementAccessExpression(expr, ParseExpressionList());
                AssertAndAdvance(TokenID.RBracket);
            }
            return ParseSubexpression(1, expr);


            //
        // treat as type
        //
        foundType:
            TypeNode typeNode = new TypeNode(expr);
            if (hasQuestionMark)
                typeNode.IsNullableType = true;
            while (starCount-- > 0)
                typeNode = new TypePointerNode(typeNode);
            if (hasBracket)
            {
                while (true)
                {
                    int numCommas = 0;
                    while (curtok.ID == TokenID.Comma)
                    {
                        Advance();
                        numCommas++;
                    }
                    AssertAndAdvance(TokenID.RBracket);
                    typeNode.RankSpecifiers.Add(numCommas);

                    if (curtok.ID != TokenID.LBracket) break;
                    Advance();
                }
            }
            return typeNode;
        }

        private ExpressionNode ParseLocalDeclarationOrExpression()
        {
            ExpressionNode expr = ParseExpressionOrType(true);
            if (expr is TypeNode)
                return ParseLocalDeclaration((IType)expr);
            else
                return expr;
        }

        private UsingStatement ParseUsing()
        {
            UsingStatement node = new UsingStatement(curtok);
            Advance(); // advance over Using

            AssertAndAdvance(TokenID.LParen);
            node.Resource = ParseLocalDeclarationOrExpression();
            AssertAndAdvance(TokenID.RParen);
            ParseBlock(node.Statements);

            return node;
        }

        private YieldStatement ParseYieldStatement()
        {
            YieldStatement node = null;
            Advance(); // over yield

            if (isAnonymous > 0)
            {
                ReportError("yield not be authorized in anonymous method.");
            }

            if (curIterator != null)
            {
                //double check ...
                if (!curIterator.CouldBeIterator)
                {
                    ReportError("yield is permitted only in iterator's body.");
                }
                else
                {
                    curIterator.IsIterator = true;
                }
            }
            else
            {
                ReportError("yield is permitted only in iterator's body.");
            }

            switch (curtok.ID)
            {
                case TokenID.Return:
                    node = new YieldStatement(false, true, curtok);
                    Advance();
                    node.ReturnValue = ParseExpression();
                    break;
                case TokenID.Break:
                    node = new YieldStatement(true, false, curtok);
                    Advance();
                    break;
                default:
                    //TODO: Wait, but 'node' will still be null!
                    ReportError("Expected return or break. Found '" + curtok.ID.ToString().ToLower() + "'.");
                    Advance();
                    break;
            }

            //try .. catch .. finally checks
            if (node.IsReturn)
            {
                if (isTry > 0)
                    hasYieldReturnInTry = true;

                if (isCatch > 0)
                {
                    ReportError("'yield return' is not permitted in catch block.");

                    if (hasYieldReturnInTry)
                    {
                        ReportError("'yield return' is not permitted in try block with catch block.");
                    }
                }
            }

            if (isFinally > 0)
            {
                ReportError("'yield return' and 'yield break' are not permitted in finally block.");
            }

            AssertAndAdvance(TokenID.Semi);

            return node;
        }
        private BlockStatement ParseUnsafeCode()
        {
            isUnsafe++;

            BlockStatement ret = new BlockStatement(isUnsafe > 0, curtok);
            ret.IsUnsafeDeclared = true;
            ret.IsUnsafe = isUnsafe > 0;

            // todo: fully parse unsafe code
            AssertAndAdvance(TokenID.Unsafe); // over 'unsafe'

            if (curIterator != null)
            {
                if (curIterator.IsIterator)
                {
                    ReportError("Unsafe block not authorized in iterator");
                }
                else
                {
                    //at this point the parser did not parsed any yield statement
                    // so the block is not really an iterator, but it could be ;)
                    if (curIterator.CouldBeIterator)
                    {
                        ReportError("Warning : unsafe block in potential iterator");
                    }
                }
            }

            if (curtok.ID != TokenID.RCurly)
            {
                ParseBlock(ret);
            }
            else
            {
                Advance();
            }

            isUnsafe--;

            return ret;
        }

        private void ParseAddressOfIdentifier()
        {
            AssertAndAdvance(TokenID.BAnd);
            exprStack.Push(new AddressOfExpression(ParseExpression()));
        }

        private FixedStatementStatement ParseFixedStatement()
        {
            FixedStatementStatement ret = new FixedStatementStatement(curtok);

            AssertAndAdvance(TokenID.Fixed);
            AssertAndAdvance(TokenID.LParen);

            // parse  pointer-type   fixed-pointer-declarators
            ret.LocalPointers.Type = ParseType(false); // not sure about rank specifiers here...

            //parse 
            //  fixed-pointer-declarators:
            //      fixed-pointer-declarator
            //      fixed-pointer-declarators   ,   fixed-pointer-declarator
            //
            //  fixed-pointer-declarator:
            //      identifier   =   fixed-pointer-initializer
            //
            //  fixed-pointer-initializer:
            //      &   variable-reference
            //      expression

            if (curtok.ID != TokenID.RParen)
            {
                while (true)
                {
                    IdentifierExpression ident = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);

                    AssertAndAdvance(TokenID.Equal);

                    ret.LocalPointers.AddDeclarator(ident, ParseExpression());

                    if (curtok.ID != TokenID.Comma)
                        break;

                    Advance();
                }
            }

            AssertAndAdvance(TokenID.RParen);

            ParseBlock(ret.Statements);

            return ret;
        }


        private ExpressionNode ParseExpression(TokenID endToken)
        {
            /*			TokenID id = curtok.ID;
                        while (	id != endToken		&& id != TokenID.Eof	&& 
                                id != TokenID.Semi	&& id != TokenID.RParen &&
                                id != TokenID.Comma && id != TokenID.Colon)
                        {
                            ParseExpressionSegment();
                            id = curtok.ID;	
                        }
                        return exprStack.Pop();*/
            return ParseSubexpression(1);
        }

        private ExpressionNode ParseExpression()
        {
            return ParseSubexpression(1);
        }

        /// <summary>
        /// tag a type as nullable.
        /// If node is not a type, the method converts it to a type
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static ExpressionNode TagAsNullableType(ExpressionNode node)
        {
            // this is not a type declaration
            // the parser convert it to inullable type
            if (!(node is INullableType))
            {
                node = new TypeNode(node);
            }

            ((INullableType)node).IsNullableType = true;

            return node;
        }

        /// <summary>
        /// tag a type as pointer.
        /// If node is not a type pointer, the method converts it to a type pointer
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static ExpressionNode TagAsPointerType(ExpressionNode node)
        {
            if (!(node is IPointer))
            {
                node = new TypeNode(node);
            }
            return new TypePointerNode(node);
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            Token tok = curtok;
            ExpressionNode result;	// TODO: handle error cases (result = null)
            switch (curtok.ID)
            {
                #region Literals
                case TokenID.Null:
                    result = new NullPrimitive(curtok);
                    Advance();
                    break;

                case TokenID.True:
                    result = new BooleanPrimitive(true, curtok);
                    Advance();
                    break;

                case TokenID.False:
                    result = new BooleanPrimitive(false, curtok);
                    Advance();
                    break;

                case TokenID.IntLiteral:
                    result = new IntegralPrimitive(strings[curtok.Data], IntegralType.Int, curtok);
                    Advance();
                    break;
                case TokenID.UIntLiteral:
                    result = new IntegralPrimitive(strings[curtok.Data], IntegralType.UInt, curtok);
                    Advance();
                    break;
                case TokenID.LongLiteral:
                    result = new IntegralPrimitive(strings[curtok.Data], IntegralType.Long, curtok);
                    Advance();
                    break;
                case TokenID.ULongLiteral:
                    result = new IntegralPrimitive(strings[curtok.Data], IntegralType.ULong, curtok);
                    Advance();
                    break;

                case TokenID.RealLiteral:
                    result = new RealPrimitive(strings[curtok.Data], curtok);
                    Advance();
                    break;

                case TokenID.CharLiteral:
                    result = new CharPrimitive(strings[curtok.Data], curtok);
                    Advance();
                    break;

                case TokenID.StringLiteral:
                    result = new StringPrimitive(strings[curtok.Data], curtok);
                    Advance();
                    break;
                #endregion

                #region Predefined Types

                case TokenID.Bool:
                case TokenID.Byte:
                case TokenID.Char:
                case TokenID.Decimal:
                case TokenID.Double:
                case TokenID.Float:
                case TokenID.Int:
                case TokenID.Long:
                case TokenID.Object:
                case TokenID.SByte:
                case TokenID.Short:
                case TokenID.String:
                case TokenID.UInt:
                case TokenID.ULong:
                case TokenID.UShort:
                    result = new PredefinedTypeNode(tok.ID, tok);
                    Advance();
                    result = ParseMemberAccess(result);
                    break;

                #endregion

                #region Keywords
                case TokenID.This:
                    {
                        Advance();
                        IdentifierExpression idExpr = new IdentifierExpression("this", tok);
                        idExpr.StartingPredefinedType = tok.ID;
                        result = idExpr;
                        break;
                    }

                case TokenID.Base:
                    Advance();   // over base
                    if (curtok.ID == TokenID.LBracket)         // base indexer expression
                    {
                        Advance();
                        result = new ElementAccessExpression(new IdentifierExpression("base", tok), ParseExpressionList());
                        AssertAndAdvance(TokenID.RBracket);
                        break;
                    }

                    if (curtok.ID != TokenID.Dot)
                    {
                        RecoverFromError(TokenID.Dot, TokenID.LBracket);
                        result = null;
                        break;
                    }

                    // base access expression
                    Advance();   // advance over dot
                    result = new BaseAccessExpression(ParseMember(false));
                    break;

                case TokenID.Checked:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    result = new CheckedExpression(ParseExpression());
                    AssertAndAdvance(TokenID.RParen);
                    break;

                case TokenID.Unchecked:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    result = new UncheckedExpression(ParseExpression());
                    AssertAndAdvance(TokenID.RParen);
                    break;

                case TokenID.Default:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    result = new DefaultConstantExpression(ParseType(false));
                    AssertAndAdvance(TokenID.RParen);
                    break;

                case TokenID.New:
                    Advance();

                    isNewStatement = true;

                    IType newType = ParseType(false);

                    //ApplyTypeParameters(newType); -> not sure, but a type pointer can not be generic!

                    if (curtok.ID == TokenID.LParen)
                    {
                        Advance();
                        result = new ObjectCreationExpression(newType, ParseArgumentList(), tok);
                        AssertAndAdvance(TokenID.RParen);
                    }
                    else if (curtok.ID == TokenID.LBracket)
                    {
                        result = ParseArrayCreation(newType);
                    }
                    else
                    {
                        RecoverFromError(TokenID.LParen, TokenID.LBracket);
                        result = null;
                    }

                    isNewStatement = false;
                    break;

                case TokenID.Typeof:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    result = new TypeOfExpression(ParseType(false));
                    CheckRankSpecifier(result);
                    AssertAndAdvance(TokenID.RParen);
                    break;

                case TokenID.Sizeof:
                    Advance();
                    AssertAndAdvance(TokenID.LParen);
                    result = new SizeOfExpression(ParseType(false));
                    AssertAndAdvance(TokenID.RParen);
                    break;

                case TokenID.Delegate:    //anonymous method
                    Advance();
                    result = ParseAnonymousMethod();
                    break;

                #endregion Keywords

                case TokenID.Ident:
                    result = ParseIdentifierOrKeyword(false, false, false, false, false);
                    if (curtok.ID == TokenID.ColonColon)
                    {
                        // id :: id type-parameter-list_opt . id type-parameter-list_opt

                        QualifiedIdentifierExpression qualID = new QualifiedIdentifierExpression(curtok);	// TODO: Replace by QualifiedAliasMember instance
                        qualID.IsNamespaceAliasQualifier = true;
                        qualID.Expressions.Add(result);

                        Advance(); // over ColonColon
                        qualID.Expressions.Add(ParseMember(false));

                        result = ParseMemberAccess(qualID);
                    }
                    else if (ParsePossibleTypeParameterNode(false, false, false, false))
                    {
                        result = new TypeNode(result);
                        ApplyTypeParameters((TypeNode)result);
                    }
                    break;

                case TokenID.LParen:
                    Advance();
                    result = ParseCastOrGroup();
                    break;

                default:
                    RecoverFromError("Unexpected token in primary expression");
                    return null;		// TODO: Invalid expression
            }
            return result;
        }

        private ExpressionNode ParseSubexpression(int precBound)
        {
            ExpressionNode expr;
            switch (curtok.ID)
            {
                case TokenID.Plus:
                case TokenID.Minus:
                case TokenID.Not:
                case TokenID.Tilde:
                case TokenID.PlusPlus:
                case TokenID.MinusMinus:
                case TokenID.BAnd:
                case TokenID.Star:
                    {
                        UnaryExpression unExpr = new UnaryExpression(curtok.ID, curtok);
                        Advance();
                        unExpr.Child = ParseSubexpression(PRECEDENCE_UNARY);
                        expr = unExpr;
                        break;
                    }

                default:
                    expr = ParsePrimaryExpression();
                    break;
            }
            return ParseSubexpression(precBound, expr);
        }

        private ExpressionNode ParseSubexpression(int precBound, ExpressionNode left)
        {
            while (true)
            {
                int curPrec = precedence[(int)curtok.ID];
                if (curPrec < precBound) break;

                int associativity = ASSOC_LEFT;
                TokenID curOp = curtok.ID;
                switch (curOp)
                {
                    case TokenID.Equal:
                    case TokenID.PlusEqual:
                    case TokenID.MinusEqual:
                    case TokenID.StarEqual:
                    case TokenID.SlashEqual:
                    case TokenID.PercentEqual:
                    case TokenID.BAndEqual:
                    case TokenID.BOrEqual:
                    case TokenID.BXorEqual:
                    case TokenID.ShiftLeftEqual:
                    case TokenID.QuestionQuestion:
                        associativity = ASSOC_RIGHT;
                        goto case TokenID.Percent;		// "FALL THROUGH"

                    case TokenID.Greater:
                        Advance();
                        if (curtok.ID == TokenID.Greater && curtok.LastCharWasGreater)
                        {
                            curOp = TokenID.ShiftRight;
                            Advance();
                        }
                        else if (curtok.ID == TokenID.GreaterEqual && curtok.LastCharWasGreater)
                        {
                            curOp = TokenID.ShiftRightEqual;
                            associativity = ASSOC_RIGHT;
                            Advance();
                        }
                        goto parseBinOp;

                    case TokenID.Or:
                    case TokenID.And:
                    case TokenID.BOr:
                    case TokenID.BXor:
                    case TokenID.BAnd:
                    case TokenID.EqualEqual:
                    case TokenID.NotEqual:
                    case TokenID.Less:
                    case TokenID.LessEqual:
                    case TokenID.GreaterEqual:
                    case TokenID.ShiftLeft:
                    case TokenID.Plus:
                    case TokenID.Minus:
                    case TokenID.Star:
                    case TokenID.Slash:
                    case TokenID.Percent:
                        Advance();
                    parseBinOp: left = new BinaryExpression(curOp, left, ParseSubexpression(curPrec + associativity));
                    break;

                    case TokenID.PlusPlus:								// postfix
                    Advance();
                    left = new PostIncrementExpression(left);
                    break;

                    case TokenID.MinusMinus:							// postfix
                    Advance();
                    left = new PostDecrementExpression(left);
                    break;

                    case TokenID.Is:
                    case TokenID.As:
                    Advance();
                    left = new BinaryExpression(curOp, left, ParseType(true));
                    break;

                    case TokenID.Question:
                    {
                        Advance();
                        ExpressionNode thenExpr = ParseExpression();
                        AssertAndAdvance(TokenID.Colon);
                        left = new ConditionalExpression(left, thenExpr, ParseExpression());
                        break;
                    }

                    case TokenID.LParen:								// invocation
                    Advance();
                    left = new InvocationExpression(left, ParseArgumentList());
                    AssertAndAdvance(TokenID.RParen);
                    break;

                    case TokenID.LBracket:								// element access
                    Advance();
                    left = new ElementAccessExpression(left, ParseExpressionList());
                    AssertAndAdvance(TokenID.RBracket);
                    break;

                    case TokenID.Dot:									// member access
                    case TokenID.MinusGreater:
                    left = ParseMemberAccess(left);
                    break;

                    default:
                    ReportError("Unexpected token", curtok);
                    return left;
                }
            }
            return left;
        }

        [Obsolete]
        private void ParseExpressionSegment()
        {
            #region Chart
            // baseAccess	base		: Dot
            //				base		: LBracket
            // delgCre		new			: delgType : LParen
            // typeof		typeof		: LParen
            // checked		checked		: LParen
            // unchecked	unchecked	: LParen
            #endregion

            bool stackCountUnaltered = false;
            int startStackCount = exprStack.Count;
            TokenID startToken = curtok.ID;
            switch (curtok.ID)
            {
                #region Conditional
                case TokenID.Question:
                    mayBeLocalDecl = false;
                    stackCountUnaltered = true;
                    if (curtok.NullableDeclaration)
                    {
                        Advance();

                        ExpressionNode expr = TagAsNullableType(exprStack.Pop());
                        CheckRankSpecifier(expr);

                        exprStack.Push(expr);
                    }
                    else
                    {
                        Advance();

                        ConditionalExpression conditionalExpression = new ConditionalExpression(exprStack.Pop(), null, null);

                        exprStack.Push(conditionalExpression);

                        ExpressionNode cond1 = ParseExpression(TokenID.Equal);
                        AssertAndAdvance(TokenID.Colon);
                        ExpressionNode cond2 = ParseExpression();

                        conditionalExpression.Left = cond1;
                        conditionalExpression.Right = cond2;
                    }

                    break;

                #endregion

                #region Keywords
                // keywords
                case TokenID.Ref:
                    mayBeLocalDecl = false;
                    Advance();
                    ParseExpressionSegment();
                    exprStack.Push(new RefNode(exprStack.Pop()));
                    break;

                case TokenID.Out:
                    mayBeLocalDecl = false;
                    Advance();
                    ParseExpressionSegment();
                    exprStack.Push(new OutNode(exprStack.Pop()));
                    break;

                case TokenID.Void:
                    // this can happen in typeof(void) or void* x;
                    exprStack.Push(new VoidPrimitive(curtok));
                    Advance();
                    if (curtok.ID == TokenID.Star)
                    {
                        do
                        {
                            Advance();
                            exprStack.Push(new TypePointerNode(exprStack.Pop()));
                        }
                        while (curtok.ID == TokenID.Star);
                    }
                    else
                        mayBeLocalDecl = false;
                    break;

                #endregion

                case TokenID.Stackalloc:
                    mayBeLocalDecl = false;
                    ParseStackalloc();
                    break;
            }

            if (exprStack.Count != startStackCount + (stackCountUnaltered ? 0 : 1))
            {
                ReportError("Internal compiler error: Expression stack inconsistent!", curtok);
            }
        }

        private bool isAfterType()
        {
            bool result = false;
            if (exprStack.Count > 0)
            {
                ExpressionNode node = exprStack.Peek();

                if (node is QualifiedIdentifierExpression)
                {
                    QualifiedIdentifierExpression qie = (QualifiedIdentifierExpression)exprStack.Pop();
                    TypeNode t = new TypeNode(qie);
                    exprStack.Push(t);
                    result = true;
                }
                else
                {
                    if (node is IdentifierExpression)
                    {
                        IdentifierExpression ie = (IdentifierExpression)exprStack.Pop();
                        TypeNode t = new TypeNode(ie);
                        exprStack.Push(t);
                        result = true;
                    }
                    else
                    {
                        if (node is TypeNode
                            || node is TypePointerNode
                            || node is MemberAccessExpression)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        private NodeCollection<ArgumentNode> ParseArgumentList()
        {
            NodeCollection<ArgumentNode> list = new NodeCollection<ArgumentNode>();
            if (curtok.ID == TokenID.RParen) return list;

            while (true)
            {
                ArgumentNode arg = new ArgumentNode(curtok);
                switch (curtok.ID)
                {
                    case TokenID.Out:
                        Advance();
                        arg.IsOut = true;
                        break;

                    case TokenID.Ref:
                        Advance();
                        arg.IsRef = true;
                        break;
                }
                arg.Expression = ParseExpression();
                list.Add(arg);
                if (curtok.ID != TokenID.Comma) break;
                Advance();
            }
            return list;
        }

        private ExpressionList ParseExpressionList()
        {
            ExpressionList list = new ExpressionList();
            while (true)
            {
                list.Add(ParseExpression());
                if (curtok.ID != TokenID.Comma) break;
                Advance();
            }
            return list;
        }

        /// <summary>
        /// Parses a local declaration.
        /// </summary>
        /// <param name="type">The already parsed type for the local declaration</param>
        private LocalDeclaration ParseLocalDeclaration(IType type)
        {
            LocalDeclaration lnode = new LocalDeclaration(curtok);
            lnode.Type = type;
            lnode.IsConstant = isLocalConst;
            isLocalConst = false;

            while (true)
            {
                IdentifierExpression declIdentifier = (IdentifierExpression)ParseIdentifierOrKeyword(false, false, false, false, false);
                ExpressionNode initializer = null;
                if (curtok.ID == TokenID.Equal)                                      // Optional initializer?
                {
                    Advance();
                    if (curtok.ID == TokenID.LCurly)
                        initializer = ParseArrayInitializer();
                    else if (curtok.ID == TokenID.Stackalloc)
                        initializer = ParseStackalloc();
                    else
                        initializer = ParseExpression();
                }
                lnode.AddDeclarator(declIdentifier, initializer);

                if (curtok.ID != TokenID.Comma) break;
                Advance();
            }
            return lnode;
        }

        private LocalDeclarationStatement ParseLocalDeclarationStatement(IType type)
        {
            return new LocalDeclarationStatement(ParseLocalDeclaration(type));
        }

        private AnonymousMethodNode ParseAnonymousMethod()
        {
            AnonymousMethodNode ret = new AnonymousMethodNode(curtok);

            isAnonymous++;

            if (curtok.ID == TokenID.LParen)
            {
                ret.Parameters = ParseParamList();
                if (ret.Parameters == null)
                    ret.Parameters = new NodeCollection<ParamDeclNode>();
            }

            ParseBlock(ret.StatementBlock);

            isAnonymous--;

            return ret;
        }

        private void ParseSizeOf()
        {
            AssertAndAdvance(TokenID.Sizeof);
            AssertAndAdvance(TokenID.LParen);

            SizeOfExpression expr = new SizeOfExpression(ParseExpression(TokenID.RParen));

            AssertAndAdvance(TokenID.RParen);

            exprStack.Push(expr);
        }

        private StackallocExpression ParseStackalloc()
        {
            if (isUnsafe <= 0)
            {
                ReportError("stackalloc only allowed in unsafe context.");
            }

            if (isFinally > 0 || isCatch > 0)
            {
                ReportError("stackalloc is not allowed in catch/finally block.");
            }

            Token tok = curtok;
            AssertAndAdvance(TokenID.Stackalloc);

            TypeNode type = ParseType(false);

            AssertAndAdvance(TokenID.LBracket);

            ExpressionNode n = ParseExpression(TokenID.RBracket);

            StackallocExpression expr = new StackallocExpression(type, n, tok);

            AssertAndAdvance(TokenID.RBracket);

            return expr;
        }

        /// <summary>
        /// Checks, whether the sequence of tokens in parentheses represented by "interior"
        /// shall be considered as a cast according to ECMA-334 section 14.6.6
        /// </summary>
        private bool ConsiderAsCast(ExpressionNode interior)
        {
            // Cannot be an expression?					TODO: More conditions may be missing
            if (interior is PredefinedTypeNode) return true;

            // Is not correct grammar for a type?		TODO: More conditions may be missing
            if (!(interior is IdentifierExpression) && !(interior is MemberAccessExpression)
                    && !(interior is TypeNode) && !(interior is TypePointerNode))
                return false;

            if (curtok.ID == TokenID.As || curtok.ID == TokenID.Is) return false;
            if (Lexer.IsKeyWord(curtok.ID)) return true;
            switch (curtok.ID)
            {
                case TokenID.Tilde:
                case TokenID.Not:
                case TokenID.LParen:
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
                    return true;
            }
            return false;
        }

        private ExpressionNode ParseCastOrGroup()
        {
            // cast-expression: (type) unary-expression
            // parenthesized-expression: (expression)

            ExpressionNode interior = ParseExpressionOrType(false);
            AssertAndAdvance(TokenID.RParen);

            if (ConsiderAsCast(interior))
            {
                UnaryCastExpression castNode = new UnaryCastExpression(curtok);
                castNode.Type = (IType)interior;
                castNode.Child = ParseSubexpression(PRECEDENCE_UNARY);
                return castNode;
            }
            else
                return new ParenthesizedExpression(interior);
        }

        private ArrayInitializerExpression ParseArrayInitializer()
        {
            ArrayInitializerExpression init = new ArrayInitializerExpression(curtok);
            AssertAndAdvance(TokenID.LCurly);
            if (curtok.ID != TokenID.RCurly)
            {
                do
                {
                    init.Expressions.Add(curtok.ID == TokenID.LCurly ? ParseArrayInitializer() : ParseExpression());
                    if (curtok.ID != TokenID.Comma) break;
                    Advance();
                }
                while (curtok.ID != TokenID.RCurly);
            }
            AssertAndAdvance(TokenID.RCurly);
            return init;
        }

        private ArrayCreationExpression ParseArrayCreation(IType type)
        {
            ArrayCreationExpression arNode = new ArrayCreationExpression(curtok);
            arNode.Type = type;

            Advance(); // over lbracket

            bool mustHaveInitializer;
            if (curtok.ID == TokenID.Comma)
            {
                int comma = 0;
                // comma specifier
                do
                {
                    ++comma;
                    Advance();
                } while (curtok.ID == TokenID.Comma);
                arNode.AdditionalRankSpecifiers.Add(comma);
                mustHaveInitializer = true;
            }
            else if (curtok.ID == TokenID.RBracket)
            {
                arNode.AdditionalRankSpecifiers.Add(0);
                mustHaveInitializer = true;
            }
            else
            {
                // this tests for literal size declarations on first rank specifiers
                arNode.RankSpecifier = ParseExpressionList();
                mustHaveInitializer = false;
            }

            AssertAndAdvance(TokenID.RBracket);

            // now any 'rank only' specifiers (without size decls)
            while (curtok.ID == TokenID.LBracket)
            {
                Advance(); // over lbracket
                int commaCount = 0;
                while (curtok.ID == TokenID.Comma)
                {
                    commaCount++;
                    Advance();
                }
                arNode.AdditionalRankSpecifiers.Add(commaCount);
                AssertAndAdvance(TokenID.RBracket);
            }
            if (curtok.ID == TokenID.LCurly)
                arNode.Initializer = ParseArrayInitializer();
            else if (mustHaveInitializer)
                ReportError("Array allocation with only rank specifiers must be initialized.", arNode.RelatedToken);

            return arNode;
        }

        [Obsolete]
        private void ParseContinuingPrimary()
        {
        }

        private bool ParseElementAccess()
        {
            bool isElementAccess = true;
            Advance(); // over lbracket
            ExpressionNode type = exprStack.Pop(); // the caller pushed, so must have at least one element

            // case one is actually a type decl (like T[,,]), not element access (like T[2,4])
            // in this case we need to push the type, and abort parsing the continuing
            if (curtok.ID == TokenID.Comma || curtok.ID == TokenID.RBracket)
            {
                isElementAccess = false;

                if (!(type is IType))
                {
                    type = new TypeNode(type);
                }

                exprStack.Push(type);
                ParseArrayRank((IType)type);
            }
            else
            {
                // element access case
                if (type is PrimaryExpression)
                {
                    PrimaryExpression tp = (PrimaryExpression)type;
                    ExpressionList el = ParseExpressionList();
                    AssertAndAdvance(TokenID.RBracket);
                    exprStack.Push(new ElementAccessExpression(tp, el));
                }
                else
                {
                    ReportError("Left side of Element Access must be primary expression.");
                }
            }

            return isElementAccess;
        }
        private void ParseArrayRank(IType type)
        {
            // now any 'rank only' specifiers (without size decls)
            bool firstTime = true;
            while (curtok.ID == TokenID.LBracket || firstTime)
            {
                if (!firstTime)
                {
                    Advance();
                }
                firstTime = false;
                int commaCount = 0;
                while (curtok.ID == TokenID.Comma)
                {
                    commaCount++;
                    Advance();
                }
                type.RankSpecifiers.Add(commaCount);
                AssertAndAdvance(TokenID.RBracket);
            }
        }

        // utility
        private void RecoverFromError(params TokenID[] ids)
        {
            RecoverFromError("", ids);
        }
        private void RecoverFromError(string message, params TokenID[] ids)
        {
            string msg = "Error: ";
            if (ids.Length != 0)
            {
                msg += "Expected ";
                bool first = true;
                foreach (TokenID id in ids)
                {
                    if (first) first = false;
                    else msg += " or ";
                    msg += id;
                }
                msg += " found: " + curtok.ID;
            }
            else msg += "Found: " + curtok.ID;
            if (message != null)
                msg = message + msg;
            if (message == null && ids.Length == 0)
                msg += "Unknown error at " + curtok.Line + ", " + curtok.Col;

            ReportError(msg);

            // the recover from try to recover a correct state from the erroned token
            // flush all stacks 

            //typeStack;
            //curMethod = null;
            //curOperator = null;
            //curIterator = null;

            //exprStack;
            //curState;
            //curInterface;
            //curtok;
            //curTokNode = null;

            //curmods;
            //nextIsPartial = false;
            //curAttributes;

            //curTypeParameters;

            //curTypeParameterConstraints;

            //blockStack;

            //inPPDirective = false;
            //isAnonynous = 0;

            //isNewStatement = false;
            //isUnsafe = 0;

            //isTry = 0;
            //isCatch = 0;
            //isFinally = 0;

            Advance();
        }
        private void ReportError(string message)
        {
            ReportError(message, curtok);
        }

        private void ReportError(string message, Token tok)
        {
            Errors.Add(new Error(message, tok, tok.Line, tok.Col, currentFileName));
        }

        private void AssertAndAdvance(TokenID id)
        {
            if (curtok.ID != id)
            {
                RecoverFromError(id);
            }
            Advance();
        }

        private Token PeekNextNonWhitespace()
        {
            Token result;
            LinkedListNode<Token> nextNode  = nextTokNode;
            bool skipping = true;
            do
            {
                if (nextNode == null)
                {
                    result = EOF;
                    break;
                }
                else
                {
                    result = nextNode.Value;
                }

                switch (nextNode.Value.ID)
                {
                    case TokenID.SingleComment:
                    case TokenID.MultiComment:
                    case TokenID.DocComment:
                    case TokenID.Newline:
                        break;

                    default:
                        skipping = false;
                        break;
                }
            } while (skipping);

            return result;
        }
        private void Advance()
        {
            bool skipping = true;
            int startLine = lineCount;
            do
            {
                if (nextTokNode != null)
                {
                    curtok = nextTokNode.Value;
                    nextTokNode = nextTokNode.Next;
                }
                else
                {
                    curtok = EOF;
                }

                switch (curtok.ID)
                {
                    case TokenID.SingleComment:
                    case TokenID.MultiComment:
                    case TokenID.DocComment:
                        ParsePossibleDocComment();
                        break;
                    case TokenID.Newline:
                        lineCount++;
                        break;

                    case TokenID.Hash:
                        // preprocessor directives
                        if (!inPPDirective)
                        {
                            ParsePreprocessorDirective();
                            if (curtok.ID != TokenID.Newline &&
                                curtok.ID != TokenID.SingleComment &&
                                curtok.ID != TokenID.MultiComment &&
                                curtok.ID != TokenID.Hash)
                            {
                                skipping = false;
                            }
                            else if (curtok.ID == TokenID.Hash)
                            {
                                //index--;
                                nextTokNode = nextTokNode.Previous;
                            }
                        }
                        else
                        {
                            skipping = false;
                        }
                        break;

                    default:
                        skipping = false;
                        break;
                }
            } while (skipping);
        }
        private void SkipToEOL(int startLine)
        {
            if (lineCount > startLine)
            {
                return;
            }
            bool skipping = true;
            do
            {
                if (nextTokNode != null)
                {
                    curtok = nextTokNode.Value;
                    nextTokNode = nextTokNode.Next;
                }
                else
                {
                    curtok = EOF;
                    skipping = false;
                }

                if (curtok.ID == TokenID.Newline)
                {
                    lineCount++;
                    skipping = false;
                }
            } while (skipping);
        }
        private void SkipToNextHash()
        {
            bool skipping = true;
            do
            {

                if (nextTokNode != null)
                {
                    curtok = nextTokNode.Value;
                    nextTokNode = nextTokNode.Next;
                }
                else
                {
                    curtok = EOF;
                    skipping = false;
                }

                if (curtok.ID == TokenID.Hash)
                {
                    skipping = false;
                }
                else if (curtok.ID == TokenID.Newline)
                {
                    lineCount++;
                }
            } while (skipping);
        }
        private void SkipToElseOrEndIf()
        {
            // advance to elif, else, or endif
            int endCount = 1;
            bool firstPassHash = curtok.ID == TokenID.Hash;
            while (endCount > 0)
            {
                if (!firstPassHash)
                {
                    SkipToNextHash();
                }
                firstPassHash = false;

                if (nextTokNode == tokens.Last)
                {
                    break;
                }

                Token tk = nextTokNode.Value;

                if (tk.ID == TokenID.Ident)
                {
                    string sKind = strings[tk.Data];
                    if (sKind == "endif")
                    {
                        endCount--;
                    }
                    else if (sKind == "elif")
                    {
                        if (endCount == 1)
                        {
                            break;
                        }
                    }
                }
                else if (tk.ID == TokenID.If)
                {
                    endCount++;
                }
                else if (tk.ID == TokenID.Else)
                {
                    if (endCount == 1)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        #region Nested type: Error

        /// <summary>
        /// Error message struct
        /// </summary>
        [DebuggerDisplay("Message = {Message}, Line = {Line}, Column={Column} \n File={FileName}")]
        public struct Error
        {
            public readonly int Column;
            public readonly string FileName;
            public readonly int Line;
            public readonly string Message;
            public readonly Token Token;

            public Error(string message, Token token, int line, int column, string fileName)
            {
                Message = message;
                Token = token;
                Line = line;
                Column = column;
                FileName = fileName;
            }
        }

        #endregion
    }
}
