using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions.Mef;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;
using Microsoft.Dynamics.Nav.CodeAnalysis.Editing;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.ActionV1Converter;

[CodeRefactoringProvider("ApplicationAreaRefactoring")]
public sealed class ApplicationAreaRefactoringCodeFixProvider : CodeRefactoringWithFixAllProvider
{
	private sealed class Analyser
	{
		private sealed class MyCodeAction : CodeAction.DocumentChangeAction
		{
			public override CodeActionKind Kind => CodeActionKind.Refactor;

			public MyCodeAction(string title, Func<CancellationToken, Task<Document>> createChangedDocument, string equivalenceKey)
				: base(title, createChangedDocument, equivalenceKey)
			{
			}
		}

		public void ComputeRefactorings(Document document, ApplicationObjectSyntax applicationObjectSyntax, CodeRefactoringContext context, PropertySyntax applicationAreaProperty)
		{
			Document document2 = document;
			ApplicationObjectSyntax applicationObjectSyntax2 = applicationObjectSyntax;
			PropertySyntax applicationAreaProperty2 = applicationAreaProperty;
			bool flag = IsObjectLevelProperty(applicationAreaProperty2);
			if (flag && HasRedundantApplicationAreaProperty(applicationObjectSyntax2, applicationAreaProperty2))
			{
				context.RegisterRefactoring(new MyCodeAction(WorkspacesResources.CleanApplicationArea, (CancellationToken c) => CleanUpApplicationArea(document2, ImmutableArray.Create((SyntaxNode)applicationObjectSyntax2), context.CancellationToken), "CleanUpDefaultedApplicationAreas"));
			}
			else if (!flag && applicationObjectSyntax2.GetProperty("ApplicationArea") == null)
			{
				context.RegisterRefactoring(new MyCodeAction(WorkspacesResources.DefaultApplicationArea, (CancellationToken c) => RefactorApplicationArea(document2, applicationAreaProperty2, ImmutableArray.Create((SyntaxNode)applicationObjectSyntax2), context.CancellationToken), "DefaultApplicationAreasOnObject"));
			}
		}

		public static async Task<Document> RefactorApplicationArea(Document document, PropertySyntax applicationAreaProperty, ImmutableArray<SyntaxNode> topLevelNodes, CancellationToken cancellationToken)
		{
			SyntaxNode syntaxNode = await GetRefactoredRootAsync(document, applicationAreaProperty, topLevelNodes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (syntaxNode != null)
			{
				document = document.WithSyntaxRoot(syntaxNode);
			}
			return document;
		}

		public static async Task<SyntaxNode> GetRefactoredRootAsync(Document document, PropertySyntax applicationAreaProperty, ImmutableArray<SyntaxNode> nodes, CancellationToken cancellationToken)
		{
			PooledNameComparisonHashSet valuesSet = PooledNameComparisonHashSet.GetInstance();
			PooledDictionary<SyntaxNode, SyntaxNode> nodeMap = PooledDictionary<SyntaxNode, SyntaxNode>.GetInstance();
			try
			{
				GetApplicationAreaPropertyValues(applicationAreaProperty, valuesSet);
				_ = Environment.NewLine;
				PropertySyntaxOrEmpty applicationAreaProperty2 = GetApplicationAreaProperty(valuesSet);
				ImmutableArray<SyntaxNode>.Enumerator enumerator = nodes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (!(enumerator.Current is ApplicationObjectSyntax applicationObjectSyntax) || !IsValidKindForConversion(applicationObjectSyntax.Kind))
					{
						continue;
					}
					ApplicationObjectSyntax applicationObjectSyntax2 = CleanApplicationAreaPropertiesByValue(applicationObjectSyntax, valuesSet);
					if (!HasApplicationArea(applicationObjectSyntax2))
					{
						if (applicationObjectSyntax.Kind == SyntaxKind.PageObject)
						{
							PageSyntax pageSyntax = (PageSyntax)applicationObjectSyntax2;
							applicationObjectSyntax2 = pageSyntax.AddPropertyListProperties(GetPropertyWithProperNewLineTrivia(pageSyntax.PropertyList, applicationAreaProperty2));
						}
						else if (applicationObjectSyntax.Kind == SyntaxKind.ReportObject)
						{
							ReportSyntax reportSyntax = (ReportSyntax)applicationObjectSyntax2;
							applicationObjectSyntax2 = reportSyntax.AddPropertyListProperties(GetPropertyWithProperNewLineTrivia(reportSyntax.PropertyList, applicationAreaProperty2));
						}
					}
					if (applicationObjectSyntax2 != null)
					{
						nodeMap.Add(applicationObjectSyntax, applicationObjectSyntax2);
					}
				}
				return (await document.GetSyntaxRootAsync().ConfigureAwait(continueOnCapturedContext: false)).ReplaceNodes(nodeMap.Keys, (SyntaxNode o, SyntaxNode n) => nodeMap[o]);
			}
			finally
			{
				valuesSet.Free();
				nodeMap.Free();
			}
		}

