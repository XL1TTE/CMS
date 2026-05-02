<h1 align="center">CMS</h1>

<p align="center">
  <b>A tiny, code-first Content Management System for Unity.</b><br/>
  <sub>Define your game's data like you define your code — entities, components, and zero ScriptableObject boilerplate.</sub>
</p>

<p align="center">
  <a href="#installation"><img src="https://img.shields.io/badge/Unity-2020.1%2B-black?logo=unity" alt="Unity 2020.1+"/></a>
  <a href="./LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"/></a>
  <a href="https://openupm.com/"><img src="https://img.shields.io/badge/registry-OpenUPM-1B1B1B?logo=unity" alt="OpenUPM"/></a>
  <a href="https://www.twitch.tv/xkoster"><img src="https://img.shields.io/badge/twitch-xkoster-9146FF?logo=twitch&logoColor=white" alt="Twitch"/></a>
</p>

---

## ✨ Why CMS?

Game data is code. Stop dragging ScriptableObjects around. Stop renaming fields and watching half your asset references vanish. Stop writing the same `[CreateAssetMenu]` boilerplate for the 400th time.

**CMS** is a minimal, reflection-driven content system that treats your game definitions as plain C# classes — and still lets you author content from the inspector when you want to.

```csharp
public class FireballSpell : CMSEntity { }

CMS.Init();
var spell = CMS.Get<FireballSpell>();
```

That's it. No menus. No assets. No GUIDs. Just types.

---

## 🚀 Features

- 🧩 **Entity-Component definitions** — compose data the way you compose behaviors.
- 🔍 **Auto-discovery** — every `CMSEntity` subclass is registered for free via reflection.
- 📦 **Hybrid authoring** — code-defined entities + optional Unity prefabs from `Resources/CMS/`.
- ⚡ **Hot-reload** — `CMS → Reload` from the menu bar, no domain reload required.
- 🍃 **Tiny surface area** — one static class, a handful of helpers, ~200 LOC of runtime.
- 🧠 **Type-safe lookups** — `CMS.Get<T>()` and `CMS.GetAll<T>()` return strongly-typed results.

---

## 📦 Installation

### Option 1 — Unity Package Manager (Git URL)

`Window → Package Manager → +  → Add package from git URL`

```
https://github.com/koster/CMS.git
```

### Option 2 — `manifest.json`

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [ "com.mackysoft" ]
    }
  ],
  "dependencies": {
    "com.xk.cms": "https://github.com/koster/CMS.git",
    "com.mackysoft.serializereferenceextensions": "1.6.1"
  }
}
```

> **Requires** Unity `2020.1` or newer and [`serializereference-extensions`](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) for inspector authoring.
>
> 💡 On first import, CMS will prompt you once to add the OpenUPM scoped registry and the `serializereference-extensions` dependency to your `Packages/manifest.json` automatically. Choose *Don't ask again* to silence it; re-run any time via **CMS → Check Dependencies**.

---

## 💻 Quick Start

### 1. Define an entity

```csharp
public class GoblinDef : CMSEntity
{
    public GoblinDef()
    {
        Define<HealthData>().max = 30;
        Define<SFXArray>().files = new() { /* AudioClips */ };
    }
}

[Serializable]
public class HealthData : EntityComponentDefinition
{
    public int max = 1;
}
```

### 2. Boot the system

```csharp
void Awake() => CMS.Init();
```

### 3. Query your data

```csharp
// By type
var goblin = CMS.Get<GoblinDef>();

// By component
var health = CMS.GetData<HealthData>("GoblinDef");

// All of a kind
foreach (var (entity, sfx) in CMS.GetAllData<SFXArray>())
    Play(sfx);
```

---

## 🧰 Authoring from the Editor

Need designers in the loop? Drop a `CMSEntityPfb` MonoBehaviour onto a prefab in `Assets/ldgame/data/Resources/CMS/` and pick components in the inspector via the `[SubclassSelector]` dropdown. CMS will pick it up automatically on `Init()`.

---

## 🔧 Menu Items

| Menu                          | Action                                                 |
| ----------------------------- | ------------------------------------------------------ |
| `CMS → Reload`                | Re-scans assemblies + Resources folder.                |
| `CMS → Check Dependencies`    | Verify OpenUPM registry + required deps in manifest.   |

---

## 📁 Project Structure

```
CMS/
├── Runtime/
│   ├── CMS/
│   │   ├── CMS.cs                      # the whole runtime
│   │   └── Common/                     # built-in components (SFX, tags, etc.)
│   └── Editor/
│       ├── CMSMenuItems.cs             # CMS → Reload
│       └── Bootstrap/                  # standalone editor asmdef
│           └── CMSDependencyBootstrap  # auto-installs OpenUPM dep
├── package.json
└── LICENSE.md
```

---

## 🤝 Contributing

PRs and issues welcome. Keep the runtime small and the API obvious — that's the whole pitch.

---

## 📜 License

[MIT](./LICENSE.md) © [xk](https://www.twitch.tv/xkoster)
