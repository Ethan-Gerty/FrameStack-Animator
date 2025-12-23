using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FSAnimator : MonoBehaviour
{
    // Rendering, Animation and Frames
    [SerializeField] private SpriteRenderer sRenderer;
    [SerializeField] private int defaultFps = 24;
    private int fps;
    private int frame = 0;
    public FSAnimation currentAnimation {  get; private set; }

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
        if (currentAnimation == null)
        {
            fps = defaultFps;
        } else if (currentAnimation != null)
        {
            fps = currentAnimation.overrideFps > 0 ? currentAnimation.overrideFps : defaultFps;
        }

        isPaused = false;
        isFinished = false;

        frameTime = 1f / fps;
        timer = frameTime;
        frame = 0;

        if (currentAnimation != null && currentAnimation.cels != null && currentAnimation.cels.Count > 0)
            ApplyFrame();
    }



    // Update Loop
    private void Update()
    {
        if(currentAnimation == null || currentAnimation.cels == null || currentAnimation.cels.Count == 0 || isPaused || isFinished)
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
        if (anim == null || anim.cels == null || anim.cels.Count == 0) return;
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
        if (anim == null || anim.cels == null || anim.cels.Count == 0) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;

        pendingAnim = anim;
        pendingStartFrame = Mathf.Clamp(startFrame, 0, anim.cels.Count - 1);
    }

    public void QueueAfterCurrent(FSAnimation anim, int startFrame, bool overrideTransition = false, bool restartIfSame = true)
    {
        if (anim == null || anim.cels == null || anim.cels.Count == 0) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (isPaused)
            isPaused = false;
        if (isFinished)
            isFinished = false;

        queuedAnim = anim;
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



    public void Pause()
    {
        if (!isPaused)
            isPaused = true;
    }
    public void UnPause()
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


        fps = currentAnimation.overrideFps > 0 ? currentAnimation.overrideFps : defaultFps;
        frameTime = 1f / fps;


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
                if (queuedAnim != null && overrideTransition)
                {
                    ApplyAnimation(queuedAnim, queuedStartFrame);
                } else
                {
                    ApplyAnimation(currentAnimation.transitionInto, currentAnimation.transitionStartFrame);
                }
            } else
            {
                if (queuedAnim != null)
                {
                    ApplyAnimation(queuedAnim, queuedStartFrame);
                } else
                {
                    frame = currentAnimation.cels.Count - 1;

                    if (!isFinished)
                    {
                        isFinished = true;
                        onAnimationFinished?.Invoke(currentAnimation);
                    }
                }
            }
        }
    }



    private void ApplyAnimation(FSAnimation newAnim, int newFrame)
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
}