# FrameStack Animator

FrameStack Animator is a lightweight, frame-based sprite animation system for Unity, built for **gameplay-driven animation control**.

It focuses on predictable timing, clean transitions, and direct control from code, without relying on Animator Controllers or timeline-based clips.

---

## What problem does it solve?

Unity’s Animator is well-suited to authored, timeline-based animation.  
FrameStack Animator is designed for cases where animation needs to behave like **game logic**:

- start animations from any frame
- guarantee frame-boundary switching (no mid-frame snapping)
- freeze cleanly on the final frame
- advance correctly during lag spikes
- transition between animations without state machines

It is not a replacement for Animator, but an alternative approach for simple 2D, flipbook-style animation where deterministic control matters.

---

## Key features

- **Frame-accurate playback**  
  Frames advance on fixed timing, independent of frame rate.

- **Queued animation switching**  
  Animation changes are applied only on frame boundaries.

- **Start-frame control**  
  Animations can begin from any frame index.

- **Looping and non-looping support**  
  Non-looping animations finish once and hold their final frame.

- **Transition animations**  
  Animations can optionally transition into another when they finish.

- **Minimal runtime overhead**  
  No Animator Controller, Animation Clips, or editor-driven state logic.

---

## Best suited for

- 2D platformers and pixel-art games  
- Gameplay-driven character animation  
- UI and VFX flipbook animations  
- Projects that prefer code-first animation control

---

## Installation (Unity Package Manager)

Window → Package Manager → + → Add package from git URL…