using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class WeakEventHandler<TArgs>
{
	public static EventHandler<TArgs> Create<TTarget>(TTarget target, Action<TTarget, object, TArgs> invoker) where TTarget : class
	{
		Action<TTarget, object, TArgs> invoker2 = invoker;
		WeakReference<TTarget> weakTarget = new WeakReference<TTarget>(target);
		return delegate(object? sender, TArgs args)
		{
			if (weakTarget.TryGetTarget(out var target2))
			{
				invoker2(target2, sender, args);
			}
		};
	}
}