		private static PropertySyntaxOrEmpty GetPropertyWithProperNewLineTrivia(PropertyListSyntax propertyListSyntax, PropertySyntaxOrEmpty newProperty)
		{
			string newLine = Environment.NewLine;
			if (propertyListSyntax.Properties.Count > 0)
			{
				return newProperty.WithTrailingTrivia(SyntaxFactory.EndOfLine(newLine, elastic: false));
			}
			return newProperty.WithTrailingTrivia(SyntaxFactory.EndOfLine(newLine + newLine, elastic: false));
		}

		private static bool HasApplicationArea(ApplicationObjectSyntax newSyntax)
		{
			return newSyntax.GetProperty("ApplicationArea") != null;
		}

		public static async Task<Document> CleanUpApplicationArea(Document document, ImmutableArray<SyntaxNode> topLevelNodes, CancellationToken cancellationToken)
		{
			SyntaxNode syntaxNode = await GetCleanedRootAsync(document, topLevelNodes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (syntaxNode != null)
			{
				document = document.WithSyntaxRoot(syntaxNode);
			}
			return document;
		}

		public static async Task<SyntaxNode> GetCleanedRootAsync(Document document, ImmutableArray<SyntaxNode> topLevelNodes, CancellationToken cancellationToken)
		{
			PooledNameComparisonHashSet valuesSet = PooledNameComparisonHashSet.GetInstance();
			PooledDictionary<SyntaxNode, SyntaxNode> nodeMap = PooledDictionary<SyntaxNode, SyntaxNode>.GetInstance();
			try
			{
				ImmutableArray<SyntaxNode>.Enumerator enumerator = topLevelNodes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					if (!(enumerator.Current is ApplicationObjectSyntax applicationObjectSyntax) || !IsValidKindForConversion(applicationObjectSyntax.Kind))
					{
						continue;
					}
					GetApplicationAreaPropertyValues(applicationObjectSyntax, valuesSet);
					if (!valuesSet.IsNullOrEmpty())
					{
						ApplicationObjectSyntax applicationObjectSyntax2 = CleanApplicationAreaPropertiesByValue(applicationObjectSyntax, valuesSet);
						if (applicationObjectSyntax2 != null)
						{
							nodeMap.Add(applicationObjectSyntax, applicationObjectSyntax2);
						}
						valuesSet.Clear();
					}
				}
				return (await document.GetSyntaxRootAsync().ConfigureAwait(continueOnCapturedContext: false)).ReplaceNodes(nodeMap.Keys, (SyntaxNode o, SyntaxNode n) => nodeMap[o]);
			}
			finally
			{
				valuesSet.Free();
				nodeMap.Free();
			}
		}

		private static void GetApplicationAreaPropertyValues(PropertySyntax applicationAreaProperty, PooledNameComparisonHashSet valuesSet)
		{
			if (applicationAreaProperty.Value is CommaSeparatedPropertyValueSyntax { Values: var values })
			{
				SeparatedSyntaxList<IdentifierNameSyntax>.Enumerator enumerator = values.GetEnumerator();
				while (enumerator.MoveNext())
				{
					IdentifierNameSyntax current = enumerator.Current;
					valuesSet.Add(current.Identifier.ValueText);
				}
			}
		}

