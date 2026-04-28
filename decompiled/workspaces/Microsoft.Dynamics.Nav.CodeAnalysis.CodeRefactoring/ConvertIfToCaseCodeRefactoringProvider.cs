using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

[CodeRefactoringProvider("IfToCase")]
public class ConvertIfToCaseCodeRefactoringProvider : CodeRefactoringProvider
{
	private sealed class Analyser
	{
		private const int MinNumberOfSubsequentIfStatementsToConvert = 2;

		private CodeExpressionSyntax caseExpression;

		private StatementSyntax caseDefaultBody;

		public void ComputeRefactorings(Document document, SyntaxNode root, IfStatementSyntax ifStatement, CodeRefactoringContext context)
		{
			Document document2 = document;
			SyntaxNode root2 = root;
			IfStatementSyntax ifStatement2 = ifStatement;
			List<(CodeExpressionSyntax, StatementSyntax)> caseSections = GetCaseSections(ifStatement2);
			if (caseSections != null && caseSections.Count >= 2)
			{
				context.RegisterRefactoring(new RefactorCodeAction(WorkspacesResources.ConvertToCase, (CancellationToken c) => UpdateDocumentAsync(document2, root2, ifStatement2, caseSections, context.CancellationToken)));
			}
		}

		private List<(CodeExpressionSyntax label, StatementSyntax statement)> GetCaseSections(IfStatementSyntax ifStatement)
		{
			List<(CodeExpressionSyntax, StatementSyntax)> list = new List<(CodeExpressionSyntax, StatementSyntax)>();
			foreach (var (codeExpressionSyntax, syntax) in GetIfElseStatementChain(ifStatement))
			{
				if (codeExpressionSyntax == null)
				{
					caseDefaultBody = syntax.WithoutTrivia();
					break;
				}
				CodeExpressionSyntax codeExpressionSyntax2 = CreateCaseLabelFromExpression(codeExpressionSyntax);
				if (codeExpressionSyntax2 == null)
				{
					return null;
				}
				list.Add((codeExpressionSyntax2, syntax.WithoutTrivia()));
			}
			return list;
		}

		private IEnumerable<(CodeExpressionSyntax, StatementSyntax)> GetIfElseStatementChain(IfStatementSyntax currentStatement)
		{
			StatementSyntax elseStatement;
			do
			{
				yield return (currentStatement.Condition, currentStatement.Statement);
				elseStatement = currentStatement.ElseStatement;
				currentStatement = elseStatement as IfStatementSyntax;
			}
			while (currentStatement != null);
			if (elseStatement != null)
			{
				yield return (null, elseStatement);
			}
		}

		private CodeExpressionSyntax CreateCaseLabelFromExpression(CodeExpressionSyntax operand)
		{
			if (operand.Kind != SyntaxKind.EqualsExpression)
			{
				return null;
			}
			BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)operand;
			if (!TryDetermineConstant(binaryExpressionSyntax.Right, binaryExpressionSyntax.Left, out CodeExpressionSyntax constant, out CodeExpressionSyntax expression))
			{
				return null;
			}
			if (!SetInitialOrIsEquivalentToCaseExpression(expression))
			{
				return null;
			}
			return constant.WithoutTrivia();
		}

		private bool TryDetermineConstant(CodeExpressionSyntax expression1, CodeExpressionSyntax expression2, out CodeExpressionSyntax constant, out CodeExpressionSyntax expression)
		{
			if (!IsConstant(expression1))
			{
				(constant, expression) = (IsConstant(expression2) ? (expression2, expression1) : (null, null));
			}
			else
			{
				constant = expression1;
				expression = expression2;
			}
			return constant != null;
		}

		private bool IsConstant(ExpressionSyntax node)
		{
			switch (node.Kind)
			{
			case SyntaxKind.Int32LiteralToken:
			case SyntaxKind.Int64LiteralToken:
			case SyntaxKind.DecimalLiteralToken:
			case SyntaxKind.DateLiteralToken:
			case SyntaxKind.TimeLiteralToken:
			case SyntaxKind.DateTimeLiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.OptionAccessExpression:
			case SyntaxKind.LiteralExpression:
				return true;
			default:
				return false;
			}
		}

