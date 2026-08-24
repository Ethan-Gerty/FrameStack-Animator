# FrameStack Animator

> **Deterministic. Lightweight. Explicit.**

FrameStack Animator is a lightweight, frame-based sprite animation system for Unity built for **gameplay-driven animation control**.

It was created as an alternative to using Unity's Animator for simple 2D flipbook animation where animation needs to behave predictably, respond directly to gameplay code, and avoid large editor-driven state machines.

FrameStack focuses on **frame accuracy**, **predictable transitions**, and **explicit control from code**.

I also made a dev-log explaining why I built it and how the core system works:

https://youtu.be/PQa9nJF2XP8

---

## Why I Made FrameStack

Unity's Animator is powerful and works well for many types of animation.

For the kind of 2D gameplay animation I usually build, however, I often wanted something smaller and more direct. I wanted to be able to tell an animator exactly what should happen from code without building a large Animator Controller or relying on a collection of transition conditions.

FrameStack was built around a few simple ideas:

- animation changes should occur on **frame boundaries**
- the current cel should be allowed to finish before a requested animation change
- non-looping animations should be able to finish cleanly without extra exit states
- transitions should be easy to define directly on an animation
- gameplay code should be able to queue, interrupt, inspect, pause, and react to animation state
- animation timing should remain predictable even when the game's frame rate changes

FrameStack is **not intended to replace Unity's Animator**. It is a focused alternative for projects where direct, code-first control is a better fit.

---

# Installation

FrameStack Animator can be installed either through Unity's Package Manager using the Git URL or by downloading the `.unitypackage` from a GitHub release.

> **Use one installation method or the other.** Installing both versions into the same project may create duplicate scripts.

## Option 1 — Install through Git URL

In Unity:

1. Open **Window → Package Manager**
2. Click the **+** button
3. Select **Add package from git URL...**
4. Enter:

```text
https://github.com/Ethan-Gerty/FrameStack-Animator.git?path=/Packages/com.ethangerty.framestackanimator#v0.7.0
```

5. Click **Add**

Using the version tag keeps the package locked to the `v0.7.0` release.

If you intentionally want the current development version instead, remove the version tag:

```text
https://github.com/Ethan-Gerty/FrameStack-Animator.git?path=/Packages/com.ethangerty.framestackanimator
```

---

## Option 2 — Install the `.unitypackage`

1. Open the **v0.7.0 release** on GitHub
2. Download:

```text
FrameStack-Animator-v0.7.0.unitypackage
```

3. Open your Unity project
4. Go to **Assets → Import Package → Custom Package...**
5. Select the downloaded `.unitypackage`
6. Import the included files

---

# What FrameStack Includes

## Core Runtime Features

- **Frame-accurate sprite animation**
- **Deterministic playback with lag catch-up**
- **Frame-boundary animation switching**
- **Looping and non-looping animations**
- **Automatic transitions between animations**
- **Queued animations**
- **Starting animations from arbitrary frames**
- **Per-animation FPS overrides**
- **Runtime playback speed**
- **Scaled or unscaled time**
- **Per-cel string events**
- **Animation lifecycle events**
- **Pause and resume controls**
- **Runtime animation state inspection**

## Editor & Authoring Features

- **Bulk sprite importing**
- **Create an FSAnimation directly from selected sprites**
- **Sprite previews for individual cels**
- **Animated FSAnimation Inspector preview**
- **Frame-by-frame preview controls**
- **Runtime frame debug display**
- **Pause and Step Animation debug controls**
- **Error checks and warnings for invalid animation setups**

---

# Quick Start

## 1. Create an FSAnimation

In the Project window:

**Right-click → Create → FrameStackAnimator → Animation**

This creates an `FSAnimation` asset.

An animation contains a list of **Cels**. Each cel contains:

- a `Sprite`
- a list of optional string `Events`

You can add sprites manually or use the faster authoring tools described later in this README.

---

## 2. Add an FSAnimator

Add `FSAnimator` to the GameObject that will control the animation.

Assign:

