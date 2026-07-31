using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Horizontal { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld => Input.GetButton("Jump");

    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        JumpPressed = Input.GetButtonDown("Jump");
    }

    public void ConsumeJump() => JumpPressed = false;
}
