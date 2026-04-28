using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[CompilerGenerated]
public static class SyntaxFacts
{
	public const string AddActionMethodName = "AddAction";

	public const int AddActionCodeunitIdArgumentPosition = 1;

	public const int AddActionMethodNameArgumentPosition = 2;

	public const int AttributeObjectTypeCommonPosition = 0;

	public const int AttributeObjectIdCommonPosition = 1;

	public const int EventSubscriberParameterAttributesLength = 6;

	public const int EventSubscriberObjectTypePosition = 0;

	public const int EventSubscriberObjectIdPosition = 1;

	public const int EventSubscriberEventNamePosition = 2;

	public const int EventSubscriberElementNamePosition = 3;

	public const int EventSubscriberSkipOnMissingLicense = 4;

	public const int EventSubscriberSkipOnMissingPermission = 5;

	public const int EventPublisherIncludeSenderPosition = 0;

	public const int EventPublisherEventIncludeGlobalsPosition = 1;

	public const int EventPublisherIsolatedPosition = 2;

	public const int EventPublisherIsolatedPositionIntegrationEvent = 3;

	public const int ObsoleteReasonPosition = 0;

	public const int ObsoleteTagPosition = 1;

	public const int ScopeNamePosition = 0;

	public const int CaptionTextPosition = 0;

	public const int CaptionLockedPosition = 1;

	public const int CaptionCommentPosition = 2;

	public const int CaptionMaxLengthPosition = 3;

	public const int InherentPermissionObjectTypePosition = 0;

	public const int InherentPermissionObjectIdPosition = 1;

	public const int InherentPermissionPermissionValuePosition = 2;

	public const int InherentPermissionScopePosition = 3;

	public const int InherentPermissionArgumentCount = 4;

	public const int ExternalBusinessEventNamePosition = 0;

	public const int ExternalBusinessEventDisplayNamePosition = 1;

	public const int ExternalBusinessEventDescriptionPosition = 2;

	public const int ExternalBusinessEventCategoryPosition = 3;

	public const int ExternalBusinessEventVersionPosition = 4;

	public const string ExternalBusinessEventName = "Name";

	public const string ExternalBusinessEventDisplayName = "Display Name";

	public const string ExternalBusinessEventDescription = "Description";

	public const string ExternalBusinessEventCategory = "Category";

	public const string ExternalBusinessEventVersion = "Version";

	public const string ExternalBusinessEventDefaultVersion = "0.0";

	public const string ExternalBusinessEventCategoryEnumObjectName = "EventCategory";

	public const int ExternalBusinessEventCategoryEnumObjectId = 2000000001;

	public const string ExternalBusinessEventCategoryEnumMinPlatformVersion = "22.0.0.0";

	public const string ExternalBusinessEventEmptyJsonArray = "[]";

	public const int ExternalBusinessEventNameMaxLength = 80;

	public const int ExternalBusinessEventDisplayNameMaxLength = 250;

	public const int ExternalBusinessEventDescriptionNameMaxLength = 1024;

	public const int RequiredPermissionsObjectTypePosition = 0;

	public const int RequiredPermissionsObjectIdPosition = 1;

	public const int RequiredPermissionsPermissionValuePosition = 2;

	public const int RequiredPermissionsArgumentCount = 3;

	public const char OptionMembersSeparatorChar = ',';

	public const char PromotedCategoriesSeparatorChar = ',';

	public const int MaximumAlIdentifierLength = 120;

	public const string SynthesizedActionRefSuffix = "_Promoted";

	public const string DefaultSummarySystemPartName = "DefaultSummaryPart";

	public const string ApplicationAreaAllValueText = "#All";

	public const string Space = " ";

	internal static readonly ImmutableArray<AreaKind> AreaValues = (from AreaKind v in typeof(AreaKind).GetEnumValues()
		where v != AreaKind.None
		select v).ToImmutableArray();

	public static readonly ImmutableArray<PromotedCategoryKind> DefaultPromotedCategories = typeof(PromotedCategoryKind).GetEnumValues().Cast<PromotedCategoryKind>().ToImmutableArray();

	public const string HomePromotedCategoryName = "Category_Process";

	public static readonly ImmutableDictionary<PromotedCategoryKind, string> DefaultPromotedCategoriesCaptions = new Dictionary<PromotedCategoryKind, string>
	{
		{
			PromotedCategoryKind.New,
			"New"
		},
		{
			PromotedCategoryKind.Process,
			"Process"
		},
		{
			PromotedCategoryKind.Report,
			"Reports"
		},
		{
			PromotedCategoryKind.Category4,
			"Category 4"
		},
		{
			PromotedCategoryKind.Category5,
			"Category 5"
		},
		{
			PromotedCategoryKind.Category6,
			"Category 6"
		},
		{
			PromotedCategoryKind.Category7,
			"Category 7"
		},
		{
			PromotedCategoryKind.Category8,
			"Category 8"
		},
		{
			PromotedCategoryKind.Category9,
			"Category 9"
		},
		{
			PromotedCategoryKind.Category10,
			"Category 10"
		},
		{
			PromotedCategoryKind.Category11,
			"Category 11"
		},
		{
			PromotedCategoryKind.Category12,
			"Category 12"
		},
		{
			PromotedCategoryKind.Category13,
			"Category 13"
		},
		{
			PromotedCategoryKind.Category14,
			"Category 14"
		},
		{
			PromotedCategoryKind.Category15,
			"Category 15"
		},
		{
			PromotedCategoryKind.Category16,
			"Category 16"
		},
		{
			PromotedCategoryKind.Category17,
			"Category 17"
		},
		{
			PromotedCategoryKind.Category18,
			"Category 18"
		},
		{
			PromotedCategoryKind.Category19,
			"Category 19"
		},
		{
			PromotedCategoryKind.Category20,
			"Category 20"
		}
	}.ToImmutableDictionary();

	public static Dictionary<string, int> PredefinedActionCategoryNames = new Dictionary<string, int>
	{
		{ "Category_New", 0 },
		{ "Category_Process", 1 },
		{ "Category_Report", 2 },
		{ "Category_Category4", 3 },
		{ "Category_Category5", 4 },
		{ "Category_Category6", 5 },
		{ "Category_Category7", 6 },
		{ "Category_Category8", 7 },
		{ "Category_Category9", 8 },
		{ "Category_Category10", 9 },
		{ "Category_Category11", 10 },
		{ "Category_Category12", 11 },
		{ "Category_Category13", 12 },
		{ "Category_Category14", 13 },
		{ "Category_Category15", 14 },
		{ "Category_Category16", 15 },
		{ "Category_Category17", 16 },
		{ "Category_Category18", 17 },
		{ "Category_Category19", 18 },
		{ "Category_Category20", 19 }
	};

