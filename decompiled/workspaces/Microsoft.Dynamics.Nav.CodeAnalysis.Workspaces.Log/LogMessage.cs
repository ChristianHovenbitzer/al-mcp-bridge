using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;

internal abstract class LogMessage
{
	private sealed class StaticLogMessage : LogMessage
	{
		private static readonly ObjectPool<StaticLogMessage> objectPool = SharedPools.Default<StaticLogMessage>();

		public static LogMessage Construct(string message)
		{
			StaticLogMessage staticLogMessage = objectPool.Allocate();
			staticLogMessage.message = message;
			return staticLogMessage;
		}

		protected override string CreateMessage()
		{
			return message;
		}

		protected override void FreeCore()
		{
			if (message != null)
			{
				message = null;
				objectPool.Free(this);
			}
		}
	}

	private sealed class LazyLogMessage : LogMessage
	{
		private static readonly ObjectPool<LazyLogMessage> objectPool = SharedPools.Default<LazyLogMessage>();

		private Func<string> messageGetter;

		public static LogMessage Construct(Func<string> messageGetter)
		{
			LazyLogMessage lazyLogMessage = objectPool.Allocate();
			lazyLogMessage.messageGetter = messageGetter;
			return lazyLogMessage;
		}

		protected override string CreateMessage()
		{
			return messageGetter();
		}

		protected override void FreeCore()
		{
			if (messageGetter != null)
			{
				messageGetter = null;
				objectPool.Free(this);
			}
		}
	}

	private sealed class LazyLogMessage<TArg0> : LogMessage
	{
		private static readonly ObjectPool<LazyLogMessage<TArg0>> objectPool = SharedPools.Default<LazyLogMessage<TArg0>>();

		private Func<TArg0, string> messageGetter;

		private TArg0 argument;

		public static LogMessage Construct(Func<TArg0, string> messageGetter, TArg0 arg)
		{
			LazyLogMessage<TArg0> lazyLogMessage = objectPool.Allocate();
			lazyLogMessage.messageGetter = messageGetter;
			lazyLogMessage.argument = arg;
			return lazyLogMessage;
		}

		protected override string CreateMessage()
		{
			return messageGetter(argument);
		}

		protected override void FreeCore()
		{
			if (messageGetter != null)
			{
				messageGetter = null;
				argument = default(TArg0);
				objectPool.Free(this);
			}
		}
	}

	private sealed class LazyLogMessage<TArg0, TArg1> : LogMessage
	{
		private static readonly ObjectPool<LazyLogMessage<TArg0, TArg1>> objectPool = SharedPools.Default<LazyLogMessage<TArg0, TArg1>>();

		private Func<TArg0, TArg1, string> messageGetter;

		private TArg0 arg0;

		private TArg1 arg1;

		internal static LogMessage Construct(Func<TArg0, TArg1, string> messageGetter, TArg0 arg0, TArg1 arg1)
		{
			LazyLogMessage<TArg0, TArg1> lazyLogMessage = objectPool.Allocate();
			lazyLogMessage.messageGetter = messageGetter;
			lazyLogMessage.arg0 = arg0;
			lazyLogMessage.arg1 = arg1;
			return lazyLogMessage;
		}

		protected override string CreateMessage()
		{
			return messageGetter(arg0, arg1);
		}

		protected override void FreeCore()
		{
			if (messageGetter != null)
			{
				messageGetter = null;
				arg0 = default(TArg0);
				arg1 = default(TArg1);
				objectPool.Free(this);
			}
		}
	}

	private sealed class LazyLogMessage<TArg0, TArg1, TArg2> : LogMessage
	{
		private static readonly ObjectPool<LazyLogMessage<TArg0, TArg1, TArg2>> objectPool = SharedPools.Default<LazyLogMessage<TArg0, TArg1, TArg2>>();

		private Func<TArg0, TArg1, TArg2, string> messageGetter;

		private TArg0 arg0;

