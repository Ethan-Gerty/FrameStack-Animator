using System;
using UnityEngine;

public class FSAnimator : MonoBehaviour
{
    // Rendering, Animation and Frames
    [SerializeField] private SpriteRenderer sRenderer;
    [Min(1)] [SerializeField] public int defaultFps = 24;
    [Min(0.001f)] [SerializeField] public float playbackSpeed = 1;
    private int fps;
    private int frame = 0;
    public int currentFrame => frame;
    public FSAnimation currentAnimation { get; private set; }

    // Timing
    private float timer;
    private float frameTime;

    // Queued Animation
    public FSAnimation pendingAnim { get; private set; }
    private int pendingStartFrame;
    public FSAnimation queuedAnim { get; private set; }
    private int queuedStartFrame;
    private bool overrideTransition = false;

    // Events
    public event Action<int> onFrameChanged;
    public event Action<FSAnimation, FSAnimation> onAnimationChanged;
    public event Action<FSAnimation> onAnimationFinished;
    public event Action<string> animEvent;

    // Checks
    public bool isPaused { get; private set; }
    public bool isFinished { get; private set; }
    public bool isPlaying(FSAnimation anim) => currentAnimation == anim;
    public bool hasPendingAnimation() => pendingAnim != null;
    public bool hasQueuedAnimation() => queuedAnim != null;