- **S Renderer** — the `SpriteRenderer` that FrameStack should animate
- **Default Fps** — the default playback FPS
- **Playback Speed** — a runtime multiplier applied to animation speed

The `SpriteRenderer` can be on the same GameObject or assigned from another GameObject.

---

## 3. Play an animation

Get a reference to the animator:

```csharp
[SerializeField] private FSAnimator animator;
[SerializeField] private FSAnimation idle;
```

Then play an animation:

```csharp
animator.Play(idle);
```

Calling `Play()` does **not** immediately snap to the new animation. The request becomes the pending animation and is applied on the next frame boundary.

---

# FSAnimation Settings

Every `FSAnimation` asset contains the following settings.

## Loop

```text
Loop
```

When enabled, the animation returns to its first cel after reaching the end.

When disabled, the animation either:

- transitions into its configured transition animation,
- plays a queued animation,
- or stops on its final cel.

---

## Override FPS

```text
Override Fps
```

Controls the playback rate for this specific animation.

- `-1` — use the `FSAnimator` Default FPS
- positive value — use this animation's own FPS
- `0` — automatic frame advancement is disabled and FrameStack will produce a warning

Example:

```text
FSAnimator Default FPS: 12
Run Override FPS:       18
Idle Override FPS:      -1
```

`Run` plays at 18 FPS while `Idle` uses the animator's 12 FPS default.

---

## Transition Into

```text
Transition Into
```

A non-looping animation can automatically continue into another `FSAnimation`.

For example:

```text
JumpStart → Jump
Attack    → Idle
Land      → Idle
```

Once the first animation reaches its end, FrameStack switches into the configured transition animation.

---

## Transition Start Frame

```text
Transition Start Frame
```

Controls which cel the target transition animation begins from.

For example, an animation could transition into `Run` at cel `2` instead of starting from cel `0`.

Invalid values are detected and warned about by FrameStack.

---

## Ignore Time Scale

```text
Ignore Time Scale
```

When enabled, the animation uses `Time.unscaledDeltaTime`.

This is useful for animation that should continue while gameplay is paused or slowed, such as:

- UI
- menus
- certain VFX
- pause-screen animation

---

## Cels

Each animation is made from a list of `AnimCel` entries.

Each cel contains:

```text
Sprite
Events
```

The sprite is displayed when that cel is reached.

The Events list can contain any number of string events that are emitted when the cel is applied.

---

# Playing Animations From Code

## Play

```csharp
animator.Play(animation);
```

Requests an animation from its first cel.

By default, requesting the animation that is already playing will not restart it.

To explicitly restart the same animation:

```csharp
animator.Play(animation, true);
```

The animation request is applied at the next frame boundary.

---

## Play From Frame

```csharp
animator.PlayFromFrame(animation, 3);
```

Starts the requested animation from a specific cel.

This can be useful for:

- shared animations
- anticipation frames
- skipping intros
- starting part-way through a sequence

If the requested frame is outside the valid range, FrameStack warns in the Console and clamps it to a valid cel.

You can also control whether the same animation may restart:

```csharp
animator.PlayFromFrame(animation, 3, false);
```

---

## Queue After Current

```csharp
animator.QueueAfterCurrent(animation);
```

Queues an animation to begin when the current animation reaches its end.

You can choose a start frame:

```csharp
animator.QueueAfterCurrent(animation, 2);
```

By default, an `FSAnimation`'s configured `Transition Into` takes priority.

To make the queued animation override that transition:

```csharp
animator.QueueAfterCurrent(animation, 0, true);
```

Full method:

```csharp
animator.QueueAfterCurrent(
    animation,
    startFrame: 0,
    overrideTransition: true,
    restartIfSame: true
);
```

---

## Restart

```csharp
animator.Restart();
```

Resets the current animation back to cel `0` and clears its paused/finished state.

---

## Pause

```csharp
animator.Pause();
```

Stops automatic frame advancement.

---

## UnPause

```csharp
animator.UnPause();
```

Resumes playback.

---

# Pending vs Queued Animations

