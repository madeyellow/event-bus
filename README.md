# EVENT BUS: Typed publish/subscribe for Unity

[![Unity](https://img.shields.io/badge/Unity-6000.0-black?logo=unity&logoColor=white)](https://unity.com/)

> A small event bus for Unity 6. Publish a value, and every listener of that exact type receives it.

Pass messages between systems without a direct reference. Keep a bus in code, or share one `ScriptableEventBus` asset across the project.

## Why you'll use it

* **Typed listeners:** `Subscribe<T>` only receives events of type `T`. A damage event never reaches a heal listener.
* **Two hosts, one API:** `EventBus` is a plain class. `ScriptableEventBus` is an asset you create from the menu and assign in the Inspector.
* **No leftover listeners:** The asset clears its subscriptions when it is enabled or disabled, so they do not survive a domain reload.
* **Small payload:** Prefer a struct for the event. The payload is passed by value.

## Installation

* Open **Unity Package Manager** `Window > Package Management > Package Manager`;
* Click "+" → `Add package from git URL`;
* Paste `https://github.com/madeyellow/event-bus.git`;
* Hit **Install**.

## Getting started

Create an asset via `Create > MadeYellow > Event Bus`, or construct a bus in code.

```csharp
struct Damage
{
    public int amount;
}

var bus = new EventBus();
bus.Subscribe<Damage>(OnDamage);
bus.Publish(new Damage { amount = 5 });
bus.Unsubscribe<Damage>(OnDamage);

void OnDamage(Damage damage)
{
    // damage.amount
}
```

`ScriptableEventBus` implements `IEventBus`, so the same three calls work on the asset.

## API

`Subscribe<T>` registers a callback. `Publish<T>` invokes every callback registered for `T`. `Unsubscribe<T>` removes one callback and drops the type entry when nobody is left.

`EventBusInfo.PackageName` and `EventBusInfo.Version` match this package.
