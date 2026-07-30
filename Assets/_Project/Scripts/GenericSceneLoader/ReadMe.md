# Generic Scene Loader

A lightweight, reusable scene management system for Unity that wraps Unity's built-in `SceneManager` API while providing additional validation, events, and helper utilities.

This package is designed to be copied into any Unity project without requiring project-specific dependencies.

---

# Features

* Load scenes by **name** or **Build Index**
* Load scenes **synchronously** or **asynchronously**
* Load and unload **additive scenes**
* Prevent duplicate additive scene loads
* Set the active scene
* Validate build indices
* Check whether a scene exists in Build Settings
* Enumerate all loaded scenes
* Scene loaded, unloaded, and active scene changed events
* Optional fade transition service
* Uses Unity's built-in logging (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`)
* No third-party dependencies

---

# Folder Structure

```
GenericSceneLoader
│
├── SceneLoader.cs
├── SceneValidator.cs
├── SceneExtensions.cs
└── SceneTransitionService.cs
```

---

# Installation

1. Copy the scripts into your Unity project's `Assets` folder.
2. Add all required scenes to **File → Build Settings**.
3. (Optional) Create a UI Canvas with a `CanvasGroup` if you want to use fade transitions.

No additional setup is required.

---

# Basic Usage

## Load a Scene

```csharp
SceneLoader.Load("MainMenu");
```

or

```csharp
SceneLoader.Load(0);
```

---

## Load a Scene Asynchronously

```csharp
SceneLoader.LoadAsync("Gameplay");
```

---

## Load an Additive Scene

```csharp
SceneLoader.LoadAdditive("HUD");
```

If the scene is already loaded or is currently loading, the loader safely ignores the request.

---

## Unload an Additive Scene

```csharp
SceneLoader.Unload("HUD");
```

---

## Check if a Scene is Loaded

```csharp
bool loaded = SceneLoader.IsLoaded("Gameplay");
```

---

## Set the Active Scene

```csharp
SceneLoader.SetActive("Gameplay");
```

---

## Get the Active Scene

```csharp
Scene active = SceneLoader.ActiveScene;
```

---

# Scene Validation

## Validate Build Index

```csharp
bool valid = SceneValidator.IsValidBuildIndex(index);
```

---

## Check if a Scene Exists

```csharp
bool exists = SceneValidator.SceneExists("MainMenu");
```

This checks whether the scene is included in Unity's Build Settings.

---

# Enumerate Loaded Scenes

```csharp
foreach (Scene scene in SceneExtensions.LoadedScenes)
{
    Debug.Log(scene.name);
}
```

---

# Scene Events

The loader exposes Unity scene events through static C# events.

```csharp
SceneLoader.SceneLoaded += OnSceneLoaded;
SceneLoader.SceneUnloaded += OnSceneUnloaded;
SceneLoader.ActiveSceneChanged += OnActiveSceneChanged;
```

Example:

```csharp
private void OnEnable()
{
    SceneLoader.SceneLoaded += HandleSceneLoaded;
}

private void OnDisable()
{
    SceneLoader.SceneLoaded -= HandleSceneLoaded;
}

private void HandleSceneLoaded(Scene scene)
{
    Debug.Log($"Loaded: {scene.name}");
}
```

---

# Fade Transition Service

The included `SceneTransitionService` provides a simple fade transition.

## Setup

1. Create a full-screen Canvas.
2. Add a black Image.
3. Add a `CanvasGroup` to the Image.
4. Attach `SceneTransitionService` to a GameObject.
5. Assign the `CanvasGroup`.

Example:

```csharp
transitionService.Transition("Gameplay");
```

The service will:

1. Fade Out
2. Load the Scene
3. Fade In

This implementation is intentionally simple and can be extended with loading screens, progress bars, or custom animations.

---

# Logging

This package uses Unity's built-in logging system.

```csharp
Debug.Log(...);
Debug.LogWarning(...);
Debug.LogError(...);
```

No custom logging framework is required.

---

# Design Goals

This package was built with the following principles in mind:

* Lightweight
* Reusable
* No project-specific code
* Easy to understand
* Easy to extend
* Safe defaults
* Minimal API surface

It serves as a foundation for more advanced scene management systems.

---

# Future Improvements

Possible extensions include:

* Loading progress reporting
* Async/await support
* Addressables support
* Loading screen interface
* Scene references (instead of strings)
* Assembly Definitions (.asmdef)
* Unity Package Manager (UPM) support
* Unit tests
* Scene history and back navigation
* Persistent bootstrap scene support
* Scene groups

---

# License

This project is provided as a reusable utility for Unity projects. Feel free to modify and extend it to fit your project's needs.
