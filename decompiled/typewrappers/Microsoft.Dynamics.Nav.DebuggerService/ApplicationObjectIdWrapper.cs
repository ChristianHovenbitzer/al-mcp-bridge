using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.DebuggerService;

[DebuggerDisplay("({ObjectType} {ObjectNumber})")]
[JsonObject]
[DataContract]
public struct ApplicationObjectIdWrapper : IEquatable<ApplicationObjectIdWrapper>
{
	[DataMember]
	private readonly int objectNumber;

	[DataMember]
	private readonly ObjectTypeWrapper objectType;

	[JsonProperty]
	public int ObjectNumber => objectNumber;

	[JsonProperty]
	public ObjectTypeWrapper ObjectType => objectType;

	[JsonConstructor]
	public ApplicationObjectIdWrapper(ObjectTypeWrapper objectType, int objectNumber)
	{
		this.objectNumber = objectNumber;
		this.objectType = objectType;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", ObjectType, ObjectNumber.ToString(CultureInfo.CurrentCulture));
	}

	public static bool operator ==(ApplicationObjectIdWrapper left, ApplicationObjectIdWrapper right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ApplicationObjectIdWrapper left, ApplicationObjectIdWrapper right)
	{
		return !left.Equals(right);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(ApplicationObjectIdWrapper))
		{
			return false;
		}
		return Equals((ApplicationObjectIdWrapper)obj);
	}

	public bool Equals(ApplicationObjectIdWrapper other)
	{
		if (other.ObjectNumber == ObjectNumber)
		{
			return other.ObjectType == ObjectType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ObjectNumber ^ ((int)ObjectType << 25);
	}
}