	public static readonly ImmutableHashSet<string> PromotedCategoriesSynthesizedSymbolNames = DefaultPromotedCategories.Select((PromotedCategoryKind x) => SynthesizedActionSymbolHelper.GetSynthesizedPromotedCategoryKindName(x)).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);

	public static readonly ImmutableHashSet<string> PromotedActionPropertyNames = (from PropertyKind x in typeof(PropertyKind).GetEnumValues()
		where x.IsPromotedActionProperty()
		select x.ToString()).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);

	public static readonly ImmutableHashSet<string> PromotedActionCategoriesPropertyNames = (from PropertyKind x in typeof(PropertyKind).GetEnumValues()
		where x.IsAnyPromotedActionCategoriesProperty()
		select x.ToString()).ToImmutableHashSet(SemanticFacts.NameEqualityComparer);

	internal static readonly ImmutableArray<SyntaxKind> SupportedPermissionKeywordKinds;

	internal static readonly string SupportedPermissionApplicationObjectsString;

	internal static ImmutableArray<SyntaxKind> SupportedObjectSyntaxKeywords;

	public static string SupportedApplicationObjectsString;

	internal static ImmutableArray<SyntaxKind> SupportedApplicationObjectReferenceKeywordKinds;

	internal static string SupportedApplicationObjectReferencesString;

	internal static ImmutableArray<ActionAreaKind> ActionAreaValues;

	public const string RecFieldName = "Rec";

	internal const string OldRecFieldName = "xRec";

	internal const string CurrFieldNoFieldName = "CurrFieldNo";

	internal const string CurrPageFieldName = "CurrPage";

	internal const string CurrXmlPortFieldName = "currXMLport";

	internal const string ParentPageFieldName = "ParentPage";

	internal const string SecurityFilterEnumName = "SecurityFilter";

	internal const string CurrReportFieldName = "CurrReport";

	internal const string RequestPageName = "RequestOptionsPage";

	internal const string RequestPageExtensionName = "RequestPageExtension";

	internal const string CurrQueryFieldName = "CurrQuery";

	internal const string ReservedReportColumnNameSuffix = "Format";

	public const string SystemId = "SystemId";

	internal const string SystemIdMetadataName = "$systemId";

	public const string SystemCreatedAt = "SystemCreatedAt";

	public const string SystemCreatedBy = "SystemCreatedBy";

	public const string SystemModifiedAt = "SystemModifiedAt";

	public const string SystemModifiedBy = "SystemModifiedBy";

	internal const string SystemRowVersion = "SystemRowVersion";

	internal const string SystemRowVersionMetadataName = "timestamp";

	public static readonly ImmutableArray<SystemActionKind> AllSystemActions;

	public static readonly string AllSystemActionsString;

	public static readonly ImmutableArray<SystemActionKind> ConfigurationDialogSystemActions;

	public static readonly string ConfigurationDialogSystemActionsNamesString;

	internal const int MSReservedRange = 2000000000;

	internal const int SystemFieldSystemId = 2000000000;

	internal const int SystemFieldCreatedAtId = 2000000001;

	internal const int SystemFieldCreatedById = 2000000002;

	internal const int SystemFieldModifiedAtId = 2000000003;

	internal const int SystemFieldModifiedById = 2000000004;

	internal const int SystemFieldRowVersionId = 0;

	public static readonly ImmutableArray<SyntaxKind> RootObjectKinds;

	public static readonly ImmutableArray<SyntaxKind> KeywordsSyntaxKinds;

	public static readonly ImmutableArray<SyntaxKind> DataSyntaxKinds;

	public static string TryUseCachedPropertyName(string name)
	{
		return name switch
		{
			"SourceExpression" => "SourceExpression", 
			"SystemPartId" => "SystemPartId", 
			"Access" => "Access", 
			"Description" => "Description", 
			"CaptionML" => "CaptionML", 
			"Caption" => "Caption", 
			"FileUploadAction" => "FileUploadAction", 
			"ShowAs" => "ShowAs", 
			"Implementation" => "Implementation", 
			"Version" => "Version", 
			"Culture" => "Culture", 
			"PublicKeyToken" => "PublicKeyToken", 
			"IsControlAddIn" => "IsControlAddIn", 
			"InherentEntitlements" => "InherentEntitlements", 
			"InherentPermissions" => "InherentPermissions", 
			"TableNo" => "TableNo", 
			"Permissions" => "Permissions", 
			"Subtype" => "Subtype", 
			"SingleInstance" => "SingleInstance", 
			"TestIsolation" => "TestIsolation", 
			"RequiredTestIsolation" => "RequiredTestIsolation", 
			"EventSubscriberInstance" => "EventSubscriberInstance", 
			"TestPermissions" => "TestPermissions", 
			"TestHttpRequestPolicy" => "TestHttpRequestPolicy", 
			"TestType" => "TestType", 
			"Extensible" => "Extensible", 
			"DataClassification" => "DataClassification", 
			"AllowInCustomizations" => "AllowInCustomizations", 
			"DataPerCompany" => "DataPerCompany", 
			"MovedTo" => "MovedTo", 
			"MovedFrom" => "MovedFrom", 
			"LookupPageId" => "LookupPageId", 
			"DrillDownPageId" => "DrillDownPageId", 
			"DataCaptionFields" => "DataCaptionFields", 
			"PasteIsValid" => "PasteIsValid", 
			"LinkedObject" => "LinkedObject", 
			"LinkedInTransaction" => "LinkedInTransaction", 
			"TableType" => "TableType", 
			"CompressionType" => "CompressionType", 
			"ExternalName" => "ExternalName", 
			"ExternalSchema" => "ExternalSchema", 
			"Scope" => "Scope", 
			"ReplicateData" => "ReplicateData", 
			"ColumnStoreIndex" => "ColumnStoreIndex", 
			"AutoFormatType" => "AutoFormatType", 
			"AutoFormatExpression" => "AutoFormatExpression", 
			"BlankNumbers" => "BlankNumbers", 
			"BlankZero" => "BlankZero", 
			"MinValue" => "MinValue", 
			"MaxValue" => "MaxValue", 
			"MaskType" => "MaskType", 
			"NotBlank" => "NotBlank", 
			"CharAllowed" => "CharAllowed", 
			"DateFormula" => "DateFormula", 
			"ValuesAllowed" => "ValuesAllowed", 
			"OptionCaptionML" => "OptionCaptionML", 
			"OptionCaption" => "OptionCaption", 
			"ClosingDates" => "ClosingDates", 
			"DecimalPlaces" => "DecimalPlaces", 
			"AccessByPermission" => "AccessByPermission", 
			"ExtendedDatatype" => "ExtendedDatatype", 
			"Width" => "Width", 
			"SignDisplacement" => "SignDisplacement", 
			"CaptionClass" => "CaptionClass", 
			"InitValue" => "InitValue", 
			"FieldClass" => "FieldClass", 
			"CalcFormula" => "CalcFormula", 
			"TableRelation" => "TableRelation", 
			"Enabled" => "Enabled", 
			"Editable" => "Editable", 
			"SqlDataType" => "SqlDataType", 
			"ValidateTableRelation" => "ValidateTableRelation", 
			"TestTableRelation" => "TestTableRelation", 
			"Compressed" => "Compressed", 
			"AutoIncrement" => "AutoIncrement", 
			"SqlTimestamp" => "SqlTimestamp", 
			"OptionMembers" => "OptionMembers", 
			"ExternalType" => "ExternalType", 
			"ExternalAccess" => "ExternalAccess", 
			"OptionOrdinalValues" => "OptionOrdinalValues", 
			"Numeric" => "Numeric", 
			"OptimizeForTextSearch" => "OptimizeForTextSearch", 
			"SumIndexFields" => "SumIndexFields", 
			"MaintainSqlIndex" => "MaintainSqlIndex", 
			"MaintainSiftIndex" => "MaintainSiftIndex", 
			"Clustered" => "Clustered", 
			"SqlIndex" => "SqlIndex", 
			"Unique" => "Unique", 
			"IncludedFields" => "IncludedFields", 
			"InstructionalTextML" => "InstructionalTextML", 
			"InstructionalText" => "InstructionalText", 
			"HelpLink" => "HelpLink", 
			"ContextSensitiveHelpPage" => "ContextSensitiveHelpPage", 
			"AutoSplitKey" => "AutoSplitKey", 
			"CardPageId" => "CardPageId", 
			"DataCaptionExpression" => "DataCaptionExpression", 
			"InsertAllowed" => "InsertAllowed", 
			"ModifyAllowed" => "ModifyAllowed", 
			"DeleteAllowed" => "DeleteAllowed", 
			"SourceTable" => "SourceTable", 
			"SourceTableTemporary" => "SourceTableTemporary", 
			"SourceTableView" => "SourceTableView", 
			"ShowFilter" => "ShowFilter", 
			"SaveValues" => "SaveValues", 
			"LinksAllowed" => "LinksAllowed", 
			"MultipleNewLines" => "MultipleNewLines", 
			"PopulateAllFields" => "PopulateAllFields", 
			"PageType" => "PageType", 
			"IsPreview" => "IsPreview", 
			"PromptMode" => "PromptMode", 
			"DelayedInsert" => "DelayedInsert", 
			"RefreshOnActivate" => "RefreshOnActivate", 
			"PromotedActionCategoriesML" => "PromotedActionCategoriesML", 
			"PromotedActionCategories" => "PromotedActionCategories", 
			"ODataKeyFields" => "ODataKeyFields", 
			"AnalysisModeEnabled" => "AnalysisModeEnabled", 
			"APIVersion" => "APIVersion", 
			"EntityName" => "EntityName", 
			"EntitySetName" => "EntitySetName", 
			"APIGroup" => "APIGroup", 
			"APIPublisher" => "APIPublisher", 
			"ChangeTrackingAllowed" => "ChangeTrackingAllowed", 
			"QueryCategory" => "QueryCategory", 
			"AdditionalSearchTermsML" => "AdditionalSearchTermsML", 
			"AdditionalSearchTerms" => "AdditionalSearchTerms", 
			"DataAccessIntent" => "DataAccessIntent", 
			"EntityCaption" => "EntityCaption", 
			"EntitySetCaption" => "EntitySetCaption", 
			"EntityCaptionML" => "EntityCaptionML", 
			"EntitySetCaptionML" => "EntitySetCaptionML", 
			"ShowCaption" => "ShowCaption", 
			"MultiLine" => "MultiLine", 
			"HideValue" => "HideValue", 
			"Importance" => "Importance", 
			"Style" => "Style", 
			"ApplicationArea" => "ApplicationArea", 
			"Visible" => "Visible", 
			"StyleExpr" => "StyleExpr", 
			"ToolTipML" => "ToolTipML", 
			"ToolTip" => "ToolTip", 
			"RowSpan" => "RowSpan", 
			"ColumnSpan" => "ColumnSpan", 
			"ShowMandatory" => "ShowMandatory", 
			"Title" => "Title", 
			"QuickEntry" => "QuickEntry", 
			"Lookup" => "Lookup", 
			"DrillDown" => "DrillDown", 
			"AssistEdit" => "AssistEdit", 
			"Image" => "Image", 
			"ODataEDMType" => "ODataEDMType", 
			"NavigationPageId" => "NavigationPageId", 
			"GridLayout" => "GridLayout", 
			"CuegroupLayout" => "CuegroupLayout", 
			"IndentationColumn" => "IndentationColumn", 
			"IndentationControls" => "IndentationControls", 
			"FreezeColumn" => "FreezeColumn", 
			"ShowAsTree" => "ShowAsTree", 
			"TreeInitialState" => "TreeInitialState", 
			"FileUploadRowAction" => "FileUploadRowAction", 
			"Multiplicity" => "Multiplicity", 
			"SubPageView" => "SubPageView", 
			"SubPageLink" => "SubPageLink", 
			"UpdatePropagation" => "UpdatePropagation", 
			"Provider" => "Provider", 
			"RunPageMode" => "RunPageMode", 
			"Promoted" => "Promoted", 
			"PromotedIsBig" => "PromotedIsBig", 
			"PromotedOnly" => "PromotedOnly", 
			"PromotedCategory" => "PromotedCategory", 
			"Ellipsis" => "Ellipsis", 
			"ShortcutKey" => "ShortcutKey", 
			"RunObject" => "RunObject", 
			"RunPageView" => "RunPageView", 
			"RunPageLink" => "RunPageLink", 
			"RunPageOnRec" => "RunPageOnRec", 
			"InFooterBar" => "InFooterBar", 
			"Gesture" => "Gesture", 
			"IsHeader" => "IsHeader", 
			"ObsoleteState" => "ObsoleteState", 
			"ObsoleteReason" => "ObsoleteReason", 
			"ObsoleteTag" => "ObsoleteTag", 
			"AboutText" => "AboutText", 
			"AboutTextML" => "AboutTextML", 
			"AboutTitle" => "AboutTitle", 
			"AboutTitleML" => "AboutTitleML", 
			"CustomActionType" => "CustomActionType", 
			"FlowId" => "FlowId", 
			"FlowEnvironmentId" => "FlowEnvironmentId", 
			"FlowCaption" => "FlowCaption", 
			"FlowTemplateId" => "FlowTemplateId", 
			"FlowTemplateCategoryName" => "FlowTemplateCategoryName", 
			"AllowedFileExtensions" => "AllowedFileExtensions", 
			"AllowMultipleFiles" => "AllowMultipleFiles", 
			"Filters" => "Filters", 
			"OrderBy" => "OrderBy", 
			"SharedLayout" => "SharedLayout", 
			"DefinitionFile" => "DefinitionFile", 
			"TransactionType" => "TransactionType", 
			"DefaultFieldsValidation" => "DefaultFieldsValidation", 
			"DefaultNamespace" => "DefaultNamespace", 
			"Direction" => "Direction", 
			"Encoding" => "Encoding", 
			"FieldDelimiter" => "FieldDelimiter", 
			"FieldSeparator" => "FieldSeparator", 
			"FileName" => "FileName", 
			"Format" => "Format", 
			"FormatEvaluate" => "FormatEvaluate", 
			"InlineSchema" => "InlineSchema", 
			"Namespaces" => "Namespaces", 
			"PreserveWhiteSpace" => "PreserveWhiteSpace", 
			"RecordSeparator" => "RecordSeparator", 
			"TableSeparator" => "TableSeparator", 
			"TextEncoding" => "TextEncoding", 
			"UseDefaultNamespace" => "UseDefaultNamespace", 
			"UseLax" => "UseLax", 
			"UseRequestPage" => "UseRequestPage", 
			"XmlVersionNo" => "XmlVersionNo", 
			"LinkTable" => "LinkTable", 
			"LinkTableForceInsert" => "LinkTableForceInsert", 
			"LinkFields" => "LinkFields", 
			"RequestFilterFields" => "RequestFilterFields", 
			"RequestFilterHeadingML" => "RequestFilterHeadingML", 
			"RequestFilterHeading" => "RequestFilterHeading", 
			"UseTemporary" => "UseTemporary", 
			"AutoReplace" => "AutoReplace", 
			"AutoSave" => "AutoSave", 
			"AutoUpdate" => "AutoUpdate", 
			"CalcFields" => "CalcFields", 
			"TextType" => "TextType", 
			"AutoCalcField" => "AutoCalcField", 
			"FieldValidate" => "FieldValidate", 
			"Occurrence" => "Occurrence", 
			"MinOccurs" => "MinOccurs", 
			"MaxOccurs" => "MaxOccurs", 
			"XmlName" => "XmlName", 
			"NamespacePrefix" => "NamespacePrefix", 
			"Unbound" => "Unbound", 
			"UseSystemPrinter" => "UseSystemPrinter", 
			"EnableExternalImages" => "EnableExternalImages", 
			"EnableHyperlinks" => "EnableHyperlinks", 
			"AllowScheduling" => "AllowScheduling", 
			"ExcelLayoutMultipleDataSheets" => "ExcelLayoutMultipleDataSheets", 
			"MaximumDatasetSize" => "MaximumDatasetSize", 
			"MaximumDocumentCount" => "MaximumDocumentCount", 
			"ExecutionTimeout" => "ExecutionTimeout", 
			"FormatRegion" => "FormatRegion", 
			"EnableExternalAssemblies" => "EnableExternalAssemblies", 
			"ProcessingOnly" => "ProcessingOnly", 
			"ShowPrintStatus" => "ShowPrintStatus", 
			"PaperSourceFirstPage" => "PaperSourceFirstPage", 
			"PaperSourceDefaultPage" => "PaperSourceDefaultPage", 
			"PaperSourceLastPage" => "PaperSourceLastPage", 
			"DefaultLayout" => "DefaultLayout", 
			"WordMergeDataItem" => "WordMergeDataItem", 
			"PdfFontEmbedding" => "PdfFontEmbedding", 
			"RDLCLayout" => "RDLCLayout", 
			"WordLayout" => "WordLayout", 
			"ExcelLayout" => "ExcelLayout", 
			"DefaultRenderingLayout" => "DefaultRenderingLayout", 
			"PreviewMode" => "PreviewMode", 
			"DataItemTableView" => "DataItemTableView", 
			"DataItemLinkReference" => "DataItemLinkReference", 
			"DataItemLink" => "DataItemLink", 
			"MaxIteration" => "MaxIteration", 
			"PrintOnlyIfDetail" => "PrintOnlyIfDetail", 
			"IncludeCaption" => "IncludeCaption", 
			"Type" => "Type", 
			"SummaryML" => "SummaryML", 
			"Summary" => "Summary", 
			"MimeType" => "MimeType", 
			"LayoutFile" => "LayoutFile", 
			"TopNumberOfRows" => "TopNumberOfRows", 
			"ReadState" => "ReadState", 
			"QueryType" => "QueryType", 
			"UsageCategory" => "UsageCategory", 
			"SqlJoinType" => "SqlJoinType", 
			"DataItemTableFilter" => "DataItemTableFilter", 
			"ColumnFilter" => "ColumnFilter", 
			"Method" => "Method", 
			"ReverseSign" => "ReverseSign", 
			"RoleCenter" => "RoleCenter", 
			"Customizations" => "Customizations", 
			"ProfileDescription" => "ProfileDescription", 
			"ProfileDescriptionML" => "ProfileDescriptionML", 
			"ClearLayout" => "ClearLayout", 
			"ClearActions" => "ClearActions", 
			"ClearViews" => "ClearViews", 
			"Scripts" => "Scripts", 
			"StyleSheets" => "StyleSheets", 
			"Images" => "Images", 
			"StartupScript" => "StartupScript", 
			"RecreateScript" => "RecreateScript", 
			"RefreshScript" => "RefreshScript", 
			"RequestedHeight" => "RequestedHeight", 
			"RequestedWidth" => "RequestedWidth", 
			"MinimumHeight" => "MinimumHeight", 
			"MinimumWidth" => "MinimumWidth", 
			"MaximumHeight" => "MaximumHeight", 
			"MaximumWidth" => "MaximumWidth", 
			"VerticalShrink" => "VerticalShrink", 
			"HorizontalShrink" => "HorizontalShrink", 
			"VerticalStretch" => "VerticalStretch", 
			"HorizontalStretch" => "HorizontalStretch", 
			"DefaultImplementation" => "DefaultImplementation", 
			"UnknownValueImplementation" => "UnknownValueImplementation", 
			"AssignmentCompatibility" => "AssignmentCompatibility", 
			"AssignmentCompatibilityReason" => "AssignmentCompatibilityReason", 
			"Assignable" => "Assignable", 
			"ExcludedPermissionSets" => "ExcludedPermissionSets", 
			"IncludedPermissionSets" => "IncludedPermissionSets", 
			"Id" => "Id", 
			"RoleType" => "RoleType", 
			"GroupName" => "GroupName", 
			"ObjectEntitlements" => "ObjectEntitlements", 
			_ => name, 
		};
	}

	public static bool IsBooleanProperty(PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.IsControlAddIn:
		case PropertyKind.SingleInstance:
		case PropertyKind.Extensible:
		case PropertyKind.DataPerCompany:
		case PropertyKind.PasteIsValid:
		case PropertyKind.LinkedObject:
		case PropertyKind.LinkedInTransaction:
		case PropertyKind.ReplicateData:
		case PropertyKind.BlankZero:
		case PropertyKind.NotBlank:
		case PropertyKind.DateFormula:
		case PropertyKind.ClosingDates:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ValidateTableRelation:
		case PropertyKind.TestTableRelation:
		case PropertyKind.Compressed:
		case PropertyKind.AutoIncrement:
		case PropertyKind.SqlTimestamp:
		case PropertyKind.Numeric:
		case PropertyKind.OptimizeForTextSearch:
		case PropertyKind.MaintainSqlIndex:
		case PropertyKind.MaintainSiftIndex:
		case PropertyKind.Clustered:
		case PropertyKind.Unique:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.ShowFilter:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.IsPreview:
		case PropertyKind.DelayedInsert:
		case PropertyKind.RefreshOnActivate:
		case PropertyKind.AnalysisModeEnabled:
		case PropertyKind.ChangeTrackingAllowed:
		case PropertyKind.ShowCaption:
		case PropertyKind.MultiLine:
		case PropertyKind.Title:
		case PropertyKind.Lookup:
		case PropertyKind.DrillDown:
		case PropertyKind.AssistEdit:
		case PropertyKind.ShowAsTree:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.PromotedOnly:
		case PropertyKind.Ellipsis:
		case PropertyKind.RunPageOnRec:
		case PropertyKind.InFooterBar:
		case PropertyKind.IsHeader:
		case PropertyKind.AllowMultipleFiles:
		case PropertyKind.SharedLayout:
		case PropertyKind.DefaultFieldsValidation:
		case PropertyKind.InlineSchema:
		case PropertyKind.PreserveWhiteSpace:
		case PropertyKind.UseDefaultNamespace:
		case PropertyKind.UseLax:
		case PropertyKind.UseRequestPage:
		case PropertyKind.LinkTableForceInsert:
		case PropertyKind.UseTemporary:
		case PropertyKind.AutoReplace:
		case PropertyKind.AutoSave:
		case PropertyKind.AutoUpdate:
		case PropertyKind.AutoCalcField:
		case PropertyKind.Unbound:
		case PropertyKind.UseSystemPrinter:
		case PropertyKind.EnableExternalImages:
		case PropertyKind.EnableHyperlinks:
		case PropertyKind.AllowScheduling:
		case PropertyKind.ExcelLayoutMultipleDataSheets:
		case PropertyKind.EnableExternalAssemblies:
		case PropertyKind.ProcessingOnly:
		case PropertyKind.ShowPrintStatus:
		case PropertyKind.PrintOnlyIfDetail:
		case PropertyKind.IncludeCaption:
		case PropertyKind.ReverseSign:
		case PropertyKind.ClearLayout:
		case PropertyKind.ClearActions:
		case PropertyKind.ClearViews:
		case PropertyKind.VerticalShrink:
		case PropertyKind.HorizontalShrink:
		case PropertyKind.VerticalStretch:
		case PropertyKind.HorizontalStretch:
		case PropertyKind.AssignmentCompatibility:
		case PropertyKind.Assignable:
			return true;
		default:
			return false;
		}
	}

	public static bool IsEnumProperty(PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.Access:
		case PropertyKind.ShowAs:
		case PropertyKind.Subtype:
		case PropertyKind.TestIsolation:
		case PropertyKind.RequiredTestIsolation:
		case PropertyKind.EventSubscriberInstance:
		case PropertyKind.TestPermissions:
		case PropertyKind.TestHttpRequestPolicy:
		case PropertyKind.TestType:
		case PropertyKind.DataClassification:
		case PropertyKind.AllowInCustomizations:
		case PropertyKind.TableType:
		case PropertyKind.CompressionType:
		case PropertyKind.Scope:
		case PropertyKind.BlankNumbers:
		case PropertyKind.MaskType:
		case PropertyKind.ExtendedDatatype:
		case PropertyKind.FieldClass:
		case PropertyKind.SqlDataType:
		case PropertyKind.ExternalAccess:
		case PropertyKind.PageType:
		case PropertyKind.PromptMode:
		case PropertyKind.DataAccessIntent:
		case PropertyKind.Importance:
		case PropertyKind.Style:
		case PropertyKind.GridLayout:
		case PropertyKind.CuegroupLayout:
		case PropertyKind.TreeInitialState:
		case PropertyKind.Multiplicity:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.RunPageMode:
		case PropertyKind.PromotedCategory:
		case PropertyKind.Gesture:
		case PropertyKind.ObsoleteState:
		case PropertyKind.CustomActionType:
		case PropertyKind.TransactionType:
		case PropertyKind.Direction:
		case PropertyKind.Encoding:
		case PropertyKind.Format:
		case PropertyKind.FormatEvaluate:
		case PropertyKind.TextEncoding:
		case PropertyKind.XmlVersionNo:
		case PropertyKind.TextType:
		case PropertyKind.FieldValidate:
		case PropertyKind.Occurrence:
		case PropertyKind.MinOccurs:
		case PropertyKind.MaxOccurs:
		case PropertyKind.PaperSourceFirstPage:
		case PropertyKind.PaperSourceDefaultPage:
		case PropertyKind.PaperSourceLastPage:
		case PropertyKind.DefaultLayout:
		case PropertyKind.PdfFontEmbedding:
		case PropertyKind.PreviewMode:
		case PropertyKind.Type:
		case PropertyKind.ReadState:
		case PropertyKind.QueryType:
		case PropertyKind.UsageCategory:
		case PropertyKind.SqlJoinType:
		case PropertyKind.Method:
		case PropertyKind.RoleType:
			return true;
		default:
			return false;
		}
	}

	public static bool IsCodeunitProperty(PropertyKind property)
	{
		if ((uint)(property - 1) <= 1u || (uint)(property - 12) <= 11u || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsQueryProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Access:
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.InherentEntitlements:
		case PropertyKind.InherentPermissions:
		case PropertyKind.Permissions:
		case PropertyKind.HelpLink:
		case PropertyKind.ContextSensitiveHelpPage:
		case PropertyKind.APIVersion:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.APIGroup:
		case PropertyKind.APIPublisher:
		case PropertyKind.QueryCategory:
		case PropertyKind.DataAccessIntent:
		case PropertyKind.EntityCaption:
		case PropertyKind.EntitySetCaption:
		case PropertyKind.EntityCaptionML:
		case PropertyKind.EntitySetCaptionML:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
		case PropertyKind.OrderBy:
		case PropertyKind.TopNumberOfRows:
		case PropertyKind.ReadState:
		case PropertyKind.QueryType:
		case PropertyKind.UsageCategory:
			return true;
		default:
			return false;
		}
	}

	public static bool IsFieldProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Access:
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Subtype:
		case PropertyKind.DataClassification:
		case PropertyKind.AllowInCustomizations:
		case PropertyKind.MovedTo:
		case PropertyKind.MovedFrom:
		case PropertyKind.ExternalName:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.BlankNumbers:
		case PropertyKind.BlankZero:
		case PropertyKind.MinValue:
		case PropertyKind.MaxValue:
		case PropertyKind.MaskType:
		case PropertyKind.NotBlank:
		case PropertyKind.CharAllowed:
		case PropertyKind.DateFormula:
		case PropertyKind.ValuesAllowed:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.ClosingDates:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.AccessByPermission:
		case PropertyKind.ExtendedDatatype:
		case PropertyKind.Width:
		case PropertyKind.SignDisplacement:
		case PropertyKind.CaptionClass:
		case PropertyKind.InitValue:
		case PropertyKind.FieldClass:
		case PropertyKind.CalcFormula:
		case PropertyKind.TableRelation:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.SqlDataType:
		case PropertyKind.ValidateTableRelation:
		case PropertyKind.TestTableRelation:
		case PropertyKind.Compressed:
		case PropertyKind.AutoIncrement:
		case PropertyKind.SqlTimestamp:
		case PropertyKind.OptionMembers:
		case PropertyKind.ExternalType:
		case PropertyKind.ExternalAccess:
		case PropertyKind.OptionOrdinalValues:
		case PropertyKind.Numeric:
		case PropertyKind.OptimizeForTextSearch:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsKeyProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Enabled:
		case PropertyKind.SumIndexFields:
		case PropertyKind.MaintainSqlIndex:
		case PropertyKind.MaintainSiftIndex:
		case PropertyKind.Clustered:
		case PropertyKind.SqlIndex:
		case PropertyKind.Unique:
		case PropertyKind.IncludedFields:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.InherentEntitlements:
		case PropertyKind.InherentPermissions:
		case PropertyKind.Permissions:
		case PropertyKind.Extensible:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Editable:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.HelpLink:
		case PropertyKind.ContextSensitiveHelpPage:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.CardPageId:
		case PropertyKind.DataCaptionExpression:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.SourceTable:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.SourceTableView:
		case PropertyKind.ShowFilter:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.PageType:
		case PropertyKind.IsPreview:
		case PropertyKind.PromptMode:
		case PropertyKind.DelayedInsert:
		case PropertyKind.RefreshOnActivate:
		case PropertyKind.PromotedActionCategoriesML:
		case PropertyKind.PromotedActionCategories:
		case PropertyKind.ODataKeyFields:
		case PropertyKind.AnalysisModeEnabled:
		case PropertyKind.APIVersion:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.APIGroup:
		case PropertyKind.APIPublisher:
		case PropertyKind.ChangeTrackingAllowed:
		case PropertyKind.QueryCategory:
		case PropertyKind.AdditionalSearchTermsML:
		case PropertyKind.AdditionalSearchTerms:
		case PropertyKind.DataAccessIntent:
		case PropertyKind.EntityCaption:
		case PropertyKind.EntitySetCaption:
		case PropertyKind.EntityCaptionML:
		case PropertyKind.EntitySetCaptionML:
		case PropertyKind.ApplicationArea:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
		case PropertyKind.UsageCategory:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageActionProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Scope:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Image:
		case PropertyKind.RunPageMode:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.PromotedOnly:
		case PropertyKind.PromotedCategory:
		case PropertyKind.Ellipsis:
		case PropertyKind.ShortcutKey:
		case PropertyKind.RunObject:
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
		case PropertyKind.RunPageOnRec:
		case PropertyKind.InFooterBar:
		case PropertyKind.Gesture:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageActionAreaProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || (uint)(property - 137) <= 1u || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageActionGroupProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.ShowAs:
		case PropertyKind.Enabled:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Image:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageAreaProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageFieldProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.FileUploadAction:
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.BlankNumbers:
		case PropertyKind.BlankZero:
		case PropertyKind.MinValue:
		case PropertyKind.MaxValue:
		case PropertyKind.MaskType:
		case PropertyKind.NotBlank:
		case PropertyKind.CharAllowed:
		case PropertyKind.DateFormula:
		case PropertyKind.ValuesAllowed:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.ClosingDates:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.AccessByPermission:
		case PropertyKind.ExtendedDatatype:
		case PropertyKind.Width:
		case PropertyKind.SignDisplacement:
		case PropertyKind.CaptionClass:
		case PropertyKind.TableRelation:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.Numeric:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.ShowCaption:
		case PropertyKind.MultiLine:
		case PropertyKind.HideValue:
		case PropertyKind.Importance:
		case PropertyKind.Style:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.StyleExpr:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.RowSpan:
		case PropertyKind.ColumnSpan:
		case PropertyKind.ShowMandatory:
		case PropertyKind.Title:
		case PropertyKind.QuickEntry:
		case PropertyKind.Lookup:
		case PropertyKind.DrillDown:
		case PropertyKind.AssistEdit:
		case PropertyKind.Image:
		case PropertyKind.ODataEDMType:
		case PropertyKind.NavigationPageId:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageGroupProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.FileUploadAction:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.ShowCaption:
		case PropertyKind.Visible:
		case PropertyKind.GridLayout:
		case PropertyKind.CuegroupLayout:
		case PropertyKind.IndentationColumn:
		case PropertyKind.IndentationControls:
		case PropertyKind.FreezeColumn:
		case PropertyKind.ShowAsTree:
		case PropertyKind.TreeInitialState:
		case PropertyKind.FileUploadRowAction:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageLabelProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Width:
		case PropertyKind.CaptionClass:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowCaption:
		case PropertyKind.MultiLine:
		case PropertyKind.HideValue:
		case PropertyKind.Importance:
		case PropertyKind.Style:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.StyleExpr:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.RowSpan:
		case PropertyKind.ColumnSpan:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPagePartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.FileUploadAction:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Multiplicity:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageSystemPartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageChartPartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsQueryColumnProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.ColumnFilter:
		case PropertyKind.Method:
		case PropertyKind.ReverseSign:
			return true;
		default:
			return false;
		}
	}

	public static bool IsQueryDataItemProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || property == PropertyKind.DataItemLink || (uint)(property - 274) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsQueryFilterProperty(PropertyKind property)
	{
		if ((uint)(property - 2) <= 2u || (uint)(property - 177) <= 2u || property == PropertyKind.ColumnFilter)
		{
			return true;
		}
		return false;
	}

	public static bool IsReportProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.InherentEntitlements:
		case PropertyKind.InherentPermissions:
		case PropertyKind.Permissions:
		case PropertyKind.Extensible:
		case PropertyKind.AccessByPermission:
		case PropertyKind.AdditionalSearchTermsML:
		case PropertyKind.AdditionalSearchTerms:
		case PropertyKind.DataAccessIntent:
		case PropertyKind.ApplicationArea:
		case PropertyKind.ToolTip:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.TransactionType:
		case PropertyKind.UseRequestPage:
		case PropertyKind.UseSystemPrinter:
		case PropertyKind.EnableExternalImages:
		case PropertyKind.EnableHyperlinks:
		case PropertyKind.AllowScheduling:
		case PropertyKind.ExcelLayoutMultipleDataSheets:
		case PropertyKind.MaximumDatasetSize:
		case PropertyKind.MaximumDocumentCount:
		case PropertyKind.ExecutionTimeout:
		case PropertyKind.FormatRegion:
		case PropertyKind.EnableExternalAssemblies:
		case PropertyKind.ProcessingOnly:
		case PropertyKind.ShowPrintStatus:
		case PropertyKind.PaperSourceFirstPage:
		case PropertyKind.PaperSourceDefaultPage:
		case PropertyKind.PaperSourceLastPage:
		case PropertyKind.DefaultLayout:
		case PropertyKind.WordMergeDataItem:
		case PropertyKind.PdfFontEmbedding:
		case PropertyKind.RDLCLayout:
		case PropertyKind.WordLayout:
		case PropertyKind.ExcelLayout:
		case PropertyKind.DefaultRenderingLayout:
		case PropertyKind.PreviewMode:
		case PropertyKind.UsageCategory:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportDataItemProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.RequestFilterFields:
		case PropertyKind.RequestFilterHeadingML:
		case PropertyKind.RequestFilterHeading:
		case PropertyKind.UseTemporary:
		case PropertyKind.CalcFields:
		case PropertyKind.DataItemTableView:
		case PropertyKind.DataItemLinkReference:
		case PropertyKind.DataItemLink:
		case PropertyKind.MaxIteration:
		case PropertyKind.PrintOnlyIfDetail:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportColumnProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.OptionMembers:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AutoCalcField:
		case PropertyKind.IncludeCaption:
			return true;
		default:
			return false;
		}
	}

	public static bool IsRequestPageProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.Editable:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.HelpLink:
		case PropertyKind.ContextSensitiveHelpPage:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.CardPageId:
		case PropertyKind.DataCaptionExpression:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.SourceTable:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.SourceTableView:
		case PropertyKind.ShowFilter:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsTableProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Access:
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.InherentEntitlements:
		case PropertyKind.InherentPermissions:
		case PropertyKind.Permissions:
		case PropertyKind.Extensible:
		case PropertyKind.DataClassification:
		case PropertyKind.AllowInCustomizations:
		case PropertyKind.DataPerCompany:
		case PropertyKind.MovedTo:
		case PropertyKind.MovedFrom:
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.PasteIsValid:
		case PropertyKind.LinkedObject:
		case PropertyKind.LinkedInTransaction:
		case PropertyKind.TableType:
		case PropertyKind.CompressionType:
		case PropertyKind.ExternalName:
		case PropertyKind.ExternalSchema:
		case PropertyKind.Scope:
		case PropertyKind.ReplicateData:
		case PropertyKind.ColumnStoreIndex:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.InherentEntitlements:
		case PropertyKind.InherentPermissions:
		case PropertyKind.Permissions:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.TransactionType:
		case PropertyKind.DefaultFieldsValidation:
		case PropertyKind.DefaultNamespace:
		case PropertyKind.Direction:
		case PropertyKind.Encoding:
		case PropertyKind.FieldDelimiter:
		case PropertyKind.FieldSeparator:
		case PropertyKind.FileName:
		case PropertyKind.Format:
		case PropertyKind.FormatEvaluate:
		case PropertyKind.InlineSchema:
		case PropertyKind.Namespaces:
		case PropertyKind.PreserveWhiteSpace:
		case PropertyKind.RecordSeparator:
		case PropertyKind.TableSeparator:
		case PropertyKind.TextEncoding:
		case PropertyKind.UseDefaultNamespace:
		case PropertyKind.UseLax:
		case PropertyKind.UseRequestPage:
		case PropertyKind.XmlVersionNo:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortTextElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.TextType:
		case PropertyKind.MinOccurs:
		case PropertyKind.MaxOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortFieldElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.AutoCalcField:
		case PropertyKind.FieldValidate:
		case PropertyKind.MinOccurs:
		case PropertyKind.MaxOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortTableElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.SourceTableView:
		case PropertyKind.LinkTable:
		case PropertyKind.LinkTableForceInsert:
		case PropertyKind.LinkFields:
		case PropertyKind.RequestFilterFields:
		case PropertyKind.RequestFilterHeadingML:
		case PropertyKind.RequestFilterHeading:
		case PropertyKind.UseTemporary:
		case PropertyKind.AutoReplace:
		case PropertyKind.AutoSave:
		case PropertyKind.AutoUpdate:
		case PropertyKind.CalcFields:
		case PropertyKind.MinOccurs:
		case PropertyKind.MaxOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortFieldAttributeProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.AutoCalcField:
		case PropertyKind.FieldValidate:
		case PropertyKind.Occurrence:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortTextAttributeProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.TextType:
		case PropertyKind.Occurrence:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsFieldGroupProperty(PropertyKind property)
	{
		if ((uint)(property - 3) <= 1u || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageActionSeparatorProperty(PropertyKind property)
	{
		if ((uint)(property - 3) <= 1u || (uint)(property - 176) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnumValueProperty(PropertyKind property)
	{
		if ((uint)(property - 3) <= 1u || property == PropertyKind.Implementation || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsDotNetAssemblyProperty(PropertyKind property)
	{
		if ((uint)(property - 8) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsDotNetTypeDeclarationProperty(PropertyKind property)
	{
		if (property == PropertyKind.IsControlAddIn)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageActionRefProperty(PropertyKind property)
	{
		if (property == PropertyKind.Visible || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageCustomActionProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Scope:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Ellipsis:
		case PropertyKind.ShortcutKey:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
		case PropertyKind.CustomActionType:
		case PropertyKind.FlowId:
		case PropertyKind.FlowEnvironmentId:
		case PropertyKind.FlowCaption:
		case PropertyKind.FlowTemplateId:
		case PropertyKind.FlowTemplateCategoryName:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageSystemActionProperty(PropertyKind property)
	{
		if ((uint)(property - 3) <= 1u || property == PropertyKind.Enabled || (uint)(property - 137) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageFileUploadActionProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Scope:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Image:
		case PropertyKind.ShortcutKey:
		case PropertyKind.InFooterBar:
		case PropertyKind.Gesture:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.AboutText:
		case PropertyKind.AboutTextML:
		case PropertyKind.AboutTitle:
		case PropertyKind.AboutTitleML:
		case PropertyKind.AllowedFileExtensions:
		case PropertyKind.AllowMultipleFiles:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageViewProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Visible:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.Filters:
		case PropertyKind.OrderBy:
		case PropertyKind.SharedLayout:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageAnalysisViewProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.DefinitionFile:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportExtensionProperty(PropertyKind property)
	{
		if ((uint)(property - 254) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsRequestPageExtensionProperty(PropertyKind property)
	{
		return false;
	}

	public static bool IsReportLayoutProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.ExcelLayoutMultipleDataSheets:
		case PropertyKind.Type:
		case PropertyKind.SummaryML:
		case PropertyKind.Summary:
		case PropertyKind.MimeType:
		case PropertyKind.LayoutFile:
			return true;
		default:
			return false;
		}
	}

	public static bool IsProfileProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Enabled:
		case PropertyKind.Promoted:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.RoleCenter:
		case PropertyKind.Customizations:
		case PropertyKind.ProfileDescription:
		case PropertyKind.ProfileDescriptionML:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageCustomizationObjectProperty(PropertyKind property)
	{
		if ((uint)(property - 283) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsControlAddInObjectProperty(PropertyKind property)
	{
		if ((uint)(property - 177) <= 2u || (uint)(property - 286) <= 15u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageUserControlProperty(PropertyKind property)
	{
		if ((uint)(property - 134) <= 1u || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnumTypeProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Access:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Extensible:
		case PropertyKind.Scope:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.DefaultImplementation:
		case PropertyKind.UnknownValueImplementation:
		case PropertyKind.AssignmentCompatibility:
		case PropertyKind.AssignmentCompatibilityReason:
			return true;
		default:
			return false;
		}
	}

	public static bool IsInterfaceProperty(PropertyKind property)
	{
		if (property == PropertyKind.Access || property == PropertyKind.Scope || (uint)(property - 177) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPermissionSetProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Access:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
		case PropertyKind.ObsoleteTag:
		case PropertyKind.Assignable:
		case PropertyKind.ExcludedPermissionSets:
		case PropertyKind.IncludedPermissionSets:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPermissionSetExtensionProperty(PropertyKind property)
	{
		if (property == PropertyKind.Permissions || property == PropertyKind.IncludedPermissionSets)
		{
			return true;
		}
		return false;
	}

	public static bool IsEntitlementProperty(PropertyKind property)
	{
		if (property == PropertyKind.Type || (uint)(property - 309) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static PropertyKind GetCodeunitPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"ACCESS" => PropertyKind.Access, 
			"DESCRIPTION" => PropertyKind.Description, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"TABLENO" => PropertyKind.TableNo, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"SINGLEINSTANCE" => PropertyKind.SingleInstance, 
			"TESTISOLATION" => PropertyKind.TestIsolation, 
			"REQUIREDTESTISOLATION" => PropertyKind.RequiredTestIsolation, 
			"EVENTSUBSCRIBERINSTANCE" => PropertyKind.EventSubscriberInstance, 
			"TESTPERMISSIONS" => PropertyKind.TestPermissions, 
			"TESTHTTPREQUESTPOLICY" => PropertyKind.TestHttpRequestPolicy, 
			"TESTTYPE" => PropertyKind.TestType, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetQueryPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"ACCESS" => PropertyKind.Access, 
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"ORDERBY" => PropertyKind.OrderBy, 
			"TOPNUMBEROFROWS" => PropertyKind.TopNumberOfRows, 
			"READSTATE" => PropertyKind.ReadState, 
			"QUERYTYPE" => PropertyKind.QueryType, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			"DATAACCESSINTENT" => PropertyKind.DataAccessIntent, 
			"ENTITYCAPTION" => PropertyKind.EntityCaption, 
			"ENTITYSETCAPTION" => PropertyKind.EntitySetCaption, 
			"ENTITYCAPTIONML" => PropertyKind.EntityCaptionML, 
			"ENTITYSETCAPTIONML" => PropertyKind.EntitySetCaptionML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetFieldPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"MASKTYPE" => PropertyKind.MaskType, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"INITVALUE" => PropertyKind.InitValue, 
			"FIELDCLASS" => PropertyKind.FieldClass, 
			"CALCFORMULA" => PropertyKind.CalcFormula, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"SQLDATATYPE" => PropertyKind.SqlDataType, 
			"VALIDATETABLERELATION" => PropertyKind.ValidateTableRelation, 
			"TESTTABLERELATION" => PropertyKind.TestTableRelation, 
			"ACCESS" => PropertyKind.Access, 
			"ALLOWINCUSTOMIZATIONS" => PropertyKind.AllowInCustomizations, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"COMPRESSED" => PropertyKind.Compressed, 
			"AUTOINCREMENT" => PropertyKind.AutoIncrement, 
			"SQLTIMESTAMP" => PropertyKind.SqlTimestamp, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALTYPE" => PropertyKind.ExternalType, 
			"MOVEDTO" => PropertyKind.MovedTo, 
			"MOVEDFROM" => PropertyKind.MovedFrom, 
			"EXTERNALACCESS" => PropertyKind.ExternalAccess, 
			"OPTIONORDINALVALUES" => PropertyKind.OptionOrdinalValues, 
			"NUMERIC" => PropertyKind.Numeric, 
			"OPTIMIZEFORTEXTSEARCH" => PropertyKind.OptimizeForTextSearch, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetKeyPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"ENABLED" => PropertyKind.Enabled, 
			"SUMINDEXFIELDS" => PropertyKind.SumIndexFields, 
			"MAINTAINSQLINDEX" => PropertyKind.MaintainSqlIndex, 
			"MAINTAINSIFTINDEX" => PropertyKind.MaintainSiftIndex, 
			"CLUSTERED" => PropertyKind.Clustered, 
			"SQLINDEX" => PropertyKind.SqlIndex, 
			"UNIQUE" => PropertyKind.Unique, 
			"INCLUDEDFIELDS" => PropertyKind.IncludedFields, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPagePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"EDITABLE" => PropertyKind.Editable, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PAGETYPE" => PropertyKind.PageType, 
			"ISPREVIEW" => PropertyKind.IsPreview, 
			"PROMPTMODE" => PropertyKind.PromptMode, 
			"DELAYEDINSERT" => PropertyKind.DelayedInsert, 
			"REFRESHONACTIVATE" => PropertyKind.RefreshOnActivate, 
			"PROMOTEDACTIONCATEGORIESML" => PropertyKind.PromotedActionCategoriesML, 
			"PROMOTEDACTIONCATEGORIES" => PropertyKind.PromotedActionCategories, 
			"ODATAKEYFIELDS" => PropertyKind.ODataKeyFields, 
			"ANALYSISMODEENABLED" => PropertyKind.AnalysisModeEnabled, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"CHANGETRACKINGALLOWED" => PropertyKind.ChangeTrackingAllowed, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"DATAACCESSINTENT" => PropertyKind.DataAccessIntent, 
			"ENTITYCAPTION" => PropertyKind.EntityCaption, 
			"ENTITYSETCAPTION" => PropertyKind.EntitySetCaption, 
			"ENTITYCAPTIONML" => PropertyKind.EntityCaptionML, 
			"ENTITYSETCAPTIONML" => PropertyKind.EntitySetCaptionML, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageActionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"IMAGE" => PropertyKind.Image, 
			"RUNPAGEMODE" => PropertyKind.RunPageMode, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"PROMOTED" => PropertyKind.Promoted, 
			"PROMOTEDISBIG" => PropertyKind.PromotedIsBig, 
			"PROMOTEDONLY" => PropertyKind.PromotedOnly, 
			"PROMOTEDCATEGORY" => PropertyKind.PromotedCategory, 
			"SCOPE" => PropertyKind.Scope, 
			"ELLIPSIS" => PropertyKind.Ellipsis, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"RUNOBJECT" => PropertyKind.RunObject, 
			"RUNPAGEVIEW" => PropertyKind.RunPageView, 
			"RUNPAGELINK" => PropertyKind.RunPageLink, 
			"RUNPAGEONREC" => PropertyKind.RunPageOnRec, 
			"INFOOTERBAR" => PropertyKind.InFooterBar, 
			"GESTURE" => PropertyKind.Gesture, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageActionAreaPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageActionGroupPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"SHOWAS" => PropertyKind.ShowAs, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"IMAGE" => PropertyKind.Image, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageAreaPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageFieldPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"FILEUPLOADACTION" => PropertyKind.FileUploadAction, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"MASKTYPE" => PropertyKind.MaskType, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"SHOWMANDATORY" => PropertyKind.ShowMandatory, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"TITLE" => PropertyKind.Title, 
			"QUICKENTRY" => PropertyKind.QuickEntry, 
			"LOOKUP" => PropertyKind.Lookup, 
			"DRILLDOWN" => PropertyKind.DrillDown, 
			"ASSISTEDIT" => PropertyKind.AssistEdit, 
			"IMAGE" => PropertyKind.Image, 
			"ODATAEDMTYPE" => PropertyKind.ODataEDMType, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"NAVIGATIONPAGEID" => PropertyKind.NavigationPageId, 
			"NUMERIC" => PropertyKind.Numeric, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageGroupPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"FILEUPLOADACTION" => PropertyKind.FileUploadAction, 
			"VISIBLE" => PropertyKind.Visible, 
			"EDITABLE" => PropertyKind.Editable, 
			"ENABLED" => PropertyKind.Enabled, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"GRIDLAYOUT" => PropertyKind.GridLayout, 
			"CUEGROUPLAYOUT" => PropertyKind.CuegroupLayout, 
			"INDENTATIONCOLUMN" => PropertyKind.IndentationColumn, 
			"INDENTATIONCONTROLS" => PropertyKind.IndentationControls, 
			"FREEZECOLUMN" => PropertyKind.FreezeColumn, 
			"SHOWASTREE" => PropertyKind.ShowAsTree, 
			"TREEINITIALSTATE" => PropertyKind.TreeInitialState, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"FILEUPLOADROWACTION" => PropertyKind.FileUploadRowAction, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageLabelPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"EDITABLE" => PropertyKind.Editable, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"ENABLED" => PropertyKind.Enabled, 
			"WIDTH" => PropertyKind.Width, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPagePartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"FILEUPLOADACTION" => PropertyKind.FileUploadAction, 
			"EDITABLE" => PropertyKind.Editable, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"MULTIPLICITY" => PropertyKind.Multiplicity, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageSystemPartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"EDITABLE" => PropertyKind.Editable, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageChartPartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"EDITABLE" => PropertyKind.Editable, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetQueryColumnPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			"METHOD" => PropertyKind.Method, 
			"REVERSESIGN" => PropertyKind.ReverseSign, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetQueryDataItemPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"SQLJOINTYPE" => PropertyKind.SqlJoinType, 
			"DATAITEMTABLEFILTER" => PropertyKind.DataItemTableFilter, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetQueryFilterPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetReportPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"USESYSTEMPRINTER" => PropertyKind.UseSystemPrinter, 
			"ENABLEEXTERNALIMAGES" => PropertyKind.EnableExternalImages, 
			"ENABLEHYPERLINKS" => PropertyKind.EnableHyperlinks, 
			"ALLOWSCHEDULING" => PropertyKind.AllowScheduling, 
			"EXCELLAYOUTMULTIPLEDATASHEETS" => PropertyKind.ExcelLayoutMultipleDataSheets, 
			"MAXIMUMDATASETSIZE" => PropertyKind.MaximumDatasetSize, 
			"MAXIMUMDOCUMENTCOUNT" => PropertyKind.MaximumDocumentCount, 
			"EXECUTIONTIMEOUT" => PropertyKind.ExecutionTimeout, 
			"FORMATREGION" => PropertyKind.FormatRegion, 
			"ENABLEEXTERNALASSEMBLIES" => PropertyKind.EnableExternalAssemblies, 
			"PROCESSINGONLY" => PropertyKind.ProcessingOnly, 
			"SHOWPRINTSTATUS" => PropertyKind.ShowPrintStatus, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PAPERSOURCEFIRSTPAGE" => PropertyKind.PaperSourceFirstPage, 
			"PAPERSOURCEDEFAULTPAGE" => PropertyKind.PaperSourceDefaultPage, 
			"PAPERSOURCELASTPAGE" => PropertyKind.PaperSourceLastPage, 
			"DEFAULTLAYOUT" => PropertyKind.DefaultLayout, 
			"WORDMERGEDATAITEM" => PropertyKind.WordMergeDataItem, 
			"PDFFONTEMBEDDING" => PropertyKind.PdfFontEmbedding, 
			"RDLCLAYOUT" => PropertyKind.RDLCLayout, 
			"WORDLAYOUT" => PropertyKind.WordLayout, 
			"EXCELLAYOUT" => PropertyKind.ExcelLayout, 
			"DEFAULTRENDERINGLAYOUT" => PropertyKind.DefaultRenderingLayout, 
			"PREVIEWMODE" => PropertyKind.PreviewMode, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"DATAACCESSINTENT" => PropertyKind.DataAccessIntent, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetReportDataItemPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"DATAITEMTABLEVIEW" => PropertyKind.DataItemTableView, 
			"DATAITEMLINKREFERENCE" => PropertyKind.DataItemLinkReference, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"MAXITERATION" => PropertyKind.MaxIteration, 
			"PRINTONLYIFDETAIL" => PropertyKind.PrintOnlyIfDetail, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetReportColumnPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"INCLUDECAPTION" => PropertyKind.IncludeCaption, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetRequestPagePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"EDITABLE" => PropertyKind.Editable, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetTablePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"ALLOWINCUSTOMIZATIONS" => PropertyKind.AllowInCustomizations, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"DATAPERCOMPANY" => PropertyKind.DataPerCompany, 
			"MOVEDTO" => PropertyKind.MovedTo, 
			"MOVEDFROM" => PropertyKind.MovedFrom, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"PASTEISVALID" => PropertyKind.PasteIsValid, 
			"LINKEDOBJECT" => PropertyKind.LinkedObject, 
			"LINKEDINTRANSACTION" => PropertyKind.LinkedInTransaction, 
			"TABLETYPE" => PropertyKind.TableType, 
			"COMPRESSIONTYPE" => PropertyKind.CompressionType, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALSCHEMA" => PropertyKind.ExternalSchema, 
			"SCOPE" => PropertyKind.Scope, 
			"REPLICATEDATA" => PropertyKind.ReplicateData, 
			"ACCESS" => PropertyKind.Access, 
			"COLUMNSTOREINDEX" => PropertyKind.ColumnStoreIndex, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"DEFAULTFIELDSVALIDATION" => PropertyKind.DefaultFieldsValidation, 
			"DEFAULTNAMESPACE" => PropertyKind.DefaultNamespace, 
			"DIRECTION" => PropertyKind.Direction, 
			"ENCODING" => PropertyKind.Encoding, 
			"FIELDDELIMITER" => PropertyKind.FieldDelimiter, 
			"FIELDSEPARATOR" => PropertyKind.FieldSeparator, 
			"FILENAME" => PropertyKind.FileName, 
			"FORMAT" => PropertyKind.Format, 
			"FORMATEVALUATE" => PropertyKind.FormatEvaluate, 
			"INLINESCHEMA" => PropertyKind.InlineSchema, 
			"NAMESPACES" => PropertyKind.Namespaces, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PRESERVEWHITESPACE" => PropertyKind.PreserveWhiteSpace, 
			"RECORDSEPARATOR" => PropertyKind.RecordSeparator, 
			"TABLESEPARATOR" => PropertyKind.TableSeparator, 
			"TEXTENCODING" => PropertyKind.TextEncoding, 
			"USEDEFAULTNAMESPACE" => PropertyKind.UseDefaultNamespace, 
			"USELAX" => PropertyKind.UseLax, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"XMLVERSIONNO" => PropertyKind.XmlVersionNo, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortTextElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortFieldElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortTableElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"LINKTABLE" => PropertyKind.LinkTable, 
			"LINKTABLEFORCEINSERT" => PropertyKind.LinkTableForceInsert, 
			"LINKFIELDS" => PropertyKind.LinkFields, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			"AUTOREPLACE" => PropertyKind.AutoReplace, 
			"AUTOSAVE" => PropertyKind.AutoSave, 
			"AUTOUPDATE" => PropertyKind.AutoUpdate, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortFieldAttributePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetXmlPortTextAttributePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetFieldGroupPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageActionSeparatorPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"ISHEADER" => PropertyKind.IsHeader, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetEnumValuePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"IMPLEMENTATION" => PropertyKind.Implementation, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetDotNetAssemblyPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"VERSION" => PropertyKind.Version, 
			"CULTURE" => PropertyKind.Culture, 
			"PUBLICKEYTOKEN" => PropertyKind.PublicKeyToken, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetDotNetTypeDeclarationPropertyKind(string name)
	{
		if (name.ToUpperInvariant() == "ISCONTROLADDIN")
		{
			return PropertyKind.IsControlAddIn;
		}
		return PropertyKind.None;
	}

	public static PropertyKind GetPageActionRefPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"VISIBLE" => PropertyKind.Visible, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageCustomActionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"ABOUTTEXT" => PropertyKind.AboutText, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CUSTOMACTIONTYPE" => PropertyKind.CustomActionType, 
			"ELLIPSIS" => PropertyKind.Ellipsis, 
			"ENABLED" => PropertyKind.Enabled, 
			"FLOWID" => PropertyKind.FlowId, 
			"FLOWENVIRONMENTID" => PropertyKind.FlowEnvironmentId, 
			"FLOWCAPTION" => PropertyKind.FlowCaption, 
			"FLOWTEMPLATEID" => PropertyKind.FlowTemplateId, 
			"FLOWTEMPLATECATEGORYNAME" => PropertyKind.FlowTemplateCategoryName, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"SCOPE" => PropertyKind.Scope, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"VISIBLE" => PropertyKind.Visible, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageSystemActionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"ENABLED" => PropertyKind.Enabled, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageFileUploadActionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			"ALLOWEDFILEEXTENSIONS" => PropertyKind.AllowedFileExtensions, 
			"ALLOWMULTIPLEFILES" => PropertyKind.AllowMultipleFiles, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ENABLED" => PropertyKind.Enabled, 
			"VISIBLE" => PropertyKind.Visible, 
			"IMAGE" => PropertyKind.Image, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"SCOPE" => PropertyKind.Scope, 
			"GESTURE" => PropertyKind.Gesture, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"INFOOTERBAR" => PropertyKind.InFooterBar, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageViewPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"FILTERS" => PropertyKind.Filters, 
			"ORDERBY" => PropertyKind.OrderBy, 
			"SHAREDLAYOUT" => PropertyKind.SharedLayout, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageAnalysisViewPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DEFINITIONFILE" => PropertyKind.DefinitionFile, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetReportExtensionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"RDLCLAYOUT" => PropertyKind.RDLCLayout, 
			"WORDLAYOUT" => PropertyKind.WordLayout, 
			"EXCELLAYOUT" => PropertyKind.ExcelLayout, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetRequestPageExtensionPropertyKind(string name)
	{
		return PropertyKind.None;
	}

	public static PropertyKind GetReportLayoutPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"TYPE" => PropertyKind.Type, 
			"SUMMARYML" => PropertyKind.SummaryML, 
			"SUMMARY" => PropertyKind.Summary, 
			"MIMETYPE" => PropertyKind.MimeType, 
			"LAYOUTFILE" => PropertyKind.LayoutFile, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"EXCELLAYOUTMULTIPLEDATASHEETS" => PropertyKind.ExcelLayoutMultipleDataSheets, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetProfilePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"ROLECENTER" => PropertyKind.RoleCenter, 
			"CUSTOMIZATIONS" => PropertyKind.Customizations, 
			"ENABLED" => PropertyKind.Enabled, 
			"PROMOTED" => PropertyKind.Promoted, 
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"PROFILEDESCRIPTION" => PropertyKind.ProfileDescription, 
			"PROFILEDESCRIPTIONML" => PropertyKind.ProfileDescriptionML, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageCustomizationObjectPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CLEARLAYOUT" => PropertyKind.ClearLayout, 
			"CLEARACTIONS" => PropertyKind.ClearActions, 
			"CLEARVIEWS" => PropertyKind.ClearViews, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetControlAddInObjectPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"SCRIPTS" => PropertyKind.Scripts, 
			"STYLESHEETS" => PropertyKind.StyleSheets, 
			"IMAGES" => PropertyKind.Images, 
			"STARTUPSCRIPT" => PropertyKind.StartupScript, 
			"RECREATESCRIPT" => PropertyKind.RecreateScript, 
			"REFRESHSCRIPT" => PropertyKind.RefreshScript, 
			"REQUESTEDHEIGHT" => PropertyKind.RequestedHeight, 
			"REQUESTEDWIDTH" => PropertyKind.RequestedWidth, 
			"MINIMUMHEIGHT" => PropertyKind.MinimumHeight, 
			"MINIMUMWIDTH" => PropertyKind.MinimumWidth, 
			"MAXIMUMHEIGHT" => PropertyKind.MaximumHeight, 
			"MAXIMUMWIDTH" => PropertyKind.MaximumWidth, 
			"VERTICALSHRINK" => PropertyKind.VerticalShrink, 
			"HORIZONTALSHRINK" => PropertyKind.HorizontalShrink, 
			"VERTICALSTRETCH" => PropertyKind.VerticalStretch, 
			"HORIZONTALSTRETCH" => PropertyKind.HorizontalStretch, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPageUserControlPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"VISIBLE" => PropertyKind.Visible, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetEnumTypePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			"DEFAULTIMPLEMENTATION" => PropertyKind.DefaultImplementation, 
			"UNKNOWNVALUEIMPLEMENTATION" => PropertyKind.UnknownValueImplementation, 
			"ASSIGNMENTCOMPATIBILITY" => PropertyKind.AssignmentCompatibility, 
			"ASSIGNMENTCOMPATIBILITYREASON" => PropertyKind.AssignmentCompatibilityReason, 
			"CAPTION" => PropertyKind.Caption, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"SCOPE" => PropertyKind.Scope, 
			"ACCESS" => PropertyKind.Access, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetInterfacePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"SCOPE" => PropertyKind.Scope, 
			"ACCESS" => PropertyKind.Access, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPermissionSetPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ACCESS" => PropertyKind.Access, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"ASSIGNABLE" => PropertyKind.Assignable, 
			"EXCLUDEDPERMISSIONSETS" => PropertyKind.ExcludedPermissionSets, 
			"INCLUDEDPERMISSIONSETS" => PropertyKind.IncludedPermissionSets, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPermissionSetExtensionPropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (!(text == "INCLUDEDPERMISSIONSETS"))
		{
			if (text == "PERMISSIONS")
			{
				return PropertyKind.Permissions;
			}
			return PropertyKind.None;
		}
		return PropertyKind.IncludedPermissionSets;
	}

	public static PropertyKind GetEntitlementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"TYPE" => PropertyKind.Type, 
			"ID" => PropertyKind.Id, 
			"ROLETYPE" => PropertyKind.RoleType, 
			"GROUPNAME" => PropertyKind.GroupName, 
			"OBJECTENTITLEMENTS" => PropertyKind.ObjectEntitlements, 
			_ => PropertyKind.None, 
		};
	}

	public static PropertyKind GetPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"ACCESS" => PropertyKind.Access, 
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"FILEUPLOADACTION" => PropertyKind.FileUploadAction, 
			"SHOWAS" => PropertyKind.ShowAs, 
			"IMPLEMENTATION" => PropertyKind.Implementation, 
			"VERSION" => PropertyKind.Version, 
			"CULTURE" => PropertyKind.Culture, 
			"PUBLICKEYTOKEN" => PropertyKind.PublicKeyToken, 
			"ISCONTROLADDIN" => PropertyKind.IsControlAddIn, 
			"INHERENTENTITLEMENTS" => PropertyKind.InherentEntitlements, 
			"INHERENTPERMISSIONS" => PropertyKind.InherentPermissions, 
			"TABLENO" => PropertyKind.TableNo, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"SINGLEINSTANCE" => PropertyKind.SingleInstance, 
			"TESTISOLATION" => PropertyKind.TestIsolation, 
			"REQUIREDTESTISOLATION" => PropertyKind.RequiredTestIsolation, 
			"EVENTSUBSCRIBERINSTANCE" => PropertyKind.EventSubscriberInstance, 
			"TESTPERMISSIONS" => PropertyKind.TestPermissions, 
			"TESTHTTPREQUESTPOLICY" => PropertyKind.TestHttpRequestPolicy, 
			"TESTTYPE" => PropertyKind.TestType, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"ALLOWINCUSTOMIZATIONS" => PropertyKind.AllowInCustomizations, 
			"DATAPERCOMPANY" => PropertyKind.DataPerCompany, 
			"MOVEDTO" => PropertyKind.MovedTo, 
			"MOVEDFROM" => PropertyKind.MovedFrom, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"PASTEISVALID" => PropertyKind.PasteIsValid, 
			"LINKEDOBJECT" => PropertyKind.LinkedObject, 
			"LINKEDINTRANSACTION" => PropertyKind.LinkedInTransaction, 
			"TABLETYPE" => PropertyKind.TableType, 
			"COMPRESSIONTYPE" => PropertyKind.CompressionType, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALSCHEMA" => PropertyKind.ExternalSchema, 
			"SCOPE" => PropertyKind.Scope, 
			"REPLICATEDATA" => PropertyKind.ReplicateData, 
			"COLUMNSTOREINDEX" => PropertyKind.ColumnStoreIndex, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"MASKTYPE" => PropertyKind.MaskType, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"INITVALUE" => PropertyKind.InitValue, 
			"FIELDCLASS" => PropertyKind.FieldClass, 
			"CALCFORMULA" => PropertyKind.CalcFormula, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"SQLDATATYPE" => PropertyKind.SqlDataType, 
			"VALIDATETABLERELATION" => PropertyKind.ValidateTableRelation, 
			"TESTTABLERELATION" => PropertyKind.TestTableRelation, 
			"COMPRESSED" => PropertyKind.Compressed, 
			"AUTOINCREMENT" => PropertyKind.AutoIncrement, 
			"SQLTIMESTAMP" => PropertyKind.SqlTimestamp, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"EXTERNALTYPE" => PropertyKind.ExternalType, 
			"EXTERNALACCESS" => PropertyKind.ExternalAccess, 
			"OPTIONORDINALVALUES" => PropertyKind.OptionOrdinalValues, 
			"NUMERIC" => PropertyKind.Numeric, 
			"OPTIMIZEFORTEXTSEARCH" => PropertyKind.OptimizeForTextSearch, 
			"SUMINDEXFIELDS" => PropertyKind.SumIndexFields, 
			"MAINTAINSQLINDEX" => PropertyKind.MaintainSqlIndex, 
			"MAINTAINSIFTINDEX" => PropertyKind.MaintainSiftIndex, 
			"CLUSTERED" => PropertyKind.Clustered, 
			"SQLINDEX" => PropertyKind.SqlIndex, 
			"UNIQUE" => PropertyKind.Unique, 
			"INCLUDEDFIELDS" => PropertyKind.IncludedFields, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"PAGETYPE" => PropertyKind.PageType, 
			"ISPREVIEW" => PropertyKind.IsPreview, 
			"PROMPTMODE" => PropertyKind.PromptMode, 
			"DELAYEDINSERT" => PropertyKind.DelayedInsert, 
			"REFRESHONACTIVATE" => PropertyKind.RefreshOnActivate, 
			"PROMOTEDACTIONCATEGORIESML" => PropertyKind.PromotedActionCategoriesML, 
			"PROMOTEDACTIONCATEGORIES" => PropertyKind.PromotedActionCategories, 
			"ODATAKEYFIELDS" => PropertyKind.ODataKeyFields, 
			"ANALYSISMODEENABLED" => PropertyKind.AnalysisModeEnabled, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"CHANGETRACKINGALLOWED" => PropertyKind.ChangeTrackingAllowed, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"DATAACCESSINTENT" => PropertyKind.DataAccessIntent, 
			"ENTITYCAPTION" => PropertyKind.EntityCaption, 
			"ENTITYSETCAPTION" => PropertyKind.EntitySetCaption, 
			"ENTITYCAPTIONML" => PropertyKind.EntityCaptionML, 
			"ENTITYSETCAPTIONML" => PropertyKind.EntitySetCaptionML, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"SHOWMANDATORY" => PropertyKind.ShowMandatory, 
			"TITLE" => PropertyKind.Title, 
			"QUICKENTRY" => PropertyKind.QuickEntry, 
			"LOOKUP" => PropertyKind.Lookup, 
			"DRILLDOWN" => PropertyKind.DrillDown, 
			"ASSISTEDIT" => PropertyKind.AssistEdit, 
			"IMAGE" => PropertyKind.Image, 
			"ODATAEDMTYPE" => PropertyKind.ODataEDMType, 
			"NAVIGATIONPAGEID" => PropertyKind.NavigationPageId, 
			"GRIDLAYOUT" => PropertyKind.GridLayout, 
			"CUEGROUPLAYOUT" => PropertyKind.CuegroupLayout, 
			"INDENTATIONCOLUMN" => PropertyKind.IndentationColumn, 
			"INDENTATIONCONTROLS" => PropertyKind.IndentationControls, 
			"FREEZECOLUMN" => PropertyKind.FreezeColumn, 
			"SHOWASTREE" => PropertyKind.ShowAsTree, 
			"TREEINITIALSTATE" => PropertyKind.TreeInitialState, 
			"FILEUPLOADROWACTION" => PropertyKind.FileUploadRowAction, 
			"MULTIPLICITY" => PropertyKind.Multiplicity, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"RUNPAGEMODE" => PropertyKind.RunPageMode, 
			"PROMOTED" => PropertyKind.Promoted, 
			"PROMOTEDISBIG" => PropertyKind.PromotedIsBig, 
			"PROMOTEDONLY" => PropertyKind.PromotedOnly, 
			"PROMOTEDCATEGORY" => PropertyKind.PromotedCategory, 
			"ELLIPSIS" => PropertyKind.Ellipsis, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"RUNOBJECT" => PropertyKind.RunObject, 
			"RUNPAGEVIEW" => PropertyKind.RunPageView, 
			"RUNPAGELINK" => PropertyKind.RunPageLink, 
			"RUNPAGEONREC" => PropertyKind.RunPageOnRec, 
			"INFOOTERBAR" => PropertyKind.InFooterBar, 
			"GESTURE" => PropertyKind.Gesture, 
			"ISHEADER" => PropertyKind.IsHeader, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"OBSOLETETAG" => PropertyKind.ObsoleteTag, 
			"ABOUTTEXT" => PropertyKind.AboutText, 
			"ABOUTTEXTML" => PropertyKind.AboutTextML, 
			"ABOUTTITLE" => PropertyKind.AboutTitle, 
			"ABOUTTITLEML" => PropertyKind.AboutTitleML, 
			"CUSTOMACTIONTYPE" => PropertyKind.CustomActionType, 
			"FLOWID" => PropertyKind.FlowId, 
			"FLOWENVIRONMENTID" => PropertyKind.FlowEnvironmentId, 
			"FLOWCAPTION" => PropertyKind.FlowCaption, 
			"FLOWTEMPLATEID" => PropertyKind.FlowTemplateId, 
			"FLOWTEMPLATECATEGORYNAME" => PropertyKind.FlowTemplateCategoryName, 
			"ALLOWEDFILEEXTENSIONS" => PropertyKind.AllowedFileExtensions, 
			"ALLOWMULTIPLEFILES" => PropertyKind.AllowMultipleFiles, 
			"FILTERS" => PropertyKind.Filters, 
			"ORDERBY" => PropertyKind.OrderBy, 
			"SHAREDLAYOUT" => PropertyKind.SharedLayout, 
			"DEFINITIONFILE" => PropertyKind.DefinitionFile, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"DEFAULTFIELDSVALIDATION" => PropertyKind.DefaultFieldsValidation, 
			"DEFAULTNAMESPACE" => PropertyKind.DefaultNamespace, 
			"DIRECTION" => PropertyKind.Direction, 
			"ENCODING" => PropertyKind.Encoding, 
			"FIELDDELIMITER" => PropertyKind.FieldDelimiter, 
			"FIELDSEPARATOR" => PropertyKind.FieldSeparator, 
			"FILENAME" => PropertyKind.FileName, 
			"FORMAT" => PropertyKind.Format, 
			"FORMATEVALUATE" => PropertyKind.FormatEvaluate, 
			"INLINESCHEMA" => PropertyKind.InlineSchema, 
			"NAMESPACES" => PropertyKind.Namespaces, 
			"PRESERVEWHITESPACE" => PropertyKind.PreserveWhiteSpace, 
			"RECORDSEPARATOR" => PropertyKind.RecordSeparator, 
			"TABLESEPARATOR" => PropertyKind.TableSeparator, 
			"TEXTENCODING" => PropertyKind.TextEncoding, 
			"USEDEFAULTNAMESPACE" => PropertyKind.UseDefaultNamespace, 
			"USELAX" => PropertyKind.UseLax, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"XMLVERSIONNO" => PropertyKind.XmlVersionNo, 
			"LINKTABLE" => PropertyKind.LinkTable, 
			"LINKTABLEFORCEINSERT" => PropertyKind.LinkTableForceInsert, 
			"LINKFIELDS" => PropertyKind.LinkFields, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			"AUTOREPLACE" => PropertyKind.AutoReplace, 
			"AUTOSAVE" => PropertyKind.AutoSave, 
			"AUTOUPDATE" => PropertyKind.AutoUpdate, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"USESYSTEMPRINTER" => PropertyKind.UseSystemPrinter, 
			"ENABLEEXTERNALIMAGES" => PropertyKind.EnableExternalImages, 
			"ENABLEHYPERLINKS" => PropertyKind.EnableHyperlinks, 
			"ALLOWSCHEDULING" => PropertyKind.AllowScheduling, 
			"EXCELLAYOUTMULTIPLEDATASHEETS" => PropertyKind.ExcelLayoutMultipleDataSheets, 
			"MAXIMUMDATASETSIZE" => PropertyKind.MaximumDatasetSize, 
			"MAXIMUMDOCUMENTCOUNT" => PropertyKind.MaximumDocumentCount, 
			"EXECUTIONTIMEOUT" => PropertyKind.ExecutionTimeout, 
			"FORMATREGION" => PropertyKind.FormatRegion, 
			"ENABLEEXTERNALASSEMBLIES" => PropertyKind.EnableExternalAssemblies, 
			"PROCESSINGONLY" => PropertyKind.ProcessingOnly, 
			"SHOWPRINTSTATUS" => PropertyKind.ShowPrintStatus, 
			"PAPERSOURCEFIRSTPAGE" => PropertyKind.PaperSourceFirstPage, 
			"PAPERSOURCEDEFAULTPAGE" => PropertyKind.PaperSourceDefaultPage, 
			"PAPERSOURCELASTPAGE" => PropertyKind.PaperSourceLastPage, 
			"DEFAULTLAYOUT" => PropertyKind.DefaultLayout, 
			"WORDMERGEDATAITEM" => PropertyKind.WordMergeDataItem, 
			"PDFFONTEMBEDDING" => PropertyKind.PdfFontEmbedding, 
			"RDLCLAYOUT" => PropertyKind.RDLCLayout, 
			"WORDLAYOUT" => PropertyKind.WordLayout, 
			"EXCELLAYOUT" => PropertyKind.ExcelLayout, 
			"DEFAULTRENDERINGLAYOUT" => PropertyKind.DefaultRenderingLayout, 
			"PREVIEWMODE" => PropertyKind.PreviewMode, 
			"DATAITEMTABLEVIEW" => PropertyKind.DataItemTableView, 
			"DATAITEMLINKREFERENCE" => PropertyKind.DataItemLinkReference, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"MAXITERATION" => PropertyKind.MaxIteration, 
			"PRINTONLYIFDETAIL" => PropertyKind.PrintOnlyIfDetail, 
			"INCLUDECAPTION" => PropertyKind.IncludeCaption, 
			"TYPE" => PropertyKind.Type, 
			"SUMMARYML" => PropertyKind.SummaryML, 
			"SUMMARY" => PropertyKind.Summary, 
			"MIMETYPE" => PropertyKind.MimeType, 
			"LAYOUTFILE" => PropertyKind.LayoutFile, 
			"TOPNUMBEROFROWS" => PropertyKind.TopNumberOfRows, 
			"READSTATE" => PropertyKind.ReadState, 
			"QUERYTYPE" => PropertyKind.QueryType, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"SQLJOINTYPE" => PropertyKind.SqlJoinType, 
			"DATAITEMTABLEFILTER" => PropertyKind.DataItemTableFilter, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			"METHOD" => PropertyKind.Method, 
			"REVERSESIGN" => PropertyKind.ReverseSign, 
			"ROLECENTER" => PropertyKind.RoleCenter, 
			"CUSTOMIZATIONS" => PropertyKind.Customizations, 
			"PROFILEDESCRIPTION" => PropertyKind.ProfileDescription, 
			"PROFILEDESCRIPTIONML" => PropertyKind.ProfileDescriptionML, 
			"CLEARLAYOUT" => PropertyKind.ClearLayout, 
			"CLEARACTIONS" => PropertyKind.ClearActions, 
			"CLEARVIEWS" => PropertyKind.ClearViews, 
			"SCRIPTS" => PropertyKind.Scripts, 
			"STYLESHEETS" => PropertyKind.StyleSheets, 
			"IMAGES" => PropertyKind.Images, 
			"STARTUPSCRIPT" => PropertyKind.StartupScript, 
			"RECREATESCRIPT" => PropertyKind.RecreateScript, 
			"REFRESHSCRIPT" => PropertyKind.RefreshScript, 
			"REQUESTEDHEIGHT" => PropertyKind.RequestedHeight, 
			"REQUESTEDWIDTH" => PropertyKind.RequestedWidth, 
			"MINIMUMHEIGHT" => PropertyKind.MinimumHeight, 
			"MINIMUMWIDTH" => PropertyKind.MinimumWidth, 
			"MAXIMUMHEIGHT" => PropertyKind.MaximumHeight, 
			"MAXIMUMWIDTH" => PropertyKind.MaximumWidth, 
			"VERTICALSHRINK" => PropertyKind.VerticalShrink, 
			"HORIZONTALSHRINK" => PropertyKind.HorizontalShrink, 
			"VERTICALSTRETCH" => PropertyKind.VerticalStretch, 
			"HORIZONTALSTRETCH" => PropertyKind.HorizontalStretch, 
			"DEFAULTIMPLEMENTATION" => PropertyKind.DefaultImplementation, 
			"UNKNOWNVALUEIMPLEMENTATION" => PropertyKind.UnknownValueImplementation, 
			"ASSIGNMENTCOMPATIBILITY" => PropertyKind.AssignmentCompatibility, 
			"ASSIGNMENTCOMPATIBILITYREASON" => PropertyKind.AssignmentCompatibilityReason, 
			"ASSIGNABLE" => PropertyKind.Assignable, 
			"EXCLUDEDPERMISSIONSETS" => PropertyKind.ExcludedPermissionSets, 
			"INCLUDEDPERMISSIONSETS" => PropertyKind.IncludedPermissionSets, 
			"ID" => PropertyKind.Id, 
			"ROLETYPE" => PropertyKind.RoleType, 
			"GROUPNAME" => PropertyKind.GroupName, 
			"OBJECTENTITLEMENTS" => PropertyKind.ObjectEntitlements, 
			_ => PropertyKind.None, 
		};
	}

	internal static string GetExternalBusinessEventVersionOrDefault(AttributeSymbol? attributeSymbol)
	{
		if ((object)attributeSymbol == null || attributeSymbol.Arguments.Length <= 4)
		{
			return "0.0";
		}
		return attributeSymbol.Arguments[4].ValueText;
	}

	public static bool IsIdentifierStartCharacter(char identifierStart)
	{
		return UnicodeCharacterUtilities.IsIdentifierStartCharacter(identifierStart);
	}

	public static bool IsIdentifierPartCharacter(char identifierPart)
	{
		return UnicodeCharacterUtilities.IsIdentifierPartCharacter(identifierPart);
	}

	public static bool IsValidIdentifier(string name)
	{
		return UnicodeCharacterUtilities.IsValidIdentifier(name);
	}

	public static bool IsWhitespace(char ch)
	{
		if (ch != ' ' && ch != '\t' && ch != '\v' && ch != '\f' && ch != '\u00a0' && ch != '\ufeff' && ch != '\u001a')
		{
			if (ch > 'ÿ')
			{
				return CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator;
			}
			return false;
		}
		return true;
	}

	public static bool IsNewLine(char ch)
	{
		if (ch != '\r' && ch != '\n' && ch != '\u0085' && ch != '\u2028')
		{
			return ch == '\u2029';
		}
		return true;
	}

	internal static bool IsHexDigit(char c)
	{
		if ((c < '0' || c > '9') && (c < 'A' || c > 'F'))
		{
			if (c >= 'a')
			{
				return c <= 'f';
			}
			return false;
		}
		return true;
	}

	internal static bool IsBinaryDigit(char c)
	{
		return c == '0' || c == '1';
	}

	internal static bool IsDecDigit(char c)
	{
		if (c >= '0')
		{
			return c <= '9';
		}
		return false;
	}

	internal static int HexValue(char c)
	{
		DebugAssertHelper.Assert(IsHexDigit(c));
		if (c < '0' || c > '9')
		{
			return (c & 0xDF) - 65 + 10;
		}
		return c - 48;
	}

	internal static int BinaryValue(char c)
	{
		DebugAssertHelper.Assert(IsBinaryDigit(c));
		return c - 48;
	}

	internal static int DecValue(char c)
	{
		DebugAssertHelper.Assert(IsDecDigit(c));
		return c - 48;
	}

	internal static bool IsNonAsciiQuotationMark(char ch)
	{
		switch (ch)
		{
		case ''':
		case ''':
			return true;
		case '"':
		case '"':
			return true;
		default:
			return false;
		}
	}

	public static bool IsTokenIdentifier(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return kind.IsKeywordAllowedIdentifier();
		}
		return true;
	}

	public static bool IsCompoundAssignmentOperatorToken(SyntaxKind token)
	{
		if (token - 23 <= SyntaxKind.Int64LiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool CanGetNameRecommended(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PageField:
		case SyntaxKind.ReportDataItem:
		case SyntaxKind.ReportColumn:
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.QueryDataItem:
		case SyntaxKind.QueryColumn:
		case SyntaxKind.QueryFilter:
			return true;
		default:
			return false;
		}
	}

	public static bool IsLiteral(this SyntaxKind kind)
	{
		if (kind - 2 <= SyntaxKind.XmlEntityLiteralToken || kind - 14 <= SyntaxKind.EmptyToken || kind - 237 <= SyntaxKind.DateTimeLiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsDateTimeLiteral(this SyntaxKind kind)
	{
		if (kind - 5 <= SyntaxKind.Int32LiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumericLiteral(this SyntaxKind kind)
	{
		if (kind - 2 <= SyntaxKind.EmptyToken || kind == SyntaxKind.Int32PropertyValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsStringLiteral(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.StringLiteralToken || kind == SyntaxKind.StringPropertyValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsBooleanLiteral(this SyntaxKind kind)
	{
		if (kind - 14 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsFilterTokenUnary(this SyntaxKind kind)
	{
		if (kind - 27 <= SyntaxKind.DateLiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsAnyToken(this SyntaxKind kind)
	{
		if ((int)kind > 0)
		{
			return (int)kind < 227;
		}
		return false;
	}

	public static bool IsBinaryComparisonExpressionOperatorToken(SyntaxKind token)
	{
		if (token - 27 <= SyntaxKind.DateLiteralToken || token == SyntaxKind.InKeyword || token - 182 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnaryFilterExpressionValueToken(SyntaxKind token)
	{
		if (token == SyntaxKind.GreaterThanEqualsToken || token - 507 <= SyntaxKind.DateLiteralToken || token == SyntaxKind.RangeToFilterExpression)
		{
			return true;
		}
		return false;
	}

	public static bool IsBinaryFilterExpressionValueToken(SyntaxKind token)
	{
		if (token - 513 <= SyntaxKind.Int32LiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsBinaryFilterExpressionToken(this SyntaxKind token)
	{
		if (token == SyntaxKind.DotDotToken || token - 165 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public static SyntaxKind ToBinaryFilterExpressionKind(this SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.AndFilterKeyword => SyntaxKind.AndFilterExpression, 
			SyntaxKind.OrFilterKeyword => SyntaxKind.OrFilterExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeBetweenFilterExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static SyntaxKind ToUnaryFilterExpressionKind(this SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.NotEqualsToken => SyntaxKind.UnaryNotEqualsFilterExpression, 
			SyntaxKind.EqualsToken => SyntaxKind.UnaryEqualsFilterExpression, 
			SyntaxKind.LessThanToken => SyntaxKind.UnaryLessThanFilterExpression, 
			SyntaxKind.LessThanEqualsToken => SyntaxKind.UnaryLessThanEqualsFilterExpression, 
			SyntaxKind.GreaterThanToken => SyntaxKind.UnaryGreaterThanFilterExpression, 
			SyntaxKind.GreaterThanEqualsToken => SyntaxKind.UnaryGreaterThanEqualsFilterExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeToFilterExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsKeyword(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.IDivKeyword:
		case SyntaxKind.ModuloKeyword:
		case SyntaxKind.AndKeyword:
		case SyntaxKind.OrKeyword:
		case SyntaxKind.XorKeyword:
		case SyntaxKind.NotKeyword:
		case SyntaxKind.ExitKeyword:
		case SyntaxKind.BeginKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.DownToKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.EndKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.OfKeyword:
		case SyntaxKind.RepeatKeyword:
		case SyntaxKind.ThenKeyword:
		case SyntaxKind.ToKeyword:
		case SyntaxKind.UntilKeyword:
		case SyntaxKind.WithKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ProgramKeyword:
		case SyntaxKind.ProcedureKeyword:
		case SyntaxKind.FunctionKeyword:
		case SyntaxKind.VarKeyword:
		case SyntaxKind.ArrayKeyword:
		case SyntaxKind.TemporaryKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.AssertErrorKeyword:
		case SyntaxKind.SuppressDisposeKeyword:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.TriggerKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableDataKeyword:
		case SyntaxKind.SystemKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.ProfileKeyword:
		case SyntaxKind.ProfileExtensionKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.PageCustomizationKeyword:
		case SyntaxKind.CustomizesKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.FieldKeyword:
		case SyntaxKind.AssemblyKeyword:
		case SyntaxKind.TypeKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.FieldGroupsKeyword:
		case SyntaxKind.FieldGroupKeyword:
		case SyntaxKind.KeysKeyword:
		case SyntaxKind.KeyKeyword:
		case SyntaxKind.LayoutKeyword:
		case SyntaxKind.PageAreaKeyword:
		case SyntaxKind.PageGroupKeyword:
		case SyntaxKind.PageRepeaterKeyword:
		case SyntaxKind.PageCueGroupKeyword:
		case SyntaxKind.PageFixedKeyword:
		case SyntaxKind.PageGridKeyword:
		case SyntaxKind.PagePartKeyword:
		case SyntaxKind.PageSystemPartKeyword:
		case SyntaxKind.PageChartPartKeyword:
		case SyntaxKind.PageUserControlKeyword:
		case SyntaxKind.ActionsKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.ActionRefKeyword:
		case SyntaxKind.CustomActionKeyword:
		case SyntaxKind.SystemActionKeyword:
		case SyntaxKind.FileUploadActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.TableExtensionKeyword:
		case SyntaxKind.PageExtensionKeyword:
		case SyntaxKind.ExtendsKeyword:
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		case SyntaxKind.ModifyKeyword:
		case SyntaxKind.DataSetKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.LabelsKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.RequestPageKeyword:
		case SyntaxKind.XmlPortSchemaKeyword:
		case SyntaxKind.XmlPortTableElementKeyword:
		case SyntaxKind.XmlPortFieldElementKeyword:
		case SyntaxKind.XmlPortTextElementKeyword:
		case SyntaxKind.XmlPortFieldAttributeKeyword:
		case SyntaxKind.XmlPortTextAttributeKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.QueryElementsKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.EnumExtensionKeyword:
		case SyntaxKind.EnumValueKeyword:
		case SyntaxKind.ViewsKeyword:
		case SyntaxKind.ViewKeyword:
		case SyntaxKind.AnalysisViewsKeyword:
		case SyntaxKind.AnalysisViewKeyword:
		case SyntaxKind.ReportExtensionKeyword:
		case SyntaxKind.AddKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.ImplementsKeyword:
		case SyntaxKind.PermissionSetKeyword:
		case SyntaxKind.PermissionSetExtensionKeyword:
		case SyntaxKind.EntitlementKeyword:
		case SyntaxKind.RenderingKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.ThisKeyword:
		case SyntaxKind.WhereFormulaKeyword:
		case SyntaxKind.FieldFormulaKeyword:
		case SyntaxKind.ConstFormulaKeyword:
		case SyntaxKind.FilterFormulaKeyword:
		case SyntaxKind.UpperLimitFormulaKeyword:
		case SyntaxKind.ExistCalculationFormulaKeyword:
		case SyntaxKind.CountCalculationFormulaKeyword:
		case SyntaxKind.SumCalculationFormulaKeyword:
		case SyntaxKind.AverageCalculationFormulaKeyword:
		case SyntaxKind.MinCalculationFormulaKeyword:
		case SyntaxKind.MaxCalculationFormulaKeyword:
		case SyntaxKind.LookupCalculationFormulaKeyword:
		case SyntaxKind.OrderKeyword:
		case SyntaxKind.SortingKeyword:
		case SyntaxKind.AscendingKeyword:
		case SyntaxKind.DescendingKeyword:
		case SyntaxKind.ElifKeyword:
		case SyntaxKind.EndIfKeyword:
		case SyntaxKind.RegionKeyword:
		case SyntaxKind.EndRegionKeyword:
		case SyntaxKind.DefineKeyword:
		case SyntaxKind.UndefKeyword:
		case SyntaxKind.PragmaKeyword:
		case SyntaxKind.WarningKeyword:
		case SyntaxKind.DisableKeyword:
		case SyntaxKind.RestoreKeyword:
		case SyntaxKind.EnableKeyword:
		case SyntaxKind.ImplicitWithKeyword:
		case SyntaxKind.ActionsV2Keyword:
		case SyntaxKind.NamespaceKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		default:
			return false;
		}
	}

	public static SyntaxKind GetALKeywordKind(string text)
	{
		return text switch
		{
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"DIV" => SyntaxKind.IDivKeyword, 
			"MOD" => SyntaxKind.ModuloKeyword, 
			"AND" => SyntaxKind.AndKeyword, 
			"OR" => SyntaxKind.OrKeyword, 
			"XOR" => SyntaxKind.XorKeyword, 
			"NOT" => SyntaxKind.NotKeyword, 
			"EXIT" => SyntaxKind.ExitKeyword, 
			"BEGIN" => SyntaxKind.BeginKeyword, 
			"CASE" => SyntaxKind.CaseKeyword, 
			"DO" => SyntaxKind.DoKeyword, 
			"DOWNTO" => SyntaxKind.DownToKeyword, 
			"ELSE" => SyntaxKind.ElseKeyword, 
			"END" => SyntaxKind.EndKeyword, 
			"FOR" => SyntaxKind.ForKeyword, 
			"FOREACH" => SyntaxKind.ForEachKeyword, 
			"IF" => SyntaxKind.IfKeyword, 
			"IN" => SyntaxKind.InKeyword, 
			"OF" => SyntaxKind.OfKeyword, 
			"REPEAT" => SyntaxKind.RepeatKeyword, 
			"THEN" => SyntaxKind.ThenKeyword, 
			"TO" => SyntaxKind.ToKeyword, 
			"UNTIL" => SyntaxKind.UntilKeyword, 
			"WITH" => SyntaxKind.WithKeyword, 
			"WHILE" => SyntaxKind.WhileKeyword, 
			"ASSERTERROR" => SyntaxKind.AssertErrorKeyword, 
			"VAR" => SyntaxKind.VarKeyword, 
			"TRIGGER" => SyntaxKind.TriggerKeyword, 
			"PROCEDURE" => SyntaxKind.ProcedureKeyword, 
			"LOCAL" => SyntaxKind.LocalKeyword, 
			"INTERNAL" => SyntaxKind.InternalKeyword, 
			"PROTECTED" => SyntaxKind.ProtectedKeyword, 
			"BREAK" => SyntaxKind.BreakKeyword, 
			"EVENT" => SyntaxKind.EventKeyword, 
			"AS" => SyntaxKind.AsKeyword, 
			"IS" => SyntaxKind.IsKeyword, 
			"THIS" => SyntaxKind.ThisKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static SyntaxKind GetPropertyKeywordKind(string text)
	{
		SyntaxKind objectKeyword = GetObjectKeyword(text);
		if (objectKeyword != 0)
		{
			return objectKeyword;
		}
		return text switch
		{
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"IF" => SyntaxKind.IfKeyword, 
			"ELSE" => SyntaxKind.ElseKeyword, 
			"WHERE" => SyntaxKind.WhereFormulaKeyword, 
			"FIELD" => SyntaxKind.FieldFormulaKeyword, 
			"CONST" => SyntaxKind.ConstFormulaKeyword, 
			"FILTER" => SyntaxKind.FilterFormulaKeyword, 
			"UPPERLIMIT" => SyntaxKind.UpperLimitFormulaKeyword, 
			"EXIST" => SyntaxKind.ExistCalculationFormulaKeyword, 
			"LOOKUP" => SyntaxKind.LookupCalculationFormulaKeyword, 
			"MIN" => SyntaxKind.MinCalculationFormulaKeyword, 
			"MAX" => SyntaxKind.MaxCalculationFormulaKeyword, 
			"AVERAGE" => SyntaxKind.AverageCalculationFormulaKeyword, 
			"SUM" => SyntaxKind.SumCalculationFormulaKeyword, 
			"COUNT" => SyntaxKind.CountCalculationFormulaKeyword, 
			"TABLEDATA" => SyntaxKind.TableDataKeyword, 
			"SYSTEM" => SyntaxKind.SystemKeyword, 
			"SORTING" => SyntaxKind.SortingKeyword, 
			"ORDER" => SyntaxKind.OrderKeyword, 
			"ASCENDING" => SyntaxKind.AscendingKeyword, 
			"DESCENDING" => SyntaxKind.DescendingKeyword, 
			"ELIF" => SyntaxKind.ElifKeyword, 
			"ENDIF" => SyntaxKind.EndIfKeyword, 
			"REGION" => SyntaxKind.RegionKeyword, 
			"ENDREGION" => SyntaxKind.EndRegionKeyword, 
			"DEFINE" => SyntaxKind.DefineKeyword, 
			"UNDEF" => SyntaxKind.UndefKeyword, 
			"PRAGMA" => SyntaxKind.PragmaKeyword, 
			"WARNING" => SyntaxKind.WarningKeyword, 
			"DISABLE" => SyntaxKind.DisableKeyword, 
			"RESTORE" => SyntaxKind.RestoreKeyword, 
			"ENABLE" => SyntaxKind.RestoreKeyword, 
			"IMPLICITWITH" => SyntaxKind.ImplicitWithKeyword, 
			"ACTIONSV2" => SyntaxKind.ActionsV2Keyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static SyntaxKind GetObjectKeywordKind(string text)
	{
		SyntaxKind objectKeyword = GetObjectKeyword(text);
		if (objectKeyword != 0)
		{
			return objectKeyword;
		}
		return text switch
		{
			"EVENT" => SyntaxKind.EventKeyword, 
			"TEMPORARY" => SyntaxKind.TemporaryKeyword, 
			"TRIGGER" => SyntaxKind.TriggerKeyword, 
			"PROGRAM" => SyntaxKind.ProgramKeyword, 
			"PROCEDURE" => SyntaxKind.ProcedureKeyword, 
			"FUNCTION" => SyntaxKind.FunctionKeyword, 
			"VAR" => SyntaxKind.VarKeyword, 
			"ARRAY" => SyntaxKind.ArrayKeyword, 
			"OF" => SyntaxKind.OfKeyword, 
			"LOCAL" => SyntaxKind.LocalKeyword, 
			"INTERNAL" => SyntaxKind.InternalKeyword, 
			"PROTECTED" => SyntaxKind.ProtectedKeyword, 
			"SUPPRESSDISPOSE" => SyntaxKind.SuppressDisposeKeyword, 
			"SECURITYFILTERING" => SyntaxKind.SecurityFilteringKeyword, 
			"FIELDS" => SyntaxKind.FieldsKeyword, 
			"FIELD" => SyntaxKind.FieldKeyword, 
			"KEYS" => SyntaxKind.KeysKeyword, 
			"KEY" => SyntaxKind.KeyKeyword, 
			"FIELDGROUPS" => SyntaxKind.FieldGroupsKeyword, 
			"FIELDGROUP" => SyntaxKind.FieldGroupKeyword, 
			"EXTENDS" => SyntaxKind.ExtendsKeyword, 
			"ADD" => SyntaxKind.AddKeyword, 
			"ADDFIRST" => SyntaxKind.AddFirstKeyword, 
			"ADDLAST" => SyntaxKind.AddLastKeyword, 
			"ADDBEFORE" => SyntaxKind.AddBeforeKeyword, 
			"ADDAFTER" => SyntaxKind.AddAfterKeyword, 
			"MOVEFIRST" => SyntaxKind.MoveFirstKeyword, 
			"MOVELAST" => SyntaxKind.MoveLastKeyword, 
			"MOVEBEFORE" => SyntaxKind.MoveBeforeKeyword, 
			"MOVEAFTER" => SyntaxKind.MoveAfterKeyword, 
			"MODIFY" => SyntaxKind.ModifyKeyword, 
			"LAYOUT" => SyntaxKind.LayoutKeyword, 
			"AREA" => SyntaxKind.PageAreaKeyword, 
			"GROUP" => SyntaxKind.PageGroupKeyword, 
			"REPEATER" => SyntaxKind.PageRepeaterKeyword, 
			"CUEGROUP" => SyntaxKind.PageCueGroupKeyword, 
			"FIXED" => SyntaxKind.PageFixedKeyword, 
			"GRID" => SyntaxKind.PageGridKeyword, 
			"PART" => SyntaxKind.PagePartKeyword, 
			"SYSTEMPART" => SyntaxKind.PageSystemPartKeyword, 
			"CHARTPART" => SyntaxKind.PageChartPartKeyword, 
			"USERCONTROL" => SyntaxKind.PageUserControlKeyword, 
			"ACTIONS" => SyntaxKind.ActionsKeyword, 
			"ACTION" => SyntaxKind.ActionKeyword, 
			"ACTIONREF" => SyntaxKind.ActionRefKeyword, 
			"CUSTOMACTION" => SyntaxKind.CustomActionKeyword, 
			"SEPARATOR" => SyntaxKind.SeparatorKeyword, 
			"SYSTEMACTION" => SyntaxKind.SystemActionKeyword, 
			"FILEUPLOADACTION" => SyntaxKind.FileUploadActionKeyword, 
			"VIEWS" => SyntaxKind.ViewsKeyword, 
			"VIEW" => SyntaxKind.ViewKeyword, 
			"ANALYSISVIEWS" => SyntaxKind.AnalysisViewsKeyword, 
			"ANALYSISVIEW" => SyntaxKind.AnalysisViewKeyword, 
			"DATASET" => SyntaxKind.DataSetKeyword, 
			"DATAITEM" => SyntaxKind.DataItemKeyword, 
			"COLUMN" => SyntaxKind.ColumnKeyword, 
			"LABELS" => SyntaxKind.LabelsKeyword, 
			"LABEL" => SyntaxKind.LabelKeyword, 
			"RENDERING" => SyntaxKind.RenderingKeyword, 
			"REQUESTPAGE" => SyntaxKind.RequestPageKeyword, 
			"ELEMENTS" => SyntaxKind.QueryElementsKeyword, 
			"FILTER" => SyntaxKind.FilterKeyword, 
			"SCHEMA" => SyntaxKind.XmlPortSchemaKeyword, 
			"TABLEELEMENT" => SyntaxKind.XmlPortTableElementKeyword, 
			"FIELDELEMENT" => SyntaxKind.XmlPortFieldElementKeyword, 
			"TEXTELEMENT" => SyntaxKind.XmlPortTextElementKeyword, 
			"FIELDATTRIBUTE" => SyntaxKind.XmlPortFieldAttributeKeyword, 
			"TEXTATTRIBUTE" => SyntaxKind.XmlPortTextAttributeKeyword, 
			"CUSTOMIZES" => SyntaxKind.CustomizesKeyword, 
			"BEGIN" => SyntaxKind.BeginKeyword, 
			"END" => SyntaxKind.EndKeyword, 
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"ASSEMBLY" => SyntaxKind.AssemblyKeyword, 
			"TYPE" => SyntaxKind.TypeKeyword, 
			"ENUM" => SyntaxKind.EnumKeyword, 
			"VALUE" => SyntaxKind.EnumValueKeyword, 
			"ENUMEXTENSION" => SyntaxKind.EnumExtensionKeyword, 
			"NAMESPACE" => SyntaxKind.NamespaceKeyword, 
			"USING" => SyntaxKind.UsingKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static SyntaxKind GetDirectiveKeywordKind(string text)
	{
		return text switch
		{
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"IF" => SyntaxKind.IfKeyword, 
			"ELSE" => SyntaxKind.ElseKeyword, 
			"ELIF" => SyntaxKind.ElifKeyword, 
			"ENDIF" => SyntaxKind.EndIfKeyword, 
			"REGION" => SyntaxKind.RegionKeyword, 
			"ENDREGION" => SyntaxKind.EndRegionKeyword, 
			"DEFINE" => SyntaxKind.DefineKeyword, 
			"UNDEF" => SyntaxKind.UndefKeyword, 
			"PRAGMA" => SyntaxKind.PragmaKeyword, 
			"WARNING" => SyntaxKind.WarningKeyword, 
			"DISABLE" => SyntaxKind.DisableKeyword, 
			"RESTORE" => SyntaxKind.RestoreKeyword, 
			"ENABLE" => SyntaxKind.EnableKeyword, 
			"AND" => SyntaxKind.AndKeyword, 
			"OR" => SyntaxKind.OrKeyword, 
			"NOT" => SyntaxKind.NotKeyword, 
			"IMPLICITWITH" => SyntaxKind.ImplicitWithKeyword, 
			"ACTIONSV2" => SyntaxKind.ActionsV2Keyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsControlKeyword(this SyntaxKind kind)
	{
		if (kind - 65 <= SyntaxKind.GreaterThanToken || kind - 115 <= SyntaxKind.EmptyToken || kind - 182 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsControlFlowKeyword(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ExitKeyword:
		case SyntaxKind.BeginKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.DownToKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.EndKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.OfKeyword:
		case SyntaxKind.RepeatKeyword:
		case SyntaxKind.ThenKeyword:
		case SyntaxKind.ToKeyword:
		case SyntaxKind.UntilKeyword:
		case SyntaxKind.WithKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ArrayKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.IsKeyword:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortNodeKeyword(this SyntaxKind kind)
	{
		if (kind - 158 <= SyntaxKind.DecimalLiteralToken)
		{
			return true;
		}
		return false;
	}

	private static bool IsOperatorKeyword(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.RDivToken:
		case SyntaxKind.PlusToken:
		case SyntaxKind.MinusToken:
		case SyntaxKind.MultiplyToken:
		case SyntaxKind.IDivKeyword:
		case SyntaxKind.ModuloKeyword:
		case SyntaxKind.AssignToken:
		case SyntaxKind.AssignRDivToken:
		case SyntaxKind.AssignPlusToken:
		case SyntaxKind.AssignMinusToken:
		case SyntaxKind.AssignMultiplyToken:
		case SyntaxKind.LessThanToken:
		case SyntaxKind.LessThanEqualsToken:
		case SyntaxKind.NotEqualsToken:
		case SyntaxKind.EqualsToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.GreaterThanEqualsToken:
		case SyntaxKind.DotDotToken:
		case SyntaxKind.AndKeyword:
		case SyntaxKind.OrKeyword:
		case SyntaxKind.XorKeyword:
		case SyntaxKind.NotKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.IsKeyword:
			return true;
		default:
			return false;
		}
	}

	public static bool IsOperatorToken(this SyntaxToken token)
	{
		if (!token.Kind.IsOperatorKeyword())
		{
			if (token.IsKind(SyntaxKind.QuestionToken, SyntaxKind.ColonToken))
			{
				return token.ParentIsKind(SyntaxKind.ConditionalExpression);
			}
			return false;
		}
		return true;
	}

	public static bool IsLogicalOperator(this SyntaxKind kind)
	{
		if (kind - 61 <= SyntaxKind.Int64LiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsMethodOrTriggerDeclaration(this SyntaxKind kind)
	{
		return kind.IsMethodOrTriggerDeclarationSyntax();
	}

	public static bool IsPropertyKeyword(this SyntaxKind kind)
	{
		if (kind - 99 <= SyntaxKind.EmptyToken || kind - 185 <= SyntaxKind.TrueKeyword)
		{
			return true;
		}
		return false;
	}

	public static bool IsMetadataDefinitionKeyword(this SyntaxKind kind)
	{
		if (kind - 111 <= SyntaxKind.Int64LiteralToken || kind - 117 <= SyntaxKind.ModuloKeyword || kind - 170 <= SyntaxKind.Int64LiteralToken)
		{
			return true;
		}
		return false;
	}

	public static string GetText(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.RDivToken => "/", 
			SyntaxKind.PlusToken => "+", 
			SyntaxKind.MinusToken => "-", 
			SyntaxKind.MultiplyToken => "*", 
			SyntaxKind.IDivKeyword => "div", 
			SyntaxKind.ModuloKeyword => "mod", 
			SyntaxKind.AssignToken => ":=", 
			SyntaxKind.AssignRDivToken => "/=", 
			SyntaxKind.AssignPlusToken => "+=", 
			SyntaxKind.AssignMinusToken => "-=", 
			SyntaxKind.AssignMultiplyToken => "*=", 
			SyntaxKind.LessThanToken => "<", 
			SyntaxKind.LessThanEqualsToken => "<=", 
			SyntaxKind.NotEqualsToken => "<>", 
			SyntaxKind.EqualsToken => "=", 
			SyntaxKind.GreaterThanToken => ">", 
			SyntaxKind.GreaterThanEqualsToken => ">=", 
			SyntaxKind.CommaToken => ",", 
			SyntaxKind.DotToken => ".", 
			SyntaxKind.ColonToken => ":", 
			SyntaxKind.SemicolonToken => ";", 
			SyntaxKind.ColonColonToken => "::", 
			SyntaxKind.DotDotToken => "..", 
			SyntaxKind.AtToken => "@", 
			SyntaxKind.HashToken => "#", 
			SyntaxKind.OpenParenToken => "(", 
			SyntaxKind.CloseParenToken => ")", 
			SyntaxKind.OpenBracketToken => "[", 
			SyntaxKind.CloseBracketToken => "]", 
			SyntaxKind.OpenBraceToken => "{", 
			SyntaxKind.CloseBraceToken => "}", 
			SyntaxKind.DoubleQuoteToken => "\"", 
			SyntaxKind.SingleQuoteToken => "'", 
			SyntaxKind.QuestionToken => "?", 
			SyntaxKind.AmpersandToken => "&", 
			SyntaxKind.BarToken => "|", 
			SyntaxKind.CaretToken => "^", 
			SyntaxKind.PercentToken => "%", 
			SyntaxKind.TildeToken => "~", 
			SyntaxKind.MinusMinusToken => "--", 
			SyntaxKind.PlusPlusToken => "++", 
			SyntaxKind.EqualsEqualsToken => "==", 
			SyntaxKind.ExclamationEqualsToken => "!=", 
			SyntaxKind.ExclamationToken => "!", 
			SyntaxKind.SlashGreaterThanToken => "/>", 
			SyntaxKind.LessThanSlashToken => "</", 
			SyntaxKind.XmlCommentStartToken => "<!--", 
			SyntaxKind.XmlCommentEndToken => "-->", 
			SyntaxKind.XmlCDataStartToken => "<![CDATA[", 
			SyntaxKind.XmlCDataEndToken => "]]>", 
			SyntaxKind.XmlProcessingInstructionStartToken => "<?", 
			SyntaxKind.XmlProcessingInstructionEndToken => "?>", 
			SyntaxKind.TrueKeyword => "true", 
			SyntaxKind.FalseKeyword => "false", 
			SyntaxKind.AndKeyword => "and", 
			SyntaxKind.OrKeyword => "or", 
			SyntaxKind.XorKeyword => "xor", 
			SyntaxKind.NotKeyword => "not", 
			SyntaxKind.ExitKeyword => "exit", 
			SyntaxKind.BeginKeyword => "begin", 
			SyntaxKind.CaseKeyword => "case", 
			SyntaxKind.DoKeyword => "do", 
			SyntaxKind.DownToKeyword => "downto", 
			SyntaxKind.ElseKeyword => "else", 
			SyntaxKind.EndKeyword => "end", 
			SyntaxKind.ForKeyword => "for", 
			SyntaxKind.IfKeyword => "if", 
			SyntaxKind.InKeyword => "in", 
			SyntaxKind.OfKeyword => "of", 
			SyntaxKind.RepeatKeyword => "repeat", 
			SyntaxKind.ThenKeyword => "then", 
			SyntaxKind.ToKeyword => "to", 
			SyntaxKind.UntilKeyword => "until", 
			SyntaxKind.WithKeyword => "with", 
			SyntaxKind.WhileKeyword => "while", 
			SyntaxKind.ProgramKeyword => "program", 
			SyntaxKind.ProcedureKeyword => "procedure", 
			SyntaxKind.FunctionKeyword => "function", 
			SyntaxKind.VarKeyword => "var", 
			SyntaxKind.ArrayKeyword => "array", 
			SyntaxKind.TemporaryKeyword => "temporary", 
			SyntaxKind.LocalKeyword => "local", 
			SyntaxKind.InternalKeyword => "internal", 
			SyntaxKind.ProtectedKeyword => "protected", 
			SyntaxKind.BreakKeyword => "break", 
			SyntaxKind.ContinueKeyword => "continue", 
			SyntaxKind.EventKeyword => "event", 
			SyntaxKind.AssertErrorKeyword => "asserterror", 
			SyntaxKind.SuppressDisposeKeyword => "suppressdispose", 
			SyntaxKind.SecurityFilteringKeyword => "securityfiltering", 
			SyntaxKind.ForEachKeyword => "foreach", 
			SyntaxKind.TriggerKeyword => "trigger", 
			SyntaxKind.CodeunitKeyword => "codeunit", 
			SyntaxKind.TableKeyword => "table", 
			SyntaxKind.TableDataKeyword => "tabledata", 
			SyntaxKind.SystemKeyword => "system", 
			SyntaxKind.QueryKeyword => "query", 
			SyntaxKind.PageKeyword => "page", 
			SyntaxKind.PageExtensionKeyword => "pageextension", 
			SyntaxKind.TableExtensionKeyword => "tableextension", 
			SyntaxKind.ReportKeyword => "report", 
			SyntaxKind.ReportExtensionKeyword => "reportextension", 
			SyntaxKind.XmlPortKeyword => "xmlport", 
			SyntaxKind.ProfileKeyword => "profile", 
			SyntaxKind.ProfileExtensionKeyword => "profileextension", 
			SyntaxKind.InterfaceKeyword => "interface", 
			SyntaxKind.ImplementsKeyword => "implements", 
			SyntaxKind.ControlAddInKeyword => "controladdin", 
			SyntaxKind.PageCustomizationKeyword => "pagecustomization", 
			SyntaxKind.DotNetKeyword => "dotnet", 
			SyntaxKind.AssemblyKeyword => "assembly", 
			SyntaxKind.TypeKeyword => "type", 
			SyntaxKind.FieldsKeyword => "fields", 
			SyntaxKind.FieldKeyword => "field", 
			SyntaxKind.KeysKeyword => "keys", 
			SyntaxKind.KeyKeyword => "key", 
			SyntaxKind.FieldGroupsKeyword => "fieldgroups", 
			SyntaxKind.FieldGroupKeyword => "fieldgroup", 
			SyntaxKind.ThisKeyword => "this", 
			SyntaxKind.LayoutKeyword => "layout", 
			SyntaxKind.PageAreaKeyword => "area", 
			SyntaxKind.PageGroupKeyword => "group", 
			SyntaxKind.PageRepeaterKeyword => "repeater", 
			SyntaxKind.PageCueGroupKeyword => "cuegroup", 
			SyntaxKind.PageFixedKeyword => "fixed", 
			SyntaxKind.PageGridKeyword => "grid", 
			SyntaxKind.PagePartKeyword => "part", 
			SyntaxKind.PageSystemPartKeyword => "systempart", 
			SyntaxKind.PageChartPartKeyword => "chartpart", 
			SyntaxKind.PageUserControlKeyword => "usercontrol", 
			SyntaxKind.ActionsKeyword => "actions", 
			SyntaxKind.ActionKeyword => "action", 
			SyntaxKind.ActionRefKeyword => "actionref", 
			SyntaxKind.CustomActionKeyword => "customaction", 
			SyntaxKind.SeparatorKeyword => "separator", 
			SyntaxKind.SystemActionKeyword => "systemaction", 
			SyntaxKind.FileUploadActionKeyword => "fileuploadaction", 
			SyntaxKind.ViewsKeyword => "views", 
			SyntaxKind.ViewKeyword => "view", 
			SyntaxKind.AnalysisViewsKeyword => "analysisviews", 
			SyntaxKind.AnalysisViewKeyword => "analysisview", 
			SyntaxKind.ExtendsKeyword => "extends", 
			SyntaxKind.AddKeyword => "add", 
			SyntaxKind.AddFirstKeyword => "addfirst", 
			SyntaxKind.AddLastKeyword => "addlast", 
			SyntaxKind.AddBeforeKeyword => "addbefore", 
			SyntaxKind.AddAfterKeyword => "addafter", 
			SyntaxKind.MoveFirstKeyword => "movefirst", 
			SyntaxKind.MoveLastKeyword => "movelast", 
			SyntaxKind.MoveBeforeKeyword => "movebefore", 
			SyntaxKind.MoveAfterKeyword => "moveafter", 
			SyntaxKind.ModifyKeyword => "modify", 
			SyntaxKind.DataSetKeyword => "dataset", 
			SyntaxKind.DataItemKeyword => "dataitem", 
			SyntaxKind.ColumnKeyword => "column", 
			SyntaxKind.LabelsKeyword => "labels", 
			SyntaxKind.LabelKeyword => "label", 
			SyntaxKind.RequestPageKeyword => "requestpage", 
			SyntaxKind.RenderingKeyword => "rendering", 
			SyntaxKind.XmlPortSchemaKeyword => "schema", 
			SyntaxKind.XmlPortTableElementKeyword => "tableelement", 
			SyntaxKind.XmlPortFieldElementKeyword => "fieldelement", 
			SyntaxKind.XmlPortTextElementKeyword => "textelement", 
			SyntaxKind.XmlPortFieldAttributeKeyword => "fieldattribute", 
			SyntaxKind.XmlPortTextAttributeKeyword => "textattribute", 
			SyntaxKind.QueryElementsKeyword => "elements", 
			SyntaxKind.FilterKeyword => "filter", 
			SyntaxKind.WhereFormulaKeyword => "where", 
			SyntaxKind.FieldFormulaKeyword => "field", 
			SyntaxKind.ConstFormulaKeyword => "const", 
			SyntaxKind.FilterFormulaKeyword => "filter", 
			SyntaxKind.UpperLimitFormulaKeyword => "upperlimit", 
			SyntaxKind.AverageCalculationFormulaKeyword => "average", 
			SyntaxKind.CountCalculationFormulaKeyword => "count", 
			SyntaxKind.ExistCalculationFormulaKeyword => "exist", 
			SyntaxKind.LookupCalculationFormulaKeyword => "lookup", 
			SyntaxKind.MinCalculationFormulaKeyword => "min", 
			SyntaxKind.MaxCalculationFormulaKeyword => "max", 
			SyntaxKind.SumCalculationFormulaKeyword => "sum", 
			SyntaxKind.AndFilterKeyword => "&", 
			SyntaxKind.OrFilterKeyword => "|", 
			SyntaxKind.SortingKeyword => "sorting", 
			SyntaxKind.OrderKeyword => "order", 
			SyntaxKind.AscendingKeyword => "ascending", 
			SyntaxKind.DescendingKeyword => "descending", 
			SyntaxKind.CustomizesKeyword => "customizes", 
			SyntaxKind.EnumKeyword => "enum", 
			SyntaxKind.EnumValueKeyword => "value", 
			SyntaxKind.EnumExtensionKeyword => "enumextension", 
			SyntaxKind.ElifKeyword => "elif", 
			SyntaxKind.EndIfKeyword => "endif", 
			SyntaxKind.RegionKeyword => "region", 
			SyntaxKind.EndRegionKeyword => "endregion", 
			SyntaxKind.DefineKeyword => "define", 
			SyntaxKind.UndefKeyword => "undef", 
			SyntaxKind.PragmaKeyword => "pragma", 
			SyntaxKind.WarningKeyword => "warning", 
			SyntaxKind.DisableKeyword => "disable", 
			SyntaxKind.RestoreKeyword => "restore", 
			SyntaxKind.EnableKeyword => "enable", 
			SyntaxKind.ImplicitWithKeyword => "implicitwith", 
			SyntaxKind.ActionsV2Keyword => "actionsv2", 
			SyntaxKind.NamespaceKeyword => "namespace", 
			SyntaxKind.UsingKeyword => "using", 
			SyntaxKind.PermissionSetKeyword => "permissionset", 
			SyntaxKind.PermissionSetExtensionKeyword => "permissionsetextension", 
			SyntaxKind.EntitlementKeyword => "entitlement", 
			SyntaxKind.AsKeyword => "as", 
			SyntaxKind.IsKeyword => "is", 
			_ => string.Empty, 
		};
	}

	public static bool IsObject(this SyntaxKind kind)
	{
		return kind.IsObjectSyntax();
	}

	public static bool IsObjectWithMemberDeclarations(this SyntaxKind kind)
	{
		if (!kind.IsApplicationObject())
		{
			return kind == SyntaxKind.ControlAddInObject;
		}
		return true;
	}

	public static bool IsApplicationObject(this SyntaxKind kind)
	{
		return kind.IsApplicationObjectSyntax();
	}

	public static bool IsApplicationObjectWithId(this SyntaxKind kind)
	{
		if (kind - 529 <= SyntaxKind.EmptyToken)
		{
			return false;
		}
		return kind.IsApplicationObject();
	}

	public static bool CanDefineTriggers(this SyntaxKind kind)
	{
		if (kind.IsApplicationObject())
		{
			return true;
		}
		switch (kind)
		{
		case SyntaxKind.Field:
		case SyntaxKind.FieldModification:
		case SyntaxKind.PageField:
		case SyntaxKind.PageUserControl:
		case SyntaxKind.PageAction:
		case SyntaxKind.PageSystemAction:
		case SyntaxKind.PageFileUploadAction:
		case SyntaxKind.ActionModifyChange:
		case SyntaxKind.ControlModifyChange:
		case SyntaxKind.ReportDataItem:
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
		case SyntaxKind.RequestPage:
		case SyntaxKind.RequestPageExtension:
		case SyntaxKind.ReportExtensionModifyChange:
			return true;
		default:
			return false;
		}
	}

	public static bool IsObjectKeyword(this SyntaxKind kind)
	{
		return SupportedObjectSyntaxKeywords.Contains(kind);
	}

	internal static bool IsSignedOrSignInvariantLiteralToken(this SyntaxKind kind)
	{
		if (kind - 2 <= SyntaxKind.TimeLiteralToken || kind - 14 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPossibleSignedLiteralToken(this SyntaxKind kind)
	{
		SyntaxKind syntaxKind = kind;
		if (syntaxKind == SyntaxKind.MinusToken)
		{
			return true;
		}
		if (syntaxKind.IsSignedOrSignInvariantLiteralToken())
		{
			return true;
		}
		return false;
	}

	internal static bool IsKeywordAllowedIdentifier(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ProgramKeyword:
		case SyntaxKind.FunctionKeyword:
		case SyntaxKind.ArrayKeyword:
		case SyntaxKind.TemporaryKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.SuppressDisposeKeyword:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableDataKeyword:
		case SyntaxKind.SystemKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.ProfileKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.PageCustomizationKeyword:
		case SyntaxKind.CustomizesKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.FieldKeyword:
		case SyntaxKind.AssemblyKeyword:
		case SyntaxKind.TypeKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.ContinueKeyword:
		case SyntaxKind.FieldGroupKeyword:
		case SyntaxKind.KeysKeyword:
		case SyntaxKind.LayoutKeyword:
		case SyntaxKind.PageGroupKeyword:
		case SyntaxKind.PageRepeaterKeyword:
		case SyntaxKind.PageFixedKeyword:
		case SyntaxKind.PageGridKeyword:
		case SyntaxKind.PagePartKeyword:
		case SyntaxKind.PageSystemPartKeyword:
		case SyntaxKind.PageUserControlKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.ActionRefKeyword:
		case SyntaxKind.CustomActionKeyword:
		case SyntaxKind.SystemActionKeyword:
		case SyntaxKind.FileUploadActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		case SyntaxKind.ModifyKeyword:
		case SyntaxKind.DataSetKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.LabelsKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.RequestPageKeyword:
		case SyntaxKind.XmlPortSchemaKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.QueryElementsKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.EnumExtensionKeyword:
		case SyntaxKind.EnumValueKeyword:
		case SyntaxKind.ViewsKeyword:
		case SyntaxKind.ViewKeyword:
		case SyntaxKind.AnalysisViewsKeyword:
		case SyntaxKind.AnalysisViewKeyword:
		case SyntaxKind.ReportExtensionKeyword:
		case SyntaxKind.AddKeyword:
		case SyntaxKind.InterfaceKeyword:
		case SyntaxKind.ImplementsKeyword:
		case SyntaxKind.PermissionSetKeyword:
		case SyntaxKind.PermissionSetExtensionKeyword:
		case SyntaxKind.EntitlementKeyword:
		case SyntaxKind.RenderingKeyword:
		case SyntaxKind.AsKeyword:
		case SyntaxKind.IsKeyword:
		case SyntaxKind.FieldFormulaKeyword:
		case SyntaxKind.FilterFormulaKeyword:
		case SyntaxKind.CountCalculationFormulaKeyword:
		case SyntaxKind.SumCalculationFormulaKeyword:
		case SyntaxKind.AverageCalculationFormulaKeyword:
		case SyntaxKind.MinCalculationFormulaKeyword:
		case SyntaxKind.MaxCalculationFormulaKeyword:
		case SyntaxKind.OrderKeyword:
		case SyntaxKind.ElifKeyword:
		case SyntaxKind.EndIfKeyword:
		case SyntaxKind.RegionKeyword:
		case SyntaxKind.EndRegionKeyword:
		case SyntaxKind.DefineKeyword:
		case SyntaxKind.UndefKeyword:
		case SyntaxKind.PragmaKeyword:
		case SyntaxKind.WarningKeyword:
		case SyntaxKind.DisableKeyword:
		case SyntaxKind.RestoreKeyword:
		case SyntaxKind.ImplicitWithKeyword:
		case SyntaxKind.ActionsV2Keyword:
		case SyntaxKind.NamespaceKeyword:
		case SyntaxKind.UsingKeyword:
			return true;
		default:
			return false;
		}
	}

	public static bool IsRootObject(this SyntaxKind kind)
	{
		if (kind.IsObject())
		{
			return !kind.IsNestedObjectSyntaxKind();
		}
		return false;
	}

	internal static IEnumerable<TriggerTypeInfo> GetDeclaredTriggerTypeInfos(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.QueryObject:
			return TriggerDefinitions.QueryTriggers.Values;
		case SyntaxKind.TableObject:
			return TriggerDefinitions.TableTriggers.Values;
		case SyntaxKind.TableExtensionObject:
			return TriggerDefinitions.TableExtensionTriggers.Values;
		case SyntaxKind.RequestPage:
		case SyntaxKind.PageObject:
			return TriggerDefinitions.PageTriggers.Values;
		case SyntaxKind.PageExtensionObject:
			return TriggerDefinitions.PageExtensionTriggers.Values;
		case SyntaxKind.RequestPageExtension:
			return TriggerDefinitions.RequestPageExtensionTriggers.Values;
		case SyntaxKind.PageCustomizationObject:
			return Enumerable.Empty<TriggerTypeInfo>();
		case SyntaxKind.CodeunitObject:
			return TriggerDefinitions.CodeunitTriggers.Values;
		case SyntaxKind.Field:
			return TriggerDefinitions.FieldTriggers.Values;
		case SyntaxKind.FieldModification:
			return TriggerDefinitions.FieldExtensionTriggers.Values;
		case SyntaxKind.PageAction:
		case SyntaxKind.PageSystemAction:
			return TriggerDefinitions.ActionTriggers.Values;
		case SyntaxKind.PageFileUploadAction:
			return TriggerDefinitions.FileUploadActionTriggers.Values;
		case SyntaxKind.ActionModifyChange:
			return TriggerDefinitions.ActionExtensionTriggers.Values;
		case SyntaxKind.PageField:
			return TriggerDefinitions.ControlTriggers.Values;
		case SyntaxKind.ControlModifyChange:
			return TriggerDefinitions.ControlExtensionTriggers.Values;
		case SyntaxKind.ReportObject:
			return TriggerDefinitions.ReportTriggers.Values;
		case SyntaxKind.ReportExtensionObject:
			return TriggerDefinitions.ReportExtensionTriggers.Values;
		case SyntaxKind.ReportExtensionModifyChange:
			return TriggerDefinitions.ReportExtensionDataSetModifyTriggers.Values;
		case SyntaxKind.ReportDataItem:
			return TriggerDefinitions.ReportDataItemTriggers.Values;
		case SyntaxKind.XmlPortObject:
			return TriggerDefinitions.XmlPortTriggers.Values;
		case SyntaxKind.XmlPortTableElement:
			return TriggerDefinitions.XmlPortTableElementTriggers.Values;
		case SyntaxKind.XmlPortFieldElement:
			return TriggerDefinitions.XmlPortFieldElementTriggers.Values;
		case SyntaxKind.XmlPortTextElement:
			return TriggerDefinitions.XmlPortTextElementTriggers.Values;
		case SyntaxKind.XmlPortFieldAttribute:
			return TriggerDefinitions.XmlPortFieldAttributeTriggers.Values;
		case SyntaxKind.XmlPortTextAttribute:
			return TriggerDefinitions.XmlPortTextAttributeTriggers.Values;
		default:
			return ImmutableArray<TriggerTypeInfo>.Empty;
		}
	}

	public static bool IsSupportedPermissionKeyword(this SyntaxKind kind)
	{
		if (kind - 97 <= SyntaxKind.DateTimeLiteralToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsAllowedVariableType(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.IdentifierToken:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.InterfaceKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsOptionType(InternalSyntaxToken type)
	{
		if (type.Kind == SyntaxKind.IdentifierToken)
		{
			return NavTypeExtensions.IsOptionType(type.ValueText);
		}
		return false;
	}

	internal static bool IsCalculationFormulaMethodKind(this SyntaxKind kind)
	{
		if (kind - 190 <= SyntaxKind.TimeLiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsCalculationFormulaStatementKind(this SyntaxKind kind)
	{
		if (kind - 455 <= SyntaxKind.TimeLiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageControlOrGroupKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.FieldKeyword || kind - 123 <= SyntaxKind.StringLiteralToken || kind == SyntaxKind.LabelKeyword)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageChangeKeyword(this SyntaxKind kind)
	{
		if (!kind.IsPageAddChangeKeyword() && !kind.IsPageMoveChangeKeyword())
		{
			return kind == SyntaxKind.ModifyKeyword;
		}
		return true;
	}

	internal static bool IsPageAddChangeKeyword(this SyntaxKind kind)
	{
		if (kind - 142 <= SyntaxKind.Int64LiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageMoveChangeKeyword(this SyntaxKind kind)
	{
		if (kind - 146 <= SyntaxKind.Int64LiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFieldGroupChangeKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.AddLastKeyword)
		{
			return true;
		}
		return false;
	}

	internal static bool IsChangeKind(this SyntaxKind kind)
	{
		if (!kind.IsFieldModificationChange() && !kind.IsControlChangeBaseSyntax() && !kind.IsActionChangeBaseSyntax() && !kind.IsViewChangeBaseSyntax() && !kind.IsReportExtensionDataSetChangeBaseSyntax() && !kind.IsFieldGroupChangeBaseSyntax())
		{
			return kind.IsAnalysisViewChangeBaseSyntax();
		}
		return true;
	}

	internal static ChangeKind ToChangeKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AddFirstKeyword => ChangeKind.AddFirst, 
			SyntaxKind.AddLastKeyword => ChangeKind.AddLast, 
			SyntaxKind.AddBeforeKeyword => ChangeKind.AddBefore, 
			SyntaxKind.AddAfterKeyword => ChangeKind.AddAfter, 
			SyntaxKind.AddKeyword => ChangeKind.Add, 
			SyntaxKind.MoveFirstKeyword => ChangeKind.MoveFirst, 
			SyntaxKind.MoveLastKeyword => ChangeKind.MoveLast, 
			SyntaxKind.MoveBeforeKeyword => ChangeKind.MoveBefore, 
			SyntaxKind.MoveAfterKeyword => ChangeKind.MoveAfter, 
			SyntaxKind.ModifyKeyword => ChangeKind.Modify, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static bool IsPageActionOrGroupKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.PageGroupKeyword || kind - 133 <= SyntaxKind.DateLiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFieldModificationChange(this SyntaxKind kind)
	{
		return kind == SyntaxKind.FieldModification;
	}

	internal static bool IsMoveChange(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ActionMoveChange:
		case SyntaxKind.ControlMoveChange:
		case SyntaxKind.ViewMoveChange:
		case SyntaxKind.AnalysisViewMoveChange:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsModifyChange(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.FieldModification:
		case SyntaxKind.ActionModifyChange:
		case SyntaxKind.ControlModifyChange:
		case SyntaxKind.ViewModifyChange:
		case SyntaxKind.AnalysisViewModifyChange:
		case SyntaxKind.ReportExtensionModifyChange:
			return true;
		default:
			return false;
		}
	}

	internal static ControlKind ToControlKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PageAreaKeyword => ControlKind.Area, 
			SyntaxKind.PageGroupKeyword => ControlKind.Group, 
			SyntaxKind.PageRepeaterKeyword => ControlKind.Repeater, 
			SyntaxKind.PageCueGroupKeyword => ControlKind.CueGroup, 
			SyntaxKind.PageFixedKeyword => ControlKind.Fixed, 
			SyntaxKind.PageGridKeyword => ControlKind.Grid, 
			SyntaxKind.PagePartKeyword => ControlKind.Part, 
			SyntaxKind.PageSystemPartKeyword => ControlKind.SystemPart, 
			SyntaxKind.PageChartPartKeyword => ControlKind.ChartPart, 
			SyntaxKind.LabelKeyword => ControlKind.Label, 
			SyntaxKind.FieldKeyword => ControlKind.Field, 
			SyntaxKind.PageUserControlKeyword => ControlKind.UserControl, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static SyntaxKind ToSyntaxKeyword(this ControlKind controlKind)
	{
		return controlKind switch
		{
			ControlKind.Area => SyntaxKind.PageAreaKeyword, 
			ControlKind.ChartPart => SyntaxKind.PageChartPartKeyword, 
			ControlKind.CueGroup => SyntaxKind.PageCueGroupKeyword, 
			ControlKind.Field => SyntaxKind.FieldKeyword, 
			ControlKind.Fixed => SyntaxKind.PageFixedKeyword, 
			ControlKind.Grid => SyntaxKind.PageGridKeyword, 
			ControlKind.Group => SyntaxKind.PageGroupKeyword, 
			ControlKind.Label => SyntaxKind.LabelKeyword, 
			ControlKind.Part => SyntaxKind.PagePartKeyword, 
			ControlKind.Repeater => SyntaxKind.PageRepeaterKeyword, 
			ControlKind.SystemPart => SyntaxKind.PageSystemPartKeyword, 
			ControlKind.UserControl => SyntaxKind.PageUserControlKeyword, 
			_ => throw ExceptionUtilities.UnexpectedValue(controlKind), 
		};
	}

	internal static ActionKind ToActionKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PageAreaKeyword => ActionKind.Area, 
			SyntaxKind.PageGroupKeyword => ActionKind.Group, 
			SyntaxKind.ActionKeyword => ActionKind.Action, 
			SyntaxKind.ActionRefKeyword => ActionKind.ActionRef, 
			SyntaxKind.SeparatorKeyword => ActionKind.Separator, 
			SyntaxKind.CustomActionKeyword => ActionKind.CustomAction, 
			SyntaxKind.SystemActionKeyword => ActionKind.SystemAction, 
			SyntaxKind.FileUploadActionKeyword => ActionKind.FileUploadAction, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static AreaKind GetAreaKind(string value)
	{
		return value?.ToUpperInvariant() switch
		{
			"CONTENT" => AreaKind.Content, 
			"FACTBOXES" => AreaKind.FactBoxes, 
			"ROLECENTER" => AreaKind.RoleCenter, 
			"PROMPT" => AreaKind.Prompt, 
			"PROMPTOPTIONS" => AreaKind.PromptOptions, 
			"NAVIGATION" => AreaKind.Navigation, 
			_ => AreaKind.None, 
		};
	}

	public static ActionAreaKind GetActionAreaKind(string value)
	{
		return value?.ToUpperInvariant() switch
		{
			"PROCESSING" => ActionAreaKind.Processing, 
			"REPORTING" => ActionAreaKind.Reporting, 
			"NAVIGATION" => ActionAreaKind.Navigation, 
			"CREATION" => ActionAreaKind.Creation, 
			"EMBEDDING" => ActionAreaKind.Embedding, 
			"SECTIONS" => ActionAreaKind.Sections, 
			"PROMOTED" => ActionAreaKind.Promoted, 
			"SYSTEMACTIONS" => ActionAreaKind.SystemActions, 
			"PROMPTING" => ActionAreaKind.Prompting, 
			"PROMPTGUIDE" => ActionAreaKind.PromptGuide, 
			_ => ActionAreaKind.None, 
		};
	}

	internal static SystemOptionKinds.CommitBehaviorKind GetCommitBehaviorKind(string? commitBehaviorValueText)
	{
		string text = commitBehaviorValueText?.ToUpperInvariant();
		if (!(text == "IGNORE"))
		{
			if (text == "ERROR")
			{
				return SystemOptionKinds.CommitBehaviorKind.Error;
			}
			throw ExceptionUtilities.UnexpectedValue(commitBehaviorValueText);
		}
		return SystemOptionKinds.CommitBehaviorKind.Ignore;
	}

	internal static SystemOptionKinds.ErrorBehaviorKind GetErrorBehaviorKind(string errorBehaviorValueText)
	{
		if (errorBehaviorValueText?.ToUpperInvariant() == "COLLECT")
		{
			return SystemOptionKinds.ErrorBehaviorKind.Collect;
		}
		throw ExceptionUtilities.UnexpectedValue(errorBehaviorValueText);
	}

	internal static PageSystemPartKind GetSystemPartKind(string value)
	{
		if (Enum.TryParse<PageSystemPartKind>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		return PageSystemPartKind.None;
	}

	internal static bool IsIdentifierMultilanguageComment(string valueText)
	{
		return valueText.Equals("Comment", StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsPropertyContextSourceContext(this SyntaxKind kind)
	{
		if (kind - 484 <= SyntaxKind.Int32LiteralToken || kind == SyntaxKind.TextExpressionPropertyValue || kind == SyntaxKind.StyleExpressionPropertyValue)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPropertyClientExpresion(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.ClientSideBooleanExpressionPropertyValue || kind == SyntaxKind.StyleExpressionPropertyValue)
		{
			return true;
		}
		return false;
	}

	private static SyntaxKind GetObjectKeyword(string text)
	{
		return text switch
		{
			"CODEUNIT" => SyntaxKind.CodeunitKeyword, 
			"PAGE" => SyntaxKind.PageKeyword, 
			"QUERY" => SyntaxKind.QueryKeyword, 
			"XMLPORT" => SyntaxKind.XmlPortKeyword, 
			"REPORT" => SyntaxKind.ReportKeyword, 
			"REPORTEXTENSION" => SyntaxKind.ReportExtensionKeyword, 
			"TABLE" => SyntaxKind.TableKeyword, 
			"TABLEEXTENSION" => SyntaxKind.TableExtensionKeyword, 
			"PAGEEXTENSION" => SyntaxKind.PageExtensionKeyword, 
			"PROFILE" => SyntaxKind.ProfileKeyword, 
			"INTERFACE" => SyntaxKind.InterfaceKeyword, 
			"IMPLEMENTS" => SyntaxKind.ImplementsKeyword, 
			"PROFILEEXTENSION" => SyntaxKind.ProfileExtensionKeyword, 
			"PAGECUSTOMIZATION" => SyntaxKind.PageCustomizationKeyword, 
			"CONTROLADDIN" => SyntaxKind.ControlAddInKeyword, 
			"DOTNET" => SyntaxKind.DotNetKeyword, 
			"ENUM" => SyntaxKind.EnumKeyword, 
			"ENUMEXTENSION" => SyntaxKind.EnumExtensionKeyword, 
			"PERMISSIONSET" => SyntaxKind.PermissionSetKeyword, 
			"PERMISSIONSETEXTENSION" => SyntaxKind.PermissionSetExtensionKeyword, 
			"ENTITLEMENT" => SyntaxKind.EntitlementKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsDocumentationCommentTrivia(SyntaxKind kind)
	{
		if (kind != SyntaxKind.SingleLineDocumentationCommentTrivia)
		{
			return kind == SyntaxKind.MultiLineDocumentationCommentTrivia;
		}
		return true;
	}

	public static bool IsContextualKeyword(SyntaxKind kind)
	{
		return kind.IsKeyword();
	}

	public static bool IsMemberStartKeyword(this SyntaxKind kind)
	{
		if (kind.IsPageControlOrGroupKeyword() || kind.IsPageActionOrGroupKeyword() || kind.IsXmlPortNodeKeyword())
		{
			return true;
		}
		switch (kind)
		{
		case SyntaxKind.ProcedureKeyword:
		case SyntaxKind.VarKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.InternalKeyword:
		case SyntaxKind.ProtectedKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.TriggerKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.LayoutKeyword:
		case SyntaxKind.ActionsKeyword:
		case SyntaxKind.DataSetKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.LabelsKeyword:
		case SyntaxKind.RequestPageKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.ViewsKeyword:
		case SyntaxKind.AnalysisViewsKeyword:
		case SyntaxKind.RenderingKeyword:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportDatasetChangeKeyword(this SyntaxKind kind)
	{
		if (kind - 142 <= SyntaxKind.Int64LiteralToken || kind == SyntaxKind.ModifyKeyword || kind == SyntaxKind.AddKeyword)
		{
			return true;
		}
		return false;
	}

	public static bool IsPunctuation(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.CommaToken:
		case SyntaxKind.DotToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.DotDotToken:
		case SyntaxKind.OpenParenToken:
		case SyntaxKind.CloseParenToken:
		case SyntaxKind.OpenBracketToken:
		case SyntaxKind.CloseBracketToken:
		case SyntaxKind.OpenBraceToken:
		case SyntaxKind.CloseBraceToken:
			return true;
		default:
			return false;
		}
	}

	public static ImmutableArray<SystemActionKind> GetSupportedSystemActionKinds(PageTypeKind pageTypeKind)
	{
		return pageTypeKind switch
		{
			PageTypeKind.ConfigurationDialog => ConfigurationDialogSystemActions, 
			PageTypeKind.PromptDialog => AllSystemActions, 
			_ => ImmutableArray<SystemActionKind>.Empty, 
		};
	}

	public static string GetSupportedSystemActionKindsString(PageTypeKind pageTypeKind)
	{
		return pageTypeKind switch
		{
			PageTypeKind.ConfigurationDialog => ConfigurationDialogSystemActionsNamesString, 
			PageTypeKind.PromptDialog => AllSystemActionsString, 
			_ => string.Empty, 
		};
	}

	public static SyntaxKind GetUnaryExpression(SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.PlusToken => SyntaxKind.UnaryPlusExpression, 
			SyntaxKind.MinusToken => SyntaxKind.UnaryMinusExpression, 
			SyntaxKind.NotKeyword => SyntaxKind.UnaryNotExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsNestedObjectSyntaxKind(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.RequestPage)
		{
			return kind == SyntaxKind.RequestPageExtension;
		}
		return true;
	}

	public static SyntaxKind GetBinaryExpression(SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.EqualsToken => SyntaxKind.EqualsExpression, 
			SyntaxKind.NotEqualsToken => SyntaxKind.NotEqualsExpression, 
			SyntaxKind.LessThanToken => SyntaxKind.LessThanExpression, 
			SyntaxKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression, 
			SyntaxKind.GreaterThanToken => SyntaxKind.GreaterThanExpression, 
			SyntaxKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression, 
			SyntaxKind.PlusToken => SyntaxKind.AddExpression, 
			SyntaxKind.MinusToken => SyntaxKind.SubtractExpression, 
			SyntaxKind.MultiplyToken => SyntaxKind.MultiplyExpression, 
			SyntaxKind.IDivKeyword => SyntaxKind.IntegerDivideExpression, 
			SyntaxKind.RDivToken => SyntaxKind.DivideExpression, 
			SyntaxKind.ModuloKeyword => SyntaxKind.ModuloExpression, 
			SyntaxKind.AndKeyword => SyntaxKind.LogicalAndExpression, 
			SyntaxKind.OrKeyword => SyntaxKind.LogicalOrExpression, 
			SyntaxKind.XorKeyword => SyntaxKind.LogicalXorExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeExpression, 
			SyntaxKind.AsKeyword => SyntaxKind.AsExpression, 
			SyntaxKind.IsKeyword => SyntaxKind.IsExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool HasGroupActions(SyntaxKind kind)
	{
		return HasGroupActionList(kind);
	}

	public static bool HasPromptingActions(SyntaxKind kind)
	{
		return kind == SyntaxKind.PageRepeaterKeyword;
	}

	public static bool HasGroupActionList(SyntaxKind kind)
	{
		return kind == SyntaxKind.PageCueGroupKeyword;
	}

	internal static bool NameMustBeClsCompliant(this SyntaxKind kind)
	{
		if (kind - 385 <= SyntaxKind.Int32LiteralToken || kind - 398 <= SyntaxKind.Int32LiteralToken)
		{
			return true;
		}
		return false;
	}

	internal static TypeSymbol GetTypeFromSyntaxKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.BooleanLiteralValue:
			return NavCorLib.BooleanType;
		case SyntaxKind.Int32SignedLiteralValue:
			return NavCorLib.IntegerType;
		case SyntaxKind.Int64SignedLiteralValue:
			return NavCorLib.BigIntegerType;
		case SyntaxKind.StringLiteralValue:
			return NavCorLib.StringType;
		case SyntaxKind.DateLiteralValue:
			return NavCorLib.DateType;
		case SyntaxKind.DateTimeLiteralValue:
			return NavCorLib.DateTimeType;
		case SyntaxKind.TimeLiteralValue:
			return NavCorLib.TimeType;
		default:
			DebugAssertHelper.Assert(condition: false, "Unreachable");
			return NavCorLib.NoneType;
		}
	}

	internal static bool IsSyntaxAllowed(SyntaxNode parent, SyntaxKind childKind)
	{
		SyntaxKind kind = parent.Kind;
		switch (kind)
		{
		case SyntaxKind.ControlAddInObject:
			if (childKind != SyntaxKind.MethodDeclaration && childKind != SyntaxKind.EventDeclaration && childKind != SyntaxKind.EventKeyword)
			{
				return childKind == SyntaxKind.ProcedureKeyword;
			}
			return true;
		case SyntaxKind.ConstExpression:
		case SyntaxKind.FilterExpression:
			return childKind != SyntaxKind.NotKeyword;
		case SyntaxKind.EnumType:
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.ProfileObject:
		case SyntaxKind.ProfileExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			if (childKind != SyntaxKind.VarKeyword && childKind != SyntaxKind.TriggerKeyword && childKind != SyntaxKind.ProcedureKeyword && childKind != SyntaxKind.LocalKeyword && childKind != SyntaxKind.InternalKeyword && childKind != SyntaxKind.ProtectedKeyword && childKind != SyntaxKind.EventKeyword)
			{
				return childKind != SyntaxKind.EventDeclaration;
			}
			return false;
		case SyntaxKind.MethodDeclaration:
		{
			SyntaxNode parent2 = parent.Parent;
			if (parent2 != null && parent2.Kind == SyntaxKind.ControlAddInObject)
			{
				if (childKind != SyntaxKind.VarKeyword)
				{
					return childKind != SyntaxKind.BeginKeyword;
				}
				return false;
			}
			break;
		}
		case SyntaxKind.PageArea:
			if (GetAreaKind(parent.GetNameStringValue()) == AreaKind.Content && parent.GetContainingApplicationObjectSyntax().GetEnumPropertyValue(PropertyKind.PageType, PageTypeKind.Card) == PageTypeKind.ConfigurationDialog)
			{
				return childKind == SyntaxKind.PageGroupKeyword;
			}
			return childKind != SyntaxKind.PageAreaKeyword;
		case SyntaxKind.PageActionArea:
		case SyntaxKind.ActionAddChange:
			switch (GetActionAreaKind(parent.GetNameStringValue()))
			{
			case ActionAreaKind.Prompting:
			case ActionAreaKind.PromptGuide:
				if (childKind != SyntaxKind.PageGroupKeyword)
				{
					return childKind == SyntaxKind.ActionKeyword;
				}
				return true;
			case ActionAreaKind.Promoted:
				if (childKind != SyntaxKind.PageGroupKeyword)
				{
					return childKind == SyntaxKind.ActionRefKeyword;
				}
				return true;
			case ActionAreaKind.SystemActions:
				return childKind == SyntaxKind.SystemActionKeyword;
			default:
				if (childKind != SyntaxKind.SystemActionKeyword && childKind != SyntaxKind.ActionRefKeyword)
				{
					return childKind != SyntaxKind.PageAreaKeyword;
				}
				return false;
			}
		case SyntaxKind.PageActionGroup:
			if (childKind == SyntaxKind.SystemActionKeyword)
			{
				return false;
			}
			break;
		case SyntaxKind.ControlAddChange:
			if (IsPageCustomizationOrViewContext(parent))
			{
				if (childKind != SyntaxKind.FieldKeyword)
				{
					return childKind == SyntaxKind.PageGroupKeyword;
				}
				return true;
			}
			return childKind != SyntaxKind.PageAreaKeyword;
		case SyntaxKind.PageActionList:
		{
			SyntaxNode parent3 = parent.Parent;
			if (parent3 != null && parent3.Kind == SyntaxKind.PageGroup && HasGroupActionList(((PageGroupSyntax)parent.Parent).ControlKeyword.Kind))
			{
				return childKind == SyntaxKind.ActionKeyword;
			}
			return childKind == SyntaxKind.PageAreaKeyword;
		}
		case SyntaxKind.PageGroup:
			switch (childKind)
			{
			case SyntaxKind.PageAreaKeyword:
				return false;
			case SyntaxKind.ActionsKeyword:
				return HasGroupActions(((PageGroupSyntax)parent).ControlKeyword.Kind);
			}
			if (IsPageCustomizationOrViewContext(parent))
			{
				if (childKind != SyntaxKind.FieldKeyword)
				{
					return childKind == SyntaxKind.PageGroupKeyword;
				}
				return true;
			}
			break;
		case SyntaxKind.IdentifierEqualsLiteral:
			if (parent.GetAncestor<LabelSyntax>() != null)
			{
				string propertyName = (parent as IdentifierEqualsLiteralSyntax)?.Identifier.ValueText;
				if (childKind == SyntaxKind.TrueKeyword || childKind == SyntaxKind.FalseKeyword)
				{
					return LabelPropertyHelper.IsBooleanProperty(propertyName);
				}
			}
			break;
		}
		if (kind.IsObject())
		{
			if (childKind != SyntaxKind.EventDeclaration)
			{
				return childKind != SyntaxKind.EventKeyword;
			}
			return false;
		}
		if (childKind == SyntaxKind.TriggerKeyword)
		{
			if (kind.IsActionChangeBaseSyntax())
			{
				return false;
			}
			if (kind.IsControlBaseSyntax() || kind.IsActionBaseSyntax() || kind.IsControlChangeBaseSyntax())
			{
				return !IsPageCustomizationOrViewContext(parent);
			}
		}
		if (childKind == SyntaxKind.RequestPageKeyword)
		{
			if (kind != SyntaxKind.ReportObject)
			{
				return kind == SyntaxKind.XmlPortObject;
			}
			return true;
		}
		return true;
	}

	private static bool IsPageCustomizationOrViewContext(SyntaxNode parent)
	{
		if (parent.GetAncestor<PageCustomizationSyntax>() == null)
		{
			return parent.GetAncestor<PageViewSyntax>() != null;
		}
		return true;
	}

	internal static string GetPermissionValueObjectTypeName(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.TableDataKeyword => "TableData", 
			SyntaxKind.SystemKeyword => "System", 
			SyntaxKind.TableKeyword => "Table", 
			SyntaxKind.PageKeyword => "Page", 
			SyntaxKind.QueryKeyword => "Query", 
			SyntaxKind.ReportKeyword => "Report", 
			SyntaxKind.CodeunitKeyword => "Codeunit", 
			SyntaxKind.XmlPortKeyword => "XMLport", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static bool IsTrivia(SyntaxKind kind)
	{
		if (kind - 228 <= SyntaxKind.TimeLiteralToken || kind == SyntaxKind.DisabledTextTrivia)
		{
			return true;
		}
		return IsPreprocessorDirective(kind);
	}

	public static bool IsPreprocessorDirective(SyntaxKind kind)
	{
		if (kind - 554 <= SyntaxKind.DateTimeLiteralToken || kind - 563 <= SyntaxKind.EmptyToken)
		{
			return true;
		}
		return false;
	}

	public static bool IsPreprocessorKeyword(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.ElifKeyword:
		case SyntaxKind.EndIfKeyword:
		case SyntaxKind.RegionKeyword:
		case SyntaxKind.EndRegionKeyword:
		case SyntaxKind.DefineKeyword:
		case SyntaxKind.UndefKeyword:
		case SyntaxKind.PragmaKeyword:
		case SyntaxKind.WarningKeyword:
		case SyntaxKind.DisableKeyword:
		case SyntaxKind.RestoreKeyword:
		case SyntaxKind.EnableKeyword:
			return true;
		default:
			return false;
		}
	}

	static SyntaxFacts()
	{
		List<SyntaxKind> list = new List<SyntaxKind>();
		list.AddRange(from SyntaxKind kind in Enum.GetValues(typeof(SyntaxKind))
			where kind - 97 <= SyntaxKind.DateTimeLiteralToken
			select kind);
		SupportedPermissionKeywordKinds = ImmutableCollectionsMarshal.AsImmutableArray(list.ToArray());
		SupportedPermissionApplicationObjectsString = string.Join(", ", SupportedPermissionKeywordKinds.Select((SyntaxKind kind) => kind switch
		{
			SyntaxKind.RDivToken => "/", 
			SyntaxKind.PlusToken => "+", 
			SyntaxKind.MinusToken => "-", 
			SyntaxKind.MultiplyToken => "*", 
			SyntaxKind.IDivKeyword => "div", 
			SyntaxKind.ModuloKeyword => "mod", 
			SyntaxKind.AssignToken => ":=", 
			SyntaxKind.AssignRDivToken => "/=", 
			SyntaxKind.AssignPlusToken => "+=", 
			SyntaxKind.AssignMinusToken => "-=", 
			SyntaxKind.AssignMultiplyToken => "*=", 
			SyntaxKind.LessThanToken => "<", 
			SyntaxKind.LessThanEqualsToken => "<=", 
			SyntaxKind.NotEqualsToken => "<>", 
			SyntaxKind.EqualsToken => "=", 
			SyntaxKind.GreaterThanToken => ">", 
			SyntaxKind.GreaterThanEqualsToken => ">=", 
			SyntaxKind.CommaToken => ",", 
			SyntaxKind.DotToken => ".", 
			SyntaxKind.ColonToken => ":", 
			SyntaxKind.SemicolonToken => ";", 
			SyntaxKind.ColonColonToken => "::", 
			SyntaxKind.DotDotToken => "..", 
			SyntaxKind.AtToken => "@", 
			SyntaxKind.HashToken => "#", 
			SyntaxKind.OpenParenToken => "(", 
			SyntaxKind.CloseParenToken => ")", 
			SyntaxKind.OpenBracketToken => "[", 
			SyntaxKind.CloseBracketToken => "]", 
			SyntaxKind.OpenBraceToken => "{", 
			SyntaxKind.CloseBraceToken => "}", 
			SyntaxKind.DoubleQuoteToken => "\"", 
			SyntaxKind.SingleQuoteToken => "'", 
			SyntaxKind.QuestionToken => "?", 
			SyntaxKind.AmpersandToken => "&", 
			SyntaxKind.BarToken => "|", 
			SyntaxKind.CaretToken => "^", 
			SyntaxKind.PercentToken => "%", 
			SyntaxKind.TildeToken => "~", 
			SyntaxKind.MinusMinusToken => "--", 
			SyntaxKind.PlusPlusToken => "++", 
			SyntaxKind.EqualsEqualsToken => "==", 
			SyntaxKind.ExclamationEqualsToken => "!=", 
			SyntaxKind.ExclamationToken => "!", 
			SyntaxKind.SlashGreaterThanToken => "/>", 
			SyntaxKind.LessThanSlashToken => "</", 
			SyntaxKind.XmlCommentStartToken => "<!--", 
			SyntaxKind.XmlCommentEndToken => "-->", 
			SyntaxKind.XmlCDataStartToken => "<![CDATA[", 
			SyntaxKind.XmlCDataEndToken => "]]>", 
			SyntaxKind.XmlProcessingInstructionStartToken => "<?", 
			SyntaxKind.XmlProcessingInstructionEndToken => "?>", 
			SyntaxKind.TrueKeyword => "true", 
			SyntaxKind.FalseKeyword => "false", 
			SyntaxKind.AndKeyword => "and", 
			SyntaxKind.OrKeyword => "or", 
			SyntaxKind.XorKeyword => "xor", 
			SyntaxKind.NotKeyword => "not", 
			SyntaxKind.ExitKeyword => "exit", 
			SyntaxKind.BeginKeyword => "begin", 
			SyntaxKind.CaseKeyword => "case", 
			SyntaxKind.DoKeyword => "do", 
			SyntaxKind.DownToKeyword => "downto", 
			SyntaxKind.ElseKeyword => "else", 
			SyntaxKind.EndKeyword => "end", 
			SyntaxKind.ForKeyword => "for", 
			SyntaxKind.IfKeyword => "if", 
			SyntaxKind.InKeyword => "in", 
			SyntaxKind.OfKeyword => "of", 
			SyntaxKind.RepeatKeyword => "repeat", 
			SyntaxKind.ThenKeyword => "then", 
			SyntaxKind.ToKeyword => "to", 
			SyntaxKind.UntilKeyword => "until", 
			SyntaxKind.WithKeyword => "with", 
			SyntaxKind.WhileKeyword => "while", 
			SyntaxKind.ProgramKeyword => "program", 
			SyntaxKind.ProcedureKeyword => "procedure", 
			SyntaxKind.FunctionKeyword => "function", 
			SyntaxKind.VarKeyword => "var", 
			SyntaxKind.ArrayKeyword => "array", 
			SyntaxKind.TemporaryKeyword => "temporary", 
			SyntaxKind.LocalKeyword => "local", 
			SyntaxKind.InternalKeyword => "internal", 
			SyntaxKind.ProtectedKeyword => "protected", 
			SyntaxKind.BreakKeyword => "break", 
			SyntaxKind.ContinueKeyword => "continue", 
			SyntaxKind.EventKeyword => "event", 
			SyntaxKind.AssertErrorKeyword => "asserterror", 
			SyntaxKind.SuppressDisposeKeyword => "suppressdispose", 
			SyntaxKind.SecurityFilteringKeyword => "securityfiltering", 
			SyntaxKind.ForEachKeyword => "foreach", 
			SyntaxKind.TriggerKeyword => "trigger", 
			SyntaxKind.CodeunitKeyword => "codeunit", 
			SyntaxKind.TableKeyword => "table", 
			SyntaxKind.TableDataKeyword => "tabledata", 
			SyntaxKind.SystemKeyword => "system", 
			SyntaxKind.QueryKeyword => "query", 
			SyntaxKind.PageKeyword => "page", 
			SyntaxKind.PageExtensionKeyword => "pageextension", 
			SyntaxKind.TableExtensionKeyword => "tableextension", 
			SyntaxKind.ReportKeyword => "report", 
			SyntaxKind.ReportExtensionKeyword => "reportextension", 
			SyntaxKind.XmlPortKeyword => "xmlport", 
			SyntaxKind.ProfileKeyword => "profile", 
			SyntaxKind.ProfileExtensionKeyword => "profileextension", 
			SyntaxKind.InterfaceKeyword => "interface", 
			SyntaxKind.ImplementsKeyword => "implements", 
			SyntaxKind.ControlAddInKeyword => "controladdin", 
			SyntaxKind.PageCustomizationKeyword => "pagecustomization", 
			SyntaxKind.DotNetKeyword => "dotnet", 
			SyntaxKind.AssemblyKeyword => "assembly", 
			SyntaxKind.TypeKeyword => "type", 
			SyntaxKind.FieldsKeyword => "fields", 
			SyntaxKind.FieldKeyword => "field", 
			SyntaxKind.KeysKeyword => "keys", 
			SyntaxKind.KeyKeyword => "key", 
			SyntaxKind.FieldGroupsKeyword => "fieldgroups", 
			SyntaxKind.FieldGroupKeyword => "fieldgroup", 
			SyntaxKind.ThisKeyword => "this", 
			SyntaxKind.LayoutKeyword => "layout", 
			SyntaxKind.PageAreaKeyword => "area", 
			SyntaxKind.PageGroupKeyword => "group", 
			SyntaxKind.PageRepeaterKeyword => "repeater", 
			SyntaxKind.PageCueGroupKeyword => "cuegroup", 
			SyntaxKind.PageFixedKeyword => "fixed", 
			SyntaxKind.PageGridKeyword => "grid", 
			SyntaxKind.PagePartKeyword => "part", 
			SyntaxKind.PageSystemPartKeyword => "systempart", 
			SyntaxKind.PageChartPartKeyword => "chartpart", 
			SyntaxKind.PageUserControlKeyword => "usercontrol", 
			SyntaxKind.ActionsKeyword => "actions", 
			SyntaxKind.ActionKeyword => "action", 
			SyntaxKind.ActionRefKeyword => "actionref", 
			SyntaxKind.CustomActionKeyword => "customaction", 
			SyntaxKind.SeparatorKeyword => "separator", 
			SyntaxKind.SystemActionKeyword => "systemaction", 
			SyntaxKind.FileUploadActionKeyword => "fileuploadaction", 
			SyntaxKind.ViewsKeyword => "views", 
			SyntaxKind.ViewKeyword => "view", 
			SyntaxKind.AnalysisViewsKeyword => "analysisviews", 
			SyntaxKind.AnalysisViewKeyword => "analysisview", 
			SyntaxKind.ExtendsKeyword => "extends", 
			SyntaxKind.AddKeyword => "add", 
			SyntaxKind.AddFirstKeyword => "addfirst", 
			SyntaxKind.AddLastKeyword => "addlast", 
			SyntaxKind.AddBeforeKeyword => "addbefore", 
			SyntaxKind.AddAfterKeyword => "addafter", 
			SyntaxKind.MoveFirstKeyword => "movefirst", 
			SyntaxKind.MoveLastKeyword => "movelast", 
			SyntaxKind.MoveBeforeKeyword => "movebefore", 
			SyntaxKind.MoveAfterKeyword => "moveafter", 
			SyntaxKind.ModifyKeyword => "modify", 
			SyntaxKind.DataSetKeyword => "dataset", 
			SyntaxKind.DataItemKeyword => "dataitem", 
			SyntaxKind.ColumnKeyword => "column", 
			SyntaxKind.LabelsKeyword => "labels", 
			SyntaxKind.LabelKeyword => "label", 
			SyntaxKind.RequestPageKeyword => "requestpage", 
			SyntaxKind.RenderingKeyword => "rendering", 
			SyntaxKind.XmlPortSchemaKeyword => "schema", 
			SyntaxKind.XmlPortTableElementKeyword => "tableelement", 
			SyntaxKind.XmlPortFieldElementKeyword => "fieldelement", 
			SyntaxKind.XmlPortTextElementKeyword => "textelement", 
			SyntaxKind.XmlPortFieldAttributeKeyword => "fieldattribute", 
			SyntaxKind.XmlPortTextAttributeKeyword => "textattribute", 
			SyntaxKind.QueryElementsKeyword => "elements", 
			SyntaxKind.FilterKeyword => "filter", 
			SyntaxKind.WhereFormulaKeyword => "where", 
			SyntaxKind.FieldFormulaKeyword => "field", 
			SyntaxKind.ConstFormulaKeyword => "const", 
			SyntaxKind.FilterFormulaKeyword => "filter", 
			SyntaxKind.UpperLimitFormulaKeyword => "upperlimit", 
			SyntaxKind.AverageCalculationFormulaKeyword => "average", 
			SyntaxKind.CountCalculationFormulaKeyword => "count", 
			SyntaxKind.ExistCalculationFormulaKeyword => "exist", 
			SyntaxKind.LookupCalculationFormulaKeyword => "lookup", 
			SyntaxKind.MinCalculationFormulaKeyword => "min", 
			SyntaxKind.MaxCalculationFormulaKeyword => "max", 
			SyntaxKind.SumCalculationFormulaKeyword => "sum", 
			SyntaxKind.AndFilterKeyword => "&", 
			SyntaxKind.OrFilterKeyword => "|", 
			SyntaxKind.SortingKeyword => "sorting", 
			SyntaxKind.OrderKeyword => "order", 
			SyntaxKind.AscendingKeyword => "ascending", 
			SyntaxKind.DescendingKeyword => "descending", 
			SyntaxKind.CustomizesKeyword => "customizes", 
			SyntaxKind.EnumKeyword => "enum", 
			SyntaxKind.EnumValueKeyword => "value", 
			SyntaxKind.EnumExtensionKeyword => "enumextension", 
			SyntaxKind.ElifKeyword => "elif", 
			SyntaxKind.EndIfKeyword => "endif", 
			SyntaxKind.RegionKeyword => "region", 
			SyntaxKind.EndRegionKeyword => "endregion", 
			SyntaxKind.DefineKeyword => "define", 
			SyntaxKind.UndefKeyword => "undef", 
			SyntaxKind.PragmaKeyword => "pragma", 
			SyntaxKind.WarningKeyword => "warning", 
			SyntaxKind.DisableKeyword => "disable", 
			SyntaxKind.RestoreKeyword => "restore", 
			SyntaxKind.EnableKeyword => "enable", 
			SyntaxKind.ImplicitWithKeyword => "implicitwith", 
			SyntaxKind.ActionsV2Keyword => "actionsv2", 
			SyntaxKind.NamespaceKeyword => "namespace", 
			SyntaxKind.UsingKeyword => "using", 
			SyntaxKind.PermissionSetKeyword => "permissionset", 
			SyntaxKind.PermissionSetExtensionKeyword => "permissionsetextension", 
			SyntaxKind.EntitlementKeyword => "entitlement", 
			SyntaxKind.AsKeyword => "as", 
			SyntaxKind.IsKeyword => "is", 
			_ => string.Empty, 
		}));
		SupportedObjectSyntaxKeywords = new SyntaxKind[20]
		{
			SyntaxKind.TableKeyword,
			SyntaxKind.TableExtensionKeyword,
			SyntaxKind.PageKeyword,
			SyntaxKind.PageExtensionKeyword,
			SyntaxKind.PageCustomizationKeyword,
			SyntaxKind.ProfileKeyword,
			SyntaxKind.ProfileExtensionKeyword,
			SyntaxKind.CodeunitKeyword,
			SyntaxKind.ReportKeyword,
			SyntaxKind.ReportExtensionKeyword,
			SyntaxKind.XmlPortKeyword,
			SyntaxKind.QueryKeyword,
			SyntaxKind.ControlAddInKeyword,
			SyntaxKind.DotNetKeyword,
			SyntaxKind.EnumKeyword,
			SyntaxKind.EnumExtensionKeyword,
			SyntaxKind.InterfaceKeyword,
			SyntaxKind.PermissionSetKeyword,
			SyntaxKind.PermissionSetExtensionKeyword,
			SyntaxKind.EntitlementKeyword
		}.AsImmutable();
		SupportedApplicationObjectsString = string.Join(", ", SupportedObjectSyntaxKeywords.Select((SyntaxKind kind) => kind switch
		{
			SyntaxKind.RDivToken => "/", 
			SyntaxKind.PlusToken => "+", 
			SyntaxKind.MinusToken => "-", 
			SyntaxKind.MultiplyToken => "*", 
			SyntaxKind.IDivKeyword => "div", 
			SyntaxKind.ModuloKeyword => "mod", 
			SyntaxKind.AssignToken => ":=", 
			SyntaxKind.AssignRDivToken => "/=", 
			SyntaxKind.AssignPlusToken => "+=", 
			SyntaxKind.AssignMinusToken => "-=", 
			SyntaxKind.AssignMultiplyToken => "*=", 
			SyntaxKind.LessThanToken => "<", 
			SyntaxKind.LessThanEqualsToken => "<=", 
			SyntaxKind.NotEqualsToken => "<>", 
			SyntaxKind.EqualsToken => "=", 
			SyntaxKind.GreaterThanToken => ">", 
			SyntaxKind.GreaterThanEqualsToken => ">=", 
			SyntaxKind.CommaToken => ",", 
			SyntaxKind.DotToken => ".", 
			SyntaxKind.ColonToken => ":", 
			SyntaxKind.SemicolonToken => ";", 
			SyntaxKind.ColonColonToken => "::", 
			SyntaxKind.DotDotToken => "..", 
			SyntaxKind.AtToken => "@", 
			SyntaxKind.HashToken => "#", 
			SyntaxKind.OpenParenToken => "(", 
			SyntaxKind.CloseParenToken => ")", 
			SyntaxKind.OpenBracketToken => "[", 
			SyntaxKind.CloseBracketToken => "]", 
			SyntaxKind.OpenBraceToken => "{", 
			SyntaxKind.CloseBraceToken => "}", 
			SyntaxKind.DoubleQuoteToken => "\"", 
			SyntaxKind.SingleQuoteToken => "'", 
			SyntaxKind.QuestionToken => "?", 
			SyntaxKind.AmpersandToken => "&", 
			SyntaxKind.BarToken => "|", 
			SyntaxKind.CaretToken => "^", 
			SyntaxKind.PercentToken => "%", 
			SyntaxKind.TildeToken => "~", 
			SyntaxKind.MinusMinusToken => "--", 
			SyntaxKind.PlusPlusToken => "++", 
			SyntaxKind.EqualsEqualsToken => "==", 
			SyntaxKind.ExclamationEqualsToken => "!=", 
			SyntaxKind.ExclamationToken => "!", 
			SyntaxKind.SlashGreaterThanToken => "/>", 
			SyntaxKind.LessThanSlashToken => "</", 
			SyntaxKind.XmlCommentStartToken => "<!--", 
			SyntaxKind.XmlCommentEndToken => "-->", 
			SyntaxKind.XmlCDataStartToken => "<![CDATA[", 
			SyntaxKind.XmlCDataEndToken => "]]>", 
			SyntaxKind.XmlProcessingInstructionStartToken => "<?", 
			SyntaxKind.XmlProcessingInstructionEndToken => "?>", 
			SyntaxKind.TrueKeyword => "true", 
			SyntaxKind.FalseKeyword => "false", 
			SyntaxKind.AndKeyword => "and", 
			SyntaxKind.OrKeyword => "or", 
			SyntaxKind.XorKeyword => "xor", 
			SyntaxKind.NotKeyword => "not", 
			SyntaxKind.ExitKeyword => "exit", 
			SyntaxKind.BeginKeyword => "begin", 
			SyntaxKind.CaseKeyword => "case", 
			SyntaxKind.DoKeyword => "do", 
			SyntaxKind.DownToKeyword => "downto", 
			SyntaxKind.ElseKeyword => "else", 
			SyntaxKind.EndKeyword => "end", 
			SyntaxKind.ForKeyword => "for", 
			SyntaxKind.IfKeyword => "if", 
			SyntaxKind.InKeyword => "in", 
			SyntaxKind.OfKeyword => "of", 
			SyntaxKind.RepeatKeyword => "repeat", 
			SyntaxKind.ThenKeyword => "then", 
			SyntaxKind.ToKeyword => "to", 
			SyntaxKind.UntilKeyword => "until", 
			SyntaxKind.WithKeyword => "with", 
			SyntaxKind.WhileKeyword => "while", 
			SyntaxKind.ProgramKeyword => "program", 
			SyntaxKind.ProcedureKeyword => "procedure", 
			SyntaxKind.FunctionKeyword => "function", 
			SyntaxKind.VarKeyword => "var", 
			SyntaxKind.ArrayKeyword => "array", 
			SyntaxKind.TemporaryKeyword => "temporary", 
			SyntaxKind.LocalKeyword => "local", 
			SyntaxKind.InternalKeyword => "internal", 
			SyntaxKind.ProtectedKeyword => "protected", 
			SyntaxKind.BreakKeyword => "break", 
			SyntaxKind.ContinueKeyword => "continue", 
			SyntaxKind.EventKeyword => "event", 
			SyntaxKind.AssertErrorKeyword => "asserterror", 
			SyntaxKind.SuppressDisposeKeyword => "suppressdispose", 
			SyntaxKind.SecurityFilteringKeyword => "securityfiltering", 
			SyntaxKind.ForEachKeyword => "foreach", 
			SyntaxKind.TriggerKeyword => "trigger", 
			SyntaxKind.CodeunitKeyword => "codeunit", 
			SyntaxKind.TableKeyword => "table", 
			SyntaxKind.TableDataKeyword => "tabledata", 
			SyntaxKind.SystemKeyword => "system", 
			SyntaxKind.QueryKeyword => "query", 
			SyntaxKind.PageKeyword => "page", 
			SyntaxKind.PageExtensionKeyword => "pageextension", 
			SyntaxKind.TableExtensionKeyword => "tableextension", 
			SyntaxKind.ReportKeyword => "report", 
			SyntaxKind.ReportExtensionKeyword => "reportextension", 
			SyntaxKind.XmlPortKeyword => "xmlport", 
			SyntaxKind.ProfileKeyword => "profile", 
			SyntaxKind.ProfileExtensionKeyword => "profileextension", 
			SyntaxKind.InterfaceKeyword => "interface", 
			SyntaxKind.ImplementsKeyword => "implements", 
			SyntaxKind.ControlAddInKeyword => "controladdin", 
			SyntaxKind.PageCustomizationKeyword => "pagecustomization", 
			SyntaxKind.DotNetKeyword => "dotnet", 
			SyntaxKind.AssemblyKeyword => "assembly", 
			SyntaxKind.TypeKeyword => "type", 
			SyntaxKind.FieldsKeyword => "fields", 
			SyntaxKind.FieldKeyword => "field", 
			SyntaxKind.KeysKeyword => "keys", 
			SyntaxKind.KeyKeyword => "key", 
			SyntaxKind.FieldGroupsKeyword => "fieldgroups", 
			SyntaxKind.FieldGroupKeyword => "fieldgroup", 
			SyntaxKind.ThisKeyword => "this", 
			SyntaxKind.LayoutKeyword => "layout", 
			SyntaxKind.PageAreaKeyword => "area", 
			SyntaxKind.PageGroupKeyword => "group", 
			SyntaxKind.PageRepeaterKeyword => "repeater", 
			SyntaxKind.PageCueGroupKeyword => "cuegroup", 
			SyntaxKind.PageFixedKeyword => "fixed", 
			SyntaxKind.PageGridKeyword => "grid", 
			SyntaxKind.PagePartKeyword => "part", 
			SyntaxKind.PageSystemPartKeyword => "systempart", 
			SyntaxKind.PageChartPartKeyword => "chartpart", 
			SyntaxKind.PageUserControlKeyword => "usercontrol", 
			SyntaxKind.ActionsKeyword => "actions", 
			SyntaxKind.ActionKeyword => "action", 
			SyntaxKind.ActionRefKeyword => "actionref", 
			SyntaxKind.CustomActionKeyword => "customaction", 
			SyntaxKind.SeparatorKeyword => "separator", 
			SyntaxKind.SystemActionKeyword => "systemaction", 
			SyntaxKind.FileUploadActionKeyword => "fileuploadaction", 
			SyntaxKind.ViewsKeyword => "views", 
			SyntaxKind.ViewKeyword => "view", 
			SyntaxKind.AnalysisViewsKeyword => "analysisviews", 
			SyntaxKind.AnalysisViewKeyword => "analysisview", 
			SyntaxKind.ExtendsKeyword => "extends", 
			SyntaxKind.AddKeyword => "add", 
			SyntaxKind.AddFirstKeyword => "addfirst", 
			SyntaxKind.AddLastKeyword => "addlast", 
			SyntaxKind.AddBeforeKeyword => "addbefore", 
			SyntaxKind.AddAfterKeyword => "addafter", 
			SyntaxKind.MoveFirstKeyword => "movefirst", 
			SyntaxKind.MoveLastKeyword => "movelast", 
			SyntaxKind.MoveBeforeKeyword => "movebefore", 
			SyntaxKind.MoveAfterKeyword => "moveafter", 
			SyntaxKind.ModifyKeyword => "modify", 
			SyntaxKind.DataSetKeyword => "dataset", 
			SyntaxKind.DataItemKeyword => "dataitem", 
			SyntaxKind.ColumnKeyword => "column", 
			SyntaxKind.LabelsKeyword => "labels", 
			SyntaxKind.LabelKeyword => "label", 
			SyntaxKind.RequestPageKeyword => "requestpage", 
			SyntaxKind.RenderingKeyword => "rendering", 
			SyntaxKind.XmlPortSchemaKeyword => "schema", 
			SyntaxKind.XmlPortTableElementKeyword => "tableelement", 
			SyntaxKind.XmlPortFieldElementKeyword => "fieldelement", 
			SyntaxKind.XmlPortTextElementKeyword => "textelement", 
			SyntaxKind.XmlPortFieldAttributeKeyword => "fieldattribute", 
			SyntaxKind.XmlPortTextAttributeKeyword => "textattribute", 
			SyntaxKind.QueryElementsKeyword => "elements", 
			SyntaxKind.FilterKeyword => "filter", 
			SyntaxKind.WhereFormulaKeyword => "where", 
			SyntaxKind.FieldFormulaKeyword => "field", 
			SyntaxKind.ConstFormulaKeyword => "const", 
			SyntaxKind.FilterFormulaKeyword => "filter", 
			SyntaxKind.UpperLimitFormulaKeyword => "upperlimit", 
			SyntaxKind.AverageCalculationFormulaKeyword => "average", 
			SyntaxKind.CountCalculationFormulaKeyword => "count", 
			SyntaxKind.ExistCalculationFormulaKeyword => "exist", 
			SyntaxKind.LookupCalculationFormulaKeyword => "lookup", 
			SyntaxKind.MinCalculationFormulaKeyword => "min", 
			SyntaxKind.MaxCalculationFormulaKeyword => "max", 
			SyntaxKind.SumCalculationFormulaKeyword => "sum", 
			SyntaxKind.AndFilterKeyword => "&", 
			SyntaxKind.OrFilterKeyword => "|", 
			SyntaxKind.SortingKeyword => "sorting", 
			SyntaxKind.OrderKeyword => "order", 
			SyntaxKind.AscendingKeyword => "ascending", 
			SyntaxKind.DescendingKeyword => "descending", 
			SyntaxKind.CustomizesKeyword => "customizes", 
			SyntaxKind.EnumKeyword => "enum", 
			SyntaxKind.EnumValueKeyword => "value", 
			SyntaxKind.EnumExtensionKeyword => "enumextension", 
			SyntaxKind.ElifKeyword => "elif", 
			SyntaxKind.EndIfKeyword => "endif", 
			SyntaxKind.RegionKeyword => "region", 
			SyntaxKind.EndRegionKeyword => "endregion", 
			SyntaxKind.DefineKeyword => "define", 
			SyntaxKind.UndefKeyword => "undef", 
			SyntaxKind.PragmaKeyword => "pragma", 
			SyntaxKind.WarningKeyword => "warning", 
			SyntaxKind.DisableKeyword => "disable", 
			SyntaxKind.RestoreKeyword => "restore", 
			SyntaxKind.EnableKeyword => "enable", 
			SyntaxKind.ImplicitWithKeyword => "implicitwith", 
			SyntaxKind.ActionsV2Keyword => "actionsv2", 
			SyntaxKind.NamespaceKeyword => "namespace", 
			SyntaxKind.UsingKeyword => "using", 
			SyntaxKind.PermissionSetKeyword => "permissionset", 
			SyntaxKind.PermissionSetExtensionKeyword => "permissionsetextension", 
			SyntaxKind.EntitlementKeyword => "entitlement", 
			SyntaxKind.AsKeyword => "as", 
			SyntaxKind.IsKeyword => "is", 
			_ => string.Empty, 
		}));
		SupportedApplicationObjectReferenceKeywordKinds = new SyntaxKind[6]
		{
			SyntaxKind.CodeunitKeyword,
			SyntaxKind.PageKeyword,
			SyntaxKind.TableKeyword,
			SyntaxKind.ReportKeyword,
			SyntaxKind.XmlPortKeyword,
			SyntaxKind.QueryKeyword
		}.AsImmutable();
		SupportedApplicationObjectReferencesString = string.Join(", ", SupportedApplicationObjectReferenceKeywordKinds.Select((SyntaxKind kind) => kind switch
		{
			SyntaxKind.RDivToken => "/", 
			SyntaxKind.PlusToken => "+", 
			SyntaxKind.MinusToken => "-", 
			SyntaxKind.MultiplyToken => "*", 
			SyntaxKind.IDivKeyword => "div", 
			SyntaxKind.ModuloKeyword => "mod", 
			SyntaxKind.AssignToken => ":=", 
			SyntaxKind.AssignRDivToken => "/=", 
			SyntaxKind.AssignPlusToken => "+=", 
			SyntaxKind.AssignMinusToken => "-=", 
			SyntaxKind.AssignMultiplyToken => "*=", 
			SyntaxKind.LessThanToken => "<", 
			SyntaxKind.LessThanEqualsToken => "<=", 
			SyntaxKind.NotEqualsToken => "<>", 
			SyntaxKind.EqualsToken => "=", 
			SyntaxKind.GreaterThanToken => ">", 
			SyntaxKind.GreaterThanEqualsToken => ">=", 
			SyntaxKind.CommaToken => ",", 
			SyntaxKind.DotToken => ".", 
			SyntaxKind.ColonToken => ":", 
			SyntaxKind.SemicolonToken => ";", 
			SyntaxKind.ColonColonToken => "::", 
			SyntaxKind.DotDotToken => "..", 
			SyntaxKind.AtToken => "@", 
			SyntaxKind.HashToken => "#", 
			SyntaxKind.OpenParenToken => "(", 
			SyntaxKind.CloseParenToken => ")", 
			SyntaxKind.OpenBracketToken => "[", 
			SyntaxKind.CloseBracketToken => "]", 
			SyntaxKind.OpenBraceToken => "{", 
			SyntaxKind.CloseBraceToken => "}", 
			SyntaxKind.DoubleQuoteToken => "\"", 
			SyntaxKind.SingleQuoteToken => "'", 
			SyntaxKind.QuestionToken => "?", 
			SyntaxKind.AmpersandToken => "&", 
			SyntaxKind.BarToken => "|", 
			SyntaxKind.CaretToken => "^", 
			SyntaxKind.PercentToken => "%", 
			SyntaxKind.TildeToken => "~", 
			SyntaxKind.MinusMinusToken => "--", 
			SyntaxKind.PlusPlusToken => "++", 
			SyntaxKind.EqualsEqualsToken => "==", 
			SyntaxKind.ExclamationEqualsToken => "!=", 
			SyntaxKind.ExclamationToken => "!", 
			SyntaxKind.SlashGreaterThanToken => "/>", 
			SyntaxKind.LessThanSlashToken => "</", 
			SyntaxKind.XmlCommentStartToken => "<!--", 
			SyntaxKind.XmlCommentEndToken => "-->", 
			SyntaxKind.XmlCDataStartToken => "<![CDATA[", 
			SyntaxKind.XmlCDataEndToken => "]]>", 
			SyntaxKind.XmlProcessingInstructionStartToken => "<?", 
			SyntaxKind.XmlProcessingInstructionEndToken => "?>", 
			SyntaxKind.TrueKeyword => "true", 
			SyntaxKind.FalseKeyword => "false", 
			SyntaxKind.AndKeyword => "and", 
			SyntaxKind.OrKeyword => "or", 
			SyntaxKind.XorKeyword => "xor", 
			SyntaxKind.NotKeyword => "not", 
			SyntaxKind.ExitKeyword => "exit", 
			SyntaxKind.BeginKeyword => "begin", 
			SyntaxKind.CaseKeyword => "case", 
			SyntaxKind.DoKeyword => "do", 
			SyntaxKind.DownToKeyword => "downto", 
			SyntaxKind.ElseKeyword => "else", 
			SyntaxKind.EndKeyword => "end", 
			SyntaxKind.ForKeyword => "for", 
			SyntaxKind.IfKeyword => "if", 
			SyntaxKind.InKeyword => "in", 
			SyntaxKind.OfKeyword => "of", 
			SyntaxKind.RepeatKeyword => "repeat", 
			SyntaxKind.ThenKeyword => "then", 
			SyntaxKind.ToKeyword => "to", 
			SyntaxKind.UntilKeyword => "until", 
			SyntaxKind.WithKeyword => "with", 
			SyntaxKind.WhileKeyword => "while", 
			SyntaxKind.ProgramKeyword => "program", 
			SyntaxKind.ProcedureKeyword => "procedure", 
			SyntaxKind.FunctionKeyword => "function", 
			SyntaxKind.VarKeyword => "var", 
			SyntaxKind.ArrayKeyword => "array", 
			SyntaxKind.TemporaryKeyword => "temporary", 
			SyntaxKind.LocalKeyword => "local", 
			SyntaxKind.InternalKeyword => "internal", 
			SyntaxKind.ProtectedKeyword => "protected", 
			SyntaxKind.BreakKeyword => "break", 
			SyntaxKind.ContinueKeyword => "continue", 
			SyntaxKind.EventKeyword => "event", 
			SyntaxKind.AssertErrorKeyword => "asserterror", 
			SyntaxKind.SuppressDisposeKeyword => "suppressdispose", 
			SyntaxKind.SecurityFilteringKeyword => "securityfiltering", 
			SyntaxKind.ForEachKeyword => "foreach", 
			SyntaxKind.TriggerKeyword => "trigger", 
			SyntaxKind.CodeunitKeyword => "codeunit", 
			SyntaxKind.TableKeyword => "table", 
			SyntaxKind.TableDataKeyword => "tabledata", 
			SyntaxKind.SystemKeyword => "system", 
			SyntaxKind.QueryKeyword => "query", 
			SyntaxKind.PageKeyword => "page", 
			SyntaxKind.PageExtensionKeyword => "pageextension", 
			SyntaxKind.TableExtensionKeyword => "tableextension", 
			SyntaxKind.ReportKeyword => "report", 
			SyntaxKind.ReportExtensionKeyword => "reportextension", 
			SyntaxKind.XmlPortKeyword => "xmlport", 
			SyntaxKind.ProfileKeyword => "profile", 
			SyntaxKind.ProfileExtensionKeyword => "profileextension", 
			SyntaxKind.InterfaceKeyword => "interface", 
			SyntaxKind.ImplementsKeyword => "implements", 
			SyntaxKind.ControlAddInKeyword => "controladdin", 
			SyntaxKind.PageCustomizationKeyword => "pagecustomization", 
			SyntaxKind.DotNetKeyword => "dotnet", 
			SyntaxKind.AssemblyKeyword => "assembly", 
			SyntaxKind.TypeKeyword => "type", 
			SyntaxKind.FieldsKeyword => "fields", 
			SyntaxKind.FieldKeyword => "field", 
			SyntaxKind.KeysKeyword => "keys", 
			SyntaxKind.KeyKeyword => "key", 
			SyntaxKind.FieldGroupsKeyword => "fieldgroups", 
			SyntaxKind.FieldGroupKeyword => "fieldgroup", 
			SyntaxKind.ThisKeyword => "this", 
			SyntaxKind.LayoutKeyword => "layout", 
			SyntaxKind.PageAreaKeyword => "area", 
			SyntaxKind.PageGroupKeyword => "group", 
			SyntaxKind.PageRepeaterKeyword => "repeater", 
			SyntaxKind.PageCueGroupKeyword => "cuegroup", 
			SyntaxKind.PageFixedKeyword => "fixed", 
			SyntaxKind.PageGridKeyword => "grid", 
			SyntaxKind.PagePartKeyword => "part", 
			SyntaxKind.PageSystemPartKeyword => "systempart", 
			SyntaxKind.PageChartPartKeyword => "chartpart", 
			SyntaxKind.PageUserControlKeyword => "usercontrol", 
			SyntaxKind.ActionsKeyword => "actions", 
			SyntaxKind.ActionKeyword => "action", 
			SyntaxKind.ActionRefKeyword => "actionref", 
			SyntaxKind.CustomActionKeyword => "customaction", 
			SyntaxKind.SeparatorKeyword => "separator", 
			SyntaxKind.SystemActionKeyword => "systemaction", 
			SyntaxKind.FileUploadActionKeyword => "fileuploadaction", 
			SyntaxKind.ViewsKeyword => "views", 
			SyntaxKind.ViewKeyword => "view", 
			SyntaxKind.AnalysisViewsKeyword => "analysisviews", 
			SyntaxKind.AnalysisViewKeyword => "analysisview", 
			SyntaxKind.ExtendsKeyword => "extends", 
			SyntaxKind.AddKeyword => "add", 
			SyntaxKind.AddFirstKeyword => "addfirst", 
			SyntaxKind.AddLastKeyword => "addlast", 
			SyntaxKind.AddBeforeKeyword => "addbefore", 
			SyntaxKind.AddAfterKeyword => "addafter", 
			SyntaxKind.MoveFirstKeyword => "movefirst", 
			SyntaxKind.MoveLastKeyword => "movelast", 
			SyntaxKind.MoveBeforeKeyword => "movebefore", 
			SyntaxKind.MoveAfterKeyword => "moveafter", 
			SyntaxKind.ModifyKeyword => "modify", 
			SyntaxKind.DataSetKeyword => "dataset", 
			SyntaxKind.DataItemKeyword => "dataitem", 
			SyntaxKind.ColumnKeyword => "column", 
			SyntaxKind.LabelsKeyword => "labels", 
			SyntaxKind.LabelKeyword => "label", 
			SyntaxKind.RequestPageKeyword => "requestpage", 
			SyntaxKind.RenderingKeyword => "rendering", 
			SyntaxKind.XmlPortSchemaKeyword => "schema", 
			SyntaxKind.XmlPortTableElementKeyword => "tableelement", 
			SyntaxKind.XmlPortFieldElementKeyword => "fieldelement", 
			SyntaxKind.XmlPortTextElementKeyword => "textelement", 
			SyntaxKind.XmlPortFieldAttributeKeyword => "fieldattribute", 
			SyntaxKind.XmlPortTextAttributeKeyword => "textattribute", 
			SyntaxKind.QueryElementsKeyword => "elements", 
			SyntaxKind.FilterKeyword => "filter", 
			SyntaxKind.WhereFormulaKeyword => "where", 
			SyntaxKind.FieldFormulaKeyword => "field", 
			SyntaxKind.ConstFormulaKeyword => "const", 
			SyntaxKind.FilterFormulaKeyword => "filter", 
			SyntaxKind.UpperLimitFormulaKeyword => "upperlimit", 
			SyntaxKind.AverageCalculationFormulaKeyword => "average", 
			SyntaxKind.CountCalculationFormulaKeyword => "count", 
			SyntaxKind.ExistCalculationFormulaKeyword => "exist", 
			SyntaxKind.LookupCalculationFormulaKeyword => "lookup", 
			SyntaxKind.MinCalculationFormulaKeyword => "min", 
			SyntaxKind.MaxCalculationFormulaKeyword => "max", 
			SyntaxKind.SumCalculationFormulaKeyword => "sum", 
			SyntaxKind.AndFilterKeyword => "&", 
			SyntaxKind.OrFilterKeyword => "|", 
			SyntaxKind.SortingKeyword => "sorting", 
			SyntaxKind.OrderKeyword => "order", 
			SyntaxKind.AscendingKeyword => "ascending", 
			SyntaxKind.DescendingKeyword => "descending", 
			SyntaxKind.CustomizesKeyword => "customizes", 
			SyntaxKind.EnumKeyword => "enum", 
			SyntaxKind.EnumValueKeyword => "value", 
			SyntaxKind.EnumExtensionKeyword => "enumextension", 
			SyntaxKind.ElifKeyword => "elif", 
			SyntaxKind.EndIfKeyword => "endif", 
			SyntaxKind.RegionKeyword => "region", 
			SyntaxKind.EndRegionKeyword => "endregion", 
			SyntaxKind.DefineKeyword => "define", 
			SyntaxKind.UndefKeyword => "undef", 
			SyntaxKind.PragmaKeyword => "pragma", 
			SyntaxKind.WarningKeyword => "warning", 
			SyntaxKind.DisableKeyword => "disable", 
			SyntaxKind.RestoreKeyword => "restore", 
			SyntaxKind.EnableKeyword => "enable", 
			SyntaxKind.ImplicitWithKeyword => "implicitwith", 
			SyntaxKind.ActionsV2Keyword => "actionsv2", 
			SyntaxKind.NamespaceKeyword => "namespace", 
			SyntaxKind.UsingKeyword => "using", 
			SyntaxKind.PermissionSetKeyword => "permissionset", 
			SyntaxKind.PermissionSetExtensionKeyword => "permissionsetextension", 
			SyntaxKind.EntitlementKeyword => "entitlement", 
			SyntaxKind.AsKeyword => "as", 
			SyntaxKind.IsKeyword => "is", 
			_ => string.Empty, 
		}));
		ActionAreaValues = (from ActionAreaKind v in typeof(ActionAreaKind).GetEnumValues()
			where v != ActionAreaKind.None
			select v).ToImmutableArray();
		AllSystemActions = (from SystemActionKind x in typeof(SystemActionKind).GetEnumValues()
			orderby x.ToString()
			select x).ToImmutableArray();
		AllSystemActionsString = string.Join(", ", AllSystemActions.Select((SystemActionKind x) => x.ToString()));
		ConfigurationDialogSystemActions = new SystemActionKind[2]
		{
			SystemActionKind.Cancel,
			SystemActionKind.Ok
		}.ToImmutableArray();
		ConfigurationDialogSystemActionsNamesString = string.Join(", ", ConfigurationDialogSystemActions.Select((SystemActionKind x) => x.ToString()));
		List<SyntaxKind> list2 = new List<SyntaxKind>();
		list2.AddRange(from SyntaxKind kind in Enum.GetValues(typeof(SyntaxKind))
			where kind.IsObject() && !kind.IsNestedObjectSyntaxKind()
			select kind);
		RootObjectKinds = ImmutableCollectionsMarshal.AsImmutableArray(list2.ToArray());
		List<SyntaxKind> list3 = new List<SyntaxKind>();
		list3.AddRange(Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Where(delegate(SyntaxKind kind)
		{
			switch (kind)
			{
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.TrueKeyword:
			case SyntaxKind.IDivKeyword:
			case SyntaxKind.ModuloKeyword:
			case SyntaxKind.AndKeyword:
			case SyntaxKind.OrKeyword:
			case SyntaxKind.XorKeyword:
			case SyntaxKind.NotKeyword:
			case SyntaxKind.ExitKeyword:
			case SyntaxKind.BeginKeyword:
			case SyntaxKind.CaseKeyword:
			case SyntaxKind.DoKeyword:
			case SyntaxKind.DownToKeyword:
			case SyntaxKind.ElseKeyword:
			case SyntaxKind.EndKeyword:
			case SyntaxKind.ForKeyword:
			case SyntaxKind.IfKeyword:
			case SyntaxKind.InKeyword:
			case SyntaxKind.OfKeyword:
			case SyntaxKind.RepeatKeyword:
			case SyntaxKind.ThenKeyword:
			case SyntaxKind.ToKeyword:
			case SyntaxKind.UntilKeyword:
			case SyntaxKind.WithKeyword:
			case SyntaxKind.WhileKeyword:
			case SyntaxKind.ProgramKeyword:
			case SyntaxKind.ProcedureKeyword:
			case SyntaxKind.FunctionKeyword:
			case SyntaxKind.VarKeyword:
			case SyntaxKind.ArrayKeyword:
			case SyntaxKind.TemporaryKeyword:
			case SyntaxKind.LocalKeyword:
			case SyntaxKind.InternalKeyword:
			case SyntaxKind.ProtectedKeyword:
			case SyntaxKind.EventKeyword:
			case SyntaxKind.AssertErrorKeyword:
			case SyntaxKind.SuppressDisposeKeyword:
			case SyntaxKind.SecurityFilteringKeyword:
			case SyntaxKind.ForEachKeyword:
			case SyntaxKind.TriggerKeyword:
			case SyntaxKind.CodeunitKeyword:
			case SyntaxKind.TableKeyword:
			case SyntaxKind.TableDataKeyword:
			case SyntaxKind.SystemKeyword:
			case SyntaxKind.PageKeyword:
			case SyntaxKind.ReportKeyword:
			case SyntaxKind.QueryKeyword:
			case SyntaxKind.XmlPortKeyword:
			case SyntaxKind.ControlAddInKeyword:
			case SyntaxKind.ProfileKeyword:
			case SyntaxKind.ProfileExtensionKeyword:
			case SyntaxKind.DotNetKeyword:
			case SyntaxKind.PageCustomizationKeyword:
			case SyntaxKind.CustomizesKeyword:
			case SyntaxKind.FieldsKeyword:
			case SyntaxKind.FieldKeyword:
			case SyntaxKind.AssemblyKeyword:
			case SyntaxKind.TypeKeyword:
			case SyntaxKind.BreakKeyword:
			case SyntaxKind.FieldGroupsKeyword:
			case SyntaxKind.FieldGroupKeyword:
			case SyntaxKind.KeysKeyword:
			case SyntaxKind.KeyKeyword:
			case SyntaxKind.LayoutKeyword:
			case SyntaxKind.PageAreaKeyword:
			case SyntaxKind.PageGroupKeyword:
			case SyntaxKind.PageRepeaterKeyword:
			case SyntaxKind.PageCueGroupKeyword:
			case SyntaxKind.PageFixedKeyword:
			case SyntaxKind.PageGridKeyword:
			case SyntaxKind.PagePartKeyword:
			case SyntaxKind.PageSystemPartKeyword:
			case SyntaxKind.PageChartPartKeyword:
			case SyntaxKind.PageUserControlKeyword:
			case SyntaxKind.ActionsKeyword:
			case SyntaxKind.ActionKeyword:
			case SyntaxKind.ActionRefKeyword:
			case SyntaxKind.CustomActionKeyword:
			case SyntaxKind.SystemActionKeyword:
			case SyntaxKind.FileUploadActionKeyword:
			case SyntaxKind.SeparatorKeyword:
			case SyntaxKind.TableExtensionKeyword:
			case SyntaxKind.PageExtensionKeyword:
			case SyntaxKind.ExtendsKeyword:
			case SyntaxKind.AddFirstKeyword:
			case SyntaxKind.AddLastKeyword:
			case SyntaxKind.AddBeforeKeyword:
			case SyntaxKind.AddAfterKeyword:
			case SyntaxKind.MoveFirstKeyword:
			case SyntaxKind.MoveLastKeyword:
			case SyntaxKind.MoveBeforeKeyword:
			case SyntaxKind.MoveAfterKeyword:
			case SyntaxKind.ModifyKeyword:
			case SyntaxKind.DataSetKeyword:
			case SyntaxKind.DataItemKeyword:
			case SyntaxKind.ColumnKeyword:
			case SyntaxKind.LabelsKeyword:
			case SyntaxKind.LabelKeyword:
			case SyntaxKind.RequestPageKeyword:
			case SyntaxKind.XmlPortSchemaKeyword:
			case SyntaxKind.XmlPortTableElementKeyword:
			case SyntaxKind.XmlPortFieldElementKeyword:
			case SyntaxKind.XmlPortTextElementKeyword:
			case SyntaxKind.XmlPortFieldAttributeKeyword:
			case SyntaxKind.XmlPortTextAttributeKeyword:
			case SyntaxKind.FilterKeyword:
			case SyntaxKind.QueryElementsKeyword:
			case SyntaxKind.EnumKeyword:
			case SyntaxKind.EnumExtensionKeyword:
			case SyntaxKind.EnumValueKeyword:
			case SyntaxKind.ViewsKeyword:
			case SyntaxKind.ViewKeyword:
			case SyntaxKind.AnalysisViewsKeyword:
			case SyntaxKind.AnalysisViewKeyword:
			case SyntaxKind.ReportExtensionKeyword:
			case SyntaxKind.AddKeyword:
			case SyntaxKind.InterfaceKeyword:
			case SyntaxKind.ImplementsKeyword:
			case SyntaxKind.PermissionSetKeyword:
			case SyntaxKind.PermissionSetExtensionKeyword:
			case SyntaxKind.EntitlementKeyword:
			case SyntaxKind.RenderingKeyword:
			case SyntaxKind.AsKeyword:
			case SyntaxKind.IsKeyword:
			case SyntaxKind.ThisKeyword:
			case SyntaxKind.WhereFormulaKeyword:
			case SyntaxKind.FieldFormulaKeyword:
			case SyntaxKind.ConstFormulaKeyword:
			case SyntaxKind.FilterFormulaKeyword:
			case SyntaxKind.UpperLimitFormulaKeyword:
			case SyntaxKind.ExistCalculationFormulaKeyword:
			case SyntaxKind.CountCalculationFormulaKeyword:
			case SyntaxKind.SumCalculationFormulaKeyword:
			case SyntaxKind.AverageCalculationFormulaKeyword:
			case SyntaxKind.MinCalculationFormulaKeyword:
			case SyntaxKind.MaxCalculationFormulaKeyword:
			case SyntaxKind.LookupCalculationFormulaKeyword:
			case SyntaxKind.OrderKeyword:
			case SyntaxKind.SortingKeyword:
			case SyntaxKind.AscendingKeyword:
			case SyntaxKind.DescendingKeyword:
			case SyntaxKind.ElifKeyword:
			case SyntaxKind.EndIfKeyword:
			case SyntaxKind.RegionKeyword:
			case SyntaxKind.EndRegionKeyword:
			case SyntaxKind.DefineKeyword:
			case SyntaxKind.UndefKeyword:
			case SyntaxKind.PragmaKeyword:
			case SyntaxKind.WarningKeyword:
			case SyntaxKind.DisableKeyword:
			case SyntaxKind.RestoreKeyword:
			case SyntaxKind.EnableKeyword:
			case SyntaxKind.ImplicitWithKeyword:
			case SyntaxKind.ActionsV2Keyword:
			case SyntaxKind.NamespaceKeyword:
			case SyntaxKind.UsingKeyword:
				return true;
			default:
				return false;
			}
		}));
		KeywordsSyntaxKinds = ImmutableCollectionsMarshal.AsImmutableArray(list3.ToArray());
		List<SyntaxKind> list4 = new List<SyntaxKind>();
		list4.AddRange(Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Where(SyntaxKindExtensions.IsDataTypeSyntax));
		DataSyntaxKinds = ImmutableCollectionsMarshal.AsImmutableArray(list4.ToArray());
	}
}
