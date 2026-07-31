# Simple 2D Platformer System

Components:
- Rigidbody2D
- PlayerInput
- PlayerMovement
- PlayerJump
- GroundChecker

Scene setup:
1. Add Rigidbody2D (freeze Z rotation).
2. Add an empty child called GroundCheck below the feet.
3. Assign GroundCheck and Ground layer.
4. Attach all scripts and wire references.

Included features:
- Horizontal movement
- Coyote time
- Jump buffering
- Variable jump height
- Faster falling

Recommended Rigidbody2D:
Gravity Scale: 3
Collision Detection: Continuous
Interpolate: Interpolate
