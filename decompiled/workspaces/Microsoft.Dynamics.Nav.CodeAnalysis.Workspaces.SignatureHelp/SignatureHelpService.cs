using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.SignatureHelp;

internal class SignatureHelpService : ISignatureHelpService, ILanguageService
{
	public static readonly int DefaultSelectedSignature;

	public async Task<SignatureHelpResult<SymbolSignature>> ProvideSymbolicHelpAsync(Document document, int position, CancellationToken cancellationToken)
	{
		SyntaxTree tree = await document.GetSyntaxTreeAsync(cancellationToken);
		await tree.GetRootAsync(cancellationToken);
		for (SyntaxNode parent = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(tree, position, cancellationToken).Parent; parent != null; parent = parent.Parent)
		{
			if (parent.IsKind(SyntaxKind.InvocationExpression))
			{
				InvocationExpressionSyntax invocationExpressionSyntax = (InvocationExpressionSyntax)parent;
				TextSpan? textSpan = invocationExpressionSyntax.ArgumentList?.Span;
				if ((!textSpan.HasValue || textSpan.GetValueOrDefault().Start != position) && (textSpan?.Contains(position) ?? false))
				{
					InvocationExpressionSyntax invocation = invocationExpressionSyntax;
					return BuildInvocationSignatures(invocation, await document.GetSemanticModelAsync(cancellationToken), position, cancellationToken);
				}
			}
			if (parent.IsKind(SyntaxKind.MemberAttribute))
			{
				MemberAttributeSyntax memberAttributeSyntax = (MemberAttributeSyntax)parent;
				TextSpan? textSpan2 = memberAttributeSyntax.ArgumentList?.Span;
				if ((!textSpan2.HasValue || textSpan2.GetValueOrDefault().Start != position) && (textSpan2?.Contains(position) ?? false))
				{
					MemberAttributeSyntax attribute = memberAttributeSyntax;
					return BuildAttributeSignatures(attribute, await document.GetSemanticModelAsync(cancellationToken), position);
				}
			}
		}
		return null;
	}

	public async Task<int?> GetActiveParameterAsync(Document document, int position, CancellationToken cancellationToken)
	{
		for (SyntaxNode parent = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(await document.GetSyntaxTreeAsync(cancellationToken), position, cancellationToken).Parent; parent != null; parent = parent.Parent)
		{
			if (parent.IsKind(SyntaxKind.InvocationExpression))
			{
				return GetActiveParameter((InvocationExpressionSyntax)parent, position);
			}
		}
		return null;
	}