FrameStack uses two different animation requests.

## Pending Animation

A **pending animation** is an animation that has been requested with:

```csharp
Play()
```

or:

```csharp
PlayFromFrame()
```

It waits until the next frame boundary before becoming the current animation.

This is what prevents mid-frame animation snapping.

## Queued Animation

A **queued animation** is created with:

```csharp
QueueAfterCurrent()
```

It waits for the current animation to reach its end before playing.

In short:

```text
Play()              → switch on the next frame boundary
QueueAfterCurrent() → switch when the current animation finishes
```

---

# Runtime Playback Speed

`FSAnimator` includes a `Playback Speed` value.

```text
1.0 = normal speed
0.5 = half speed
2.0 = double speed
```

For example:

```csharp
animator.playbackSpeed = 0.5f;
```

Playback Speed is a multiplier applied on top of the active animation's FPS.

This makes it useful for:

- slow-motion effects
- speeding up a character temporarily
- testing animation timing
- adjusting playback without changing the source animation asset

---

# Animation Events

Every cel can contain string-based animation events.

For example, a cel could contain:

```text
Footstep
AttackHit
SpawnEffect
```

Subscribe to `animEvent`:

```csharp
private void OnEnable()
{
    animator.animEvent += OnAnimationEvent;
}

private void OnDisable()
{
    animator.animEvent -= OnAnimationEvent;
}

private void OnAnimationEvent(string animationEvent)
{
    if (animationEvent == "Footstep")
    {
        // Play footstep sound
    }

    if (animationEvent == "AttackHit")
    {
        // Enable attack hitbox
    }
}
```

This keeps frame-specific gameplay events attached directly to the cel that should trigger them.

---

# Runtime Events

FrameStack exposes several runtime callbacks.

## Frame Changed

```csharp
animator.onFrameChanged += OnFrameChanged;

private void OnFrameChanged(int frame)
{
    Debug.Log($"Now on cel {frame}");
}
```

Called when a cel is applied.

---

## Animation Changed

```csharp
animator.onAnimationChanged += OnAnimationChanged;

private void OnAnimationChanged(FSAnimation previous, FSAnimation next)
{
    Debug.Log($"{previous} → {next}");
}
```

Called when FrameStack commits a new animation.

---

## Animation Finished

```csharp
animator.onAnimationFinished += OnAnimationFinished;

private void OnAnimationFinished(FSAnimation animation)
{
    Debug.Log($"{animation.name} stopped");
}
```

`onAnimationFinished` is fired when a non-looping animation reaches its end **and the animator stops**.

If playback continues into a transition or queued animation, the animator has not stopped, so `onAnimationFinished` is not fired. That change is represented by `onAnimationChanged` instead.

---

# Runtime State & Introspection

FrameStack exposes its current state so gameplay code can inspect the animator without having to maintain a separate copy of that state.

## Current Animation

```csharp
animator.currentAnimation
```

Returns the active `FSAnimation`.

---

## Current Frame

```csharp
animator.currentFrame
```

Returns the current cel index.

---

## Pending Animation

```csharp
animator.pendingAnim
```

Returns the animation waiting to be applied at the next frame boundary.

---

## Queued Animation

```csharp
animator.queuedAnim
```

Returns the animation waiting for the current animation to finish.

---

## Paused / Finished State

```csharp
animator.isPaused
animator.isFinished
```

Useful when gameplay needs to react to the animator's current playback state.

---

## Check Current Animation

```csharp
if (animator.isPlaying(runAnimation))
{
    // Run is currently active
}
```

---

## Check Pending / Queued State

```csharp
if (animator.hasPendingAnimation())
{
    // An animation change is waiting
}

if (animator.hasQueuedAnimation())
{
    // An animation is queued after the current one
}
```

---

# Editor Authoring Tools

FrameStack includes several tools intended to make creating sprite animations faster.

## Bulk Sprite Import

Open an `FSAnimation` asset and drag multiple sprites into:

```text
Drag Sprites Here To Add Cels
```

FrameStack creates a cel for every sprite and assigns it automatically.

