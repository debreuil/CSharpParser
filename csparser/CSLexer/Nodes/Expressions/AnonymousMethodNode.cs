using System.Text;
using DDW.Collections;

namespace DDW
{
	public class AnonymousMethodNode : ExpressionNode
	{
	  private readonly BlockStatement statementBlock;
	  private NodeCollection<ParamDeclNode> parameters;

	  public AnonymousMethodNode(Token relatedtoken)
	    : base(relatedtoken)
	  {
	    statementBlock = new BlockStatement(relatedtoken);
	  }

	  public NodeCollection<ParamDeclNode> Parameters
		{
			get { return parameters; }
			set { parameters = value; }
        }

	  public BlockStatement StatementBlock
		{
			get { return statementBlock; }
		}

		public override void ToSource(StringBuilder sb)
		{			
			sb.Append("delegate ");

			if (parameters != null)
			{
                sb.Append("( ");
				parameters.ToSource(sb, ", ");
                sb.Append(") ");
			}

            indent++;
			NewLine(sb);
			statementBlock.ToSource(sb);
            indent--;
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAnonymousMethodExpression(this, data);
        }
	}
}
