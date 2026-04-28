using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class EventMap
{
	private class Registry<TEventHandler> : IEquatable<Registry<TEventHandler>> where TEventHandler : class
	{
		private TEventHandler handler;

		public Registry(TEventHandler handler)
		{
			this.handler = handler;
		}

		public void Unregister()
		{
			handler = null;
		}

		public void Invoke(Action<TEventHandler> invoker)
		{
			TEventHandler val = handler;
			if (val != null)
			{
				invoker(val);
			}
		}

		public bool HasHandler(TEventHandler testHandler)
		{
			return testHandler.Equals(handler);
		}

		public bool Equals(Registry<TEventHandler> other)
		{
			if (other == null)
			{
				return false;
			}
			if (other.handler == null && handler == null)
			{
				return true;
			}
			if (other.handler == null || handler == null)
			{
				return false;
			}
			return other.handler.Equals(handler);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Registry<TEventHandler>);
		}

		public override int GetHashCode()
		{
			if (handler != null)
			{
				return handler.GetHashCode();
			}
			return 0;
		}
	}

	internal struct EventHandlerSet<TEventHandler> where TEventHandler : class
	{
		private ImmutableArray<Registry<TEventHandler>> registries;

		public bool HasHandlers
		{
			get
			{
				if (registries != null)
				{
					return registries.Length > 0;
				}
				return false;
			}
		}

		internal EventHandlerSet(object registries)
		{
			this.registries = (ImmutableArray<Registry<TEventHandler>>)registries;
		}

		public void RaiseEvent(Action<TEventHandler> invoker)
		{
			if (HasHandlers)
			{
				ImmutableArray<Registry<TEventHandler>>.Enumerator enumerator = registries.GetEnumerator();
				while (enumerator.MoveNext())
				{
					enumerator.Current.Invoke(invoker);
				}
			}
		}
	}

	private readonly NonReentrantLock guard = new NonReentrantLock();

	private readonly Dictionary<string, object> eventNameToRegistries = new Dictionary<string, object>();

	public void AddEventHandler<TEventHandler>(string eventName, TEventHandler eventHandler) where TEventHandler : class
	{
		using (guard.DisposableWait())
		{
			ImmutableArray<Registry<TEventHandler>> registries = GetRegistries_NoLock<TEventHandler>(eventName).Add(new Registry<TEventHandler>(eventHandler));
			SetRegistries_NoLock(eventName, registries);
		}
	}

	public void RemoveEventHandler<TEventHandler>(string eventName, TEventHandler eventHandler) where TEventHandler : class
	{
		TEventHandler eventHandler2 = eventHandler;
		using (guard.DisposableWait())
		{
			ImmutableArray<Registry<TEventHandler>> registries_NoLock = GetRegistries_NoLock<TEventHandler>(eventName);
			ImmutableArray<Registry<TEventHandler>> immutableArray = registries_NoLock.RemoveAll((Registry<TEventHandler> r) => r.HasHandler(eventHandler2));
			if (!(immutableArray != registries_NoLock))
			{
				return;
			}
			foreach (Registry<TEventHandler> item in registries_NoLock.Where((Registry<TEventHandler> r) => r.HasHandler(eventHandler2)))
			{
				item.Unregister();
			}
			SetRegistries_NoLock(eventName, immutableArray);
		}
	}

	public EventHandlerSet<TEventHandler> GetEventHandlers<TEventHandler>(string eventName) where TEventHandler : class
	{
		return new EventHandlerSet<TEventHandler>(GetRegistries<TEventHandler>(eventName));
	}

	private ImmutableArray<Registry<TEventHandler>> GetRegistries<TEventHandler>(string eventName) where TEventHandler : class
	{
		using (guard.DisposableWait())
		{
			return GetRegistries_NoLock<TEventHandler>(eventName);
		}
	}

	private ImmutableArray<Registry<TEventHandler>> GetRegistries_NoLock<TEventHandler>(string eventName) where TEventHandler : class
	{
		guard.AssertHasLock();
		if (eventNameToRegistries.TryGetValue(eventName, out object value))
		{
			return (ImmutableArray<Registry<TEventHandler>>)value;
		}
		return ImmutableArray.Create<Registry<TEventHandler>>();
	}

	private void SetRegistries_NoLock<TEventHandler>(string eventName, ImmutableArray<Registry<TEventHandler>> registries) where TEventHandler : class
	{
		guard.AssertHasLock();
		eventNameToRegistries[eventName] = registries;
	}
}
