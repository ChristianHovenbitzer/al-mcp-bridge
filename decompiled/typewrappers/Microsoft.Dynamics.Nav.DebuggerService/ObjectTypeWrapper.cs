using System;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[Serializable]
public enum ObjectTypeWrapper
{
	TableData = 0,
	Table = 1,
	Form = 2,
	Report = 3,
	Dataport = 4,
	CodeUnit = 5,
	XmlPort = 6,
	MenuSuite = 7,
	Page = 8,
	Query = 9,
	System = 10,
	FieldNumber = 11,
	LimitedUsageTableData = 12,
	TablePage = 13,
	PageExtension = 14,
	TableExtension = 15,
	Enum = 16,
	EnumExtension = 17,
	ReportExtension = 22
}
