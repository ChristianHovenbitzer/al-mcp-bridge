using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.Interfaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

[CodeRefactoringProvider("ReportLayoutPropertyToRendering")]
public class ConvertReportLayoutPropetyToRenderingCodeRefactoringProvider : CodeRefactoringProvider
{
	private sealed class ReportLayoutSyntaxTransformer
	{
		public void ComputeRefactoring(CodeRefactoringContext context, SyntaxNode root, ImmutableArray<PropertySymbol> propertiesToRemove, bool replaceDefaultLayoutProperty)
		{
			SyntaxNode root2 = root;
			context.RegisterRefactoring(new RefactorCodeAction(WorkspacesResources.ConvertReportLayoutToRendering, async (CancellationToken c) => await UpdateDocumentAsync(context, root2, propertiesToRemove, replaceDefaultLayoutProperty, c).ConfigureAwait(continueOnCapturedContext: false)));
		}

		private async Task<Document> UpdateDocumentAsync(CodeRefactoringContext context, SyntaxNode root, ImmutableArray<PropertySymbol> propertiesToRemove, bool replaceDefaultLayoutProperty, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return context.Document;
			}
			if (propertiesToRemove != null && propertiesToRemove.Length > 0)
			{
				PooledList<ReportLayoutSyntax> instance = PooledList<ReportLayoutSyntax>.GetInstance();
				PooledList<PropertySyntax> instance2 = PooledList<PropertySyntax>.GetInstance();
				try
				{
					SyntaxNode syntaxNode = propertiesToRemove[0].ContainingSymbol?.DeclaringSyntaxNode;
					if (syntaxNode != null)
					{
						for (int i = 0; i < propertiesToRemove.Length; i++)
						{
							PropertySymbol propertySymbol = propertiesToRemove[i];
							if (propertySymbol.Value != null)
							{
								TypeKind reportLayoutType = propertySymbol.PropertyKind.ToReportLayoutTypeKind();
								instance.Add(ComposeLayoutSyntax(propertySymbol.Value.ToString(), reportLayoutType));
								instance2.Add((PropertySyntax)propertySymbol.DeclaringSyntaxNode);
							}
						}
						SyntaxNode reportParentSyntax = syntaxNode;
						if (instance2.Count != 0)
						{
							reportParentSyntax = syntaxNode.RemoveNodes(instance2.ToArray(), SyntaxRemoveOptions.KeepNoTrivia);
						}
						reportParentSyntax = ComposeRefactoredRenderingSyntax(reportParentSyntax, instance.ToArray());
						if (replaceDefaultLayoutProperty)
						{
							reportParentSyntax = UpdateDefaultLayoutProperty(reportParentSyntax);
						}
						root = root.ReplaceNode(syntaxNode, reportParentSyntax);
					}
				}
				finally
				{
					instance.Free();
					instance2.Free();
				}
			}
			return await Formatter.FormatAsync(context.Document.WithSyntaxRoot(root), context.Document.Project.Solution.Workspace.Options, context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}

		private static SyntaxNode UpdateDefaultLayoutProperty(SyntaxNode reportParentSyntax)
		{
			if (reportParentSyntax.IsKind(SyntaxKind.ReportObject))
			{
				ReportSyntax reportSyntax = (ReportSyntax)reportParentSyntax;
				PropertySyntax property = reportParentSyntax.GetProperty("DefaultLayout");
				if (property != null && reportSyntax != null && reportSyntax.Rendering != null)
				{
					reportParentSyntax = ReplaceDefaultRenderingLayout(reportParentSyntax, property, reportSyntax.Rendering.Layouts);
				}
			}
			return reportParentSyntax;
		}

