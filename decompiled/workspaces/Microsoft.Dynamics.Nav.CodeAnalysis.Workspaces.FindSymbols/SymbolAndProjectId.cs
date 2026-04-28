namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal struct SymbolAndProjectId
{
	public readonly ISymbol Symbol;

	public readonly ProjectId ProjectId;

	public SymbolAndProjectId(ISymbol symbol, ProjectId projectId)
	{
		Symbol = symbol;
		ProjectId = projectId;
	}

	public override bool Equals(object obj)
	{
		return Equals((SymbolAndProjectId)obj);
	}

	public bool Equals(SymbolAndProjectId other)
	{
		return object.Equals(Symbol, other.Symbol);
	}

	public override int GetHashCode()
	{
		return Symbol.GetHashCode();
	}

	public static SymbolAndProjectId Create(ISymbol symbol, ProjectId projectId)
	{
		return new SymbolAndProjectId(symbol, projectId);
	}

	public static SymbolAndProjectId<TSymbol> Create<TSymbol>(TSymbol symbol, ProjectId projectId) where TSymbol : ISymbol
	{
		return new SymbolAndProjectId<TSymbol>(symbol, projectId);
	}

	public SymbolAndProjectId<TOther> WithSymbol<TOther>(TOther other) where TOther : ISymbol
	{
		return new SymbolAndProjectId<TOther>(other, ProjectId);
	}

	public SymbolAndProjectId WithSymbol(ISymbol other)
	{
		return new SymbolAndProjectId(other, ProjectId);
	}
}
internal struct SymbolAndProjectId<TSymbol> where TSymbol : ISymbol
{
	public readonly TSymbol Symbol;

	public readonly ProjectId ProjectId;

	public SymbolAndProjectId(TSymbol symbol, ProjectId projectId)
	{
		Symbol = symbol;
		ProjectId = projectId;
	}

	public static implicit operator SymbolAndProjectId(SymbolAndProjectId<TSymbol> value)
	{
		return new SymbolAndProjectId(value.Symbol, value.ProjectId);
	}

	public SymbolAndProjectId<TOther> WithSymbol<TOther>(TOther other) where TOther : ISymbol
	{
		return new SymbolAndProjectId<TOther>(other, ProjectId);
	}

	public SymbolAndProjectId WithSymbol(ISymbol other)
	{
		return new SymbolAndProjectId(other, ProjectId);
	}
}
