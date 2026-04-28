using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class MovedFromToPropertyValueRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	public MovedFromToPropertyValueRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		PropertyTypeInfo? propertyTypeInfo = base.PropertyTypeInfo;
		if (propertyTypeInfo == null || propertyTypeInfo.Kind != PropertyKind.MovedFrom)
		{
			PropertyTypeInfo? propertyTypeInfo2 = base.PropertyTypeInfo;
			if (propertyTypeInfo2 == null || propertyTypeInfo2.Kind != PropertyKind.MovedTo)
			{
				return await base.RecommendPropertyValuesAsync(cancellationToken);
			}
		}
		using PooledDictionary<Guid, string> pooledDictionary = PooledDictionary<Guid, string>.GetInstance();
		Guid appId = base.Context.SemanticModel.Compilation.CompiledModule.AppId;
		foreach (Project project in base.Context.Workspace.CurrentSolution.Projects)
		{
			ProjectDefinition projectDefinition = project.State.ProjectDefinition;
			Guid appId2 = projectDefinition.AppId;
			if (!(appId2 == appId) && !pooledDictionary.ContainsKey(appId2))
			{
				pooledDictionary.Add(appId2, projectDefinition.Name);
			}
		}
		if (base.DeclaringObject != null)
		{
			AddMoveDestinationOrSource(base.DeclaringObject, pooledDictionary);
			IEnumerable<IFieldSymbol> enumerable2;
			if (base.DeclaringObject.Kind != SymbolKind.Table)
			{
				if (base.DeclaringObject.Kind != SymbolKind.TableExtension)
				{
					IEnumerable<IFieldSymbol> enumerable = Array.Empty<IFieldSymbol>();
					enumerable2 = enumerable;
				}
				else
				{
					IEnumerable<IFieldSymbol> enumerable = ((ITableExtensionTypeSymbol)base.DeclaringObject).AddedFields;
					enumerable2 = enumerable;
				}
			}
			else
			{
				IEnumerable<IFieldSymbol> enumerable = ((ITableTypeSymbol)base.DeclaringObject).Fields;
				enumerable2 = enumerable;
			}
			foreach (IFieldSymbol item in enumerable2)
			{
				AddMoveDestinationOrSource(item, pooledDictionary);
			}
		}
		return pooledDictionary.SelectAsArray(delegate(KeyValuePair<Guid, string> s)
		{
			string text = s.Key.ToString();
			string obj = ((!string.IsNullOrEmpty(s.Value)) ? s.Value : text);
			PropertyValueRecommendation propertyValueRecommendation = new PropertyValueRecommendation(obj)
			{
				InsertionText = CalculateInsertionText(text)
			};
			if (!obj.Equals(text, StringComparison.OrdinalIgnoreCase))
			{
				propertyValueRecommendation.DescriptionValue = text;
			}
			return propertyValueRecommendation;
		});
	}

	private string CalculateInsertionText(string projectId)
	{
		if (base.Context.LeftToken.Kind == SyntaxKind.EqualsToken)
		{
			projectId = "'" + projectId;
		}
		return projectId + "'";
	}

	private static void AddMoveDestinationOrSource(ISymbol symbol, PooledDictionary<Guid, string> collection)
	{
		IPropertySymbol propertySymbol = symbol.GetProperty(PropertyKind.MovedTo) ?? symbol.GetProperty(PropertyKind.MovedFrom);
		if (propertySymbol != null && Guid.TryParse(propertySymbol.ValueText, out var result) && !collection.ContainsKey(result))
		{
			collection.Add(result, string.Empty);
		}
	}
}
