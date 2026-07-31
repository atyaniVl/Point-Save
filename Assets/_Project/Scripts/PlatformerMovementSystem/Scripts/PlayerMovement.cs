using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] PlayerInput input;
    [SerializeField] float moveSpeed=8f;

    Rigidbody2D rb;
    void Awake()=>rb=GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        rb.linearVelocity=new Vector2(input.Horizontal*moveSpeed, rb.linearVelocity.y);
    }
}
