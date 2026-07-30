# Modular Audio System

A lightweight, modular audio management system for Unity built around `ScriptableObject` assets.

The system separates audio data from playback logic, making it easy to organize, reuse, and scale audio across projects while keeping gameplay code clean.

---

# Features

* ScriptableObject-based audio library
* Separate SFX and Music collections
* AudioMixer integration
* Automatic AudioSource creation
* Persistent singleton (`DontDestroyOnLoad`)
* Volume controls
* Enable/Disable Master, SFX, and Music channels
* PlayerPrefs persistence
* Random pitch variation for SFX
* Automatic clip lookup by ID
* Runtime source pooling for sound effects
* Supports looping music and SFX

---

# Architecture

```text
AudioManager
│
├── SFX Library
│     └── SfxClipDataSO
│
├── Music Library
│     └── MusicClipDataSO
│
├── AudioMixer
│
├── Runtime Audio Sources
│     ├── Music Sources
│     └── SFX Sources
│
└── PlayerPrefs
```

---

# Included Scripts

## AudioManager

The central audio controller responsible for:

* Playing SFX
* Playing Music
* Creating AudioSources
* Loading saved settings
* Managing AudioMixer values
* Maintaining clip lookup dictionaries

The manager automatically persists across scene changes using `DontDestroyOnLoad`.

---

## AudioClipDataSO

Base ScriptableObject containing shared audio information.

Properties:

* ID
* Audio Clip
* Default Volume
* Loop
* Audio Mixer Group

The asset automatically synchronizes its ID with the asset name during validation, making asset names the unique identifiers used by the manager.

---

## SfxClipDataSO

ScriptableObject representing a Sound Effect.

Create from:

```text
Create
└── Pharma
    └── Audio
        └── SFX Clip
```

---

## MusicClipDataSO

ScriptableObject representing a Music track.

Create from:

```text
Create
└── Pharma
    └── Audio
        └── Music Clip
```

---

# Installation

1. Import the scripts into your project.
2. Create an AudioMixer.
3. Expose parameters for:

* MasterVolume
* SfxVolume
* MusicVolume

4. Create SFX and Music ScriptableObject assets.
5. Add the assets to the AudioManager.
6. Place the AudioManager in your startup scene.

The manager will automatically create runtime AudioSources.

No manual AudioSource setup is required.

---

# Creating Audio Assets

Create a new SFX:

```text
Right Click
Create
→ Pharma
→ Audio
→ SFX Clip
```

Create Music:

```text
Right Click
Create
→ Pharma
→ Audio
→ Music Clip
```

Assign:

* Audio Clip
* Volume
* Loop
* Mixer Group

The asset name automatically becomes its playback ID.

---

# Playing Audio

## Play SFX

```csharp
AudioManager.Instance.PlaySfx("ButtonClick");
```

---

## Play SFX With Custom Pitch

```csharp
AudioManager.Instance.PlaySfx("Explosion", 0.9f);
```

---

## Play Randomized Pitch

```csharp
AudioManager.Instance.PlaySfxRandomPitch(
    "Footstep",
    0.9f,
    1.1f
);
```

Useful for reducing repetition.

---

## Play Music

```csharp
AudioManager.Instance.PlayMusic("MainTheme");
```

---

## Prevent Restarting Current Music

```csharp
AudioManager.Instance.PlayMusic(
    "MainTheme",
    restart: false
);
```

---

## Stop Music

```csharp
AudioManager.Instance.StopMusic("MainTheme");
```

---

## Stop All Music

```csharp
AudioManager.Instance.StopAllMusic();
```

---

## Stop All SFX

```csharp
AudioManager.Instance.StopAllSfx();
```

---

# Volume Controls

Master

```csharp
AudioManager.Instance.SetMasterVolume(1f);
```

SFX

```csharp
AudioManager.Instance.SetSfxVolume(0.75f);
```

Music

```csharp
AudioManager.Instance.SetMusicVolume(0.5f);
```

Values range from **0.0 → 1.0**.

Internally these values are converted to decibels before being applied to the AudioMixer.

---

# Mute Controls

Disable all audio

```csharp
AudioManager.Instance.SetMasterEnabled(false);
```

Disable only SFX

```csharp
AudioManager.Instance.SetSfxEnabled(false);
```

Disable only Music

```csharp
AudioManager.Instance.SetMusicEnabled(false);
```

These settings are automatically saved and restored between sessions.

---

# Runtime Audio Sources

The manager automatically creates:

* One AudioSource for each music asset
* One AudioSource for each SFX asset

If all SFX sources are busy, additional sources are created automatically to prevent sounds from being cut off.

No manual pooling setup is required.

---

# Persistence

The following settings are automatically saved using PlayerPrefs:

* Master Volume
* Music Volume
* SFX Volume
* Master Enabled
* Music Enabled
* SFX Enabled

Settings are restored automatically when the AudioManager starts.

---

# Design Goals

This system was designed around the following principles:

* Data-driven architecture
* ScriptableObject workflow
* Easy to extend
* Low setup cost
* Runtime efficiency
* AudioMixer integration
* Separation between audio data and playback logic
* Reusable across multiple Unity projects

---

# Possible Future Improvements

Potential extensions include:

* Audio fading
* Music crossfading
* Playlist support
* Audio categories
* Audio snapshots
* Addressables support
* 3D positional audio
* Spatial sound helpers
* Voice playback system
* Audio events
* Audio pooling optimization
* Editor validation tools
* Custom inspector
* Async audio loading

---

# License

This project is intended as a reusable Unity audio framework. Feel free to modify and extend it to suit your project's requirements.
