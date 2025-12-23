using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    public enum States
    {
        idle,
        moving,
        jumping,
        falling
    }



    [SerializeField] private SpriteRenderer sprRenderer;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] public States state;


    [SerializeField] private FSAnimator animator;
    [SerializeField] private Animations anims;


    [SerializeField] private float speed = 1;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private float xInput;



    private void Awake()
    {
       rb = gameObject.GetComponent<Rigidbody2D>();
       sprRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        state = States.idle;

        if (animator != null && anims != null && anims.idleAnim != null)
            animator.Play(anims.idleAnim);


        animator.onFrameChanged += OnFrameChanged;
        animator.onAnimationChanged += OnAnimationChanged;
        animator.onAnimationFinished += OnAnimationFinished;
        animator.animEvent += AnimationEvents;
    }



    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (xInput == 0 && isGrounded)
        {
            state = States.idle;
        } else if (isGrounded)
        {
            state = States.moving;
        } else if (!isGrounded && rb.linearVelocityY >= 1.5f)
        {
            state = States.jumping;
            rb.gravityScale = 1f;
        } else if (!isGrounded && rb.linearVelocityY < 1.5f)
        {
            state = States.falling;
            rb.gravityScale = 2.25f;
        }


        if (xInput > 0)
        {
            sprRenderer.flipX = false;
        } else if (xInput < 0)
        {
            sprRenderer.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(xInput * speed, rb.linearVelocity.y);

        if (Input.GetKey(KeyCode.Space) && isGrounded) 
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void LateUpdate()
    {
        switch (state)
        {
            case States.idle:

                if (animator.currentAnimation != anims.idleAnim)
                {
                    animator.Play(anims.idleAnim);
                }

                break;
            case States.moving:

                if (animator.currentAnimation != anims.movingAnim)
                {
                    animator.Play(anims.movingAnim);
                }

                break;
            case States.falling:

                if (animator.currentAnimation != anims.fallingAnim && animator.currentAnimation != anims.fallingTransition)
                {
                    animator.Play(anims.fallingTransition);
                }

                break;
            case States.jumping:

                if (animator.currentAnimation != anims.jumpAnim)
                {
                    animator.Play(anims.jumpAnim);
                }

                break;
        }
    }



    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D col)
    {

    }





    public void OnFrameChanged(int frame)
    {

    }
    public void OnAnimationChanged(FSAnimation prevAnim, FSAnimation newAnim)
    {

    }
    public void OnAnimationFinished(FSAnimation finishedAnim)
    {

    }
    public void AnimationEvents(string eventName)
    {
        Debug.Log("Animation Event: " + eventName);
    }
}


[System.Serializable]
public class Animations
{
    [SerializeField] public FSAnimation idleAnim;
    [SerializeField] public FSAnimation movingAnim;
    [SerializeField] public FSAnimation jumpAnim;
    [SerializeField] public FSAnimation fallingAnim;

    [SerializeField] public FSAnimation fallingTransition;
}