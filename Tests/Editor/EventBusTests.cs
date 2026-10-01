using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MadeYellow.EventBus
{
    public class EventBusInfoTests
    {
        [Test]
        public void Version_IsSet()
        {
            Assert.That(EventBusInfo.Version, Is.Not.Null.And.Not.Empty);
            Assert.That(EventBusInfo.PackageName, Is.EqualTo("com.madeyellow.event-bus"));
        }
    }

    public class EventBusTests
    {
        struct Damage
        {
            public int amount;
        }

        struct Heal
        {
            public int amount;
        }

        [Test]
        public void Publish_InvokesSubscribersOfThatType()
        {
            var bus = new EventBus();
            var received = 0;
            bus.Subscribe<Damage>(e => received += e.amount);

            bus.Publish(new Damage { amount = 5 });

            Assert.That(received, Is.EqualTo(5));
        }

        [Test]
        public void Publish_DoesNotInvokeOtherEventTypes()
        {
            var bus = new EventBus();
            var received = 0;
            bus.Subscribe<Damage>(_ => received++);

            bus.Publish(new Heal { amount = 1 });

            Assert.That(received, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var bus = new EventBus();
            var received = 0;
            Action<Damage> listener = _ => received++;
            bus.Subscribe(listener);
            bus.Unsubscribe(listener);

            bus.Publish(new Damage { amount = 1 });

            Assert.That(received, Is.EqualTo(0));
        }

        [Test]
        public void Unsubscribe_RemovesOnlyTheGivenListener()
        {
            var bus = new EventBus();
            var first = 0;
            var second = 0;
            Action<Damage> firstListener = _ => first++;
            Action<Damage> secondListener = _ => second++;
            bus.Subscribe(firstListener);
            bus.Subscribe(secondListener);
            bus.Unsubscribe(firstListener);

            bus.Publish(new Damage { amount = 1 });

            Assert.That(first, Is.EqualTo(0));
            Assert.That(second, Is.EqualTo(1));
        }
    }

    public class ScriptableEventBusTests
    {
        [Test]
        public void Publish_InvokesSubscribers()
        {
            var bus = ScriptableObject.CreateInstance<ScriptableEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<int>(value => received += value);

                bus.Publish(3);

                Assert.That(received, Is.EqualTo(3));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void OnDisable_DropsSubscriptions()
        {
            var bus = ScriptableObject.CreateInstance<ScriptableEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<int>(_ => received++);
                bus.Publish(1);

                InvokeLifecycle(bus, "OnDisable");
                bus.Publish(1);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        static void InvokeLifecycle(ScriptableEventBus bus, string methodName)
        {
            var method = typeof(ScriptableEventBus).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(bus, null);
        }
    }
}
