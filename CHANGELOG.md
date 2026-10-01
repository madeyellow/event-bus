# Changelog

## [0.1.0] - 2026-10-01

- Package scaffold for Unity 6: runtime assembly and edit-mode tests.
- Typed publish/subscribe through `EventBus`, or through a `ScriptableEventBus` asset created from **Assets > Create > MadeYellow > Event Bus**.
- Subscriptions on a `ScriptableEventBus` asset are cleared when the asset is enabled or disabled, so they do not survive a domain reload.
