using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadeYellow.EventBus
{
    /// <summary>
    /// Asset-backed bus. Subscriptions exist only at runtime and are cleared when the asset is enabled or disabled.
    /// </summary>
    [CreateAssetMenu(fileName = "Event Bus", menuName = "MadeYellow/Event Bus")]
    [Icon("Packages/com.madeyellow.event-bus/Editor/Icons/event-icon.png")]
    public sealed class ScriptableEventBus : ScriptableObject, IEventBus
    {
        /// <summary>Subscriptions. The key is the event type; the value is an <see cref="Action{T}"/> multicast delegate.</summary>
        readonly Dictionary<Type, Delegate> _events = new();

        void OnEnable() => _events.Clear();

        void OnDisable() => _events.Clear();

        /// <inheritdoc />
        public void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var existing))
                _events[type] = Delegate.Combine(existing, listener);
            else
                _events[type] = listener;
        }

        /// <inheritdoc />
        public void Unsubscribe<T>(Action<T> listener)
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
        public void Publish<T>(T eventData)
        {
            if (_events.TryGetValue(typeof(T), out var action))
                ((Action<T>)action)?.Invoke(eventData);
        }
    }
}
