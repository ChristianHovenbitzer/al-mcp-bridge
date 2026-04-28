using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DataContract]
[JsonObject]
public class StackFrame
{
	[DataMember]
	private readonly string methodName;

	[DataMember]
	private readonly ApplicationObjectIdWrapper objectId;

	[DataMember]
	private readonly string objectName;

	[DataMember]
	private readonly SourceSpan statementSpan;

	[JsonProperty]
	public string MethodName
	{
		[DebuggerStepThrough]
		get
		{
			return methodName;
		}
	}

	[JsonProperty]
	public ApplicationObjectIdWrapper ObjectId
	{
		[DebuggerStepThrough]
		get
		{
			return objectId;
		}
	}

	[JsonProperty]
	public string ObjectName
	{
		[DebuggerStepThrough]
		get
		{
			return objectName;
		}
	}

	[JsonProperty]
	public SourceSpan StatementSpan
	{
		[DebuggerStepThrough]
		get
		{
			return statementSpan;
		}
	}

	[JsonConstructor]
	public StackFrame(ApplicationObjectIdWrapper objectId, string objectName, string methodName, SourceSpan statementSpan)
	{
		this.objectId = objectId;
		this.objectName = objectName;
		this.methodName = methodName;
		this.statementSpan = statementSpan;
	}
}
