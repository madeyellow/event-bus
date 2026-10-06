# Event Bus

Typed publish/subscribe for Unity 6. One bus instance keeps listeners apart by event type. Event types are structs.

Create a shared asset from **Assets > Create > MadeYellow > High Pub Low Sub Event Bus**, or create an in-memory bus with `EventBus`. The asset implements `IEventBus` through `ScriptableEventBase`. Listeners on that asset are cleared when it is enabled or disabled, so they do not survive a domain reload. Each asset has its own listeners.

`HighPubLowSubEventBus` does not allocate when publishing. Cache the `Action<T>` in a field and pass that same instance to `Subscribe` and `Unsubscribe`.

```csharp
struct Damage
{
    public int amount;
}

[SerializeField] HighPubLowSubEventBus bus;
Action<Damage> onDamage;

void OnEnable()
{
    onDamage = OnDamage;
    bus.Subscribe(onDamage);
}

void OnDamage(Damage damage)
{
    // damage.amount
}

void DealDamage()
{
    var damage = new Damage { amount = 5 };
    bus.Publish(in damage);
}
```

`Subscribe`, `Unsubscribe`, and `Publish` only talk to listeners registered for that exact type on that bus.
