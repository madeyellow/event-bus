using System;

namespace MadeYellow.EventBus
{
    /// <summary>Accepts an event and forwards it to subscribers.</summary>
    public interface IEventBus
    {
        /// <summary>Registers <paramref name="listener"/> for events of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type. Prefer a struct so the payload is not allocated on the heap.</typeparam>
        /// <param name="listener">Called when an event of type <typeparamref name="T"/> is published.</param>
        void Subscribe<T>(Action<T> listener);

        /// <summary>Removes <paramref name="listener"/> from events of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type. Prefer a struct so the payload is not allocated on the heap.</typeparam>
        /// <param name="listener">The callback previously passed to <see cref="Subscribe{T}"/>.</param>
        void Unsubscribe<T>(Action<T> listener);

        /// <summary>Sends <paramref name="eventData"/> to every subscriber of type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="eventData">Payload delivered to subscribers.</param>
        void Publish<T>(T eventData);
    }
}
