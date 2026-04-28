using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public class MemberSyntaxContext : AbstractSyntaxContext
{
	internal GeneralContexts General { get; }

	internal PageContexts Page { get; }

	internal TableContexts Table { get; }

	internal ReportContexts Report { get; }

	internal XmlPortContexts XmlPort { get; }

	internal QueryContexts Query { get; }

	internal PropertyExpressionContexts PropertyExpressionContexts { get; }

	private MemberSyntaxContext(Workspace workspace, SemanticModel semanticModel, int position, SyntaxToken leftToken, SyntaxToken targetToken, ObjectSyntax declaringObject, bool isRightOfDot, bool isRightOfColonColon, bool isInNonUserCode, GeneralContexts general, PageContexts page, TableContexts table, ReportContexts report, XmlPortContexts xmlport, QueryContexts query, PropertyExpressionContexts propertyExpressionContexts)
		: base(workspace, semanticModel, position, leftToken, targetToken, declaringObject, isRightOfDot, isRightOfColonColon, isInNonUserCode)
	{
		General = general;
		Page = page;
		Table = table;
		Report = report;
		XmlPort = xmlport;
		Query = query;
		PropertyExpressionContexts = propertyExpressionContexts;
	}

	public static MemberSyntaxContext CreateContext(Workspace workspace, SemanticModel semanticModel, int position, CancellationToken cancellationToken)
	{
		GeneralContexts targetEnum = GeneralContexts.None;
		PageContexts pageContext = PageContexts.None;
		TableContexts tableContext = TableContexts.None;
		ReportContexts reportContext = ReportContexts.None;
		XmlPortContexts xmlPortContext = XmlPortContexts.None;
		QueryContexts queryContext = QueryContexts.None;
		PropertyExpressionContexts propertyContexts = PropertyExpressionContexts.None;
		SyntaxTree syntaxTree = semanticModel.SyntaxTree;
		ObjectSyntax declaringObject = syntaxTree.GetDeclaringObject(position, cancellationToken);
		SyntaxToken syntaxToken = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTreeExtensions.FindTokenOnLeftOfPosition(syntaxTree, position, cancellationToken);
		SyntaxToken previousTokenIfTouchingWord = syntaxToken.GetPreviousTokenIfTouchingWord(position);
		bool isInNonUserCode = syntaxTree.IsInNonUserCode(position, cancellationToken);
		bool isRightOfDot = syntaxTree.IsRightOfDot(position, cancellationToken);
		bool isRightOfColonColon = syntaxTree.IsRightOfColonColon(position, cancellationToken);
		SetFlagIfTrue(ref targetEnum, GeneralContexts.PropertyDeclaration, syntaxToken.IsPropertyDeclarationContext());
		SetFlagIfTrue(ref targetEnum, GeneralContexts.MemberName, syntaxToken.IsMemberNamingContext());
		SetFlagIfTrue(ref targetEnum, GeneralContexts.Interface, syntaxToken.IsInterfaceCastingContext());
		switch (declaringObject?.Kind)
		{
		case SyntaxKind.RequestPage:
		case SyntaxKind.RequestPageExtension:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			SetPageContexts(previousTokenIfTouchingWord, ref pageContext);
			if (declaringObject.IsKind(SyntaxKind.PageExtensionObject, SyntaxKind.PageCustomizationObject, SyntaxKind.RequestPageExtension) || pageContext.HasFlag(PageContexts.View))
			{
				SetFlagIfTrue(ref pageContext, PageContexts.PageChangeAnchor, syntaxTree.IsPageChangeAnchorContext(position, cancellationToken));
			}
			break;
		case SyntaxKind.TableObject:
		case SyntaxKind.TableExtensionObject:
			SetTableContexts(previousTokenIfTouchingWord, ref tableContext);
			if (declaringObject.Kind == SyntaxKind.TableExtensionObject)
			{
				SetFlagIfTrue(ref tableContext, TableContexts.ModifyContext, syntaxTree.IsPageChangeAnchorContext(position, cancellationToken));
			}
			break;
		case SyntaxKind.CodeunitObject:
			SetFlagIfTrue(ref targetEnum, GeneralContexts.Implements, IsTokenInCodeunitImplementsNodeContext(previousTokenIfTouchingWord));
			break;
		case SyntaxKind.EnumType:
			SetFlagIfTrue(ref targetEnum, GeneralContexts.Implements, IsTokenInEnumImplementsNodeContext(previousTokenIfTouchingWord));
			break;
		case SyntaxKind.Interface:
			SetFlagIfTrue(ref targetEnum, GeneralContexts.Interface, IsTokenInInterfaceExtendsNodeContext(previousTokenIfTouchingWord));
			break;
		case SyntaxKind.ReportObject:
		case SyntaxKind.ReportExtensionObject:
			SetReportAndReportExtensionContexts(previousTokenIfTouchingWord, ref reportContext);
			break;
		case SyntaxKind.XmlPortObject:
			SetXmlPortContexts(previousTokenIfTouchingWord, ref xmlPortContext);
			break;
		case SyntaxKind.QueryObject:
			SetQueryContexts(previousTokenIfTouchingWord, ref queryContext);
			break;
		}
		if (!targetEnum.HasFlag(GeneralContexts.PropertyDeclaration))
		{
			SetFlagIfTrue(ref targetEnum, GeneralContexts.PropertyValue, syntaxToken.IsPropertyValueContext());
			if (targetEnum.HasFlag(GeneralContexts.PropertyValue))
			{
				SetFlagIfTrue(ref targetEnum, GeneralContexts.AnyComplexPropertyExpression, IsPropertyExpressionContext(previousTokenIfTouchingWord));
				SetDestinationPropertyExpressionContexts(previousTokenIfTouchingWord, isRightOfDot, ref propertyContexts);
			}
			if (!targetEnum.HasFlag(GeneralContexts.AnyComplexPropertyExpression))
			{
				SetFlagIfTrue(ref targetEnum, GeneralContexts.AnyExpression, syntaxTree.IsExpressionContext(position, syntaxToken, attributes: true, cancellationToken, semanticModel));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.Statement, previousTokenIfTouchingWord.IsBeginningOfStatementContext());
			}
			if (!targetEnum.HasFlag(GeneralContexts.PropertyValue))
			{
				SetFlagIfTrue(ref targetEnum, GeneralContexts.ApplicationObject, syntaxTree.IsApplicationObjectContext(position, syntaxToken, cancellationToken));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.Extends, IsTokenInExtendsContext(previousTokenIfTouchingWord));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.Namespace, IsTokenInNamespaceContext(previousTokenIfTouchingWord));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.Trigger, IsTokenInTriggerContext(previousTokenIfTouchingWord));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.EventTrigger, IsTokenInEventTriggerContext(previousTokenIfTouchingWord));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.Type, syntaxTree.IsTypeContext(position, previousTokenIfTouchingWord, cancellationToken));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.AttributeName, IsTokenInAttributeNameContext(previousTokenIfTouchingWord));
				SetFlagIfTrue(ref targetEnum, GeneralContexts.AttributeArgumentList, IsTokenInAttributeArgumentListContext(previousTokenIfTouchingWord));
				if (targetEnum.HasFlag(GeneralContexts.AttributeArgumentList) && syntaxTree.IsEntirelyWithinStringLiteral(position, cancellationToken))
				{
					isInNonUserCode = false;
				}
			}
			SetFlagIfTrue(ref targetEnum, GeneralContexts.ReferenceMemberList, (!targetEnum.HasFlag(GeneralContexts.PropertyValue)) ? syntaxTree.IsReferenceMemberListContext(position, cancellationToken) : IsSortingExpressionContext(previousTokenIfTouchingWord));
		}
		return new MemberSyntaxContext(workspace, semanticModel, position, syntaxToken, previousTokenIfTouchingWord, declaringObject, isRightOfDot, isRightOfColonColon, isInNonUserCode, targetEnum, pageContext, tableContext, reportContext, xmlPortContext, queryContext, propertyContexts);
	}

	private static bool IsSortingExpressionContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken))
		{
			return targetToken.ParentIsKind(SyntaxKind.SortingExpression);
		}
		return false;
	}

	private static void SetDestinationPropertyExpressionContexts(SyntaxToken targetToken, bool isRightOfDot, ref PropertyExpressionContexts propertyContexts)
	{
		SetFlagIfTrue(ref propertyContexts, PropertyExpressionContexts.DestinationTable, IsDestinationTablePropertyExpressionContext(targetToken));
		SetFlagIfTrue(ref propertyContexts, PropertyExpressionContexts.DestinationField, IsDestinationFieldPropertyExpressionContext(targetToken, isRightOfDot));
	}

	private static bool IsDestinationFieldPropertyExpressionContext(SyntaxToken targetToken, bool isRightOfDot)
	{
		if (!targetToken.Parent.TryGetAncestorOfKind<PropertySyntax>(SyntaxKind.Property, out PropertySyntax ancestor))
		{
			return false;
		}
		switch (ancestor.Value.Kind)
		{
		default:
			return false;
		case SyntaxKind.TableRelationStatement:
		case SyntaxKind.ExistCalculationFormulaStatement:
		case SyntaxKind.CountCalculationFormulaStatement:
		case SyntaxKind.SumCalculationFormulaStatement:
		case SyntaxKind.AverageCalculationFormulaStatement:
		case SyntaxKind.MinCalculationFormulaStatement:
		case SyntaxKind.MaxCalculationFormulaStatement:
		case SyntaxKind.LookupCalculationFormulaStatement:
		case SyntaxKind.QueryDataItemLinkPropertyValue:
		case SyntaxKind.ReportDataItemLinkPropertyValue:
			if (isRightOfDot)
			{
				return true;
			}
			if (targetToken.IsKind(SyntaxKind.CloseParenToken) && targetToken.ParentIsKind(SyntaxKind.TableFilterExpression))
			{
				return true;
			}
			if (targetToken.IsKind(SyntaxKind.EqualsToken) && targetToken.ParentIsKind(SyntaxKind.Property))
			{
				ancestor = (PropertySyntax)targetToken.Parent;
				PropertyValueSyntax value = ancestor.Value;
				if (value == null)
				{
					return false;
				}
				return value.Kind == SyntaxKind.TableRelationStatement;
			}
			if (targetToken.IsKind(SyntaxKind.ElseKeyword) && targetToken.ParentIsKind(SyntaxKind.ElseTableRelationExpression))
			{
				return true;
			}
			if (targetToken.IsKind(SyntaxKind.OpenParenToken) && targetToken.ParentIsKind(SyntaxKind.SumCalculationFormulaStatement, SyntaxKind.AverageCalculationFormulaStatement, SyntaxKind.MinCalculationFormulaStatement, SyntaxKind.MaxCalculationFormulaStatement, SyntaxKind.LookupCalculationFormulaStatement))
			{
				return true;
			}
			return false;
		}
	}

	private static bool IsDestinationTablePropertyExpressionContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenParenToken) && targetToken.ParentIsKind(SyntaxKind.ExistCalculationFormulaStatement, SyntaxKind.CountCalculationFormulaStatement))
		{
			return true;
		}
		return false;
	}

	private static void SetTableContexts(SyntaxToken targetToken, ref TableContexts tableContext)
	{
		SetFlagIfTrue(ref tableContext, TableContexts.TopLevelTable, IsTokenInTopLevelTableContext(targetToken));
		SetFlagIfTrue(ref tableContext, TableContexts.Fields, IsTokenInFieldsContext(targetToken));
		SetFlagIfTrue(ref tableContext, TableContexts.Keys, IsTokenInKeysContext(targetToken));
		SetFlagIfTrue(ref tableContext, TableContexts.FieldGroup, IsTokenInFieldGroupContext(targetToken));
		SetFlagIfTrue(ref tableContext, TableContexts.Field, IsTokenInFieldContext(targetToken));
	}

	private static void SetPageContexts(SyntaxToken targetToken, ref PageContexts pageContext)
	{
		SetFlagIfTrue(ref pageContext, PageContexts.TopLevelPage, IsTokenInTopLevelPageContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.ControlGroup, IsTokenInControlGroupContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.ActionGroup, IsTokenInActionGroupContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.Control, IsTokenInControlContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.Action, IsTokenInActionContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.View, IsTokenInPageViewsContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.Area, IsTokenInAreaNameContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.PartPage, IsTokenInPartPageContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.SystemPartPage, IsTokenInSystemPartPageContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.PageLayout, IsTokenInPageLayoutContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.PageActions, IsTokenInPageActionsContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.ControlAddIn, IsTokenInControlAddInContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.ActionRef, IsTokenInActionRefContext(targetToken));
		SetFlagIfTrue(ref pageContext, PageContexts.SystemAction, IsTokenInSystemActionContext(targetToken));
	}

	private static void SetReportAndReportExtensionContexts(SyntaxToken targetToken, ref ReportContexts reportContext)
	{
		SetFlagIfTrue(ref reportContext, ReportContexts.DataItemSource, IsTokenInDataItemSourceContext(targetToken));
	}

	private static void SetXmlPortContexts(SyntaxToken targetToken, ref XmlPortContexts xmlPortContext)
	{
		SetFlagIfTrue(ref xmlPortContext, XmlPortContexts.TableNodeSource, IsTokenInXmlPortTableNodeSourceContext(targetToken));
		SetFlagIfTrue(ref xmlPortContext, XmlPortContexts.FieldNodeSource, IsTokenInXmlPortFieldNodeSourceContext(targetToken));
	}

	private static void SetQueryContexts(SyntaxToken targetToken, ref QueryContexts queryContext)
	{
		SetFlagIfTrue(ref queryContext, QueryContexts.DataItemSource, IsTokenInDataItemSourceContext(targetToken));
		SetFlagIfTrue(ref queryContext, QueryContexts.ColumnOrFilterSource, IsTokenInQueryElementSourceContext(targetToken));
	}

	private static bool IsTokenInTopLevelTableContext(SyntaxToken targetToken)
	{
		return IsTokenInTopLevelObjectContext(targetToken, SyntaxKind.TableObject);
	}

	private static bool IsTokenInTopLevelPageContext(SyntaxToken targetToken)
	{
		return IsTokenInTopLevelObjectContext(targetToken, SyntaxKind.PageObject);
	}

	private static bool IsTokenInTopLevelObjectContext(SyntaxToken targetToken, SyntaxKind topLevelObject)
	{
		if (targetToken.ParentIsKind(topLevelObject))
		{
			return true;
		}
		if (targetToken.ParentIsKind(SyntaxKind.Property))
		{
			SyntaxNode parent = targetToken.Parent;
			if (parent != null && (parent.Parent?.ParentIsKind(topLevelObject)).GetValueOrDefault())
			{
				return true;
			}
		}
		if (targetToken.Kind == SyntaxKind.CloseBraceToken)
		{
			SyntaxNode parent2 = targetToken.Parent;
			if (parent2 != null && parent2.ParentIsKind(topLevelObject))
			{
				return true;
			}
		}
		if (targetToken.Kind == SyntaxKind.SemicolonToken)
		{
			SyntaxNode parent3 = targetToken.Parent;
			if (parent3 != null && (parent3.Parent?.ParentIsKind(topLevelObject)).GetValueOrDefault())
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsTokenInKeysContext(SyntaxToken targetToken)
	{
		if ((targetToken.Kind == SyntaxKind.OpenBraceToken && targetToken.ParentIsKind(SyntaxKind.KeyList)) || (targetToken.Kind == SyntaxKind.CloseBraceToken && targetToken.ParentIsKind(SyntaxKind.Key)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInFieldGroupContext(SyntaxToken targetToken)
	{
		if ((targetToken.Kind == SyntaxKind.OpenBraceToken && targetToken.ParentIsKind(SyntaxKind.FieldGroupList)) || (targetToken.Kind == SyntaxKind.CloseBraceToken && targetToken.ParentIsKind(SyntaxKind.FieldGroup)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInFieldsContext(SyntaxToken targetToken)
	{
		if ((targetToken.Kind == SyntaxKind.OpenBraceToken && targetToken.ParentIsKind(SyntaxKind.FieldList)) || (targetToken.Kind == SyntaxKind.CloseBraceToken && targetToken.ParentIsKind(SyntaxKind.Field)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInPageLayoutContext(SyntaxToken targetToken)
	{
		if ((targetToken.Kind == SyntaxKind.OpenBraceToken && targetToken.ParentIsKind(SyntaxKind.PageLayout)) || (targetToken.Kind == SyntaxKind.CloseBraceToken && targetToken.ParentIsKind(SyntaxKind.PageArea)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInPageActionsContext(SyntaxToken targetToken)
	{
		if ((targetToken.Kind == SyntaxKind.OpenBraceToken && targetToken.ParentIsKind(SyntaxKind.PageActionList)) || (targetToken.Kind == SyntaxKind.CloseBraceToken && targetToken.ParentIsKind(SyntaxKind.PageActionArea)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInPageViewsContext(SyntaxToken targetToken)
	{
		if (Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PageViewSyntax>(targetToken) != null)
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInExtendsContext(SyntaxToken targetToken)
	{
		if (targetToken.Parent == null)
		{
			return false;
		}
		if (targetToken.IsKind(SyntaxKind.ExtendsKeyword, SyntaxKind.CustomizesKeyword) && targetToken.Parent.Kind.IsApplicationObjectExtensionSyntax())
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.ExtendsKeyword) && targetToken.Parent.Kind == SyntaxKind.Interface)
		{
			return true;
		}
		if (!targetToken.ParentIsKind(SyntaxKind.QualifiedName, SyntaxKind.IdentifierName))
		{
			return false;
		}
		SyntaxNode parent = targetToken.Parent;
		if (!parent.ParentIsKind(SyntaxKind.ObjectReference))
		{
			return false;
		}
		if (parent.Parent.Parent.Kind.IsApplicationObjectExtensionSyntax())
		{
			return ((ApplicationObjectExtensionSyntax)parent.Parent.Parent)?.BaseObject == parent.Parent;
		}
		return false;
	}

	private static bool IsTokenInNamespaceContext(SyntaxToken targetToken)
	{
		if (targetToken.Parent == null)
		{
			return false;
		}
		SyntaxNode parent = targetToken.Parent;
		while (parent.Kind == SyntaxKind.QualifiedName)
		{
			parent = parent.Parent;
		}
		return parent.IsKind(SyntaxKind.NamespaceDeclaration, SyntaxKind.UsingDirective);
	}

	private static bool IsTokenInDataItemSourceContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.ReportDataItem, SyntaxKind.QueryDataItem))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.DotToken))
		{
			if (targetToken.TryGetAncestorOfKind<ReportDataItemSyntax>(SyntaxKind.ReportDataItem, out ReportDataItemSyntax ancestor))
			{
				return ancestor.DataItemTable.FullSpan.Contains(targetToken.Position);
			}
			if (targetToken.TryGetAncestorOfKind<QueryDataItemSyntax>(SyntaxKind.QueryDataItem, out QueryDataItemSyntax ancestor2))
			{
				return ancestor2.DataItemTable.FullSpan.Contains(targetToken.Position);
			}
		}
		return false;
	}

	private static bool IsTokenInXmlPortTableNodeSourceContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.XmlPortTableElement))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.DotToken) && targetToken.TryGetAncestorOfKind<XmlPortTableElementSyntax>(SyntaxKind.XmlPortTableElement, out XmlPortTableElementSyntax ancestor))
		{
			return ancestor.SourceTable.FullSpan.Contains(targetToken.Position);
		}
		return false;
	}

	private static bool IsTokenInCodeunitImplementsNodeContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.CodeunitObject))
		{
			return ((CodeunitSyntax)targetToken.Parent).Interfaces.FullSpan.Contains(targetToken.Position);
		}
		if (targetToken.IsKind(SyntaxKind.DotToken) && targetToken.TryGetAncestorOfKind<CodeunitSyntax>(SyntaxKind.CodeunitObject, out CodeunitSyntax ancestor))
		{
			return ancestor.Interfaces.FullSpan.Contains(targetToken.Position);
		}
		return targetToken.IsKind(SyntaxKind.ImplementsKeyword);
	}

	private static bool IsTokenInEnumImplementsNodeContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.EnumType))
		{
			return ((EnumTypeSyntax)targetToken.Parent).Interfaces.FullSpan.Contains(targetToken.Position);
		}
		if (targetToken.IsKind(SyntaxKind.DotToken) && targetToken.TryGetAncestorOfKind<EnumTypeSyntax>(SyntaxKind.EnumType, out EnumTypeSyntax ancestor))
		{
			return ancestor.Interfaces.FullSpan.Contains(targetToken.Position);
		}
		return targetToken.IsKind(SyntaxKind.ImplementsKeyword);
	}

	private static bool IsTokenInInterfaceExtendsNodeContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.CommaToken) && targetToken.ParentIsKind(SyntaxKind.Interface))
		{
			return ((InterfaceSyntax)targetToken.Parent).ExtendsInterfaces.FullSpan.Contains(targetToken.Position);
		}
		if (targetToken.IsKind(SyntaxKind.DotToken) && targetToken.TryGetAncestorOfKind<InterfaceSyntax>(SyntaxKind.Interface, out InterfaceSyntax ancestor))
		{
			return ancestor.ExtendsInterfaces.FullSpan.Contains(targetToken.Position);
		}
		return targetToken.IsKind(SyntaxKind.ExtendsKeyword);
	}

	private static bool IsTokenInXmlPortFieldNodeSourceContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.SemicolonToken))
		{
			return false;
		}
		if (targetToken.Parent.Kind != SyntaxKind.XmlPortFieldAttribute)
		{
			return targetToken.Parent.Kind == SyntaxKind.XmlPortFieldElement;
		}
		return true;
	}

	private static bool IsTokenInQueryElementSourceContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.SemicolonToken))
		{
			return false;
		}
		if (targetToken.Parent.Kind != SyntaxKind.QueryFilter)
		{
			return targetToken.Parent.Kind == SyntaxKind.QueryColumn;
		}
		return true;
	}

	private static bool IsTokenInPartPageContext(SyntaxToken targetToken)
	{
		if (!targetToken.TryGetAncestorOfKind<PagePartSyntax>(SyntaxKind.PagePart, out PagePartSyntax ancestor))
		{
			return false;
		}
		if (!targetToken.IsKind(SyntaxKind.SemicolonToken) && ancestor.PartName != null)
		{
			return ancestor.PartName.Span.Contains(targetToken.Position);
		}
		return true;
	}

	private static bool IsTokenInControlAddInContext(SyntaxToken targetToken)
	{
		if (!targetToken.TryGetAncestorOfKind<PageUserControlSyntax>(SyntaxKind.PageUserControl, out PageUserControlSyntax ancestor))
		{
			return false;
		}
		if (!targetToken.IsKind(SyntaxKind.SemicolonToken) && ancestor.ControlAddIn != null)
		{
			return ancestor.ControlAddIn.Span.Contains(targetToken.Position);
		}
		return true;
	}

	private static bool IsTokenInActionRefContext(SyntaxToken targetToken)
	{
		return targetToken.Parent.Kind == SyntaxKind.PageActionRef;
	}

	private static bool IsTokenInSystemActionContext(SyntaxToken targetToken)
	{
		return targetToken.Parent.Kind == SyntaxKind.PageSystemAction;
	}

	private static bool IsTokenInSystemPartPageContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.SemicolonToken))
		{
			return false;
		}
		return targetToken.Parent.Kind == SyntaxKind.PageSystemPart;
	}

	private static bool IsTokenInEventTriggerContext(SyntaxToken targetToken)
	{
		if (!targetToken.ParentIsKind(SyntaxKind.EventTriggerDeclaration))
		{
			return false;
		}
		if (!targetToken.IsKind(SyntaxKind.ColonColonToken))
		{
			return false;
		}
		return targetToken.Parent.Parent?.Kind.IsObject() ?? false;
	}

	private static bool IsTokenInTriggerContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.TriggerKeyword))
		{
			return false;
		}
		if (!targetToken.ParentIsKind(SyntaxKind.TriggerDeclaration))
		{
			return false;
		}
		return targetToken.Parent.Parent?.Kind.CanDefineTriggers() ?? false;
	}

	private static bool IsTokenInAreaNameContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.OpenParenToken))
		{
			return false;
		}
		if (!targetToken.ParentIsKind(SyntaxKind.PageArea))
		{
			return targetToken.ParentIsKind(SyntaxKind.PageActionArea);
		}
		return true;
	}

	private static bool IsTokenInControlGroupContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenBraceToken) && (targetToken.ParentIsKind(SyntaxKind.PageArea) || targetToken.ParentIsKind(SyntaxKind.PageGroup)))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Property) && (targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageArea) || targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageGroup)))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.CloseBraceToken) && (targetToken.ParentIsKind(SyntaxKind.PageGroup) || targetToken.ParentIsKind(SyntaxKind.PageField)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInFieldContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenBraceToken) && targetToken.ParentIsKind(SyntaxKind.Field))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Property) && targetToken.Parent.Parent.ParentIsKind(SyntaxKind.Field))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInControlContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenBraceToken) && targetToken.ParentIsKind(SyntaxKind.PageField))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Property) && targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageField))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInActionGroupContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenBraceToken) && (targetToken.ParentIsKind(SyntaxKind.PageActionArea) || targetToken.ParentIsKind(SyntaxKind.PageActionGroup)))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Property) && (targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageActionArea) || targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageActionGroup)))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.CloseBraceToken) && (targetToken.ParentIsKind(SyntaxKind.PageActionGroup) || targetToken.ParentIsKind(SyntaxKind.PageAction) || targetToken.ParentIsKind(SyntaxKind.PageActionSeparator)))
		{
			return true;
		}
		return false;
	}

	private static bool IsTokenInActionContext(SyntaxToken targetToken)
	{
		if (targetToken.IsKind(SyntaxKind.OpenBraceToken) && targetToken.ParentIsKind(SyntaxKind.PageAction, SyntaxKind.PageSystemAction))
		{
			return true;
		}
		if (targetToken.IsKind(SyntaxKind.SemicolonToken) && targetToken.ParentIsKind(SyntaxKind.Property) && (targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageAction) || targetToken.Parent.Parent.ParentIsKind(SyntaxKind.PageSystemAction)))
		{
			return true;
		}
		return false;
	}

	private static void SetFlagIfTrue(ref GeneralContexts targetEnum, GeneralContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static void SetFlagIfTrue(ref PageContexts targetEnum, PageContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static void SetFlagIfTrue(ref PropertyExpressionContexts targetEnum, PropertyExpressionContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static bool IsTokenInAttributeNameContext(SyntaxToken targetToken)
	{
		if (!targetToken.IsKind(SyntaxKind.OpenBracketToken))
		{
			return false;
		}
		if (!targetToken.ParentIsKind(SyntaxKind.MemberAttribute))
		{
			return false;
		}
		if (!targetToken.Parent.ParentIsKind(SyntaxKind.MethodDeclaration) && !targetToken.Parent.ParentIsKind(SyntaxKind.VariableDeclaration) && !targetToken.Parent.ParentIsKind(SyntaxKind.VariableListDeclaration))
		{
			return targetToken.Parent.ParentIsKind(SyntaxKind.EventDeclaration);
		}
		return true;
	}

	private static bool IsTokenInAttributeArgumentListContext(SyntaxToken targetToken)
	{
		return FindAttributeArgumentListContext(targetToken) != null;
	}

	internal static AttributeArgumentListSyntax FindAttributeArgumentListContext(SyntaxToken targetToken)
	{
		SyntaxNode syntaxNode = targetToken.Parent;
		if (syntaxNode == null)
		{
			return null;
		}
		if (syntaxNode.IsKind(SyntaxKind.SkippedTokensTrivia))
		{
			syntaxNode = syntaxNode.ParentOrStructuredTriviaParent;
		}
		SyntaxNode syntaxNode2 = null;
		if (syntaxNode.IsKind(SyntaxKind.AttributeArgumentList))
		{
			syntaxNode2 = syntaxNode;
		}
		else if (syntaxNode.ParentIsKind(SyntaxKind.AttributeArgumentList))
		{
			syntaxNode2 = syntaxNode.Parent;
		}
		else
		{
			SyntaxNode parent = syntaxNode.Parent;
			if (parent != null && parent.ParentIsKind(SyntaxKind.AttributeArgumentList))
			{
				syntaxNode2 = syntaxNode.Parent?.Parent;
			}
			else
			{
				SyntaxNode parent2 = syntaxNode.Parent;
				if (parent2 != null && (parent2.Parent?.ParentIsKind(SyntaxKind.AttributeArgumentList)).GetValueOrDefault())
				{
					syntaxNode2 = syntaxNode.Parent.Parent.Parent;
				}
			}
		}
		return syntaxNode2 as AttributeArgumentListSyntax;
	}

	internal static bool IsPropertyExpressionContext(SyntaxToken token)
	{
		if (token.Parent == null)
		{
			return false;
		}
		PropertyValueSyntax propertyValueSyntax = token.Parent.FirstAncestorOrSelf<PropertySyntax>()?.Value;
		if (propertyValueSyntax == null || propertyValueSyntax.Kind.IsPropertyContextSourceContext())
		{
			return false;
		}
		if (token.Parent.FirstAncestorOrSelf<PropertyExpressionSyntax>() != null)
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken) && token.ParentIsKind(SyntaxKind.SortingExpression))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.ConstExpression, SyntaxKind.SimpleFieldExpression) && token.ParentIsKind(SyntaxKind.EqualsToken))
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.EqualsToken, SyntaxKind.CommaToken) && propertyValueSyntax.Kind == SyntaxKind.TableFilterPropertyValue)
		{
			return true;
		}
		if (token.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken) && token.ParentIsKind(SyntaxKind.OrderByExpression))
		{
			return true;
		}
		if (IsInsideDataItemLinkProperty(token, propertyValueSyntax))
		{
			return true;
		}
		if (IsInsideQueryColumnFilterProperty(token, propertyValueSyntax))
		{
			return true;
		}
		return false;
	}

	private static bool IsInsideDataItemLinkProperty(SyntaxToken token, PropertyValueSyntax propertyValueSyntax)
	{
		if (token.IsKind(SyntaxKind.EqualsToken, SyntaxKind.CommaToken))
		{
			if (propertyValueSyntax.Kind != SyntaxKind.QueryDataItemLinkPropertyValue && !token.ParentIsKind(SyntaxKind.QueryDataItemLinkExpression) && propertyValueSyntax.Kind != SyntaxKind.ReportDataItemLinkPropertyValue)
			{
				return token.ParentIsKind(SyntaxKind.ReportDataItemLinkExpression);
			}
			return true;
		}
		return false;
	}

	private static bool IsInsideQueryColumnFilterProperty(SyntaxToken token, PropertyValueSyntax propertyValueSyntax)
	{
		SyntaxKind? syntaxKind = propertyValueSyntax?.Parent?.Parent?.Parent?.Kind;
		if (token.IsKind(SyntaxKind.EqualsToken, SyntaxKind.CommaToken) && propertyValueSyntax.Kind == SyntaxKind.TableFilterPropertyValue)
		{
			if (syntaxKind != SyntaxKind.QueryColumn)
			{
				return syntaxKind == SyntaxKind.QueryFilter;
			}
			return true;
		}
		return false;
	}

	private static void SetFlagIfTrue(ref TableContexts targetEnum, TableContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static void SetFlagIfTrue(ref ReportContexts targetEnum, ReportContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static void SetFlagIfTrue(ref XmlPortContexts targetEnum, XmlPortContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	private static void SetFlagIfTrue(ref QueryContexts targetEnum, QueryContexts flag, bool condition)
	{
		if (condition)
		{
			targetEnum |= flag;
		}
	}

	protected internal IEnumerable<ISymbol> LookupSymbols(SymbolKind? kind, CancellationToken cancellationToken)
	{
		if (!kind.HasValue)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (kind.GetValueOrDefault() == SymbolKind.Undefined)
		{
			return base.SemanticModel.LookupSymbols(base.LeftToken.SpanStart, LookupOptions.Default, null, null, SymbolKind.Undefined, cancellationToken);
		}
		if (base.IsRightOfDot && base.LeftToken.Parent is QualifiedNameSyntax qualifiedNameSyntax)
		{
			NamespaceSymbol nestedNamespace = base.SemanticModel.Compilation.GlobalNamespace.GetNestedNamespace(qualifiedNameSyntax.Left);
			if (nestedNamespace == null)
			{
				return SpecializedCollections.EmptyEnumerable<ISymbol>();
			}
			return base.SemanticModel.LookupSymbols(base.LeftToken.SpanStart, LookupOptions.MustBeObjectTypeOrNamespaceSymbol, nestedNamespace, null, kind.Value, cancellationToken);
		}
		ImmutableArray<ISymbol> immutableArray = base.SemanticModel.LookupSymbols(base.LeftToken.SpanStart, LookupOptions.MustBeObjectTypeOrNamespaceSymbol, null, null, kind.Value, cancellationToken);
		INamespaceSymbol obj = base.SemanticModel.GetEnclosingSymbol(base.LeftToken.SpanStart, cancellationToken) as INamespaceSymbol;
		if (obj != null && obj.IsGlobalNamespace)
		{
			return immutableArray;
		}
		return immutableArray.Concat(base.SemanticModel.Compilation.GetObjectSymbolsByKindAcrossModules(kind.Value, accessibleOnly: true));
	}

	internal IEnumerable<ISymbol> LookupMemberSymbols(SymbolKind containerKind, SymbolKind memberKind, CancellationToken cancellationToken)
	{
		if (!base.IsRightOfNameSeparator)
		{
			return LookupSymbols(containerKind, cancellationToken);
		}
		SyntaxNode parent = base.TargetToken.Parent;
		NameSyntax nameSyntax = null;
		switch (parent.Kind)
		{
		case SyntaxKind.MemberAccessExpression:
			nameSyntax = ((MemberAccessExpressionSyntax)parent).Expression as NameSyntax;
			break;
		case SyntaxKind.QualifiedName:
			nameSyntax = ((QualifiedNameSyntax)parent).Left;
			break;
		}
		if (nameSyntax == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ContainerSymbol containerSymbol = base.SemanticModel.GetEnclosingBinder(base.TargetToken.Position).BindNamespaceOrType(nameSyntax, containerKind, null);
		if (containerSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (containerSymbol.Kind == SymbolKind.Namespace)
		{
			return base.SemanticModel.LookupSymbols(base.LeftToken.SpanStart, LookupOptions.MustBeObjectTypeOrNamespaceSymbol, containerSymbol, null, containerKind, cancellationToken);
		}
		return base.SemanticModel.LookupSymbols(base.LeftToken.SpanStart, LookupOptions.Default, containerSymbol, null, memberKind, cancellationToken);
	}
}
