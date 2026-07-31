using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] PlayerInput input;
    [SerializeField] GroundChecker ground;
    [SerializeField] float jumpForce=12f;
    [SerializeField] float coyoteTime=.12f;
    [SerializeField] float jumpBuffer=.12f;
    [SerializeField] float fallGravityMultiplier=2.5f;
    [SerializeField] float lowJumpMultiplier=2f;

    Rigidbody2D rb;
    float coyoteCounter;
    float jumpBufferCounter;
    float gravity;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        gravity=rb.gravityScale;
    }

    void Update()
    {
        if(ground.IsGrounded) coyoteCounter=coyoteTime;
        else coyoteCounter-=Time.deltaTime;

        if(input.JumpPressed) jumpBufferCounter=jumpBuffer;
        else jumpBufferCounter-=Time.deltaTime;
    }

    void FixedUpdate()
    {
        if(jumpBufferCounter>0 && coyoteCounter>0)
        {
            rb.linearVelocity=new Vector2(rb.linearVelocity.x,jumpForce);
            coyoteCounter=0;
            jumpBufferCounter=0;
            input.ConsumeJump();
        }

        if(rb.linearVelocity.y<0)
            rb.gravityScale=gravity*fallGravityMultiplier;
        else if(rb.linearVelocity.y>0 && !input.JumpHeld)
            rb.gravityScale=gravity*lowJumpMultiplier;
        else
            rb.gravityScale=gravity;
    }
}
