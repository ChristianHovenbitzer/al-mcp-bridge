using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal class StreamingProgressCollector : IStreamingFindReferencesProgress
{
	private readonly object gate = new object();

	private readonly Dictionary<SymbolAndProjectId, List<ReferenceLocation>> symbolToLocations = new Dictionary<SymbolAndProjectId, List<ReferenceLocation>>();

	private readonly IStreamingFindReferencesProgress underlyingProgress;

	public StreamingProgressCollector(IStreamingFindReferencesProgress underlyingProgress)
	{
		this.underlyingProgress = underlyingProgress;
	}

	public Task OnStartedAsync()
	{
		return underlyingProgress.OnStartedAsync();
	}

	public Task OnCompletedAsync()
	{
		return underlyingProgress.OnCompletedAsync();
	}

	public Task ReportProgressAsync(int current, int maximum)
	{
		return underlyingProgress.ReportProgressAsync(current, maximum);
	}

	public Task OnFindInDocumentCompletedAsync(Document document)
	{
		return underlyingProgress.OnFindInDocumentCompletedAsync(document);
	}

	public Task OnFindInDocumentStartedAsync(Document document)
	{
		return underlyingProgress.OnFindInDocumentStartedAsync(document);
	}

	public Task OnDefinitionFoundAsync(SymbolAndProjectId definition)
	{
		lock (gate)
		{
			symbolToLocations[definition] = new List<ReferenceLocation>();
		}
		return underlyingProgress.OnDefinitionFoundAsync(definition);
	}

	public Task OnReferenceFoundAsync(SymbolAndProjectId definition, ReferenceLocation location)
	{
		lock (gate)
		{
			symbolToLocations[definition].Add(location);
		}
		return underlyingProgress.OnReferenceFoundAsync(definition, location);
	}

	public ImmutableArray<ReferencedSymbol> GetReferencedSymbols()
	{
		lock (gate)
		{
			ArrayBuilder<ReferencedSymbol> instance = ArrayBuilder<ReferencedSymbol>.GetInstance();
			foreach (KeyValuePair<SymbolAndProjectId, List<ReferenceLocation>> symbolToLocation in symbolToLocations)
			{
				instance.Add(new ReferencedSymbol(symbolToLocation.Key, symbolToLocation.Value.ToList()));
			}
			return instance.ToImmutableAndFree();
		}
	}
}
