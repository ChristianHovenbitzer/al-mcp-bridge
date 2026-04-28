using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindReferences;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.FindSymbols;

internal static class ReferenceFinders
{
	public static readonly IReferenceFinder LocalVariable = new LocalVariableSymbolReferenceFinder();

	public static readonly IReferenceFinder GlobalVariable = new GlobalVariableSymbolReferenceFinder();

	public static readonly IReferenceFinder Parameter = new ParameterSymbolReferenceFinder();

	public static readonly IReferenceFinder ReturnValue = new ReturnValueSymbolReferenceFinder();

	public static readonly IReferenceFinder OptionValue = new OptionValueSymbolReferenceFinder();

	public static readonly IReferenceFinder Field = new FieldSymbolReferenceFinder();

	public static readonly IReferenceFinder Method = new MethodSymbolReferenceFinder();

	public static readonly IReferenceFinder BuiltInMethod = new BuiltInMethodSymbolReferenceFinder();

	public static readonly IReferenceFinder Event = new EventSymbolReferenceFinder();

	public static readonly IReferenceFinder LocalMethod = new LocalMethodSymbolReferenceFinder();

	public static readonly IReferenceFinder TriggerMethod = new TriggerSymbolReferenceFinder();

	public static readonly IReferenceFinder AttributeArgument = new AttributeArgumentSymbolReferenceFinder();

	public static readonly IReferenceFinder ApplicationObject = new ApplicationObjectSymbolReferenceFinder();

	public static readonly IReferenceFinder Control = new GenericReferenceFinder<IControlSymbol>();

	public static readonly IReferenceFinder Action = new GenericReferenceFinder<IActionSymbol>();

	public static readonly IReferenceFinder View = new GenericReferenceFinder<IViewSymbol>();

	public static readonly IReferenceFinder ReportDataItem = new GenericReferenceFinder<IReportDataItemSymbol>();

	public static readonly IReferenceFinder ReportLabel = new NoReferenceFinder<IReportLabelSymbol>();

	public static readonly IReferenceFinder ReportLayout = new GenericReferenceFinder<IReportLayoutSymbol>();

	public static readonly IReferenceFinder XmlPortNode = new GenericReferenceFinder<IXmlPortNodeSymbol>();

	public static readonly IReferenceFinder QueryDataItem = new GenericReferenceFinder<IQueryDataItemSymbol>();

	public static readonly IReferenceFinder QueryColumn = new GenericReferenceFinder<IQueryColumnSymbol>();

	public static readonly IReferenceFinder QueryFilter = new GenericReferenceFinder<IQueryFilterSymbol>();

	public static readonly IReferenceFinder Namespace = new GenericReferenceFinder<INamespaceSymbol>();

	public static readonly ImmutableArray<IReferenceFinder> DefaultReferenceFinders = ImmutableArray.Create<IReferenceFinder>(LocalVariable, GlobalVariable, Parameter, ReturnValue, OptionValue, Field, Method, LocalMethod, BuiltInMethod, TriggerMethod, Event, ApplicationObject, Control, Action, View, ReportDataItem, ReportLabel, ReportLayout, XmlPortNode, QueryDataItem, QueryColumn, QueryFilter, Namespace, AttributeArgument);
}
