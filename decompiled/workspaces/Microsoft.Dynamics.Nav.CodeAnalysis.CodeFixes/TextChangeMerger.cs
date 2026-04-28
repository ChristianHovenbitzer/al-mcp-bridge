using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal class TextChangeMerger
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct IntervalIntrospector : IIntervalIntrospector<TextChange>
	{
		int IIntervalIntrospector<TextChange>.GetStart(TextChange value)
		{
			return value.Span.Start;
		}

		int IIntervalIntrospector<TextChange>.GetLength(TextChange value)
		{
			return value.Span.Length;
		}
	}

	private readonly Document oldDocument;

	private readonly IDocumentTextDifferencingService differenceService;

	private readonly SimpleIntervalTree<TextChange> totalChangesIntervalTree = SimpleIntervalTree.Create(default(IntervalIntrospector), Array.Empty<TextChange>());

	public TextChangeMerger(Document document)
	{
		oldDocument = document;
		differenceService = document.Project.Solution.Workspace.Services.GetRequiredService<IDocumentTextDifferencingService>();
	}

	public async Task TryMergeChangesAsync(Document newDocument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ImmutableArray<TextChange> immutableArray = await differenceService.GetTextChangesAsync(oldDocument, newDocument, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (AllChangesCanBeApplied(totalChangesIntervalTree, immutableArray.ToImmutableArray()))
		{
			ImmutableArray<TextChange>.Enumerator enumerator = immutableArray.GetEnumerator();
			while (enumerator.MoveNext())
			{
				TextChange current = enumerator.Current;
				totalChangesIntervalTree.AddIntervalInPlace(current);
			}
		}
	}

	public async Task TryMergeChangesAsync(ImmutableArray<Document> newDocuments, CancellationToken cancellationToken)
	{
		ImmutableArray<Document>.Enumerator enumerator = newDocuments.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Document current = enumerator.Current;
			await TryMergeChangesAsync(current, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task<SourceText> GetFinalMergedTextAsync(CancellationToken cancellationToken)
	{
		IOrderedEnumerable<TextChange> changesToApply = from tc in totalChangesIntervalTree.Distinct()
			orderby tc.Span.Start
			select tc;
		return (await oldDocument.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).WithChanges(changesToApply);
	}

	private static bool AllChangesCanBeApplied(SimpleIntervalTree<TextChange> cumulativeChanges, ImmutableArray<TextChange> currentChanges)
	{
		ArrayBuilder<TextChange> instance = ArrayBuilder<TextChange>.GetInstance();
		ArrayBuilder<TextChange> instance2 = ArrayBuilder<TextChange>.GetInstance();
		try
		{
			return AllChangesCanBeApplied(cumulativeChanges, currentChanges, instance, instance2);
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private static bool AllChangesCanBeApplied(SimpleIntervalTree<TextChange> cumulativeChanges, ImmutableArray<TextChange> currentChanges, ArrayBuilder<TextChange> overlappingSpans, ArrayBuilder<TextChange> intersectingSpans)
	{
		ImmutableArray<TextChange>.Enumerator enumerator = currentChanges.GetEnumerator();
		while (enumerator.MoveNext())
		{
			TextChange current = enumerator.Current;
			overlappingSpans.Clear();
			intersectingSpans.Clear();
			cumulativeChanges.FillWithIntervalsThatOverlapWith(current.Span.Start, current.Span.Length, overlappingSpans);
			cumulativeChanges.FillWithIntervalsThatIntersectWith(current.Span.Start, current.Span.Length, intersectingSpans);
			if (!ChangeCanBeApplied(current, in overlappingSpans, in intersectingSpans))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ChangeCanBeApplied(TextChange change, in ArrayBuilder<TextChange> overlappingSpans, in ArrayBuilder<TextChange> intersectingSpans)
	{
		if (!IsPureInsertion(change))
		{
			return OverwriteChangeCanBeApplied(change, in overlappingSpans, in intersectingSpans);
		}
		return PureInsertionChangeCanBeApplied(change, in overlappingSpans, in intersectingSpans);
	}

	private static bool IsPureInsertion(TextChange change)
	{
		return change.Span.IsEmpty;
	}

	private static bool PureInsertionChangeCanBeApplied(TextChange change, in ArrayBuilder<TextChange> overlappingSpans, in ArrayBuilder<TextChange> intersectingSpans)
	{
		if (intersectingSpans.Count == 0)
		{
			return true;
		}
		if (intersectingSpans.Count == 1)
		{
			TextChange textChange = intersectingSpans[0];
			if (textChange == change)
			{
				return true;
			}
			if (!IsPureInsertion(textChange))
			{
				return textChange.Span.End == change.Span.Start;
			}
			return false;
		}
		return false;
	}

	private static bool OverwriteChangeCanBeApplied(TextChange change, in ArrayBuilder<TextChange> overlappingSpans, in ArrayBuilder<TextChange> intersectingSpans)
	{
		if (!OverwriteChangeConflictsWithOverlappingSpans(change, in overlappingSpans))
		{
			return !OverwriteChangeConflictsWithIntersectingSpans(change, in intersectingSpans);
		}
		return false;
	}

	private static bool OverwriteChangeConflictsWithOverlappingSpans(TextChange change, in ArrayBuilder<TextChange> overlappingSpans)
	{
		if (overlappingSpans.Count == 0)
		{
			return false;
		}
		return overlappingSpans.Count != 1 || !(overlappingSpans[0] == change);
	}

	private static bool OverwriteChangeConflictsWithIntersectingSpans(TextChange change, in ArrayBuilder<TextChange> intersectingSpans)
	{
		return intersectingSpans.Any((TextChange otherSpan) => IsPureInsertion(otherSpan));
	}
}