		private bool SetInitialOrIsEquivalentToCaseExpression(CodeExpressionSyntax expression)
		{
			if (caseExpression == null)
			{
				caseExpression = expression.WithoutTrivia();
				return true;
			}
			return SyntaxFactory.AreEquivalent(caseExpression, expression);
		}

		private Task<Document> UpdateDocumentAsync(Document document, SyntaxNode root, IfStatementSyntax ifStatement, IEnumerable<(CodeExpressionSyntax label, StatementSyntax statement)> sections, CancellationToken cancellationToken)
		{
			IEnumerable<CaseLineSyntax> caseLines = sections.Select<(CodeExpressionSyntax, StatementSyntax), CaseLineSyntax>(delegate((CodeExpressionSyntax label, StatementSyntax statement) s)
			{
				SeparatedSyntaxList<CodeExpressionSyntax> expressions = new SeparatedSyntaxList<CodeExpressionSyntax>(s.label, 0);
				SyntaxToken colonToken = SyntaxFactory.Token(SyntaxKind.ColonToken).WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed);
				StatementSyntax statement = CreateCaseLineBody(s.statement);
				return SyntaxFactory.CaseLine(expressions, colonToken, statement).WithoutLeadingTrivia().WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed);
			});
			CaseElseSyntax caseElse = ((caseDefaultBody == null) ? null : SyntaxFactory.CaseElse(SyntaxFactory.Token(SyntaxKind.ElseKeyword).WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed), new SyntaxList<StatementSyntax>(caseDefaultBody)).WithoutLeadingTrivia().WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed));
			SyntaxNode syntaxNode = CreateCaseStatement(ifStatement, caseLines, caseElse).WithLeadingTrivia(ifStatement.GetLeadingTrivia()).WithTrailingTrivia(ifStatement.GetTrailingTrivia());
			TextSpan span = ifStatement.Span;
			root = root.ReplaceNode(root.FindNode(span), syntaxNode);
			return Formatter.FormatAsync(document.WithSyntaxRoot(root), new TextSpan(span.Start, syntaxNode.FullWidth), document.Project.Solution.Workspace.Options, cancellationToken);
		}

		private StatementSyntax CreateCaseLineBody(StatementSyntax section)
		{
			if (section.GetLastToken().Kind == SyntaxKind.SemicolonToken)
			{
				return section;
			}
			return section.UpdateWithSemicolon();
		}

		private SyntaxNode CreateCaseStatement(IfStatementSyntax ifStatement, IEnumerable<CaseLineSyntax> caseLines, CaseElseSyntax caseElse)
		{
			return SyntaxFactory.CaseStatement(SyntaxFactory.Token(SyntaxKind.CaseKeyword), caseExpression, SyntaxFactory.Token(SyntaxKind.OfKeyword).WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed), new SyntaxList<CaseLineSyntax>(caseLines.First()).AddRange(caseLines.Skip(1)), caseElse, SyntaxFactory.Token(SyntaxKind.EndKeyword).WithoutTrivia(), SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithoutTrivia());
		}
	}

	public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		IfStatementSyntax ifStatementSyntax = syntaxNode.FindNode(context.Span).FirstAncestorOrSelf<IfStatementSyntax>();
		if (ifStatementSyntax != null && ifStatementSyntax.Condition.Kind == SyntaxKind.EqualsExpression && !ifStatementSyntax.ContainsDiagnostics && ifStatementSyntax.FullSpan.Contains(context.Span))
		{
			CreateAnalyser().ComputeRefactorings(document, syntaxNode, ifStatementSyntax, context);
		}
	}

	private Analyser CreateAnalyser()
	{
		return new Analyser();
	}
}
