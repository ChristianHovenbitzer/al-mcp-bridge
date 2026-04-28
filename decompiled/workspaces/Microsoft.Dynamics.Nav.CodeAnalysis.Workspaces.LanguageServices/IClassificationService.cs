using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal interface IClassificationService : ILanguageService
{
	IEnumerable<ISyntaxClassifier> GetDefaultSyntaxClassifiers();

	void AddLexicalClassifications(SourceText text, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	void AddSyntacticClassifications(SyntaxTree syntaxTree, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	Task AddSemanticClassificationsAsync(Document document, TextSpan textSpan, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	void AddSemanticClassifications(SemanticModel semanticModel, TextSpan textSpan, Workspace workspace, Func<SyntaxNode, List<ISyntaxClassifier>> getNodeClassifiers, Func<SyntaxToken, List<ISyntaxClassifier>> getTokenClassifiers, List<ClassifiedSpan> result, CancellationToken cancellationToken);

	ClassifiedSpan FixClassification(SourceText text, ClassifiedSpan classifiedSpan);
}
