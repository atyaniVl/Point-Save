using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] Transform groundCheck;
    [SerializeField] float radius = .15f;
    [SerializeField] LayerMask groundMask;

    public bool IsGrounded =>
        Physics2D.OverlapCircle(groundCheck.position, radius, groundMask);

    private void OnDrawGizmos()
    {
        if (groundCheck == null)
            return;

        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            radius,
            groundMask);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, radius);
    }
}
