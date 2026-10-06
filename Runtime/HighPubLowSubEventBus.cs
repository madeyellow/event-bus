using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MadeYellow.EventBus
{
    /// <summary>
    /// Asset-backed bus for many publishers and few listeners.
    /// Publishing does not allocate. Subscriptions exist only at runtime and are cleared when the asset is enabled or disabled.
    /// </summary>
    /// <remarks>
    /// Listeners live on this asset. Two assets do not share subscribers.
    /// Keep the <see cref="Action{T}"/> passed to <see cref="Subscribe{T}"/> and pass that same instance to <see cref="Unsubscribe{T}"/>.
    /// A listener added during <see cref="Publish{T}"/> runs on the next publish. A listener removed during <see cref="Publish{T}"/> still runs for the current publish.
    /// </remarks>
    [CreateAssetMenu(fileName = "High Pub Low Sub Event Bus", menuName = "MadeYellow/High Pub Low Sub Event Bus")]
    public sealed class HighPubLowSubEventBus : ScriptableEventBase
    {
        object[] _slots = Array.Empty<object>();

        void OnEnable() => _slots = Array.Empty<object>();

        void OnDisable() => _slots = Array.Empty<object>();

        /// <inheritdoc />
        public override void Subscribe<T>(Action<T> listener)
        {
            if (listener == null)
                return;

            var channel = ChannelFor<T>();
            var current = channel.Handlers;
            var next = new Action<T>[current.Length + 1];
            Array.Copy(current, next, current.Length);
            next[current.Length] = listener;
            channel.Handlers = next;
        }

        /// <inheritdoc />
        public override void Unsubscribe<T>(Action<T> listener)
        {
            if (listener == null)
                return;

            var id = TypeId<T>.Value;
            if ((uint)id >= (uint)_slots.Length)
                return;

            if (_slots[id] is not Channel<T> channel)
                return;

            var current = channel.Handlers;
            var index = Array.LastIndexOf(current, listener);
            if (index < 0)
                return;

            if (current.Length == 1)
            {
                _slots[id] = null;
                return;
            }

            var next = new Action<T>[current.Length - 1];
            Array.Copy(current, 0, next, 0, index);
            Array.Copy(current, index + 1, next, index, current.Length - index - 1);
            channel.Handlers = next;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Publish<T>(in T eventData)
        {
            var slots = _slots;
            var id = TypeId<T>.Value;
            if ((uint)id >= (uint)slots.Length)
                return;

            if (slots[id] is not Channel<T> channel)
                return;

            channel.Publish(in eventData);
        }

        Channel<T> ChannelFor<T>() where T : struct
        {
            var id = TypeId<T>.Value;
            if ((uint)id >= (uint)_slots.Length)
            {
                var grown = new object[id + 1];
                Array.Copy(_slots, grown, _slots.Length);
                _slots = grown;
            }

            if (_slots[id] is Channel<T> channel)
                return channel;

            channel = new Channel<T>();
            _slots[id] = channel;
            return channel;
        }

        static class TypeId
        {
            static int _next;

            public static int Next() => _next++;
        }

        static class TypeId<T> where T : struct
        {
            public static readonly int Value = TypeId.Next();
        }

        sealed class Channel<T> where T : struct
        {
            public Action<T>[] Handlers = Array.Empty<Action<T>>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish(in T eventData)
            {
                var handlers = Handlers;
                for (var i = 0; i < handlers.Length; i++)
                    handlers[i](eventData);
            }
        }
    }
}
