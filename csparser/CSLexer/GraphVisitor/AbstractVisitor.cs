using System.Collections.Generic;
using DDW.Collections;

namespace DDW
{
  public abstract class AbstractVisitor
  {

    protected Stack<object> stackMap = new Stack<object>();

    #region primitives expression

    public virtual object VisitBooleanPrimitive(BooleanPrimitive booleanPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitCharPrimitive(CharPrimitive charPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitDecimalPrimitive(DecimalPrimitive decimalPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitIntegralPrimitive(IntegralPrimitive integralPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitNullPrimitive(NullPrimitive nullPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitRealPrimitive(RealPrimitive realPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitStringPrimitive(StringPrimitive stringPrimitive, object data)
    {
      return null;
    }

    public virtual object VisitStructNode(StructNode structNode, object data)
    {
      stackMap.Push(structNode);

      structNode.Attributes.AcceptVisitor(this, data);

      structNode.BaseClasses.AcceptVisitor(this, data);

      structNode.Classes.AcceptVisitor(this, data);

      structNode.Constants.AcceptVisitor(this, data);

      structNode.Constructors.AcceptVisitor(this, data);

      structNode.Delegates.AcceptVisitor(this, data);

      structNode.Destructors.AcceptVisitor(this, data);

      structNode.Enums.AcceptVisitor(this, data);

      structNode.Events.AcceptVisitor(this, data);

      structNode.Fields.AcceptVisitor(this, data);

      structNode.FixedBuffers.AcceptVisitor(this, data);

      if (structNode.Generic != null)
      {
        structNode.Generic.AcceptVisitor(this, data);
      }

      structNode.Indexers.AcceptVisitor(this, data);

      structNode.Interfaces.AcceptVisitor(this, data);

      structNode.Methods.AcceptVisitor(this, data);

      structNode.Operators.AcceptVisitor(this, data);

      if (structNode.Partials != null)
      {
        structNode.Partials.AcceptVisitor(this, data);
      }

      structNode.Properties.AcceptVisitor(this, data);

      structNode.Structs.AcceptVisitor(this, data);

      stackMap.Pop();

      return null;
    }



    public virtual object VisitVoidPrimitive(VoidPrimitive voidPrimitive, object data)
    {
      return null;
    }
    #endregion

    public virtual object VisitConstantNode(ConstantNode constantNode, object data)
    {
      stackMap.Push(constantNode);
      constantNode.Attributes.AcceptVisitor(this, data);
      constantNode.Type.AcceptVisitor(this, data);
      constantNode.Value.AcceptVisitor(this, data);
      stackMap.Pop();

      return null;
    }


    public virtual object VisitArgumentNode(ArgumentNode argumentNode, object data)
    {
      stackMap.Push(argumentNode);
      argumentNode.Attributes.AcceptVisitor(this, data);
      argumentNode.Expression.AcceptVisitor(this, data);
      stackMap.Pop();
      return null;
    }

    public virtual object VisitFixedBufferNode(FixedBufferNode fixedBufferNode, object data)
    {
      stackMap.Push(fixedBufferNode);
      fixedBufferNode.Attributes.AcceptVisitor(this, data);
      fixedBufferNode.FixedBufferConstants.AcceptVisitor(this, data);
      fixedBufferNode.Type.AcceptVisitor(this, data);

      if (fixedBufferNode.Value != null)
      {
        fixedBufferNode.Value.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitInterfaceEventNode(InterfaceEventNode interfaceEventNode, object data)
    {
      stackMap.Push(interfaceEventNode);
      interfaceEventNode.Attributes.AcceptVisitor(this, data);
      interfaceEventNode.Type.AcceptVisitor(this, data);
      stackMap.Pop();
      return null;

    }

    public virtual object VisitInterfaceIndexerNode(InterfaceIndexerNode interfaceIndexerNode, object data)
    {
      stackMap.Push(interfaceIndexerNode);
      interfaceIndexerNode.Attributes.AcceptVisitor(this, data);
      interfaceIndexerNode.Params.AcceptVisitor(this, data);
      stackMap.Pop();
      return null;

    }

    public virtual object VisitInterfaceMethodNode(InterfaceMethodNode interfaceMethodNode, object data)
    {
      stackMap.Push(interfaceMethodNode);
      interfaceMethodNode.Attributes.AcceptVisitor(this, data);

      if (interfaceMethodNode.Generic != null)
      {
        interfaceMethodNode.Generic.AcceptVisitor(this, data);
      }

      interfaceMethodNode.Params.AcceptVisitor(this, data);
      interfaceMethodNode.Type.AcceptVisitor(this, data);
      stackMap.Pop();
      return null;

    }

    public virtual object VisitInterfacePropertyNode(InterfacePropertyNode interfacePropertyNode, object data)
    {
      stackMap.Push(interfacePropertyNode);
      interfacePropertyNode.Attributes.AcceptVisitor(this, data);

      interfacePropertyNode.Type.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAttributeArgumentNode(AttributeArgumentNode attributeArgumentNode, object data)
    {
      stackMap.Push(attributeArgumentNode);
      attributeArgumentNode.Attributes.AcceptVisitor(this, data);

      if (attributeArgumentNode.ArgumentName != null)
      {
        attributeArgumentNode.ArgumentName.AcceptVisitor(this, data);
      }

      if (attributeArgumentNode.Expression != null)
      {
        attributeArgumentNode.Expression.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }



    public virtual object VisitDereferenceExpression(DereferenceExpression dereferenceExpression, object data)
    {
      stackMap.Push(dereferenceExpression);
      dereferenceExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitElementAccessExpression(ElementAccessExpression elementAccessExpression, object data)
    {
      stackMap.Push(elementAccessExpression);
      elementAccessExpression.LeftSide.AcceptVisitor(this, data);

      elementAccessExpression.Expressions.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAddressOutNode(OutNode outNode, object data)
    {
      stackMap.Push(outNode);
      outNode.VariableReference.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitPostDecrementExpression(PostDecrementExpression postDecrementExpression, object data)
    {
      stackMap.Push(postDecrementExpression);
      postDecrementExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitPostIncrementExpression(PostIncrementExpression postIncrementExpression, object data)
    {
      stackMap.Push(postIncrementExpression);
      postIncrementExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }


    public virtual object VisitQualifiedIdentifierExpression(QualifiedIdentifierExpression qualifiedIdentifierExpression, object data)
    {
      stackMap.Push(qualifiedIdentifierExpression);
      qualifiedIdentifierExpression.Expressions.AcceptVisitor(this, data);

      if (qualifiedIdentifierExpression.Generic != null)
      {
        qualifiedIdentifierExpression.Generic.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitRefNode(RefNode refNode, object data)
    {
      stackMap.Push(refNode);
      refNode.VariableReference.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitExternAliasDirectiveNode(ExternAliasDirectiveNode externAliasDirectiveNode, object data)
    {
      stackMap.Push(externAliasDirectiveNode);
      externAliasDirectiveNode.Attributes.AcceptVisitor(this, data);
      externAliasDirectiveNode.ExternAliasName.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAccessorNode(AccessorNode accessorNode, object data)
    {
      stackMap.Push(accessorNode);
      accessorNode.Attributes.AcceptVisitor(this, data);

      accessorNode.StatementBlock.AcceptVisitor(this, data);
      stackMap.Pop();
      return null;


    }

    public virtual object VisitAddressOfExpression(AddressOfExpression addressOfExpression, object data)
    {
      stackMap.Push(addressOfExpression);
      addressOfExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAnonymousMethodExpression(AnonymousMethodNode anonymousMethodExpression, object data)
    {
      stackMap.Push(anonymousMethodExpression);

      if (anonymousMethodExpression.Parameters != null)
        anonymousMethodExpression.Parameters.AcceptVisitor(this, data);
      anonymousMethodExpression.StatementBlock.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitArrayCreateExpression(ArrayCreationExpression arrayCreateExpression, object data)
    {
      stackMap.Push(arrayCreateExpression);
      arrayCreateExpression.Type.AcceptVisitor(this, data);

      if (arrayCreateExpression.Initializer != null)
      {
        arrayCreateExpression.Initializer.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression, object data)
    {
      stackMap.Push(arrayInitializerExpression);
      arrayInitializerExpression.Expressions.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAssignmentExpression(AssignmentExpression assignmentExpression, object data)
    {
      stackMap.Push(assignmentExpression);
      assignmentExpression.Variable.AcceptVisitor(this, data);

      assignmentExpression.RightSide.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitAttribute(AttributeNode attribute, object data)
    {
      stackMap.Push(attribute);
      attribute.Arguments.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitBaseReferenceExpression(BaseAccessExpression baseAccessExpression, object data)
    {
      stackMap.Push(baseAccessExpression);
      baseAccessExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitBinaryOperatorExpression(BinaryExpression binaryOperatorExpression, object data)
    {
      stackMap.Push(binaryOperatorExpression);
      binaryOperatorExpression.Left.AcceptVisitor(this, data);
      binaryOperatorExpression.Right.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitBlockStatement(BlockStatement blockStatement, object data)
    {
      stackMap.Push(blockStatement);
      blockStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitBreakStatement(BreakStatement breakStatement, object data)
    {
      return null;
    }

    public virtual object VisitCaseLabel(CaseNode caseLabel, object data)
    {
      stackMap.Push(caseLabel);
      caseLabel.Ranges.AcceptVisitor(this, data);

      caseLabel.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitCastExpression(UnaryCastExpression castExpression, object data)
    {
      stackMap.Push(castExpression);
      castExpression.Child.AcceptVisitor(this, data);

      castExpression.Type.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitCatchClause(CatchNode catchClause, object data)
    {
      stackMap.Push(catchClause);
      if (catchClause.ClassType != null)
      {
        catchClause.ClassType.AcceptVisitor(this, data);
      }

      catchClause.CatchBlock.AcceptVisitor(this, data);

      if (catchClause.Identifier != null)
      {
        catchClause.Identifier.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitCheckedExpression(CheckedExpression checkedExpression, object data)
    {
      stackMap.Push(checkedExpression);
      checkedExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitCheckedStatement(CheckedStatement checkedStatement, object data)
    {
      stackMap.Push(checkedStatement);
      if (checkedStatement.CheckedExpression != null)
      {
        checkedStatement.CheckedExpression.AcceptVisitor(this, data);
      }

      if (checkedStatement.CheckedBlock != null)
      {
        checkedStatement.CheckedBlock.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;
    }

    public virtual object VisitCompilationUnit(CompilationUnitNode compilationUnit, object data)
    {
      stackMap.Push(compilationUnit);
      compilationUnit.Attributes.AcceptVisitor(this, data);

      compilationUnit.DefaultNamespace.AcceptVisitor(this, data);


      compilationUnit.Namespaces.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitConditionalExpression(ConditionalExpression conditionalExpression, object data)
    {
      stackMap.Push(conditionalExpression);
      conditionalExpression.Test.AcceptVisitor(this, data);
      conditionalExpression.Left.AcceptVisitor(this, data);
      conditionalExpression.Right.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitConstantExpressions(ConstantExpression constantExpression, object data)
    {
      stackMap.Push(constantExpression);
      constantExpression.Value.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitConstructorDeclaration(ConstructorNode constructorDeclaration, object data)
    {
      stackMap.Push(constructorDeclaration);
      constructorDeclaration.Attributes.AcceptVisitor(this, data);

      constructorDeclaration.Params.AcceptVisitor(this, data);

      if (constructorDeclaration.ThisBaseArgs != null)
      {
        constructorDeclaration.ThisBaseArgs.AcceptVisitor(this, data);
      }

      constructorDeclaration.StatementBlock.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitContinueStatement(ContinueStatement continueStatement, object data)
    {
      return null;
    }

    public virtual object VisitCommentStatement(CommentStatement commentStatement, object data)
    {
      return null;
    }

    public virtual object VisitDefaultValueExpression(DefaultConstantExpression defaultConstantExpression, object data)
    {
      stackMap.Push(defaultConstantExpression);
      defaultConstantExpression.Type.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitDelegateDeclaration(DelegateNode delegateDeclaration, object data)
    {
      stackMap.Push(delegateDeclaration);
      delegateDeclaration.Attributes.AcceptVisitor(this, data);

      delegateDeclaration.Type.AcceptVisitor(this, data);

      delegateDeclaration.Params.AcceptVisitor(this, data);

      if (delegateDeclaration.Generic != null)
      {
        delegateDeclaration.Generic.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;
    }

    public virtual object VisitDestructorDeclaration(DestructorNode destructorDeclaration, object data)
    {
      stackMap.Push(destructorDeclaration);
      destructorDeclaration.Attributes.AcceptVisitor(this, data);

      destructorDeclaration.StatementBlock.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitDoLoopStatement(DoStatement doLoopStatement, object data)
    {
      stackMap.Push(doLoopStatement);
      doLoopStatement.Test.AcceptVisitor(this, data);

      doLoopStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }
    public virtual object VisitWhileStatement(WhileStatement whileStatement, object data)
    {
      stackMap.Push(whileStatement);
      whileStatement.Test.AcceptVisitor(this, data);

      whileStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitIfStatement(IfStatement ifStatement, object data)
    {
      stackMap.Push(ifStatement);
      ifStatement.Test.AcceptVisitor(this, data);

      ifStatement.ElseStatements.AcceptVisitor(this, data);

      ifStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitEventDeclaration(EventNode eventDeclaration, object data)
    {
      stackMap.Push(eventDeclaration);
      eventDeclaration.Attributes.AcceptVisitor(this, data);

      if (eventDeclaration.AddBlock != null)
      {
        eventDeclaration.AddBlock.AcceptVisitor(this, data);
      }

      if (eventDeclaration.RemoveBlock != null)
      {
        eventDeclaration.RemoveBlock.AcceptVisitor(this, data);
      }

      eventDeclaration.Type.AcceptVisitor(this, data);

      if (eventDeclaration.Value != null)
      {
        eventDeclaration.Value.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitExpressionStatement(ExpressionStatement expressionStatement, object data)
    {
      stackMap.Push(expressionStatement);
      expressionStatement.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitFieldDeclaration(FieldNode fieldDeclaration, object data)
    {
      stackMap.Push(fieldDeclaration);
      fieldDeclaration.Attributes.AcceptVisitor(this, data);

      fieldDeclaration.Type.AcceptVisitor(this, data);

      if (fieldDeclaration.Value != null)
      {
        fieldDeclaration.Value.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitMemberAccessExpression(MemberAccessExpression memberAccessExpression, object data)
    {
      stackMap.Push(memberAccessExpression);
      memberAccessExpression.Left.AcceptVisitor(this, data);
      memberAccessExpression.Right.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitFixedDeclarationStatement(FixedDeclarationsStatement fixedDeclarationsStatement, object data)
    {
      stackMap.Push(fixedDeclarationsStatement);
      fixedDeclarationsStatement.Attributes.AcceptVisitor(this, data);

      fixedDeclarationsStatement.Type.AcceptVisitor(this, data);
      fixedDeclarationsStatement.Declarators.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitFixedStatement(FixedStatementStatement fixedStatement, object data)
    {
      stackMap.Push(fixedStatement);
      fixedStatement.LocalPointers.AcceptVisitor(this, data);
      fixedStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitForeachStatement(ForEachStatement foreachStatement, object data)
    {
      stackMap.Push(foreachStatement);

      foreachStatement.Iterator.AcceptVisitor(this, data);
      foreachStatement.Collection.AcceptVisitor(this, data);
      foreachStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitForStatement(ForStatement forStatement, object data)
    {
      stackMap.Push(forStatement);

      if (forStatement.Init != null)
        forStatement.Init.AcceptVisitor(this, data);

      if (forStatement.Test != null)
        forStatement.Test.AcceptVisitor(this, data);

      if (forStatement.Inc != null)
        forStatement.Inc.AcceptVisitor(this, data);

      forStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitGotoStatement(GotoStatement gotoStatement, object data)
    {
      stackMap.Push(gotoStatement);
      if (gotoStatement.Target != null)
      {
        gotoStatement.Target.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitIdentifierExpression(IdentifierExpression identifierExpression, object data)
    {
      return null;
    }

    public virtual object VisitIndexerDeclaration(IndexerNode indexerDeclaration, object data)
    {
      stackMap.Push(indexerDeclaration);
      indexerDeclaration.Attributes.AcceptVisitor(this, data);

      indexerDeclaration.Params.AcceptVisitor(this, data);

      indexerDeclaration.Type.AcceptVisitor(this, data);

      if (indexerDeclaration.Getter != null)
      {
        indexerDeclaration.Getter.AcceptVisitor(this, data);
      }

      if (indexerDeclaration.Setter != null)
      {
        indexerDeclaration.Setter.AcceptVisitor(this, data);
      }

      if (indexerDeclaration.InterfaceType != null)
      {
        indexerDeclaration.InterfaceType.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitInvocationExpression(InvocationExpression invocationExpression, object data)
    {
      stackMap.Push(invocationExpression);
      invocationExpression.LeftSide.AcceptVisitor(this, data);
      invocationExpression.ArgumentList.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitLabelStatement(LabeledStatement labelStatement, object data)
    {
      stackMap.Push(labelStatement);
      labelStatement.Statement.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitLocalDeclaration(LocalDeclaration localDeclaration, object data)
    {
      stackMap.Push(localDeclaration);
      localDeclaration.Attributes.AcceptVisitor(this, data);

      localDeclaration.Type.AcceptVisitor(this, data);
      localDeclaration.Declarators.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }

    public virtual object VisitLocalDeclarationStatement(LocalDeclarationStatement localVariableDeclaration, object data)
    {
      stackMap.Push(localVariableDeclaration);
      localVariableDeclaration.Attributes.AcceptVisitor(this, data);

      localVariableDeclaration.Type.AcceptVisitor(this, data);
      localVariableDeclaration.Declarators.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitLockStatement(LockStatement lockStatement, object data)
    {
      stackMap.Push(lockStatement);
      lockStatement.Target.AcceptVisitor(this, data);
      lockStatement.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitMethodDeclaration(MethodNode methodDeclaration, object data)
    {
      stackMap.Push(methodDeclaration);
      methodDeclaration.Attributes.AcceptVisitor(this, data);

      methodDeclaration.Params.AcceptVisitor(this, data);

      if (methodDeclaration.Generic != null)
      {
        methodDeclaration.Generic.AcceptVisitor(this, data);
      }

      methodDeclaration.Type.AcceptVisitor(this, data);

      methodDeclaration.StatementBlock.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitNamespaceDeclaration(NamespaceNode namespaceDeclaration, object data)
    {
      stackMap.Push(namespaceDeclaration);
      namespaceDeclaration.Attributes.AcceptVisitor(this, data);

      namespaceDeclaration.Classes.AcceptVisitor(this, data);

      namespaceDeclaration.Structs.AcceptVisitor(this, data);

      namespaceDeclaration.Delegates.AcceptVisitor(this, data);

      namespaceDeclaration.Enums.AcceptVisitor(this, data);

      namespaceDeclaration.Interfaces.AcceptVisitor(this, data);

      namespaceDeclaration.ExternAliases.AcceptVisitor(this, data);

      namespaceDeclaration.Namespaces.AcceptVisitor(this, data);

      namespaceDeclaration.UsingDirectives.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitObjectCreateExpression(ObjectCreationExpression objectCreateExpression, object data)
    {
      stackMap.Push(objectCreateExpression);
      objectCreateExpression.Type.AcceptVisitor(this, data);
      objectCreateExpression.ArgumentList.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitOperatorDeclaration(OperatorNode operatorDeclaration, object data)
    {
      stackMap.Push(operatorDeclaration);
      operatorDeclaration.Attributes.AcceptVisitor(this, data);



      if (operatorDeclaration.Param1 != null)
      {
        operatorDeclaration.Param1.AcceptVisitor(this, data);
      }

      if (operatorDeclaration.Param2 != null)
      {
        operatorDeclaration.Param2.AcceptVisitor(this, data);
      }

      operatorDeclaration.Type.AcceptVisitor(this, data);

      operatorDeclaration.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitParameterDeclarationExpression(ParamDeclNode parameterDeclarationExpression, object data)
    {
      stackMap.Push(parameterDeclarationExpression);
      parameterDeclarationExpression.Attributes.AcceptVisitor(this, data);

      parameterDeclarationExpression.Type.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, object data)
    {
      stackMap.Push(parenthesizedExpression);
      parenthesizedExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitPrimaryExpression(PrimaryExpression primitiveExpression, object data)
    {
      return null;
    }

    public virtual object VisitPropertyDeclaration(PropertyNode propertyDeclaration, object data)
    {
      stackMap.Push(propertyDeclaration);
      propertyDeclaration.Attributes.AcceptVisitor(this, data);

      propertyDeclaration.Type.AcceptVisitor(this, data);

      if (propertyDeclaration.Getter != null)
      {
        propertyDeclaration.Getter.AcceptVisitor(this, data);
      }

      if (propertyDeclaration.Setter != null)
      {
        propertyDeclaration.Setter.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitReturnStatement(ReturnStatement returnStatement, object data)
    {
      stackMap.Push(returnStatement);
      if (returnStatement.ReturnValue != null)
      {
        return returnStatement.ReturnValue.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitSizeOfExpression(SizeOfExpression sizeOfExpression, object data)
    {
      stackMap.Push(sizeOfExpression);
      sizeOfExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitStackAllocExpression(StackallocExpression stackAllocExpression, object data)
    {
      stackMap.Push(stackAllocExpression);
      stackAllocExpression.Type.AcceptVisitor(this, data);
      stackAllocExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }


    public virtual object VisitStatementNode(StatementNode statementNode, object data)
    {
      return null;
    }



    public virtual object VisitSwitchStatement(SwitchStatement switchStatement, object data)
    {
      stackMap.Push(switchStatement);
      switchStatement.Test.AcceptVisitor(this, data);

      switchStatement.Cases.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitGenericDefinition(GenericNode genericNode, object data)
    {
      stackMap.Push(genericNode);
      genericNode.Constraints.AcceptVisitor(this, data);

      genericNode.TypeParameters.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }
    public virtual object VisitTypeParameter(TypeParameterNode typeParameter, object data)
    {
      stackMap.Push(typeParameter);
      typeParameter.Attributes.AcceptVisitor(this, data);

      if (typeParameter.Identifier != null)
      {
        typeParameter.Identifier.AcceptVisitor(this, data);
      }

      if (typeParameter.Type != null)
      {
        typeParameter.Type.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitGenericConstraint(Constraint genericConstraint, object data)
    {
      stackMap.Push(genericConstraint);
      genericConstraint.ConstraintExpressions.AcceptVisitor(this, data);

      if (genericConstraint.ConstructorConstraint != null)
      {
        genericConstraint.ConstructorConstraint.AcceptVisitor(this, data);
      }

      genericConstraint.TypeParameter.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitConstraintExpression(ConstraintExpressionNode constraintExpression, object data)
    {
      stackMap.Push(constraintExpression);
      if (constraintExpression.Expression != null)
      {
        constraintExpression.Expression.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitConstructorConstraint(ConstructorConstraint constructorConstraint, object data)
    {
      return null;
    }

    public virtual object VisitThisReferenceExpression(ThisAccessExpression thisReferenceExpression, object data)
    {
      return null;
    }

    public virtual object VisitThrowStatement(ThrowNode throwStatement, object data)
    {
      stackMap.Push(throwStatement);
      if (throwStatement.ThrowExpression != null)
      {
        throwStatement.ThrowExpression.AcceptVisitor(this, data);
      }
      stackMap.Pop();
      return null;

    }

    public virtual object VisitTryStatement(TryStatement tryStatement, object data)
    {
      stackMap.Push(tryStatement);
      tryStatement.TryBlock.AcceptVisitor(this, data);

      if (tryStatement.CatchBlocks != null)
      {
        tryStatement.CatchBlocks.AcceptVisitor(this, data);
      }

      if (tryStatement.FinallyBlock != null)
      {
        tryStatement.FinallyBlock.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }
    public virtual object VisitFinallyStatement(FinallyNode finallyStatement, object data)
    {
      stackMap.Push(finallyStatement);
      finallyStatement.FinallyBlock.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitClassDeclaration(ClassNode classDeclaration, object data)
    {
      stackMap.Push(classDeclaration);
      classDeclaration.Attributes.AcceptVisitor(this, data);

      classDeclaration.BaseClasses.AcceptVisitor(this, data);

      classDeclaration.Classes.AcceptVisitor(this, data);

      classDeclaration.Constants.AcceptVisitor(this, data);

      classDeclaration.Constructors.AcceptVisitor(this, data);

      classDeclaration.Delegates.AcceptVisitor(this, data);

      classDeclaration.Destructors.AcceptVisitor(this, data);

      classDeclaration.Enums.AcceptVisitor(this, data);

      classDeclaration.Events.AcceptVisitor(this, data);

      classDeclaration.Fields.AcceptVisitor(this, data);

      classDeclaration.FixedBuffers.AcceptVisitor(this, data);

      if (classDeclaration.Generic != null)
      {
        classDeclaration.Generic.AcceptVisitor(this, data);
      }

      classDeclaration.Indexers.AcceptVisitor(this, data);

      classDeclaration.Interfaces.AcceptVisitor(this, data);

      classDeclaration.Methods.AcceptVisitor(this, data);

      classDeclaration.Operators.AcceptVisitor(this, data);

      if (classDeclaration.Partials != null)
      {
        classDeclaration.Partials.AcceptVisitor(this, data);
      }

      classDeclaration.Properties.AcceptVisitor(this, data);

      classDeclaration.Structs.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }
    public virtual object VisitInterfaceDeclaration(InterfaceNode interfaceDeclaration, object data)
    {
      stackMap.Push(interfaceDeclaration);
      interfaceDeclaration.Attributes.AcceptVisitor(this, data);

      interfaceDeclaration.BaseClasses.AcceptVisitor(this, data);

      interfaceDeclaration.Events.AcceptVisitor(this, data);

      if (interfaceDeclaration.Generic != null)
      {
        interfaceDeclaration.Generic.AcceptVisitor(this, data);
      }

      interfaceDeclaration.Indexers.AcceptVisitor(this, data);

      interfaceDeclaration.Methods.AcceptVisitor(this, data);

      if (interfaceDeclaration.Partials != null)
      {
        interfaceDeclaration.Partials.AcceptVisitor(this, data);
      }

      interfaceDeclaration.Properties.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitEnumDeclaration(EnumNode enumDeclaration, object data)
    {
      stackMap.Push(enumDeclaration);
      enumDeclaration.Attributes.AcceptVisitor(this, data);

      if (enumDeclaration.BaseClass != null)
      {
        enumDeclaration.BaseClass.AcceptVisitor(this, data);
      }

      if (enumDeclaration.Value != null)
      {
        if (enumDeclaration.Value is NodeCollection<EnumNode>)
        {
          (enumDeclaration.Value as NodeCollection<EnumNode>).AcceptVisitor(this, data);
        }
        else
        {
          (enumDeclaration.Value as BaseNode).AcceptVisitor(this, data);
        }
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitTypeOfExpression(TypeOfExpression typeOfExpression, object data)
    {
      stackMap.Push(typeOfExpression);
      typeOfExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitTypeReference(TypeNode typeReference, object data)
    {
      stackMap.Push(typeReference);
      if (typeReference.Generic != null)
      {
        typeReference.Generic.AcceptVisitor(this, data);
      }
      typeReference.Identifier.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }
    public virtual object VisitTypePointerReference(TypePointerNode typePointerReference, object data)
    {
      stackMap.Push(typePointerReference);
      typePointerReference.Expression.AcceptVisitor(this, data); ;

      stackMap.Pop();
      return null;

    }

    public virtual object VisitPredefinedTypeReference(PredefinedTypeNode predefinedTypeNode, object data)
    {
      stackMap.Push(predefinedTypeNode);
      if (predefinedTypeNode.Generic != null)
      {
        predefinedTypeNode.Generic.AcceptVisitor(this, data);
      }
      stackMap.Pop();
      return null;

    }

    public virtual object VisitUnaryExpression(UnaryExpression unaryExpression, object data)
    {
      stackMap.Push(unaryExpression);
      unaryExpression.Child.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitUncheckedExpression(UncheckedExpression uncheckedExpression, object data)
    {
      stackMap.Push(uncheckedExpression);
      uncheckedExpression.Expression.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitUncheckedStatement(UncheckedStatement uncheckedStatement, object data)
    {
      stackMap.Push(uncheckedStatement);
      if (uncheckedStatement.UncheckedExpression != null)
      {
        uncheckedStatement.UncheckedExpression.AcceptVisitor(this, data);
      }

      if (uncheckedStatement.UncheckedBlock != null)
      {
        uncheckedStatement.UncheckedBlock.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitUsingDirective(UsingDirectiveNode @using, object data)
    {
      stackMap.Push(@using);
      if (@using.AliasName != null)
      {
        @using.AliasName.AcceptVisitor(this, data);
      }
      @using.Target.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitUsingStatement(UsingStatement usingDeclaration, object data)
    {
      stackMap.Push(usingDeclaration);
      usingDeclaration.Resource.AcceptVisitor(this, data);

      usingDeclaration.Statements.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;

    }

    public virtual object VisitYieldStatement(YieldStatement yieldStatement, object data)
    {
      stackMap.Push(yieldStatement);
      if (yieldStatement.ReturnValue != null)
      {
        yieldStatement.ReturnValue.AcceptVisitor(this, data);
      }

      stackMap.Pop();
      return null;

    }

    public virtual object VisitDeclarator(Declarator declarator, object data)
    {
      stackMap.Push(declarator);

      declarator.Identifier.AcceptVisitor(this, data);
      if (declarator.Initializer != null)
        declarator.Initializer.AcceptVisitor(this, data);

      stackMap.Pop();
      return null;
    }
  }
}
