using System;
using System.Collections.Generic;

namespace MadeYellow.EventBus
{
    /// <summary>In-memory publish/subscribe bus keyed by event type.</summary>
    public sealed class EventBus : IEventBus
    {
        /// <summary>Subscriptions. The key is the event type; the value is an <see cref="Action{T}"/> multicast delegate.</summary>
        readonly Dictionary<Type, Delegate> _events = new();

        /// <inheritdoc />
        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var existing))
                _events[type] = Delegate.Combine(existing, listener);
            else
                _events[type] = listener;
        }

        /// <inheritdoc />
        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (!_events.TryGetValue(type, out var existing))
                return;

            var result = Delegate.Remove(existing, listener);
            if (result == null)
                _events.Remove(type);
            else
                _events[type] = result;
        }

        /// <inheritdoc />
        public void Publish<T>(in T eventData) where T : struct
        {
            if (_events.TryGetValue(typeof(T), out var action))
                ((Action<T>)action)?.Invoke(eventData);
        }
    }
}