		private static void GetApplicationAreaPropertyValues(ApplicationObjectSyntax applicationObjectSyntax, PooledNameComparisonHashSet valuesSet)
		{
			PropertySyntax property = applicationObjectSyntax.GetProperty("ApplicationArea");
			if (property != null && property.Value is CommaSeparatedPropertyValueSyntax { Values: var values })
			{
				SeparatedSyntaxList<IdentifierNameSyntax>.Enumerator enumerator = values.GetEnumerator();
				while (enumerator.MoveNext())
				{
					IdentifierNameSyntax current = enumerator.Current;
					valuesSet.Add(current.Identifier.ValueText);
				}
			}
		}

		private static PropertySyntaxOrEmpty GetApplicationAreaProperty(PooledNameComparisonHashSet applicationAreaValues)
		{
			SeparatedSyntaxList<IdentifierNameSyntax> values = default(SeparatedSyntaxList<IdentifierNameSyntax>);
			foreach (string applicationAreaValue in applicationAreaValues)
			{
				values = values.Add(SyntaxFactory.IdentifierName(applicationAreaValue));
			}
			return SyntaxFactory.Property(PropertyKind.ApplicationArea, (PropertyValueSyntax)SyntaxFactory.CommaSeparatedPropertyValue(values));
		}

		private static T CleanApplicationAreaPropertiesByValue<T>(T applicationObjectSyntax, PooledNameComparisonHashSet applicationAreaValues) where T : ApplicationObjectSyntax
		{
			PooledList<PropertySyntax> instance = PooledList<PropertySyntax>.GetInstance();
			try
			{
				GetApplicationAreaPropertiesByValue(applicationObjectSyntax, applicationAreaValues, instance);
				return applicationObjectSyntax.RemoveNodes(instance, SyntaxRemoveOptions.KeepNoTrivia);
			}
			finally
			{
				instance.Free();
			}
		}

		private static bool HasRedundantApplicationAreaProperty(ApplicationObjectSyntax applicationObjectSyntax, PropertySyntax applicationAreaPropertySyntax)
		{
			PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
			GetApplicationAreaPropertyValues(applicationAreaPropertySyntax, instance);
			foreach (SyntaxNode item in applicationObjectSyntax.SyntaxTree.GetRoot().DescendantNodes())
			{
				if (item.IsKind(SyntaxKind.Property) && !IsObjectLevelProperty(item) && AreEqualApplicationAreaProperty(instance, (PropertySyntax)item))
				{
					return true;
				}
			}
			return false;
		}

		private static bool AreEqualApplicationAreaProperty(PooledNameComparisonHashSet applicationAreaValue, PropertySyntax property)
		{
			if (SemanticFacts.IsSameName(property.Name.Identifier.ValueText, "ApplicationArea"))
			{
				return ValuesAreEqual(applicationAreaValue, (CommaSeparatedPropertyValueSyntax)property.Value);
			}
			return false;
		}

		private static void GetApplicationAreaPropertiesByValue(ApplicationObjectSyntax applicationObjectSyntax, PooledNameComparisonHashSet applicationAreaValue, PooledList<PropertySyntax> bag)
		{
			foreach (SyntaxNode item in applicationObjectSyntax.SyntaxTree.GetRoot().DescendantNodes())
			{
				if (item.IsKind(SyntaxKind.Property) && !IsObjectLevelProperty(item))
				{
					PropertySyntax propertySyntax = (PropertySyntax)item;
					if (AreEqualApplicationAreaProperty(applicationAreaValue, propertySyntax))
					{
						bag.Add(propertySyntax);
					}
				}
			}
		}

