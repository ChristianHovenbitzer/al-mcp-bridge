using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal static class Logger
{
	private class RoslynLogBlock : IDisposable
	{
		private readonly ObjectPool<RoslynLogBlock> pool;

		private ILogger logger;

		private LogMessage logMessage;

		private CancellationToken cancellationToken;

		private FunctionId functionId;

		private int tick;

		private int blockId;

		public RoslynLogBlock(ObjectPool<RoslynLogBlock> pool)
		{
			this.pool = pool;
		}

		public void Construct(ILogger newLogger, FunctionId newFunctionId, LogMessage newLogMessage, int newBlockId, CancellationToken newCancellationToken)
		{
			logger = newLogger;
			functionId = newFunctionId;
			logMessage = newLogMessage;
			tick = Environment.TickCount;
			blockId = newBlockId;
			cancellationToken = newCancellationToken;
			newLogger.LogBlockStart(newFunctionId, newLogMessage, newBlockId, newCancellationToken);
		}

		public void Dispose()
		{
			if (logger != null)
			{
				int delta = Environment.TickCount - tick;
				logger.LogBlockEnd(functionId, logMessage, blockId, delta, cancellationToken);
				logMessage.Free();
				logMessage = null;
				logger = null;
				cancellationToken = default(CancellationToken);
				pool.Free(this);
			}
		}
	}

	private static ILogger currentLogger;

	private static int lastUniqueBlockId;

	private static readonly ObjectPool<RoslynLogBlock> objectPool = new ObjectPool<RoslynLogBlock>(() => new RoslynLogBlock(objectPool), Math.Min(Environment.ProcessorCount * 8, 256));

	public static ILogger SetLogger(ILogger logger)
	{
		return Interlocked.Exchange(ref currentLogger, logger);
	}

	public static ILogger GetLogger()
	{
		return currentLogger;
	}

	public static void Log(FunctionId functionId, string message = null)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			logger.Log(functionId, LogMessage.Create(message));
		}
	}

	public static void Log(FunctionId functionId, Func<string> messageGetter)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			LogMessage logMessage = LogMessage.Create(messageGetter);
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	public static void Log<TArg>(FunctionId functionId, Func<TArg, string> messageGetter, TArg arg)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			LogMessage logMessage = LogMessage.Create(messageGetter, arg);
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	public static void Log<TArg0, TArg1>(FunctionId functionId, Func<TArg0, TArg1, string> messageGetter, TArg0 arg0, TArg1 arg1)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			LogMessage logMessage = LogMessage.Create(messageGetter, arg0, arg1);
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	public static void Log<TArg0, TArg1, TArg2>(FunctionId functionId, Func<TArg0, TArg1, TArg2, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			LogMessage logMessage = LogMessage.Create(messageGetter, arg0, arg1, arg2);
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	public static void Log<TArg0, TArg1, TArg2, TArg3>(FunctionId functionId, Func<TArg0, TArg1, TArg2, TArg3, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			LogMessage logMessage = LogMessage.Create(messageGetter, arg0, arg1, arg2, arg3);
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	public static void Log(FunctionId functionId, LogMessage logMessage)
	{
		ILogger logger = GetLogger();
		if (logger != null && logger.IsEnabled(functionId))
		{
			logger.Log(functionId, logMessage);
			logMessage.Free();
		}
	}

	private static int GetNextUniqueBlockId()
	{
		return Interlocked.Increment(ref lastUniqueBlockId);
	}

	public static IDisposable LogBlock(FunctionId functionId, CancellationToken token)
	{
		return LogBlock(functionId, string.Empty, token);
	}

	public static IDisposable LogBlock(FunctionId functionId, string message, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(message), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock(FunctionId functionId, Func<string> messageGetter, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(messageGetter), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock<TArg>(FunctionId functionId, Func<TArg, string> messageGetter, TArg arg, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(messageGetter, arg), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock<TArg0, TArg1>(FunctionId functionId, Func<TArg0, TArg1, string> messageGetter, TArg0 arg0, TArg1 arg1, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(messageGetter, arg0, arg1), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock<TArg0, TArg1, TArg2>(FunctionId functionId, Func<TArg0, TArg1, TArg2, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(messageGetter, arg0, arg1, arg2), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock<TArg0, TArg1, TArg2, TArg3>(FunctionId functionId, Func<TArg0, TArg1, TArg2, TArg3, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, LogMessage.Create(messageGetter, arg0, arg1, arg2, arg3), GetNextUniqueBlockId(), token);
	}

	public static IDisposable LogBlock(FunctionId functionId, LogMessage logMessage, CancellationToken token)
	{
		ILogger logger = GetLogger();
		if (logger == null)
		{
			return EmptyLogBlock.Instance;
		}
		if (!logger.IsEnabled(functionId))
		{
			return EmptyLogBlock.Instance;
		}
		return CreateLogBlock(functionId, logMessage, GetNextUniqueBlockId(), token);
	}

	public static Func<FunctionId, bool> GetLoggingChecker(IOptionService optionService)
	{
		IOptionService optionService2 = optionService;
		IEnumerable<FunctionId> source = Enum.GetValues(typeof(FunctionId)).Cast<FunctionId>();
		Dictionary<FunctionId, bool> functionIdOptions = source.ToDictionary((FunctionId id) => id, (FunctionId id) => optionService2.GetOption(FunctionIdOptions.GetOption(id)));
		return (FunctionId functionId) => functionIdOptions[functionId];
	}

	private static IDisposable CreateLogBlock(FunctionId functionId, LogMessage message, int blockId, CancellationToken cancellationToken)
	{
		RoslynLogBlock roslynLogBlock = objectPool.Allocate();
		roslynLogBlock.Construct(currentLogger, functionId, message, blockId, cancellationToken);
		return roslynLogBlock;
	}
}
