# Event Bus

Typed publish/subscribe for Unity 6. One bus instance keeps listeners apart by event type.

Create a bus in code with `EventBus`, or create a shared asset from **Assets > Create > MadeYellow > Event Bus**. The asset implements the same `IEventBus` API. Listeners on that asset are cleared when it is enabled or disabled, so they do not survive a domain reload.

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

Prefer a struct for the event type so the payload is not allocated on the heap. `Subscribe`, `Unsubscribe`, and `Publish` only talk to listeners registered for that exact type.
