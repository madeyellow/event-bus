# EVENT BUS: Typed publish/subscribe for Unity

[![Unity](https://img.shields.io/badge/Unity-6000.0-black?logo=unity&logoColor=white)](https://unity.com/)

> A small event bus for Unity 6. Publish a struct, and every listener of that exact type receives it.

Pass messages between systems without a direct reference. Share one `HighPubLowSubEventBus` asset across the project, or keep an `EventBus` in code.

## Why you'll use it

* **Typed listeners:** `Subscribe<T>` only receives events of type `T`. A damage event never reaches a heal listener.
* **Struct events:** `T` is a struct, so the payload is not boxed.
* **Asset for a hot path:** `HighPubLowSubEventBus` is built for many publishers, few listeners, and a high publish rate. Publishing does not allocate.
* **Isolated assets:** Each asset has its own listeners. Publishing on one does not reach another.
* **No leftover listeners:** The asset clears its subscriptions when it is enabled or disabled, so they do not survive a domain reload.

## Installation

* Open **Unity Package Manager** `Window > Package Management > Package Manager`;
* Click "+" → `Add package from git URL`;
* Paste `https://github.com/madeyellow/event-bus.git`;
* Hit **Install**.

## Getting started

Create an asset via `Create > MadeYellow > High Pub Low Sub Event Bus` and assign it to a `HighPubLowSubEventBus` field.

`EventBus` is the in-memory bus for code and tests that do not need an asset. `ScriptableEventBus` is obsolete.

```csharp
struct Damage
{
    public int amount;
}

public sealed class Health : MonoBehaviour
{
    [SerializeField] HighPubLowSubEventBus bus;
    Action<Damage> onDamage;

    void OnEnable()
    {
        onDamage = OnDamage;
        bus.Subscribe(onDamage);
    }

    void OnDisable()
    {
        bus.Unsubscribe(onDamage);
    }

    void OnDamage(Damage damage)
    {
        // damage.amount
    }
}
```

Publish from anywhere that holds the same asset:

```csharp
var damage = new Damage { amount = 5 };
bus.Publish(in damage);
```

## API

`Subscribe<T>`, `Unsubscribe<T>`, and `Publish<T>(in T)` accept a struct only. `Publish<T>` invokes every callback registered for `T` on that bus. `Unsubscribe<T>` removes one callback.

`HighPubLowSubEventBus` and `ScriptableEventBus` both derive from `ScriptableEventBase`. Use `HighPubLowSubEventBus`. Listeners are stored on the asset, so each asset is its own bus.

`EventBusInfo.PackageName` and `EventBusInfo.Version` match this package.

### Avoiding GC

Publishing does not allocate. Subscribing and unsubscribing allocate a new listener array; do that when the listener starts and stops, not per event.

* Store the callback in a field (`onDamage = OnDamage`) and pass that field to `Subscribe` and `Unsubscribe`. A lambda written at the call site allocates a delegate every time it runs, and a capturing lambda also allocates a closure.
* Subscribe from `OnEnable` with the cached delegate. The asset drops listeners when it is enabled or disabled, so a delegate created once in `Awake` is the instance you register again after the next enable.
* Pass the struct to `Publish` by value or with `in`. Do not cast it to `object`.
* The listener runs on the publisher's stack. On a hot event, keep `new`, boxing, LINQ, and string work out of that method.
* A listener added during `Publish` runs on the next publish. A listener removed during `Publish` still runs for the current one.
