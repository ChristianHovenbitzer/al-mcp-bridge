using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

internal static class OptionServiceFactory
{
	public static OptionService Create()
	{
		return new OptionService(new IOptionProvider[1]
		{
			new CompletionOptionsProvider()
		});
	}
}