		private TArg1 arg1;

		private TArg2 arg2;

		public static LogMessage Construct(Func<TArg0, TArg1, TArg2, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2)
		{
			LazyLogMessage<TArg0, TArg1, TArg2> lazyLogMessage = objectPool.Allocate();
			lazyLogMessage.messageGetter = messageGetter;
			lazyLogMessage.arg0 = arg0;
			lazyLogMessage.arg1 = arg1;
			lazyLogMessage.arg2 = arg2;
			return lazyLogMessage;
		}

		protected override string CreateMessage()
		{
			return messageGetter(arg0, arg1, arg2);
		}

		protected override void FreeCore()
		{
			if (messageGetter != null)
			{
				messageGetter = null;
				arg0 = default(TArg0);
				arg1 = default(TArg1);
				arg2 = default(TArg2);
				objectPool.Free(this);
			}
		}
	}

	private sealed class LazyLogMessage<TArg0, TArg1, TArg2, TArg3> : LogMessage
	{
		private static readonly ObjectPool<LazyLogMessage<TArg0, TArg1, TArg2, TArg3>> objectPool = SharedPools.Default<LazyLogMessage<TArg0, TArg1, TArg2, TArg3>>();

		private Func<TArg0, TArg1, TArg2, TArg3, string> messageGetter;

		private TArg0 arg0;

		private TArg1 arg1;

		private TArg2 arg2;

		private TArg3 arg3;

		public static LogMessage Construct(Func<TArg0, TArg1, TArg2, TArg3, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			LazyLogMessage<TArg0, TArg1, TArg2, TArg3> lazyLogMessage = objectPool.Allocate();
			lazyLogMessage.messageGetter = messageGetter;
			lazyLogMessage.arg0 = arg0;
			lazyLogMessage.arg1 = arg1;
			lazyLogMessage.arg2 = arg2;
			lazyLogMessage.arg3 = arg3;
			return lazyLogMessage;
		}

		protected override string CreateMessage()
		{
			return messageGetter(arg0, arg1, arg2, arg3);
		}

		protected override void FreeCore()
		{
			if (messageGetter != null)
			{
				messageGetter = null;
				arg0 = default(TArg0);
				arg1 = default(TArg1);
				arg2 = default(TArg2);
				arg3 = default(TArg3);
				objectPool.Free(this);
			}
		}
	}

	private string message;

	public static LogMessage Create(string message)
	{
		return StaticLogMessage.Construct(message);
	}

	public static LogMessage Create(Func<string> messageGetter)
	{
		return LazyLogMessage.Construct(messageGetter);
	}

	public static LogMessage Create<TArg>(Func<TArg, string> messageGetter, TArg arg)
	{
		return LazyLogMessage<TArg>.Construct(messageGetter, arg);
	}

	public static LogMessage Create<TArg0, TArg1>(Func<TArg0, TArg1, string> messageGetter, TArg0 arg0, TArg1 arg1)
	{
		return LazyLogMessage<TArg0, TArg1>.Construct(messageGetter, arg0, arg1);
	}

	public static LogMessage Create<TArg0, TArg1, TArg2>(Func<TArg0, TArg1, TArg2, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2)
	{
		return LazyLogMessage<TArg0, TArg1, TArg2>.Construct(messageGetter, arg0, arg1, arg2);
	}

	public static LogMessage Create<TArg0, TArg1, TArg2, TArg3>(Func<TArg0, TArg1, TArg2, TArg3, string> messageGetter, TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		return LazyLogMessage<TArg0, TArg1, TArg2, TArg3>.Construct(messageGetter, arg0, arg1, arg2, arg3);
	}

	protected abstract string CreateMessage();

	protected abstract void FreeCore();

	public string GetMessage()
	{
		if (message == null)
		{
			message = CreateMessage();
		}
		return message;
	}

	public void Free()
	{
		message = null;
		FreeCore();
	}
}
