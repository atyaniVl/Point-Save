# UI Button Animation System

This starter package provides:
- DOTween-driven button animations
- State-based profiles (Normal, Hover, Pressed, Selected, Disabled)
- Scale, Color and Sprite transitions
- ScriptableObject animation profiles

## Requirements
- Unity 6+
- DOTween

## Included
- UIButtonAnimator
- ButtonAnimationProfile
- ButtonVisualState
- UIButtonState

This is a starter implementation intended to be expanded with rotation, alpha, position, custom inspector, animation sequencing, and demo assets.

## Improvement Plan
Phase 1 — Core System (MVP)

This is the version I'd actually integrate into the game first.

Features
✅ Normal
✅ Hover
✅ Pressed
✅ Selected
✅ Disabled

Supported animations

Scale
Color
Sprite

Architecture

Runtime
    UIButtonAnimator
    ButtonVisualState
    ButtonAnimationProfile
    UIButtonState

Inspector

Animation Profile

Target Transform

Target Image

Usage

Create
Create
    UI
        Button Animation Profile
Configure every state
Normal

Hover

Pressed

Selected

Disabled
Add
UIButtonAnimator

to every button.

Assign the profile.

Done.

Phase 2 — Better Animations

Instead of

Scale

Color

Sprite

support

Scale
Rotation
Position
Color
Alpha
Sprite

Every property should be optional.

Example

Hover

Scale ✔
Rotation ✔
Color ✘
Sprite ✘
Position ✔

instead of forcing everything.

Phase 3 — Animation Layers

Instead of one animation per state

Hover

allow

Hover

Scale
Glow
Shadow
Sound
Particles

Each layer plays independently.

Phase 4 — Animation Sequence

Instead of

Scale

allow multiple tweens.

Example

Hover

Scale 1.05

↓

Rotate 2°

↓

Scale 1.03

using

DOTween.Sequence()
Phase 5 — Custom Inspector

This is probably the biggest quality-of-life improvement.

Instead of

Normal

Scale

Color

Duration

Ease

for five states...

Display

▼ Normal

▼ Hover

▼ Pressed

▼ Selected

▼ Disabled

Each foldout shows only enabled properties.

Like Unity's built-in Button inspector.

Phase 6 — Preview Button

This is my favorite feature.

A button in the inspector

Preview Hover

Preview Pressed

Preview Disabled

No entering Play Mode.

Phase 7 — Animation Profiles

Instead of editing every button

Play

Quit

Credits

Continue

Inventory

they all reference

Pixel UI Profile

Changing one profile updates the whole game.

Phase 8 — Theme System

Support multiple themes.

Classic

Modern

CRT

GameBoy

Sci-Fi

Runtime

ThemeManager.SetTheme(pixelTheme);

Every button updates automatically.

Phase 9 — Events

Useful for SFX.

Hover

↓

Play Hover Sound

Pressed

↓

Play Click Sound

No code.

Just UnityEvents.

Phase 10 — Advanced Animations

Support

Shake
Punch Scale
Punch Rotation
Flash
Ripple
Bounce
Elastic
Fade

using DOTween shortcuts.

Example

Pressed

Animation Type

○ Scale
○ Punch
○ Shake
○ Bounce
Phase 11 — Navigation Support

Unity keyboard/controller navigation

Tab

Arrow Keys

Gamepad

Steam Deck

should automatically use

Selected

instead of Hover.

Phase 12 — State Machine

Instead of

if(...)

everywhere

create

Normal

↓

Hover

↓

Pressed

↓

Selected

↓

Disabled

Everything passes through

SetState(UIButtonState state);

No duplicated logic.

Phase 13 — Performance

Avoid allocations.

Cache

Image

RectTransform

CanvasGroup

Kill tweens correctly

transform.DOKill();
image.DOKill();

instead of

DOTween.KillAll();
Phase 14 — Base Animator

Instead of

UIButtonAnimator

build

UIStateAnimator

Then derive

UIButtonAnimator

UIToggleAnimator

UITabAnimator

UICheckboxAnimator

UISliderAnimator

All reuse the same animation engine.

Phase 15 — Polish
XML documentation
Tooltips
Validation
Warnings
Samples
Demo scene
Assembly Definitions
Namespace organization
Unit tests (for state transitions)
Runtime examples
Recommended folder structure
UIAnimationSystem
│
├── Runtime
│   ├── Core
│   │   ├── UIStateAnimator.cs
│   │   ├── UIButtonAnimator.cs
│   │   ├── UIToggleAnimator.cs
│   │   ├── UISliderAnimator.cs
│   │   └── UIStateMachine.cs
│   │
│   ├── Data
│   │   ├── ButtonVisualState.cs
│   │   ├── AnimationLayer.cs
│   │   ├── AnimationSequence.cs
│   │   └── ButtonAnimationProfile.cs
│   │
│   ├── Themes
│   │   ├── UITheme.cs
│   │   └── UIThemeManager.cs
│   │
│   └── Utilities
│
├── Editor
│   ├── Inspectors
│   ├── Drawers
│   └── Preview
│
├── Samples
│
├── Demo
│
└── Documentation