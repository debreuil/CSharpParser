using System;
using System.Collections.Generic;

namespace DDW
{
    [Flags]
    public enum IdentifierType 
    { 
        Local,
        Nested,
        Parameters,
        Field, 
        Property,
        Enum,
        Struct,
        Class,
        Interface,
        Delegate,
        GenericParameter
    }

    public struct GraphTypeNode
    {
        public IdentifierType Type;
        public BaseNode Node;

        public GraphTypeNode(IdentifierType type, BaseNode node)
        {
            Type = type;
            Node = node;
        }
    }

    public class GraphTypeStack
    {
        Dictionary<string, Stack<GraphTypeNode>> stack = new Dictionary<string,Stack<GraphTypeNode>>();

        public GraphTypeNode this[string ident]
        {
            get
            {
                return stack[ident].Peek();
            }
        }

        public GraphTypeNode PopIdentifierType(string ident)
        {
            return stack[ident].Pop();
        }

        public void PushIdentifierType(string ident, GraphTypeNode type)
        {
            stack[ident].Push(type);
        }

        public bool CheckExist(string ident)
        {
            return stack.ContainsKey(ident);
        }
    }

    public class GraphVisitor
    {
        List<CompilationUnitNode> compilationUnits = null;
        GraphTypeStack typeStack = new GraphTypeStack();

        public GraphVisitor(List<CompilationUnitNode> cus )
        {
            compilationUnits = cus;
        }

        //this parse only declared types.
        private void ParseNameSpaceTypes( NamespaceNode ns )
        {
            //first parse all delegate
            foreach (DelegateNode d in ns.Delegates)
            {
                typeStack.PushIdentifierType(d.Name.Identifier, new GraphTypeNode(IdentifierType.Delegate, d));
            }

            //parse enum
            foreach (EnumNode e in ns.Enums)
            {
                typeStack.PushIdentifierType( e.Name.Identifier, new GraphTypeNode(IdentifierType.Enum, e));
            }

            //parse Classes
            foreach (ClassNode cl in ns.Classes)
            {
                typeStack.PushIdentifierType(cl.Name.Identifier, new GraphTypeNode(IdentifierType.Class, cl));
            }

            //parse structs
            foreach (StructNode st in ns.Structs)
            {
                typeStack.PushIdentifierType(st.Name.Identifier, new GraphTypeNode(IdentifierType.Struct, st));
            }

            //parse interfaces
            foreach (InterfaceNode i in ns.Interfaces)
            {
                typeStack.PushIdentifierType(i.Name.Identifier, new GraphTypeNode(IdentifierType.Interface, i));
            }

            //other namespaces
            foreach (NamespaceNode nsChild in ns.Namespaces)
            {
                ParseNameSpaceTypes(nsChild);
            }
        }

        public void Start()
        {
            foreach (CompilationUnitNode cu in compilationUnits)
            {
                //default namespace
                ParseNameSpace(cu.DefaultNamespace);

                //other namespaces
                foreach (NamespaceNode ns in cu.Namespaces)
                {
                    ParseNameSpaceTypes(ns);
                }
            }
        }
    }
}
