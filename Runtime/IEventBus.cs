using System;

namespace MadeYellow.EventBus
{
    /// <summary>Accepts a struct event and forwards it to subscribers.</summary>
    public interface IEventBus
    {
        /// <summary>Registers <paramref name="listener"/> for events of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type. Must be a struct so the payload is not boxed.</typeparam>
        /// <param name="listener">Called when an event of type <typeparamref name="T"/> is published. Pass the same instance to <see cref="Unsubscribe{T}"/>.</param>
        void Subscribe<T>(Action<T> listener) where T : struct;

        /// <summary>Removes <paramref name="listener"/> from events of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type. Must be a struct so the payload is not boxed.</typeparam>
        /// <param name="listener">The callback instance previously passed to <see cref="Subscribe{T}"/>.</param>
        void Unsubscribe<T>(Action<T> listener) where T : struct;

        /// <summary>Sends <paramref name="eventData"/> to every subscriber of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type. Must be a struct so the payload is not boxed.</typeparam>
        /// <param name="eventData">Payload delivered to subscribers.</param>
        void Publish<T>(in T eventData) where T : struct;
    }
}
