using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

[Serializable]
public class CompletionException : Exception
{
	public static async Task<CompletionException> CreateExceptionAsync(Document document, int position, Exception originalException, CancellationToken cancellationToken)
	{
		TextSpan span = new TextSpan(position, 0);
		SyntaxTree obj = (await document.GetSemanticModelForSpanAsync(span, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.SyntaxTree;
		Location location = obj?.GetLocation(span);
		SyntaxToken? syntaxToken = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(obj?, position, cancellationToken);
		SyntaxToken? rightToken = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnRightOfPosition(obj?, position, cancellationToken);
		SyntaxToken? targetToken = syntaxToken?.GetPreviousTokenIfTouchingWord(position);
		return new CompletionException(document, position, location, syntaxToken, targetToken, rightToken, originalException);
	}

	private CompletionException(Document document, int position, Location? location, SyntaxToken? leftToken, SyntaxToken? targetToken, SyntaxToken? rightToken, Exception originalException)
		: base(CreateExceptionMessage(document, position, location, leftToken, targetToken, rightToken, originalException))
	{
	}

	private static string CreateExceptionMessage(Document document, int position, Location? location, SyntaxToken? leftToken, SyntaxToken? targetToken, SyntaxToken? rightToken, Exception originalException)
	{
		string text = position.ToString();
		if (location != null)
		{
			FileLinePositionSpan lineSpan = location.GetLineSpan();
			text = string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.CompletionExceptionMessage_LineCharacter, lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1);
		}
		return string.Format(CultureInfo.InvariantCulture, CodeAnalysisResources.CompletionExceptionMessage, document.FilePath, text, leftToken?.ToString() ?? CodeAnalysisResources.CompletionExceptionMessage_UnknownToken, targetToken?.ToString() ?? CodeAnalysisResources.CompletionExceptionMessage_UnknownToken, rightToken?.ToString() ?? CodeAnalysisResources.CompletionExceptionMessage_UnknownToken, originalException);
	}
}
