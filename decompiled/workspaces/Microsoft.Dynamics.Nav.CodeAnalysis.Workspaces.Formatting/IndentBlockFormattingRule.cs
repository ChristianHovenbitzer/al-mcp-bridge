using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting.Rules;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal class IndentBlockFormattingRule : BaseFormattingRule
{
	internal const string Name = "AL IndentBlock Formatting Rule";

	public override void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation)
	{
		nextOperation.Invoke(list);
		AddPropertyIndentationOperation(list, node);
		AddAlignmentBlockOperation(list, node, optionSet);
		AddBlockIndentationOperation(list, node);
		AddEmbeddedStatementsIndentation(list, node);
	}

	private void AddAlignmentBlockOperation(List<IndentBlockOperation> list, SyntaxNode node, OptionSet optionSet)
	{
		if (!(node is GlobalVarSectionSyntax { Variables: var variables } globalVarSectionSyntax))
		{
			if (!(node is VarSectionSyntax { Variables: var variables2 } varSectionSyntax))
			{
				if (!(node is PropertyListSyntax { Properties: var properties } propertyListSyntax))
				{
					if (!(node is RepeatStatementSyntax { Statements: var statements } repeatStatementSyntax))
					{
						if (!(node is CaseLineSyntax caseLineSyntax))
						{
							if (!(node is CaseElseSyntax caseElseSyntax))
							{
								if (node is StatementSyntax statementSyntax && (statementSyntax.Parent.IsKind(SyntaxKind.CaseLine) || (statementSyntax.Parent.IsKind(SyntaxKind.CaseElse) && !statementSyntax.IsKind(SyntaxKind.Block))))
								{
									SyntaxToken firstToken = statementSyntax.Parent.GetFirstToken(includeZeroWidth: true);
									SyntaxToken firstToken2 = statementSyntax.GetFirstToken(includeZeroWidth: true);
									SyntaxToken lastToken = statementSyntax.GetLastToken(includeZeroWidth: true);
									if (!firstToken2.IsMissing && !firstToken.IsMissing)
									{
										AddIndentBlockOperation(list, firstToken, firstToken2, lastToken);
									}
								}
							}
							else
							{
								SyntaxToken firstToken3 = caseElseSyntax.Parent.GetFirstToken(includeZeroWidth: true);
								SyntaxToken firstToken4 = caseElseSyntax.GetFirstToken(includeZeroWidth: true);
								SyntaxToken lastToken2 = caseElseSyntax.GetLastToken(includeZeroWidth: true);
								AddIndentBlockOperation(list, firstToken3, firstToken4, lastToken2);
							}
						}
						else
						{
							SyntaxToken firstToken5 = caseLineSyntax.Parent.GetFirstToken(includeZeroWidth: true);
							SyntaxToken firstToken6 = caseLineSyntax.GetFirstToken(includeZeroWidth: true);
							SyntaxToken lastToken3 = caseLineSyntax.GetLastToken(includeZeroWidth: true);
							AddIndentBlockOperation(list, firstToken5, firstToken6, lastToken3);
						}
					}
					else if (statements.Count != 0)
					{
						SyntaxList<StatementSyntax> statements2 = repeatStatementSyntax.Statements;
						StatementSyntax statementSyntax2 = statements2.First();
						if (statementSyntax2.Kind == SyntaxKind.Block)
						{
							AddBlockIndentationOperation(list, statementSyntax2);
						}
						else if (repeatStatementSyntax.Parent.Kind == SyntaxKind.CaseLine)
						{
							SyntaxToken firstToken7 = repeatStatementSyntax.Parent.GetFirstToken(includeZeroWidth: true);
							SyntaxToken firstToken8 = repeatStatementSyntax.GetFirstToken(includeZeroWidth: true);
							SyntaxToken lastToken4 = repeatStatementSyntax.GetLastToken(includeZeroWidth: true);
							AddIndentBlockOperation(list, firstToken7, firstToken8, lastToken4);
						}
						else
						{
							SyntaxToken firstToken9 = repeatStatementSyntax.GetFirstToken(includeZeroWidth: true);
							SyntaxToken firstToken10 = statements2.First().GetFirstToken(includeZeroWidth: true);
							SyntaxToken lastToken5 = statements2.Last().GetLastToken(includeZeroWidth: true);
							AddIndentBlockOperation(list, firstToken9, firstToken10, lastToken5);
						}
					}
				}
				else if (properties.Count != 0)
				{
					SyntaxToken firstToken11 = propertyListSyntax.Parent.GetFirstToken(includeZeroWidth: true);
					SyntaxToken firstToken12 = propertyListSyntax.Properties.First().GetFirstToken(includeZeroWidth: true);
					SyntaxToken lastToken6 = propertyListSyntax.Properties.Last().GetLastToken(includeZeroWidth: true);
					AddIndentBlockOperation(list, firstToken11, firstToken12, lastToken6);
				}
			}
			else if (variables2.Count != 0)
			{
				SyntaxToken firstToken13 = varSectionSyntax.GetFirstToken(includeZeroWidth: true);
				SyntaxToken lastToken7 = varSectionSyntax.GetLastToken(includeZeroWidth: true);
				if (!firstToken13.IsMissing && !firstToken13.Equals(lastToken7))
				{
					SyntaxToken nextToken = firstToken13.GetNextToken(includeZeroWidth: true);
					AddIndentBlockOperation(list, firstToken13, nextToken, lastToken7);
				}
			}
		}
		else if (variables.Count != 0)
		{
			SyntaxToken baseToken = ((globalVarSectionSyntax.AccessModifier.Kind == SyntaxKind.ProtectedKeyword) ? globalVarSectionSyntax.AccessModifier : globalVarSectionSyntax.VarKeyword);
			SyntaxToken lastToken8 = globalVarSectionSyntax.GetLastToken(includeZeroWidth: true);
			if (!baseToken.IsMissing && !baseToken.Equals(lastToken8))
			{
				SyntaxToken nextToken2 = baseToken.GetNextToken(includeZeroWidth: true);
				AddIndentBlockOperation(list, baseToken, nextToken2, lastToken8);
			}
		}
	}

	private void AddPropertyIndentationOperation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		if (node is PropertySyntax)
		{
			AddIndentBlockOperation(list, node.Parent.Parent.GetFirstToken(), node.GetFirstToken(includeZeroWidth: true), node.GetLastToken(includeZeroWidth: true), IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine);
		}
	}

	private void SetAlignmentBlockOperation(List<IndentBlockOperation> list, SyntaxNode baseNode, SyntaxNode body)
	{
		SyntaxToken firstToken = baseNode.GetFirstToken(includeZeroWidth: true);
		SyntaxToken firstToken2 = body.GetFirstToken(includeZeroWidth: true);
		SyntaxToken lastToken = body.GetLastToken(includeZeroWidth: true);
		SetAlignmentBlockOperation(list, firstToken, firstToken2, lastToken, IndentBlockOption.RelativeToFirstTokenOnBaseTokenLine);
	}

	private void AddBlockIndentationOperation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		(SyntaxToken, SyntaxToken) scopeDelimiters = node.GetScopeDelimiters();
		AddIndentBlockOperation(list, scopeDelimiters.Item1.GetNextToken(includeZeroWidth: true), scopeDelimiters.Item2.GetPreviousToken(includeZeroWidth: true));
	}

	private void AddEmbeddedStatementsIndentation(List<IndentBlockOperation> list, SyntaxNode node)
	{
		if (node is IfStatementSyntax ifStatementSyntax)
		{
			if (ifStatementSyntax.Statement != null && !(ifStatementSyntax.Statement is BlockSyntax))
			{
				SyntaxToken baseToken = default(SyntaxToken);
				SyntaxToken previousToken = ifStatementSyntax.IfKeywordToken.GetPreviousToken(includeZeroWidth: true);
				if (previousToken.IsKind(SyntaxKind.ElseKeyword) && previousToken.IsOnTheSameLineAs(ifStatementSyntax.IfKeywordToken))
				{
					baseToken = previousToken;
				}
				AddEmbeddedStatementsIndentationOperation(list, ifStatementSyntax.Statement, baseToken);
			}
			if (ifStatementSyntax.ElseStatement != null && !(ifStatementSyntax.ElseStatement is BlockSyntax))
			{
				SyntaxToken firstToken = ifStatementSyntax.ElseStatement.GetFirstToken(includeZeroWidth: true);
				if (!(ifStatementSyntax.ElseStatement is IfStatementSyntax) || !ifStatementSyntax.ElseKeywordToken.IsOnTheSameLineAs(firstToken))
				{
					AddEmbeddedStatementsIndentationOperation(list, ifStatementSyntax.ElseStatement);
				}
			}
		}
		else if (node is WhileStatementSyntax { Statement: not null } whileStatementSyntax && !(whileStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, whileStatementSyntax.Statement);
		}
		else if (node is ForStatementSyntax { Statement: not null } forStatementSyntax && !(forStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, forStatementSyntax.Statement);
		}
		else if (node is ForEachStatementSyntax { Statement: not null } forEachStatementSyntax && !(forEachStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, forEachStatementSyntax.Statement);
		}
		else if (node is WithStatementSyntax { Statement: not null } withStatementSyntax && !(withStatementSyntax.Statement is BlockSyntax))
		{
			AddEmbeddedStatementsIndentationOperation(list, withStatementSyntax.Statement);
		}
		else
		{
			if (!(node is RepeatStatementSyntax repeatStatementSyntax) || repeatStatementSyntax.Parent.Kind != SyntaxKind.CaseLine)
			{
				return;
			}
			SyntaxList<StatementSyntax>.Enumerator enumerator = repeatStatementSyntax.Statements.GetEnumerator();
			while (enumerator.MoveNext())
			{
				StatementSyntax current = enumerator.Current;
				if (current != null && !(current is BlockSyntax))
				{
					AddEmbeddedStatementsIndentationOperation(list, current);
				}
			}
		}
	}

	private void AddEmbeddedStatementsIndentationOperation(List<IndentBlockOperation> list, SyntaxNode statement, SyntaxToken baseToken = default(SyntaxToken))
	{
		SyntaxToken firstToken = statement.GetFirstToken(includeZeroWidth: true);
		SyntaxToken lastToken = statement.GetLastToken(includeZeroWidth: true);
		if (lastToken.IsMissing)
		{
			if (baseToken.IsKind(SyntaxKind.None))
			{
				AddIndentBlockOperation(list, firstToken, lastToken);
			}
			else
			{
				AddIndentBlockOperation(list, baseToken, firstToken, lastToken);
			}
		}
		else if (baseToken.IsKind(SyntaxKind.None))
		{
			AddIndentBlockOperation(list, firstToken, lastToken, TextSpan.FromBounds(firstToken.FullSpan.Start, lastToken.FullSpan.End));
		}
		else
		{
			AddIndentBlockOperation(list, baseToken, firstToken, lastToken, TextSpan.FromBounds(firstToken.FullSpan.Start, lastToken.FullSpan.End));
		}
	}
}
