using System;
using System.Collections.Generic;

namespace Lanternfall.Core.Events
{

/// <summary>Synchronous typed events with disposable ownership.</summary>
public sealed class GameEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public IDisposable Subscribe<T>(Action<T> handler) where T : struct
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            _handlers.Add(typeof(T), list = new List<Delegate>());
        if (!list.Contains(handler)) list.Add(handler);
        return new Subscription<T>(this, handler);
    }

    public void Publish<T>(T message) where T : struct
    {
        if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list)) return;
        foreach (Delegate item in list.ToArray()) ((Action<T>)item)(message);
    }

    private void Remove<T>(Action<T> handler) where T : struct
    {
        if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            list.Remove(handler);
    }

    private sealed class Subscription<T> : IDisposable where T : struct
    {
        private GameEventBus _owner;
        private Action<T> _handler;
        public Subscription(GameEventBus owner, Action<T> handler)
        { _owner = owner; _handler = handler; }
        public void Dispose()
        {
            _owner?.Remove(_handler);
            _owner = null;
            _handler = null;
        }
    }
}

}
