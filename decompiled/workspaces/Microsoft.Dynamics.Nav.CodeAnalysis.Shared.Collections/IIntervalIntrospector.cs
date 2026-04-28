namespace Microsoft.Dynamics.Nav.CodeAnalysis.Shared.Collections;

internal interface IIntervalIntrospector<T>
{
	int GetStart(T value);

	int GetLength(T value);
}
