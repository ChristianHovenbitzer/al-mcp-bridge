using System;

namespace Microsoft.Dynamics.Nav.Deployment;

public interface IEmitLogger
{
	void Info(string message, params object[] args);

	void Error(string message, params object[] args);

	void Error(string message);

	void Exception(Exception ex);

	void ShowDeviceLoginDialog(string message, string uri, string token);

	void OpenUri(string uri);

	void NetworkException(Exception ex);
}