	private static int GetActiveParameter(InvocationExpressionSyntax invocation, int position)
	{
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator = invocation.ArgumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Span.Start < position)
			{
				num++;
			}
		}
		return num;
	}

	private static SignatureHelpResult<SymbolSignature> BuildInvocationSignatures(InvocationExpressionSyntax invocation, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
	{
		int activeParameter = GetActiveParameter(invocation, position);
		ArrayBuilder<SymbolSignature> instance = ArrayBuilder<SymbolSignature>.GetInstance();
		ImmutableArray<MethodSymbol>.Enumerator enumerator = OverloadRecommender.GetOverloadsInOrderOfRelevance(invocation, semanticModel, activeParameter, cancellationToken).GetEnumerator();
		while (enumerator.MoveNext())
		{
			IMethodSymbol current = enumerator.Current;
			instance.Add(new SymbolSignature(current, current.Parameters));
		}
		return new SignatureHelpResult<SymbolSignature>(instance.ToArrayAndFree(), DefaultSelectedSignature, activeParameter);
	}

	private static IContainerSymbol GetSymbolThroughWhichMethodIsAccessed(ISymbol target, SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node.Kind == SyntaxKind.MemberAccessExpression)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)node;
			IContainerSymbol containerSymbol = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression, cancellationToken).Symbol?.GetTypeSymbol();
			if (containerSymbol != null)
			{
				return containerSymbol;
			}
		}
		return target.ContainingType;
	}

	private static SignatureHelpResult<SymbolSignature> BuildAttributeSignatures(MemberAttributeSyntax attribute, SemanticModel semanticModel, int position)
	{
		AttributeSymbol attributeSymbol = semanticModel.GetDeclaredSymbol(attribute.ArgumentList.Parent) as AttributeSymbol;
		if (attributeSymbol?.AttributeInfo == null)
		{
			return null;
		}
		ArrayBuilder<SymbolSignature> instance = ArrayBuilder<SymbolSignature>.GetInstance();
		SymbolSignature item = new SymbolSignature(attributeSymbol, attributeSymbol.SignatureArguments);
		instance.Add(item);
		int num = 0;
		using (IEnumerator<SyntaxToken> enumerator = attribute.ArgumentList.Arguments.GetSeparators().GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Span.Start <= position)
			{
				num++;
			}
		}
		return new SignatureHelpResult<SymbolSignature>(instance.ToArrayAndFree(), DefaultSelectedSignature, num);
	}

	public async Task<SignatureHelpResult<SyntaxSignature>> ProvideSyntacticHelpAsync(Document document, int position, CancellationToken cancellationToken)
	{
		SyntaxToken token = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(await document.GetSyntaxTreeAsync(cancellationToken), position, cancellationToken);
		token = token.GetPreviousTokenIfTouchingWord(position);
		if (token.Parent == null)
		{
			return null;
		}
		SyntaxNode parent = token.Parent;
		SignatureHelpResult<SyntaxSignature> signatureHelpResult = ProvideSyntacticalHelp(parent, position);
		if (signatureHelpResult == null && parent.Parent != null)
		{
			signatureHelpResult = ProvideSyntacticalHelp(parent.Parent, position);
			if (signatureHelpResult == null && parent.Parent.IsKind(SyntaxKind.LiteralExpression) && parent.Parent.Parent != null)
			{
				signatureHelpResult = ProvideSyntacticalHelp(parent.Parent.Parent, position);
			}
		}
		return signatureHelpResult;
	}

	private static SignatureHelpResult<SyntaxSignature> ProvideSyntacticalHelp(SyntaxNode node, int position)
	{
		SyntaxNode node2 = node;
		SyntaxNodeSlotDefinition currentDefinition = null;
		InternalSyntaxNode currentNode = null;
		ImmutableArray<SyntaxNodeSlotDefinition> slots = node2.Node.Definition.Slots.Value;
		int slotIndex = 0;
		int currentPosition = node2.SpanStart;
		Func<bool> func = delegate
		{
			if (slotIndex >= slots.Length)
			{
				return false;
			}
			if (currentNode != null && slotIndex > 0)
			{
				currentPosition += currentNode.FullWidth - currentNode.GetLeadingTriviaWidth();
			}
			currentDefinition = slots[slotIndex];
			currentNode = node2.Node.GetSlot(slotIndex);
			int num4 = slotIndex + 1;
			slotIndex = num4;
			return true;
		};
		SyntaxKind? syntaxKind = null;
		while (func())
		{
			if (currentNode == null || currentNode.IsMissing)
			{
				return null;
			}
			if (currentNode.Kind.IsKeyword())
			{
				syntaxKind = currentNode.Kind;
				break;
			}
		}
		if (!syntaxKind.HasValue)
		{
			return null;
		}
		func();
		InternalSyntaxNode internalSyntaxNode = currentNode;
		if (internalSyntaxNode == null || !internalSyntaxNode.IsMissing)
		{
			InternalSyntaxNode internalSyntaxNode2 = currentNode;
			if (internalSyntaxNode2 != null && internalSyntaxNode2.Kind == SyntaxKind.OpenParenToken)
			{
				ArrayBuilder<SyntaxKind> instance = ArrayBuilder<SyntaxKind>.GetInstance();
				ArrayBuilder<SyntaxNodeSlotDefinition> instance2 = ArrayBuilder<SyntaxNodeSlotDefinition>.GetInstance();
				bool flag = false;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				while (func())
				{
					if (currentDefinition.Kinds.Length > 0 && currentDefinition.Kinds[0] == SyntaxKind.CloseParenToken)
					{
						InternalSyntaxNode internalSyntaxNode3 = currentNode;
						if (internalSyntaxNode3 == null || internalSyntaxNode3.IsMissing || currentPosition >= position)
						{
							break;
						}
						return null;
					}
					if (flag)
					{
						DebugAssertHelper.Assert(currentDefinition.Kinds.Length == 1);
						SyntaxKind item = currentDefinition.Kinds.SingleOrDefault();
						instance.Add(item);
						InternalSyntaxNode internalSyntaxNode4 = currentNode;
						if (internalSyntaxNode4 != null && !internalSyntaxNode4.IsMissing)
						{
							num2++;
							if (currentPosition < position)
							{
								num3++;
							}
						}
					}
					else
					{
						instance2.Add(currentDefinition);
						InternalSyntaxNode internalSyntaxNode5 = currentNode;
						if (internalSyntaxNode5 != null && !internalSyntaxNode5.IsMissing)
						{
							num++;
						}
					}
					flag = !flag;
				}
				if (num2 > 0 && num2 == num)
				{
					num++;
				}
				return BuildSyntaxHelp(new SyntaxSignature(node2.Node.Definition, syntaxKind.Value, instance2.ToArrayAndFree(), instance.ToArrayAndFree()), num3, num);
			}
		}
		return null;
	}

	private static SignatureHelpResult<SyntaxSignature> BuildSyntaxHelp(SyntaxSignature fullSignature, int activeParameter, int parametersSpecified)
	{
		IEnumerable<SyntaxSignature> enumerable = BuildSignatureCombinations(fullSignature);
		int num = 0;
		using (IEnumerator<SyntaxSignature> enumerator = enumerable.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Parameters.Count < parametersSpecified)
			{
				num++;
			}
		}
		return new SignatureHelpResult<SyntaxSignature>(enumerable, num, activeParameter);
	}

	private static IEnumerable<SyntaxSignature> BuildSignatureCombinations(SyntaxSignature fullSignature)
	{
		ArrayBuilder<SyntaxSignature> instance = ArrayBuilder<SyntaxSignature>.GetInstance();
		instance.Add(fullSignature);
		for (int i = 0; i < instance.Count; i++)
		{
			SyntaxSignature syntaxSignature = instance[i];
			for (int j = 0; j < syntaxSignature.Parameters.Count; j++)
			{
				if (!syntaxSignature.Parameters[j].IsOptional)
				{
					continue;
				}
				ArrayBuilder<SyntaxNodeSlotDefinition> instance2 = ArrayBuilder<SyntaxNodeSlotDefinition>.GetInstance();
				ArrayBuilder<SyntaxKind> instance3 = ArrayBuilder<SyntaxKind>.GetInstance();
				for (int k = 0; k < syntaxSignature.Parameters.Count; k++)
				{
					if (k != j)
					{
						if (instance2.Count > 0)
						{
							instance3.Add(syntaxSignature.Separators[k - 1]);
						}
						instance2.Add(syntaxSignature.Parameters[k]);
					}
				}
				SyntaxNodeSlotDefinition[] newParameters = instance2.ToArrayAndFree();
				SyntaxKind[] separators = instance3.ToArrayAndFree();
				if (!instance.Any((SyntaxSignature x) => x.Parameters.SequenceEqual(newParameters)))
				{
					instance.Add(new SyntaxSignature(fullSignature.Definition, fullSignature.KeywordKind, newParameters, separators));
				}
			}
		}
		return instance.ToArrayAndFree().Reverse();
	}
}
