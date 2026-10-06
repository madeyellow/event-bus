# Changelog

## [1.1.0] - 2026-10-06

- `IEventBus` accepts struct events only. `Publish<T>` takes the payload as `in T`.
- `ScriptableEventBase` is the abstract ScriptableObject bus. `HighPubLowSubEventBus` stores listeners on the asset, keeps buses isolated, and does not allocate when publishing.
- `ScriptableEventBus` is obsolete. Use `HighPubLowSubEventBus`, created from **Assets > Create > MadeYellow > High Pub Low Sub Event Bus**.

## [0.1.0] - 2026-10-01

- Package scaffold for Unity 6: runtime assembly and edit-mode tests.
- Typed publish/subscribe through `EventBus`, or through a `ScriptableEventBus` asset created from **Assets > Create > MadeYellow > Event Bus**.
- Subscriptions on a `ScriptableEventBus` asset are cleared when the asset is enabled or disabled, so they do not survive a domain reload.
