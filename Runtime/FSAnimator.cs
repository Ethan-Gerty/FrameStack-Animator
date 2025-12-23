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
    public FSAnimation currentAnimation = null;
    public bool paused = false;
    public bool restartIfSame = false;

    // Timing
    private float timer;
    private float frameTime;

    // Queued Animation
    private FSAnimation pendingAnim;
    private int pendingStartFrame;

    // Events
    public event Action<int> onFrameChanged;
    public event Action<FSAnimation, FSAnimation> onAnimationChanged;
    public event Action<FSAnimation> onAnimationFinished;
    public event Action<string> animEvent;



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

        frameTime = 1f / fps;
        timer = frameTime;
        frame = 0;

        if (currentAnimation != null && currentAnimation.cels != null && currentAnimation.cels.Count > 0)
            ApplyFrame();
    }



    // Update Loop
    private void Update()
    {
        if(currentAnimation == null || currentAnimation.cels == null || currentAnimation.cels.Count == 0 || paused)
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



    // Change Animation Function - Changes Animation To Set Frame
    public void Play(FSAnimation anim)
    {
        if (anim == null || anim.cels == null || anim.cels.Count == 0) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (paused)
            paused = false;

        pendingAnim = anim;
        pendingStartFrame = 0;
    }

    public void PlayFromFrame(FSAnimation anim, int startFrame)
    {
        if (anim == null || anim.cels == null || anim.cels.Count == 0) return;
        if (!restartIfSame && anim == currentAnimation) return;

        if (paused)
            paused = false;

        pendingAnim = anim;
        pendingStartFrame = Mathf.Clamp(startFrame, 0, anim.cels.Count - 1);
    }

    public void Restart()
    {
        frame = 0;

        if (paused)
            paused = false;
    }



    // Frame Advancement, Stops If Animation Isn't Set To Loop
    private void AdvanceFrame()
    {
        if (pendingAnim != null)
        {
            onAnimationChanged?.Invoke(currentAnimation, pendingAnim);

            currentAnimation = pendingAnim;

            frame = Mathf.Clamp(pendingStartFrame, 0, currentAnimation.cels.Count - 1);

            pendingAnim = null;
        }


        fps = currentAnimation.overrideFps > 0 ? currentAnimation.overrideFps : defaultFps;
        frameTime = 1f / fps;


        ApplyFrame();

        frame++;

        if (frame >= currentAnimation.cels.Count) // Loop Or Stop Case
        {
            if (currentAnimation.loop)
            {
                frame = 0;
            } else if (currentAnimation.transitionInto != null)
            {
                PlayFromFrame(currentAnimation.transitionInto, currentAnimation.transitionStartFrame);
                timer = frameTime;  // optional: avoids skipping into the next anim after lag

                onAnimationFinished?.Invoke(currentAnimation);
                return;
            } else
            {
                frame = currentAnimation.cels.Count - 1;

                if (!paused)
                {
                    paused = true;
                    onAnimationFinished?.Invoke(currentAnimation);
                }
            }
        }
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