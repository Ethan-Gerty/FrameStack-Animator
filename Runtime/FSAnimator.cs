using UnityEngine;

public class FSAnimator : MonoBehaviour
{
    // Rendering, Animation and Frames
    [SerializeField] private SpriteRenderer sRenderer;
    [SerializeField] private int fps = 24;
    [SerializeField] private int frame = 0;
    [SerializeField] public FSAnimation currentAnimation = null;

    // Timing
    private float timer;

    private FSAnimation pendingAnim;
    private int pendingStartFrame;

    private float frameTime => (fps <= 0) ? 0.1f : 1f / fps;



    // Set Components
    private void Awake()
    {
        if (sRenderer == null)
            sRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Set Variables
    private void OnEnable()
    {
        timer = frameTime;
        frame = 0;

        if (currentAnimation != null && currentAnimation.sprites != null && currentAnimation.sprites.Count > 0)
            ApplyFrame();
    }


    // Update Loop
    private void Update()
    {
        if(currentAnimation == null || currentAnimation.sprites == null || currentAnimation.sprites.Count == 0)
    {
            if (pendingAnim != null) 
                AdvanceFrame();
            return;
        }

        timer -= Time.deltaTime;

        // Keeps Frames Moving In Case Of Lag Spikes
        while (timer <= 0f)
        {
            timer += frameTime;
            AdvanceFrame();
        }
    }



    // Change Animation Function - Changes Animation To Set Frame
    public void ChangeAnim(FSAnimation anim, int startFrame)
    {
        if (anim == null || anim.sprites == null || anim.sprites.Count == 0)
            return;

        pendingAnim = anim;
        pendingStartFrame = startFrame;
    }

    // Frame Advancement, Stops If Animation Isn't Set To Loop
    private void AdvanceFrame()
    {
        if (pendingAnim != null)
        {
            currentAnimation = pendingAnim;
            frame = Mathf.Clamp(pendingStartFrame, 0, currentAnimation.sprites.Count - 1);
            
            pendingAnim = null;
        }


        ApplyFrame();

        frame++;

        if (frame >= currentAnimation.sprites.Count) // Loop Or Stop Case
        {
            if (currentAnimation.loop)
            {
                frame = 0;
            } else if (currentAnimation.transitionInto != null)
            {
                ChangeAnim(currentAnimation.transitionInto, currentAnimation.transitionStartFrame);
                timer = frameTime;  // optional: avoids skipping into the next anim after lag
                return;
            } else
            {
                frame = currentAnimation.sprites.Count - 1;
            }
        }
    }

    private void ApplyFrame() // Applies New Frame
    {
        sRenderer.sprite = currentAnimation.sprites[frame];
    }
}