using System;
using System.Globalization;
using System.Net.Sockets;

namespace Microsoft.Dynamics.Nav.Deployment;

internal abstract class EmitLogger : IEmitLogger
{
	public void Info(string message, params object[] args)
	{
		SendNotification(string.Format(CultureInfo.InvariantCulture, message, args));
	}

	public void Error(string message, params object[] args)
	{
		SendNotification(string.Format(CultureInfo.InvariantCulture, DeploymentResources.ErrorTemplate, string.Format(CultureInfo.InvariantCulture, message, args)));
	}

	public void Error(string message)
	{
		SendNotification(string.Format(CultureInfo.InvariantCulture, DeploymentResources.ErrorTemplate, message));
	}

	public void Exception(Exception ex)
	{
		Error(ex.AllMessagesToString());
	}

	public virtual void ShowDeviceLoginDialog(string message, string uri, string token)
	{
		Send(message);
	}

	public virtual void OpenUri(string uri)
	{
		Send("OpenUri not supported.");
	}

	public void NetworkException(Exception ex)
	{
		switch ((ex.FindInnermostException() as SocketException)?.SocketErrorCode)
		{
		case SocketError.ConnectionRefused:
			Error(DeploymentResources.ConnectionRefusedError);
			break;
		case SocketError.OperationAborted:
			Error(DeploymentResources.RequestTimedOut);
			break;
		default:
			Error(ex.AllMessagesToString());
			break;
		}
	}

	private void SendNotification(string message)
	{
		Send(string.Format(CultureInfo.InvariantCulture, DeploymentResources.LogMessagePattern, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture), message));
	}

	protected abstract void Send(string message);
}