		private static bool ValuesAreEqual(PooledNameComparisonHashSet expectedApplicationAreaValue, CommaSeparatedPropertyValueSyntax currentNodeValues)
		{
			if (expectedApplicationAreaValue.Count != currentNodeValues.Values.Count)
			{
				return false;
			}
			SeparatedSyntaxList<IdentifierNameSyntax>.Enumerator enumerator = currentNodeValues.Values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				IdentifierNameSyntax current = enumerator.Current;
				if (!expectedApplicationAreaValue.Contains(current.Identifier.ValueText))
				{
					return false;
				}
			}
			return true;
		}
	}

	private const string CleanUpEquivalenceKey = "CleanUpDefaultedApplicationAreas";

	private const string SetDefaultEquivalenceKey = "DefaultApplicationAreasOnObject";

	private static readonly HashSet<SyntaxKind> parentNodesContainingSet = new HashSet<SyntaxKind>
	{
		SyntaxKind.PageObject,
		SyntaxKind.ReportObject
	};

	protected sealed override ImmutableArray<FixAllScope> SupportedFixAllScopes => ImmutableArray.Create(FixAllScope.Document, FixAllScope.Project, FixAllScope.Workspace);

	public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
	{
		Document document = context.Document;
		SyntaxNode syntaxNode = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (!VersionChecker.IsSupported(syntaxNode, VersionCompatibility.Fall2022OrGreater))
		{
			return;
		}
		PropertySyntax applicationAreaProperty = GetApplicationAreaProperty(syntaxNode.FindNode(context.Span));
		if (applicationAreaProperty != null)
		{
			ApplicationObjectSyntax containingApplicationObjectSyntax = applicationAreaProperty.GetContainingApplicationObjectSyntax();
			if (containingApplicationObjectSyntax != null && IsValidKindForConversion(containingApplicationObjectSyntax.Kind))
			{
				CreateAnalyser().ComputeRefactorings(document, containingApplicationObjectSyntax, context, applicationAreaProperty);
			}
		}
	}

	protected sealed override async Task FixAllAsync(FixAllContext fixAllContext, Document document, ImmutableArray<TextSpan> fixAllSpans, SyntaxEditor editor, CodeActionOptionsProvider optionsProvider, string? equivalenceKey, CancellationToken cancellationToken)
	{
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (root == null)
		{
			return;
		}
		SyntaxNode syntaxNode = null;
		if (equivalenceKey == "DefaultApplicationAreasOnObject")
		{
			PropertySyntax applicationAreaProperty = GetApplicationAreaProperty(await GetOriginalNodeAsync(fixAllContext.State.Document, fixAllContext.State.Span, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			if (applicationAreaProperty == null)
			{
				return;
			}
			syntaxNode = await Analyser.GetRefactoredRootAsync(document, applicationAreaProperty, root.ChildNodes().ToImmutableArray(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else if (equivalenceKey == "CleanUpDefaultedApplicationAreas")
		{
			syntaxNode = await Analyser.GetCleanedRootAsync(document, root.ChildNodes().ToImmutableArray(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (syntaxNode != null)
		{
			editor.ReplaceNode(editor.OriginalRoot, syntaxNode);
		}
	}

	private async Task<SyntaxNode> GetOriginalNodeAsync(Document? document, TextSpan? span, CancellationToken cancellationToken)
	{
		if (document == null || !span.HasValue)
		{
			return null;
		}
		return (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))?.FindNode(span.Value);
	}

	private static bool IsValidKindForConversion(SyntaxKind syntaxKind)
	{
		if (syntaxKind != SyntaxKind.PageObject)
		{
			return syntaxKind == SyntaxKind.ReportObject;
		}
		return true;
	}

	private static bool IsObjectLevelProperty(SyntaxNode syntaxNode)
	{
		return syntaxNode.Parent?.IsParentKind(parentNodesContainingSet) ?? false;
	}

	private static PropertySyntax GetApplicationAreaProperty(SyntaxNode node)
	{
		if (node == null)
		{
			return null;
		}
		if (node.IsKind(SyntaxKind.Property))
		{
			return IsApplicationAreaProperty((PropertySyntaxOrEmpty)node);
		}
		SyntaxNode parent = node.Parent;
		if (parent != null && parent.IsKind(SyntaxKind.Property))
		{
			return IsApplicationAreaProperty((PropertySyntaxOrEmpty)node.Parent);
		}
		if (node.IsKind(SyntaxKind.PropertyList))
		{
			return node.GetProperty("ApplicationArea");
		}
		return null;
	}

	private static PropertySyntax IsApplicationAreaProperty(PropertySyntaxOrEmpty propertySyntaxOrEmpty)
	{
		if (propertySyntaxOrEmpty.Kind != SyntaxKind.EmptyProperty)
		{
			PropertySyntax propertySyntax = (PropertySyntax)propertySyntaxOrEmpty;
			if (SemanticFacts.IsSameName(propertySyntax.Name.Identifier.ValueText, "ApplicationArea"))
			{
				return propertySyntax;
			}
		}
		return null;
	}

	private Analyser CreateAnalyser()
	{
		return new Analyser();
	}
}
