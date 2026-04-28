using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[CompilerGenerated]
internal sealed class ObjectParser : LanguageParser
{
	internal enum PermissionValidationStatus
	{
		Valid,
		ExpectedRIMD,
		ExpectedX,
		ExpectedRIMDX
	}

	private static readonly SyntaxAnnotation[] ExternalBusinessEventAnnotation;

	private static readonly SyntaxAnnotation[] MovedSymbolsAnnotation;

	private const string Moved = "Moved";

	private const string PendingMove = "PendingMove";

	internal static ImmutableDictionary<string, PropertyTypeInfo> CodeunitProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> FieldProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> KeyProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionAreaProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageAreaProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageFieldProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageLabelProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PagePartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageSystemPartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageChartPartProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryColumnProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryDataItemProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> QueryFilterProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportDataItemProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportColumnProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> RequestPageProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> TableProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTextElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortFieldElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTableElementProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortFieldAttributeProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> XmlPortTextAttributeProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> FieldGroupProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionSeparatorProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> EnumValueProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> DotNetAssemblyProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> DotNetTypeDeclarationProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageActionRefProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageCustomActionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageSystemActionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageFileUploadActionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageViewProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageAnalysisViewProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportExtensionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> RequestPageExtensionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ReportLayoutProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ProfileProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageCustomizationObjectProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> ControlAddInObjectProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PageUserControlProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> EnumTypeProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> InterfaceProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PermissionSetProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> PermissionSetExtensionProperties { get; }

	internal static ImmutableDictionary<string, PropertyTypeInfo> EntitlementProperties { get; }

