using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Deployment.Authentication;

internal class UserProtectedFileStorage : IFileCache
{
	private readonly IEmitLogger logger;

	private static readonly object FileLock = new object();

	private readonly string filePath;

	private readonly Encoding encoding = Encoding.UTF8;

	private UserProtectedFileStorage(IEmitLogger logger, string fileName)
	{
		this.logger = logger;
		string baseDirectory = GetBaseDirectory();
		filePath = Path.Combine(baseDirectory, fileName);
	}

	public static UserProtectedFileStorage CreateUserPasswordCache(IEmitLogger logger)
	{
		return new UserProtectedFileStorage(logger, "UserPasswordCache.dat");
	}

	public static UserProtectedFileStorage CreateServerInfoCache(IEmitLogger logger)
	{
		return new UserProtectedFileStorage(logger, "ServerInfoCache.dat");
	}

	public static UserProtectedFileStorage CreateClientUsageMap(IEmitLogger logger)
	{
		return new UserProtectedFileStorage(logger, "TokenKeyCache.dat");
	}

	public static UserProtectedFileStorage CreateTenantMapCache(IEmitLogger logger)
	{
		return new UserProtectedFileStorage(logger, "TenantMapCache.dat");
	}

	public bool Exists()
	{
		return File.Exists(filePath);
	}

	public byte[] Read()
	{
		lock (FileLock)
		{
			if (Exists())
			{
				try
				{
					return Unprotect(File.ReadAllBytes(filePath));
				}
				catch (Exception ex)
				{
					logger.Exception(ex);
				}
			}
			return null;
		}
	}

	public T Read<T>()
	{
		byte[] array = Read();
		if (array == null)
		{
			return default(T);
		}
		try
		{
			return JsonConvert.DeserializeObject<T>(encoding.GetString(array));
		}
		catch (Exception ex)
		{
			logger.Exception(ex);
			return default(T);
		}
	}

	public void Write(byte[] bytes)
	{
		try
		{
			File.WriteAllBytes(filePath, Protect(bytes));
		}
		catch (Exception ex)
		{
			logger.Exception(ex);
		}
	}

	public void Write(object obj)
	{
		string s = JsonConvert.SerializeObject(obj);
		Write(encoding.GetBytes(s));
	}

	public bool Clear()
	{
		try
		{
			if (Exists())
			{
				File.Delete(filePath);
			}
			return true;
		}
		catch (Exception ex)
		{
			logger.Exception(ex);
		}
		return false;
	}

	private static string GetBaseDirectory()
	{
		return AppDomain.CurrentDomain.BaseDirectory;
	}

	private byte[] Protect(byte[] bytes)
	{
		if (!OperatingSystem.IsWindows())
		{
			return AspNetDataProtectionHelper.Protect(bytes, null);
		}
		return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
	}

	private byte[] Unprotect(byte[] bytes)
	{
		if (!OperatingSystem.IsWindows())
		{
			return AspNetDataProtectionHelper.Unprotect(bytes, null);
		}
		return ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
	}
}
