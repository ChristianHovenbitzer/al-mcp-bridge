using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public abstract class AbstractSyntaxContext
{
	private Binder lazyEnclosingBinder;

	internal Workspace Workspace { get; }

	internal SemanticModel SemanticModel { get; }

	internal SyntaxTree SyntaxTree { get; }

	internal int Position { get; }

	internal SyntaxToken LeftToken { get; }

	internal SyntaxToken TargetToken { get; }

	internal bool IsInNonUserCode { get; }

	internal bool IsRightOfDot { get; }

	internal bool IsRightOfNameSeparator { get; }

	internal bool IsRightOfOptionAccess { get; }

	internal ObjectSyntax DeclaringObject { get; }

	internal bool ShouldAddUsingStatementWhenCompleting => !IsRightOfDot;

	public Version RuntimeVersion { get; }

	internal Binder EnclosingBinder
	{
		get
		{
			if (lazyEnclosingBinder == null)
			{
				if (LeftToken.IsPartOfStructuredTrivia())
				{
					lazyEnclosingBinder = SemanticModel.GetEnclosingBinder(SemanticModel.CheckAndAdjustPosition(LeftToken.SpanStart));
				}
				else
				{
					lazyEnclosingBinder = SemanticModel.GetEnclosingBinder(LeftToken.SpanStart);
				}
			}
			return lazyEnclosingBinder;
		}
	}

	protected AbstractSyntaxContext(Workspace workspace, SemanticModel semanticModel, int position, SyntaxToken leftToken, SyntaxToken targetToken, ObjectSyntax declaringObject, bool isRightOfDot, bool isRightOfColonColon, bool isInNonuserCode)
	{
		Workspace = workspace;
		SemanticModel = semanticModel;
		SyntaxTree = semanticModel.SyntaxTree;
		Position = position;
		LeftToken = leftToken;
		TargetToken = targetToken;
		DeclaringObject = declaringObject;
		RuntimeVersion = SyntaxTree.Options.RuntimeVersion;
		IsRightOfDot = isRightOfDot;
		IsRightOfNameSeparator = isRightOfDot || isRightOfColonColon;
		IsRightOfOptionAccess = isRightOfColonColon;
		IsInNonUserCode = isInNonuserCode;
	}

	internal TService GetLanguageService<TService>() where TService : class, ILanguageService
	{
		return Workspace.Services.GetLanguageServices("AL").GetService<TService>();
	}

	internal TService GetWorkspaceService<TService>() where TService : class, IWorkspaceService
	{
		return Workspace.Services.GetService<TService>();
	}

	internal bool IsArgumentExpression()
	{
		return LeftToken.IsArgumentExpression(Position);
	}

	internal async Task<Tuple<ITypeSymbol, ImmutableArray<ISymbol>>> GetSymbolInfoAtPositionAsync(int position, CancellationToken cancellationToken)
	{
		ISyntaxFactsService syntaxFacts = GetLanguageService<ISyntaxFactsService>();
		SyntaxToken token = await Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.GetTouchingTokenAsync(SyntaxTree, position, syntaxFacts.IsBindableToken, cancellationToken, findInsideTrivia: true).ConfigureAwait(continueOnCapturedContext: false);
		if (!syntaxFacts.IsBindableToken(token))
		{
			return null;
		}
		SyntaxNode bindableParent = syntaxFacts.GetBindableParent(token);
		return new Tuple<ITypeSymbol, ImmutableArray<ISymbol>>(SemanticModel.GetTypeInfo(bindableParent, cancellationToken).Type, SemanticModel.GetSymbolInfo(bindableParent, cancellationToken).GetBestOrAllSymbols().ToImmutableArray());
	}

	internal bool IsCommentContext(int position, CancellationToken cancellationToken)
	{
		if (Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnRightOfPosition(SyntaxTree, position, cancellationToken).GetAllPrecedingTriviaToPreviousToken().IsPositionInCommentTrivia(position))
		{
			return true;
		}
		return SyntaxTree.GetCompilationUnitRoot().GetEndOfFileToken().GetAllPrecedingTriviaToPreviousToken()
			.IsPositionInCommentTrivia(position);
	}
}