		private static SyntaxNode ReplaceDefaultRenderingLayout(SyntaxNode reportSyntax, PropertySyntax defaultLayoutPropertyToRemove, SyntaxList<ReportLayoutSyntax> layouts)
		{
			string text = defaultLayoutPropertyToRemove.Value.ToString().Trim('\'');
			SyntaxList<ReportLayoutSyntax>.Enumerator enumerator = layouts.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReportLayoutSyntax current = enumerator.Current;
				SyntaxList<PropertySyntaxOrEmpty>.Enumerator enumerator2 = current.PropertyList.Properties.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					PropertySyntax propertySyntax = (PropertySyntax)enumerator2.Current;
					if (propertySyntax.Name.ToString() == "Type" && propertySyntax.Value.GetPropertyValue().ToString() == text)
					{
						reportSyntax = reportSyntax.ReplaceNode(defaultLayoutPropertyToRemove, SyntaxFactory.Property(PropertyKind.DefaultRenderingLayout, (PropertyValueSyntax)SyntaxFactory.MemberReferencePropertyValue(current.Name)).NormalizeWhiteSpace().WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed));
						break;
					}
				}
			}
			return reportSyntax;
		}

		private static SyntaxNode ComposeRefactoredRenderingSyntax(SyntaxNode reportParentSyntax, ReportLayoutSyntax[] reportLayouts)
		{
			if (reportLayouts.Length != 0)
			{
				if (!(reportParentSyntax is ReportSyntax reportSyntax))
				{
					if (reportParentSyntax is ReportExtensionSyntax reportExtensionSyntax)
					{
						ReportExtensionSyntax reportExtensionSyntax2 = reportExtensionSyntax.AddRenderingLayouts(reportLayouts);
						reportExtensionSyntax2 = reportExtensionSyntax2.WithRendering(reportExtensionSyntax2.Rendering.NormalizeWhiteSpace().WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed).WithLeadingTrivia(SyntaxFactory.CarriageReturnLinefeed));
						reportParentSyntax = reportParentSyntax.ReplaceNode(reportExtensionSyntax, reportExtensionSyntax2);
					}
				}
				else
				{
					ReportSyntax reportSyntax2 = reportSyntax.AddRenderingLayouts(reportLayouts);
					reportSyntax2 = reportSyntax2.WithRendering(reportSyntax2.Rendering.NormalizeWhiteSpace().WithTrailingTrivia(SyntaxFactory.CarriageReturnLinefeed).WithLeadingTrivia(SyntaxFactory.CarriageReturnLinefeed));
					reportParentSyntax = reportParentSyntax.ReplaceNode(reportSyntax, reportSyntax2);
				}
			}
			return reportParentSyntax;
		}

		private static ReportLayoutSyntax ComposeLayoutSyntax(string filename, TypeKind reportLayoutType)
		{
			filename = filename.Trim('\'');
			SyntaxList<PropertySyntaxOrEmpty> properties = default(SyntaxList<PropertySyntaxOrEmpty>).Add(SyntaxFactory.Property(PropertyKind.Type, reportLayoutType)).Add(SyntaxFactory.PropertyLiteral(PropertyKind.LayoutFile, filename));
			return SyntaxFactory.ReportLayout(SyntaxFactory.IdentifierName(filename), SyntaxFactory.PropertyList(properties));
		}
	}

	public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
	{
		SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		PropertyListSyntax propertyListSyntax = root.FindNode(context.Span).FirstAncestorOrSelf<PropertyListSyntax>(IsReportLayoutPropertyFound);
		if (propertyListSyntax == null || propertyListSyntax.Properties.Count == 0)
		{
			return;
		}
		ISymbol symbol = await context.Document.GetSymbolAtPositionAsync(propertyListSyntax.Properties[0].Position, context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (symbol != null && symbol.ContainingSymbol.IsKind(SymbolKind.Report, SymbolKind.ReportExtension))
		{
			IReportRenderingHelper obj = (IReportRenderingHelper)symbol.ContainingSymbol;
			ImmutableArray<PropertySymbol> reportLayoutProperties = obj.GetReportLayoutProperties();
			PropertySymbol defaultLayoutProperty = obj.GetDefaultLayoutProperty();
			bool flag = obj.HasRenderingSection();
			if (!(reportLayoutProperties == null || reportLayoutProperties.Length == 0 || flag))
			{
				CreateReportLayoutSyntaxTransformer().ComputeRefactoring(context, root, reportLayoutProperties, defaultLayoutProperty != null);
			}
		}
	}

	private ReportLayoutSyntaxTransformer CreateReportLayoutSyntaxTransformer()
	{
		return new ReportLayoutSyntaxTransformer();
	}

	internal static bool IsReportLayoutPropertyFound(SyntaxNode node)
	{
		if (node.IsKind(SyntaxKind.PropertyList))
		{
			PropertyListSyntax propertyListSyntax = (PropertyListSyntax)node;
			if (propertyListSyntax != null)
			{
				SyntaxList<PropertySyntaxOrEmpty>.Enumerator enumerator = propertyListSyntax.Properties.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PropertySyntaxOrEmpty current = enumerator.Current;
					if (current.Kind == SyntaxKind.Property && ((PropertySyntax)current).IsReportLayoutProperty())
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
