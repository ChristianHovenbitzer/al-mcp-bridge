using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

internal class CodeRefactoringService : ICodeRefactoringService
{
	private readonly Lazy<ImmutableArray<CodeRefactoringProvider>> lazyCodeRefactoringProviders;

	private ImmutableDictionary<object, FixAllProviderInfo?> fixAllProviderMap = ImmutableDictionary<object, FixAllProviderInfo>.Empty;

	private ImmutableArray<CodeRefactoringProvider> CodeRefactoringProviders => lazyCodeRefactoringProviders.Value;

	public CodeRefactoringService(IEnumerable<CodeRefactoringProvider> providers)
	{
		IEnumerable<CodeRefactoringProvider> providers2 = providers;
		base._002Ector();
		lazyCodeRefactoringProviders = new Lazy<ImmutableArray<CodeRefactoringProvider>>(() => (providers2 != null) ? providers2.ToImmutableArray() : ImmutableArray<CodeRefactoringProvider>.Empty);
	}

	public async Task<ImmutableArray<CodeRefactoring>> GetRefactoringsAsync(Document document, TextSpan state, CancellationToken cancellationToken)
	{
		Document document2 = document;
		List<Task<CodeRefactoring>> list = new List<Task<CodeRefactoring>>();
		ImmutableArray<CodeRefactoringProvider>.Enumerator enumerator = CodeRefactoringProviders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			CodeRefactoringProvider provider = enumerator.Current;
			list.Add(Task.Run(() => GetRefactoringFromProviderAsync(document2, state, provider, cancellationToken), cancellationToken));
		}
		return (await Task.WhenAll(list).ConfigureAwait(continueOnCapturedContext: false)).WhereNotNull().ToImmutableArray();
	}

	private async Task<CodeRefactoring> GetRefactoringFromProviderAsync(Document document, TextSpan textSpan, CodeRefactoringProvider provider, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayBuilder<(CodeAction action, TextSpan? applicableSpan)> actions = ArrayBuilder<(CodeAction, TextSpan?)>.GetInstance();
		try
		{
			CodeRefactoringContext context = new CodeRefactoringContext(document, textSpan, delegate(CodeAction a)
			{
				lock (actions)
				{
					actions.Add((a, textSpan));
				}
			}, cancellationToken);
			await (provider.ComputeRefactoringsAsync(context) ?? Task.CompletedTask).ConfigureAwait(continueOnCapturedContext: false);
			if (actions.IsEmpty())
			{
				return null;
			}
			CodeRefactoring codeRefactoring = new CodeRefactoring(provider, actions.ToImmutable());
			FixAllProviderInfo orAdd = ImmutableInterlocked.GetOrAdd(ref fixAllProviderMap, provider, FixAllProviderInfo.Create);
			if (orAdd != null)
			{
				ArrayBuilder<FixAllState> instance = ArrayBuilder<FixAllState>.GetInstance();
				try
				{
					ImmutableArray<FixAllScope>.Enumerator enumerator = orAdd.SupportedScopes.GetEnumerator();
					while (enumerator.MoveNext())
					{
						FixAllScope current = enumerator.Current;
						FixAllState item = new FixAllState(orAdd.FixAllProvider, document, textSpan, provider, null, current, actions[0].action.EquivalenceKey);
						instance.Add(item);
					}
					codeRefactoring.WithFixAllStates(instance.ToImmutableArray());
				}
				finally
				{
					instance.Free();
				}
			}
			return codeRefactoring;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
			throw;
		}
		finally
		{
			actions.Free();
		}
	}
}
