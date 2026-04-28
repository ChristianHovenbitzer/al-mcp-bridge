namespace Microsoft.Dynamics.Nav.Deployment;

internal interface IFileCache
{
	bool Clear();

	bool Exists();

	byte[] Read();

	T Read<T>();

	void Write(byte[] bytes);

	void Write(object obj);
}
