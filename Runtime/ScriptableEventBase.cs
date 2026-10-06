using System;
using UnityEngine;

namespace MadeYellow.EventBus
{
    /// <summary>ScriptableObject bus. A concrete asset stores listeners for struct events.</summary>
    public abstract class ScriptableEventBase : ScriptableObject, IEventBus
    {
        /// <inheritdoc />
        public abstract void Subscribe<T>(Action<T> listener) where T : struct;

        /// <inheritdoc />
        public abstract void Unsubscribe<T>(Action<T> listener) where T : struct;

        /// <inheritdoc />
        public abstract void Publish<T>(in T eventData) where T : struct;
    }
}
