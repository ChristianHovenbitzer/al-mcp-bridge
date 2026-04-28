using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal static class AspNetDataProtectionHelper
{
	public static byte[] Protect(byte[] userData, byte[]? optionalEntropy)
	{
		return GetUserProtector(optionalEntropy).Protect(userData);
	}

	public static byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy)
	{
		return GetUserProtector(optionalEntropy).Unprotect(encryptedData);
	}

	private static IDataProtector GetUserProtector(byte[]? optionalEntropy)
	{
		IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly()?.FullName ?? "Microsoft.Dynamics.BusinessCentral.EditorService")));
		string purpose = CreatePurpose(optionalEntropy);
		return dataProtectionProvider.CreateProtector(purpose);
	}

	private static string CreatePurpose(byte[]? optionalEntropy)
	{
		return Uri.EscapeDataString("Microsoft.Dynamics.BusinessCentral.EditorService.Data" + Convert.ToBase64String(optionalEntropy ?? Array.Empty<byte>()));
	}
}
