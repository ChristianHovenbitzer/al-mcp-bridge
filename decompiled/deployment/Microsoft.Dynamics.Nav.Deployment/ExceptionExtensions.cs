using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.Dynamics.Nav.Deployment;

public static class ExceptionExtensions
{
	public class SanitizedException : Exception
	{
		public SanitizedException(Exception original, Exception inner)
			: base(original.Message, inner)
		{
		}

		public SanitizedException(SocketException socketException)
			: base(FormattableString.Invariant($"SocketException, SocketErrorCode {socketException.SocketErrorCode}, NativeErrorCode {socketException.NativeErrorCode}"))
		{
		}

		public SanitizedException(IOException ioException)
			: base(FormattableString.Invariant($"IOException, HResult {ioException.HResult}"))
		{
		}

		public SanitizedException(WebException webException)
			: base(FormattableString.Invariant($"WebException, Status {webException.Status}"))
		{
		}
	}

	public class SanitizedAggregateException : AggregateException
	{
		public SanitizedAggregateException(AggregateException original, IEnumerable<Exception> innerExceptions)
			: base(original.Message, innerExceptions)
		{
		}
	}

	public static string AllMessagesToString(this Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (Exception ex2 = ex; ex2 != null; ex2 = ex2.InnerException)
		{
			stringBuilder.AppendLine(ex2.Message);
		}
		return stringBuilder.ToString();
	}

	public static Exception FindInnermostException(this Exception ex)
	{
		Exception ex2 = ex;
		while (ex2.InnerException != null)
		{
			ex2 = ex2.InnerException;
		}
		return ex2;
	}

	public static Exception Sanitize(this Exception ex)
	{
		return ReduceExceptionTree(ex, SanitizeReducer);
	}

	private static Exception SanitizeReducer(Exception accumulator, Exception current)
	{
		if (accumulator is SanitizedException)
		{
			return new SanitizedException(current, accumulator);
		}
		if (current is AggregateException ex)
		{
			List<Exception> list = new List<Exception>(ex.InnerExceptions.Count);
			bool flag = false;
			foreach (Exception innerException in ex.InnerExceptions)
			{
				Exception ex2 = ReduceExceptionTree(innerException, SanitizeReducer);
				list.Add(ex2);
				if (ex2 != innerException)
				{
					flag = true;
				}
			}
			if (flag)
			{
				return new SanitizedAggregateException(ex, list);
			}
			return ex;
		}
		if (current is SocketException socketException)
		{
			return new SanitizedException(socketException);
		}
		if (current is IOException ioException)
		{
			return new SanitizedException(ioException);
		}
		if (current is WebException webException)
		{
			return new SanitizedException(webException);
		}
		return current;
	}

	private static Exception ReduceExceptionTree(Exception current, Func<Exception, Exception, Exception> reducer)
	{
		Exception arg = null;
		if (current.InnerException != null)
		{
			arg = ReduceExceptionTree(current.InnerException, reducer);
		}
		return reducer(arg, current);
	}
}
