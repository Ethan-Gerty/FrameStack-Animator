# FrameStack Animator

FrameStack Animator is a lightweight, frame-based sprite animation system for Unity, built for **gameplay-driven animation control**.

It was created to solve common problems encountered when using Unity’s Animator for simple 2D flipbook animation—particularly when animation needs to behave deterministically, respond directly to gameplay logic, and avoid editor-heavy state machines.

This system prioritises **frame accuracy**, **predictable transitions**, and **explicit control from code**.

I made a Dev-Log for this as an insight to how it works if you want to go check it out:
https://youtu.be/PQa9nJF2XP8

---

## Why this exists

Unity’s Animator is well suited to authored, timeline-based animation.  
However, for simple sprite animations driven by gameplay state, it can become overly complex or restrictive.

FrameStack Animator was built to:
- guarantee animation changes only occur on frame boundaries
- allow animations to finish naturally before transitioning
- support clean non-looping behaviour without exit states
- provide deterministic timing under variable frame rates
- expose lightweight animation events without Animation Clips

It is **not intended to replace Unity’s Animator**, but to offer an alternative approach for projects where direct, code-first control is preferred.

---

## Core Features

These define the fundamental behaviour of the system.

- **Frame-accurate sprite animation**  
  Frames advance at a fixed rate independent of frame rate.

- **Deterministic playback with lag catch-up**  
  Animations remain in sync during frame drops using a catch-up stepping loop.

- **Queued animation switching**  
  Animation changes are deferred and applied only at frame boundaries, preventing mid-frame snapping.

- **Looping and non-looping animations**  
  Non-looping animations play once and hold their final frame.

- **Pause-on-completion behaviour**  
  Finished animations automatically pause on the last frame, preventing repeated events or unintended replays.

- **Transition animations**  
  Animations can optionally transition into another animation when they complete.

- **Data-driven animation assets**  
  Animations are defined using ScriptableObjects, keeping data separate from logic.

---

## Advanced Features

These extend the core system but are not required for basic use.

- **Start animations from arbitrary frames**  
  Useful for anticipation frames, partial loops, or shared animations.

- **Per-animation FPS overrides**  
  Individual animations can control their playback speed.

- **Unscaled time playback option**  
  Animations can ignore `Time.timeScale`, suitable for UI or paused gameplay.

- **Per-frame animation events**  
  Frames can emit string-based events without using Animation Events.

- **Runtime animation lifecycle callbacks**  
  Includes events for:
  - frame changes
  - animation changes
  - animation completion

---

## Basic Usage

### 1. Create an animation asset
Right-click in the Project window:

Create → FrameStackAnimator → Animation

Add sprites to the animation’s frame list (`cels`) and configure looping, FPS overrides, or transitions as needed.


### 2. Add FSAnimator to a GameObject

Assign a SpriteRenderer and optionally set a default FPS.

```
FSAnimator animator = GetComponent<FSAnimator>();
```

### 3. Play animations from code

```
animator.Play(idleAnimation);
animator.Play(runTransition);
animator.PlayFromFrame(attackAnimation, 2);
```

Animation changes are queued and applied at the next frame boundary.


### 4. React to animation events

```
animator.animEvent += (string evt) =>
{
    if (evt == "Footstep")
        PlayFootstepSound();
};

animator.onAnimationFinished += (anim) =>
{
    Debug.Log($"{anim.name} finished");
};
```

---

## Intended Use Cases

- 2D platformers
- Pixel-art games
- Gameplay-driven character animation
- UI or VFX flipbook animations
- Projects that prefer code-first animation control

---

## Project Status

FrameStack Animator is actively developed and used in a work-in-progress Unity game.

The core architecture is stable, but the API may evolve as new features and use cases are explored.

---

## Author

Created by Ethan Gerty as a gameplay-focused animation system designed for clarity, control, and extensibility.