This avoids manually adding and assigning every cel one at a time.

---

## Create Animation From Selected Sprites

Select the sprites you want to use in the Project window.

Then use:

**Assets → Create → FrameStackAnimator → Animation From Selected Sprites**

FrameStack creates a new `FSAnimation` and automatically creates a cel for each selected sprite.

---

## Cel Sprite Previews

Each cel displays a preview of its assigned sprite directly in the Inspector.

This makes it easier to identify frames without repeatedly opening each sprite asset.

---

# FSAnimation Inspector Preview

`FSAnimation` assets can be previewed directly from the Inspector without entering Play Mode.

At the bottom of the Inspector, expand the **FS Animation Preview**.

The preview includes:

```text
<     Previous Cel
▶/⏸   Play / Pause
>     Next Cel
```

The current preview cel is also displayed.

The preview follows the animation's loop setting.

### Preview FPS

If the animation has a positive `Override Fps`, the preview uses it.

If the animation uses the animator default (`Override Fps = -1`), the Inspector preview uses **24 FPS** because an `FSAnimation` asset does not have access to a particular `FSAnimator`'s Default FPS while being previewed on its own.

An Override FPS of `0` prevents the preview from automatically advancing.

---

# Runtime Debug Display

In Play Mode, select a GameObject containing an `FSAnimator`.

The Inspector displays:

```text
Runtime Info / Debug Controls
```

This section shows:

- whether the animator is finished
- the current animation
- the current cel
- the pending animation
- the queued animation

This makes it possible to inspect FrameStack's internal playback state without adding temporary debug code.

---

# Debug Controls

The runtime Inspector intentionally keeps its debug controls small.

## Pause Toggle

Toggle:

```text
Paused
```

to pause or resume the animator while the game is running.

## Step Animation

While the animator is paused, press:

```text
Step Animation
```

to advance the animation by one cel.

The button is disabled while the animator is not paused.

This is useful for inspecting:

- exact frame timing
- transitions
- cel events
- queued animations
- animation state changes

one frame at a time.

---

# Error Checks & Warnings

FrameStack performs runtime checks for common invalid configurations and requests.

Warnings include cases such as:

- an `FSAnimator` having no assigned `SpriteRenderer`
- trying to play a null animation
- an animation containing no cels
- a cel containing a null sprite
- an animation transitioning into itself
- an animation transitioning into an animation with no cels
- invalid transition start frames
- invalid `PlayFromFrame()` start frames
- invalid `QueueAfterCurrent()` start frames
- FPS values that prevent playback
- negative default FPS values
- empty animation event strings

Where possible, invalid frame requests are clamped to a valid range rather than causing playback to fail.

The warnings are intended to make incorrect animation setups easier to identify while keeping the runtime system simple.

---

# Example Animation Flow

A character controller can combine looping animations with short transition animations:

```text
Idle ↔ Run

JumpStart
    ↓
   Jump
    ↓
JumpToFall
    ↓
   Fall
    ↓
FallToRun
    ↓
   Run

Attack
    ↓
   Idle

Hurt
    ↓
   Idle
```

The short animations can define their destination using `Transition Into`, while gameplay code only needs to request the animation that begins the sequence.

---

# Intended Use Cases

FrameStack Animator is primarily designed for:

- 2D platformers
- pixel-art games
- gameplay-driven character animation
- UI flipbook animation
- VFX flipbook animation
- projects that prefer code-first animation control
- developers who want animation state to remain explicit and inspectable

---

# Project Status

**Current release: v0.7.0**

FrameStack Animator is actively developed.

The current core runtime is intentionally small, while recent releases have focused increasingly on authoring, debugging, validation, and general workflow improvements.

A feature-focused sample scene demonstrating the system working together is planned as FrameStack moves toward `v1.0.0`.

The API may still change before the `1.0.0` release.

---

# Author

Created by **Ethan Gerty** as a gameplay-focused animation system designed around clarity, control, and predictable frame-based behaviour.

GitHub: https://github.com/Ethan-Gerty