	static ObjectParser()
	{
		CodeunitProperties = GetCodeunitProperties();
		QueryProperties = GetQueryProperties();
		FieldProperties = GetFieldProperties();
		KeyProperties = GetKeyProperties();
		PageProperties = GetPageProperties();
		PageActionProperties = GetPageActionProperties();
		PageActionAreaProperties = GetPageActionAreaProperties();
		PageActionGroupProperties = GetPageActionGroupProperties();
		PageAreaProperties = GetPageAreaProperties();
		PageFieldProperties = GetPageFieldProperties();
		PageGroupProperties = GetPageGroupProperties();
		PageLabelProperties = GetPageLabelProperties();
		PagePartProperties = GetPagePartProperties();
		PageSystemPartProperties = GetPageSystemPartProperties();
		PageChartPartProperties = GetPageChartPartProperties();
		QueryColumnProperties = GetQueryColumnProperties();
		QueryDataItemProperties = GetQueryDataItemProperties();
		QueryFilterProperties = GetQueryFilterProperties();
		ReportProperties = GetReportProperties();
		ReportDataItemProperties = GetReportDataItemProperties();
		ReportColumnProperties = GetReportColumnProperties();
		RequestPageProperties = GetRequestPageProperties();
		TableProperties = GetTableProperties();
		XmlPortProperties = GetXmlPortProperties();
		XmlPortTextElementProperties = GetXmlPortTextElementProperties();
		XmlPortFieldElementProperties = GetXmlPortFieldElementProperties();
		XmlPortTableElementProperties = GetXmlPortTableElementProperties();
		XmlPortFieldAttributeProperties = GetXmlPortFieldAttributeProperties();
		XmlPortTextAttributeProperties = GetXmlPortTextAttributeProperties();
		FieldGroupProperties = GetFieldGroupProperties();
		PageActionSeparatorProperties = GetPageActionSeparatorProperties();
		EnumValueProperties = GetEnumValueProperties();
		DotNetAssemblyProperties = GetDotNetAssemblyProperties();
		DotNetTypeDeclarationProperties = GetDotNetTypeDeclarationProperties();
		PageActionRefProperties = GetPageActionRefProperties();
		PageCustomActionProperties = GetPageCustomActionProperties();
		PageSystemActionProperties = GetPageSystemActionProperties();
		PageFileUploadActionProperties = GetPageFileUploadActionProperties();
		PageViewProperties = GetPageViewProperties();
		PageAnalysisViewProperties = GetPageAnalysisViewProperties();
		ReportExtensionProperties = GetReportExtensionProperties();
		RequestPageExtensionProperties = GetRequestPageExtensionProperties();
		ReportLayoutProperties = GetReportLayoutProperties();
		ProfileProperties = GetProfileProperties();
		PageCustomizationObjectProperties = GetPageCustomizationObjectProperties();
		ControlAddInObjectProperties = GetControlAddInObjectProperties();
		PageUserControlProperties = GetPageUserControlProperties();
		EnumTypeProperties = GetEnumTypeProperties();
		InterfaceProperties = GetInterfaceProperties();
		PermissionSetProperties = GetPermissionSetProperties();
		PermissionSetExtensionProperties = GetPermissionSetExtensionProperties();
		EntitlementProperties = GetEntitlementProperties();
		ExternalBusinessEventAnnotation = new SyntaxAnnotation[1]
		{
			new SyntaxAnnotation(AnnotationKind.ExternalBusinessEvent)
		};
		MovedSymbolsAnnotation = new SyntaxAnnotation[1]
		{
			new SyntaxAnnotation(AnnotationKind.MovedSymbols)
		};
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetCodeunitProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_Query", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			instance.Add("TABLENO", new PropertyTypeInfo(PropertyKind.TableNo, "TableNo", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Table));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("Normal"),
				new EnumPropertyMemberInfo("Test"),
				new EnumPropertyMemberInfo("TestRunner"),
				new EnumPropertyMemberInfo("Upgrade"),
				new EnumPropertyMemberInfo("Install")
			};
			instance.Add("SUBTYPE", new EnumPropertyTypeInfo(PropertyKind.Subtype, "Subtype", "Enum", "EnumLiteral", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Normal", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Codeunit"));
			instance.Add("SINGLEINSTANCE", new PropertyTypeInfo(PropertyKind.SingleInstance, "SingleInstance", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("Subtype(TestRunner)"));
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Disabled"),
				new EnumPropertyMemberInfo("Codeunit"),
				new EnumPropertyMemberInfo("Function")
			};
			instance.Add("TESTISOLATION", new EnumPropertyTypeInfo(PropertyKind.TestIsolation, "TestIsolation", "Enum", "EnumLiteral", parseFunc6, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Disabled", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("Subtype(Test)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Disabled"),
				new EnumPropertyMemberInfo("Codeunit"),
				new EnumPropertyMemberInfo("Function")
			};
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("REQUIREDTESTISOLATION", new EnumPropertyTypeInfo(PropertyKind.RequiredTestIsolation, "RequiredTestIsolation", "Enum", "EnumLiteral", parseFunc7, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "None", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("StaticAutomatic"),
				new EnumPropertyMemberInfo("Manual")
			};
			instance.Add("EVENTSUBSCRIBERINSTANCE", new EnumPropertyTypeInfo(PropertyKind.EventSubscriberInstance, "EventSubscriberInstance", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "StaticAutomatic", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("Subtype(Test)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("InheritFromTestCodeunit"),
				new EnumPropertyMemberInfo("Restrictive"),
				new EnumPropertyMemberInfo("NonRestrictive"),
				new EnumPropertyMemberInfo("Disabled")
			};
			instance.Add("TESTPERMISSIONS", new EnumPropertyTypeInfo(PropertyKind.TestPermissions, "TestPermissions", "Enum", "EnumLiteral", parseFunc9, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Restrictive", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("Subtype(Test)"));
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("BlockOutboundRequests"),
				new EnumPropertyMemberInfo("AllowOutboundFromHandler"),
				new EnumPropertyMemberInfo("AllowAllOutboundRequests")
			};
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("TESTHTTPREQUESTPOLICY", new EnumPropertyTypeInfo(PropertyKind.TestHttpRequestPolicy, "TestHttpRequestPolicy", "Enum", "EnumLiteral", parseFunc10, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("Subtype(Test)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("UnitTest"),
				new EnumPropertyMemberInfo("IntegrationTest"),
				new EnumPropertyMemberInfo("Uncategorized"),
				new EnumPropertyMemberInfo("AITest")
			};
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("TESTTYPE", new EnumPropertyTypeInfo(PropertyKind.TestType, "TestType", "Enum", "EnumLiteral", parseFunc11, dependentProperties5, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "UnitTest", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc13, dependentProperties6, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties7 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc14, dependentProperties7, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupCodeunitProperty(string name)
	{
		PropertyTypeInfo value = null;
		CodeunitProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_Query", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Query"));
			instance.Add("ORDERBY", new PropertyTypeInfo(PropertyKind.OrderBy, "OrderBy", "String", "OrderBy", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOrderByPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			instance.Add("TOPNUMBEROFROWS", new PropertyTypeInfo(PropertyKind.TopNumberOfRows, "TopNumberOfRows", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("ReadUncommitted"),
				new EnumPropertyMemberInfo("ReadShared"),
				new EnumPropertyMemberInfo("ReadExclusive")
			};
			instance.Add("READSTATE", new EnumPropertyTypeInfo(PropertyKind.ReadState, "ReadState", "Enum", "EnumLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ReadUncommitted", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Query"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Normal"),
				new EnumPropertyMemberInfo("API")
			};
			instance.Add("QUERYTYPE", new EnumPropertyTypeInfo(PropertyKind.QueryType, "QueryType", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Normal", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Query"));
			instance.Add("APIVERSION", new PropertyTypeInfo(PropertyKind.APIVersion, "APIVersion", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("QueryType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "beta", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", parseFunc9, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", parseFunc10, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			instance.Add("APIGROUP", new PropertyTypeInfo(PropertyKind.APIGroup, "APIGroup", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("QueryType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			instance.Add("APIPUBLISHER", new PropertyTypeInfo(PropertyKind.APIPublisher, "APIPublisher", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("QueryType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("QUERYCATEGORY", new PropertyTypeInfo(PropertyKind.QueryCategory, "QueryCategory", "String", "CommaSeparatedStrings", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("ReadOnly"),
				new EnumPropertyMemberInfo("ReadWrite")
			};
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("DATAACCESSINTENT", new EnumPropertyTypeInfo(PropertyKind.DataAccessIntent, "DataAccessIntent", "Enum", "EnumLiteral", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ReadWrite", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: true, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYCAPTION", new PropertyTypeInfo(PropertyKind.EntityCaption, "EntityCaption", "String", "Label", parseFunc13, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYSETCAPTION", new PropertyTypeInfo(PropertyKind.EntitySetCaption, "EntitySetCaption", "String", "Label", parseFunc14, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYCAPTIONML", new PropertyTypeInfo(PropertyKind.EntityCaptionML, "EntityCaptionML", "String", "Multilanguage", parseFunc15, dependentProperties5, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("QueryType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYSETCAPTIONML", new PropertyTypeInfo(PropertyKind.EntitySetCaptionML, "EntitySetCaptionML", "String", "Multilanguage", parseFunc16, dependentProperties6, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Lists"),
				new EnumPropertyMemberInfo("Tasks"),
				new EnumPropertyMemberInfo("ReportsAndAnalysis"),
				new EnumPropertyMemberInfo("Documents"),
				new EnumPropertyMemberInfo("History"),
				new EnumPropertyMemberInfo("Administration")
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("USAGECATEGORY", new EnumPropertyTypeInfo(PropertyKind.UsageCategory, "UsageCategory", "Enum", "EnumLiteral", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("CONTEXTSENSITIVEHELPPAGE", new PropertyTypeInfo(PropertyKind.ContextSensitiveHelpPage, "ContextSensitiveHelpPage", "String", "StringLiteral", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("HELPLINK", new PropertyTypeInfo(PropertyKind.HelpLink, "HelpLink", "String", "StringLiteral", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query", compatibility));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc25 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties7 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc25, dependentProperties7, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc26 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties8 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc26, dependentProperties8, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetFieldProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", parseFunc4, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", parseFunc5, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,DateTime,Decimal,Duration,Enum,Integer,Option,Time)"));
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[6]
			{
				new EnumPropertyMemberInfo("DontBlank"),
				new EnumPropertyMemberInfo("BlankNeg"),
				new EnumPropertyMemberInfo("BlankNegAndZero"),
				new EnumPropertyMemberInfo("BlankZero"),
				new EnumPropertyMemberInfo("BlankZeroAndPos"),
				new EnumPropertyMemberInfo("BlankPos")
			};
			instance.Add("BLANKNUMBERS", new EnumPropertyTypeInfo(PropertyKind.BlankNumbers, "BlankNumbers", "Enum", "EnumLiteral", parseFunc6, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Integer,Decimal,Duration,Enum,Option)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			instance.Add("BLANKZERO", new PropertyTypeInfo(PropertyKind.BlankZero, "BlankZero", "Boolean", "Boolean", parseFunc7, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("MINVALUE", new PropertyTypeInfo(PropertyKind.MinValue, "MinValue", "String", "Literal", parseFunc8, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("MAXVALUE", new PropertyTypeInfo(PropertyKind.MaxValue, "MaxValue", "String", "Literal", parseFunc9, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Code,Text,Decimal,Integer,BigInteger)"));
			ImmutableArray<DependentProperty>? incompatibleProperties = ImmutableArray.Create(new DependentProperty("ExtendedDataType(Masked)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Concealed")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("MASKTYPE", new EnumPropertyTypeInfo(PropertyKind.MaskType, "MaskType", "Enum", "EnumLiteral", parseFunc10, null, dependentParentProperties, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("NOTBLANK", new PropertyTypeInfo(PropertyKind.NotBlank, "NotBlank", "Boolean", "Boolean", parseFunc11, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Char,Code,Label,String,Text,TextConst)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = CharAllowedPropertyValueValidator.ValidatePropertyValue;
			instance.Add("CHARALLOWED", new PropertyTypeInfo(PropertyKind.CharAllowed, "CharAllowed", "String", "StringLiteral", parseFunc12, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Char,Code,Label,String,Text,TextConst)"));
			instance.Add("DATEFORMULA", new PropertyTypeInfo(PropertyKind.DateFormula, "DateFormula", "Boolean", "Boolean", parseFunc13, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Boolean,Enum,Option,Integer,BigInteger,Decimal,Code,Text)"));
			instance.Add("VALUESALLOWED", new PropertyTypeInfo(PropertyKind.ValuesAllowed, "ValuesAllowed", "String", "CommaSeparatedIdentifierOrLiteral", parseFunc14, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", parseFunc15, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", parseFunc16, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Date)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CLOSINGDATES", new PropertyTypeInfo(PropertyKind.ClosingDates, "ClosingDates", "Boolean", "Boolean", parseFunc17, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Decimal)"));
			validator = DecimalPlacesPropertyValueValidator.ValidatePropertyValue;
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", parseFunc18, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,BigText,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			options = new EnumPropertyMemberInfo[11]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("PhoneNo", "Phone No."),
				new EnumPropertyMemberInfo("URL"),
				new EnumPropertyMemberInfo("EMail", "E-Mail"),
				new EnumPropertyMemberInfo("Ratio"),
				new EnumPropertyMemberInfo("Masked"),
				new EnumPropertyMemberInfo("Person"),
				new EnumPropertyMemberInfo("Document", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.0")),
				new EnumPropertyMemberInfo("Barcode", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("12.0")),
				new EnumPropertyMemberInfo("RichContent", null, SymbolCompilationScope.Cloud, dependentProperties: ImmutableArray.Create(new DependentProperty("MultiLine(true)")), compatibility: VersionCompatibility.Parse("12.0")),
				new EnumPropertyMemberInfo("Task", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.1"))
			};
			instance.Add("EXTENDEDDATATYPE", new EnumPropertyTypeInfo(PropertyKind.ExtendedDatatype, "ExtendedDatatype", "Enum", "EnumLiteral", parseFunc19, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = WidthPropertyValueValidator.ValidatePropertyValue;
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("SIGNDISPLACEMENT", new PropertyTypeInfo(PropertyKind.SignDisplacement, "SignDisplacement", "Int32", "Int32Literal", parseFunc21, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", parseFunc22, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("FieldClass(Normal)"));
			options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("CustomerContent"),
				new EnumPropertyMemberInfo("EndUserIdentifiableInformation"),
				new EnumPropertyMemberInfo("AccountData"),
				new EnumPropertyMemberInfo("EndUserPseudonymousIdentifiers"),
				new EnumPropertyMemberInfo("OrganizationIdentifiableInformation"),
				new EnumPropertyMemberInfo("SystemMetadata"),
				new EnumPropertyMemberInfo("ToBeClassified")
			};
			instance.Add("DATACLASSIFICATION", new EnumPropertyTypeInfo(PropertyKind.DataClassification, "DataClassification", "Enum", "EnumLiteral", parseFunc23, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "CustomerContent", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field", null, null, null, emitDefaultValue: true, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Enum,Integer,Label,Option,String,Text,Time,TextConst)"));
			instance.Add("INITVALUE", new PropertyTypeInfo(PropertyKind.InitValue, "InitValue", "String", "Literal", parseFunc24, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc25 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Normal"),
				new EnumPropertyMemberInfo("FlowField"),
				new EnumPropertyMemberInfo("FlowFilter")
			};
			instance.Add("FIELDCLASS", new EnumPropertyTypeInfo(PropertyKind.FieldClass, "FieldClass", "Enum", "EnumLiteral", parseFunc25, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Normal", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field"));
			instance.Add("CALCFORMULA", new PropertyTypeInfo(PropertyKind.CalcFormula, "CalcFormula", "String", "CalculationFormula", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCalculationFormulaPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("FieldClass(FlowField)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc26 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableRelationPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("TABLERELATION", new PropertyTypeInfo(PropertyKind.TableRelation, "TableRelation", "String", "TableRelation", parseFunc26, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Field"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc27 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", parseFunc27, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc28 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Code)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Varchar"),
				new EnumPropertyMemberInfo("Integer"),
				new EnumPropertyMemberInfo("Variant"),
				new EnumPropertyMemberInfo("BigInteger")
			};
			instance.Add("SQLDATATYPE", new EnumPropertyTypeInfo(PropertyKind.SqlDataType, "SqlDataType", "Enum", "EnumLiteral", parseFunc28, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Varchar", isObsolete: false, generateMetadata: true, null, null, null, "SQL_Data_Type", null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field", null, null, null, emitDefaultValue: false));
			instance.Add("VALIDATETABLERELATION", new PropertyTypeInfo(PropertyKind.ValidateTableRelation, "ValidateTableRelation", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("TableRelation")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc29 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("TableRelation"));
			compatibility = VersionCompatibility.Parse("1.0");
			instance.Add("TESTTABLERELATION", new PropertyTypeInfo(PropertyKind.TestTableRelation, "TestTableRelation", "Boolean", "Boolean", parseFunc29, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", null, compatibility));
			ParseFunc parseFunc30 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal"),
				new EnumPropertyMemberInfo("Protected"),
				new EnumPropertyMemberInfo("Local")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc30, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc31 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("ToBeClassified", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.0")),
				new EnumPropertyMemberInfo("Always", "ToBeClassified", SymbolCompilationScope.Cloud, null, VersionCompatibility.Parse("16.0"), "The property value 'Always' is deprecated in favor of 'AsReadOnly' or 'AsReadWrite' which are more explicit about their intent."),
				new EnumPropertyMemberInfo("Never"),
				new EnumPropertyMemberInfo("AsReadOnly", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.0")),
				new EnumPropertyMemberInfo("AsReadWrite", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.0"))
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ALLOWINCUSTOMIZATIONS", new EnumPropertyTypeInfo(PropertyKind.AllowInCustomizations, "AllowInCustomizations", "Enum", "EnumLiteral", parseFunc31, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ToBeClassified", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field", compatibility, null, null, emitDefaultValue: true, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc32 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BLOB)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Json"),
				new EnumPropertyMemberInfo("UserDefined"),
				new EnumPropertyMemberInfo("Bitmap"),
				new EnumPropertyMemberInfo("Memo")
			};
			instance.Add("SUBTYPE", new EnumPropertyTypeInfo(PropertyKind.Subtype, "Subtype", "Enum", "EnumLiteral", parseFunc32, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "UserDefined", isObsolete: false, generateMetadata: true, null, null, null, "SubType", null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Field"));
			ParseFunc parseFunc33 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BLOB)"));
			instance.Add("COMPRESSED", new PropertyTypeInfo(PropertyKind.Compressed, "Compressed", "Boolean", "Boolean", parseFunc33, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc34 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("AUTOINCREMENT", new PropertyTypeInfo(PropertyKind.AutoIncrement, "AutoIncrement", "Boolean", "Boolean", parseFunc34, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			instance.Add("SQLTIMESTAMP", new PropertyTypeInfo(PropertyKind.SqlTimestamp, "SqlTimestamp", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("FieldClass(Normal)")), ImmutableArray.Create(new DependentProperty("Type(BigInteger)")), ImmutableArray.Create(new DependentProperty("TableType(Normal,ExternalSQL)")), null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, "SQL_Timestamp", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc35 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOptionValuesPropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			instance.Add("OPTIONMEMBERS", new PropertyTypeInfo(PropertyKind.OptionMembers, "OptionMembers", "String", "OptionValues", parseFunc35, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "OptionString", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc36 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph,CDS)"));
			instance.Add("EXTERNALNAME", new PropertyTypeInfo(PropertyKind.ExternalName, "ExternalName", "String", "StringLiteral", parseFunc36, null, null, incompatibleProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc37 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph,CDS)"));
			instance.Add("EXTERNALTYPE", new PropertyTypeInfo(PropertyKind.ExternalType, "ExternalType", "String", "StringLiteral", parseFunc37, null, null, incompatibleProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc38 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("ObsoleteState(PendingMove,Moved)"));
			validator = MovedToPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("MOVEDTO", new PropertyTypeInfo(PropertyKind.MovedTo, "MovedTo", "String", "Literal", parseFunc38, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc39 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			validator = MovedFromPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("MOVEDFROM", new PropertyTypeInfo(PropertyKind.MovedFrom, "MovedFrom", "String", "Literal", parseFunc39, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc40 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("TableType(CRM,CDS)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Full"),
				new EnumPropertyMemberInfo("Insert"),
				new EnumPropertyMemberInfo("Modify"),
				new EnumPropertyMemberInfo("Read")
			};
			instance.Add("EXTERNALACCESS", new EnumPropertyTypeInfo(PropertyKind.ExternalAccess, "ExternalAccess", "Enum", "EnumLiteral", parseFunc40, null, null, incompatibleProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Full", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc41 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			instance.Add("OPTIONORDINALVALUES", new PropertyTypeInfo(PropertyKind.OptionOrdinalValues, "OptionOrdinalValues", "String", "CommaSeparatedIdentifierOrLiteral", parseFunc41, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Integer", prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc42 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Code,Text)"));
			instance.Add("NUMERIC", new PropertyTypeInfo(PropertyKind.Numeric, "Numeric", "Boolean", "Boolean", parseFunc42, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field"));
			ParseFunc parseFunc43 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("FieldClass(Normal)"));
			ImmutableArray<DependentProperty>? dependentParentProperties2 = ImmutableArray.Create(new DependentProperty("Type(Code,Text)"));
			ImmutableArray<DependentProperty>? dependentDeclaringApplicationObjectProperties = ImmutableArray.Create(new DependentProperty("TableType(Normal)"));
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("OPTIMIZEFORTEXTSEARCH", new PropertyTypeInfo(PropertyKind.OptimizeForTextSearch, "OptimizeForTextSearch", "Boolean", "Boolean", parseFunc43, dependentProperties5, dependentParentProperties2, dependentDeclaringApplicationObjectProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc44 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending"),
				new EnumPropertyMemberInfo("Removed"),
				new EnumPropertyMemberInfo("PendingMove", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("13.0")),
				new EnumPropertyMemberInfo("Moved", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("13.0"))
			};
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc44, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Field", null, null, null, emitDefaultValue: false));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed,PendingMove,Moved)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: true, isRequired: false, null, "Field"));
			ParseFunc parseFunc45 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed,PendingMove,Moved)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc45, dependentProperties6, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc46 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc46, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			ParseFunc parseFunc47 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc47, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupFieldProperty(string name)
	{
		PropertyTypeInfo value = null;
		FieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetKeyProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key"));
			instance.Add("SUMINDEXFIELDS", new PropertyTypeInfo(PropertyKind.SumIndexFields, "SumIndexFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Key"));
			instance.Add("MAINTAINSQLINDEX", new PropertyTypeInfo(PropertyKind.MaintainSqlIndex, "MaintainSqlIndex", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, "MaintainSQLIndex", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key"));
			instance.Add("MAINTAINSIFTINDEX", new PropertyTypeInfo(PropertyKind.MaintainSiftIndex, "MaintainSiftIndex", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, "MaintainSIFTIndex", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key"));
			instance.Add("CLUSTERED", new PropertyTypeInfo(PropertyKind.Clustered, "Clustered", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("MaintainSqlIndex(true)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key"));
			instance.Add("SQLINDEX", new PropertyTypeInfo(PropertyKind.SqlIndex, "SqlIndex", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "SQLIndex", null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Key"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("UNIQUE", new PropertyTypeInfo(PropertyKind.Unique, "Unique", "Boolean", "Boolean", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("8.0");
			instance.Add("INCLUDEDFIELDS", new PropertyTypeInfo(PropertyKind.IncludedFields, "IncludedFields", "String", "CommaSeparated", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Key", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending"),
				new EnumPropertyMemberInfo("Removed")
			};
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Key", null, null, null, emitDefaultValue: false));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: true, isRequired: false, null, "Key"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc5, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Key", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupKeyProperty(string name)
	{
		PropertyTypeInfo value = null;
		KeyProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("HELPLINK", new PropertyTypeInfo(PropertyKind.HelpLink, "HelpLink", "String", "StringLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("CONTEXTSENSITIVEHELPPAGE", new PropertyTypeInfo(PropertyKind.ContextSensitiveHelpPage, "ContextSensitiveHelpPage", "String", "StringLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("AUTOSPLITKEY", new PropertyTypeInfo(PropertyKind.AutoSplitKey, "AutoSplitKey", "Boolean", "Boolean", parseFunc10, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("15.0"));
			instance.Add("CARDPAGEID", new PropertyTypeInfo(PropertyKind.CardPageId, "CardPageId", "String", "ObjectReference", parseFunc11, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "CardFormID", null, null, emitAsAttribute: true, "Page", prependObjectName: false, isRequired: false, null, "Page", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DATACAPTIONEXPRESSION", new PropertyTypeInfo(PropertyKind.DataCaptionExpression, "DataCaptionExpression", "String", "TextExpression", parseFunc12, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "DataCaptionExpr", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("SourceTable"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", parseFunc13, dependentProperties, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("INSERTALLOWED", new PropertyTypeInfo(PropertyKind.InsertAllowed, "InsertAllowed", "Boolean", "Boolean", parseFunc14, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("MODIFYALLOWED", new PropertyTypeInfo(PropertyKind.ModifyAllowed, "ModifyAllowed", "Boolean", "Boolean", parseFunc15, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("DELETEALLOWED", new PropertyTypeInfo(PropertyKind.DeleteAllowed, "DeleteAllowed", "Boolean", "Boolean", parseFunc16, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("SOURCETABLE", new PropertyTypeInfo(PropertyKind.SourceTable, "SourceTable", "String", "ObjectReference", parseFunc17, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Table));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("SourceTable"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("SOURCETABLETEMPORARY", new PropertyTypeInfo(PropertyKind.SourceTableTemporary, "SourceTableTemporary", "Boolean", "Boolean", parseFunc18, dependentProperties2, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("SourceTable"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", parseFunc19, dependentProperties3, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", parseFunc21, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("SAVEVALUES", new PropertyTypeInfo(PropertyKind.SaveValues, "SaveValues", "Boolean", "Boolean", parseFunc22, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("LINKSALLOWED", new PropertyTypeInfo(PropertyKind.LinksAllowed, "LinksAllowed", "Boolean", "Boolean", parseFunc23, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("MULTIPLENEWLINES", new PropertyTypeInfo(PropertyKind.MultipleNewLines, "MultipleNewLines", "Boolean", "Boolean", parseFunc24, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc25 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("POPULATEALLFIELDS", new PropertyTypeInfo(PropertyKind.PopulateAllFields, "PopulateAllFields", "Boolean", "Boolean", parseFunc25, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc26 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("EXTENSIBLE", new PropertyTypeInfo(PropertyKind.Extensible, "Extensible", "Boolean", "Boolean", parseFunc26, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc27 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[19]
			{
				new EnumPropertyMemberInfo("Card"),
				new EnumPropertyMemberInfo("List"),
				new EnumPropertyMemberInfo("RoleCenter"),
				new EnumPropertyMemberInfo("CardPart"),
				new EnumPropertyMemberInfo("ListPart"),
				new EnumPropertyMemberInfo("Document"),
				new EnumPropertyMemberInfo("Worksheet"),
				new EnumPropertyMemberInfo("ListPlus"),
				new EnumPropertyMemberInfo("ConfirmationDialog"),
				new EnumPropertyMemberInfo("NavigatePage"),
				new EnumPropertyMemberInfo("StandardDialog"),
				new EnumPropertyMemberInfo("API"),
				new EnumPropertyMemberInfo("ReportPreview"),
				new EnumPropertyMemberInfo("ReportProcessingOnly"),
				new EnumPropertyMemberInfo("XmlPort"),
				new EnumPropertyMemberInfo("HeadlinePart"),
				new EnumPropertyMemberInfo("PromptDialog", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("12.1")),
				new EnumPropertyMemberInfo("ConfigurationDialog", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("14.0")),
				new EnumPropertyMemberInfo("UserControlHost", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("15.0"))
			};
			instance.Add("PAGETYPE", new EnumPropertyTypeInfo(PropertyKind.PageType, "PageType", "Enum", "EnumLiteral", parseFunc27, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Card", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc28 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("PageType(PromptDialog,ConfigurationDialog)"));
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("ISPREVIEW", new PropertyTypeInfo(PropertyKind.IsPreview, "IsPreview", "Boolean", "Boolean", parseFunc28, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc29 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("PageType(PromptDialog)"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Content"),
				new EnumPropertyMemberInfo("Generate"),
				new EnumPropertyMemberInfo("Prompt")
			};
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("PROMPTMODE", new EnumPropertyTypeInfo(PropertyKind.PromptMode, "PromptMode", "Enum", "EnumLiteral", parseFunc29, dependentProperties5, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Prompt", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Page", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc30 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("DELAYEDINSERT", new PropertyTypeInfo(PropertyKind.DelayedInsert, "DelayedInsert", "Boolean", "Boolean", parseFunc30, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc31 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("REFRESHONACTIVATE", new PropertyTypeInfo(PropertyKind.RefreshOnActivate, "RefreshOnActivate", "Boolean", "Boolean", parseFunc31, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc32 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			ImmutableArray<DependentProperty>? incompatibleProperties2 = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			validator = PromotedActionCategoriesMLPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTEDACTIONCATEGORIESML", new PropertyTypeInfo(PropertyKind.PromotedActionCategoriesML, "PromotedActionCategoriesML", "String", "Multilanguage", parseFunc32, null, null, incompatibleProperties, incompatibleProperties2, dependentDeclaringApplicationObjectPropertiesWarning: true, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc33 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			incompatibleProperties2 = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			validator = PromotedActionCategoriesPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTEDACTIONCATEGORIES", new PropertyTypeInfo(PropertyKind.PromotedActionCategories, "PromotedActionCategories", "String", "Label", parseFunc33, null, null, incompatibleProperties2, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: true, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc34 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("SourceTable"));
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			instance.Add("ODATAKEYFIELDS", new PropertyTypeInfo(PropertyKind.ODataKeyFields, "ODataKeyFields", "String", "CommaSeparated", parseFunc34, dependentProperties6, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc35 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties7 = ImmutableArray.Create(new DependentProperty("PageType(List,Worksheet)"));
			validator = AnalysisModeEnabledPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("ANALYSISMODEENABLED", new PropertyTypeInfo(PropertyKind.AnalysisModeEnabled, "AnalysisModeEnabled", "Boolean", "Boolean", parseFunc35, dependentProperties7, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			instance.Add("APIVERSION", new PropertyTypeInfo(PropertyKind.APIVersion, "APIVersion", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("PageType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "beta", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc36 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties8 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			validator = EntityNamePropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", parseFunc36, dependentProperties8, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc37 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties9 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			validator = EntitySetNamePropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", parseFunc37, dependentProperties9, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			instance.Add("APIGROUP", new PropertyTypeInfo(PropertyKind.APIGroup, "APIGroup", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("PageType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			instance.Add("APIPUBLISHER", new PropertyTypeInfo(PropertyKind.APIPublisher, "APIPublisher", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("PageType(API)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page"));
			ParseFunc parseFunc38 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties10 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			compatibility = VersionCompatibility.Parse("2.0");
			instance.Add("CHANGETRACKINGALLOWED", new PropertyTypeInfo(PropertyKind.ChangeTrackingAllowed, "ChangeTrackingAllowed", "Boolean", "Boolean", parseFunc38, dependentProperties10, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc39 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("PageType(UserControlHost)"));
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("QUERYCATEGORY", new PropertyTypeInfo(PropertyKind.QueryCategory, "QueryCategory", "String", "StringLiteral", parseFunc39, null, null, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc40 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("ADDITIONALSEARCHTERMSML", new PropertyTypeInfo(PropertyKind.AdditionalSearchTermsML, "AdditionalSearchTermsML", "String", "Multilanguage", parseFunc40, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc41 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("ADDITIONALSEARCHTERMS", new PropertyTypeInfo(PropertyKind.AdditionalSearchTerms, "AdditionalSearchTerms", "String", "Label", parseFunc41, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc42 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties11 = ImmutableArray.Create(new DependentProperty("PageType(API)"), new DependentProperty("Editable(false)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("ReadOnly"),
				new EnumPropertyMemberInfo("ReadWrite")
			};
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("DATAACCESSINTENT", new EnumPropertyTypeInfo(PropertyKind.DataAccessIntent, "DataAccessIntent", "Enum", "EnumLiteral", parseFunc42, dependentProperties11, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ReadWrite", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc43 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties12 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYCAPTION", new PropertyTypeInfo(PropertyKind.EntityCaption, "EntityCaption", "String", "Label", parseFunc43, dependentProperties12, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc44 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties13 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYSETCAPTION", new PropertyTypeInfo(PropertyKind.EntitySetCaption, "EntitySetCaption", "String", "Label", parseFunc44, dependentProperties13, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc45 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties14 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYCAPTIONML", new PropertyTypeInfo(PropertyKind.EntityCaptionML, "EntityCaptionML", "String", "Multilanguage", parseFunc45, dependentProperties14, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc46 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties15 = ImmutableArray.Create(new DependentProperty("PageType(API)"));
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ENTITYSETCAPTIONML", new PropertyTypeInfo(PropertyKind.EntitySetCaptionML, "EntitySetCaptionML", "String", "Multilanguage", parseFunc46, dependentProperties15, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc47 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc47, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc48 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties16 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc48, dependentProperties16, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc49 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties17 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("9.1"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc49, dependentProperties17, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page", compatibility));
			ParseFunc parseFunc50 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Lists"),
				new EnumPropertyMemberInfo("Tasks"),
				new EnumPropertyMemberInfo("ReportsAndAnalysis"),
				new EnumPropertyMemberInfo("Documents"),
				new EnumPropertyMemberInfo("History"),
				new EnumPropertyMemberInfo("Administration")
			};
			instance.Add("USAGECATEGORY", new EnumPropertyTypeInfo(PropertyKind.UsageCategory, "UsageCategory", "Enum", "EnumLiteral", parseFunc50, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Page_Report"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)", VersionCompatibility.Parse("1.0-10.0-true"))), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "Page_Report"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_Report"));
			ParseFunc parseFunc51 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc51, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc52 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc52, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc53 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc53, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc54 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc54, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("View"),
				new EnumPropertyMemberInfo("Edit"),
				new EnumPropertyMemberInfo("Create")
			};
			instance.Add("RUNPAGEMODE", new EnumPropertyTypeInfo(PropertyKind.RunPageMode, "RunPageMode", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Edit", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageAction", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentDeclaringApplicationObjectProperties = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = PromotedPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTED", new PropertyTypeInfo(PropertyKind.Promoted, "Promoted", "Boolean", "Boolean", parseFunc10, null, null, dependentDeclaringApplicationObjectProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: true, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("Promoted(true)"));
			dependentDeclaringApplicationObjectProperties = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			validator = PromotedIsBigPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTEDISBIG", new PropertyTypeInfo(PropertyKind.PromotedIsBig, "PromotedIsBig", "Boolean", "Boolean", parseFunc11, dependentProperties, null, dependentDeclaringApplicationObjectProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: true, "false", generateMetadata: false, emitDefaultValue: true, customizationModifiability, extensionModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("Promoted(true)"));
			dependentDeclaringApplicationObjectProperties = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			validator = PromotedOnlyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTEDONLY", new PropertyTypeInfo(PropertyKind.PromotedOnly, "PromotedOnly", "Boolean", "Boolean", parseFunc12, dependentProperties2, null, dependentDeclaringApplicationObjectProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: true, "false", generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("Promoted(true)"));
			dependentDeclaringApplicationObjectProperties = ImmutableArray.Create(new DependentProperty("PageType(Card,Document,List,ListPlus,Worksheet)"));
			options = new EnumPropertyMemberInfo[20]
			{
				new EnumPropertyMemberInfo("New"),
				new EnumPropertyMemberInfo("Process"),
				new EnumPropertyMemberInfo("Report"),
				new EnumPropertyMemberInfo("Category4"),
				new EnumPropertyMemberInfo("Category5"),
				new EnumPropertyMemberInfo("Category6"),
				new EnumPropertyMemberInfo("Category7"),
				new EnumPropertyMemberInfo("Category8"),
				new EnumPropertyMemberInfo("Category9"),
				new EnumPropertyMemberInfo("Category10"),
				new EnumPropertyMemberInfo("Category11"),
				new EnumPropertyMemberInfo("Category12"),
				new EnumPropertyMemberInfo("Category13"),
				new EnumPropertyMemberInfo("Category14"),
				new EnumPropertyMemberInfo("Category15"),
				new EnumPropertyMemberInfo("Category16"),
				new EnumPropertyMemberInfo("Category17"),
				new EnumPropertyMemberInfo("Category18"),
				new EnumPropertyMemberInfo("Category19"),
				new EnumPropertyMemberInfo("Category20")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			validator = PromotedCategoryPropertyValueValidator.ValidatePropertyValue;
			instance.Add("PROMOTEDCATEGORY", new EnumPropertyTypeInfo(PropertyKind.PromotedCategory, "PromotedCategory", "Enum", "EnumLiteral", parseFunc13, dependentProperties3, null, dependentDeclaringApplicationObjectProperties, null, dependentDeclaringApplicationObjectPropertiesWarning: true, options, "New", isObsolete: false, generateMetadata: false, customizationModifiability, extensionModifiability, validator, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Page"),
				new EnumPropertyMemberInfo("Repeater")
			};
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "PageAction"));
			instance.Add("ELLIPSIS", new PropertyTypeInfo(PropertyKind.Ellipsis, "Ellipsis", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("SHORTCUTKEY", new PropertyTypeInfo(PropertyKind.ShortcutKey, "ShortcutKey", "String", "Literal", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, "ShortCutKey", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("RUNOBJECT", new PropertyTypeInfo(PropertyKind.RunObject, "RunObject", "String", "QualifiedObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseQualifiedObjectReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("RUNPAGEVIEW", new PropertyTypeInfo(PropertyKind.RunPageView, "RunPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("RunObject")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("RUNPAGELINK", new PropertyTypeInfo(PropertyKind.RunPageLink, "RunPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("RunObject")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			instance.Add("RUNPAGEONREC", new PropertyTypeInfo(PropertyKind.RunPageOnRec, "RunPageOnRec", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("INFOOTERBAR", new PropertyTypeInfo(PropertyKind.InFooterBar, "InFooterBar", "Boolean", "Boolean", parseFunc16, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("LeftSwipe"),
				new EnumPropertyMemberInfo("RightSwipe"),
				new EnumPropertyMemberInfo("ContextMenu")
			};
			instance.Add("GESTURE", new EnumPropertyTypeInfo(PropertyKind.Gesture, "Gesture", "Enum", "EnumLiteral", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageAction"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc19, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc20, dependentProperties5, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionAreaProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0-13.0-true");
			VersionCompatibility deprecated = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", compatibility, deprecated, "This property is not allowed on areas."));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.0-13.0-true");
			compatibility = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc4, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", deprecated, compatibility, "This property is not allowed on areas."));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3-13.0-true");
			deprecated = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc5, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", compatibility, deprecated, "This property is not allowed on areas."));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionAreaProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("10.0"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("10.0"));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Standard"),
				new EnumPropertyMemberInfo("SplitButton")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("SHOWAS", new EnumPropertyTypeInfo(PropertyKind.ShowAs, "ShowAs", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Standard", isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageActionGroup", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionGroup"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc10, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc11, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageAreaProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0-13.0-true");
			VersionCompatibility deprecated = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", compatibility, deprecated, "This property is not allowed on areas."));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.0-13.0-true");
			compatibility = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc3, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", deprecated, compatibility, "This property is not allowed on areas."));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3-13.0-true");
			deprecated = VersionCompatibility.Parse("1.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc4, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionArea_PageArea", compatibility, deprecated, "This property is not allowed on areas."));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageAreaProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageFieldProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("FILEUPLOADACTION", new PropertyTypeInfo(PropertyKind.FileUploadAction, "FileUploadAction", "String", "MemberReference", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField_PageGroup_PagePart", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", parseFunc5, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", parseFunc6, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,DateTime,Decimal,Duration,Enum,Integer,Option,Time)"));
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[6]
			{
				new EnumPropertyMemberInfo("DontBlank"),
				new EnumPropertyMemberInfo("BlankNeg"),
				new EnumPropertyMemberInfo("BlankNegAndZero"),
				new EnumPropertyMemberInfo("BlankZero"),
				new EnumPropertyMemberInfo("BlankZeroAndPos"),
				new EnumPropertyMemberInfo("BlankPos")
			};
			instance.Add("BLANKNUMBERS", new EnumPropertyTypeInfo(PropertyKind.BlankNumbers, "BlankNumbers", "Enum", "EnumLiteral", parseFunc7, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Integer,Decimal,Duration,Enum,Option)"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			instance.Add("BLANKZERO", new PropertyTypeInfo(PropertyKind.BlankZero, "BlankZero", "Boolean", "Boolean", parseFunc8, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("MINVALUE", new PropertyTypeInfo(PropertyKind.MinValue, "MinValue", "String", "Literal", parseFunc9, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			instance.Add("MAXVALUE", new PropertyTypeInfo(PropertyKind.MaxValue, "MaxValue", "String", "Literal", parseFunc10, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Code,Text,Decimal,Integer,BigInteger)"));
			ImmutableArray<DependentProperty>? incompatibleProperties = ImmutableArray.Create(new DependentProperty("ExtendedDataType(Masked)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Concealed")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("MASKTYPE", new EnumPropertyTypeInfo(PropertyKind.MaskType, "MaskType", "Enum", "EnumLiteral", parseFunc11, null, dependentParentProperties, null, incompatibleProperties, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("NOTBLANK", new PropertyTypeInfo(PropertyKind.NotBlank, "NotBlank", "Boolean", "Boolean", parseFunc12, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Char,Code,Label,String,Text,TextConst)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = CharAllowedPropertyValueValidator.ValidatePropertyValue;
			instance.Add("CHARALLOWED", new PropertyTypeInfo(PropertyKind.CharAllowed, "CharAllowed", "String", "StringLiteral", parseFunc13, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Char,Code,Label,String,Text,TextConst)"));
			instance.Add("DATEFORMULA", new PropertyTypeInfo(PropertyKind.DateFormula, "DateFormula", "Boolean", "Boolean", parseFunc14, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierOrLiteralPropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Boolean,Enum,Option,Integer,BigInteger,Decimal,Code,Text)"));
			instance.Add("VALUESALLOWED", new PropertyTypeInfo(PropertyKind.ValuesAllowed, "ValuesAllowed", "String", "CommaSeparatedIdentifierOrLiteral", parseFunc15, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", parseFunc16, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", parseFunc17, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Date)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CLOSINGDATES", new PropertyTypeInfo(PropertyKind.ClosingDates, "ClosingDates", "Boolean", "Boolean", parseFunc18, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Decimal)"));
			validator = DecimalPlacesPropertyValueValidator.ValidatePropertyValue;
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", parseFunc19, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,BigText,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			options = new EnumPropertyMemberInfo[11]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("PhoneNo", "Phone No."),
				new EnumPropertyMemberInfo("URL"),
				new EnumPropertyMemberInfo("EMail", "E-Mail"),
				new EnumPropertyMemberInfo("Ratio"),
				new EnumPropertyMemberInfo("Masked"),
				new EnumPropertyMemberInfo("Person"),
				new EnumPropertyMemberInfo("Document", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.0")),
				new EnumPropertyMemberInfo("Barcode", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("12.0")),
				new EnumPropertyMemberInfo("RichContent", null, SymbolCompilationScope.Cloud, dependentProperties: ImmutableArray.Create(new DependentProperty("MultiLine(true)")), compatibility: VersionCompatibility.Parse("12.0")),
				new EnumPropertyMemberInfo("Task", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("16.1"))
			};
			instance.Add("EXTENDEDDATATYPE", new EnumPropertyTypeInfo(PropertyKind.ExtendedDatatype, "ExtendedDatatype", "Enum", "EnumLiteral", parseFunc20, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = WidthPropertyValueValidator.ValidatePropertyValue;
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Field_PageField"));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[11]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Standard"),
				new EnumPropertyMemberInfo("StandardAccent"),
				new EnumPropertyMemberInfo("Strong"),
				new EnumPropertyMemberInfo("StrongAccent"),
				new EnumPropertyMemberInfo("Attention"),
				new EnumPropertyMemberInfo("AttentionAccent"),
				new EnumPropertyMemberInfo("Favorable"),
				new EnumPropertyMemberInfo("Unfavorable"),
				new EnumPropertyMemberInfo("Ambiguous"),
				new EnumPropertyMemberInfo("Subordinate")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("STYLE", new EnumPropertyTypeInfo(PropertyKind.Style, "Style", "Enum", "EnumLiteral", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageLabel_PageField", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc25 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStyleExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("STYLEEXPR", new PropertyTypeInfo(PropertyKind.StyleExpr, "StyleExpr", "String", "StyleExpression", parseFunc25, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc26 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc26, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc27 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc27, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			instance.Add("ROWSPAN", new PropertyTypeInfo(PropertyKind.RowSpan, "RowSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			instance.Add("COLUMNSPAN", new PropertyTypeInfo(PropertyKind.ColumnSpan, "ColumnSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc28 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Blob,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", parseFunc28, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc29 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc29, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc30 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("12.0"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", parseFunc30, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc31 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("HIDEVALUE", new PropertyTypeInfo(PropertyKind.HideValue, "HideValue", "String", "ClientSideBooleanExpression", parseFunc31, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc32 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.4"));
			instance.Add("SHOWMANDATORY", new PropertyTypeInfo(PropertyKind.ShowMandatory, "ShowMandatory", "String", "ClientSideBooleanExpression", parseFunc32, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			instance.Add("MULTILINE", new PropertyTypeInfo(PropertyKind.MultiLine, "MultiLine", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc33 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", parseFunc33, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc34 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("LOOKUPPAGEID", new PropertyTypeInfo(PropertyKind.LookupPageId, "LookupPageId", "String", "ObjectReference", parseFunc34, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "LookupFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc35 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DRILLDOWNPAGEID", new PropertyTypeInfo(PropertyKind.DrillDownPageId, "DrillDownPageId", "String", "ObjectReference", parseFunc35, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "DrillDownFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc36 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Standard"),
				new EnumPropertyMemberInfo("Promoted"),
				new EnumPropertyMemberInfo("Optional", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("1.0-3.0-true")),
				new EnumPropertyMemberInfo("Additional")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("IMPORTANCE", new EnumPropertyTypeInfo(PropertyKind.Importance, "Importance", "Enum", "EnumLiteral", parseFunc36, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Standard", isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc37 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("1.0");
			instance.Add("TITLE", new PropertyTypeInfo(PropertyKind.Title, "Title", "Boolean", "Boolean", parseFunc37, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", null, compatibility));
			ParseFunc parseFunc38 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("QUICKENTRY", new PropertyTypeInfo(PropertyKind.QuickEntry, "QuickEntry", "String", "ClientSideBooleanExpression", parseFunc38, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			instance.Add("LOOKUP", new PropertyTypeInfo(PropertyKind.Lookup, "Lookup", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			instance.Add("DRILLDOWN", new PropertyTypeInfo(PropertyKind.DrillDown, "DrillDown", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc39 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("ASSISTEDIT", new PropertyTypeInfo(PropertyKind.AssistEdit, "AssistEdit", "Boolean", "Boolean", parseFunc39, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc40 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("ODATAEDMTYPE", new PropertyTypeInfo(PropertyKind.ODataEDMType, "ODataEDMType", "String", "StringLiteral", parseFunc40, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", null, compatibility));
			ParseFunc parseFunc41 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableRelationPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("TABLERELATION", new PropertyTypeInfo(PropertyKind.TableRelation, "TableRelation", "String", "TableRelation", parseFunc41, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc42 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Date,Decimal,Duration,Integer,Time,DateTime)"));
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("SIGNDISPLACEMENT", new PropertyTypeInfo(PropertyKind.SignDisplacement, "SignDisplacement", "Int32", "Int32Literal", parseFunc42, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", null, compatibility, "The property SignDisplacement is being deprected on page fields. It should be specified on the table field."));
			ParseFunc parseFunc43 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.3");
			instance.Add("NAVIGATIONPAGEID", new PropertyTypeInfo(PropertyKind.NavigationPageId, "NavigationPageId", "String", "ObjectReference", parseFunc43, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", compatibility, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc44 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(Code,Text)"));
			instance.Add("NUMERIC", new PropertyTypeInfo(PropertyKind.Numeric, "Numeric", "Boolean", "Boolean", parseFunc44, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField"));
			ParseFunc parseFunc45 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigText,Code,Guid,Text)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", parseFunc45, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", compatibility));
			ParseFunc parseFunc46 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			incompatibleProperties = ImmutableArray.Create(new DependentProperty("Type(BigText,Code,Guid,Text)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", parseFunc46, null, incompatibleProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", compatibility));
			ParseFunc parseFunc47 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc47, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageField", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc48 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc48, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", compatibility));
			ParseFunc parseFunc49 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc49, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField", compatibility));
			ParseFunc parseFunc50 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc50, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc51 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc51, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc52 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc52, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc53 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc53, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageFieldProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageFieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("FILEUPLOADACTION", new PropertyTypeInfo(PropertyKind.FileUploadAction, "FileUploadAction", "String", "MemberReference", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField_PageGroup_PagePart", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Rows"),
				new EnumPropertyMemberInfo("Columns")
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = GridLayoutPropertyValueValidator.ValidatePropertyValue;
			instance.Add("GRIDLAYOUT", new EnumPropertyTypeInfo(PropertyKind.GridLayout, "GridLayout", "Enum", "EnumLiteral", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, validator, "Layout", null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[1]
			{
				new EnumPropertyMemberInfo("Wide")
			};
			validator = CuegroupLayoutPropertyValueValidator.ValidatePropertyValue;
			instance.Add("CUEGROUPLAYOUT", new EnumPropertyTypeInfo(PropertyKind.CuegroupLayout, "CuegroupLayout", "Enum", "EnumLiteral", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, validator, "Layout", null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseIntegerExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("INDENTATIONCOLUMN", new PropertyTypeInfo(PropertyKind.IndentationColumn, "IndentationColumn", "String", "IntegerExpression", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, "IndentationColumnName", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("INDENTATIONCONTROLS", new PropertyTypeInfo(PropertyKind.IndentationControls, "IndentationControls", "String", "CommaSeparated", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, "Control", prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("FREEZECOLUMN", new PropertyTypeInfo(PropertyKind.FreezeColumn, "FreezeColumn", "String", "PageFieldReference", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, "FreezeColumnID", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			validator = ShowAsTreePropertyValueValidator.ValidatePropertyValue;
			instance.Add("SHOWASTREE", new PropertyTypeInfo(PropertyKind.ShowAsTree, "ShowAsTree", "Boolean", "Boolean", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ShowAsTree(true)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("ExpandAll"),
				new EnumPropertyMemberInfo("CollapseAll")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			validator = TreeInitialStatePropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("TREEINITIALSTATE", new EnumPropertyTypeInfo(PropertyKind.TreeInitialState, "TreeInitialState", "Enum", "EnumLiteral", parseFunc16, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ExpandAll", isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, validator, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageGroup", compatibility, null, null, emitDefaultValue: false));
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			};
			validator = FileUploadRowActionPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("FILEUPLOADROWACTION", new PropertyTypeInfo(PropertyKind.FileUploadRowAction, "FileUploadRowAction", "String", "MemberReference", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageGroup", compatibility));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc19, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc20, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageLabelProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("CAPTIONCLASS", new PropertyTypeInfo(PropertyKind.CaptionClass, "CaptionClass", "String", "TextExpression", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("SHOWCAPTION", new PropertyTypeInfo(PropertyKind.ShowCaption, "ShowCaption", "Boolean", "Boolean", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("MULTILINE", new PropertyTypeInfo(PropertyKind.MultiLine, "MultiLine", "Boolean", "Boolean", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("HIDEVALUE", new PropertyTypeInfo(PropertyKind.HideValue, "HideValue", "String", "ClientSideBooleanExpression", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Standard"),
				new EnumPropertyMemberInfo("Promoted"),
				new EnumPropertyMemberInfo("Additional")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("IMPORTANCE", new EnumPropertyTypeInfo(PropertyKind.Importance, "Importance", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Standard", isObsolete: false, generateMetadata: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.2");
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = WidthPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("4.4");
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[11]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Standard"),
				new EnumPropertyMemberInfo("StandardAccent"),
				new EnumPropertyMemberInfo("Strong"),
				new EnumPropertyMemberInfo("StrongAccent"),
				new EnumPropertyMemberInfo("Attention"),
				new EnumPropertyMemberInfo("AttentionAccent"),
				new EnumPropertyMemberInfo("Favorable"),
				new EnumPropertyMemberInfo("Unfavorable"),
				new EnumPropertyMemberInfo("Ambiguous"),
				new EnumPropertyMemberInfo("Subordinate")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("STYLE", new EnumPropertyTypeInfo(PropertyKind.Style, "Style", "Enum", "EnumLiteral", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageLabel_PageField", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStyleExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("STYLEEXPR", new PropertyTypeInfo(PropertyKind.StyleExpr, "StyleExpr", "String", "StyleExpression", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc16, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			instance.Add("ROWSPAN", new PropertyTypeInfo(PropertyKind.RowSpan, "RowSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			instance.Add("COLUMNSPAN", new PropertyTypeInfo(PropertyKind.ColumnSpan, "ColumnSpan", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageLabel_PageField"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc19, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc20, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageLabelProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageLabelProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPagePartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("FILEUPLOADACTION", new PropertyTypeInfo(PropertyKind.FileUploadAction, "FileUploadAction", "String", "MemberReference", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageField_PageGroup_PagePart", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("1.0-10.1-true"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("ZeroOrOne"),
				new EnumPropertyMemberInfo("Many")
			};
			compatibility = VersionCompatibility.Parse("6.3");
			instance.Add("MULTIPLICITY", new EnumPropertyTypeInfo(PropertyKind.Multiplicity, "Multiplicity", "Enum", "EnumLiteral", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PagePart", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("SubPart"),
				new EnumPropertyMemberInfo("Both")
			};
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, compatibility, "This property does not have any effect on parts."));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, compatibility, "This property does not have any effect on parts."));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc15, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc16, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPagePartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PagePartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageSystemPartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("1.0-10.1-true"));
			VersionCompatibility deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on system and chart parts."));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on system and chart parts."));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("SubPart"),
				new EnumPropertyMemberInfo("Both")
			};
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on parts."));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on parts."));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			deprecated = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc14, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc15, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageSystemPartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageSystemPartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageChartPartProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("1.0-10.1-true"));
			VersionCompatibility deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "String", "ClientSideBooleanExpression", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on system and chart parts."));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on system and chart parts."));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			instance.Add("SUBPAGEVIEW", new PropertyTypeInfo(PropertyKind.SubPageView, "SubPageView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("SUBPAGELINK", new PropertyTypeInfo(PropertyKind.SubPageLink, "SubPageLink", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("SubPart"),
				new EnumPropertyMemberInfo("Both")
			};
			instance.Add("UPDATEPROPAGATION", new EnumPropertyTypeInfo(PropertyKind.UpdatePropagation, "UpdatePropagation", "Enum", "EnumLiteral", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("PROVIDER", new PropertyTypeInfo(PropertyKind.Provider, "Provider", "String", "PageFieldReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePageFieldReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "ProviderID", null, null, emitAsAttribute: true, "Control", prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYNAME", new PropertyTypeInfo(PropertyKind.EntityName, "EntityName", "String", "StringLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("ENTITYSETNAME", new PropertyTypeInfo(PropertyKind.EntitySetName, "EntitySetName", "String", "StringLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on parts."));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			deprecated = VersionCompatibility.Parse("5.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PagePart_PageSystemPart_PageChartPart", null, deprecated, "This property does not have any effect on parts."));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			deprecated = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc12, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			deprecated = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc13, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", deprecated));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageChartPartProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageChartPartProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryColumnProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			instance.Add("COLUMNFILTER", new PropertyTypeInfo(PropertyKind.ColumnFilter, "ColumnFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryColumn_QueryFilter"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[8]
			{
				new EnumPropertyMemberInfo("Day"),
				new EnumPropertyMemberInfo("Month"),
				new EnumPropertyMemberInfo("Year"),
				new EnumPropertyMemberInfo("Sum"),
				new EnumPropertyMemberInfo("Count"),
				new EnumPropertyMemberInfo("Average"),
				new EnumPropertyMemberInfo("Min"),
				new EnumPropertyMemberInfo("Max")
			};
			instance.Add("METHOD", new EnumPropertyTypeInfo(PropertyKind.Method, "Method", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "QueryColumn"));
			instance.Add("REVERSESIGN", new PropertyTypeInfo(PropertyKind.ReverseSign, "ReverseSign", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryColumn"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryColumn", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryColumn", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc8, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc9, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryColumnProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryDataItemProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("DATAITEMLINK", new PropertyTypeInfo(PropertyKind.DataItemLink, "DataItemLink", "String", "QueryDataItemLink", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseQueryDataItemLinkPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryDataItem"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("LeftOuterJoin"),
				new EnumPropertyMemberInfo("InnerJoin"),
				new EnumPropertyMemberInfo("RightOuterJoin"),
				new EnumPropertyMemberInfo("FullOuterJoin"),
				new EnumPropertyMemberInfo("CrossJoin")
			};
			instance.Add("SQLJOINTYPE", new EnumPropertyTypeInfo(PropertyKind.SqlJoinType, "SqlJoinType", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, "DataItemLinkType", null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "QueryDataItem"));
			instance.Add("DATAITEMTABLEFILTER", new PropertyTypeInfo(PropertyKind.DataItemTableFilter, "DataItemTableFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryDataItem"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryDataItemProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetQueryFilterProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			instance.Add("COLUMNFILTER", new PropertyTypeInfo(PropertyKind.ColumnFilter, "ColumnFilter", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "QueryColumn_QueryFilter"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc5, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc6, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupQueryFilterProperty(string name)
	{
		PropertyTypeInfo value = null;
		QueryFilterProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("UpdateNoLocks"),
				new EnumPropertyMemberInfo("Update"),
				new EnumPropertyMemberInfo("Snapshot"),
				new EnumPropertyMemberInfo("Browse"),
				new EnumPropertyMemberInfo("Report")
			};
			instance.Add("TRANSACTIONTYPE", new EnumPropertyTypeInfo(PropertyKind.TransactionType, "TransactionType", "Enum", "EnumLiteral", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "UpdateNoLocks", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort_Report"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("EXTENSIBLE", new PropertyTypeInfo(PropertyKind.Extensible, "Extensible", "Boolean", "Boolean", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			instance.Add("USEREQUESTPAGE", new PropertyTypeInfo(PropertyKind.UseRequestPage, "UseRequestPage", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("USESYSTEMPRINTER", new PropertyTypeInfo(PropertyKind.UseSystemPrinter, "UseSystemPrinter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("ENABLEEXTERNALIMAGES", new PropertyTypeInfo(PropertyKind.EnableExternalImages, "EnableExternalImages", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("ENABLEHYPERLINKS", new PropertyTypeInfo(PropertyKind.EnableHyperlinks, "EnableHyperlinks", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.2");
			instance.Add("ALLOWSCHEDULING", new PropertyTypeInfo(PropertyKind.AllowScheduling, "AllowScheduling", "Boolean", "Boolean", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("EXCELLAYOUTMULTIPLEDATASHEETS", new PropertyTypeInfo(PropertyKind.ExcelLayoutMultipleDataSheets, "ExcelLayoutMultipleDataSheets", "Boolean", "Boolean", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("MAXIMUMDATASETSIZE", new PropertyTypeInfo(PropertyKind.MaximumDatasetSize, "MaximumDatasetSize", "Int32", "Int32Literal", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("MAXIMUMDOCUMENTCOUNT", new PropertyTypeInfo(PropertyKind.MaximumDocumentCount, "MaximumDocumentCount", "Int32", "Int32Literal", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("EXECUTIONTIMEOUT", new PropertyTypeInfo(PropertyKind.ExecutionTimeout, "ExecutionTimeout", "String", "StringLiteral", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = FormatRegionPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("FORMATREGION", new PropertyTypeInfo(PropertyKind.FormatRegion, "FormatRegion", "String", "StringLiteral", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			instance.Add("ENABLEEXTERNALASSEMBLIES", new PropertyTypeInfo(PropertyKind.EnableExternalAssemblies, "EnableExternalAssemblies", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, "OnPrem", emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("PROCESSINGONLY", new PropertyTypeInfo(PropertyKind.ProcessingOnly, "ProcessingOnly", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("SHOWPRINTSTATUS", new PropertyTypeInfo(PropertyKind.ShowPrintStatus, "ShowPrintStatus", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[29]
			{
				new EnumPropertyMemberInfo("Upper"),
				new EnumPropertyMemberInfo("Lower"),
				new EnumPropertyMemberInfo("Middle"),
				new EnumPropertyMemberInfo("Manual"),
				new EnumPropertyMemberInfo("Envelope"),
				new EnumPropertyMemberInfo("ManualFeed"),
				new EnumPropertyMemberInfo("AutomaticFeed"),
				new EnumPropertyMemberInfo("TractorFeed"),
				new EnumPropertyMemberInfo("SmallFormat"),
				new EnumPropertyMemberInfo("LargeFormat"),
				new EnumPropertyMemberInfo("LargeCapacity"),
				new EnumPropertyMemberInfo("Cassette"),
				new EnumPropertyMemberInfo("FormSource"),
				new EnumPropertyMemberInfo("Custom1"),
				new EnumPropertyMemberInfo("Custom2"),
				new EnumPropertyMemberInfo("Custom3"),
				new EnumPropertyMemberInfo("Custom4"),
				new EnumPropertyMemberInfo("Custom5"),
				new EnumPropertyMemberInfo("Custom6"),
				new EnumPropertyMemberInfo("Custom7"),
				new EnumPropertyMemberInfo("Custom8"),
				new EnumPropertyMemberInfo("Custom9"),
				new EnumPropertyMemberInfo("Custom10"),
				new EnumPropertyMemberInfo("Custom11"),
				new EnumPropertyMemberInfo("Custom12"),
				new EnumPropertyMemberInfo("Custom13"),
				new EnumPropertyMemberInfo("Custom14"),
				new EnumPropertyMemberInfo("Custom15"),
				new EnumPropertyMemberInfo("Custom16")
			};
			instance.Add("PAPERSOURCEFIRSTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceFirstPage, "PaperSourceFirstPage", "Enum", "EnumLiteral", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[29]
			{
				new EnumPropertyMemberInfo("Upper"),
				new EnumPropertyMemberInfo("Lower"),
				new EnumPropertyMemberInfo("Middle"),
				new EnumPropertyMemberInfo("Manual"),
				new EnumPropertyMemberInfo("Envelope"),
				new EnumPropertyMemberInfo("ManualFeed"),
				new EnumPropertyMemberInfo("AutomaticFeed"),
				new EnumPropertyMemberInfo("TractorFeed"),
				new EnumPropertyMemberInfo("SmallFormat"),
				new EnumPropertyMemberInfo("LargeFormat"),
				new EnumPropertyMemberInfo("LargeCapacity"),
				new EnumPropertyMemberInfo("Cassette"),
				new EnumPropertyMemberInfo("FormSource"),
				new EnumPropertyMemberInfo("Custom1"),
				new EnumPropertyMemberInfo("Custom2"),
				new EnumPropertyMemberInfo("Custom3"),
				new EnumPropertyMemberInfo("Custom4"),
				new EnumPropertyMemberInfo("Custom5"),
				new EnumPropertyMemberInfo("Custom6"),
				new EnumPropertyMemberInfo("Custom7"),
				new EnumPropertyMemberInfo("Custom8"),
				new EnumPropertyMemberInfo("Custom9"),
				new EnumPropertyMemberInfo("Custom10"),
				new EnumPropertyMemberInfo("Custom11"),
				new EnumPropertyMemberInfo("Custom12"),
				new EnumPropertyMemberInfo("Custom13"),
				new EnumPropertyMemberInfo("Custom14"),
				new EnumPropertyMemberInfo("Custom15"),
				new EnumPropertyMemberInfo("Custom16")
			};
			instance.Add("PAPERSOURCEDEFAULTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceDefaultPage, "PaperSourceDefaultPage", "Enum", "EnumLiteral", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[29]
			{
				new EnumPropertyMemberInfo("Upper"),
				new EnumPropertyMemberInfo("Lower"),
				new EnumPropertyMemberInfo("Middle"),
				new EnumPropertyMemberInfo("Manual"),
				new EnumPropertyMemberInfo("Envelope"),
				new EnumPropertyMemberInfo("ManualFeed"),
				new EnumPropertyMemberInfo("AutomaticFeed"),
				new EnumPropertyMemberInfo("TractorFeed"),
				new EnumPropertyMemberInfo("SmallFormat"),
				new EnumPropertyMemberInfo("LargeFormat"),
				new EnumPropertyMemberInfo("LargeCapacity"),
				new EnumPropertyMemberInfo("Cassette"),
				new EnumPropertyMemberInfo("FormSource"),
				new EnumPropertyMemberInfo("Custom1"),
				new EnumPropertyMemberInfo("Custom2"),
				new EnumPropertyMemberInfo("Custom3"),
				new EnumPropertyMemberInfo("Custom4"),
				new EnumPropertyMemberInfo("Custom5"),
				new EnumPropertyMemberInfo("Custom6"),
				new EnumPropertyMemberInfo("Custom7"),
				new EnumPropertyMemberInfo("Custom8"),
				new EnumPropertyMemberInfo("Custom9"),
				new EnumPropertyMemberInfo("Custom10"),
				new EnumPropertyMemberInfo("Custom11"),
				new EnumPropertyMemberInfo("Custom12"),
				new EnumPropertyMemberInfo("Custom13"),
				new EnumPropertyMemberInfo("Custom14"),
				new EnumPropertyMemberInfo("Custom15"),
				new EnumPropertyMemberInfo("Custom16")
			};
			instance.Add("PAPERSOURCELASTPAGE", new EnumPropertyTypeInfo(PropertyKind.PaperSourceLastPage, "PaperSourceLastPage", "Enum", "EnumLiteral", parseFunc16, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("RDLC"),
				new EnumPropertyMemberInfo("Word"),
				new EnumPropertyMemberInfo("Excel", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("9.0"))
			};
			instance.Add("DEFAULTLAYOUT", new EnumPropertyTypeInfo(PropertyKind.DefaultLayout, "DefaultLayout", "Enum", "EnumLiteral", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "RDLC", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			instance.Add("WORDMERGEDATAITEM", new PropertyTypeInfo(PropertyKind.WordMergeDataItem, "WordMergeDataItem", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Default"),
				new EnumPropertyMemberInfo("Yes"),
				new EnumPropertyMemberInfo("No")
			};
			instance.Add("PDFFONTEMBEDDING", new EnumPropertyTypeInfo(PropertyKind.PdfFontEmbedding, "PdfFontEmbedding", "Enum", "EnumLiteral", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Default", isObsolete: false, generateMetadata: true, null, null, null, "PDFFontEmbedding", null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = RDLCLayoutPropertyValueValidator.ValidatePropertyValue;
			instance.Add("RDLCLAYOUT", new PropertyTypeInfo(PropertyKind.RDLCLayout, "RDLCLayout", "String", "StringLiteral", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = WordLayoutPropertyValueValidator.ValidatePropertyValue;
			instance.Add("WORDLAYOUT", new PropertyTypeInfo(PropertyKind.WordLayout, "WordLayout", "String", "StringLiteral", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = ExcelLayoutPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("EXCELLAYOUT", new PropertyTypeInfo(PropertyKind.ExcelLayout, "ExcelLayout", "String", "StringLiteral", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("DEFAULTRENDERINGLAYOUT", new PropertyTypeInfo(PropertyKind.DefaultRenderingLayout, "DefaultRenderingLayout", "String", "MemberReference", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Normal"),
				new EnumPropertyMemberInfo("PrintLayout")
			};
			instance.Add("PREVIEWMODE", new EnumPropertyTypeInfo(PropertyKind.PreviewMode, "PreviewMode", "Enum", "EnumLiteral", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Normal", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "Report"));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("ADDITIONALSEARCHTERMSML", new PropertyTypeInfo(PropertyKind.AdditionalSearchTermsML, "AdditionalSearchTermsML", "String", "Multilanguage", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc25 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("ADDITIONALSEARCHTERMS", new PropertyTypeInfo(PropertyKind.AdditionalSearchTerms, "AdditionalSearchTerms", "String", "Label", parseFunc25, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc26 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("ReadOnly"),
				new EnumPropertyMemberInfo("ReadWrite")
			};
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("DATAACCESSINTENT", new EnumPropertyTypeInfo(PropertyKind.DataAccessIntent, "DataAccessIntent", "Enum", "EnumLiteral", parseFunc26, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ReadWrite", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: true, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc27 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc27, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Report", compatibility));
			ParseFunc parseFunc28 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc28, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc29 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc29, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc30 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc30, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc31 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Lists"),
				new EnumPropertyMemberInfo("Tasks"),
				new EnumPropertyMemberInfo("ReportsAndAnalysis"),
				new EnumPropertyMemberInfo("Documents"),
				new EnumPropertyMemberInfo("History"),
				new EnumPropertyMemberInfo("Administration")
			};
			instance.Add("USAGECATEGORY", new EnumPropertyTypeInfo(PropertyKind.UsageCategory, "UsageCategory", "Enum", "EnumLiteral", parseFunc31, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Page_Report"));
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)", VersionCompatibility.Parse("1.0-10.0-true"))), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "Page_Report"));
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("UsageCategory(Lists,Tasks,ReportsAndAnalysis,Documents,History,Administration)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_Report"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportDataItemProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("DATAITEMTABLEVIEW", new PropertyTypeInfo(PropertyKind.DataItemTableView, "DataItemTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("DATAITEMLINKREFERENCE", new PropertyTypeInfo(PropertyKind.DataItemLinkReference, "DataItemLinkReference", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("DATAITEMLINK", new PropertyTypeInfo(PropertyKind.DataItemLink, "DataItemLink", "String", "ReportDataItemLink", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseReportDataItemLinkPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("REQUESTFILTERHEADINGML", new PropertyTypeInfo(PropertyKind.RequestFilterHeadingML, "RequestFilterHeadingML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("REQUESTFILTERHEADING", new PropertyTypeInfo(PropertyKind.RequestFilterHeading, "RequestFilterHeading", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("REQUESTFILTERFIELDS", new PropertyTypeInfo(PropertyKind.RequestFilterFields, "RequestFilterFields", "String", "CommaSeparated", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, null, "ReqFilterFields", null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CALCFIELDS", new PropertyTypeInfo(PropertyKind.CalcFields, "CalcFields", "String", "CommaSeparated", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, "Field", prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("MAXITERATION", new PropertyTypeInfo(PropertyKind.MaxIteration, "MaxIteration", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("PRINTONLYIFDETAIL", new PropertyTypeInfo(PropertyKind.PrintOnlyIfDetail, "PrintOnlyIfDetail", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			instance.Add("USETEMPORARY", new PropertyTypeInfo(PropertyKind.UseTemporary, "UseTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportDataItem"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc5, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc6, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportDataItemProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportColumnProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			instance.Add("OPTIONCAPTIONML", new PropertyTypeInfo(PropertyKind.OptionCaptionML, "OptionCaptionML", "String", "Multilanguage", parseFunc2, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			instance.Add("OPTIONCAPTION", new PropertyTypeInfo(PropertyKind.OptionCaption, "OptionCaption", "String", "Label", parseFunc3, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOptionValuesPropertyValue(pti, ref equals);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Option)"));
			instance.Add("OPTIONMEMBERS", new PropertyTypeInfo(PropertyKind.OptionMembers, "OptionMembers", "String", "OptionValues", parseFunc4, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "OptionString", null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseDecimalPlacesPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(Decimal)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = DecimalPlacesPropertyValueValidator.ValidatePropertyValue;
			instance.Add("DECIMALPLACES", new PropertyTypeInfo(PropertyKind.DecimalPlaces, "DecimalPlaces", "String", "DecimalPlaces", parseFunc5, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			instance.Add("AUTOFORMATTYPE", new PropertyTypeInfo(PropertyKind.AutoFormatType, "AutoFormatType", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			dependentParentProperties = ImmutableArray.Create(new DependentProperty("Type(BigInteger,Boolean,Char,Code,Date,DateFormula,DateTime,Decimal,Duration,Enum,Integer,Guid,Label,Media,MediaSet,Option,RecordID,String,Text,Time,TextConst)"));
			instance.Add("AUTOFORMATEXPRESSION", new PropertyTypeInfo(PropertyKind.AutoFormatExpression, "AutoFormatExpression", "String", "TextExpression", parseFunc6, null, dependentParentProperties, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			instance.Add("INCLUDECAPTION", new PropertyTypeInfo(PropertyKind.IncludeCaption, "IncludeCaption", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn", compatibility));
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportColumn"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc10, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc11, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportColumnProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetRequestPageProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXTML", new PropertyTypeInfo(PropertyKind.InstructionalTextML, "InstructionalTextML", "String", "Multilanguage", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("INSTRUCTIONALTEXT", new PropertyTypeInfo(PropertyKind.InstructionalText, "InstructionalText", "String", "Label", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("HELPLINK", new PropertyTypeInfo(PropertyKind.HelpLink, "HelpLink", "String", "StringLiteral", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			VersionCompatibility compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("CONTEXTSENSITIVEHELPPAGE", new PropertyTypeInfo(PropertyKind.ContextSensitiveHelpPage, "ContextSensitiveHelpPage", "String", "StringLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, customizationModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_RequestPage", compatibility));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("AUTOSPLITKEY", new PropertyTypeInfo(PropertyKind.AutoSplitKey, "AutoSplitKey", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("15.0"));
			instance.Add("CARDPAGEID", new PropertyTypeInfo(PropertyKind.CardPageId, "CardPageId", "String", "ObjectReference", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "CardFormID", null, null, emitAsAttribute: true, "Page", prependObjectName: false, isRequired: false, null, "RequestPage", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTextExpressionPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DATACAPTIONEXPRESSION", new PropertyTypeInfo(PropertyKind.DataCaptionExpression, "DataCaptionExpression", "String", "TextExpression", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "DataCaptionExpr", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, ImmutableArray.Create(new DependentProperty("SourceTable")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("INSERTALLOWED", new PropertyTypeInfo(PropertyKind.InsertAllowed, "InsertAllowed", "Boolean", "Boolean", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("MODIFYALLOWED", new PropertyTypeInfo(PropertyKind.ModifyAllowed, "ModifyAllowed", "Boolean", "Boolean", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("DELETEALLOWED", new PropertyTypeInfo(PropertyKind.DeleteAllowed, "DeleteAllowed", "Boolean", "Boolean", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("SOURCETABLE", new PropertyTypeInfo(PropertyKind.SourceTable, "SourceTable", "String", "ObjectReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Table));
			instance.Add("SOURCETABLETEMPORARY", new PropertyTypeInfo(PropertyKind.SourceTableTemporary, "SourceTableTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("SourceTable")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("SourceTable")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("2.2"));
			instance.Add("EDITABLE", new PropertyTypeInfo(PropertyKind.Editable, "Editable", "Boolean", "Boolean", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("SHOWFILTER", new PropertyTypeInfo(PropertyKind.ShowFilter, "ShowFilter", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("SAVEVALUES", new PropertyTypeInfo(PropertyKind.SaveValues, "SaveValues", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("LINKSALLOWED", new PropertyTypeInfo(PropertyKind.LinksAllowed, "LinksAllowed", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("MULTIPLENEWLINES", new PropertyTypeInfo(PropertyKind.MultipleNewLines, "MultipleNewLines", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			instance.Add("POPULATEALLFIELDS", new PropertyTypeInfo(PropertyKind.PopulateAllFields, "PopulateAllFields", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "RequestPage"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc15, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc16, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Page_PageAction_PageActionGroup_PageField_PagePart_PageGroup_RequestPage", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupRequestPageProperty(string name)
	{
		PropertyTypeInfo value = null;
		RequestPageProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetTableProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Codeunit_Page", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("EXTENSIBLE", new PropertyTypeInfo(PropertyKind.Extensible, "Extensible", "Boolean", "Boolean", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("CustomerContent"),
				new EnumPropertyMemberInfo("EndUserIdentifiableInformation"),
				new EnumPropertyMemberInfo("AccountData"),
				new EnumPropertyMemberInfo("EndUserPseudonymousIdentifiers"),
				new EnumPropertyMemberInfo("OrganizationIdentifiableInformation"),
				new EnumPropertyMemberInfo("SystemMetadata"),
				new EnumPropertyMemberInfo("ToBeClassified")
			};
			instance.Add("DATACLASSIFICATION", new EnumPropertyTypeInfo(PropertyKind.DataClassification, "DataClassification", "Enum", "EnumLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "CustomerContent", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Table"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("ToBeClassified"),
				new EnumPropertyMemberInfo("Never"),
				new EnumPropertyMemberInfo("AsReadOnly"),
				new EnumPropertyMemberInfo("AsReadWrite")
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("ALLOWINCUSTOMIZATIONS", new EnumPropertyTypeInfo(PropertyKind.AllowInCustomizations, "AllowInCustomizations", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "ToBeClassified", isObsolete: false, generateMetadata: false, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			instance.Add("DATAPERCOMPANY", new PropertyTypeInfo(PropertyKind.DataPerCompany, "DataPerCompany", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("TableType(Normal)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(PendingMove,Moved)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = MovedToPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("MOVEDTO", new PropertyTypeInfo(PropertyKind.MovedTo, "MovedTo", "String", "Literal", parseFunc9, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			validator = MovedFromPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("12.0");
			instance.Add("MOVEDFROM", new PropertyTypeInfo(PropertyKind.MovedFrom, "MovedFrom", "String", "Literal", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("LOOKUPPAGEID", new PropertyTypeInfo(PropertyKind.LookupPageId, "LookupPageId", "String", "ObjectReference", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "LookupFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("3.0"));
			instance.Add("DRILLDOWNPAGEID", new PropertyTypeInfo(PropertyKind.DrillDownPageId, "DrillDownPageId", "String", "ObjectReference", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "DrillDownFormID", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DATACAPTIONFIELDS", new PropertyTypeInfo(PropertyKind.DataCaptionFields, "DataCaptionFields", "String", "CommaSeparated", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Table"));
			instance.Add("PASTEISVALID", new PropertyTypeInfo(PropertyKind.PasteIsValid, "PasteIsValid", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			instance.Add("LINKEDOBJECT", new PropertyTypeInfo(PropertyKind.LinkedObject, "LinkedObject", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("LinkedObject(true)"));
			compatibility = VersionCompatibility.Parse("1.0");
			instance.Add("LINKEDINTRANSACTION", new PropertyTypeInfo(PropertyKind.LinkedInTransaction, "LinkedInTransaction", "Boolean", "Boolean", parseFunc14, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", null, compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[7]
			{
				new EnumPropertyMemberInfo("Normal"),
				new EnumPropertyMemberInfo("CRM"),
				new EnumPropertyMemberInfo("ExternalSQL", null, SymbolCompilationScope.OnPrem),
				new EnumPropertyMemberInfo("Exchange", null, SymbolCompilationScope.OnPrem),
				new EnumPropertyMemberInfo("MicrosoftGraph", null, SymbolCompilationScope.OnPrem),
				new EnumPropertyMemberInfo("CDS", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("5.0")),
				new EnumPropertyMemberInfo("Temporary", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("6.0"))
			};
			instance.Add("TABLETYPE", new EnumPropertyTypeInfo(PropertyKind.TableType, "TableType", "Enum", "EnumLiteral", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Normal", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Table"));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("TableType(Normal)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("Unspecified"),
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("Row"),
				new EnumPropertyMemberInfo("Page")
			};
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("COMPRESSIONTYPE", new EnumPropertyTypeInfo(PropertyKind.CompressionType, "CompressionType", "Enum", "EnumLiteral", parseFunc16, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Unspecified", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			instance.Add("EXTERNALNAME", new PropertyTypeInfo(PropertyKind.ExternalName, "ExternalName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("TableType(CRM,ExternalSQL,Exchange,MicrosoftGraph,CDS)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			instance.Add("EXTERNALSCHEMA", new PropertyTypeInfo(PropertyKind.ExternalSchema, "ExternalSchema", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("TableType(CRM,ExternalSQL,CDS)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table"));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("Personalization", "Cloud", SymbolCompilationScope.Cloud, null, VersionCompatibility.Parse("4.0"), "The Personalization scope is being deprecated. Use Cloud instead."),
				new EnumPropertyMemberInfo("Extension", "Cloud", SymbolCompilationScope.Cloud, null, VersionCompatibility.Parse("4.0"), "The Extension scope is being deprecated. Use Cloud instead."),
				new EnumPropertyMemberInfo("Internal", "OnPrem", SymbolCompilationScope.Cloud, null, VersionCompatibility.Parse("4.0"), "The Internal scope is being deprecated. Use OnPrem instead."),
				new EnumPropertyMemberInfo("Cloud", "Cloud", SymbolCompilationScope.Cloud, VersionCompatibility.Parse("4.0")),
				new EnumPropertyMemberInfo("OnPrem", "OnPrem", SymbolCompilationScope.Cloud, VersionCompatibility.Parse("4.0"))
			};
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Table"));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("2.0");
			instance.Add("REPLICATEDATA", new PropertyTypeInfo(PropertyKind.ReplicateData, "ReplicateData", "Boolean", "Boolean", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("8.0");
			instance.Add("COLUMNSTOREINDEX", new PropertyTypeInfo(PropertyKind.ColumnStoreIndex, "ColumnStoreIndex", "String", "CommaSeparated", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: false, null, null, null, null, null, null, emitAsAttribute: true, "Field", prependObjectName: false, isRequired: false, null, "Table", compatibility));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending"),
				new EnumPropertyMemberInfo("Removed"),
				new EnumPropertyMemberInfo("PendingMove", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("12.0")),
				new EnumPropertyMemberInfo("Moved", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("12.0"))
			};
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Table", null, null, null, emitDefaultValue: false));
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed,PendingMove,Moved)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: true, isRequired: false, null, "Table"));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending,Removed,PendingMove,Moved)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc22, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupTableProperty(string name)
	{
		PropertyTypeInfo value = null;
		TableProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort_Report_Query_QueryColumn_QueryFilter"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentEntitlementsPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("INHERENTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.InherentEntitlements, "InherentEntitlements", "String", "InherentEntitlements", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInherentPermissionsPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("11.0");
			instance.Add("INHERENTPERMISSIONS", new PropertyTypeInfo(PropertyKind.InherentPermissions, "InherentPermissions", "String", "InherentPermissions", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "Query_Report_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[5]
			{
				new EnumPropertyMemberInfo("UpdateNoLocks"),
				new EnumPropertyMemberInfo("Update"),
				new EnumPropertyMemberInfo("Snapshot"),
				new EnumPropertyMemberInfo("Browse"),
				new EnumPropertyMemberInfo("Report")
			};
			instance.Add("TRANSACTIONTYPE", new EnumPropertyTypeInfo(PropertyKind.TransactionType, "TransactionType", "Enum", "EnumLiteral", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "UpdateNoLocks", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort_Report"));
			instance.Add("DEFAULTFIELDSVALIDATION", new PropertyTypeInfo(PropertyKind.DefaultFieldsValidation, "DefaultFieldsValidation", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("DEFAULTNAMESPACE", new PropertyTypeInfo(PropertyKind.DefaultNamespace, "DefaultNamespace", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(Xml)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Import"),
				new EnumPropertyMemberInfo("Export"),
				new EnumPropertyMemberInfo("Both")
			};
			instance.Add("DIRECTION", new EnumPropertyTypeInfo(PropertyKind.Direction, "Direction", "Enum", "EnumLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Both", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("Format(Xml)"));
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("UTF8", "UTF-8"),
				new EnumPropertyMemberInfo("UTF16", "UTF-16"),
				new EnumPropertyMemberInfo("ISO88592", "ISO-8859-2")
			};
			instance.Add("ENCODING", new EnumPropertyTypeInfo(PropertyKind.Encoding, "Encoding", "Enum", "EnumLiteral", parseFunc8, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "UTF16", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("FIELDDELIMITER", new PropertyTypeInfo(PropertyKind.FieldDelimiter, "FieldDelimiter", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(VariableText)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "\"", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("FIELDSEPARATOR", new PropertyTypeInfo(PropertyKind.FieldSeparator, "FieldSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(VariableText)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, ",", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("FILENAME", new PropertyTypeInfo(PropertyKind.FileName, "FileName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Xml"),
				new EnumPropertyMemberInfo("VariableText", "Variable Text"),
				new EnumPropertyMemberInfo("FixedText", "Fixed Text")
			};
			instance.Add("FORMAT", new EnumPropertyTypeInfo(PropertyKind.Format, "Format", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Xml", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Legacy", "C/SIDE Format/Evaluate"),
				new EnumPropertyMemberInfo("Xml", "XML Format/Evaluate")
			};
			instance.Add("FORMATEVALUATE", new EnumPropertyTypeInfo(PropertyKind.FormatEvaluate, "FormatEvaluate", "Enum", "EnumLiteral", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Legacy", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("INLINESCHEMA", new PropertyTypeInfo(PropertyKind.InlineSchema, "InlineSchema", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(Xml)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierEqualsStringListPropertyValue(pti, ref equals);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("Format(Xml)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = NamespacesPropertyValueValidator.ValidatePropertyValue;
			instance.Add("NAMESPACES", new PropertyTypeInfo(PropertyKind.Namespaces, "Namespaces", "String", "CommaSeparatedIdentifierEqualsStringList", parseFunc11, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "Permission", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("PRESERVEWHITESPACE", new PropertyTypeInfo(PropertyKind.PreserveWhiteSpace, "PreserveWhiteSpace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("RECORDSEPARATOR", new PropertyTypeInfo(PropertyKind.RecordSeparator, "RecordSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(VariableText,FixedText)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "<NewLine>", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("TABLESEPARATOR", new PropertyTypeInfo(PropertyKind.TableSeparator, "TableSeparator", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(VariableText,FixedText)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "<NewLine><NewLine>", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("Format(VariableText,FixedText)"));
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("MSDOS", "MS-DOS"),
				new EnumPropertyMemberInfo("UTF8", "UTF-8"),
				new EnumPropertyMemberInfo("UTF16", "UTF-16"),
				new EnumPropertyMemberInfo("WINDOWS")
			};
			instance.Add("TEXTENCODING", new EnumPropertyTypeInfo(PropertyKind.TextEncoding, "TextEncoding", "Enum", "EnumLiteral", parseFunc12, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "MSDOS", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("USEDEFAULTNAMESPACE", new PropertyTypeInfo(PropertyKind.UseDefaultNamespace, "UseDefaultNamespace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(Xml)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("USELAX", new PropertyTypeInfo(PropertyKind.UseLax, "UseLax", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("Format(Xml)")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			instance.Add("USEREQUESTPAGE", new PropertyTypeInfo(PropertyKind.UseRequestPage, "UseRequestPage", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, "UseRequestForm", null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("Format(Xml)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("V10", "1.0"),
				new EnumPropertyMemberInfo("V11", "1.1")
			};
			instance.Add("XMLVERSIONNO", new EnumPropertyTypeInfo(PropertyKind.XmlVersionNo, "XmlVersionNo", "Enum", "EnumLiteral", parseFunc13, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "V10", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPort"));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc15, dependentProperties5, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc16, dependentProperties6, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTextElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Text"),
				new EnumPropertyMemberInfo("BigText")
			};
			instance.Add("TEXTTYPE", new EnumPropertyTypeInfo(PropertyKind.TextType, "TextType", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Text", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextAttribute_XmlPortTextElement"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Zero"),
				new EnumPropertyMemberInfo("Once")
			};
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Once", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Once"),
				new EnumPropertyMemberInfo("Unbounded")
			};
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Unbounded", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortTableElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTextElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTextElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortFieldElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortFieldElement"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Yes"),
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Undefined")
			};
			instance.Add("FIELDVALIDATE", new EnumPropertyTypeInfo(PropertyKind.FieldValidate, "FieldValidate", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Undefined", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortFieldElement"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Zero"),
				new EnumPropertyMemberInfo("Once")
			};
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Once", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Once"),
				new EnumPropertyMemberInfo("Unbounded")
			};
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Once", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortFieldElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortFieldElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortFieldElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTableElementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("LINKTABLE", new PropertyTypeInfo(PropertyKind.LinkTable, "LinkTable", "String", "MemberReference", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMemberReferencePropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("LINKTABLEFORCEINSERT", new PropertyTypeInfo(PropertyKind.LinkTableForceInsert, "LinkTableForceInsert", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("LINKFIELDS", new PropertyTypeInfo(PropertyKind.LinkFields, "LinkFields", "String", "TableFilter", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableFilterPropertyValue(pti);
			}, ImmutableArray.Create(new DependentProperty("LinkTable")), null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERFIELDS", new PropertyTypeInfo(PropertyKind.RequestFilterFields, "RequestFilterFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, "ReqFilterFields", null, null, emitAsAttribute: false, "Field", prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERHEADINGML", new PropertyTypeInfo(PropertyKind.RequestFilterHeadingML, "RequestFilterHeadingML", "String", "Multilanguage", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "ReqFilterHeadingML", null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("REQUESTFILTERHEADING", new PropertyTypeInfo(PropertyKind.RequestFilterHeading, "RequestFilterHeading", "String", "Label", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, "ReqFilterHeadingML", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("SOURCETABLEVIEW", new PropertyTypeInfo(PropertyKind.SourceTableView, "SourceTableView", "String", "TableView", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseTableViewPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("USETEMPORARY", new PropertyTypeInfo(PropertyKind.UseTemporary, "UseTemporary", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, "Temporary", null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("AUTOREPLACE", new PropertyTypeInfo(PropertyKind.AutoReplace, "AutoReplace", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("AUTOSAVE", new PropertyTypeInfo(PropertyKind.AutoSave, "AutoSave", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("AUTOUPDATE", new PropertyTypeInfo(PropertyKind.AutoUpdate, "AutoUpdate", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			instance.Add("CALCFIELDS", new PropertyTypeInfo(PropertyKind.CalcFields, "CalcFields", "String", "CommaSeparated", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, "Field", prependObjectName: false, isRequired: false, null, "XmlPortTableElement"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Zero"),
				new EnumPropertyMemberInfo("Once")
			};
			instance.Add("MINOCCURS", new EnumPropertyTypeInfo(PropertyKind.MinOccurs, "MinOccurs", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Once", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Once"),
				new EnumPropertyMemberInfo("Unbounded")
			};
			instance.Add("MAXOCCURS", new EnumPropertyTypeInfo(PropertyKind.MaxOccurs, "MaxOccurs", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Unbounded", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortTableElement"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTableElementProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTableElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortFieldAttributeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("AUTOCALCFIELD", new PropertyTypeInfo(PropertyKind.AutoCalcField, "AutoCalcField", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortFieldElement"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Yes"),
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Undefined")
			};
			instance.Add("FIELDVALIDATE", new EnumPropertyTypeInfo(PropertyKind.FieldValidate, "FieldValidate", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Undefined", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortFieldElement"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Required"),
				new EnumPropertyMemberInfo("Optional")
			};
			instance.Add("OCCURRENCE", new EnumPropertyTypeInfo(PropertyKind.Occurrence, "Occurrence", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Required", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortFieldAttributeProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortFieldAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetXmlPortTextAttributeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_Field_Key_Page_PageAction_PageActionArea_PageActionGroup_PageArea_PageField_PageGroup_PageLabel_PagePart_PageSystemPart_PageChartPart_Query_QueryColumn_QueryDataItem_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_Table_XmlPort_XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Text"),
				new EnumPropertyMemberInfo("BigText")
			};
			instance.Add("TEXTTYPE", new EnumPropertyTypeInfo(PropertyKind.TextType, "TextType", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Text", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortTextAttribute_XmlPortTextElement"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Required"),
				new EnumPropertyMemberInfo("Optional")
			};
			instance.Add("OCCURRENCE", new EnumPropertyTypeInfo(PropertyKind.Occurrence, "Occurrence", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Required", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: false, prependObjectName: false, isRequired: false, null, "XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("XMLNAME", new PropertyTypeInfo(PropertyKind.XmlName, "XmlName", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("NAMESPACEPREFIX", new PropertyTypeInfo(PropertyKind.NamespacePrefix, "NamespacePrefix", "String", "StringLiteral", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("UNBOUND", new PropertyTypeInfo(PropertyKind.Unbound, "Unbound", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			instance.Add("WIDTH", new PropertyTypeInfo(PropertyKind.Width, "Width", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "0", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "XmlPortTextElement_XmlPortFieldElement_XmlPortTableElement_XmlPortFieldAttribute_XmlPortTextAttribute"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupXmlPortTextAttributeProperty(string name)
	{
		PropertyTypeInfo value = null;
		XmlPortTextAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetFieldGroupProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "FieldGroup", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc4, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: true, isRequired: false, null, "FieldGroup", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc5, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "FieldGroup", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupFieldGroupProperty(string name)
	{
		PropertyTypeInfo value = null;
		FieldGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionSeparatorProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Table_Field_PageField_FieldGroup_Page_RequestPage_PageLabel_PageGroup_PagePart_PageSystemPart_PageAction_PageActionSeparator"));
			instance.Add("ISHEADER", new PropertyTypeInfo(PropertyKind.IsHeader, "IsHeader", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionSeparator"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc4, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc5, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionSeparatorProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionSeparatorProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetEnumValueProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("2.3");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "EnumValue", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("2.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "EnumValue", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierEqualsIdentifierListPropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("IMPLEMENTATION", new PropertyTypeInfo(PropertyKind.Implementation, "Implementation", "String", "CommaSeparatedIdentifierEqualsIdentifierList", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "EnumValue", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc5, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc6, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupEnumValueProperty(string name)
	{
		PropertyTypeInfo value = null;
		EnumValueProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetDotNetAssemblyProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("VERSION", new PropertyTypeInfo(PropertyKind.Version, "Version", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "DotNetAssembly"));
			instance.Add("CULTURE", new PropertyTypeInfo(PropertyKind.Culture, "Culture", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "DotNetAssembly"));
			instance.Add("PUBLICKEYTOKEN", new PropertyTypeInfo(PropertyKind.PublicKeyToken, "PublicKeyToken", "String", "Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "DotNetAssembly"));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupDotNetAssemblyProperty(string name)
	{
		PropertyTypeInfo value = null;
		DotNetAssemblyProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetDotNetTypeDeclarationProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("2.0");
			instance.Add("ISCONTROLADDIN", new PropertyTypeInfo(PropertyKind.IsControlAddIn, "IsControlAddIn", "Boolean", "Boolean", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "DotNetTypeDeclaration", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupDotNetTypeDeclarationProperty(string name)
	{
		PropertyTypeInfo value = null;
		DotNetTypeDeclarationProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageActionRefProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "Boolean", "Boolean", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionRef", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageActionRef", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc3, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionRef", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc4, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageActionRef", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageActionRefProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageActionRefProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageCustomActionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[3]
			{
				new EnumPropertyMemberInfo("Flow"),
				new EnumPropertyMemberInfo("FlowTemplate", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("11.0")),
				new EnumPropertyMemberInfo("FlowTemplateGallery", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("11.0"))
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("CUSTOMACTIONTYPE", new EnumPropertyTypeInfo(PropertyKind.CustomActionType, "CustomActionType", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Flow", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ELLIPSIS", new PropertyTypeInfo(PropertyKind.Ellipsis, "Ellipsis", "Boolean", "Boolean", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("CustomActionType(Flow)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = FlowIdPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("13.0");
			VersionCompatibility compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("FLOWID", new PropertyTypeInfo(PropertyKind.FlowId, "FlowId", "String", "StringLiteral", parseFunc12, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, compatibility, "PageCustomAction", compatibility2));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("CustomActionType(Flow)"), new DependentProperty("FlowId"));
			validator = FlowEnvironmentIdPropertyValueValidator.ValidatePropertyValue;
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("FLOWENVIRONMENTID", new PropertyTypeInfo(PropertyKind.FlowEnvironmentId, "FlowEnvironmentId", "String", "StringLiteral", parseFunc13, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("CustomActionType(FlowTemplate,FlowTemplateGallery)"));
			compatibility2 = VersionCompatibility.Parse("11.0");
			instance.Add("FLOWCAPTION", new PropertyTypeInfo(PropertyKind.FlowCaption, "FlowCaption", "String", "StringLiteral", parseFunc14, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("CustomActionType(FlowTemplate)"));
			validator = FlowTemplateIdPropertyValueValidator.ValidatePropertyValue;
			compatibility2 = VersionCompatibility.Parse("11.0");
			instance.Add("FLOWTEMPLATEID", new PropertyTypeInfo(PropertyKind.FlowTemplateId, "FlowTemplateId", "String", "StringLiteral", parseFunc15, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties5 = ImmutableArray.Create(new DependentProperty("CustomActionType(FlowTemplateGallery)"));
			compatibility2 = VersionCompatibility.Parse("11.0");
			instance.Add("FLOWTEMPLATECATEGORYNAME", new PropertyTypeInfo(PropertyKind.FlowTemplateCategoryName, "FlowTemplateCategoryName", "String", "StringLiteral", parseFunc16, dependentProperties5, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties6 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc17, dependentProperties6, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "No", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties7 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc19, dependentProperties7, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties8 = ImmutableArray.Create(new DependentProperty("CustomActionType(Flow)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Page"),
				new EnumPropertyMemberInfo("Repeater")
			};
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc20, dependentProperties8, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("SHORTCUTKEY", new PropertyTypeInfo(PropertyKind.ShortcutKey, "ShortcutKey", "String", "Literal", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, "ShortCutKey", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc23 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc23, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			ParseFunc parseFunc24 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility2 = VersionCompatibility.Parse("10.0");
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc24, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomAction", compatibility2));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageCustomActionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageCustomActionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageSystemActionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemAction", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemAction", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemAction", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemAction", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("12.1");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageSystemAction", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageSystemActionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageSystemActionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageFileUploadActionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ABOUTTITLEML", new PropertyTypeInfo(PropertyKind.AboutTitleML, "AboutTitleML", "String", "Multilanguage", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ABOUTTITLE", new PropertyTypeInfo(PropertyKind.AboutTitle, "AboutTitle", "String", "Label", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ABOUTTEXTML", new PropertyTypeInfo(PropertyKind.AboutTextML, "AboutTextML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ABOUTTEXT", new PropertyTypeInfo(PropertyKind.AboutText, "AboutText", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ALLOWEDFILEEXTENSIONS", new PropertyTypeInfo(PropertyKind.AllowedFileExtensions, "AllowedFileExtensions", "string", "CommaSeparatedStrings", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ALLOWMULTIPLEFILES", new PropertyTypeInfo(PropertyKind.AllowMultipleFiles, "AllowMultipleFiles", "Boolean", "Boolean", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc10, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc11, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc13 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc13, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc14 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "String", "ClientSideBooleanExpression", parseFunc14, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc15 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc15, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc16 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseImagePropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("IMAGE", new PropertyTypeInfo(PropertyKind.Image, "Image", "String", "Image", parseFunc16, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc17 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("ACCESSBYPERMISSION", new PropertyTypeInfo(PropertyKind.AccessByPermission, "AccessByPermission", "String", "Permission", parseFunc17, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc18 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc18, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc19 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Page"),
				new EnumPropertyMemberInfo("Repeater")
			};
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc19, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc20 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("None"),
				new EnumPropertyMemberInfo("LeftSwipe"),
				new EnumPropertyMemberInfo("RightSwipe"),
				new EnumPropertyMemberInfo("ContextMenu")
			};
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("GESTURE", new EnumPropertyTypeInfo(PropertyKind.Gesture, "Gesture", "Enum", "EnumLiteral", parseFunc20, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc21 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLiteralPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("SHORTCUTKEY", new PropertyTypeInfo(PropertyKind.ShortcutKey, "ShortcutKey", "String", "Literal", parseFunc21, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, "ShortCutKey", null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			ParseFunc parseFunc22 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("13.0");
			instance.Add("INFOOTERBAR", new PropertyTypeInfo(PropertyKind.InFooterBar, "InFooterBar", "Boolean", "Boolean", parseFunc22, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageFileUploadAction", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageFileUploadActionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageFileUploadActionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageViewProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("4.0"));
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("4.0"));
			VersionCompatibility compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseFiltersPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("FILTERS", new PropertyTypeInfo(PropertyKind.Filters, "Filters", "String", "Filters", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseOrderByPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("3.0");
			instance.Add("ORDERBY", new PropertyTypeInfo(PropertyKind.OrderBy, "OrderBy", "String", "OrderBy", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("SHAREDLAYOUT", new PropertyTypeInfo(PropertyKind.SharedLayout, "SharedLayout", "Boolean", "Boolean", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc8, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc9, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageViewProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageViewProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageAnalysisViewProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = DefinitionFilePropertyValueValidator.ValidatePropertyValue;
			VersionCompatibility compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("DEFINITIONFILE", new PropertyTypeInfo(PropertyKind.DefinitionFile, "DefinitionFile", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "PageAnalysisView", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAnalysisView", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAnalysisView", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAnalysisView", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			extensionModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("TOOLTIPML", new PropertyTypeInfo(PropertyKind.ToolTipML, "ToolTipML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, extensionModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAnalysisView", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("17.0");
			instance.Add("TOOLTIP", new PropertyTypeInfo(PropertyKind.ToolTip, "ToolTip", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageAnalysisView", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageAnalysisViewProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageAnalysisViewProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportExtensionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = RDLCLayoutPropertyValueValidator.ValidatePropertyValue;
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("RDLCLAYOUT", new PropertyTypeInfo(PropertyKind.RDLCLayout, "RDLCLayout", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportExtension", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			validator = WordLayoutPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("WORDLAYOUT", new PropertyTypeInfo(PropertyKind.WordLayout, "WordLayout", "String", "StringLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportExtension", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo();
			validator = ExcelLayoutPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("EXCELLAYOUT", new PropertyTypeInfo(PropertyKind.ExcelLayout, "ExcelLayout", "String", "StringLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportExtension", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportExtensionProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetRequestPageExtensionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupRequestPageExtensionProperty(string name)
	{
		PropertyTypeInfo value = null;
		RequestPageExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetReportLayoutProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[4]
			{
				new EnumPropertyMemberInfo("RDLC"),
				new EnumPropertyMemberInfo("Word"),
				new EnumPropertyMemberInfo("Excel"),
				new EnumPropertyMemberInfo("Custom")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("TYPE", new EnumPropertyTypeInfo(PropertyKind.Type, "Type", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, null, isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: true, null, "ReportLayout", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("SUMMARYML", new PropertyTypeInfo(PropertyKind.SummaryML, "SummaryML", "String", "Multilanguage", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("SUMMARY", new PropertyTypeInfo(PropertyKind.Summary, "Summary", "String", "Label", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("Type(Custom)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = MimeTypePropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("MIMETYPE", new PropertyTypeInfo(PropertyKind.MimeType, "MimeType", "String", "StringLiteral", parseFunc4, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "ReportLayout", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = LayoutFilePropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("LAYOUTFILE", new PropertyTypeInfo(PropertyKind.LayoutFile, "LayoutFile", "String", "StringLiteral", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "ReportLayout", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("9.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc9, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc10, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties4 = ImmutableArray.Create(new DependentProperty("Type(Excel)"));
			compatibility = VersionCompatibility.Parse("15.0");
			instance.Add("EXCELLAYOUTMULTIPLEDATASHEETS", new PropertyTypeInfo(PropertyKind.ExcelLayoutMultipleDataSheets, "ExcelLayoutMultipleDataSheets", "Boolean", "Boolean", parseFunc11, dependentProperties4, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: false, null, prependObjectName: false, isRequired: false, null, "ReportLayout", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupReportLayoutProperty(string name)
	{
		PropertyTypeInfo value = null;
		ReportLayoutProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetProfileProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("DESCRIPTION", new PropertyTypeInfo(PropertyKind.Description, "Description", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseObjectReferencePropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("ROLECENTER", new PropertyTypeInfo(PropertyKind.RoleCenter, "RoleCenter", "String", "ObjectReference", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.Page));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedObjectNameReferencesPropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			instance.Add("CUSTOMIZATIONS", new PropertyTypeInfo(PropertyKind.Customizations, "Customizations", "String", "CommaSeparatedObjectNameReferences", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, "PageExtensions", null, null, emitAsAttribute: true, "PageCustomization", prependObjectName: false, isRequired: false, null, "Profile", null, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.PageCustomization));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("ENABLED", new PropertyTypeInfo(PropertyKind.Enabled, "Enabled", "Boolean", "Boolean", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("PROMOTED", new PropertyTypeInfo(PropertyKind.Promoted, "Promoted", "Boolean", "Boolean", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("PROFILEDESCRIPTION", new PropertyTypeInfo(PropertyKind.ProfileDescription, "ProfileDescription", "String", "Label", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			extensionModifiability = new PropertyModifiabilityInfo(VersionCompatibility.Parse("14.0"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("PROFILEDESCRIPTIONML", new PropertyTypeInfo(PropertyKind.ProfileDescriptionML, "ProfileDescriptionML", "String", "Multilanguage", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, extensionModifiability, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Profile", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc11, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc12, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageView_Profile", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupProfileProperty(string name)
	{
		PropertyTypeInfo value = null;
		ProfileProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageCustomizationObjectProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			VersionCompatibility compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("CLEARLAYOUT", new PropertyTypeInfo(PropertyKind.ClearLayout, "ClearLayout", "Boolean", "Boolean", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomizationObject", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("CLEARACTIONS", new PropertyTypeInfo(PropertyKind.ClearActions, "ClearActions", "Boolean", "Boolean", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomizationObject", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("PageType(List)"));
			customizationModifiability = new PropertyModifiabilityInfo();
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("CLEARVIEWS", new PropertyTypeInfo(PropertyKind.ClearViews, "ClearViews", "Boolean", "Boolean", parseFunc3, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: false, null, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageCustomizationObject", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageCustomizationObjectProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageCustomizationObjectProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetControlAddInObjectProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			instance.Add("SCRIPTS", new PropertyTypeInfo(PropertyKind.Scripts, "Scripts", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("STYLESHEETS", new PropertyTypeInfo(PropertyKind.StyleSheets, "StyleSheets", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("IMAGES", new PropertyTypeInfo(PropertyKind.Images, "Images", "String", "CommaSeparatedStrings", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedStringsPropertyValue(pti, ref equals);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("STARTUPSCRIPT", new PropertyTypeInfo(PropertyKind.StartupScript, "StartupScript", "String", "StringLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("RECREATESCRIPT", new PropertyTypeInfo(PropertyKind.RecreateScript, "RecreateScript", "String", "StringLiteral", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			instance.Add("REFRESHSCRIPT", new PropertyTypeInfo(PropertyKind.RefreshScript, "RefreshScript", "String", "StringLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("REQUESTEDHEIGHT", new PropertyTypeInfo(PropertyKind.RequestedHeight, "RequestedHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("REQUESTEDWIDTH", new PropertyTypeInfo(PropertyKind.RequestedWidth, "RequestedWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("MINIMUMHEIGHT", new PropertyTypeInfo(PropertyKind.MinimumHeight, "MinimumHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("MINIMUMWIDTH", new PropertyTypeInfo(PropertyKind.MinimumWidth, "MinimumWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("MAXIMUMHEIGHT", new PropertyTypeInfo(PropertyKind.MaximumHeight, "MaximumHeight", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("MAXIMUMWIDTH", new PropertyTypeInfo(PropertyKind.MaximumWidth, "MaximumWidth", "Int32", "Int32Literal", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseInt32LiteralPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("VERTICALSHRINK", new PropertyTypeInfo(PropertyKind.VerticalShrink, "VerticalShrink", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("HORIZONTALSHRINK", new PropertyTypeInfo(PropertyKind.HorizontalShrink, "HorizontalShrink", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("VERTICALSTRETCH", new PropertyTypeInfo(PropertyKind.VerticalStretch, "VerticalStretch", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			instance.Add("HORIZONTALSTRETCH", new PropertyTypeInfo(PropertyKind.HorizontalStretch, "HorizontalStretch", "Boolean", "Boolean", delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			}, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "ControlAddInObject"));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc5, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc6, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupControlAddInObjectProperty(string name)
	{
		PropertyTypeInfo value = null;
		ControlAddInObjectProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPageUserControlProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseClientSideBooleanExpressionPropertyValue(pti);
			};
			PropertyModifiabilityInfo extensionModifiability = new PropertyModifiabilityInfo();
			PropertyModifiabilityInfo customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("VISIBLE", new PropertyTypeInfo(PropertyKind.Visible, "Visible", "String", "ClientSideBooleanExpression", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "true", generateMetadata: true, emitDefaultValue: true, extensionModifiability, customizationModifiability, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageUserControl"));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedPropertyValue(pti, ref equals);
			};
			customizationModifiability = new PropertyModifiabilityInfo();
			instance.Add("APPLICATIONAREA", new PropertyTypeInfo(PropertyKind.ApplicationArea, "ApplicationArea", "String", "CommaSeparated", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, customizationModifiability, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "PageUserControl", null, null, null, defaultValueFromDeclaringApplicationObject: true));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc3, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "PageUserControl", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc4, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageUserControl", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("16.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc5, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PageUserControl", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPageUserControlProperty(string name)
	{
		PropertyTypeInfo value = null;
		PageUserControlProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetEnumTypeProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc2, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("4.3");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc3, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Codeunit_EnumType_EnumValue_PageAction_PageActionGroup_PageActionSeparator_PagePart_PageSystemPart_PageChartPart_PageGroup_PageLabel_Query_QueryColumn_QueryFilter_Report_ReportDataItem_ReportColumn_RequestPage_XmlPort", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("2.0");
			instance.Add("EXTENSIBLE", new PropertyTypeInfo(PropertyKind.Extensible, "Extensible", "Boolean", "Boolean", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierEqualsIdentifierListPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("DEFAULTIMPLEMENTATION", new PropertyTypeInfo(PropertyKind.DefaultImplementation, "DefaultImplementation", "String", "CommaSeparatedIdentifierEqualsIdentifierList", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedIdentifierEqualsIdentifierListPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("UNKNOWNVALUEIMPLEMENTATION", new PropertyTypeInfo(PropertyKind.UnknownValueImplementation, "UnknownValueImplementation", "String", "CommaSeparatedIdentifierEqualsIdentifierList", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "Identifier", prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("ASSIGNMENTCOMPATIBILITY", new PropertyTypeInfo(PropertyKind.AssignmentCompatibility, "AssignmentCompatibility", "Boolean", "Boolean", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("AssignmentCompatibility(true)"));
			compatibility = VersionCompatibility.Parse("5.0");
			instance.Add("ASSIGNMENTCOMPATIBILITYREASON", new PropertyTypeInfo(PropertyKind.AssignmentCompatibilityReason, "AssignmentCompatibilityReason", "String", "StringLiteral", parseFunc8, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("6.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc11 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Cloud", "Cloud"),
				new EnumPropertyMemberInfo("OnPrem", "OnPrem")
			};
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc11, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Cloud", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "EnumType", compatibility));
			ParseFunc parseFunc12 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc12, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "EnumType_Interface_PermissionSet", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupEnumTypeProperty(string name)
	{
		PropertyTypeInfo value = null;
		EnumTypeProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetInterfaceProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc2, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc3, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Cloud", "Cloud"),
				new EnumPropertyMemberInfo("OnPrem", "OnPrem")
			};
			compatibility = VersionCompatibility.Parse("14.0");
			instance.Add("SCOPE", new EnumPropertyTypeInfo(PropertyKind.Scope, "Scope", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Cloud", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Interface", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "EnumType_Interface_PermissionSet", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupInterfaceProperty(string name)
	{
		PropertyTypeInfo value = null;
		InterfaceProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPermissionSetProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("No"),
				new EnumPropertyMemberInfo("Pending")
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETESTATE", new EnumPropertyTypeInfo(PropertyKind.ObsoleteState, "ObsoleteState", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "No", isObsolete: false, generateMetadata: false, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETEREASON", new PropertyTypeInfo(PropertyKind.ObsoleteReason, "ObsoleteReason", "String", "StringLiteral", parseFunc2, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("ObsoleteState(Pending)"));
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBSOLETETAG", new PropertyTypeInfo(PropertyKind.ObsoleteTag, "ObsoleteTag", "String", "StringLiteral", parseFunc3, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "Interface_ControlAddInObject_PermissionSet", compatibility));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Public"),
				new EnumPropertyMemberInfo("Internal")
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ACCESS", new EnumPropertyTypeInfo(PropertyKind.Access, "Access", "Enum", "EnumLiteral", parseFunc4, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Public", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: false, isRequired: false, null, "EnumType_Interface_PermissionSet", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseMultilanguagePropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("CAPTIONML", new PropertyTypeInfo(PropertyKind.CaptionML, "CaptionML", "String", "Multilanguage", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PermissionSet", compatibility));
			ParseFunc parseFunc6 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseLabelPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("CAPTION", new PropertyTypeInfo(PropertyKind.Caption, "Caption", "String", "Label", parseFunc6, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PermissionSet", compatibility));
			ParseFunc parseFunc7 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseBooleanPropertyValue(pti);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ASSIGNABLE", new PropertyTypeInfo(PropertyKind.Assignable, "Assignable", "Boolean", "Boolean", parseFunc7, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, "false", generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PermissionSet", compatibility));
			ParseFunc parseFunc8 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedObjectNameReferencesPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("10.0");
			instance.Add("EXCLUDEDPERMISSIONSETS", new PropertyTypeInfo(PropertyKind.ExcludedPermissionSets, "ExcludedPermissionSets", "String", "CommaSeparatedObjectNameReferences", parseFunc8, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "PermissionSet", prependObjectName: false, isRequired: false, null, "PermissionSet", compatibility, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.PermissionSet));
			ParseFunc parseFunc9 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedObjectNameReferencesPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("INCLUDEDPERMISSIONSETS", new PropertyTypeInfo(PropertyKind.IncludedPermissionSets, "IncludedPermissionSets", "String", "CommaSeparatedObjectNameReferences", parseFunc9, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "PermissionSet", prependObjectName: false, isRequired: false, null, "PermissionSet_PermissionSetExtension", compatibility, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.PermissionSet));
			ParseFunc parseFunc10 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionSetPermissionListPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "PermissionSetPermissionList", parseFunc10, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PermissionSet_PermissionSetExtension", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPermissionSetProperty(string name)
	{
		PropertyTypeInfo value = null;
		PermissionSetProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetPermissionSetExtensionProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedObjectNameReferencesPropertyValue(pti, ref equals);
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("INCLUDEDPERMISSIONSETS", new PropertyTypeInfo(PropertyKind.IncludedPermissionSets, "IncludedPermissionSets", "String", "CommaSeparatedObjectNameReferences", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "PermissionSet", prependObjectName: false, isRequired: false, null, "PermissionSet_PermissionSetExtension", compatibility, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.PermissionSet));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParsePermissionSetPermissionListPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("PERMISSIONS", new PropertyTypeInfo(PropertyKind.Permissions, "Permissions", "String", "PermissionSetPermissionList", parseFunc2, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: false, null, "PermissionSet_PermissionSetExtension", compatibility));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupPermissionSetExtensionProperty(string name)
	{
		PropertyTypeInfo value = null;
		PermissionSetExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	private static ImmutableDictionary<string, PropertyTypeInfo> GetEntitlementProperties()
	{
		PooledDictionary<string, PropertyTypeInfo> instance = PooledDictionary<string, PropertyTypeInfo>.GetInstance();
		try
		{
			ParseFunc parseFunc = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			IList<EnumPropertyMemberInfo> options = new EnumPropertyMemberInfo[10]
			{
				new EnumPropertyMemberInfo("PerUserServicePlan"),
				new EnumPropertyMemberInfo("FlatRateServicePlan"),
				new EnumPropertyMemberInfo("Role"),
				new EnumPropertyMemberInfo("ConcurrentUserServicePlan"),
				new EnumPropertyMemberInfo("Application"),
				new EnumPropertyMemberInfo("ApplicationScope"),
				new EnumPropertyMemberInfo("PerUserOfferPlan", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("10.1")),
				new EnumPropertyMemberInfo("Implicit"),
				new EnumPropertyMemberInfo("Unlicensed", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("10.2")),
				new EnumPropertyMemberInfo("Group", null, SymbolCompilationScope.Cloud, VersionCompatibility.Parse("11.1"))
			};
			VersionCompatibility compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("TYPE", new EnumPropertyTypeInfo(PropertyKind.Type, "Type", "Enum", "EnumLiteral", parseFunc, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "PerUserServicePlan", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: true, null, "Entitlement", compatibility));
			ParseFunc parseFunc2 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties = ImmutableArray.Create(new DependentProperty("Type(PerUserServicePlan,FlatRateServicePlan,Role,ConcurrentUserServicePlan,Application,ApplicationScope,PerUserOfferPlan,Group)"));
			Func<string, string, PropertyValueSyntax, CompilerFeatures, DiagnosticBag, bool> validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ID", new PropertyTypeInfo(PropertyKind.Id, "Id", "String", "StringLiteral", parseFunc2, dependentProperties, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "Entitlement", compatibility));
			ParseFunc parseFunc3 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseEnumLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties2 = ImmutableArray.Create(new DependentProperty("Type(Role)"));
			options = new EnumPropertyMemberInfo[2]
			{
				new EnumPropertyMemberInfo("Local"),
				new EnumPropertyMemberInfo("Delegated")
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("ROLETYPE", new EnumPropertyTypeInfo(PropertyKind.RoleType, "RoleType", "Enum", "EnumLiteral", parseFunc3, dependentProperties2, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, options, "Local", isObsolete: false, generateMetadata: true, null, null, null, null, null, null, emitAsAttribute: true, prependObjectName: true, isRequired: false, null, "Entitlement", compatibility, null, null, emitDefaultValue: false));
			ParseFunc parseFunc4 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseStringLiteralPropertyValue(pti);
			};
			ImmutableArray<DependentProperty>? dependentProperties3 = ImmutableArray.Create(new DependentProperty("Type(ConcurrentUserServicePlan)"));
			validator = EmptyPropertyValueValidator.ValidatePropertyValue;
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("GROUPNAME", new PropertyTypeInfo(PropertyKind.GroupName, "GroupName", "String", "StringLiteral", parseFunc4, dependentProperties3, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: true, emitDefaultValue: true, null, null, validator, null, null, null, emitAsAttribute: true, null, prependObjectName: false, isRequired: true, null, "Entitlement", compatibility));
			ParseFunc parseFunc5 = delegate(ObjectParser p, PropertyTypeInfo pti, ref InternalSyntaxToken equals)
			{
				return p.ParseCommaSeparatedObjectNameReferencesPropertyValue(pti, ref equals);
			};
			compatibility = VersionCompatibility.Parse("7.0");
			instance.Add("OBJECTENTITLEMENTS", new PropertyTypeInfo(PropertyKind.ObjectEntitlements, "ObjectEntitlements", "String", "CommaSeparatedObjectNameReferences", parseFunc5, null, null, null, null, dependentDeclaringApplicationObjectPropertiesWarning: false, null, generateMetadata: false, emitDefaultValue: true, null, null, null, null, null, null, emitAsAttribute: true, "PermissionSet", prependObjectName: false, isRequired: false, null, "Entitlement", compatibility, null, null, defaultValueFromDeclaringApplicationObject: false, SymbolKind.PermissionSet));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static PropertyTypeInfo LookupEntitlementProperty(string name)
	{
		PropertyTypeInfo value = null;
		EntitlementProperties.TryGetValue(name.ToUpperInvariant(), out value);
		return value;
	}

	internal ObjectParser(Lexer lexer, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, LexerMode.Object, oldTree, changes, cancellationToken)
	{
	}

	internal InternalCompilationUnitSyntax ParseObjects()
	{
		InternalSyntaxListBuilder<InternalObjectSyntax> objects = base.Pool.Allocate<InternalObjectSyntax>();
		try
		{
			InternalSyntaxListBuilder initialBadNodes = null;
			InternalNamespaceDeclarationSyntax internalNamespaceDeclarationSyntax = null;
			InternalSyntaxList<InternalUsingDirectiveSyntax> internalSyntaxList = default(InternalSyntaxList<InternalUsingDirectiveSyntax>);
			if (base.CurrentToken.ContextualKind == SyntaxKind.NamespaceKeyword)
			{
				internalNamespaceDeclarationSyntax = ParseNamespaceDeclaration();
			}
			while (base.CurrentToken.ContextualKind == SyntaxKind.UsingKeyword && !base.IsEndOfFile)
			{
				internalSyntaxList = ParseUsingDirectives(internalNamespaceDeclarationSyntax != null);
			}
			while (!base.IsEndOfFile)
			{
				InternalObjectSyntax internalObjectSyntax = null;
				switch (base.CurrentToken.ContextualKind)
				{
				case SyntaxKind.CodeunitKeyword:
					internalObjectSyntax = ParseCodeunit();
					break;
				case SyntaxKind.TableKeyword:
					internalObjectSyntax = ParseTable();
					break;
				case SyntaxKind.TableExtensionKeyword:
					internalObjectSyntax = ParseTableExtension();
					break;
				case SyntaxKind.PageKeyword:
					internalObjectSyntax = ParsePage();
					break;
				case SyntaxKind.PageExtensionKeyword:
					internalObjectSyntax = ParsePageExtension();
					break;
				case SyntaxKind.ReportKeyword:
					internalObjectSyntax = ParseReport();
					break;
				case SyntaxKind.ReportExtensionKeyword:
					internalObjectSyntax = ParseReportExtension();
					break;
				case SyntaxKind.ProfileKeyword:
					internalObjectSyntax = ParseProfile();
					break;
				case SyntaxKind.ProfileExtensionKeyword:
					internalObjectSyntax = ParseProfileExtension();
					break;
				case SyntaxKind.PageCustomizationKeyword:
					internalObjectSyntax = ParsePageCustomization();
					break;
				case SyntaxKind.XmlPortKeyword:
					internalObjectSyntax = ParseXmlPort();
					break;
				case SyntaxKind.QueryKeyword:
					internalObjectSyntax = ParseQuery();
					break;
				case SyntaxKind.ControlAddInKeyword:
					internalObjectSyntax = ParseControlAddIn();
					break;
				case SyntaxKind.DotNetKeyword:
					internalObjectSyntax = ParseDotNet();
					break;
				case SyntaxKind.EnumKeyword:
					internalObjectSyntax = ParseEnum();
					break;
				case SyntaxKind.EnumExtensionKeyword:
					internalObjectSyntax = ParseEnumExtension();
					break;
				case SyntaxKind.InterfaceKeyword:
					internalObjectSyntax = ParseInterface();
					break;
				case SyntaxKind.PermissionSetKeyword:
					internalObjectSyntax = ParsePermissionSet();
					break;
				case SyntaxKind.PermissionSetExtensionKeyword:
					internalObjectSyntax = ParsePermissionSetExtension();
					break;
				case SyntaxKind.EntitlementKeyword:
					internalObjectSyntax = ParseEntitlement();
					break;
				default:
					SkipBadApplicationObjectTokens(ref objects, ref initialBadNodes);
					if (objects.Count > 0)
					{
						objects[objects.Count - 1] = AddTrailingSkippedSyntax(objects[objects.Count - 1], InternalSyntaxFactory.SkippedTokensTrivia(initialBadNodes.ToList()));
						initialBadNodes = null;
					}
					else if (internalSyntaxList.Count > 0)
					{
						InternalSyntaxListBuilder<InternalUsingDirectiveSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalUsingDirectiveSyntax>();
						try
						{
							internalSyntaxListBuilder.AddRange(internalSyntaxList);
							internalSyntaxListBuilder[internalSyntaxListBuilder.Count - 1] = AddTrailingSkippedSyntax(internalSyntaxListBuilder[internalSyntaxListBuilder.Count - 1], InternalSyntaxFactory.SkippedTokensTrivia(initialBadNodes.ToList()));
							internalSyntaxList = internalSyntaxListBuilder.ToList();
						}
						finally
						{
							base.Pool.Free(internalSyntaxListBuilder);
						}
						initialBadNodes = null;
					}
					else if (internalNamespaceDeclarationSyntax != null)
					{
						internalNamespaceDeclarationSyntax = AddTrailingSkippedSyntax(internalNamespaceDeclarationSyntax, InternalSyntaxFactory.SkippedTokensTrivia(initialBadNodes.ToList()));
						initialBadNodes = null;
					}
					break;
				}
				if (internalObjectSyntax != null)
				{
					if (initialBadNodes != null)
					{
						internalObjectSyntax = AddLeadingSkippedSyntax(internalObjectSyntax, InternalSyntaxFactory.SkippedTokensTrivia(initialBadNodes.ToList()));
						initialBadNodes = null;
					}
					InternalSyntaxNode? node = internalObjectSyntax.Members.Node;
					if (node != null && node.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
					{
						internalObjectSyntax = internalObjectSyntax.WithAdditionalAnnotationsInternal(ExternalBusinessEventAnnotation);
					}
					internalObjectSyntax = AddMovedSymbolsAnnotationsIfApplicable(internalObjectSyntax);
					objects.Add(internalObjectSyntax);
				}
			}
			InternalCompilationUnitSyntax internalCompilationUnitSyntax = InternalSyntaxFactory.CompilationUnit(internalNamespaceDeclarationSyntax, internalSyntaxList, objects, (base.CurrentToken.Kind == SyntaxKind.EndOfFileToken) ? base.CurrentToken : null);
			if (initialBadNodes != null)
			{
				internalCompilationUnitSyntax = AddLeadingSkippedSyntax(internalCompilationUnitSyntax, InternalSyntaxFactory.SkippedTokensTrivia(initialBadNodes.ToList()));
			}
			return internalCompilationUnitSyntax;
		}
		finally
		{
			base.Pool.Free(objects);
		}
	}

	private InternalObjectSyntax AddMovedSymbolsAnnotationsIfApplicable(InternalObjectSyntax obj)
	{
		if (obj.Kind == SyntaxKind.TableObject || obj.Kind == SyntaxKind.TableExtensionObject)
		{
			bool flag = false;
			if (!(obj is InternalTableSyntax internalTableSyntax))
			{
				if (obj is InternalTableExtensionSyntax internalTableExtensionSyntax)
				{
					InternalFieldExtensionListSyntax fields = internalTableExtensionSyntax.Fields;
					flag = fields != null && (fields.Fields.Node?.HasAnnotations(AnnotationKind.MovedSymbols)).GetValueOrDefault();
				}
				else
				{
					DebugAssertHelper.Fail("We should never reach this code.");
				}
			}
			else
			{
				InternalFieldListSyntax fields2 = internalTableSyntax.Fields;
				flag = fields2 != null && (fields2.Fields.Node?.HasAnnotations(AnnotationKind.MovedSymbols)).GetValueOrDefault();
			}
			if (flag)
			{
				obj = obj.WithAdditionalAnnotationsInternal(MovedSymbolsAnnotation);
			}
		}
		return obj;
	}

	private InternalNamespaceDeclarationSyntax ParseNamespaceDeclaration()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.NamespaceDeclaration)
		{
			return (InternalNamespaceDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken namespaceToken = EatKeywordToken(SyntaxKind.NamespaceKeyword);
		InternalNameSyntax name = ParseQualifiedName(disallowQualified: false, mustBeValidClsIdentifier: true);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return CheckFeatureAvailability(InternalSyntaxFactory.NamespaceDeclaration(namespaceToken, name, semicolonToken), Feature.Namespaces);
	}

	private InternalSyntaxList<InternalUsingDirectiveSyntax> ParseUsingDirectives(bool hasNamespaceDeclaration)
	{
		InternalSyntaxListBuilder<InternalUsingDirectiveSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalUsingDirectiveSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.ContextualKind == SyntaxKind.UsingKeyword)
			{
				InternalUsingDirectiveSyntax node = ParseUsingDirective();
				if (!hasNamespaceDeclaration)
				{
					node = AddError(node, ErrorCode.WRN_UsingWithoutNamespace);
				}
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalUsingDirectiveSyntax ParseUsingDirective()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.UsingDirective)
		{
			return (InternalUsingDirectiveSyntax)EatNode();
		}
		InternalSyntaxToken usingToken = EatKeywordToken(SyntaxKind.UsingKeyword);
		InternalNameSyntax name = ParseQualifiedName(disallowQualified: false, mustBeValidClsIdentifier: true);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return CheckFeatureAvailability(InternalSyntaxFactory.UsingDirective(usingToken, name, semicolonToken), Feature.Namespaces);
	}

	private InternalSyntaxToken ParseCloseBraceToken(InternalSyntaxToken openBraceToken)
	{
		if (openBraceToken.IsMissing)
		{
			InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			return WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, base.CurrentToken.Kind));
		}
		return EatToken(SyntaxKind.CloseBraceToken);
	}

	private InternalPropertyListSyntax ParsePropertyList(Func<string, PropertyTypeInfo> lookupProperty, bool asExtension = false, bool deferErrorChecking = false, bool asCustomization = false)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PropertyList)
		{
			return (InternalPropertyListSyntax)EatNode();
		}
		bool flag = false;
		bool flag2 = false;
		InternalSyntaxListBuilder<InternalPropertySyntaxOrEmpty> internalSyntaxListBuilder = base.Pool.Allocate<InternalPropertySyntaxOrEmpty>();
		try
		{
			if (lookupProperty != null)
			{
				while (!base.IsEndOfFile && (base.CurrentToken.ContextualKind == SyntaxKind.IdentifierToken || base.CurrentToken.Kind == SyntaxKind.SemicolonToken || lookupProperty(base.CurrentToken.Text.ToUpperInvariant()) != null))
				{
					if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.Property)
					{
						InternalPropertySyntax node = (InternalPropertySyntax)EatNode();
						internalSyntaxListBuilder.Add(node);
						continue;
					}
					InternalPropertySyntaxOrEmpty node2;
					if (base.CurrentToken.Kind == SyntaxKind.SemicolonToken)
					{
						node2 = InternalSyntaxFactory.EmptyProperty(EatToken(SyntaxKind.SemicolonToken));
					}
					else
					{
						InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.IdentifierToken);
						InternalPropertyNameSyntax internalPropertyNameSyntax = InternalSyntaxFactory.PropertyName(internalSyntaxToken);
						string text = internalPropertyNameSyntax.Identifier.Text;
						if (SyntaxFacts.PromotedActionPropertyNames.Contains(text) || SyntaxFacts.PromotedActionCategoriesPropertyNames.Contains(text))
						{
							flag = true;
						}
						PropertyTypeInfo propertyTypeInfo = lookupProperty(internalSyntaxToken.Text.ToUpperInvariant());
						if (!deferErrorChecking)
						{
							if (propertyTypeInfo == null)
							{
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, ErrorCode.ERR_PropertyInfoNotAvailable, text);
							}
							else if ((asExtension || asCustomization) && propertyTypeInfo.Modification == ModificationKind.None)
							{
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, ErrorCode.ERR_NonCustomizableProperty, text);
							}
							else if (asExtension && !propertyTypeInfo.Modification.HasFlag(ModificationKind.Extensions))
							{
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, ErrorCode.ERR_NonCustomizableProperty, text);
							}
							else if (asCustomization && !propertyTypeInfo.Modification.HasFlag(ModificationKind.Customizations))
							{
								ErrorCode warningErrorErrorCode = VersionChecker.GetWarningErrorErrorCode(ErrorCode.WRN_ERR_NonCustomizableProperty, base.Options.RuntimeVersion);
								internalPropertyNameSyntax = AddError(internalPropertyNameSyntax, warningErrorErrorCode, text);
							}
						}
						InternalSyntaxToken equals = null;
						InternalPropertyValueSyntax previousToken = null;
						if (base.CurrentToken.Kind == SyntaxKind.EqualsToken && propertyTypeInfo != null)
						{
							previousToken = ParseInLexerMode(LexerMode.Property, delegate
							{
								equals = EatToken(SyntaxKind.EqualsToken);
								return propertyTypeInfo.ParseValue(this, ref equals);
							});
							DebugAssertHelper.Assert(previousToken != null, "value should never be null here. If it is we have a bug.");
							if (previousToken != null && previousToken.ContainsDiagnostics)
							{
								SkipBadPropertyValue(ref previousToken);
							}
						}
						else
						{
							equals = EatToken(SyntaxKind.EqualsToken);
							SkipBadPropertyValue(ref equals);
						}
						node2 = InternalSyntaxFactory.Property(internalPropertyNameSyntax, equals, previousToken, EatToken(SyntaxKind.SemicolonToken));
						if (!IsObsoleteMovedProperty(propertyTypeInfo, previousToken))
						{
							PropertyTypeInfo propertyTypeInfo2 = propertyTypeInfo;
							if (propertyTypeInfo2 == null || propertyTypeInfo2.Kind != PropertyKind.MovedTo)
							{
								PropertyTypeInfo propertyTypeInfo3 = propertyTypeInfo;
								if (propertyTypeInfo3 == null || propertyTypeInfo3.Kind != PropertyKind.MovedFrom)
								{
									goto IL_02bb;
								}
							}
						}
						flag2 = true;
					}
					goto IL_02bb;
					IL_02bb:
					internalSyntaxListBuilder.Add(node2);
				}
			}
			InternalPropertyListSyntax internalPropertyListSyntax = InternalSyntaxFactory.PropertyList(internalSyntaxListBuilder.ToList());
			if (flag2)
			{
				internalPropertyListSyntax = internalPropertyListSyntax.WithAdditionalAnnotationsInternal(MovedSymbolsAnnotation);
			}
			return flag ? AnnotateWithActionV1(internalPropertyListSyntax) : internalPropertyListSyntax;
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private static bool IsObsoleteMovedProperty(PropertyTypeInfo? propertyTypeInfo, InternalPropertyValueSyntax? value)
	{
		string text = (value as InternalEnumPropertyValueSyntax)?.Value?.Identifier?.Text;
		if (text == null)
		{
			return false;
		}
		if (propertyTypeInfo != null && propertyTypeInfo.Kind == PropertyKind.ObsoleteState)
		{
			if (!(text == "Moved"))
			{
				return text == "PendingMove";
			}
			return true;
		}
		return false;
	}

	private InternalSyntaxList<InternalTriggerDeclarationSyntax> ParseTriggerList(Func<string, TriggerTypeInfo> getTriggerInfo = null)
	{
		InternalSyntaxListBuilder<InternalTriggerDeclarationSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalTriggerDeclarationSyntax>();
		try
		{
			while (base.CurrentToken.ContextualKind == SyntaxKind.TriggerKeyword)
			{
				internalSyntaxListBuilder.Add(ParseTrigger(getTriggerInfo, default(InternalSyntaxList<InternalMemberAttributeSyntax>)));
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalSyntaxList<InternalMemberSyntax> ParseMemberList(Func<string, TriggerTypeInfo>? getTriggerInfo = null)
	{
		InternalSyntaxListBuilder<InternalMemberSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalMemberSyntax>();
		bool flag = false;
		try
		{
			InternalSyntaxList<InternalMemberAttributeSyntax> attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
			while (true)
			{
				switch (base.CurrentToken.ContextualKind)
				{
				case SyntaxKind.VarKeyword:
					internalSyntaxListBuilder.Add(ParseGlobalVarSection(ref attributeList, null));
					break;
				case SyntaxKind.OpenBracketToken:
					attributeList = ParseInLexerMode(LexerMode.Code, delegate
					{
						InternalSyntaxListBuilder<InternalMemberAttributeSyntax> internalSyntaxListBuilder2 = base.Pool.Allocate<InternalMemberAttributeSyntax>();
						try
						{
							bool flag2 = false;
							while (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
							{
								InternalMemberAttributeSyntax internalMemberAttributeSyntax = ParseAttribute();
								if (internalMemberAttributeSyntax.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
								{
									flag2 = true;
								}
								internalSyntaxListBuilder2.Add(internalMemberAttributeSyntax);
							}
							InternalSyntaxList<InternalMemberAttributeSyntax> internalSyntaxList2 = internalSyntaxListBuilder2.ToList();
							if (flag2)
							{
								internalSyntaxList2 = AnnotateWithExternalBusinessEventAnnotation(internalSyntaxList2);
							}
							return internalSyntaxList2;
						}
						finally
						{
							base.Pool.Free(internalSyntaxListBuilder2);
						}
					});
					break;
				case SyntaxKind.ProtectedKeyword:
					internalSyntaxListBuilder.Add(ParseMethodOrGlobalVarSection(ref attributeList));
					break;
				case SyntaxKind.ProcedureKeyword:
				case SyntaxKind.LocalKeyword:
				case SyntaxKind.InternalKeyword:
				{
					InternalMethodDeclarationSyntax internalMethodDeclarationSyntax = ParseMethod(attributeList);
					if (internalMethodDeclarationSyntax.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
					{
						flag = true;
					}
					internalSyntaxListBuilder.Add(internalMethodDeclarationSyntax);
					attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
					break;
				}
				case SyntaxKind.TriggerKeyword:
					internalSyntaxListBuilder.Add(ParseTriggerOrEventTrigger(getTriggerInfo, attributeList));
					attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
					break;
				case SyntaxKind.EventKeyword:
					internalSyntaxListBuilder.Add(ParseEvent(attributeList));
					attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
					break;
				default:
				{
					if (attributeList.Count > 0)
					{
						internalSyntaxListBuilder.Add(ParseMethod(attributeList));
					}
					InternalSyntaxList<InternalMemberSyntax> internalSyntaxList = internalSyntaxListBuilder.ToList();
					if (flag)
					{
						internalSyntaxList = AnnotateWithExternalBusinessEventAnnotation(internalSyntaxList);
					}
					return internalSyntaxList;
				}
				}
			}
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalVarSectionSyntax ParseVarSection(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken(SyntaxKind.VarKeyword);
		if (attributeList.Count > 0)
		{
			internalSyntaxToken = AddLeadingSkippedSyntax(internalSyntaxToken, attributeList.Node);
			if (!attributeList.Node.ContainsDiagnostics)
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_AttributeContext);
			}
			attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
		}
		return InternalSyntaxFactory.VarSection(internalSyntaxToken, ParseVariableList(ref attributeList));
	}

	private InternalGlobalVarSectionSyntax ParseGlobalVarSection(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken accessModifier)
	{
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken(SyntaxKind.VarKeyword);
		if (attributeList.Count > 0)
		{
			internalSyntaxToken = AddLeadingSkippedSyntax(internalSyntaxToken, attributeList.Node);
			if (!attributeList.Node.ContainsDiagnostics)
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_AttributeContext);
			}
			attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
		}
		return InternalSyntaxFactory.GlobalVarSection(accessModifier, internalSyntaxToken, ParseVariableList(ref attributeList));
	}

	private InternalSyntaxList<InternalVariableDeclarationBaseSyntax> ParseVariableList(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		InternalSyntaxListBuilder<InternalVariableDeclarationBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalVariableDeclarationBaseSyntax>();
		try
		{
			while (base.CurrentToken.IsAllowedVariableName() || base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				if (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					attributeList = ParseInLexerMode(LexerMode.Code, delegate
					{
						InternalSyntaxListBuilder<InternalMemberAttributeSyntax> internalSyntaxListBuilder2 = base.Pool.Allocate<InternalMemberAttributeSyntax>();
						try
						{
							bool flag = false;
							while (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
							{
								InternalMemberAttributeSyntax internalMemberAttributeSyntax = ParseAttribute();
								if (internalMemberAttributeSyntax.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
								{
									flag = true;
								}
								internalSyntaxListBuilder2.Add(internalMemberAttributeSyntax);
							}
							InternalSyntaxList<InternalMemberAttributeSyntax> internalSyntaxList = internalSyntaxListBuilder2.ToList();
							if (flag)
							{
								internalSyntaxList = AnnotateWithExternalBusinessEventAnnotation(internalSyntaxList);
							}
							return internalSyntaxList;
						}
						finally
						{
							base.Pool.Free(internalSyntaxListBuilder2);
						}
					});
				}
				if (base.CurrentToken.IsAllowedVariableName())
				{
					InternalVariableDeclarationBaseSyntax previousNode = ParseVariable(attributeList);
					SkipBadVariableDefinitionTokens(ref previousNode);
					internalSyntaxListBuilder.Add(previousNode);
					attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalVariableDeclarationBaseSyntax ParseVariable(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.VariableDeclaration || base.CurrentNodeKind == SyntaxKind.VariableListDeclaration))
		{
			return (InternalVariableDeclarationBaseSyntax)EatNode();
		}
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken colonToken;
		InternalTypeReferenceBaseSyntax type;
		if (!internalIdentifierNameSyntax.IsMissing && base.CurrentToken.Kind == SyntaxKind.CommaToken)
		{
			InternalSeparatedSyntaxListBuilder<InternalVariableDeclarationNameSyntax> internalSeparatedSyntaxListBuilder = base.Pool.AllocateSeparated<InternalVariableDeclarationNameSyntax>();
			InternalSeparatedSyntaxList<InternalVariableDeclarationNameSyntax> variableNames;
			try
			{
				InternalVariableDeclarationNameSyntax node = InternalSyntaxFactory.VariableDeclarationName(internalIdentifierNameSyntax);
				internalSeparatedSyntaxListBuilder.Add(node);
				while (base.CurrentToken.IsKind(SyntaxKind.CommaToken))
				{
					internalSeparatedSyntaxListBuilder.AddSeparator(EatToken());
					internalSeparatedSyntaxListBuilder.Add(InternalSyntaxFactory.VariableDeclarationName(ParseIdentifierName()));
				}
			}
			finally
			{
				variableNames = internalSeparatedSyntaxListBuilder.ToList();
			}
			colonToken = EatToken(SyntaxKind.ColonToken);
			type = ParseType();
			type = CheckTypeReferenceSyntax(type);
			InternalVariableListDeclarationSyntax node2 = InternalSyntaxFactory.VariableListDeclaration(attributeList, variableNames, colonToken, type, EatToken(SyntaxKind.SemicolonToken));
			return CheckFeatureAvailability(node2, Feature.VariableListDeclarations);
		}
		colonToken = EatToken(SyntaxKind.ColonToken);
		type = ParseType();
		type = CheckTypeReferenceSyntax(type);
		return InternalSyntaxFactory.VariableDeclaration(attributeList, internalIdentifierNameSyntax, colonToken, type, EatToken(SyntaxKind.SemicolonToken));
	}

	private InternalTypeReferenceBaseSyntax ParseType()
	{
		InternalArraySyntax array = ParseArray();
		InternalDataTypeSyntax internalDataTypeSyntax = ParseDatatype();
		switch ((internalDataTypeSyntax.TypeName != null) ? NavTypeExtensions.GetNavTypeKind(internalDataTypeSyntax.TypeName.ValueText, throwIfUnknown: false) : NavTypeKind.None)
		{
		case NavTypeKind.Record:
		{
			InternalSyntaxToken temporary = (base.CurrentToken.IsKeywordKind(SyntaxKind.TemporaryKeyword) ? EatKeywordToken() : null);
			return InternalSyntaxFactory.RecordTypeReference(array, internalDataTypeSyntax, temporary);
		}
		case NavTypeKind.DotNet:
			return InternalSyntaxFactory.DotNetTypeReference(array, internalDataTypeSyntax);
		default:
			return InternalSyntaxFactory.SimpleTypeReference(array, internalDataTypeSyntax);
		}
	}

	private InternalArraySyntax ParseArray()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ArrayKeyword))
		{
			return null;
		}
		InternalSyntaxToken arrayKeyword = EatKeywordToken();
		InternalBracketedDimensionListSyntax internalBracketedDimensionListSyntax = ((base.CurrentToken.Kind == SyntaxKind.OpenBracketToken) ? ParseBracketedDimensionList() : InternalSyntaxFactory.BracketedDimensionList(EatToken(SyntaxKind.OpenBracketToken), default(InternalSeparatedSyntaxList<InternalDimensionSyntax>), EatToken(SyntaxKind.CloseBracketToken)));
		if (internalBracketedDimensionListSyntax.Dimensions.Count == 0)
		{
			internalBracketedDimensionListSyntax = AddError(internalBracketedDimensionListSyntax, ErrorCode.ERR_ArrayMustHaveAtLeastOneDimension);
		}
		else if (internalBracketedDimensionListSyntax.Dimensions.Count > 10)
		{
			internalBracketedDimensionListSyntax = AddError(internalBracketedDimensionListSyntax, ErrorCode.ERR_ArrayTooManyDimensions, 10);
		}
		return InternalSyntaxFactory.Array(arrayKeyword, internalBracketedDimensionListSyntax, EatKeywordToken(SyntaxKind.OfKeyword));
	}

	private InternalBracketedDimensionListSyntax ParseBracketedDimensionList()
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions = ParseSeparatedList(ref startToken, SyntaxKind.Int32LiteralToken, (InternalSyntaxToken token) => token.Kind == SyntaxKind.Int32LiteralToken, SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, delegate(ref InternalSyntaxToken openBracket, InternalSeparatedSyntaxListBuilder<InternalDimensionSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (ParserBase p) => p.CurrentToken.Kind != expected, (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
		}, () => (base.CurrentToken.Kind != SyntaxKind.CloseBracketToken) ? InternalSyntaxFactory.Dimension(EatToken(SyntaxKind.Int32LiteralToken)) : InternalSyntaxFactory.Dimension(CreateMissingToken(SyntaxKind.Int32LiteralToken, base.CurrentToken.Kind, reportError: true)));
		InternalSyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
		return InternalSyntaxFactory.BracketedDimensionList(startToken, dimensions, closeBracketToken);
	}

	private PostSkipAction SkipBadArrayDimensionSpecifierTokens(ref InternalSyntaxToken openBracket, InternalSeparatedSyntaxListBuilder<InternalDimensionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openBracket, list, (ParserBase p) => p.CurrentToken.Kind != expected, (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private InternalDimensionSyntax ParseDimension()
	{
		if (base.CurrentToken.Kind != SyntaxKind.CloseBracketToken)
		{
			return InternalSyntaxFactory.Dimension(EatToken(SyntaxKind.Int32LiteralToken));
		}
		return InternalSyntaxFactory.Dimension(CreateMissingToken(SyntaxKind.Int32LiteralToken, base.CurrentToken.Kind, reportError: true));
	}

	private InternalDataTypeSyntax ParseDatatype()
	{
		// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
		if (SyntaxFacts.IsOptionType(base.CurrentToken))
		{
			return ParseOptionDataType();
		}
		InternalSyntaxToken internalSyntaxToken = (base.CurrentToken.ContextualKind.IsAllowedVariableType() ? EatTokenOrKeyword() : CreateMissingToken(SyntaxKind.IdentifierToken, base.CurrentToken.ContextualKind, reportError: true));
		if (internalSyntaxToken.IsMissing)
		{
			return InternalSyntaxFactory.SimpleNamedDataType(internalSyntaxToken);
		}
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(internalSyntaxToken.ValueText, throwIfUnknown: false);
		switch (navTypeKind)
		{
		case NavTypeKind.TextConst:
			return ParseTextConstDataType(internalSyntaxToken);
		case NavTypeKind.Label:
			return ParseLabelDataType(internalSyntaxToken);
		case NavTypeKind.Enum:
			return ParseEnumDataType(internalSyntaxToken);
		case NavTypeKind.Interface:
			internalSyntaxToken = CheckFeatureAvailability(internalSyntaxToken, Feature.Interfaces);
			break;
		}
		if (navTypeKind.HasLength())
		{
			InternalSyntaxToken openBracketToken = null;
			InternalSyntaxToken length = null;
			InternalSyntaxToken closeBracketToken = null;
			if (navTypeKind.LengthRequired() || base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
				length = EatToken(SyntaxKind.Int32LiteralToken);
				closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			}
			return InternalSyntaxFactory.LengthDataType(internalSyntaxToken, openBracketToken, length, closeBracketToken);
		}
		if (navTypeKind.HasSubType())
		{
			InternalObjectNameOrIdSyntax subtype = ParseObjectReferenceSyntax();
			return InternalSyntaxFactory.SubtypedDataType(internalSyntaxToken, subtype);
		}
		if (navTypeKind.IsGenericType())
		{
			InternalSyntaxToken ofKeyword = EatKeywordToken(SyntaxKind.OfKeyword);
			InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenBracketToken);
			InternalSeparatedSyntaxList<InternalDataTypeSyntax> typeArguments = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, base.SkipBadCommaSeparatedToken, ParseDatatype);
			InternalSyntaxToken closeBracketToken2 = EatToken(SyntaxKind.CloseBracketToken);
			return InternalSyntaxFactory.GenericNamedDataType(internalSyntaxToken, ofKeyword, startToken, typeArguments, closeBracketToken2);
		}
		return InternalSyntaxFactory.SimpleNamedDataType(internalSyntaxToken);
	}

	private InternalDataTypeSyntax ParseOptionDataType(bool skipParsingOptionValues = false)
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.IdentifierToken);
		DebugAssertHelper.Assert(SyntaxFacts.IsOptionType(startToken));
		InternalOptionValuesSyntax optionValues = null;
		if (!skipParsingOptionValues)
		{
			optionValues = ParseOptionMembersSyntax(ref startToken);
		}
		return InternalSyntaxFactory.OptionDataType(startToken, optionValues);
	}

	private InternalOptionValuesSyntax ParseOptionMembersSyntax(ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.OptionValues(ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.ContextualKind.IsTokenIdentifier() || token.Kind == SyntaxKind.CommaToken, SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, base.SkipBadCommaSeparatedToken, base.ParseIdentifierNameOrEmptySyntax, shouldParseMemberIfLastElementIsSeparator: true));
	}

	private InternalDataTypeSyntax ParseTextConstDataType(InternalSyntaxToken textConstType)
	{
		InternalCommaSeparatedIdentifierEqualsStringListSyntax multilanguage = ParseCommaSeparatedIdentifierEqualsStringList(ref textConstType);
		return InternalSyntaxFactory.TextConstDataType(textConstType, multilanguage);
	}

	private InternalDataTypeSyntax ParseLabelDataType(InternalSyntaxToken labelType)
	{
		return InternalSyntaxFactory.LabelDataType(labelType, ParseLabel());
	}

	private InternalDataTypeSyntax ParseEnumDataType(InternalSyntaxToken enumType)
	{
		InternalEnumDataTypeSyntax node = InternalSyntaxFactory.EnumDataType(enumType, ParseObjectNameReference());
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalLabelSyntax ParseLabel()
	{
		InternalStringLiteralValueSyntax labelText = ParseStringLiteralValue();
		if (base.CurrentToken.IsKind(SyntaxKind.CommaToken))
		{
			InternalSyntaxToken startToken = EatToken(SyntaxKind.CommaToken);
			InternalCommaSeparatedIdentifierEqualsLiteralListSyntax properties = ParseCommaSeparatedIdentifierEqualsLiteralList(ref startToken);
			return InternalSyntaxFactory.Label(labelText, startToken, properties);
		}
		return InternalSyntaxFactory.Label(labelText, null, null);
	}

	private InternalCommaSeparatedIdentifierEqualsLiteralListSyntax ParseCommaSeparatedIdentifierEqualsLiteralList(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsLiteralList(ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, base.SkipBadCommaSeparatedToken, delegate
		{
			if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsLiteral)
			{
				return (InternalIdentifierEqualsLiteralSyntax)EatNode();
			}
			InternalSyntaxToken identifier = ParseIdentifierToken();
			InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			InternalLiteralValueSyntax literal = ParseLiteralValue();
			return InternalSyntaxFactory.IdentifierEqualsLiteral(identifier, equalsToken, literal);
		}));
	}

	private InternalIdentifierEqualsStringSyntax ParseIdentifierEqualsString()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsString)
		{
			return (InternalIdentifierEqualsStringSyntax)EatNode();
		}
		InternalSyntaxToken identifier = ParseIdentifierToken();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalStringLiteralValueSyntax stringLiteral = ParseStringLiteralValue();
		return InternalSyntaxFactory.IdentifierEqualsString(identifier, equalsToken, stringLiteral);
	}

	private InternalIdentifierEqualsLiteralSyntax ParseIdentifierEqualsLiteral()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsLiteral)
		{
			return (InternalIdentifierEqualsLiteralSyntax)EatNode();
		}
		InternalSyntaxToken identifier = ParseIdentifierToken();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalLiteralValueSyntax literal = ParseLiteralValue();
		return InternalSyntaxFactory.IdentifierEqualsLiteral(identifier, equalsToken, literal);
	}

	private InternalTriggerDeclarationSyntax ParseTrigger(Func<string, TriggerTypeInfo> getTriggerInfo, InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.TriggerDeclaration)
		{
			return (InternalTriggerDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken triggerKeyword = EatKeywordToken(SyntaxKind.TriggerKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		return (InternalTriggerDeclarationSyntax)ParseTrigger(attributeList, triggerKeyword, name, getTriggerInfo);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseTriggerOrEventTrigger(Func<string, TriggerTypeInfo>? getTriggerInfo, InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.TriggerDeclaration || base.CurrentNodeKind == SyntaxKind.EventTriggerDeclaration) && CheckNodeAttributesPreserved(attributeList))
		{
			return (InternalMethodOrTriggerDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken triggerKeyword = EatKeywordToken(SyntaxKind.TriggerKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		if (base.CurrentToken.Kind == SyntaxKind.ColonColonToken)
		{
			return ParseEventTrigger(attributeList, triggerKeyword, name);
		}
		return ParseTrigger(attributeList, triggerKeyword, name, getTriggerInfo);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseEventTrigger(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name)
	{
		InternalSyntaxToken colonColonToken = EatToken(SyntaxKind.ColonColonToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalVarSectionSyntax variables = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true));
		return InternalSyntaxFactory.EventTriggerDeclaration(attributeList, triggerKeyword, name, colonColonToken, name2, parameterList, returnValue, semicolonToken, variables, body);
	}

	private InternalMethodOrTriggerDeclarationSyntax ParseTrigger(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken triggerKeyword, InternalIdentifierNameSyntax name, Func<string, TriggerTypeInfo>? getTriggerInfo)
	{
		TriggerTypeInfo triggerTypeInfo = getTriggerInfo?.Invoke(name.Identifier.ValueText);
		if (triggerTypeInfo == null && getTriggerInfo != null)
		{
			name = AddError(name, ErrorCode.ERR_UnknownTrigger, name.Identifier.ValueText);
		}
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalVarSectionSyntax variables = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true));
		InternalTriggerDeclarationSyntax previousNode = InternalSyntaxFactory.TriggerDeclaration(attributeList, triggerKeyword, name, parameterList, returnValue, semicolonToken, variables, body);
		if (triggerTypeInfo != null)
		{
			previousNode = previousNode.WithAnnotations(new SyntaxAnnotation[1] { triggerTypeInfo.TriggerAnnotation });
		}
		SkipBadMethodDefinitionTokens(ref previousNode);
		return previousNode;
	}

	private InternalMemberSyntax ParseMethodOrGlobalVarSection(ref InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.MethodDeclaration)
		{
			return (InternalMethodDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken accessModifier = EatKeywordToken(SyntaxKind.ProtectedKeyword);
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.VarKeyword))
		{
			return ParseGlobalVarSection(ref attributeList, accessModifier);
		}
		InternalMethodDeclarationSyntax result = ParseMethodCore(attributeList, accessModifier);
		attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
		return result;
	}

	private InternalMethodDeclarationSyntax ParseMethod(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.MethodDeclaration && CheckNodeAttributesPreserved(attributeList))
		{
			return (InternalMethodDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken accessModifier = (base.CurrentToken.IsKeywordKind(SyntaxKind.LocalKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword) ? EatKeywordToken() : null);
		return ParseMethodCore(attributeList, accessModifier);
	}

	private bool CheckNodeAttributesPreserved(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		return ((InternalMethodOrTriggerDeclarationSyntax)base.CurrentNode.Node).Attributes.Count == attributeList.Count;
	}

	private InternalMethodDeclarationSyntax ParseMethodCore(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList, InternalSyntaxToken? accessModifier)
	{
		if (accessModifier != null && accessModifier.IsKeywordKind(SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword))
		{
			accessModifier = CheckFeatureAvailability(accessModifier, Feature.AccessModifiers);
		}
		InternalSyntaxToken procedureKeyword = EatKeywordToken(SyntaxKind.ProcedureKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalReturnValueSyntax returnValue = ParseReturnValue();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalSyntaxNode internalSyntaxNode = ParseVarSectionAndSkipOrphanedAttributes();
		InternalBlockSyntax body = (base.CurrentToken.IsKeywordKind(SyntaxKind.BeginKeyword) ? ParseInLexerMode(LexerMode.Code, () => ParseBlock(semicolonRequired: true)) : null);
		InternalMethodDeclarationSyntax previousNode = InternalSyntaxFactory.MethodDeclaration(attributeList, accessModifier, procedureKeyword, name, parameterList, returnValue, semicolonToken, (InternalVarSectionSyntax)internalSyntaxNode, body);
		SkipBadMethodDefinitionTokens(ref previousNode);
		InternalSyntaxNode? node = attributeList.Node;
		if (node != null && node.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
		{
			previousNode = previousNode.WithAdditionalAnnotationsInternal(ExternalBusinessEventAnnotation);
		}
		return previousNode;
	}

	private InternalEventDeclarationSyntax ParseEvent(InternalSyntaxList<InternalMemberAttributeSyntax> attributeList)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.EventDeclaration)
		{
			return (InternalEventDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken eventKeyword = EatKeywordToken(SyntaxKind.EventKeyword);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalParameterListSyntax parameterList = ParseParameterList();
		InternalSyntaxToken semicolonToken = EatTokenIfKind(SyntaxKind.SemicolonToken);
		InternalEventDeclarationSyntax previousNode = InternalSyntaxFactory.EventDeclaration(attributeList, eventKeyword, name, parameterList, semicolonToken);
		SkipBadMethodDefinitionTokens(ref previousNode);
		return previousNode;
	}

	private InternalVarSectionSyntax? ParseVarSectionAndSkipOrphanedAttributes()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.VarSection)
		{
			return (InternalVarSectionSyntax)EatNode();
		}
		InternalSyntaxNode lastGoodNode = null;
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.VarKeyword))
		{
			InternalSyntaxList<InternalMemberAttributeSyntax> attributeList = default(InternalSyntaxList<InternalMemberAttributeSyntax>);
			lastGoodNode = ParseVarSection(ref attributeList);
			SkipOrphanedAttributes(ref lastGoodNode, attributeList);
		}
		return (InternalVarSectionSyntax)lastGoodNode;
	}

	private InternalParameterListSyntax ParseParameterList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ParameterList)
		{
			return (InternalParameterListSyntax)EatNode();
		}
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalParameterSyntax> parameters = ParseParameters(ref startToken);
		return InternalSyntaxFactory.ParameterList(startToken, parameters, EatToken(SyntaxKind.CloseParenToken));
	}

	private InternalSeparatedSyntaxList<InternalParameterSyntax> ParseParameters(ref InternalSyntaxToken startToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.IsPossibleParameter(), SyntaxKind.SemicolonToken, SyntaxKind.CloseParenToken, delegate(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalParameterSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.SemicolonToken && p.CurrentToken.Kind != SyntaxKind.SemicolonToken) || (expected != SyntaxKind.SemicolonToken && !base.CurrentToken.IsPossibleParameter()), (ParserBase p) => p.CurrentToken.IsPossibleMember() || p.IsTerminator() || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsKeywordKind(SyntaxKind.BeginKeyword), expected);
		}, delegate
		{
			InternalSyntaxToken varKeyword = (base.CurrentToken.IsKeywordKind(SyntaxKind.VarKeyword) ? EatKeywordToken() : null);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
			InternalTypeReferenceBaseSyntax type = ParseType();
			type = CheckTypeReferenceSyntax(type);
			return InternalSyntaxFactory.Parameter(varKeyword, name, colonToken, type);
		});
	}

	private InternalParameterSyntax ParseParameter()
	{
		InternalSyntaxToken varKeyword = (base.CurrentToken.IsKeywordKind(SyntaxKind.VarKeyword) ? EatKeywordToken() : null);
		InternalIdentifierNameSyntax name = ParseIdentifierName();
		InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		InternalTypeReferenceBaseSyntax type = ParseType();
		type = CheckTypeReferenceSyntax(type);
		return InternalSyntaxFactory.Parameter(varKeyword, name, colonToken, type);
	}

	private InternalReturnValueSyntax? ParseReturnValue()
	{
		if (!base.CurrentToken.IsAllowedVariableName() && base.CurrentToken.Kind != SyntaxKind.ColonToken)
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReturnValue)
		{
			return (InternalReturnValueSyntax)EatNode();
		}
		InternalIdentifierNameSyntax name = ((base.CurrentToken.Kind == SyntaxKind.ColonToken) ? null : ParseIdentifierName());
		InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
		return InternalSyntaxFactory.ReturnValue(name, colonToken, ParseType());
	}

	private PostSkipAction SkipBadParameterTokens(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalParameterSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.SemicolonToken && p.CurrentToken.Kind != SyntaxKind.SemicolonToken) || (expected != SyntaxKind.SemicolonToken && !base.CurrentToken.IsPossibleParameter()), (ParserBase p) => p.CurrentToken.IsPossibleMember() || p.IsTerminator() || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsKeywordKind(SyntaxKind.BeginKeyword), expected);
	}

	private void SkipBadApplicationObjectTokens(ref InternalSyntaxListBuilder<InternalObjectSyntax> objects, ref InternalSyntaxListBuilder initialBadNodes)
	{
		InternalSyntaxListBuilder<InternalSyntaxToken> internalSyntaxListBuilder = base.Pool.Allocate<InternalSyntaxToken>();
		try
		{
			while (!base.IsEndOfFile && !base.CurrentToken.ContextualKind.IsObjectKeyword())
			{
				InternalSyntaxToken node = EatToken();
				if (internalSyntaxListBuilder.Count == 0)
				{
					node = AddError(node, ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedApplicationObjectsString);
				}
				internalSyntaxListBuilder.Add(node);
			}
			if (objects.Count > 0)
			{
				objects[objects.Count - 1] = AddTrailingSkippedSyntax(objects[objects.Count - 1], internalSyntaxListBuilder.ToListNode());
				return;
			}
			initialBadNodes = new InternalSyntaxListBuilder(internalSyntaxListBuilder.Count);
			initialBadNodes.AddRange(internalSyntaxListBuilder.ToList());
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private void SkipBadMethodDefinitionTokens<T>(ref T previousNode) where T : InternalMemberSyntax
	{
		Func<ParserBase, InternalSyntaxToken, InternalSyntaxToken> errorFunction = null;
		if (!previousNode.ContainsDiagnostics)
		{
			errorFunction = (ParserBase p, InternalSyntaxToken token) => p.AddError(token, ErrorCode.ERR_InvalidValueInContext, token.ToString());
		}
		SkipBadTokens((ParserBase p) => p.IsEndOfFile || p.CurrentToken.IsAllowedVariableName() || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.IsMemberStart() || p.CurrentToken.IsObjectKeyword(), errorFunction, out InternalSyntaxNode trailingTrivia);
		if (trailingTrivia != null)
		{
			previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
		}
	}

	private void SkipBadVariableDefinitionTokens(ref InternalVariableDeclarationBaseSyntax previousNode)
	{
		Func<ParserBase, InternalSyntaxToken, InternalSyntaxToken> errorFunction = null;
		if (!previousNode.ContainsDiagnostics)
		{
			errorFunction = (ParserBase p, InternalSyntaxToken token) => p.AddError(token, ErrorCode.ERR_InvalidValueInContext, token.ToString());
		}
		SkipBadTokens((ParserBase p) => p.IsEndOfFile || p.CurrentToken.IsAllowedVariableName() || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsKeywordKind(SyntaxKind.BeginKeyword) || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.IsMemberStart(), errorFunction, out InternalSyntaxNode trailingTrivia);
		if (trailingTrivia != null)
		{
			previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
		}
	}

	private void SkipBadPropertyValue<TNode>(ref TNode previousToken) where TNode : InternalSyntaxNode
	{
		InternalSyntaxListBuilder<InternalSyntaxToken> internalSyntaxListBuilder = base.Pool.Allocate<InternalSyntaxToken>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.Kind != SyntaxKind.SemicolonToken && base.CurrentToken.Kind != SyntaxKind.CloseBraceToken && !base.CurrentToken.IsMemberStart())
			{
				InternalSyntaxToken node = EatToken();
				internalSyntaxListBuilder.Add(node);
			}
			if (internalSyntaxListBuilder.Count > 0)
			{
				previousToken = AddTrailingSkippedSyntax(previousToken, internalSyntaxListBuilder.ToListNode());
			}
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private T ParseInLexerMode<T>(LexerMode mode, Func<T> func)
	{
		LexerMode lexerMode = base.LexerMode;
		try
		{
			base.LexerMode = mode;
			return func();
		}
		finally
		{
			base.LexerMode = lexerMode;
		}
	}

	private InternalObjectNameOrIdSyntax ParseObjectReferenceSyntax(bool disallowIds = false)
	{
		if (base.CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			InternalObjectNameOrIdSyntax internalObjectNameOrIdSyntax = InternalSyntaxFactory.ObjectNameOrId(ParseObjectIdSyntax());
			if (disallowIds)
			{
				internalObjectNameOrIdSyntax = AddError(internalObjectNameOrIdSyntax, ErrorCode.ERR_IdSyntaxNotAllowed);
			}
			return internalObjectNameOrIdSyntax;
		}
		return InternalSyntaxFactory.ObjectNameOrId(ParseQualifiedName(!IsFeatureEnabled(Feature.Namespaces)));
	}

	private InternalObjectIdSyntax ParseObjectIdSyntax()
	{
		return InternalSyntaxFactory.ObjectId(EatToken(SyntaxKind.Int32LiteralToken));
	}

	private InternalTypeReferenceBaseSyntax CheckTypeReferenceSyntax(InternalTypeReferenceBaseSyntax type)
	{
		switch (type.Kind)
		{
		case SyntaxKind.DotNetTypeReference:
			type = CheckFeatureAvailability(type, Feature.DotNet);
			break;
		case SyntaxKind.SimpleTypeReference:
			if (type.DataType.Kind == SyntaxKind.EnumDataType)
			{
				type = CheckFeatureAvailability(type, Feature.Enum);
			}
			break;
		}
		return type;
	}

	private InternalSyntaxList<InternalMemberAttributeSyntax> ParseMemberAttributeList()
	{
		InternalSyntaxListBuilder<InternalMemberAttributeSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalMemberAttributeSyntax>();
		try
		{
			bool flag = false;
			while (base.CurrentToken.Kind == SyntaxKind.OpenBracketToken)
			{
				InternalMemberAttributeSyntax internalMemberAttributeSyntax = ParseAttribute();
				if (internalMemberAttributeSyntax.HasAnnotations(AnnotationKind.ExternalBusinessEvent))
				{
					flag = true;
				}
				internalSyntaxListBuilder.Add(internalMemberAttributeSyntax);
			}
			InternalSyntaxList<InternalMemberAttributeSyntax> internalSyntaxList = internalSyntaxListBuilder.ToList();
			if (flag)
			{
				internalSyntaxList = AnnotateWithExternalBusinessEventAnnotation(internalSyntaxList);
			}
			return internalSyntaxList;
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private static InternalSyntaxList<T> AnnotateWithExternalBusinessEventAnnotation<T>(InternalSyntaxList<T> internalSyntaxList) where T : InternalSyntaxNode
	{
		return new InternalSyntaxList<T>(internalSyntaxList.Node.WithAdditionalAnnotationsInternal(ExternalBusinessEventAnnotation));
	}

	private InternalMemberAttributeSyntax ParseAttribute()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.MemberAttribute)
		{
			return (InternalMemberAttributeSyntax)EatNode();
		}
		InternalSyntaxToken openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalAttributeArgumentListSyntax internalAttributeArgumentListSyntax = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		AttributeTypeInfo attributeInfo = AttributeDefinitions.GetAttributeInfo(internalIdentifierNameSyntax.Identifier.GetValueText());
		internalAttributeArgumentListSyntax = ParseAttributeArgumentList();
		InternalSyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
		InternalMemberAttributeSyntax internalMemberAttributeSyntax = InternalSyntaxFactory.MemberAttribute(openBracketToken, internalIdentifierNameSyntax, internalAttributeArgumentListSyntax, closeBracketToken);
		if (attributeInfo != null)
		{
			internalMemberAttributeSyntax = internalMemberAttributeSyntax.WithAnnotations(new SyntaxAnnotation[1] { attributeInfo.AttributeAnnotation });
			if (attributeInfo.Kind == AttributeKind.ExternalBusinessEvent)
			{
				internalMemberAttributeSyntax = internalMemberAttributeSyntax.WithAdditionalAnnotationsInternal(ExternalBusinessEventAnnotation);
			}
		}
		return internalMemberAttributeSyntax;
	}

	private void SkipOrphanedAttributes(ref InternalSyntaxNode lastGoodNode, InternalSyntaxList<InternalMemberAttributeSyntax> orphanedAttributeList)
	{
		for (int i = 0; i < orphanedAttributeList.Count; i++)
		{
			lastGoodNode = AddTrailingSkippedSyntax(lastGoodNode, AddError(orphanedAttributeList[i], ErrorCode.ERR_AttributeNotOnAValidSymbol));
		}
	}

	private InternalAttributeArgumentListSyntax ParseAttributeArgumentList()
	{
		InternalAttributeArgumentListSyntax result = null;
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> arguments = ParseAttributeArguments(ref startToken);
			result = InternalSyntaxFactory.AttributeArgumentList(startToken, arguments, EatToken(SyntaxKind.CloseParenToken));
		}
		return result;
	}

	private InternalSeparatedSyntaxList<InternalAttributeArgumentSyntax> ParseAttributeArguments(ref InternalSyntaxToken startToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.Int32LiteralToken, (InternalSyntaxToken token) => token.IsPossibleAttributeArgument(), SyntaxKind.CommaToken, SyntaxKind.CloseParenToken, delegate(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalAttributeArgumentSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected != SyntaxKind.CommaToken && !base.CurrentToken.IsPossibleAttributeArgument()), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsPossibleProcedure() || p.CurrentToken.IsPossibleTrigger() || p.CurrentToken.Kind == SyntaxKind.EndOfFileToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken, expected);
		}, delegate
		{
			switch (base.CurrentToken.ContextualKind)
			{
			case SyntaxKind.Int32LiteralToken:
			case SyntaxKind.StringLiteralToken:
			case SyntaxKind.FalseKeyword:
			case SyntaxKind.TrueKeyword:
				return InternalSyntaxFactory.LiteralAttributeArgument(InternalSyntaxFactory.LiteralExpression(ParseLiteralValue()));
			case SyntaxKind.IdentifierToken:
			{
				InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseExpression();
				return internalCodeExpressionSyntax.Kind switch
				{
					SyntaxKind.OptionAccessExpression => InternalSyntaxFactory.OptionAccessAttributeArgument(internalCodeExpressionSyntax as InternalOptionAccessExpressionSyntax), 
					SyntaxKind.IdentifierName => InternalSyntaxFactory.IdentifierAttributeArgument(InternalSyntaxFactory.ObjectNameReference(internalCodeExpressionSyntax)), 
					_ => AddError(InternalSyntaxFactory.InvalidAttributeArgument(internalCodeExpressionSyntax), ErrorCode.ERR_InvalidAttributeArgumentSyntax, internalCodeExpressionSyntax.ToString()), 
				};
			}
			default:
				return AddError(InternalSyntaxFactory.InvalidAttributeArgument(ParserBase.MissingToken(SyntaxKind.InvalidAttributeArgument)), ErrorCode.ERR_InvalidAttributeArgumentSyntax, base.CurrentToken.Text);
			}
		});
	}

	private PostSkipAction SkipBadAttributeArgumentTokens(ref InternalSyntaxToken openParen, InternalSeparatedSyntaxListBuilder<InternalAttributeArgumentSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref openParen, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected != SyntaxKind.CommaToken && !base.CurrentToken.IsPossibleAttributeArgument()), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.CloseBracketToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken || p.CurrentToken.IsPossibleProcedure() || p.CurrentToken.IsPossibleTrigger() || p.CurrentToken.Kind == SyntaxKind.EndOfFileToken || p.CurrentToken.Kind == SyntaxKind.OpenBracketToken, expected);
	}

	private InternalAttributeArgumentSyntax ParseAttributeArgument()
	{
		switch (base.CurrentToken.ContextualKind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
			return InternalSyntaxFactory.LiteralAttributeArgument(InternalSyntaxFactory.LiteralExpression(ParseLiteralValue()));
		case SyntaxKind.IdentifierToken:
		{
			InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseExpression();
			return internalCodeExpressionSyntax.Kind switch
			{
				SyntaxKind.OptionAccessExpression => InternalSyntaxFactory.OptionAccessAttributeArgument(internalCodeExpressionSyntax as InternalOptionAccessExpressionSyntax), 
				SyntaxKind.IdentifierName => InternalSyntaxFactory.IdentifierAttributeArgument(InternalSyntaxFactory.ObjectNameReference(internalCodeExpressionSyntax)), 
				_ => AddError(InternalSyntaxFactory.InvalidAttributeArgument(internalCodeExpressionSyntax), ErrorCode.ERR_InvalidAttributeArgumentSyntax, internalCodeExpressionSyntax.ToString()), 
			};
		}
		default:
			return AddError(InternalSyntaxFactory.InvalidAttributeArgument(ParserBase.MissingToken(SyntaxKind.InvalidAttributeArgument)), ErrorCode.ERR_InvalidAttributeArgumentSyntax, base.CurrentToken.Text);
		}
	}

	internal InternalPropertyValueSyntax ParseCalculationFormulaPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSyntaxToken sign = EatTokenIfKind(SyntaxKind.MinusToken);
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfCondition((SyntaxKind kind) => kind.IsCalculationFormulaMethodKind());
		if (internalSyntaxToken == null)
		{
			internalSyntaxToken = InternalSyntaxFactory.MissingToken(SyntaxKind.LookupCalculationFormulaKeyword);
			internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_InvalidCalculationFormulaMethod);
		}
		SyntaxKind calcFormulaStatementKind = GetCalcFormulaStatementKind(internalSyntaxToken.Kind);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalNameSyntax internalNameSyntax = ParseQualifiedName();
		InternalWhereExpressionSyntax whereExpression = ParseTableFilters();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		if (internalSyntaxToken.Kind == SyntaxKind.ExistCalculationFormulaKeyword || internalSyntaxToken.Kind == SyntaxKind.CountCalculationFormulaKeyword)
		{
			return InternalSyntaxFactory.TableCalculationFormula(calcFormulaStatementKind, sign, internalSyntaxToken, openParenthesisToken, internalNameSyntax, whereExpression, closeParenthesisToken);
		}
		if (internalNameSyntax.Kind != SyntaxKind.QualifiedName)
		{
			internalNameSyntax = InternalSyntaxFactory.QualifiedName(internalNameSyntax, CreateMissingToken(SyntaxKind.DotToken, internalNameSyntax.Kind, !internalSyntaxToken.ContainsDiagnostics), ParserBase.CreateMissingIdentifierName());
		}
		return InternalSyntaxFactory.FieldCalculationFormula(calcFormulaStatementKind, sign, internalSyntaxToken, openParenthesisToken, (InternalQualifiedNameSyntax)internalNameSyntax, whereExpression, closeParenthesisToken);
	}

	private InternalPropertyExpressionSyntax ParsePropertyExpression()
	{
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfCondition((SyntaxKind k) => k == SyntaxKind.FieldFormulaKeyword || k == SyntaxKind.ConstFormulaKeyword || k == SyntaxKind.FilterFormulaKeyword);
		if (internalSyntaxToken != null)
		{
			switch (internalSyntaxToken.Kind)
			{
			case SyntaxKind.FieldFormulaKeyword:
				return ParseFieldPropertyExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			case SyntaxKind.ConstFormulaKeyword:
				return ParseConstPropertyExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			case SyntaxKind.FilterFormulaKeyword:
				return ParseFilterExpression(internalIdentifierNameSyntax, equalsToken, internalSyntaxToken);
			}
		}
		return InternalSyntaxFactory.InvalidPropertyExpression(internalIdentifierNameSyntax, equalsToken, null, AddError(EatKeywordToken(SyntaxKind.FieldKeyword), ErrorCode.ERR_ExpectedFieldFilterOrConstKeyword));
	}

	private InternalPropertyExpressionSyntax ParseFieldPropertyExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeyword)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken internalSyntaxToken2 = EatKeywordTokenIfCondition((SyntaxKind k) => k == SyntaxKind.FilterFormulaKeyword || k == SyntaxKind.UpperLimitFormulaKeyword);
		if (internalSyntaxToken2 == null)
		{
			internalSyntaxToken2 = ParseIdentifierToken();
		}
		if (internalSyntaxToken2.Kind == SyntaxKind.IdentifierToken && !internalSyntaxToken2.IsKeywordKind(SyntaxKind.FilterFormulaKeyword, SyntaxKind.UpperLimitFormulaKeyword))
		{
			return InternalSyntaxFactory.SimpleFieldExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken, InternalSyntaxFactory.IdentifierName(internalSyntaxToken2), EatToken(SyntaxKind.CloseParenToken));
		}
		return internalSyntaxToken2.ContextualKind switch
		{
			SyntaxKind.FilterFormulaKeyword => InternalSyntaxFactory.FieldFilterExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken, internalSyntaxToken2, EatToken(SyntaxKind.OpenParenToken), ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken)), 
			SyntaxKind.UpperLimitFormulaKeyword => ParseFieldUpperLimitFilterExpressionSyntax(lhs, equalsToken, fieldKeyword, internalSyntaxToken, internalSyntaxToken2), 
			_ => InternalSyntaxFactory.InvalidPropertyExpression(lhs, equalsToken, fieldKeyword, internalSyntaxToken2), 
		};
	}

	private InternalPropertyExpressionSyntax ParseFieldUpperLimitFilterExpressionSyntax(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeyword, InternalSyntaxToken openParToken, InternalSyntaxToken upperLimitKeyword)
	{
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenParenToken);
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.FilterFormulaKeyword))
		{
			return InternalSyntaxFactory.FieldUpperLimitFilterExpression(lhs, equalsToken, fieldKeyword, openParToken, upperLimitKeyword, internalSyntaxToken, EatKeywordToken(SyntaxKind.FilterFormulaKeyword), EatToken(SyntaxKind.OpenParenToken), ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken));
		}
		return InternalSyntaxFactory.FieldUpperLimitExpression(lhs, equalsToken, fieldKeyword, openParToken, upperLimitKeyword, internalSyntaxToken, ParseIdentifierName(), EatToken(SyntaxKind.CloseParenToken), EatToken(SyntaxKind.CloseParenToken));
	}

	private InternalPropertyExpressionSyntax ParseConstPropertyExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken constKeyword)
	{
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax identifier = TryParseIdentifierOrLiteralOrOptionAccessExpression();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.ConstExpression(lhs, equalsToken, constKeyword, openParenthesisToken, identifier, closeParenthesisToken);
	}

	private InternalFilterExpressionSyntax ParseFilterExpression(InternalIdentifierNameSyntax lhs, InternalSyntaxToken equalsToken, InternalSyntaxToken filterFormulaKeyword)
	{
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalFilterExpressionValueSyntax internalFilterExpressionValueSyntax = ParseFilterExpressionValue();
		DebugAssertHelper.Assert(internalFilterExpressionValueSyntax != null);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.FilterExpression(lhs, equalsToken, filterFormulaKeyword, openParenthesisToken, internalFilterExpressionValueSyntax, closeParenthesisToken);
	}

	private static SyntaxKind GetCalcFormulaStatementKind(SyntaxKind calcformulaMethodKind)
	{
		return calcformulaMethodKind switch
		{
			SyntaxKind.AverageCalculationFormulaKeyword => SyntaxKind.AverageCalculationFormulaStatement, 
			SyntaxKind.CountCalculationFormulaKeyword => SyntaxKind.CountCalculationFormulaStatement, 
			SyntaxKind.MinCalculationFormulaKeyword => SyntaxKind.MinCalculationFormulaStatement, 
			SyntaxKind.MaxCalculationFormulaKeyword => SyntaxKind.MaxCalculationFormulaStatement, 
			SyntaxKind.ExistCalculationFormulaKeyword => SyntaxKind.ExistCalculationFormulaStatement, 
			SyntaxKind.LookupCalculationFormulaKeyword => SyntaxKind.LookupCalculationFormulaStatement, 
			SyntaxKind.SumCalculationFormulaKeyword => SyntaxKind.SumCalculationFormulaStatement, 
			_ => SyntaxKind.None, 
		};
	}

	private InternalCodeunitSyntax ParseCodeunit()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.CodeunitObject)
		{
			return (InternalCodeunitSyntax)EatNode();
		}
		InternalSyntaxToken codeunitKeyword = EatKeywordToken(SyntaxKind.CodeunitKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Codeunit {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken startToken = null;
		InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax> interfaces = default(InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax>);
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.ImplementsKeyword))
		{
			startToken = CheckFeatureAvailability(EatKeywordToken(), Feature.Interfaces);
			interfaces = ParseCommaSeparatedObjectNameReferences(ref startToken, SyntaxKind.OpenBraceToken);
			if (interfaces.Count == 0)
			{
				startToken = AddError(startToken, ErrorCode.ERR_IdentifierExpected);
			}
		}
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList;
		InternalSyntaxList<InternalMemberSyntax> members;
		if (!internalIdentifierNameSyntax.IsMissing && !internalSyntaxToken.IsMissing)
		{
			propertyList = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value = null;
				CodeunitProperties.TryGetValue(name.ToUpperInvariant(), out value);
				return value;
			});
			members = ParseMemberList(TriggerDefinitions.GetCodeunitTriggerInfo);
		}
		else
		{
			propertyList = InternalSyntaxFactory.PropertyList(default(InternalSyntaxList<InternalPropertySyntaxOrEmpty>));
			members = default(InternalSyntaxList<InternalMemberSyntax>);
		}
		return InternalSyntaxFactory.Codeunit(closeBraceToken: ParseCloseBraceToken(internalSyntaxToken), codeunitKeyword: codeunitKeyword, objectId: internalObjectIdSyntax, name: internalIdentifierNameSyntax, implementsKeyword: startToken, interfaces: interfaces, openBraceToken: internalSyntaxToken, propertyList: propertyList, members: members);
	}

	private InternalControlAddInSyntax ParseControlAddIn()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ControlAddInObject)
		{
			return (InternalControlAddInSyntax)EatNode();
		}
		InternalSyntaxToken controlAddInKeyword = EatKeywordToken(SyntaxKind.ControlAddInKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing ControlAddIn {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ControlAddInObjectProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.ControlAddIn(controlAddInKeyword, internalIdentifierNameSyntax, openBraceToken, propertyList, members, closeBraceToken);
	}

	private InternalDotNetPackageSyntax ParseDotNet()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.DotNetPackage)
		{
			return (InternalDotNetPackageSyntax)EatNode();
		}
		InternalSyntaxToken dotNetKeyword = EatKeywordToken(SyntaxKind.DotNetKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseOptionalName();
		LocalMachineLogger.LogVerbose("Parsing DotNet {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList((string s) => (PropertyTypeInfo)null);
		InternalSyntaxList<InternalDotNetAssemblySyntax> assemblies = ParseAssemblies();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberListAndReportErrorIfNotEmpty();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalDotNetPackageSyntax node = InternalSyntaxFactory.DotNetPackage(dotNetKeyword, internalIdentifierNameSyntax, openBraceToken, propertyList, assemblies, members, closeBraceToken);
		return CheckFeatureAvailability(node, Feature.DotNet);
	}

	private InternalSyntaxList<InternalMemberSyntax> ParseMemberListAndReportErrorIfNotEmpty()
	{
		InternalSyntaxList<InternalMemberSyntax> result = ParseMemberList();
		if (result.Count == 0)
		{
			return result;
		}
		InternalSyntaxListBuilder<InternalMemberSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalMemberSyntax>();
		try
		{
			for (int i = 0; i < result.Count; i++)
			{
				internalSyntaxListBuilder.Add(AddError(result[i], ErrorCode.ERR_MemberNotAllowedInContext));
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalSyntaxList<InternalDotNetAssemblySyntax> ParseAssemblies()
	{
		InternalSyntaxListBuilder<InternalDotNetAssemblySyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalDotNetAssemblySyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.AssemblyKeyword))
			{
				InternalDotNetAssemblySyntax node = ParseAssembly();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalDotNetAssemblySyntax ParseAssembly()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.DotNetAssembly)
		{
			return (InternalDotNetAssemblySyntax)EatNode();
		}
		InternalSyntaxToken assemblyKeyword = EatKeywordToken(SyntaxKind.AssemblyKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalNameSyntax assemblyName = ParseQualifiedName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			DotNetAssemblyProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> typeDeclarations = ParseDotNetTypeDeclarations();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.DotNetAssembly(assemblyKeyword, openParenthesisToken, assemblyName, closeParenthesisToken, openBraceToken, propertyList, typeDeclarations, closeBraceToken);
	}

	private InternalSyntaxList<InternalDotNetTypeDeclarationSyntax> ParseDotNetTypeDeclarations()
	{
		InternalSyntaxListBuilder<InternalDotNetTypeDeclarationSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalDotNetTypeDeclarationSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.TypeKeyword))
			{
				InternalDotNetTypeDeclarationSyntax node = ParseDotNetTypeDeclaration();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalDotNetTypeDeclarationSyntax ParseDotNetTypeDeclaration()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.DotNetTypeDeclaration)
		{
			return (InternalDotNetTypeDeclarationSyntax)EatNode();
		}
		InternalSyntaxToken type = EatKeywordToken(SyntaxKind.TypeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalNameSyntax typeName = ParseQualifiedName();
		ParseOptionalTypeAlias(base.CurrentToken, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression);
		return InternalSyntaxFactory.DotNetTypeDeclaration(closeParenthesisToken: EatToken(SyntaxKind.CloseParenToken), openBraceToken: EatToken(SyntaxKind.OpenBraceToken), propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			DotNetTypeDeclarationProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), closeBraceToken: EatToken(SyntaxKind.CloseBraceToken), type: type, openParenthesisToken: openParenthesisToken, typeName: typeName, semicolonToken: semiColon, typeAlias: sourceExpression);
	}

	private void ParseOptionalTypeAlias(InternalSyntaxToken ct, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression)
	{
		if (ct.Kind == SyntaxKind.SemicolonToken)
		{
			semiColon = EatToken(SyntaxKind.SemicolonToken);
			sourceExpression = ParseIdentifierName();
		}
		else
		{
			semiColon = null;
			sourceExpression = null;
		}
	}

	private InternalIdentifierNameSyntax ParseOptionalName()
	{
		if (base.CurrentToken.Kind != SyntaxKind.OpenBraceToken)
		{
			return ParseIdentifierName();
		}
		return null;
	}

	private InternalEntitlementSyntax ParseEntitlement()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.Entitlement)
		{
			return (InternalEntitlementSyntax)EatNode();
		}
		InternalSyntaxToken entitlementToken = EatKeywordToken(SyntaxKind.EntitlementKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Entitlement {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalEntitlementSyntax node = InternalSyntaxFactory.Entitlement(propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			EntitlementProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), closeBraceToken: ParseCloseBraceToken(openBraceToken), entitlementToken: entitlementToken, name: internalIdentifierNameSyntax, openBraceToken: openBraceToken, members: null);
		return CheckFeatureAvailability(node, Feature.PermissionSet);
	}

	private InternalEnumTypeSyntax ParseEnum()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.EnumType)
		{
			return (InternalEnumTypeSyntax)EatNode();
		}
		InternalSyntaxToken enumToken = EatKeywordToken(SyntaxKind.EnumKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Enum {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken startToken = null;
		InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax> interfaces = default(InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax>);
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.ImplementsKeyword))
		{
			startToken = CheckFeatureAvailability(EatKeywordToken(), Feature.Interfaces);
			interfaces = ParseCommaSeparatedObjectNameReferences(ref startToken, SyntaxKind.OpenBraceToken);
			if (interfaces.Count == 0)
			{
				startToken = AddError(startToken, ErrorCode.ERR_IdentifierExpected);
			}
		}
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalEnumTypeSyntax node = InternalSyntaxFactory.EnumType(propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			EnumTypeProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), values: ParseEnumValueList(), closeBraceToken: ParseCloseBraceToken(openBraceToken), enumToken: enumToken, objectId: internalObjectIdSyntax, name: internalIdentifierNameSyntax, implementsKeyword: startToken, interfaces: interfaces, openBraceToken: openBraceToken, members: default(InternalSyntaxList<InternalMemberSyntax>));
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalSyntaxList<InternalEnumValueSyntax> ParseEnumValueList()
	{
		InternalSyntaxListBuilder<InternalEnumValueSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalEnumValueSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.EnumValueKeyword))
			{
				InternalEnumValueSyntax node = ParseEnumValue();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalEnumValueSyntax ParseEnumValue()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.EnumValue)
		{
			return (InternalEnumValueSyntax)EatNode();
		}
		InternalSyntaxToken enumValueToken = EatKeywordToken(SyntaxKind.EnumValueKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken id = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			EnumValueProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		return InternalSyntaxFactory.EnumValue(enumValueToken, openParenthesisToken, id, semicolonToken, name2, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalEnumExtensionTypeSyntax ParseEnumExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.EnumExtensionType)
		{
			return (InternalEnumExtensionTypeSyntax)EatNode();
		}
		InternalSyntaxToken enumExtensionKeyword = EatKeywordToken(SyntaxKind.EnumExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing EnumExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			EnumTypeProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalEnumValueSyntax> values = ParseEnumValueList();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalEnumExtensionTypeSyntax node = InternalSyntaxFactory.EnumExtensionType(enumExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, values, default(InternalSyntaxList<InternalMemberSyntax>), closeBraceToken);
		return CheckFeatureAvailability(node, Feature.Enum);
	}

	private InternalFilterExpressionValueSyntax ParseFilterExpressionValue(SyntaxKind kind = SyntaxKind.OrFilterKeyword)
	{
		InternalFilterExpressionValueSyntax internalFilterExpressionValueSyntax = ParseNextFilterExpressionValue(kind);
		while (base.CurrentToken.IsKeywordKind(kind))
		{
			InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
			InternalFilterExpressionValueSyntax right = ParseNextFilterExpressionValue(kind);
			internalFilterExpressionValueSyntax = InternalSyntaxFactory.BinaryFilterExpressionValue(internalSyntaxToken.Kind.ToBinaryFilterExpressionKind(), internalFilterExpressionValueSyntax, internalSyntaxToken, right);
		}
		return internalFilterExpressionValueSyntax;
	}

	private InternalFilterExpressionValueSyntax ParseUnaryOrLiteralExpressionValue()
	{
		if (base.CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			return InternalSyntaxFactory.ParenthesizedFilterExpressionValue(EatToken(SyntaxKind.OpenParenToken), ParseFilterExpressionValue(), EatToken(SyntaxKind.CloseParenToken));
		}
		if (!TryParseIdentifierOrLiteralOrRangeFilterExpressionValue(out InternalFilterExpressionValueSyntax filterValue) && (!TryParseUnaryFilterExpressionValue(out filterValue) || filterValue == null))
		{
			filterValue = CreateInvalidFilterExpressionValueSyntax(string.Empty);
			return AddTrailingSkippedSyntax(filterValue, AddError(EatToken(), ErrorCode.ERR_ExpressionExpected));
		}
		return filterValue;
	}

	private bool TryParseIdentifierOrLiteralOrRangeFilterExpressionValue(out InternalFilterExpressionValueSyntax? filterValue)
	{
		filterValue = null;
		InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax internalIdentifierOrLiteralOrOptionAccessExpressionSyntax = TryParseIdentifierOrLiteralOrOptionAccessExpression();
		if (internalIdentifierOrLiteralOrOptionAccessExpressionSyntax == null)
		{
			return false;
		}
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.DotDotToken);
		if (internalSyntaxToken == null)
		{
			filterValue = InternalSyntaxFactory.UnaryFilterExpressionValue(SyntaxKind.UnaryEqualsFilterExpression, null, internalIdentifierOrLiteralOrOptionAccessExpressionSyntax);
		}
		else
		{
			InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax internalIdentifierOrLiteralOrOptionAccessExpressionSyntax2 = TryParseIdentifierOrLiteralOrOptionAccessExpression();
			filterValue = ((internalIdentifierOrLiteralOrOptionAccessExpressionSyntax2 != null) ? ((InternalFilterExpressionValueSyntax)InternalSyntaxFactory.BinaryFilterExpressionValue(SyntaxKind.RangeBetweenFilterExpression, internalIdentifierOrLiteralOrOptionAccessExpressionSyntax, internalSyntaxToken, internalIdentifierOrLiteralOrOptionAccessExpressionSyntax2)) : ((InternalFilterExpressionValueSyntax)InternalSyntaxFactory.RangeFromFilterExpressionValue(internalIdentifierOrLiteralOrOptionAccessExpressionSyntax, internalSyntaxToken)));
		}
		return true;
	}

	private bool TryParseUnaryFilterExpressionValue(out InternalFilterExpressionValueSyntax? filterValue)
	{
		filterValue = null;
		if (!base.CurrentToken.Kind.IsFilterTokenUnary() && base.CurrentToken.Kind != SyntaxKind.DotDotToken)
		{
			return false;
		}
		SyntaxKind kind = base.CurrentToken.Kind.ToUnaryFilterExpressionKind();
		InternalSyntaxToken operatorToken = EatToken();
		InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax identifier = ParseIdentifierOrLiteralOrOptionAccessExpression();
		filterValue = InternalSyntaxFactory.UnaryFilterExpressionValue(kind, operatorToken, identifier);
		return true;
	}

	private InternalFilterExpressionValueSyntax CreateInvalidFilterExpressionValueSyntax(string text)
	{
		InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax identifier = InternalSyntaxFactory.IdentifierOrLiteralOrOptionAccessExpression(InternalSyntaxFactory.IdentifierName(InternalSyntaxFactory.Identifier(text)));
		InternalFilterExpressionValueSyntax node = InternalSyntaxFactory.UnaryFilterExpressionValue(SyntaxKind.UnaryEqualsFilterExpression, null, identifier);
		return AddError(node, ErrorCode.ERR_InvalidFilterExpression);
	}

	private InternalFilterExpressionValueSyntax ParseNextFilterExpressionValue(SyntaxKind kind)
	{
		if (kind == SyntaxKind.OrFilterKeyword)
		{
			return ParseFilterExpressionValue(SyntaxKind.AndFilterKeyword);
		}
		return ParseUnaryOrLiteralExpressionValue();
	}

	private InternalInterfaceSyntax ParseInterface()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.Interface)
		{
			return (InternalInterfaceSyntax)EatNode();
		}
		InternalSyntaxToken interfaceToken = EatKeywordToken(SyntaxKind.InterfaceKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken startToken = null;
		InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax> extendsInterfaces = default(InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax>);
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.ExtendsKeyword))
		{
			startToken = CheckFeatureAvailability(EatKeywordToken(SyntaxKind.ExtendsKeyword), Feature.InterfaceExtends);
			extendsInterfaces = ParseCommaSeparatedObjectNameReferences(ref startToken, SyntaxKind.OpenBraceToken);
		}
		LocalMachineLogger.LogVerbose("Parsing Interface {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList;
		InternalSyntaxList<InternalMemberSyntax> members;
		if (!internalIdentifierNameSyntax.IsMissing && !internalSyntaxToken.IsMissing)
		{
			propertyList = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value = null;
				InterfaceProperties.TryGetValue(name.ToUpperInvariant(), out value);
				return value;
			});
			members = ParseMemberList();
		}
		else
		{
			propertyList = InternalSyntaxFactory.PropertyList(default(InternalSyntaxList<InternalPropertySyntaxOrEmpty>));
			members = default(InternalSyntaxList<InternalMemberSyntax>);
		}
		InternalInterfaceSyntax node = InternalSyntaxFactory.Interface(closeBraceToken: ParseCloseBraceToken(internalSyntaxToken), interfaceToken: interfaceToken, name: internalIdentifierNameSyntax, extendsKeyword: startToken, extendsInterfaces: extendsInterfaces, openBraceToken: internalSyntaxToken, propertyList: propertyList, members: members);
		return CheckFeatureAvailability(node, Feature.Interfaces);
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedIdentifierOrLiteralPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierOrLiteralPropertyValue(ParseCommaSeparatedLiteralOrIdentifier(ref startToken));
	}

	private static bool IsValidLiteralNodeForList(InternalSyntaxToken token)
	{
		if (!token.Kind.IsLiteral() && token.Kind != SyntaxKind.MinusToken)
		{
			return token.Kind.IsTokenIdentifier();
		}
		return true;
	}

	private InternalSeparatedSyntaxList<InternalIdentifierOrLiteralExpressionSyntax> ParseCommaSeparatedLiteralOrIdentifier(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsLiteral() || token.Kind == SyntaxKind.MinusToken || token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, delegate(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalIdentifierOrLiteralExpressionSyntax> list, SyntaxKind expected)
		{
			Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidLiteralNodeForList(p.CurrentToken);
			Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
		}, delegate
		{
			InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = TryParseIdentifierOrLiteralExpression();
			return (internalIdentifierOrLiteralExpressionSyntax != null) ? internalIdentifierOrLiteralExpressionSyntax : InternalSyntaxFactory.IdentifierOrLiteralExpression(InternalSyntaxFactory.IdentifierName(AddError(ParserBase.CreateMissingIdentifierToken(), ErrorCode.ERR_ExpectedIdentifierOrLiteral)));
		});
	}

	private PostSkipAction SkipBadCommaSeparatedLiteralOrIdentifier(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalIdentifierOrLiteralExpressionSyntax> list, SyntaxKind expected)
	{
		Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidLiteralNodeForList(p.CurrentToken);
		Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
	}

	private InternalPropertyValueSyntax ParseOrderByPropertyValue(PropertyTypeInfo propertyTypeInfo, ref InternalSyntaxToken equalsToken)
	{
		return ParseOrderByPropertyValue(ref equalsToken);
	}

	internal InternalPropertyValueSyntax ParseOrderByPropertyValue(ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.OrderByPropertyValue(ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => IsValidOrderSpecifierKeyword(token) || token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, delegate(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalOrderByExpressionSyntax> list, SyntaxKind expected)
		{
			Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidOrderSpecifierKeyword(p.CurrentToken);
			Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
			return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
		}, delegate
		{
			InternalSyntaxToken orderSpecifier = EatKeywordToken(SyntaxKind.DescendingKeyword, SyntaxKind.AscendingKeyword, ErrorCode.ERR_ExpectedAscendingOrDescendingKeyword);
			InternalSyntaxToken startToken2 = EatToken(SyntaxKind.OpenParenToken);
			return InternalSyntaxFactory.OrderByExpression(sortingFields: ParseCommaSeparatedIdentifierNames(ref startToken2), closeParenthesisToken: EatToken(SyntaxKind.CloseParenToken), orderSpecifier: orderSpecifier, openParenthesisToken: startToken2);
		}));
	}

	private PostSkipAction SkipBadOrderByItems(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<InternalOrderByExpressionSyntax> list, SyntaxKind expected)
	{
		Func<ParserBase, bool> isNotExpectedFunction = (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || !IsValidOrderSpecifierKeyword(p.CurrentToken);
		Func<ParserBase, bool> abortFunction = (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.CurrentToken.Kind == SyntaxKind.CloseParenToken || p.IsTerminator();
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, isNotExpectedFunction, abortFunction, expected);
	}

	private static bool IsValidOrderSpecifierKeyword(InternalSyntaxToken node)
	{
		return node.IsKeywordKind(SyntaxKind.AscendingKeyword, SyntaxKind.DescendingKeyword);
	}

	private InternalOrderByExpressionSyntax ParseOrderByElement()
	{
		InternalSyntaxToken orderSpecifier = EatKeywordToken(SyntaxKind.DescendingKeyword, SyntaxKind.AscendingKeyword, ErrorCode.ERR_ExpectedAscendingOrDescendingKeyword);
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		return InternalSyntaxFactory.OrderByExpression(sortingFields: ParseCommaSeparatedIdentifierNames(ref startToken), closeParenthesisToken: EatToken(SyntaxKind.CloseParenToken), orderSpecifier: orderSpecifier, openParenthesisToken: startToken);
	}

	private InternalPageSyntax ParsePage()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageObject)
		{
			return (InternalPageSyntax)EatNode();
		}
		InternalSyntaxToken pageKeyword = EatKeywordToken(SyntaxKind.PageKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Page {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalPageViewListSyntax views = ParsePageViews();
		InternalPageAnalysisViewListSyntax analysisViews = ParsePageAnalysisViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return AnnotateWithActionVersions(InternalSyntaxFactory.Page(pageKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, layout, actions, views, analysisViews, members, closeBraceToken), propertyList, actions);
	}

	private InternalPageCustomizationSyntax ParsePageCustomization()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageCustomizationObject)
		{
			return (InternalPageCustomizationSyntax)EatNode();
		}
		InternalSyntaxToken pageCustomizationKeyword = EatKeywordToken(SyntaxKind.PageCustomizationKeyword);
		InternalObjectIdSyntax objectId = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing PageCustomization {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken customizesKeyword = EatKeywordToken(SyntaxKind.CustomizesKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupPageProperty(name) ?? LookupPageCustomizationObjectProperty(name), asExtension: false, deferErrorChecking: false, asCustomization: true);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalPageExtensionActionListSyntax actions = ParsePageExtensionActions();
		InternalPageExtensionViewListSyntax views = ParsePageExtensionViews();
		InternalPageExtensionAnalysisViewListSyntax analysisViews = ParsePageExtensionAnalysisViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList((string s) => (TriggerTypeInfo)null);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return AnnotateWithActionVersions(InternalSyntaxFactory.PageCustomization(pageCustomizationKeyword, objectId, internalIdentifierNameSyntax, customizesKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, analysisViews, members, closeBraceToken), propertyList, actions);
	}

	private static PropertyTypeInfo LookupPageCustomizationProperty(string name)
	{
		return LookupPageProperty(name) ?? LookupPageCustomizationObjectProperty(name);
	}

	private InternalPageExtensionSyntax ParsePageExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageExtensionObject)
		{
			return (InternalPageExtensionSyntax)EatNode();
		}
		InternalSyntaxToken pageExtensionKeyword = EatKeywordToken(SyntaxKind.PageExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing PageExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}, asExtension: true);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalPageExtensionActionListSyntax actions = ParsePageExtensionActions();
		InternalPageExtensionViewListSyntax views = ParsePageExtensionViews();
		InternalPageExtensionAnalysisViewListSyntax analysisViews = ParsePageExtensionAnalysisViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return AnnotateWithActionVersions(InternalSyntaxFactory.PageExtension(pageExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, layout, actions, views, analysisViews, members, closeBraceToken), propertyList, actions);
	}

	private InternalPageActionListSyntax ParsePageActions()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ActionsKeyword))
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageActionList)
		{
			return (InternalPageActionListSyntax)EatNode();
		}
		InternalSyntaxToken actionsKeyword = EatKeywordToken(SyntaxKind.ActionsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalActionBaseSyntax> actionList = ParseActionList(disallowGroups: true);
		InternalSyntaxList<InternalPageActionAreaSyntax> areas = ParseActionAreas();
		return AnnotateWithActionVersions(InternalSyntaxFactory.PageActionList(actionsKeyword, openBraceToken, actionList, areas, EatToken(SyntaxKind.CloseBraceToken)), null, areas.Node);
	}

	private InternalSyntaxList<InternalPageActionAreaSyntax> ParseActionAreas()
	{
		InternalSyntaxListBuilder<InternalPageActionAreaSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalPageActionAreaSyntax>();
		bool flag = false;
		bool flag2 = false;
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.PageAreaKeyword))
			{
				InternalPageActionAreaSyntax internalPageActionAreaSyntax = ParseActionArea();
				if (!flag && internalPageActionAreaSyntax.HasAnnotations(AnnotationKind.ActionV1))
				{
					flag = true;
				}
				if (!flag2 && internalPageActionAreaSyntax.HasAnnotations(AnnotationKind.ActionV2))
				{
					flag2 = true;
				}
				internalSyntaxListBuilder.Add(internalPageActionAreaSyntax);
			}
			return AnnotateWithActionVersions(internalSyntaxListBuilder.ToList(), flag, flag2);
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageActionAreaSyntax ParseActionArea()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageActionArea)
		{
			return (InternalPageActionAreaSyntax)EatNode();
		}
		InternalSyntaxToken actionKeyword = EatKeywordToken(SyntaxKind.PageAreaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageActionAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalActionBaseSyntax> actions = ParseActionList(disallowGroups: false, SyntaxFacts.GetActionAreaKind(internalIdentifierNameSyntax.Identifier.GetValueText()));
		return AnnotateWithActionVersions(InternalSyntaxFactory.PageActionArea(actionKeyword, openParenthesisToken, internalIdentifierNameSyntax, closeParenthesisToken, openBraceToken, propertyList, actions, EatToken(SyntaxKind.CloseBraceToken)), null, actions.Node, internalIdentifierNameSyntax);
	}

	private InternalSyntaxList<InternalActionBaseSyntax> ParseActionList(bool disallowGroups = false, ActionAreaKind areaKind = ActionAreaKind.None)
	{
		InternalSyntaxListBuilder<InternalActionBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalActionBaseSyntax>();
		bool flag = false;
		bool flag2 = false;
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageActionOrGroupKeyword())
			{
				InternalActionBaseSyntax internalActionBaseSyntax = ParseAction(disallowGroups, areaKind);
				if (!flag && internalActionBaseSyntax.HasAnnotations(AnnotationKind.ActionV1))
				{
					flag = true;
				}
				if (!flag2 && internalActionBaseSyntax.HasAnnotations(AnnotationKind.ActionV2))
				{
					flag2 = true;
				}
				internalSyntaxListBuilder.Add(internalActionBaseSyntax);
			}
			return AnnotateWithActionVersions(internalSyntaxListBuilder.ToList(), flag, flag2);
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalActionBaseSyntax ParseAction(bool disallowGroups, ActionAreaKind areaKind)
	{
		if (base.IsIncremental && base.CurrentNodeKind != SyntaxKind.PageActionArea && base.CurrentNodeKind.IsActionBaseSyntax())
		{
			return (InternalActionBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = null;
		InternalIdentifierNameSyntax target = null;
		if (internalSyntaxToken.Kind == SyntaxKind.ActionRefKeyword)
		{
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			target = ParseIdentifierName();
		}
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.ActionKeyword:
		{
			InternalPropertyListSyntax propertyList3 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value5 = null;
				PageActionProperties.TryGetValue(name.ToUpperInvariant(), out value5);
				return value5;
			});
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers2 = ParseTriggerList(TriggerDefinitions.GetActionTriggerInfo);
			InternalActionBaseSyntax internalSyntaxNode = InternalSyntaxFactory.PageAction(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList3, triggers2, EatToken(SyntaxKind.CloseBraceToken));
			return AnnotateWithActionVersions(internalSyntaxNode, propertyList3, null);
		}
		case SyntaxKind.SystemActionKeyword:
		{
			InternalPropertyListSyntax propertyList2 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value6 = null;
				PageActionProperties.TryGetValue(name.ToUpperInvariant(), out value6);
				return value6;
			});
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetActionTriggerInfo);
			return InternalSyntaxFactory.PageSystemAction(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList2, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.FileUploadActionKeyword:
		{
			InternalPropertyListSyntax propertyList5 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value3 = null;
				PageFileUploadActionProperties.TryGetValue(name.ToUpperInvariant(), out value3);
				return value3;
			});
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers3 = ParseTriggerList(TriggerDefinitions.GetFileUploadActionTriggerInfo);
			return InternalSyntaxFactory.PageFileUploadAction(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList5, triggers3, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageGroupKeyword:
		{
			InternalPropertyListSyntax propertyList6 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value2 = null;
				PageActionGroupProperties.TryGetValue(name.ToUpperInvariant(), out value2);
				return value2;
			});
			InternalSyntaxList<InternalActionBaseSyntax> actions = ParseActionList(disallowGroups: false, areaKind);
			InternalPageActionGroupSyntax internalPageActionGroupSyntax = InternalSyntaxFactory.PageActionGroup(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList6, actions, EatToken(SyntaxKind.CloseBraceToken));
			if (disallowGroups)
			{
				internalPageActionGroupSyntax = AddErrorToFirstToken(internalPageActionGroupSyntax, ErrorCode.ERR_GroupingOfActionsNotAllowed);
			}
			return AnnotateWithActionVersions(internalPageActionGroupSyntax, propertyList6, actions.Node);
		}
		case SyntaxKind.SeparatorKeyword:
		{
			InternalPropertyListSyntax propertyList7 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value = null;
				PageActionSeparatorProperties.TryGetValue(name.ToUpperInvariant(), out value);
				return value;
			});
			return InternalSyntaxFactory.PageActionSeparator(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList7, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.ActionRefKeyword:
		{
			InternalPropertyListSyntax propertyList4 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value4 = null;
				PageActionRefProperties.TryGetValue(name.ToUpperInvariant(), out value4);
				return value4;
			});
			InternalPageActionRefSyntax internalSyntaxNode2 = InternalSyntaxFactory.PageActionRef(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, target, closeParenthesisToken, openBraceToken, propertyList4, EatToken(SyntaxKind.CloseBraceToken));
			internalSyntaxNode2 = AnnotateWithActionV2(internalSyntaxNode2);
			return CheckFeatureAvailability(internalSyntaxNode2, Feature.ActionsV2);
		}
		case SyntaxKind.CustomActionKeyword:
		{
			InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value7 = null;
				PageCustomActionProperties.TryGetValue(name.ToUpperInvariant(), out value7);
				return value7;
			});
			InternalPageCustomActionSyntax node = InternalSyntaxFactory.PageCustomAction(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
			return CheckFeatureAvailability(node, Feature.CustomActions);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static T AnnotateWithActionVersions<T>(T internalSyntaxNode, InternalSyntaxNode? propertyList, InternalSyntaxNode? actions, InternalIdentifierNameSyntax? actionReference = null) where T : InternalSyntaxNode
	{
		if ((propertyList != null && propertyList.HasAnnotations(AnnotationKind.ActionV1)) || (actions != null && actions.HasAnnotations(AnnotationKind.ActionV1)))
		{
			internalSyntaxNode = AnnotateWithActionV1(internalSyntaxNode);
		}
		if ((actions != null && actions.HasAnnotations(AnnotationKind.ActionV2)) || (actionReference != null && SyntaxFacts.GetActionAreaKind(actionReference.Identifier.GetValueText()).IsActionsV2ActionArea()))
		{
			internalSyntaxNode = AnnotateWithActionV2(internalSyntaxNode);
		}
		return internalSyntaxNode;
	}

	private static InternalSyntaxList<T> AnnotateWithActionVersions<T>(InternalSyntaxList<T> internalSyntaxList, bool hasActionV1Node, bool hasActionV2Node) where T : InternalSyntaxNode
	{
		InternalSyntaxNode internalSyntaxNode = internalSyntaxList.Node;
		if (hasActionV1Node)
		{
			internalSyntaxNode = AnnotateWithActionV1(internalSyntaxNode);
		}
		if (hasActionV2Node)
		{
			internalSyntaxNode = AnnotateWithActionV2(internalSyntaxNode);
		}
		return new InternalSyntaxList<T>(internalSyntaxNode);
	}

	private static T AnnotateWithActionV1<T>(T internalSyntaxNode) where T : InternalSyntaxNode
	{
		return internalSyntaxNode.WithAdditionalAnnotationsInternal(new SyntaxAnnotation[1]
		{
			new SyntaxAnnotation(AnnotationKind.ActionV1)
		});
	}

	private static T AnnotateWithActionV2<T>(T internalSyntaxNode) where T : InternalSyntaxNode
	{
		return internalSyntaxNode.WithAdditionalAnnotationsInternal(new SyntaxAnnotation[1]
		{
			new SyntaxAnnotation(AnnotationKind.ActionV2)
		});
	}

	private InternalPageAnalysisViewListSyntax ParsePageAnalysisViews()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageAnalysisViewList)
		{
			return (InternalPageAnalysisViewListSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.AnalysisViewsKeyword))
		{
			return null;
		}
		InternalSyntaxToken analysisViewsKeyword = EatKeywordToken(SyntaxKind.AnalysisViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageAnalysisViewSyntax> analysisViews = ParseAnalysisViewList();
		InternalPageAnalysisViewListSyntax node = InternalSyntaxFactory.PageAnalysisViewList(analysisViewsKeyword, openBraceToken, analysisViews, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.AnalysisViews);
	}

	private InternalSyntaxList<InternalPageAnalysisViewSyntax> ParseAnalysisViewList()
	{
		InternalSyntaxListBuilder<InternalPageAnalysisViewSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalPageAnalysisViewSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.AnalysisViewKeyword))
			{
				internalSyntaxListBuilder.Add(ParseAnalysisView());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageAnalysisViewSyntax ParseAnalysisView()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageAnalysisView)
		{
			return (InternalPageAnalysisViewSyntax)EatNode();
		}
		InternalSyntaxToken analysisViewKeyword = EatKeywordToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageAnalysisViewProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		ParsePageExtensionLayout();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.PageAnalysisView(analysisViewKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	private static PropertyTypeInfo LookupAnyActionProperty(string name)
	{
		return LookupPageActionProperty(name) ?? LookupPageActionRefProperty(name) ?? LookupPageActionGroupProperty(name) ?? LookupPageActionAreaProperty(name);
	}

	private InternalPageExtensionActionListSyntax ParsePageExtensionActions()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ActionsKeyword))
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageExtensionActionList)
		{
			return (InternalPageExtensionActionListSyntax)EatNode();
		}
		InternalSyntaxToken actionsKeyword = EatKeywordToken(SyntaxKind.ActionsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalActionChangeBaseSyntax> changes = ParseChangeActionList();
		return AnnotateWithActionVersions(InternalSyntaxFactory.PageExtensionActionList(actionsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken)), null, changes.Node);
	}

	private InternalSyntaxList<InternalActionChangeBaseSyntax> ParseChangeActionList()
	{
		InternalSyntaxListBuilder<InternalActionChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalActionChangeBaseSyntax>();
		bool flag = false;
		bool flag2 = false;
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageChangeKeyword())
			{
				InternalActionChangeBaseSyntax internalActionChangeBaseSyntax = ParseActionChange();
				if (!flag && internalActionChangeBaseSyntax.HasAnnotations(AnnotationKind.ActionV1))
				{
					flag = true;
				}
				if (!flag2 && internalActionChangeBaseSyntax.HasAnnotations(AnnotationKind.ActionV2))
				{
					flag2 = true;
				}
				internalSyntaxListBuilder.Add(internalActionChangeBaseSyntax);
			}
			return AnnotateWithActionVersions(internalSyntaxListBuilder.ToList(), flag, flag2);
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalActionChangeBaseSyntax ParseActionChange()
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.ActionAddChange || base.CurrentNodeKind == SyntaxKind.ActionModifyChange))
		{
			return (InternalActionChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax internalIdentifierNameSyntax2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalActionBaseSyntax> actions2 = ParseActionList();
			InternalSyntaxToken closeBraceToken2 = EatToken(SyntaxKind.CloseBraceToken);
			return AnnotateWithActionVersions(InternalSyntaxFactory.ActionAddChange(internalSyntaxToken, openParenthesisToken3, internalIdentifierNameSyntax2, closeParenthesisToken2, openBraceToken2, actions2, closeBraceToken2), null, actions2.Node, internalIdentifierNameSyntax2);
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions = ParseCommaSeparatedIdentifierNames(ref startToken);
			return AnnotateWithActionVersions(InternalSyntaxFactory.ActionMoveChange(internalSyntaxToken, openParenthesisToken2, internalIdentifierNameSyntax, startToken, actions, EatToken(SyntaxKind.CloseParenToken)), null, null, internalIdentifierNameSyntax);
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupPageActionProperty(name) ?? LookupPageActionRefProperty(name) ?? LookupPageActionGroupProperty(name) ?? LookupPageActionAreaProperty(name), asExtension: false, deferErrorChecking: true);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetActionExtensionTriggerInfo);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return AnnotateWithActionVersions(InternalSyntaxFactory.ActionModifyChange(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken), propertyList, null);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static PropertyTypeInfo LookupAnyAnalysisViewProperty(string name)
	{
		return LookupPageAnalysisViewProperty(name);
	}

	private InternalPageExtensionAnalysisViewListSyntax ParsePageExtensionAnalysisViews()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.AnalysisViewsKeyword))
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageExtensionAnalysisViewList)
		{
			return (InternalPageExtensionAnalysisViewListSyntax)EatNode();
		}
		InternalSyntaxToken analysisViewsKeyword = EatKeywordToken(SyntaxKind.AnalysisViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalAnalysisViewChangeBaseSyntax> changes = ParseChangeAnalysisViewList();
		InternalPageExtensionAnalysisViewListSyntax node = InternalSyntaxFactory.PageExtensionAnalysisViewList(analysisViewsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.AnalysisViews);
	}

	private InternalSyntaxList<InternalAnalysisViewChangeBaseSyntax> ParseChangeAnalysisViewList()
	{
		InternalSyntaxListBuilder<InternalAnalysisViewChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalAnalysisViewChangeBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageChangeKeyword())
			{
				InternalAnalysisViewChangeBaseSyntax node = ParseAnalysisViewChange();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalAnalysisViewChangeBaseSyntax ParseAnalysisViewChange()
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.AnalysisViewAddChange || base.CurrentNodeKind == SyntaxKind.AnalysisViewModifyChange))
		{
			return (InternalAnalysisViewChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		{
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageAnalysisViewSyntax> analysisViews = ParseAnalysisViewList();
			return InternalSyntaxFactory.AnalysisViewAddChange(internalSyntaxToken, null, null, null, openBraceToken2, analysisViews, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken3 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageAnalysisViewSyntax> analysisViews2 = ParseAnalysisViewList();
			return InternalSyntaxFactory.AnalysisViewAddChange(internalSyntaxToken, openParenthesisToken2, anchor, closeParenthesisToken2, openBraceToken3, analysisViews2, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		{
			InternalSyntaxToken startToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> analysisViews4 = ParseCommaSeparatedIdentifierNames(ref startToken2);
			return InternalSyntaxFactory.AnalysisViewMoveChange(internalSyntaxToken, startToken2, null, null, analysisViews4, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> analysisViews3 = ParseCommaSeparatedIdentifierNames(ref startToken);
			return InternalSyntaxFactory.AnalysisViewMoveChange(internalSyntaxToken, openParenthesisToken3, anchor2, startToken, analysisViews3, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupPageAnalysisViewProperty(name), asExtension: false, deferErrorChecking: true);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return InternalSyntaxFactory.AnalysisViewModifyChange(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static PropertyTypeInfo LookupAnyControlProperty(string name)
	{
		return LookupPageFieldProperty(name) ?? LookupPageGroupProperty(name) ?? LookupPagePartProperty(name) ?? LookupPageAreaProperty(name);
	}

	private InternalPageExtensionLayoutSyntax ParsePageExtensionLayout()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.LayoutKeyword))
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageExtensionLayout)
		{
			return (InternalPageExtensionLayoutSyntax)EatNode();
		}
		InternalSyntaxToken layoutKeyword = EatKeywordToken(SyntaxKind.LayoutKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalControlChangeBaseSyntax> changes = ParseChangeControlList();
		return InternalSyntaxFactory.PageExtensionLayout(layoutKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalControlChangeBaseSyntax> ParseChangeControlList()
	{
		InternalSyntaxListBuilder<InternalControlChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalControlChangeBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageChangeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseControlChange());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalControlChangeBaseSyntax ParseControlChange()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ControlAddChange)
		{
			return (InternalControlChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		switch (internalSyntaxToken.ContextualKind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalControlBaseSyntax> controls2 = ParseControlList();
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return InternalSyntaxFactory.ControlAddChange(internalSyntaxToken, openParenthesisToken3, anchor2, closeParenthesisToken2, openBraceToken2, controls2, closeBraceToken);
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> controls = ParseCommaSeparatedIdentifierNames(ref startToken);
			return InternalSyntaxFactory.ControlMoveChange(internalSyntaxToken, openParenthesisToken2, anchor, startToken, controls, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupPageFieldProperty(name) ?? LookupPageGroupProperty(name) ?? LookupPagePartProperty(name) ?? LookupPageAreaProperty(name), asExtension: false, deferErrorChecking: true);
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetControlExtensionTriggerInfo);
			return InternalSyntaxFactory.ControlModifyChange(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private static PropertyTypeInfo LookupAnyViewProperty(string name)
	{
		return LookupPageViewProperty(name);
	}

	private InternalPageExtensionViewListSyntax ParsePageExtensionViews()
	{
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ViewsKeyword))
		{
			return null;
		}
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageExtensionViewList)
		{
			return (InternalPageExtensionViewListSyntax)EatNode();
		}
		InternalSyntaxToken viewsKeyword = EatKeywordToken(SyntaxKind.ViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalViewChangeBaseSyntax> changes = ParseChangeViewList();
		InternalPageExtensionViewListSyntax node = InternalSyntaxFactory.PageExtensionViewList(viewsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.Views);
	}

	private InternalSyntaxList<InternalViewChangeBaseSyntax> ParseChangeViewList()
	{
		InternalSyntaxListBuilder<InternalViewChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalViewChangeBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageChangeKeyword())
			{
				InternalViewChangeBaseSyntax node = ParseViewChange();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalViewChangeBaseSyntax ParseViewChange()
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.ViewAddChange || base.CurrentNodeKind == SyntaxKind.ViewModifyChange))
		{
			return (InternalViewChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		{
			InternalSyntaxToken openBraceToken2 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageViewSyntax> views = ParseViewList();
			return InternalSyntaxFactory.ViewAddChange(internalSyntaxToken, null, null, null, openBraceToken2, views, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken2 = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken3 = EatToken(SyntaxKind.OpenBraceToken);
			InternalSyntaxList<InternalPageViewSyntax> views2 = ParseViewList();
			return InternalSyntaxFactory.ViewAddChange(internalSyntaxToken, openParenthesisToken2, anchor, closeParenthesisToken2, openBraceToken3, views2, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		{
			InternalSyntaxToken startToken2 = EatToken(SyntaxKind.OpenParenToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views4 = ParseCommaSeparatedIdentifierNames(ref startToken2);
			return InternalSyntaxFactory.ViewMoveChange(internalSyntaxToken, startToken2, null, null, views4, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		{
			InternalSyntaxToken openParenthesisToken3 = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor2 = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> views3 = ParseCommaSeparatedIdentifierNames(ref startToken);
			return InternalSyntaxFactory.ViewMoveChange(internalSyntaxToken, openParenthesisToken3, anchor2, startToken, views3, EatToken(SyntaxKind.CloseParenToken));
		}
		case SyntaxKind.ModifyKeyword:
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name2 = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupPageViewProperty(name), asExtension: false, deferErrorChecking: true);
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return InternalSyntaxFactory.ViewModifyChange(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
		}
	}

	private InternalPageLayoutSyntax ParsePageLayout()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageLayout)
		{
			return (InternalPageLayoutSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.LayoutKeyword))
		{
			return null;
		}
		InternalSyntaxToken layoutKeyword = EatKeywordToken(SyntaxKind.LayoutKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageAreaSyntax> areas = ParseAreas();
		return InternalSyntaxFactory.PageLayout(layoutKeyword, openBraceToken, areas, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalPageAreaSyntax> ParseAreas()
	{
		InternalSyntaxListBuilder<InternalPageAreaSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalPageAreaSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.PageAreaKeyword))
			{
				InternalPageAreaSyntax node = ParseArea();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageAreaSyntax ParseArea()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageArea)
		{
			return (InternalPageAreaSyntax)EatNode();
		}
		InternalSyntaxToken controlKeyword = EatKeywordToken(SyntaxKind.PageAreaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageAreaProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalControlBaseSyntax> controls = ParseControlList();
		return InternalSyntaxFactory.PageArea(controlKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, controls, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalControlBaseSyntax> ParseControlList()
	{
		InternalSyntaxListBuilder<InternalControlBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalControlBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsPageControlOrGroupKeyword())
			{
				internalSyntaxListBuilder.Add(ParseControl());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalControlBaseSyntax ParseControl()
	{
		if (base.IsIncremental && base.CurrentNode.IsKind(SyntaxKind.PageField, SyntaxKind.PageLabel, SyntaxKind.PagePart, SyntaxKind.PageSystemPart, SyntaxKind.PageChartPart, SyntaxKind.PageGroup))
		{
			return (InternalControlBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = null;
		InternalCodeExpressionSyntax expression = null;
		InternalObjectNameOrIdSyntax partName = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = null;
		InternalObjectNameReferenceSyntax controlAddIn = null;
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.FieldKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			expression = ParseInLexerMode(LexerMode.Expression, base.ParseExpression);
			break;
		case SyntaxKind.PagePartKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			partName = ParseObjectReferenceSyntax();
			break;
		case SyntaxKind.PageSystemPartKeyword:
		case SyntaxKind.PageChartPartKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			internalIdentifierNameSyntax = ParseIdentifierName();
			break;
		case SyntaxKind.PageUserControlKeyword:
			semicolonToken = EatToken(SyntaxKind.SemicolonToken);
			controlAddIn = ParseObjectNameReference();
			break;
		}
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		switch (internalSyntaxToken.Kind)
		{
		case SyntaxKind.FieldKeyword:
		{
			InternalPropertyListSyntax propertyList5 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value3 = null;
				PageFieldProperties.TryGetValue(name.ToUpperInvariant(), out value3);
				return value3;
			});
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetControlTriggerInfo);
			return InternalSyntaxFactory.PageField(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList5, triggers, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.LabelKeyword:
		{
			InternalPropertyListSyntax propertyList3 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value5 = null;
				PageLabelProperties.TryGetValue(name.ToUpperInvariant(), out value5);
				return value5;
			});
			return InternalSyntaxFactory.PageLabel(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList3, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PagePartKeyword:
		{
			InternalPropertyListSyntax propertyList4 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value4 = null;
				PagePartProperties.TryGetValue(name.ToUpperInvariant(), out value4);
				return value4;
			});
			return InternalSyntaxFactory.PagePart(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList4, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageSystemPartKeyword:
		{
			InternalPropertyListSyntax propertyList2 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value6 = null;
				PageSystemPartProperties.TryGetValue(name.ToUpperInvariant(), out value6);
				return value6;
			});
			return InternalSyntaxFactory.PageSystemPart(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, internalIdentifierNameSyntax, closeParenthesisToken, openBraceToken, propertyList2, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageChartPartKeyword:
		{
			InternalPropertyListSyntax propertyList6 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value2 = null;
				PageChartPartProperties.TryGetValue(name.ToUpperInvariant(), out value2);
				return value2;
			});
			return InternalSyntaxFactory.PageChartPart(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, internalIdentifierNameSyntax, closeParenthesisToken, openBraceToken, propertyList6, EatToken(SyntaxKind.CloseBraceToken));
		}
		case SyntaxKind.PageUserControlKeyword:
		{
			InternalPropertyListSyntax propertyList7 = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value = null;
				PageUserControlProperties.TryGetValue(name.ToUpperInvariant(), out value);
				return value;
			});
			InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers2 = ParseTriggerList();
			return InternalSyntaxFactory.PageUserControl(internalSyntaxToken, openParenthesisToken, name2, semicolonToken, controlAddIn, closeParenthesisToken, openBraceToken, propertyList7, triggers2, EatToken(SyntaxKind.CloseBraceToken));
		}
		default:
		{
			InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
			{
				PropertyTypeInfo value7 = null;
				PageGroupProperties.TryGetValue(name.ToUpperInvariant(), out value7);
				return value7;
			});
			InternalSyntaxList<InternalControlBaseSyntax> controls = ParseControlList();
			InternalPageActionListSyntax internalPageActionListSyntax = ParsePageActions();
			if (internalPageActionListSyntax != null && !SyntaxFacts.HasGroupActions(internalSyntaxToken.Kind))
			{
				internalPageActionListSyntax = AddErrorToFirstToken(internalPageActionListSyntax, ErrorCode.ERR_ActionsAreNotAllowedOnThisControlType);
			}
			return InternalSyntaxFactory.PageGroup(internalSyntaxToken, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, controls, internalPageActionListSyntax, EatToken(SyntaxKind.CloseBraceToken));
		}
		}
	}

	private InternalPageViewListSyntax ParsePageViews()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageViewList)
		{
			return (InternalPageViewListSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.ViewsKeyword))
		{
			return null;
		}
		InternalSyntaxToken viewsKeyword = EatKeywordToken(SyntaxKind.ViewsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalPageViewSyntax> views = ParseViewList();
		InternalPageViewListSyntax node = InternalSyntaxFactory.PageViewList(viewsKeyword, openBraceToken, views, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.Views);
	}

	private InternalSyntaxList<InternalPageViewSyntax> ParseViewList()
	{
		InternalSyntaxListBuilder<InternalPageViewSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalPageViewSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.ViewKeyword))
			{
				internalSyntaxListBuilder.Add(ParseView());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalPageViewSyntax ParseView()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PageView)
		{
			return (InternalPageViewSyntax)EatNode();
		}
		InternalSyntaxToken viewKeyword = EatKeywordToken();
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageViewProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.PageView(viewKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, layout, closeBraceToken);
	}

	private InternalPermissionSetSyntax ParsePermissionSet()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PermissionSet)
		{
			return (InternalPermissionSetSyntax)EatNode();
		}
		InternalSyntaxToken permissionSetToken = EatKeywordToken(SyntaxKind.PermissionSetKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing PermissionSet {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPermissionSetSyntax node = InternalSyntaxFactory.PermissionSet(propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PermissionSetProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), closeBraceToken: ParseCloseBraceToken(openBraceToken), permissionSetToken: permissionSetToken, objectId: internalObjectIdSyntax, name: internalIdentifierNameSyntax, openBraceToken: openBraceToken, members: null);
		return CheckFeatureAvailability(node, Feature.PermissionSet);
	}

	private InternalPermissionSetExtensionSyntax ParsePermissionSetExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.PermissionSetExtension)
		{
			return (InternalPermissionSetExtensionSyntax)EatNode();
		}
		InternalSyntaxToken permissionSetExtensionKeyword = EatKeywordToken(SyntaxKind.PermissionSetExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing PermissionSetExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax(disallowIds: true);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPermissionSetExtensionSyntax node = InternalSyntaxFactory.PermissionSetExtension(propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PermissionSetExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), closeBraceToken: ParseCloseBraceToken(openBraceToken), permissionSetExtensionKeyword: permissionSetExtensionKeyword, objectId: internalObjectIdSyntax, name: internalIdentifierNameSyntax, extendsKeyword: extendsKeyword, baseObject: baseObject, openBraceToken: openBraceToken, members: null);
		return CheckFeatureAvailability(node, Feature.PermissionSet);
	}

	private InternalProfileSyntax ParseProfile()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ProfileObject)
		{
			return (InternalProfileSyntax)EatNode();
		}
		InternalSyntaxToken profileToken = EatKeywordToken(SyntaxKind.ProfileKeyword);
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Profile {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ProfileProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.Profile(profileToken, internalIdentifierNameSyntax, openBraceToken, propertyList, default(InternalSyntaxList<InternalMemberSyntax>), closeBraceToken);
	}

	private InternalProfileExtensionSyntax ParseProfileExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ProfileExtensionObject)
		{
			return (InternalProfileExtensionSyntax)EatNode();
		}
		InternalSyntaxToken profileExtensionKeyword = EatKeywordToken(SyntaxKind.ProfileExtensionKeyword);
		InternalObjectIdSyntax objectId = null;
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing ProfileExtension {0}.", internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ProfileProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		InternalProfileExtensionSyntax node = InternalSyntaxFactory.ProfileExtension(profileExtensionKeyword, objectId, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, default(InternalSyntaxList<InternalMemberSyntax>), closeBraceToken);
		return CheckFeatureAvailability(node, Feature.ProfileExtensions);
	}

	internal InternalPropertyValueSyntax ParseMultilanguagePropertyValue(PropertyTypeInfo propertyTypeInfo, ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.MultilanguagePropertyValue(ParseCommaSeparatedIdentifierEqualsStringList(ref equalsToken));
	}

	internal InternalPropertyValueSyntax ParseLabelPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.LabelPropertyValue(ParseLabel());
	}

	internal InternalPropertyValueSyntax ParseInt32LiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.Int32PropertyValue(ParseInt32LiteralValue());
	}

	internal InternalPropertyValueSyntax ParseStringLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.StringPropertyValue(ParseStringLiteralValue());
	}

	internal InternalTimePropertyValueSyntax ParseTimeLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.TimePropertyValue(ParseTimeLiteralValue());
	}

	internal InternalDatePropertyValueSyntax ParseDateLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.DatePropertyValue(ParseDateLiteralValue());
	}

	internal InternalDateTimePropertyValueSyntax ParseDateTimeLiterlaPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.DateTimePropertyValue(ParseDateTimeLiteralValue());
	}

	internal InternalPropertyValueSyntax ParseBooleanPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.BooleanPropertyValue(ParseBooleanLiteralValue());
	}

	internal InternalPropertyValueSyntax ParseStyleExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(delegate(InternalCodeExpressionSyntax expression)
		{
			int hash;
			InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(495, expression, out hash);
			if (internalSyntaxNode != null)
			{
				return (InternalStyleExpressionPropertyValueSyntax)internalSyntaxNode;
			}
			InternalStyleExpressionPropertyValueSyntax internalStyleExpressionPropertyValueSyntax = new InternalStyleExpressionPropertyValueSyntax(SyntaxKind.StyleExpressionPropertyValue, expression);
			if (hash >= 0)
			{
				SyntaxNodeCache.AddNode(internalStyleExpressionPropertyValueSyntax, hash);
			}
			return internalStyleExpressionPropertyValueSyntax;
		});
	}

	internal InternalPropertyValueSyntax ParseIntegerExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(delegate(InternalCodeExpressionSyntax expression)
		{
			int hash;
			InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(487, expression, out hash);
			if (internalSyntaxNode != null)
			{
				return (InternalIntegerExpressionPropertyValueSyntax)internalSyntaxNode;
			}
			InternalIntegerExpressionPropertyValueSyntax internalIntegerExpressionPropertyValueSyntax = new InternalIntegerExpressionPropertyValueSyntax(SyntaxKind.IntegerExpressionPropertyValue, expression);
			if (hash >= 0)
			{
				SyntaxNodeCache.AddNode(internalIntegerExpressionPropertyValueSyntax, hash);
			}
			return internalIntegerExpressionPropertyValueSyntax;
		});
	}

	internal InternalPropertyValueSyntax ParseObjectReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.ObjectReferencePropertyValue(ParseObjectReferenceSyntax());
	}

	internal InternalPropertyValueSyntax ParseEnumLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		EnumPropertyTypeInfo enumPropertyTypeInfo = (EnumPropertyTypeInfo)propertyTypeInfo;
		if (!internalIdentifierNameSyntax.IsMissing)
		{
			return InternalSyntaxFactory.EnumPropertyValue(internalIdentifierNameSyntax);
		}
		return InternalSyntaxFactory.InvalidPropertyValue(AddError(internalIdentifierNameSyntax.GetFirstToken(), ErrorCode.ERR_InvalidPropertyOptionValue, propertyTypeInfo.Name, GetEnumOptionsDiagnosticText(enumPropertyTypeInfo)));
	}

	internal InternalPropertyValueSyntax ParsePermissionPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken equalsToken)
	{
		if (pti.Kind == PropertyKind.AccessByPermission)
		{
			return ParseAccessByPermission();
		}
		return ParsePermissionList(ref equalsToken);
	}

	internal InternalPropertyValueSyntax ParsePermissionSetPermissionListPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.PermissionPropertyValue(ParseSeparatedList(ref equalsToken, SyntaxKind.TableDataKeyword, (InternalSyntaxToken token) => token.ContextualKind.IsSupportedPermissionKeyword(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, delegate(ref InternalSyntaxToken firstToken, InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref firstToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected == SyntaxKind.TableDataKeyword && !p.CurrentToken.ContextualKind.IsSupportedPermissionKeyword()), (ParserBase p) => IsNotAPermissionToken(p.CurrentToken.Kind) || p.IsTerminator(), expected);
		}, () => ParsePermissionSyntax(disallowIds: true, allowAll: true)));
		static bool IsNotAPermissionToken(SyntaxKind kind)
		{
			if (kind == SyntaxKind.SemicolonToken || kind - 59 <= SyntaxKind.EmptyToken)
			{
				return true;
			}
			return false;
		}
	}

	internal InternalPropertyValueSyntax ParseInherentEntitlementsPropertyValue(PropertyTypeInfo pti)
	{
		InternalSyntaxToken token = ParseIdentifierToken();
		token = ValidatePermissionValue(token);
		return InternalSyntaxFactory.InherentEntitlementsPropertyValue(token);
	}

	internal InternalPropertyValueSyntax ParseInherentPermissionsPropertyValue(PropertyTypeInfo pti)
	{
		InternalSyntaxToken token = ParseIdentifierToken();
		token = ValidatePermissionValue(token);
		return InternalSyntaxFactory.InherentPermissionsPropertyValue(token);
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.CommaSeparatedPropertyValue(ParseCommaSeparatedIdentifierNames(ref startToken));
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedObjectNameReferencesPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.CommaSeparatedObjectNameReferencesPropertyValue(ParseCommaSeparatedObjectNameReferences(ref startToken));
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedIdentifierEqualsStringListPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringListPropertyValue(ParseCommaSeparatedIdentifierEqualsStringList(ref startToken));
	}

	internal InternalPropertyValueSyntax ParseCommaSeparatedStringsPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.CommaSeparatedStringsPropertyValue(ParseCommaSeparatedStrings(ref startToken));
	}

	private InternalSeparatedSyntaxList<InternalStringLiteralValueSyntax> ParseCommaSeparatedStrings(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.StringLiteralToken, (InternalSyntaxToken token) => token.Kind == SyntaxKind.StringLiteralToken, SyntaxKind.CommaToken, closeTokenKind, base.SkipBadCommaSeparatedStringLiteralToken, base.ParseStringLiteralValue);
	}

	private InternalCommaSeparatedIdentifierEqualsStringListSyntax ParseCommaSeparatedIdentifierEqualsStringList(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringList(ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, base.SkipBadCommaSeparatedToken, delegate
		{
			if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsString)
			{
				return (InternalIdentifierEqualsStringSyntax)EatNode();
			}
			InternalSyntaxToken identifier = ParseIdentifierToken();
			InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
			InternalStringLiteralValueSyntax stringLiteral = ParseStringLiteralValue();
			return InternalSyntaxFactory.IdentifierEqualsString(identifier, equalsToken, stringLiteral);
		}));
	}

	internal InternalPropertyValueSyntax ParseQueryDataItemLinkPropertyValue(PropertyTypeInfo info, ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.QueryDataItemLinkPropertyValue(ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, base.SkipBadCommaSeparatedToken, delegate
		{
			if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.QueryDataItemLinkExpression)
			{
				return (InternalQueryDataItemLinkExpressionSyntax)EatNode();
			}
			InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
			InternalSyntaxToken equalsToken2 = EatToken(SyntaxKind.EqualsToken);
			InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseMemberAccessExpressionOrIdentifier();
			if (internalCodeExpressionSyntax.Kind != SyntaxKind.MemberAccessExpression)
			{
				internalCodeExpressionSyntax = InternalSyntaxFactory.MemberAccessExpression(internalCodeExpressionSyntax, CreateMissingToken(SyntaxKind.DotToken, internalCodeExpressionSyntax.Kind, reportError: false), ParserBase.CreateMissingIdentifierName());
			}
			return InternalSyntaxFactory.QueryDataItemLinkExpression(sourceField, equalsToken2, internalCodeExpressionSyntax as InternalMemberAccessExpressionSyntax);
		}));
	}

	internal InternalQueryDataItemLinkExpressionSyntax ParseQueryDataItemLinkExpression()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.QueryDataItemLinkExpression)
		{
			return (InternalQueryDataItemLinkExpressionSyntax)EatNode();
		}
		InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseMemberAccessExpressionOrIdentifier();
		if (internalCodeExpressionSyntax.Kind != SyntaxKind.MemberAccessExpression)
		{
			internalCodeExpressionSyntax = InternalSyntaxFactory.MemberAccessExpression(internalCodeExpressionSyntax, CreateMissingToken(SyntaxKind.DotToken, internalCodeExpressionSyntax.Kind, reportError: false), ParserBase.CreateMissingIdentifierName());
		}
		return InternalSyntaxFactory.QueryDataItemLinkExpression(sourceField, equalsToken, internalCodeExpressionSyntax as InternalMemberAccessExpressionSyntax);
	}

	private InternalPropertyValueSyntax ParseExpressionPropertyValue(Func<InternalCodeExpressionSyntax, InternalPropertyValueSyntax> func)
	{
		return func(ParseInLexerMode(LexerMode.Expression, base.ParseExpression));
	}

	internal InternalPropertyValueSyntax ParseTextExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(delegate(InternalCodeExpressionSyntax expression)
		{
			int hash;
			InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(488, expression, out hash);
			if (internalSyntaxNode != null)
			{
				return (InternalTextExpressionPropertyValueSyntax)internalSyntaxNode;
			}
			InternalTextExpressionPropertyValueSyntax internalTextExpressionPropertyValueSyntax = new InternalTextExpressionPropertyValueSyntax(SyntaxKind.TextExpressionPropertyValue, expression);
			if (hash >= 0)
			{
				SyntaxNodeCache.AddNode(internalTextExpressionPropertyValueSyntax, hash);
			}
			return internalTextExpressionPropertyValueSyntax;
		});
	}

	internal InternalPropertyValueSyntax ParseClientSideBooleanExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(delegate(InternalCodeExpressionSyntax expression)
		{
			int hash;
			InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(486, expression, out hash);
			if (internalSyntaxNode != null)
			{
				return (InternalClientSideBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
			}
			InternalClientSideBooleanExpressionPropertyValueSyntax internalClientSideBooleanExpressionPropertyValueSyntax = new InternalClientSideBooleanExpressionPropertyValueSyntax(SyntaxKind.ClientSideBooleanExpressionPropertyValue, expression);
			if (hash >= 0)
			{
				SyntaxNodeCache.AddNode(internalClientSideBooleanExpressionPropertyValueSyntax, hash);
			}
			return internalClientSideBooleanExpressionPropertyValueSyntax;
		});
	}

	internal InternalPropertyValueSyntax ParseBooleanExpressionPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return ParseExpressionPropertyValue(delegate(InternalCodeExpressionSyntax expression)
		{
			int hash;
			InternalSyntaxNode internalSyntaxNode = SyntaxNodeCache.TryGetNode(485, expression, out hash);
			if (internalSyntaxNode != null)
			{
				return (InternalBooleanExpressionPropertyValueSyntax)internalSyntaxNode;
			}
			InternalBooleanExpressionPropertyValueSyntax internalBooleanExpressionPropertyValueSyntax = new InternalBooleanExpressionPropertyValueSyntax(SyntaxKind.BooleanExpressionPropertyValue, expression);
			if (hash >= 0)
			{
				SyntaxNodeCache.AddNode(internalBooleanExpressionPropertyValueSyntax, hash);
			}
			return internalBooleanExpressionPropertyValueSyntax;
		});
	}

	internal InternalPropertyValueSyntax ParseQualifiedObjectReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSyntaxToken internalSyntaxToken = null;
		switch (base.CurrentToken.ContextualKind)
		{
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.XmlPortKeyword:
			return InternalSyntaxFactory.QualifiedObjectReferencePropertyValue(EatKeywordToken(), ParseObjectReferenceSyntax());
		case SyntaxKind.QueryKeyword:
			if ((internalSyntaxToken = CheckFeatureAvailability(base.CurrentToken, Feature.RunQueryObject)) == base.CurrentToken)
			{
				return InternalSyntaxFactory.QualifiedObjectReferencePropertyValue(EatKeywordToken(), ParseObjectReferenceSyntax());
			}
			break;
		}
		if (internalSyntaxToken == null)
		{
			internalSyntaxToken = AddError(CreateMissingToken(SyntaxKind.CodeunitKeyword, base.CurrentToken.Kind, reportError: false), ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedApplicationObjectReferencesString);
		}
		return InternalSyntaxFactory.InvalidPropertyValue(internalSyntaxToken);
	}

	private InternalPropertyValueSyntax ParsePageFieldReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.PageFieldReferencePropertyValue(ParseIdentifierName());
	}

	internal InternalPropertyValueSyntax ParseReportDataItemLinkPropertyValue(PropertyTypeInfo info, ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.ReportDataItemLinkPropertyValue(ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, base.SkipBadCommaSeparatedToken, delegate
		{
			if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportDataItemLinkExpression)
			{
				return (InternalReportDataItemLinkExpressionSyntax)EatNode();
			}
			InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
			InternalSyntaxToken equalsToken2 = EatToken(SyntaxKind.EqualsToken);
			InternalSyntaxToken fieldKeywordToken = EatKeywordToken(SyntaxKind.FieldFormulaKeyword);
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax relatedField = ParseIdentifierName();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			return InternalSyntaxFactory.ReportDataItemLinkExpression(sourceField, equalsToken2, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
		}));
	}

	internal InternalCommaSeparatedIdentifierEqualsIdentifierListPropertyValueSyntax ParseCommaSeparatedIdentifierEqualsIdentifierListPropertyValue(PropertyTypeInfo info, ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsIdentifierListPropertyValue(ParseCommaSeparatedIdentifierEqualsIdentifierList(ref equalsToken));
	}

	private InternalCommaSeparatedIdentifierEqualsIdentifierListSyntax ParseCommaSeparatedIdentifierEqualsIdentifierList(ref InternalSyntaxToken equalsToken)
	{
		return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsIdentifierList(ParseSeparatedList(ref equalsToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, base.SkipBadCommaSeparatedToken, delegate
		{
			if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsIdentifier)
			{
				return (InternalIdentifierEqualsIdentifierSyntax)EatNode();
			}
			bool flag = IsFeatureEnabled(Feature.Namespaces);
			InternalNameSyntax leftIdentifier = ParseQualifiedName(!flag);
			InternalSyntaxToken equalsToken2 = EatToken(SyntaxKind.EqualsToken);
			InternalNameSyntax rightIdentifier = ParseQualifiedName(!flag);
			return InternalSyntaxFactory.IdentifierEqualsIdentifier(leftIdentifier, equalsToken2, rightIdentifier);
		}));
	}

	internal InternalReportDataItemLinkExpressionSyntax ParseReportDataItemLinkExpression()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportDataItemLinkExpression)
		{
			return (InternalReportDataItemLinkExpressionSyntax)EatNode();
		}
		InternalIdentifierNameSyntax sourceField = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken fieldKeywordToken = EatKeywordToken(SyntaxKind.FieldFormulaKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax relatedField = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.ReportDataItemLinkExpression(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
	}

	internal InternalIdentifierEqualsIdentifierSyntax ParseIdentifierEqualsIdentifierExpression()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IdentifierEqualsIdentifier)
		{
			return (InternalIdentifierEqualsIdentifierSyntax)EatNode();
		}
		bool flag = IsFeatureEnabled(Feature.Namespaces);
		InternalNameSyntax leftIdentifier = ParseQualifiedName(!flag);
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalNameSyntax rightIdentifier = ParseQualifiedName(!flag);
		return InternalSyntaxFactory.IdentifierEqualsIdentifier(leftIdentifier, equalsToken, rightIdentifier);
	}

	internal InternalPropertyValueSyntax ParseShortcutPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		throw ExceptionUtilities.SkippedForNow;
	}

	internal InternalPropertyValueSyntax ParseTableFilterPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.TableFilterPropertyValue(ParseTableRelationConditionValue());
	}

	internal InternalPropertyValueSyntax ParseOptionValuesPropertyValue(PropertyTypeInfo pti, ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.OptionValuesPropertyValue(ParseOptionMembersSyntax(ref startToken));
	}

	internal InternalPropertyValueSyntax ParseDecimalPlacesPropertyValue(PropertyTypeInfo pti)
	{
		return InternalSyntaxFactory.DecimalPlacesPropertyValue(ParseDecimalPlacesSyntax());
	}

	internal static PermissionValidationStatus IsValidPermissionValue(bool isTableData, string permissionValue, bool isInherentObjectPermission)
	{
		if (isInherentObjectPermission && isTableData)
		{
			if (IsRimdxExpected(permissionValue))
			{
				return PermissionValidationStatus.ExpectedRIMDX;
			}
			return PermissionValidationStatus.Valid;
		}
		if (isTableData && permissionValue.Length > 4)
		{
			return PermissionValidationStatus.ExpectedRIMD;
		}
		if (!isTableData)
		{
			if (permissionValue.Length != 1 || permissionValue[0] != 'X')
			{
				return PermissionValidationStatus.ExpectedX;
			}
			return PermissionValidationStatus.Valid;
		}
		if (IsRimdExpected(permissionValue))
		{
			return PermissionValidationStatus.ExpectedRIMD;
		}
		return PermissionValidationStatus.Valid;
	}

	internal static bool IsRimdExpected(string permissionValue)
	{
		for (int i = 0; i < permissionValue.Length; i++)
		{
			char c = permissionValue[i];
			if (c != 'R' && c != 'I' && c != 'M' && c != 'D')
			{
				return true;
			}
			for (int j = i + 1; j < permissionValue.Length; j++)
			{
				if (permissionValue[j] == c)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool IsRimdxExpected(string permissionValue)
	{
		for (int i = 0; i < permissionValue.Length; i++)
		{
			char c = permissionValue[i];
			if (c != 'R' && c != 'I' && c != 'M' && c != 'D' && c != 'X')
			{
				return true;
			}
			for (int j = i + 1; j < permissionValue.Length; j++)
			{
				if (permissionValue[j] == c)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static string GetEnumOptionsDiagnosticText(EnumPropertyTypeInfo enumPropertyTypeInfo)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		ImmutableArray<EnumPropertyMemberInfo>.Enumerator enumerator = enumPropertyTypeInfo.Options.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EnumPropertyMemberInfo current = enumerator.Current;
			if (instance.Length > 0)
			{
				instance.Builder.Append(',');
			}
			if (instance.Length > 80)
			{
				instance.Builder.Append("...");
				break;
			}
			instance.Builder.Append(current.Name);
		}
		return instance.ToStringAndFree();
	}

	private InternalPropertyValueSyntax ParseAccessByPermission()
	{
		InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> internalSeparatedSyntaxListBuilder = base.Pool.AllocateSeparated<InternalPermissionSyntax>();
		try
		{
			InternalPermissionSyntax node = ParsePermissionSyntax();
			internalSeparatedSyntaxListBuilder.Add(node);
			return InternalSyntaxFactory.PermissionPropertyValue(internalSeparatedSyntaxListBuilder);
		}
		finally
		{
			base.Pool.Free(internalSeparatedSyntaxListBuilder);
		}
	}

	private InternalPropertyValueSyntax ParsePermissionList(ref InternalSyntaxToken startToken)
	{
		return InternalSyntaxFactory.PermissionPropertyValue(ParseSeparatedList(ref startToken, SyntaxKind.TableDataKeyword, (InternalSyntaxToken token) => token.IsKeywordKind(SyntaxKind.TableDataKeyword), SyntaxKind.CommaToken, SyntaxKind.SemicolonToken, delegate(ref InternalSyntaxToken firstToken, InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> list, SyntaxKind expected)
		{
			return SkipBadSeparatedListTokensWithExpectedKind(ref firstToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected == SyntaxKind.TableDataKeyword && !p.CurrentToken.IsKeywordKind(SyntaxKind.TableDataKeyword)), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
		}, () => ParsePermissionSyntax()));
	}

	private InternalDecimalPlacesSyntax ParseDecimalPlacesSyntax()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.DecimalPlaces)
		{
			return (InternalDecimalPlacesSyntax)EatNode();
		}
		InternalSyntaxToken minimumToken = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken colonToken = null;
		InternalSyntaxToken maximumToken = null;
		if (base.CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			colonToken = EatToken();
			if (base.CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
			{
				maximumToken = EatToken();
			}
		}
		return InternalSyntaxFactory.DecimalPlaces(minimumToken, colonToken, maximumToken);
	}

	private InternalCodeExpressionSyntax ParseMemberAccessExpressionOrIdentifier()
	{
		if (!base.CurrentToken.Kind.IsTokenIdentifier())
		{
			return AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected));
		}
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		InternalSyntaxToken internalSyntaxToken = EatTokenIfKind(SyntaxKind.DotToken);
		if (internalSyntaxToken != null)
		{
			InternalIdentifierNameSyntax internalIdentifierNameSyntax2 = null;
			internalIdentifierNameSyntax2 = (base.CurrentToken.Kind.IsTokenIdentifier() ? ParseIdentifierName() : AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected)));
			return InternalSyntaxFactory.MemberAccessExpression(internalIdentifierNameSyntax, internalSyntaxToken, internalIdentifierNameSyntax2);
		}
		return internalIdentifierNameSyntax;
	}

	private InternalIdentifierOrLiteralExpressionSyntax ParseIdentifierOrLiteralExpression()
	{
		InternalIdentifierOrLiteralExpressionSyntax internalIdentifierOrLiteralExpressionSyntax = TryParseIdentifierOrLiteralExpression();
		if (internalIdentifierOrLiteralExpressionSyntax != null)
		{
			return internalIdentifierOrLiteralExpressionSyntax;
		}
		return InternalSyntaxFactory.IdentifierOrLiteralExpression(InternalSyntaxFactory.IdentifierName(AddError(ParserBase.CreateMissingIdentifierToken(), ErrorCode.ERR_ExpectedIdentifierOrLiteral)));
	}

	private InternalIdentifierOrLiteralExpressionSyntax? TryParseIdentifierOrLiteralExpression()
	{
		InternalSyntaxToken token = base.CurrentToken;
		InternalSyntaxToken internalSyntaxToken = ParseIdentifierToken(allowMissingIdentifier: true);
		if (internalSyntaxToken != null)
		{
			return InternalSyntaxFactory.IdentifierOrLiteralExpression(InternalSyntaxFactory.IdentifierName(internalSyntaxToken));
		}
		if (token.IsPossibleSignedLiteralToken())
		{
			return InternalSyntaxFactory.IdentifierOrLiteralExpression(InternalSyntaxFactory.LiteralExpression(ParseLiteralValue()));
		}
		return null;
	}

	private InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax ParseIdentifierOrLiteralOrOptionAccessExpression()
	{
		InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax internalIdentifierOrLiteralOrOptionAccessExpressionSyntax = TryParseIdentifierOrLiteralOrOptionAccessExpression();
		if (internalIdentifierOrLiteralOrOptionAccessExpressionSyntax != null)
		{
			return internalIdentifierOrLiteralOrOptionAccessExpressionSyntax;
		}
		ErrorCode code = ((base.Options.RuntimeVersion >= Feature.OptionAccessInFormulas.RequiredVersion()) ? ErrorCode.ERR_ExpectedIdentifierOrLiteralOrOptionAccess : ErrorCode.ERR_ExpectedIdentifierOrLiteral);
		return InternalSyntaxFactory.IdentifierOrLiteralOrOptionAccessExpression(InternalSyntaxFactory.IdentifierName(AddError(ParserBase.CreateMissingIdentifierToken(), code)));
	}

	private InternalIdentifierOrLiteralOrOptionAccessExpressionSyntax? TryParseIdentifierOrLiteralOrOptionAccessExpression()
	{
		InternalSyntaxToken token = base.CurrentToken;
		InternalSyntaxToken internalSyntaxToken = ParseIdentifierToken(allowMissingIdentifier: true);
		if (internalSyntaxToken != null)
		{
			InternalIdentifierNameSyntax internalIdentifierNameSyntax = InternalSyntaxFactory.IdentifierName(internalSyntaxToken);
			if (base.CurrentToken.Kind == SyntaxKind.ColonColonToken)
			{
				InternalOptionAccessExpressionSyntax node = ParseOptionAccess(internalIdentifierNameSyntax);
				node = CheckFeatureAvailability(node, Feature.OptionAccessInFormulas);
				return InternalSyntaxFactory.IdentifierOrLiteralOrOptionAccessExpression(node);
			}
			return InternalSyntaxFactory.IdentifierOrLiteralOrOptionAccessExpression(internalIdentifierNameSyntax);
		}
		if (token.IsPossibleSignedLiteralToken())
		{
			return InternalSyntaxFactory.IdentifierOrLiteralOrOptionAccessExpression(InternalSyntaxFactory.LiteralExpression(ParseLiteralValue()));
		}
		return null;
	}

	private InternalPermissionSyntax ParsePermissionSyntax(bool disallowIds = false, bool allowAll = false)
	{
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken(SyntaxFacts.SupportedPermissionKeywordKinds, ErrorCode.ERR_ExpectedApplicationObjectKeyword, SyntaxFacts.SupportedPermissionApplicationObjectsString);
		InternalSyntaxToken asteriskToken = null;
		InternalObjectNameOrIdSyntax objectReference = null;
		if (base.CurrentToken.IsKind(SyntaxKind.MultiplyToken) && allowAll)
		{
			asteriskToken = EatToken();
		}
		else
		{
			objectReference = ParseObjectReferenceSyntax(disallowIds);
		}
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalSyntaxToken permissionValues = GetPermissionValues(internalSyntaxToken.IsKeywordKind(SyntaxKind.TableDataKeyword));
		return InternalSyntaxFactory.Permission(internalSyntaxToken, objectReference, asteriskToken, equalsToken, permissionValues);
	}

	private InternalSyntaxToken? GetPermissionValues(bool isTableData)
	{
		InternalSyntaxToken token = ParseIdentifierToken();
		return GetPermissionValuesTokenWithError(isTableData, token);
	}

	private InternalSyntaxToken? GetPermissionValuesTokenWithError(bool isTableData, InternalSyntaxToken token)
	{
		string text = token.Text.ToUpperInvariant();
		if (text.Length == 0)
		{
			return null;
		}
		switch (IsValidPermissionValue(isTableData, text, isInherentObjectPermission: false))
		{
		case PermissionValidationStatus.ExpectedRIMD:
			token = AddError(token, ErrorCode.ERR_InvalidPermissionValue, "RIMD");
			break;
		case PermissionValidationStatus.ExpectedX:
			token = AddError(token, ErrorCode.ERR_InvalidPermissionValue, "X");
			break;
		}
		return token;
	}

	private PostSkipAction SkipBadPermissionSyntaxToken(ref InternalSyntaxToken firstToken, InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref firstToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected == SyntaxKind.TableDataKeyword && !p.CurrentToken.IsKeywordKind(SyntaxKind.TableDataKeyword)), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.SemicolonToken || p.IsTerminator(), expected);
	}

	private PostSkipAction SkipBadPermissionSetPermissionListSyntaxToken(ref InternalSyntaxToken firstToken, InternalSeparatedSyntaxListBuilder<InternalPermissionSyntax> list, SyntaxKind expected)
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref firstToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected == SyntaxKind.TableDataKeyword && !p.CurrentToken.ContextualKind.IsSupportedPermissionKeyword()), (ParserBase p) => IsNotAPermissionToken(p.CurrentToken.Kind) || p.IsTerminator(), expected);
		static bool IsNotAPermissionToken(SyntaxKind kind)
		{
			if (kind == SyntaxKind.SemicolonToken || kind - 59 <= SyntaxKind.EmptyToken)
			{
				return true;
			}
			return false;
		}
	}

	private InternalPropertyValueSyntax ParseLiteralPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		switch (base.CurrentToken.ContextualKind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.MinusToken:
			return ParseNumericPropertyValue();
		case SyntaxKind.StringLiteralToken:
			return ParseStringLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.TimeLiteralToken:
			return ParseTimeLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.DateLiteralToken:
			return ParseDateLiteralPropertyValue(propertyTypeInfo);
		case SyntaxKind.DateTimeLiteralToken:
			return ParseDateTimeLiterlaPropertyValue(propertyTypeInfo);
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
			return ParseBooleanPropertyValue(propertyTypeInfo);
		case SyntaxKind.IdentifierToken:
			return ParseOptionValuePropertyValue();
		default:
			return InternalSyntaxFactory.InvalidPropertyValue(AddError(CreateMissingToken(SyntaxKind.StringLiteralToken, base.CurrentToken.Kind, reportError: false), ErrorCode.ERR_UnexpectedToken, propertyTypeInfo.Name));
		}
	}

	private InternalPropertyValueSyntax ParseImagePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.ImagePropertyValue(ParseImagePropertyValueName());
	}

	private InternalIdentifierNameSyntax ParseImagePropertyValueName()
	{
		if (base.CurrentToken.Kind == SyntaxKind.FilterFormulaKeyword)
		{
			return InternalSyntaxFactory.IdentifierName(InternalSyntaxFactory.Identifier(EatToken().Text));
		}
		return ParseIdentifierName();
	}

	private InternalPropertyValueSyntax ParseMemberReferencePropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.MemberReferencePropertyValue(ParseIdentifierName());
	}

	private InternalPropertyValueSyntax ParseOptionValuePropertyValue()
	{
		return InternalSyntaxFactory.OptionValuePropertyValue(ParseIdentifierName());
	}

	private InternalPropertyValueSyntax ParseNumericPropertyValue()
	{
		InternalLiteralValueSyntax internalLiteralValueSyntax = ParseNumericLiteralValue();
		return internalLiteralValueSyntax.Kind switch
		{
			SyntaxKind.Int32SignedLiteralValue => InternalSyntaxFactory.Int32PropertyValue((InternalInt32SignedLiteralValueSyntax)internalLiteralValueSyntax), 
			SyntaxKind.Int64SignedLiteralValue => InternalSyntaxFactory.Int64PropertyValue((InternalInt64SignedLiteralValueSyntax)internalLiteralValueSyntax), 
			SyntaxKind.DecimalSignedLiteralValue => InternalSyntaxFactory.DecimalPropertyValue((InternalDecimalSignedLiteralValueSyntax)internalLiteralValueSyntax), 
			_ => throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.Kind), 
		};
	}

	internal InternalSeparatedSyntaxList<InternalObjectNameReferenceSyntax> ParseCommaSeparatedObjectNameReferences(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, base.SkipBadCommaSeparatedToken, base.ParseObjectNameReference);
	}

	private InternalSyntaxToken ValidatePermissionValue(InternalSyntaxToken token)
	{
		if (IsRimdxExpected(token.Text.ToUpperInvariant()))
		{
			return AddError(token, ErrorCode.ERR_IdentifierIsNotAPermissionValue, token.Text);
		}
		return token;
	}

	private InternalQuerySyntax ParseQuery()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.QueryObject)
		{
			return (InternalQuerySyntax)EatNode();
		}
		InternalSyntaxToken queryKeyword = EatKeywordToken(SyntaxKind.QueryKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Query {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			QueryProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalQueryElementsSyntax elements = ParseQueryElements();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetQueryTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.Query(queryKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, elements, members, closeBraceToken);
	}

	private InternalQueryElementsSyntax ParseQueryElements()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.QueryElements)
		{
			return (InternalQueryElementsSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.QueryElementsKeyword))
		{
			return null;
		}
		InternalSyntaxToken queryElementsKeyword = EatKeywordToken(SyntaxKind.QueryElementsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalQueryDataItemSyntax> dataItems = ParseQueryDataItems();
		return InternalSyntaxFactory.QueryElements(queryElementsKeyword, openBraceToken, dataItems, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalQueryDataItemSyntax> ParseQueryDataItems()
	{
		InternalSyntaxListBuilder<InternalQueryDataItemSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalQueryDataItemSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.DataItemKeyword))
			{
				InternalQueryDataItemSyntax node = ParseQueryDataItem();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalQueryDataItemSyntax ParseQueryDataItem()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.DataItemKeyword)
		{
			return (InternalQueryDataItemSyntax)EatNode();
		}
		InternalSyntaxToken dataItemKeyword = EatKeywordToken(SyntaxKind.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			QueryDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalQueryDataItemElementSyntax> elements = ParseQueryDataItemElementList();
		return InternalSyntaxFactory.QueryDataItem(dataItemKeyword, openParenthesisToken, name2, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalQueryDataItemElementSyntax> ParseQueryDataItemElementList()
	{
		InternalSyntaxListBuilder<InternalQueryDataItemElementSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalQueryDataItemElementSyntax>();
		try
		{
			while (!base.IsEndOfFile && IsCurrentTokenDataItemElement())
			{
				internalSyntaxListBuilder.Add(ParseQueryDataItemElement());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private bool IsCurrentTokenDataItemElement()
	{
		return base.CurrentToken.IsKeywordKind(SyntaxKind.DataItemKeyword, SyntaxKind.ColumnKeyword, SyntaxKind.FilterKeyword);
	}

	private InternalQueryDataItemElementSyntax ParseQueryDataItemElement()
	{
		if (base.IsIncremental && IsCurrentNodeDataItemElement())
		{
			return (InternalQueryDataItemElementSyntax)EatNode();
		}
		return base.CurrentToken.ContextualKind switch
		{
			SyntaxKind.DataItemKeyword => ParseQueryDataItem(), 
			SyntaxKind.ColumnKeyword => ParseQueryColumn(), 
			SyntaxKind.FilterKeyword => ParseQueryFilter(), 
			_ => throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.Kind), 
		};
	}

	private bool IsCurrentNodeDataItemElement()
	{
		if (base.CurrentNodeKind != SyntaxKind.QueryDataItem && base.CurrentNodeKind != SyntaxKind.QueryColumn)
		{
			return base.CurrentNodeKind == SyntaxKind.QueryFilter;
		}
		return true;
	}

	private InternalQueryColumnSyntax ParseQueryColumn()
	{
		InternalSyntaxToken columnKeyword = EatKeywordToken(SyntaxKind.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken ct = base.CurrentToken;
		ParseOptionalQueryColumnSourceExpression(ct, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression);
		return InternalSyntaxFactory.QueryColumn(closeParenthesisToken: EatToken(SyntaxKind.CloseParenToken), openBraceToken: EatToken(SyntaxKind.OpenBraceToken), propertyList: ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			QueryColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}), columnKeyword: columnKeyword, openParenthesisToken: openParenthesisToken, name: name2, semicolonToken: semiColon, relatedField: sourceExpression, closeBraceToken: EatToken(SyntaxKind.CloseBraceToken));
	}

	private void ParseOptionalQueryColumnSourceExpression(InternalSyntaxToken ct, out InternalSyntaxToken semiColon, out InternalIdentifierNameSyntax sourceExpression)
	{
		if (ct.Kind == SyntaxKind.SemicolonToken)
		{
			semiColon = EatToken(SyntaxKind.SemicolonToken);
			sourceExpression = ParseIdentifierName();
		}
		else
		{
			semiColon = null;
			sourceExpression = null;
		}
	}

	private InternalQueryFilterSyntax ParseQueryFilter()
	{
		InternalSyntaxToken filterKeyword = EatKeywordToken(SyntaxKind.FilterKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax relatedField = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			QueryFilterProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		return InternalSyntaxFactory.QueryFilter(filterKeyword, openParenthesisToken, name2, semicolonToken, relatedField, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalReportSyntax ParseReport()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportObject)
		{
			return (InternalReportSyntax)EatNode();
		}
		InternalSyntaxToken reportKeyword = EatKeywordToken(SyntaxKind.ReportKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Report {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ReportProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalReportDataSetSectionSyntax dataSet = ParseReportDataSetSection();
		InternalRequestPageSyntax requestPage = ParseRequestPage();
		InternalReportRenderingSectionSyntax rendering = ParseReportRenderingSection();
		InternalReportLabelsSectionSyntax labels = ParseReportLabelsSection();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetReportTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.Report(reportKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, dataSet, requestPage, rendering, labels, members, closeBraceToken);
	}

	private InternalRequestPageSyntax ParseRequestPage()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.RequestPage)
		{
			return (InternalRequestPageSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.RequestPageKeyword))
		{
			return null;
		}
		InternalSyntaxToken requestPageKeyword = EatKeywordToken(SyntaxKind.RequestPageKeyword);
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			RequestPageProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken node;
		if (internalSyntaxToken.IsMissing)
		{
			node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			node = WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, base.CurrentToken.Kind));
		}
		else
		{
			node = EatToken(SyntaxKind.CloseBraceToken);
		}
		return AnnotateWithActionVersions(InternalSyntaxFactory.RequestPage(requestPageKeyword, internalSyntaxToken, null, propertyList, layout, actions, members, node), propertyList, actions);
	}

	private InternalReportDataSetSectionSyntax ParseReportDataSetSection()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportDataSetSection)
		{
			return (InternalReportDataSetSectionSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.DataSetKeyword))
		{
			return null;
		}
		InternalSyntaxToken dataSetKeyword = EatKeywordToken(SyntaxKind.DataSetKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportDataItemSyntax> dataItems = ParseReportDataItems();
		return InternalSyntaxFactory.ReportDataSetSection(dataSetKeyword, openBraceToken, dataItems, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportDataItemSyntax> ParseReportDataItems()
	{
		InternalSyntaxListBuilder<InternalReportDataItemSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportDataItemSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.DataItemKeyword))
			{
				InternalReportDataItemSyntax node = ParseReportDataItem();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportDataItemSyntax ParseReportDataItem()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportDataItem)
		{
			return (InternalReportDataItemSyntax)EatNode();
		}
		InternalSyntaxToken dataItemKeyword = EatKeywordToken(SyntaxKind.DataItemKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax dataItemTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ReportDataItemProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalReportDataItemElementSyntax> elements = ParseDataItemElementList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetReportDataItemTriggerInfo);
		return InternalSyntaxFactory.ReportDataItem(dataItemKeyword, openParenthesisToken, name2, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportDataItemElementSyntax> ParseDataItemElementList()
	{
		InternalSyntaxListBuilder<InternalReportDataItemElementSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportDataItemElementSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.DataItemKeyword, SyntaxKind.ColumnKeyword))
			{
				internalSyntaxListBuilder.Add(ParseReportDataItemElement());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportDataItemElementSyntax ParseReportDataItemElement()
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.ReportDataItem || base.CurrentNodeKind == SyntaxKind.ReportColumn))
		{
			return (InternalReportDataItemElementSyntax)EatNode();
		}
		return base.CurrentToken.ContextualKind switch
		{
			SyntaxKind.DataItemKeyword => ParseReportDataItem(), 
			SyntaxKind.ColumnKeyword => ParseReportColumn(), 
			_ => throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.Kind), 
		};
	}

	private InternalReportColumnSyntax ParseReportColumn()
	{
		InternalSyntaxToken columnKeyword = EatKeywordToken(SyntaxKind.ColumnKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceExpression = ParseInLexerMode(LexerMode.Expression, base.ParseExpression);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ReportColumnProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		return InternalSyntaxFactory.ReportColumn(columnKeyword, openParenthesisToken, name2, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalReportLabelsSectionSyntax ParseReportLabelsSection()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportLabelsSection)
		{
			return (InternalReportLabelsSectionSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.LabelsKeyword))
		{
			return null;
		}
		InternalSyntaxToken labelsKeyword = EatKeywordToken(SyntaxKind.LabelsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportLabelBaseSyntax> labels = ParseReportLabels();
		return InternalSyntaxFactory.ReportLabelsSection(labelsKeyword, openBraceToken, labels, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalReportLabelBaseSyntax> ParseReportLabels()
	{
		InternalSyntaxListBuilder<InternalReportLabelBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportLabelBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.Kind.IsTokenIdentifier())
			{
				InternalReportLabelBaseSyntax internalReportLabelBaseSyntax = ParseReportLabel();
				if (internalReportLabelBaseSyntax.IsMissing)
				{
					break;
				}
				internalSyntaxListBuilder.Add(internalReportLabelBaseSyntax);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportLabelBaseSyntax ParseReportLabel()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportLabel)
		{
			return (InternalReportLabelBaseSyntax)EatNode();
		}
		if (base.CurrentToken.IsKeywordKind(SyntaxKind.LabelKeyword))
		{
			InternalSyntaxToken labelKeyword = EatKeywordToken(SyntaxKind.LabelKeyword);
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax name = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			return InternalSyntaxFactory.ReportLabelMultilanguage(caption: ParseCommaSeparatedIdentifierEqualsStringList(ref startToken, SyntaxKind.CloseParenToken), closeParenthesisToken: EatToken(SyntaxKind.CloseParenToken), labelKeyword: labelKeyword, openParenthesisToken: openParenthesisToken, name: name, semicolonToken: startToken);
		}
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken equalsToken = EatToken(SyntaxKind.EqualsToken);
		InternalLabelSyntax label = ParseLabel();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		return InternalSyntaxFactory.ReportLabel(name2, equalsToken, label, semicolonToken);
	}

	private InternalReportRenderingSectionSyntax ParseReportRenderingSection()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportRenderingSection)
		{
			return (InternalReportRenderingSectionSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.RenderingKeyword))
		{
			return null;
		}
		InternalSyntaxToken renderingKeyword = EatKeywordToken(SyntaxKind.RenderingKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportLayoutSyntax> layouts = ParseReportLayouts();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return CheckFeatureAvailability(InternalSyntaxFactory.ReportRenderingSection(renderingKeyword, openBraceToken, layouts, closeBraceToken), Feature.ReportRendering);
	}

	private InternalSyntaxList<InternalReportLayoutSyntax> ParseReportLayouts()
	{
		InternalSyntaxListBuilder<InternalReportLayoutSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportLayoutSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.LayoutKeyword))
			{
				internalSyntaxListBuilder.Add(ParseReportLayout());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportLayoutSyntax ParseReportLayout()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportLayout)
		{
			return (InternalReportLayoutSyntax)EatNode();
		}
		InternalSyntaxToken layoutKeyword = EatKeywordToken(SyntaxKind.LayoutKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ReportLayoutProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.ReportLayout(layoutKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
	}

	private static PropertyTypeInfo LookupReportDataItemOrColumnProperty(string name)
	{
		return LookupReportDataItemProperty(name) ?? LookupReportColumnProperty(name);
	}

	private InternalReportExtensionSyntax ParseReportExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportExtension)
		{
			return (InternalReportExtensionSyntax)EatNode();
		}
		InternalSyntaxToken reportExtensionKeyword = EatKeywordToken(SyntaxKind.ReportExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Report Extension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			ReportExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}, asExtension: true);
		InternalReportExtensionDataSetSectionSyntax dataSet = ParseReportExtensionDataSetSection();
		InternalRequestPageExtensionSyntax requestPage = ParseRequestPageExtension();
		InternalReportRenderingSectionSyntax rendering = ParseReportRenderingSection();
		InternalReportLabelsSectionSyntax labels = ParseReportLabelsSection();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetReportExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return CheckFeatureAvailability(InternalSyntaxFactory.ReportExtension(reportExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, dataSet, requestPage, rendering, labels, members, closeBraceToken), Feature.ReportExtensions);
	}

	private InternalReportExtensionDataSetSectionSyntax ParseReportExtensionDataSetSection()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetSection)
		{
			return (InternalReportExtensionDataSetSectionSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.DataSetKeyword))
		{
			return null;
		}
		InternalSyntaxToken dataSetKeyword = EatKeywordToken(SyntaxKind.DataSetKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportExtensionDataSetChangeBaseSyntax> changes = ParseReportExtensionDataSetChanges();
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.ReportExtensionDataSetSection(dataSetKeyword, openBraceToken, changes, closeBraceToken);
	}

	private InternalSyntaxList<InternalReportExtensionDataSetChangeBaseSyntax> ParseReportExtensionDataSetChanges()
	{
		InternalSyntaxListBuilder<InternalReportExtensionDataSetChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportExtensionDataSetChangeBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && IsChangeKeyword(base.CurrentToken))
			{
				InternalReportExtensionDataSetChangeBaseSyntax node = ParseReportExtensionDataSetChange();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportExtensionDataSetChangeBaseSyntax ParseReportExtensionDataSetChange()
	{
		if (base.IsIncremental && (base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetAddColumn || base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetAddDataItem || base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetModify))
		{
			return (InternalReportExtensionDataSetModifySyntax)EatNode();
		}
		switch (base.CurrentToken.ContextualKind)
		{
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
			return ParseReportExtensionDataSetAddDataItem();
		case SyntaxKind.AddKeyword:
			return ParseReportExtensionDataSetAddColumn();
		case SyntaxKind.ModifyKeyword:
			return ParseReportExtensionModifications();
		default:
			throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.Kind);
		}
	}

	private InternalReportExtensionDataSetAddDataItemSyntax ParseReportExtensionDataSetAddDataItem()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetAddDataItem)
		{
			return (InternalReportExtensionDataSetAddDataItemSyntax)EatNode();
		}
		InternalSyntaxToken changeKeyword = EatKeywordToken(new SyntaxKind[4]
		{
			SyntaxKind.AddFirstKeyword,
			SyntaxKind.AddLastKeyword,
			SyntaxKind.AddAfterKeyword,
			SyntaxKind.AddBeforeKeyword
		}, ErrorCode.ERR_UnexpectedToken);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax anchor = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportDataItemSyntax> dataItems = ParseReportDataItems();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.ReportExtensionDataSetAddDataItem(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, dataItems, closeBraceToken);
	}

	private InternalSyntaxList<InternalReportColumnSyntax> ParseReportExtensionColumns()
	{
		InternalSyntaxListBuilder<InternalReportColumnSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalReportColumnSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.ColumnKeyword))
			{
				InternalReportColumnSyntax node = ParseReportColumn();
				internalSyntaxListBuilder.Add(node);
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalReportExtensionDataSetAddColumnSyntax ParseReportExtensionDataSetAddColumn()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetAddColumn)
		{
			return (InternalReportExtensionDataSetAddColumnSyntax)EatNode();
		}
		InternalSyntaxToken changeKeyword = EatKeywordToken(SyntaxKind.AddKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax anchor = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalReportColumnSyntax> columns = ParseReportExtensionColumns();
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.ReportExtensionDataSetAddColumn(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, columns, closeBraceToken);
	}

	private InternalReportExtensionDataSetModifySyntax ParseReportExtensionModifications()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.ReportExtensionDataSetModify)
		{
			return (InternalReportExtensionDataSetModifySyntax)EatNode();
		}
		InternalSyntaxToken changeKeyword = EatKeywordToken(SyntaxKind.ModifyKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax anchor = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList((string name) => LookupReportDataItemProperty(name) ?? LookupReportColumnProperty(name), asExtension: true);
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetReportExtensionDataSetModifyTriggerInfo);
		InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
		return InternalSyntaxFactory.ReportExtensionDataSetModify(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
	}

	private InternalRequestPageExtensionSyntax ParseRequestPageExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.RequestPageExtension)
		{
			return (InternalRequestPageExtensionSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.RequestPageKeyword))
		{
			return null;
		}
		InternalSyntaxToken requestPageKeyword = EatKeywordToken(SyntaxKind.RequestPageKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			RequestPageExtensionProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}, asExtension: true);
		InternalPageExtensionLayoutSyntax layout = ParsePageExtensionLayout();
		InternalPageExtensionActionListSyntax actions = ParsePageExtensionActions();
		ParsePageExtensionViews();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return AnnotateWithActionVersions(InternalSyntaxFactory.RequestPageExtension(requestPageKeyword, openBraceToken, null, propertyList, layout, actions, members, closeBraceToken), propertyList, actions);
	}

	private static bool IsChangeKeyword(InternalSyntaxToken token)
	{
		return token.IsKeywordKind(SyntaxKind.AddFirstKeyword, SyntaxKind.AddLastKeyword, SyntaxKind.AddAfterKeyword, SyntaxKind.AddBeforeKeyword, SyntaxKind.ModifyKeyword, SyntaxKind.AddKeyword);
	}

	private InternalTableSyntax ParseTable()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.TableObject)
		{
			return (InternalTableSyntax)EatNode();
		}
		InternalSyntaxToken tableKeyword = EatKeywordToken();
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing Table {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			TableProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalFieldListSyntax fields = ParseFieldList();
		InternalKeyListSyntax keys = ParseKeyList();
		InternalFieldGroupListSyntax fieldGroups = ParseFieldGroupList();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetTableTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.Table(tableKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	private InternalFieldGroupListSyntax ParseFieldGroupList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldGroupList)
		{
			return (InternalFieldGroupListSyntax)EatNode();
		}
		return ParseKeysOrFieldGroups(SyntaxKind.FieldGroupsKeyword, SyntaxKind.FieldGroup, SyntaxKind.FieldGroupKeyword, (InternalSyntaxToken fieldGroupsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalFieldGroupSyntax> fieldGroups, InternalSyntaxToken closeBraceToken) => new InternalFieldGroupListSyntax(SyntaxKind.FieldGroupList, fieldGroupsKeyword, openBraceToken, fieldGroups.Node, closeBraceToken), (InternalSyntaxToken fieldGroupKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken) => new InternalFieldGroupSyntax(SyntaxKind.FieldGroup, fieldGroupKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken), delegate(string name)
		{
			PropertyTypeInfo value = null;
			FieldGroupProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
	}

	private InternalKeyListSyntax ParseKeyList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.KeyList)
		{
			return (InternalKeyListSyntax)EatNode();
		}
		return ParseKeysOrFieldGroups(SyntaxKind.KeysKeyword, SyntaxKind.Key, SyntaxKind.KeyKeyword, (InternalSyntaxToken keysKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalKeySyntax> keys, InternalSyntaxToken closeBraceToken) => new InternalKeyListSyntax(SyntaxKind.KeyList, keysKeyword, openBraceToken, keys.Node, closeBraceToken), (InternalSyntaxToken keyKeyword, InternalSyntaxToken openParenthesisToken, InternalSyntaxToken id, InternalSyntaxToken semicolonToken1, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken2, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken) => new InternalKeySyntax(SyntaxKind.Key, keyKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields.Node, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken), delegate(string name)
		{
			PropertyTypeInfo value = null;
			KeyProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
	}

	private InternalFieldListSyntax ParseFieldList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldList)
		{
			return (InternalFieldListSyntax)EatNode();
		}
		InternalSyntaxToken fieldsKeyword = EatKeywordToken(SyntaxKind.FieldsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldSyntax> fields = ParseFields();
		return InternalSyntaxFactory.FieldList(fieldsKeyword, openBraceToken, fields, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalFieldSyntax> ParseFields()
	{
		InternalSyntaxListBuilder<InternalFieldSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalFieldSyntax>();
		bool flag = false;
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(SyntaxKind.FieldKeyword))
			{
				InternalFieldSyntax internalFieldSyntax = ParseField();
				if (internalFieldSyntax.HasAnnotations(AnnotationKind.MovedSymbols))
				{
					flag = true;
				}
				internalSyntaxListBuilder.Add(internalFieldSyntax);
			}
			return flag ? AnnotateWithMovedSymbolAnnotation(internalSyntaxListBuilder.ToList()) : internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldSyntax ParseField()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.Field)
		{
			return (InternalFieldSyntax)EatNode();
		}
		InternalSyntaxToken fieldKeyword = EatKeywordToken(SyntaxKind.FieldKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken no = EatToken(SyntaxKind.Int32LiteralToken);
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken2 = EatToken(SyntaxKind.SemicolonToken);
		InternalDataTypeSyntax type = ParseTableFieldType();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax internalPropertyListSyntax = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			FieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetFieldTriggerInfo);
		InternalFieldSyntax internalFieldSyntax = InternalSyntaxFactory.Field(fieldKeyword, openParenthesisToken, no, semicolonToken, name2, semicolonToken2, type, closeParenthesisToken, openBraceToken, internalPropertyListSyntax, triggers, EatToken(SyntaxKind.CloseBraceToken));
		if (internalPropertyListSyntax != null && internalPropertyListSyntax.HasAnnotations(AnnotationKind.MovedSymbols))
		{
			internalFieldSyntax = internalFieldSyntax.WithAdditionalAnnotationsInternal(MovedSymbolsAnnotation);
		}
		return internalFieldSyntax;
	}

	private static InternalSyntaxList<T> AnnotateWithMovedSymbolAnnotation<T>(InternalSyntaxList<T> internalSyntaxList) where T : InternalSyntaxNode
	{
		return new InternalSyntaxList<T>(internalSyntaxList.Node.WithAdditionalAnnotationsInternal(MovedSymbolsAnnotation));
	}

	private InternalDataTypeSyntax ParseTableFieldType()
	{
		if (SyntaxFacts.IsOptionType(base.CurrentToken))
		{
			return ParseOptionDataType(skipParsingOptionValues: true);
		}
		return ParseDatatype();
	}

	private U ParseKeysOrFieldGroups<U, V>(SyntaxKind listKindKeyword, SyntaxKind memberKind, SyntaxKind memberKindKeyword, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxList<V>, InternalSyntaxToken, U> createList, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, V> createMember, Func<string, PropertyTypeInfo> lookupProperty) where U : InternalSyntaxNode where V : InternalKeyFieldGroupBaseSyntax
	{
		if (!base.CurrentToken.IsKeywordKind(listKindKeyword))
		{
			return null;
		}
		InternalSyntaxToken arg = EatKeywordToken(listKindKeyword);
		InternalSyntaxToken arg2 = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<V> arg3 = ParseKeyOrFieldGroupList(memberKind, memberKindKeyword, createMember, lookupProperty);
		return createList(arg, arg2, arg3, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<T> ParseKeyOrFieldGroupList<T>(SyntaxKind memberKind, SyntaxKind keywordKind, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, T> createNode, Func<string, PropertyTypeInfo> lookupProperty) where T : InternalKeyFieldGroupBaseSyntax
	{
		InternalSyntaxListBuilder<T> internalSyntaxListBuilder = base.Pool.Allocate<T>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsKeywordKind(keywordKind))
			{
				if (base.IsIncremental && base.CurrentNodeKind == memberKind)
				{
					internalSyntaxListBuilder.Add((T)EatNode());
				}
				else
				{
					internalSyntaxListBuilder.Add(ParseKeyOrFieldGroup(keywordKind, createNode, lookupProperty));
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private T ParseKeyOrFieldGroup<T>(SyntaxKind keywordKind, Func<InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalSyntaxToken, InternalIdentifierNameSyntax, InternalSyntaxToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>, InternalSyntaxToken, InternalSyntaxToken, InternalPropertyListSyntax, InternalSyntaxToken, T> createNode, Func<string, PropertyTypeInfo> lookupProperty) where T : InternalKeyFieldGroupBaseSyntax
	{
		InternalSyntaxToken arg = EatKeywordToken(keywordKind);
		InternalSyntaxToken arg2 = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken arg3 = null;
		InternalSyntaxToken arg4 = null;
		if (base.CurrentToken.Kind == SyntaxKind.Int32LiteralToken)
		{
			arg3 = AddError(EatToken(SyntaxKind.Int32LiteralToken), ErrorCode.WRN_DeprecatedIdSyntax);
			arg4 = EatToken(SyntaxKind.SemicolonToken);
		}
		InternalIdentifierNameSyntax arg5 = ParseIdentifierName();
		InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> arg6 = ParseCommaSeparatedIdentifierNames(ref startToken, SyntaxKind.CloseParenToken);
		InternalSyntaxToken arg7 = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken arg8 = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax arg9 = ParsePropertyList(lookupProperty);
		return createNode(arg, arg2, arg3, arg4, arg5, startToken, arg6, arg7, arg8, arg9, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalTableExtensionSyntax ParseTableExtension()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.TableExtensionObject)
		{
			return (InternalTableExtensionSyntax)EatNode();
		}
		InternalSyntaxToken tableExtensionKeyword = EatKeywordToken(SyntaxKind.TableExtensionKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing TableExtension {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken extendsKeyword = EatKeywordToken(SyntaxKind.ExtendsKeyword);
		InternalObjectNameOrIdSyntax baseObject = ParseObjectReferenceSyntax();
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			TableProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		}, asExtension: true);
		InternalFieldExtensionListSyntax fields = (base.CurrentToken.IsKeywordKind(SyntaxKind.FieldsKeyword) ? ParseFieldExtensionList() : null);
		InternalKeyListSyntax keys = (base.CurrentToken.IsKeywordKind(SyntaxKind.KeysKeyword) ? ParseKeyList() : null);
		InternalFieldGroupExtensionListSyntax fieldGroups = (base.CurrentToken.IsKeywordKind(SyntaxKind.FieldGroupsKeyword) ? ParseFieldGroupExtensionList() : null);
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetTableExtensionTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.TableExtension(tableExtensionKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
	}

	private InternalFieldExtensionListSyntax ParseFieldExtensionList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldExtensionList)
		{
			return (InternalFieldExtensionListSyntax)EatNode();
		}
		InternalSyntaxToken fieldsKeyword = EatKeywordToken(SyntaxKind.FieldsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldBaseSyntax> fields = ParseExtensionFields();
		return InternalSyntaxFactory.FieldExtensionList(fieldsKeyword, openBraceToken, fields, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalFieldBaseSyntax> ParseExtensionFields()
	{
		InternalSyntaxListBuilder<InternalFieldBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalFieldBaseSyntax>();
		bool flag = false;
		try
		{
			while (!base.IsEndOfFile)
			{
				InternalFieldBaseSyntax internalFieldBaseSyntax = null;
				switch (base.CurrentToken.ContextualKind)
				{
				case SyntaxKind.FieldKeyword:
					internalFieldBaseSyntax = ParseField();
					break;
				case SyntaxKind.ModifyKeyword:
					internalFieldBaseSyntax = ParseFieldModification();
					break;
				}
				if (internalFieldBaseSyntax == null)
				{
					break;
				}
				if (internalFieldBaseSyntax.HasAnnotations(AnnotationKind.MovedSymbols))
				{
					flag = true;
				}
				internalSyntaxListBuilder.Add(internalFieldBaseSyntax);
			}
			return flag ? AnnotateWithMovedSymbolAnnotation(internalSyntaxListBuilder.ToList()) : internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldBaseSyntax ParseFieldModification()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldModification)
		{
			return (InternalFieldBaseSyntax)EatNode();
		}
		InternalSyntaxToken fieldKeyword = EatKeywordToken(SyntaxKind.ModifyKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			FieldProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetFieldExtensionTriggerInfo);
		return InternalSyntaxFactory.FieldModification(fieldKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalFieldGroupExtensionListSyntax ParseFieldGroupExtensionList()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldGroupExtensionList)
		{
			return (InternalFieldGroupExtensionListSyntax)EatNode();
		}
		InternalSyntaxToken fieldGroupsKeyword = EatKeywordToken(SyntaxKind.FieldGroupsKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> changes = ParseChangeFieldGroupList();
		InternalFieldGroupExtensionListSyntax node = InternalSyntaxFactory.FieldGroupExtensionList(fieldGroupsKeyword, openBraceToken, changes, EatToken(SyntaxKind.CloseBraceToken));
		return CheckFeatureAvailability(node, Feature.FieldGroupExtension);
	}

	private InternalSyntaxList<InternalFieldGroupChangeBaseSyntax> ParseChangeFieldGroupList()
	{
		InternalSyntaxListBuilder<InternalFieldGroupChangeBaseSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalFieldGroupChangeBaseSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.ContextualKind.IsFieldGroupChangeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseFieldGroupChange());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalFieldGroupChangeBaseSyntax ParseFieldGroupChange()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.FieldGroupAddChange)
		{
			return (InternalFieldGroupChangeBaseSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordToken();
		if (internalSyntaxToken.Kind == SyntaxKind.AddLastKeyword)
		{
			InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
			InternalIdentifierNameSyntax anchor = ParseIdentifierName();
			InternalSyntaxToken startToken = EatToken(SyntaxKind.SemicolonToken);
			InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> fields = ParseCommaSeparatedIdentifierNames(ref startToken, SyntaxKind.CloseParenToken);
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
			InternalPropertyListSyntax propertyList = InternalSyntaxFactory.PropertyList(default(InternalSyntaxList<InternalPropertySyntaxOrEmpty>));
			InternalSyntaxToken closeBraceToken = EatToken(SyntaxKind.CloseBraceToken);
			return InternalSyntaxFactory.FieldGroupAddChange(internalSyntaxToken, openParenthesisToken, anchor, startToken, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
		}
		throw ExceptionUtilities.UnexpectedValue(internalSyntaxToken.Kind);
	}

	private InternalTableRelationPropertyValueSyntax ParseTableRelationPropertyValue(PropertyTypeInfo pti)
	{
		string name = pti.Name;
		InternalIfTableRelationSyntax internalIfTableRelationSyntax = ParseIfTableRelation();
		InternalNameSyntax relatedTableField = ParseQualifiedName();
		InternalWhereExpressionSyntax tableFilter = ParseTableFilters();
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfKind(SyntaxKind.ElseKeyword);
		InternalElseTableRelationSyntax elseExpression = null;
		if (internalSyntaxToken != null)
		{
			if (internalIfTableRelationSyntax == null)
			{
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_ElseWithoutIf, name);
			}
			InternalTableRelationPropertyValueSyntax elseTableRelationCondition = ParseTableRelationPropertyValue(pti);
			elseExpression = InternalSyntaxFactory.ElseTableRelation(internalSyntaxToken, elseTableRelationCondition);
		}
		return InternalSyntaxFactory.TableRelationPropertyValue(internalIfTableRelationSyntax, relatedTableField, tableFilter, elseExpression);
	}

	private InternalIfTableRelationSyntax ParseIfTableRelation()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.IfTableRelationExpression)
		{
			return (InternalIfTableRelationSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfKind(SyntaxKind.IfKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalTableFilterExpressionSyntax ifTableRelationCondition = ParseTableRelationCondition();
		return InternalSyntaxFactory.IfTableRelation(internalSyntaxToken, ifTableRelationCondition);
	}

	private InternalTableFilterExpressionSyntax ParseTableRelationCondition()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.TableFilterExpression)
		{
			return (InternalTableFilterExpressionSyntax)EatNode();
		}
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions = ParseTableRelationConditionValue();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.TableFilterExpression(openParenthesisToken, conditions, closeParenthesisToken);
	}

	internal InternalWhereExpressionSyntax ParseTableFilters(bool tokenExpected = false)
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.WhereExpression)
		{
			return (InternalWhereExpressionSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfKind(SyntaxKind.WhereFormulaKeyword);
		if (internalSyntaxToken == null)
		{
			if (!tokenExpected)
			{
				return null;
			}
			InternalSyntaxToken node = InternalSyntaxFactory.MissingToken(SyntaxKind.WhereFormulaKeyword);
			internalSyntaxToken = AddError(node, ErrorCode.ERR_InvalidFilterExpression);
		}
		InternalTableFilterExpressionSyntax filter = ParseTableRelationCondition();
		return InternalSyntaxFactory.WhereExpression(internalSyntaxToken, filter);
	}

	private InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> ParseTableRelationConditionValue()
	{
		InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> result = default(InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax>);
		InternalSeparatedSyntaxListBuilder<InternalPropertyExpressionSyntax> item = base.Pool.AllocateSeparated<InternalPropertyExpressionSyntax>();
		try
		{
			InternalPropertyExpressionSyntax internalPropertyExpressionSyntax = ParsePropertyExpression();
			if (internalPropertyExpressionSyntax != null)
			{
				item.Add(internalPropertyExpressionSyntax);
				while (base.CurrentToken.Kind == SyntaxKind.CommaToken)
				{
					item.AddSeparator(base.CurrentToken);
					EatToken();
					item.Add(ParsePropertyExpression());
				}
				return item.ToList();
			}
			return result;
		}
		finally
		{
			base.Pool.Free(item);
		}
	}

	internal InternalPropertyValueSyntax ParseTableViewPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		InternalSortingSyntax sorting = ParseSorting();
		InternalOrderSyntax order = ParseOrder();
		InternalWhereExpressionSyntax tableFilter = ParseTableFilters();
		return InternalSyntaxFactory.TableViewPropertyValue(sorting, order, tableFilter);
	}

	private InternalPropertyValueSyntax ParseFiltersPropertyValue(PropertyTypeInfo propertyTypeInfo)
	{
		return InternalSyntaxFactory.FiltersPropertyValue(ParseTableFilters(tokenExpected: true));
	}

	private InternalOrderSyntax ParseOrder()
	{
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfKind(SyntaxKind.OrderKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSyntaxToken orderBy = EatKeywordToken(SyntaxKind.DescendingKeyword, SyntaxKind.AscendingKeyword, ErrorCode.ERR_ExpectedAscendingOrDescendingKeyword);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.Order(internalSyntaxToken, openParenthesisToken, orderBy, closeParenthesisToken);
	}

	private InternalSortingSyntax ParseSorting()
	{
		InternalSyntaxToken internalSyntaxToken = EatKeywordTokenIfKind(SyntaxKind.SortingKeyword);
		if (internalSyntaxToken == null)
		{
			return null;
		}
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> sortingFields = ParseCommaSeparatedIdentifierNames(ref startToken);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return InternalSyntaxFactory.Sorting(internalSyntaxToken, startToken, sortingFields, closeParenthesisToken);
	}

	private InternalXmlPortSyntax ParseXmlPort()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.XmlPortObject)
		{
			return (InternalXmlPortSyntax)EatNode();
		}
		InternalSyntaxToken xmlPortKeyword = EatKeywordToken(SyntaxKind.XmlPortKeyword);
		InternalObjectIdSyntax internalObjectIdSyntax = ParseObjectIdSyntax();
		InternalIdentifierNameSyntax internalIdentifierNameSyntax = ParseIdentifierName();
		LocalMachineLogger.LogVerbose("Parsing XmlPort {0} {1}.", internalObjectIdSyntax, internalIdentifierNameSyntax);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalXmlPortSchemaSyntax xmlPortSchema = ParseXmlPortLayout();
		InternalRequestPageSyntax xmlPortRequestPage = ParseXmlPortRequestPage();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetXmlPortTriggerInfo);
		InternalSyntaxToken closeBraceToken = ParseCloseBraceToken(openBraceToken);
		return InternalSyntaxFactory.XmlPort(xmlPortKeyword, internalObjectIdSyntax, internalIdentifierNameSyntax, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken);
	}

	private InternalRequestPageSyntax ParseXmlPortRequestPage()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.RequestPage)
		{
			return (InternalRequestPageSyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = base.CurrentToken;
		if (internalSyntaxToken != null && !internalSyntaxToken.IsKeywordKind(SyntaxKind.RequestPageKeyword))
		{
			return null;
		}
		InternalSyntaxToken requestPageKeyword = EatKeywordToken(SyntaxKind.RequestPageKeyword);
		InternalSyntaxToken internalSyntaxToken2 = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			PageProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalPageLayoutSyntax layout = ParsePageLayout();
		InternalPageActionListSyntax actions = ParsePageActions();
		InternalSyntaxList<InternalMemberSyntax> members = ParseMemberList(TriggerDefinitions.GetPageTriggerInfo);
		InternalSyntaxToken node;
		if (internalSyntaxToken2.IsMissing)
		{
			node = InternalSyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken);
			node = WithAdditionalDiagnostics(node, GetExpectedTokenError(SyntaxKind.CloseBraceToken, base.CurrentToken.Kind));
		}
		else
		{
			node = EatToken(SyntaxKind.CloseBraceToken);
		}
		return InternalSyntaxFactory.RequestPage(requestPageKeyword, internalSyntaxToken2, null, propertyList, layout, actions, members, node);
	}

	private InternalXmlPortSchemaSyntax ParseXmlPortLayout()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.XmlPortSchema)
		{
			return (InternalXmlPortSchemaSyntax)EatNode();
		}
		if (!base.CurrentToken.IsKeywordKind(SyntaxKind.XmlPortSchemaKeyword))
		{
			return null;
		}
		InternalSyntaxToken xmlPortSchemaKeyword = EatKeywordToken(SyntaxKind.XmlPortSchemaKeyword);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalSyntaxList<InternalXmlPortNodeSyntax> xmlPortSchema = ParseXmlPortSchemaList();
		return InternalSyntaxFactory.XmlPortSchema(xmlPortSchemaKeyword, openBraceToken, xmlPortSchema, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTableElementSyntax ParseXmlPortTableElement()
	{
		if (base.IsIncremental && base.CurrentNodeKind == SyntaxKind.XmlPortTableElementKeyword)
		{
			return (InternalXmlPortTableElementSyntax)EatNode();
		}
		InternalSyntaxToken xmlPortTableElementKeyword = EatKeywordToken(SyntaxKind.XmlPortTableElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalObjectNameOrIdSyntax sourceTable = ParseObjectReferenceSyntax();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortTableElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTableElementTriggerInfo);
		return InternalSyntaxFactory.XmlPortTableElement(xmlPortTableElementKeyword, openParenthesisToken, name2, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalSyntaxList<InternalXmlPortNodeSyntax> ParseXmlPortSchemaList()
	{
		InternalSyntaxListBuilder<InternalXmlPortNodeSyntax> internalSyntaxListBuilder = base.Pool.Allocate<InternalXmlPortNodeSyntax>();
		try
		{
			while (!base.IsEndOfFile && base.CurrentToken.IsXmlPortNodeKeyword())
			{
				internalSyntaxListBuilder.Add(ParseXmlPortNode());
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			base.Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalXmlPortNodeSyntax ParseXmlPortNode()
	{
		if (base.IsIncremental && base.CurrentToken.Kind.IsXmlPortNodeSyntax())
		{
			return (InternalXmlPortNodeSyntax)EatNode();
		}
		return base.CurrentToken.ContextualKind switch
		{
			SyntaxKind.XmlPortTableElementKeyword => ParseXmlPortTableElement(), 
			SyntaxKind.XmlPortFieldElementKeyword => ParseXmlPortFieldElement(), 
			SyntaxKind.XmlPortTextElementKeyword => ParseXmlPortTextElement(), 
			SyntaxKind.XmlPortFieldAttributeKeyword => ParseXmlPortFieldAttribute(), 
			SyntaxKind.XmlPortTextAttributeKeyword => ParseXmlPortTextAttribute(), 
			_ => throw ExceptionUtilities.UnexpectedValue(base.CurrentToken.ContextualKind), 
		};
	}

	private InternalXmlPortFieldNodeSyntax ParseXmlPortFieldElement()
	{
		InternalSyntaxToken xmlPortFieldElementKeyword = EatKeywordToken(SyntaxKind.XmlPortFieldElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = ParseMemberAccessExpressionOrIdentifier();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortFieldElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortFieldElementTriggerInfo);
		return InternalSyntaxFactory.XmlPortFieldElement(xmlPortFieldElementKeyword, openParenthesisToken, name2, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortFieldNodeSyntax ParseXmlPortFieldAttribute()
	{
		InternalSyntaxToken xmlPortFieldAttributeKeyword = EatKeywordToken(SyntaxKind.XmlPortFieldAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken semicolonToken = EatToken(SyntaxKind.SemicolonToken);
		InternalCodeExpressionSyntax sourceField = ParseMemberAccessExpressionOrIdentifier();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortFieldAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortFieldAttributeTriggerInfo);
		return InternalSyntaxFactory.XmlPortFieldAttribute(xmlPortFieldAttributeKeyword, openParenthesisToken, name2, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTextNodeSyntax ParseXmlPortTextElement()
	{
		InternalSyntaxToken xmlPortTextElementKeyword = EatKeywordToken(SyntaxKind.XmlPortTextElementKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortTextElementProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTextElementTriggerInfo);
		return InternalSyntaxFactory.XmlPortTextElement(xmlPortTextElementKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}

	private InternalXmlPortTextNodeSyntax ParseXmlPortTextAttribute()
	{
		InternalSyntaxToken xmlPortTextAttributeKeyword = EatKeywordToken(SyntaxKind.XmlPortTextAttributeKeyword);
		InternalSyntaxToken openParenthesisToken = EatToken(SyntaxKind.OpenParenToken);
		InternalIdentifierNameSyntax name2 = ParseIdentifierName();
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		InternalSyntaxToken openBraceToken = EatToken(SyntaxKind.OpenBraceToken);
		InternalPropertyListSyntax propertyList = ParsePropertyList(delegate(string name)
		{
			PropertyTypeInfo value = null;
			XmlPortTextAttributeProperties.TryGetValue(name.ToUpperInvariant(), out value);
			return value;
		});
		InternalSyntaxList<InternalXmlPortNodeSyntax> schema = ParseXmlPortSchemaList();
		InternalSyntaxList<InternalTriggerDeclarationSyntax> triggers = ParseTriggerList(TriggerDefinitions.GetXmlPortTextAttributeTriggerInfo);
		return InternalSyntaxFactory.XmlPortTextAttribute(xmlPortTextAttributeKeyword, openParenthesisToken, name2, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, EatToken(SyntaxKind.CloseBraceToken));
	}
}
