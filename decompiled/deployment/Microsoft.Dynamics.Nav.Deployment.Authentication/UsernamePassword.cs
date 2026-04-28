namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal class UsernamePassword
{
	public string Username { get; set; }

	public string Password { get; set; }

	public UsernamePassword()
	{
	}

	public UsernamePassword(string username, string password)
	{
		Username = username;
		Password = password;
	}
}
