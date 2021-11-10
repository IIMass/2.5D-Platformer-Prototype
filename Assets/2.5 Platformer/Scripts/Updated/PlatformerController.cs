using UnityEngine;

public class PlatformerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [Header("Inputs")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool jumpPressed;

    [Header("States")]
    [SerializeField] private bool grounded;
    public bool Grounded
    {
        get { return grounded; }
        set
        {
            if (grounded != value)
            {
                grounded = value;
                OnGroundStateChange(value);
            }
        }
    }

    [SerializeField] private bool jumped;
    [SerializeField] private bool onLedge;
    [SerializeField] private bool isClimbing;

    [SerializeField] private bool isFacingRight = true;

    [Header("Horizontal Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float walkDecceleration;

    [SerializeField] [Range(0f, 1f)] private float airControl;

    [Header("Vertical Movement")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityForce;
    [SerializeField] private float gravityFallMultiplier;
    [SerializeField] private float maxFallSpeed;

    [Header("Movement Values")]
    [SerializeField] private Vector3 movement = Vector3.zero;

    [Header("Character Flip")]
    [SerializeField] private float rotateSpeed;

    [Header("Ledge Grab & Climb")]
    [SerializeField] private Vector2 ledgeRaysLength;
    [SerializeField] private Vector2 ledgeVerticalOffsets;

    [Space(5)]

    [SerializeField] private Vector2 ledgeGrabOffsets;

    [Space(5)]

    [SerializeField] private Vector3 ledgeClimbOffset;

    // [Header("Vault")]
    // [Header("Ladder")]
    // [Header("Roll")]


    private void Update()
    {
        Grounded = controller.isGrounded;

        UpdateInputs();
        Movement();
        RotateCharacter();
        LedgeCheck();
        LedgeStartClimb();

        UpdateAnimatorValues();
    }

    private void UpdateInputs()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
    }

    private void Movement()
    {
        if (onLedge) return;

        if (grounded)
        {
            // Horizontal Movement. Accelerates towards walkSpeed when horInput is not 0, deccelerates otherwise
            movement.z = (moveInput.x != 0)
                ? Mathf.MoveTowards(movement.z, walkSpeed * moveInput.x, walkAcceleration * Time.deltaTime)
                : Mathf.MoveTowards(movement.z, 0f, walkDecceleration * Time.deltaTime);

            // Apply constant downwards force
            movement.y = -gravityForce;
        }
        else
        {
            movement.z = (moveInput.x != 0)
                ? Mathf.MoveTowards(movement.z, walkSpeed * moveInput.x, walkAcceleration * airControl * Time.deltaTime)
                : movement.z;

            movement.y = (movement.y >= 0)
                ? movement.y - gravityForce * Time.deltaTime
                : movement.y - gravityForce * gravityFallMultiplier * Time.deltaTime;
        }

        // Jump
        if (jumpPressed && !jumped && Grounded)
        {
            jumped = true;
            movement.y = jumpForce;

            animator.SetTrigger("Jump");
        }

        movement.y = Mathf.Clamp(movement.y, -maxFallSpeed, jumpForce);

        // Moves controller
        controller.Move(movement * Time.deltaTime);
    }
    private void RotateCharacter()
    {
        if (Grounded)
        {
            // Flip character only when in ground
            if (movement.z != 0) isFacingRight = movement.z > 0;
        }

        if (isFacingRight) animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, Quaternion.Euler(0f, 0f, 0f), rotateSpeed * Time.deltaTime);
        else animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, Quaternion.Euler(0f, -180, 0f), rotateSpeed * Time.deltaTime);
    }

    private void LedgeCheck()
    {
        if (onLedge) return;

        if (!Grounded)
        {
            Vector3 verticalRayOrigin = new Vector3
                (transform.position.x,
                controller.bounds.max.y + ledgeVerticalOffsets.y,
                (isFacingRight) ? controller.bounds.max.z + ledgeVerticalOffsets.x : controller.bounds.min.z - ledgeVerticalOffsets.x);

            Debug.DrawRay(verticalRayOrigin, Vector3.down * ledgeRaysLength.y, Color.red);

            if (Physics.Raycast(verticalRayOrigin, Vector3.down, out RaycastHit verticalLedgeRayHit, ledgeRaysLength.y))
            {
                Vector3 horizontalRayOrigin = new Vector3
                    (transform.position.x,
                    verticalLedgeRayHit.point.y,
                    (isFacingRight) ? controller.bounds.max.z : controller.bounds.min.z);

                Debug.DrawRay(horizontalRayOrigin, ledgeRaysLength.x * ((isFacingRight) ? Vector3.forward : Vector3.back), Color.blue);

                if (Physics.Raycast(horizontalRayOrigin, (isFacingRight) ? Vector3.forward : Vector3.back, out RaycastHit horizontalLedgeRayHit, ledgeRaysLength.x))
                {
                    LedgeGrab(horizontalLedgeRayHit, verticalLedgeRayHit);
                }
            }
        }
    }
    private void LedgeGrab(RaycastHit horHit, RaycastHit verHit)
    {
        onLedge = true;
        movement = Vector3.zero;
        controller.enabled = false;

        transform.position = new Vector3
            (transform.position.x,
            transform.position.y - (ledgeRaysLength.y + ledgeVerticalOffsets.y + ledgeGrabOffsets.y),
            horHit.point.z - (isFacingRight ? 1 : -1) * (ledgeRaysLength.x - ledgeGrabOffsets.x));

        transform.parent = verHit.transform;

        animator.SetTrigger("OnLedge");     // Triggers the Ledge Grab animation
        animator.SetFloat("Speed", 0f);     // Resets the movement speed animator value
        animator.SetBool("Grounded", false);
    }
    private void LedgeStartClimb()
    {
        // If the player presses Space, and is on a ledge and not currently climbing it...
        if (jumpPressed && onLedge && !isClimbing)
        {
            animator.SetTrigger("Climb");   // Trigger the Climb animation
            isClimbing = true;                // This state avoids triggering again the Climb animation
        }
    }
    public void LedgeClimb()
    {
        transform.parent = null;

        // Moves the character controller to an approximate position.
        // Z offset is applied differently taking into account the direction the controller is facing
        transform.position = new Vector3
            (transform.position.x,
            transform.position.y + ledgeClimbOffset.y,
            transform.position.z + (isFacingRight ? 1 : -1) * ledgeClimbOffset.z);

        onLedge = false;
        isClimbing = false;
        controller.enabled = true;
    }

    private void OnGroundStateChange(bool grounded)
    {
        if (grounded)
        {
            jumped = false;
        }
        else
        {
            if (!jumped) movement.y = 0;
        }
    }
    private void UpdateAnimatorValues()
    {
        if (onLedge) return;

        animator.SetFloat("Speed", controller.velocity.magnitude);
        animator.SetBool("Grounded", controller.isGrounded);
    }
}