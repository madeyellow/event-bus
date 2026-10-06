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

#pragma warning disable CS0618
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
#pragma warning restore CS0618

    public class HighPubLowSubEventBusTests
    {
        struct Damage
        {
            public int amount;
        }

        struct Heal
        {
            public int amount;
        }

        struct GapProbe
        {
            public int value;
        }

        [Test]
        public void Publish_InvokesSubscribersOfThatType()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<Damage>(e => received += e.amount);

                var damage = new Damage { amount = 5 };
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(5));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_DoesNotInvokeOtherEventTypes()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<Damage>(_ => received++);

                var heal = new Heal { amount = 1 };
                bus.Publish(in heal);

                Assert.That(received, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_StaysOnTheBusThatWasPublishedTo()
        {
            var first = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            var second = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var firstReceived = 0;
                var secondReceived = 0;
                first.Subscribe<Damage>(_ => firstReceived++);
                second.Subscribe<Damage>(_ => secondReceived++);

                var damage = new Damage { amount = 1 };
                first.Publish(in damage);

                Assert.That(firstReceived, Is.EqualTo(1));
                Assert.That(secondReceived, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Unsubscribe_RemovesOnlyTheGivenListener()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var first = 0;
                var second = 0;
                Action<Damage> firstListener = _ => first++;
                Action<Damage> secondListener = _ => second++;
                bus.Subscribe(firstListener);
                bus.Subscribe(secondListener);
                bus.Unsubscribe(firstListener);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(first, Is.EqualTo(0));
                Assert.That(second, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_DuringPublish_StillInvokesTheCurrentList()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = null;
                listener = _ =>
                {
                    received++;
                    bus.Unsubscribe(listener);
                };
                bus.Subscribe(listener);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Subscribe_DuringPublish_RunsOnTheNextPublish()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var extra = 0;
                var added = false;
                Action<Damage> second = _ => extra++;
                Action<Damage> first = _ =>
                {
                    if (added)
                        return;

                    added = true;
                    bus.Subscribe(second);
                };
                bus.Subscribe(first);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                Assert.That(extra, Is.EqualTo(0));

                bus.Publish(in damage);
                Assert.That(extra, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void OnDisable_DropsSubscriptions()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<Damage>(_ => received++);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                InvokeLifecycle(bus, "OnDisable");
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void OnEnable_DropsSubscriptions()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                bus.Subscribe<Damage>(_ => received++);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                InvokeLifecycle(bus, "OnEnable");
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Subscribe_AfterDisable_ReceivesLaterPublishes()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = _ => received++;
                bus.Subscribe(listener);
                InvokeLifecycle(bus, "OnDisable");

                bus.Subscribe(listener);
                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNothing()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var damage = new Damage { amount = 1 };

                Assert.DoesNotThrow(() => bus.Publish(in damage));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_AfterLastListenerIsRemoved_DoesNothing()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = _ => received++;
                bus.Subscribe(listener);
                bus.Unsubscribe(listener);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Subscribe_AfterTheSlotWasCleared_ReceivesPublishes()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var firstCount = 0;
                var secondCount = 0;
                Action<Damage> first = _ => firstCount++;
                Action<Damage> second = _ => secondCount++;
                bus.Subscribe(first);
                bus.Unsubscribe(first);
                bus.Subscribe(second);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(firstCount, Is.EqualTo(0));
                Assert.That(secondCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void NullListener_IsIgnored()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = _ => received++;
                bus.Subscribe(listener);

                Assert.DoesNotThrow(() => bus.Subscribe<Damage>(null));
                Assert.DoesNotThrow(() => bus.Unsubscribe<Damage>(null));

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_UnknownListener_LeavesTheOthers()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = _ => received++;
                Action<Damage> missing = _ => received += 10;
                bus.Subscribe(listener);

                Assert.DoesNotThrow(() => bus.Unsubscribe(missing));

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(received, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_BeforeAnySubscription_DoesNothing()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                Action<Damage> listener = _ => { };

                Assert.DoesNotThrow(() => bus.Unsubscribe(listener));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_InvokesListenersInSubscriptionOrder()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var order = string.Empty;
                Action<Damage> first = _ => order += "a";
                Action<Damage> second = _ => order += "b";
                Action<Damage> third = _ => order += "c";
                bus.Subscribe(first);
                bus.Subscribe(second);
                bus.Subscribe(third);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(order, Is.EqualTo("abc"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_PreservesTheRelativeOrder()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var order = string.Empty;
                Action<Damage> first = _ => order += "a";
                Action<Damage> second = _ => order += "b";
                Action<Damage> third = _ => order += "c";
                bus.Subscribe(first);
                bus.Subscribe(second);
                bus.Subscribe(third);
                bus.Unsubscribe(second);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                Assert.That(order, Is.EqualTo("ac"));

                order = string.Empty;
                bus.Unsubscribe(third);
                bus.Publish(in damage);

                Assert.That(order, Is.EqualTo("a"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_DuplicateListener_RemovesOneOccurrence()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = _ => received++;
                bus.Subscribe(listener);
                bus.Subscribe(listener);
                bus.Unsubscribe(listener);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                Assert.That(received, Is.EqualTo(1));

                bus.Unsubscribe(listener);
                received = 0;
                bus.Publish(in damage);
                Assert.That(received, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Unsubscribe_DuringPublish_StillInvokesTheRestOfTheSnapshot()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var order = string.Empty;
                Action<Damage> later = _ => order += "b";
                Action<Damage> earlier = _ =>
                {
                    order += "a";
                    bus.Unsubscribe(later);
                };
                bus.Subscribe(earlier);
                bus.Subscribe(later);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);
                Assert.That(order, Is.EqualTo("ab"));

                order = string.Empty;
                bus.Publish(in damage);
                Assert.That(order, Is.EqualTo("a"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_OnTheSecondBus_DoesNotReachTheFirst()
        {
            var first = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            var second = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var firstDamage = 0;
                var secondHeal = 0;
                first.Subscribe<Damage>(_ => firstDamage++);
                second.Subscribe<Heal>(_ => secondHeal++);

                var damage = new Damage { amount = 1 };
                var heal = new Heal { amount = 1 };
                second.Publish(in damage);
                first.Publish(in heal);
                second.Publish(in heal);

                Assert.That(firstDamage, Is.EqualTo(0));
                Assert.That(secondHeal, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Subscribe_GrowsAcrossUnusedTypeIds()
        {
            var warmup = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                warmup.Subscribe<Damage>(_ => { });

                var received = 0;
                bus.Subscribe<GapProbe>(probe => received += probe.value);

                var probe = new GapProbe { value = 4 };
                bus.Publish(in probe);

                Assert.That(received, Is.EqualTo(4));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(warmup);
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        [Test]
        public void Publish_DoesNotAllocate()
        {
            var bus = ScriptableObject.CreateInstance<HighPubLowSubEventBus>();
            try
            {
                var received = 0;
                Action<Damage> listener = damage => received += damage.amount;
                bus.Subscribe(listener);

                var damage = new Damage { amount = 1 };
                bus.Publish(in damage);

                var before = GC.GetAllocatedBytesForCurrentThread();
                for (var i = 0; i < 1000; i++)
                    bus.Publish(in damage);
                var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

                Assert.That(allocated, Is.EqualTo(0));
                Assert.That(received, Is.EqualTo(1001));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bus);
            }
        }

        static void InvokeLifecycle(HighPubLowSubEventBus bus, string methodName)
        {
            var method = typeof(HighPubLowSubEventBus).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(bus, null);
        }
    }
}
