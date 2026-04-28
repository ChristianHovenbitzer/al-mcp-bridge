using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal sealed class SyntaxTreeIndex
{
	private struct DeclarationInfo
	{
		public ImmutableArray<DeclaredSymbolInfo> DeclaredSymbolInfos { get; }

		public DeclarationInfo(ImmutableArray<DeclaredSymbolInfo> declaredSymbolInfos)
		{
			DeclaredSymbolInfos = declaredSymbolInfos;
		}
	}

	private struct IdentifierInfo
	{
		private readonly BloomFilter identifierFilter;

		private readonly BloomFilter escapedIdentifierFilter;

		public IdentifierInfo(BloomFilter identifierFilter, BloomFilter escapedIdentifierFilter)
		{
			if (identifierFilter == null)
			{
				throw new ArgumentNullException("identifierFilter");
			}
			if (escapedIdentifierFilter == null)
			{
				throw new ArgumentNullException("escapedIdentifierFilter");
			}
			this.identifierFilter = identifierFilter;
			this.escapedIdentifierFilter = escapedIdentifierFilter;
		}

		public bool ProbablyContainsIdentifier(string identifier)
		{
			return identifierFilter.ProbablyContains(identifier);
		}

		public bool ProbablyContainsEscapedIdentifier(string identifier)
		{
			return escapedIdentifierFilter.ProbablyContains(identifier);
		}
	}

	private static readonly ConditionalWeakTable<Document, SyntaxTreeIndex> infoCache = new ConditionalWeakTable<Document, SyntaxTreeIndex>();

	private static readonly Func<Document, CancellationToken, Task<SyntaxTreeIndex>> loadAsync = LoadAsync;

	private readonly DeclarationInfo declarationInfo;

	private readonly IdentifierInfo identifierInfo;

	private const double FalsePositiveProbability = 0.0001;

	public readonly VersionStamp Version;

	public ImmutableArray<DeclaredSymbolInfo> DeclaredSymbolInfos => declarationInfo.DeclaredSymbolInfos;

	private SyntaxTreeIndex(VersionStamp version, IdentifierInfo identifierInfo, DeclarationInfo declarationInfo)
	{
		Version = version;
		this.identifierInfo = identifierInfo;
		this.declarationInfo = declarationInfo;
	}

	public static Task<SyntaxTreeIndex> GetIndexAsync(Document document, CancellationToken cancellationToken)
	{
		return GetIndexAsync(document, infoCache, loadAsync, cancellationToken);
	}

	private static async Task<SyntaxTreeIndex> GetIndexAsync(Document document, ConditionalWeakTable<Document, SyntaxTreeIndex> cache, Func<Document, CancellationToken, Task<SyntaxTreeIndex>> generator, CancellationToken cancellationToken)
	{
		if (cache.TryGetValue(document, out SyntaxTreeIndex value))
		{
			return value;
		}
		SyntaxTreeIndex data = await CreateInfoAsync(document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return cache.GetValue(document, (Document _) => data);
	}

	private static async Task<SyntaxTreeIndex> CreateInfoAsync(Document document, CancellationToken cancellationToken)
	{
		ISyntaxFactsService syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
		bool ignoreCase = syntaxFacts != null && !syntaxFacts.IsCaseSensitive;
		bool isCaseSensitive = !ignoreCase;
		GetIdentifierSet(ignoreCase, out HashSet<string> identifiers, out HashSet<string> escapedIdentifiers);
		try
		{
			ArrayBuilder<DeclaredSymbolInfo> declaredSymbolInfos = ArrayBuilder<DeclaredSymbolInfo>.GetInstance();
			if (syntaxFacts != null)
			{
				foreach (SyntaxNodeOrToken item in (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).DescendantNodesAndTokensAndSelf(null, descendIntoTrivia: true))
				{
					if (item.IsNode)
					{
						_ = (SyntaxNode?)item;
						continue;
					}
					SyntaxToken token = (SyntaxToken)item;
					if (syntaxFacts.IsIdentifier(token))
					{
						string text = token.ValueText.UnquoteIdentifier();
						identifiers.Add(text);
						if (text.Length != token.Width)
						{
							escapedIdentifiers.Add(text);
						}
					}
				}
			}
			return new SyntaxTreeIndex(await document.GetSyntaxVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), new IdentifierInfo(new BloomFilter(0.0001, isCaseSensitive, identifiers), new BloomFilter(0.0001, isCaseSensitive, escapedIdentifiers)), new DeclarationInfo(declaredSymbolInfos.ToImmutableAndFree()));
		}
		finally
		{
			Free(ignoreCase, identifiers, escapedIdentifiers);
		}
	}

	private static void GetIdentifierSet(bool ignoreCase, out HashSet<string> identifiers, out HashSet<string> escapedIdentifiers)
	{
		if (ignoreCase)
		{
			identifiers = SharedPools.StringIgnoreCaseHashSet.AllocateAndClear();
			escapedIdentifiers = SharedPools.StringIgnoreCaseHashSet.AllocateAndClear();
		}
		else
		{
			identifiers = SharedPools.StringHashSet.AllocateAndClear();
			escapedIdentifiers = SharedPools.StringHashSet.AllocateAndClear();
		}
	}

	private static void Free(bool ignoreCase, HashSet<string> identifiers, HashSet<string> escapedIdentifiers)
	{
		if (ignoreCase)
		{
			SharedPools.StringIgnoreCaseHashSet.ClearAndFree(identifiers);
			SharedPools.StringIgnoreCaseHashSet.ClearAndFree(escapedIdentifiers);
		}
		else
		{
			SharedPools.StringHashSet.ClearAndFree(identifiers);
			SharedPools.StringHashSet.ClearAndFree(escapedIdentifiers);
		}
	}

	public bool ProbablyContainsIdentifier(string identifier)
	{
		return identifierInfo.ProbablyContainsIdentifier(identifier);
	}

	public bool ProbablyContainsEscapedIdentifier(string identifier)
	{
		return identifierInfo.ProbablyContainsEscapedIdentifier(identifier);
	}

	private static Task<SyntaxTreeIndex> LoadAsync(Document document, CancellationToken cancellationToken)
	{
		return null;
	}
}