    // Set Components
    private void Awake()
    {
        if (sRenderer == null)
            sRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Set Variables
    private void OnEnable()
    {
        if (sRenderer == null)
        {
            Debug.LogWarning($"FSAnimator: {gameObject.name} does not contain a SpriteRenderer.");
            return;
        }

        if (currentAnimation == null)
        {
            fps = defaultFps;
        } else if (currentAnimation != null)
        {
            fps = currentAnimation.overrideFps >= 0 ? currentAnimation.overrideFps : defaultFps;
        }

        isPaused = false;
        isFinished = false;

        if (fps == 0 || playbackSpeed < 0.001f) // If FPS = 0 Dont Run
            return;
        else if (fps < 0) // If FPS < 0 Fix Then Log Warning To User
        {
            fps = Mathf.Abs(fps);
        }

        frameTime = (1f / fps) / playbackSpeed;
        timer = frameTime;
        frame = 0;

        if (currentAnimation != null && currentAnimation.cels != null && currentAnimation.cels.Count > 0)
            ApplyFrame();
    }



    // Update Loop
    private void Update()
    {
        if (sRenderer == null)
        {
            Debug.LogWarning($"[FrameStack] FSAnimator: {gameObject.name} does not contain a SpriteRenderer.");
            return;
        }

        if (currentAnimation == null || currentAnimation.cels == null || currentAnimation.cels.Count == 0 || isPaused || isFinished || fps == 0 || playbackSpeed < 0.001f)
        {
            if (pendingAnim != null) 
                AdvanceFrame();
            return;
        }



        if (currentAnimation.ignoreTimeScale)
        {
            timer -= Time.unscaledDeltaTime;
        } else
        {
            timer -= Time.deltaTime;
        }

        // Keeps Frames Moving In Case Of Lag Spikes
        while (timer <= 0f)
        {
            timer += frameTime;
            AdvanceFrame();
        }
    }



    // Play Function, Plays animation from beginning. Restart default is false
    public void Play(FSAnimation anim, bool restartIfSame = false)
    {
        if (AnimatorChecks(anim)) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;

        pendingAnim = anim;
        pendingStartFrame = 0;
    }

    // Play From Frame, Plays Animation From Set startFrame. Restart default Is true
    public void PlayFromFrame(FSAnimation anim, int startFrame, bool restartIfSame = true)
    {
        if (AnimatorChecks(anim)) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;

        pendingAnim = anim;

        if (startFrame < 0 || startFrame >= anim.cels.Count)
            Debug.LogWarning($"[FrameStack] FSAnimator: '{gameObject.name}' PlayFromFrame requested frame " +
                $"{startFrame} for '{anim.name}', but the valid range is 0-{anim.cels.Count - 1}. The frame will be clamped.");
        pendingStartFrame = Mathf.Clamp(startFrame, 0, anim.cels.Count - 1);
    }

    // Queue After Current, Queues Animation To Play Next After Current Animation Finished
    public void QueueAfterCurrent(FSAnimation anim, int startFrame = 0, bool overrideTransition = false, bool restartIfSame = true)
    {
        if (AnimatorChecks(anim)) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;

        queuedAnim = anim;

        if (startFrame < 0 || startFrame >= anim.cels.Count)
            Debug.LogWarning($"[FrameStack] FSAnimator: '{gameObject.name}' QueueAfterCurrent requested frame " +
                $"{startFrame} for '{anim.name}', but the valid range is 0-{anim.cels.Count - 1}. The frame will be clamped.");
        queuedStartFrame = Mathf.Clamp(startFrame, 0, anim.cels.Count - 1);

        this.overrideTransition = overrideTransition;
    }

    // Restarts Current Animation By Just Setting 'frame' Back To 0
    public void Restart()
    {
        frame = 0;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;
    }



    public void Pause() // Pauses Animation
    {
        if (!isPaused)
            isPaused = true;
    }
    public void UnPause() // Unpauses Animation
    {
        if (isPaused)
            isPaused = false;
    }



    // Frame Advancement, Stops If Animation Isn't Set To Loop, Transitions If Has Transition Animation
    private void AdvanceFrame()
    {
        if (pendingAnim != null)
        {
            ApplyAnimation(pendingAnim, pendingStartFrame);
        }

        AnimationChecks();


        fps = currentAnimation.overrideFps >= 0 ? currentAnimation.overrideFps : defaultFps;

        if (fps == 0 || playbackSpeed < 0.001f) // If FPS = 0 Dont Run
            return;
        else if (fps < 0) // If FPS < 0 Fix Then Log Warning To User
        {
            fps = Mathf.Abs(fps);
        }

        frameTime = (1f / fps) / playbackSpeed;


        ApplyFrame();

        frame++;

        if (frame >= currentAnimation.cels.Count) // Loop Or Stop Case
        {
            if (currentAnimation.loop)
            {
                if (queuedAnim != null)
                {
                    ApplyAnimation(queuedAnim, queuedStartFrame);
                } else
                {
                    frame = 0;
                }
            } else if (currentAnimation.transitionInto != null)
            {
                bool canTransition = currentAnimation.transitionInto.cels.Count > 0;
                if (queuedAnim != null && overrideTransition)
                {
                    ApplyAnimation(queuedAnim, queuedStartFrame);
                }
                else if (canTransition)
                {
                    ApplyAnimation(currentAnimation.transitionInto, currentAnimation.transitionStartFrame);
                } else
                {
                    Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation}' transitions into " +
                        $"'{currentAnimation.transitionInto}', but this animation contains no cels and cannot be played.");

                    if (queuedAnim != null) ApplyAnimation(queuedAnim, queuedStartFrame);
                    else
                    {
                        frame = currentAnimation.cels.Count - 1;

                        if (isFinished) return;

                        isFinished = true;
                        onAnimationFinished?.Invoke(currentAnimation);
                    }
                }
            } else
            {
                if (queuedAnim != null)
                {
                    ApplyAnimation(queuedAnim, queuedStartFrame);
                } else
                {
                    frame = currentAnimation.cels.Count - 1;

                    if (isFinished) return;
                        
                    isFinished = true;
                    onAnimationFinished?.Invoke(currentAnimation);
                }
            }
        }
    }



    private void ApplyAnimation(FSAnimation newAnim, int newFrame) // Applies Animation
    {
        onAnimationChanged?.Invoke(currentAnimation, newAnim);

        currentAnimation = newAnim;
        frame = Mathf.Clamp(newFrame, 0, currentAnimation.cels.Count - 1);

        pendingAnim = null;
        queuedAnim = null;
    }

    private void ApplyFrame() // Applies New Frame
    {
        if (sRenderer == null || currentAnimation == null || currentAnimation.cels == null || currentAnimation.cels.Count == 0)
            return;

        frame = Mathf.Clamp(frame, 0, currentAnimation.cels.Count - 1);
        sRenderer.sprite = currentAnimation.cels[frame].sprite;

        onFrameChanged?.Invoke(frame);

        if (currentAnimation.cels[frame].events != null)
        {
            for (int i = 0; i < currentAnimation.cels[frame].events.Count; i++)
            {
                animEvent?.Invoke(currentAnimation.cels[frame].events[i]);
            }
        }
    }



    public void DebugStep()
    {
        if (!isPaused)
            return;

        AdvanceFrame();
    }





    private void AnimationChecks() // Warning Checks For Animations
    {
        if (currentAnimation == null) return;

        if (currentAnimation.cels[frame].sprite == null)
            Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation.name}' contains a null sprite at cel {frame}.");

        if (currentAnimation.transitionInto == currentAnimation)
            Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation.name}' transitions into itself.");

        if (currentAnimation.overrideFps == 0)
            Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation.name}' has an FPS override of 0.");

        if (defaultFps == 0)
            Debug.LogWarning($"[FrameStack] FSAnimator: '{gameObject.name}' default fps set to 0.");

        if (defaultFps < 0)
            Debug.LogWarning($"[FrameStack] FSAnimator: '{gameObject.name}' default fps is set to -{Mathf.Abs(defaultFps)}." +
                $"FrameStack will use {Mathf.Abs(defaultFps)} instead.");

        if (currentAnimation.cels[frame].events != null)
        {
            foreach (string evnt in currentAnimation.cels[frame].events)
            {
                if (string.IsNullOrWhiteSpace(evnt))
                    Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation.name}' contains an empty animation event at cel {frame}.");
            }
        }


        if (currentAnimation.transitionInto == null) return;

        if (currentAnimation.transitionStartFrame >= currentAnimation.transitionInto.cels.Count || currentAnimation.transitionStartFrame < 0)
            Debug.LogWarning($"[FrameStack] FSAnimation: '{currentAnimation.name}' transition start frame {currentAnimation.transitionStartFrame} " +
                $"is outside the target animation range (0-{currentAnimation.transitionInto.cels.Count - 1}).");
    }

    private bool AnimatorChecks(FSAnimation anim) // Animator Warning Entry Checks
    {
        if (anim == null)
        {
            Debug.LogWarning($"[FrameStack] FSAnimator '{gameObject.name}' cannot play null animation.");
            return true;
        }

        if (anim.cels == null)
        {
            Debug.LogWarning($"[FrameStack] FSAnimation: '{anim.name}' has a null Cel collection and cannot be played.");
            return true;
        }

        if (anim.cels.Count == 0)
        {
            Debug.LogWarning($"[FrameStack] FSAnimation: '{anim.name}' contains no Cels and cannot be played.");
            return true;
        }

        return false;
    }
}