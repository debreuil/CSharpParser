using System;
using System.Diagnostics;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class ClassNode : ConstructedTypeNode
	{
        private TypeCollection baseClasses;
      private NodeCollection<ClassNode> classes = new NodeCollection<ClassNode>();
      private NodeCollection<ConstantNode> constants;
      private NodeCollection<ConstructorNode> constructors;
      private NodeCollection<DelegateNode> delegates = new NodeCollection<DelegateNode>();
      private NodeCollection<DestructorNode> destructors;

      private NodeCollection<EnumNode> enums = new NodeCollection<EnumNode>();
      private NodeCollection<EventNode> events;
      private NodeCollection<FieldNode> fields;
      private NodeCollection<FixedBufferNode> fixedBuffers = new NodeCollection<FixedBufferNode>();
      private NodeCollection<IndexerNode> indexers;
      private NodeCollection<InterfaceNode> interfaces = new NodeCollection<InterfaceNode>();
      private NodeCollection<MethodNode> methods;
      private NodeCollection<OperatorNode> operators;
      private NodeCollection<PropertyNode> properties;
      private NodeCollection<StructNode> structs = new NodeCollection<StructNode>();

      public ClassNode(Token relatedToken) : base(relatedToken)
      {
        kind = KindEnum.Class;
      }

      public TypeCollection BaseClasses
      {
        [DebuggerStepThrough]
        get { if (baseClasses == null) baseClasses = new TypeCollection(); return baseClasses; }
      }

      public NodeCollection<EnumNode> Enums
        {
            [DebuggerStepThrough]
            get { return enums; }
            [DebuggerStepThrough]
            set { enums = value; }
        }

      public NodeCollection<ClassNode> Classes
        {
            [DebuggerStepThrough]
            get { return classes; }
            [DebuggerStepThrough]
            set { classes = value; }
        }

      public NodeCollection<DelegateNode> Delegates
        {
            [DebuggerStepThrough]
            get { return delegates; }
            [DebuggerStepThrough]
            set { delegates = value; }
        }

      public NodeCollection<InterfaceNode> Interfaces
        {
            [DebuggerStepThrough]
            get { return interfaces; }
            [DebuggerStepThrough]
            set { interfaces = value; }
        }

      public NodeCollection<StructNode> Structs
        {
            [DebuggerStepThrough]
            get { return structs; }
            [DebuggerStepThrough]
            set { structs = value; }
        }

      public NodeCollection<ConstantNode> Constants
		{
            [DebuggerStepThrough]
            get { if (constants == null) constants = new NodeCollection<ConstantNode>(); return constants; }
		}

      public NodeCollection<FieldNode> Fields
		{
            [DebuggerStepThrough]
            get { if (fields == null) fields = new NodeCollection<FieldNode>(); return fields; }
		}

      public NodeCollection<PropertyNode> Properties
		{
            [DebuggerStepThrough]
            get { if (properties == null) properties = new NodeCollection<PropertyNode>(); return properties; }
            [DebuggerStepThrough]
            set { properties = value; }
		}

      public NodeCollection<ConstructorNode> Constructors
		{
            [DebuggerStepThrough]
            get { if (constructors == null) constructors = new NodeCollection<ConstructorNode>(); return constructors; }
            [DebuggerStepThrough]
            set { constructors = value; }
		}

      public NodeCollection<DestructorNode> Destructors
		{
            [DebuggerStepThrough]
            get { if (destructors == null) destructors = new NodeCollection<DestructorNode>(); return destructors; }
            [DebuggerStepThrough]
            set { destructors = value; }
		}

      public NodeCollection<MethodNode> Methods
		{
            [DebuggerStepThrough]
            get { if (methods == null) methods = new NodeCollection<MethodNode>(); return methods; }
            [DebuggerStepThrough]
            set { methods = value; }
		}

      public NodeCollection<OperatorNode> Operators
        {
            [DebuggerStepThrough]
            get { if (operators == null) operators = new NodeCollection<OperatorNode>(); return operators; }
            [DebuggerStepThrough]
            set { operators = value; }
        }

      public NodeCollection<IndexerNode> Indexers
		{
            [DebuggerStepThrough]
            get { if (indexers == null) indexers = new NodeCollection<IndexerNode>(); return indexers; }
            [DebuggerStepThrough]
            set { indexers = value; }
		}

      public NodeCollection<EventNode> Events
		{
            [DebuggerStepThrough]
            get { if (events == null) events = new NodeCollection<EventNode>(); return events; }
            [DebuggerStepThrough]
            set { events = value; }
		}

      public NodeCollection<FixedBufferNode> FixedBuffers
        {
            [DebuggerStepThrough]
            get
            {
                return fixedBuffers;
            }
            [DebuggerStepThrough]
            set
            {
                fixedBuffers = value;
            }
        }

		//private NodeCollection<FieldNode> members;
		//public NodeCollection<FieldNode> Members
		//{
		//    get { return members; }
		//    set { members = value; }
        //}

      public override void ToSource(StringBuilder sb)
		{
            sb.Append(IdentNewLine(DocComment));

            if (attributes != null)
                attributes.ToSource(sb);

            TraceModifiers(modifiers, sb);

            if (IsPartial)
            {
                sb.Append("partial ");
            }

            sb.Append(kind.ToString().ToLower() + " ");
            name.ToSource(sb);

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

            if (baseClasses != null && baseClasses.Count > 0)
            {
                sb.Append(" : ");
                baseClasses.ToSource(sb);
            }

			if(IsGeneric)
                Generic.ConstraintsToSource(sb);

			NewLine(sb);
            sb.Append("{");
            indent++;
            NewLine(sb);

            if (interfaces != null)
                interfaces.ToSource(sb);

            if (structs != null)
                structs.ToSource(sb);

            if (classes != null)
                classes.ToSource(sb);

            if (delegates != null)
                delegates.ToSource(sb);

            if (enums != null)
                enums.ToSource(sb);

            if (constants != null)
            {
                constants.ToSource(sb);
            }
            if (fields != null)
            {
                fields.ToSource(sb);
            }
            if (properties != null)
            {
                properties.ToSource(sb);
            }
            if (constructors != null)
            {
                constructors.ToSource(sb);
            }
            if (methods != null)
            {
                methods.ToSource(sb);
            }

            if (operators != null)
            {
                operators.ToSource(sb);
            }

            if (indexers != null)
            {
                indexers.ToSource(sb);
            }
            if (events != null)
            {
                events.ToSource(sb);
            }

            if (fixedBuffers != null && fixedBuffers.Count > 0)
            {
                fixedBuffers.ToSource(sb);
            }

            if (destructors != null)
            {
                destructors.ToSource(sb);
            }

            indent--;
            NewLine(sb);
            sb.Append("}");
            NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (canEnterContext)
                resolver.Context.Enter(name.Identifier, false);

            for (int i = 0; i < BaseClasses.Count; i++)
            {
                if (BaseClasses[i] is TypeNode &&
                    !(BaseClasses[i] is IdentifiedTypeNode))
                {
                    IdentifiedTypeNode node = resolver.IdentifyType((TypeNode)BaseClasses[i]);

                    if (node != null)
                        BaseClasses[i] = node;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            foreach (EnumNode node in Enums)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (ClassNode node in Classes)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (DelegateNode node in Delegates)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (InterfaceNode node in Interfaces)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (StructNode node in Structs)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (ConstantNode node in Constants)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (FieldNode node in Fields)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (PropertyNode node in Properties)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }


            foreach (ConstructorNode node in Constructors)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (DestructorNode node in Destructors)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (MethodNode node in Methods)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (OperatorNode node in Operators)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (IndexerNode node in Indexers)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (EventNode node in Events)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (FixedBufferNode node in FixedBuffers)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            if (canEnterContext)
                resolver.Context.Leave();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitClassDeclaration(this, data);
        }
	}
}
