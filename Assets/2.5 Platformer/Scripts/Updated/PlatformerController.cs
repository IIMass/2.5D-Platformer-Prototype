using UnityEngine;

public class PlatformerController : MonoBehaviour
{
    #region Variables
    #region Components
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    #endregion

    #region Inputs
    [Header("Inputs")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private bool spacePressed;
    [SerializeField] private bool shiftPressed;
    [SerializeField] private bool ePressed;
    #endregion

    #region States
    [Header("States")]
    // Similar to a FSM, I switch behaviours from state to state to allow for better code structure
    [SerializeField] private ControllerStates currentControllerState;
    private enum ControllerStates { Grounded, InAir, OnLedge, OnLadder, Rolling }

    // This is used to rotate the character and use raycasts in the right directions
    [SerializeField] private bool isFacingRight = true;

    // Movement related booleans
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

    // Ledge related booleans
    [SerializeField] private bool onLedge;
    [SerializeField] private bool isLedgeClimbing;

    // Ladder related booleans
    [SerializeField] private bool isLadderNear;
    [SerializeField] private bool isLadderClimbing;
    [SerializeField] private bool isLadderFinishingClimb;

    // Roll related booleans
    [SerializeField] private bool isRolling;
    #endregion

    #region Animator Values
    [Header("Animator Values")]
    [SerializeField] private string animatorSpeedFloat;
    [SerializeField] private string animatorGroundedBool;
    [SerializeField] private string animatorJumpedBool;
    [SerializeField] private string animatorOnLedgeBool;
    [SerializeField] private string animatorOnLadderBool;
    [SerializeField] private string animatorRollingBool;
    [SerializeField] private string animatorJumpTrigger;
    [SerializeField] private string animatorLedgeGrabTrigger;
    [SerializeField] private string animatorLedgeClimbTrigger;
    [SerializeField] private string animatorLadderGrabTrigger;
    [SerializeField] private string animatorLadderClimbTrigger;
    [SerializeField] private string animatorRollTrigger;

    private int animatorSpeedFloatHash;
    private int animatorGroundedBoolHash;
    private int animatorJumpedBoolHash;
    private int animatorOnLedgeBoolHash;
    private int animatorOnLadderBoolHash;
    private int animatoranimatorRollingBoolHash;
    private int animatorJumpTriggerHash;
    private int animatorLedgeGrabTriggerHash;
    private int animatorLedgeClimbTriggerHash;
    private int animatorLadderGrabTriggerHash;
    private int animatorLadderClimbTriggerHash;
    private int animatorRollTriggerHash;
    #endregion

    #region Horizontal Movement
    [Header("Horizontal Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float walkDecceleration;

    [SerializeField] [Range(0f, 1f)] private float airControl;
    #endregion

    #region Vertical Movement
    [Header("Vertical Movement")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float gravityForce;
    [SerializeField] private float gravityFallMultiplier;
    [SerializeField] private float maxFallSpeed;
    #endregion

    #region Movement Current Values
    [Header("Movement Current Values")]
    [SerializeField] private Vector3 movement = Vector3.zero;
    #endregion

    #region Character Flip
    [Header("Character Flip")]
    [SerializeField] private float rotateSpeed;
    #endregion

    #region Ledge Grab & Climb
    [Header("Ledge Grab & Climb")]
    [SerializeField] private Vector2 ledgeRaysLength;
    [SerializeField] private Vector2 ledgeVerticalOffsets;
    [SerializeField] private LayerMask ledgeMask;

    [Space(5)]

    [SerializeField] private Vector2 ledgeGrabOffsets;

    [Space(5)]

    [SerializeField] private Vector3 ledgeClimbOffset;
    #endregion

    #region Ladder
    [Header("Ladder")]
    [SerializeField] private float ladderClimbSpeed;
    [SerializeField] private Ladder ladderNear;
    [SerializeField] private Ladder ladderToClimb;
    #endregion

    #region Roll
    [Header("Roll")]
    [SerializeField] private float rollSpeed;
    [SerializeField] private float rollControllerHeight;
    [SerializeField] private float rollControllerOriginalHeight;

    private bool rolledFacingRight;
    #endregion
    #endregion


    #region MonoBehaviour Methods
    // Start is called before the first frame update
    private void Start()
    {
        AnimationHash();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateInputs();
        StateUpdate();
        StateChange();
        RotateCharacter();
        UpdateAnimatorValues();
    }
    #endregion

    #region Custom Methods
    // Gets the hash of the controller's animations to optimize Animator's overhead
    private void AnimationHash()
    {
        animatorSpeedFloatHash          = Animator.StringToHash(animatorSpeedFloat);

        animatorGroundedBoolHash        = Animator.StringToHash(animatorGroundedBool);
        animatorJumpedBoolHash          = Animator.StringToHash(animatorJumpedBool);
        animatorOnLedgeBoolHash         = Animator.StringToHash(animatorOnLedgeBool);
        animatorOnLadderBoolHash        = Animator.StringToHash(animatorOnLadderBool);
        animatoranimatorRollingBoolHash = Animator.StringToHash(animatorRollingBool);

        animatorJumpTriggerHash         = Animator.StringToHash(animatorJumpTrigger);
        animatorLedgeGrabTriggerHash    = Animator.StringToHash(animatorLedgeGrabTrigger);
        animatorLedgeClimbTriggerHash   = Animator.StringToHash(animatorLedgeClimbTrigger);
        animatorLadderGrabTriggerHash   = Animator.StringToHash(animatorLadderGrabTrigger);
        animatorLadderClimbTriggerHash  = Animator.StringToHash(animatorLadderClimbTrigger);
        animatorRollTriggerHash         = Animator.StringToHash(animatorRollTrigger);
    }


    // Check for player inputs every frame
    private void UpdateInputs()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        spacePressed = Input.GetKeyDown(KeyCode.Space);
        shiftPressed = Input.GetKeyDown(KeyCode.LeftShift);
        ePressed = Input.GetKeyDown(KeyCode.E);
    }

    // Updates Speed and Ground animator values
    private void UpdateAnimatorValues()
    {
        animator.SetFloat(animatorSpeedFloatHash, controller.velocity.magnitude);
        animator.SetBool(animatorGroundedBoolHash, controller.isGrounded);
    }

    // Updates local Grounded variable
    private void GroundCheck()
    {
        Grounded = controller.isGrounded;
    }

    // Executes the current state's behaviour
    private void StateUpdate()
    {
        switch (currentControllerState)
        {
            case ControllerStates.Grounded:
                GroundedState();
                RollStart();
                GroundCheck();
                LadderCheck();
                break;

            case ControllerStates.InAir:
                InAirState();
                GroundCheck();
                LedgeCheck();
                LadderCheck();
                break;

            case ControllerStates.OnLedge:
                OnLedgeState();
                break;

            case ControllerStates.OnLadder:
                OnLadderState();
                GroundCheck();
                break;

            case ControllerStates.Rolling:
                RollState();
                GroundCheck();
                break;
        }
    }

    // Checks for available transitions and switches the current state
    private void StateChange()
    {
        switch (currentControllerState)
        {
            case ControllerStates.Grounded:
                if (!Grounded) currentControllerState = ControllerStates.InAir; 
                break;

            case ControllerStates.InAir:
                if (Grounded) currentControllerState = ControllerStates.Grounded;
                break;

            case ControllerStates.OnLedge:
                if (!onLedge) currentControllerState = (Grounded) ? ControllerStates.Grounded : ControllerStates.InAir;
                break;

            case ControllerStates.OnLadder:
                if (!isLadderClimbing) currentControllerState = (Grounded) ? ControllerStates.Grounded : ControllerStates.InAir;
                break;

            case ControllerStates.Rolling:
                if (!isRolling) currentControllerState = (Grounded) ? ControllerStates.Grounded : ControllerStates.InAir;
                break;
        }
    }


    #region Movement Methods
    private void OnGroundStateChange(bool grounded)
    {
        if (Grounded) JumpReset();
        else if (!jumped) movement.y = 0;
    }

    private void GroundedState()
    {
        // Horizontal Movement. Accelerates towards walkSpeed when horInput is not 0, deccelerates otherwise
        movement.z = (moveInput.x != 0) ? Mathf.MoveTowards(movement.z, walkSpeed * moveInput.x, walkAcceleration * Time.deltaTime) : Mathf.MoveTowards(movement.z, 0f, walkDecceleration * Time.deltaTime);

        // Gravity. Apply constant downwards force
        movement.y = -gravityForce;

        // Allow Jump when Grounded
        Jump();

        // Move character with current movement
        Move();
    }
    private void InAirState()
    {
        // Horizontal Movement. Effected by airControl multiplier
        movement.z = (moveInput.x != 0) ? Mathf.MoveTowards(movement.z, walkSpeed * moveInput.x, walkAcceleration * airControl * Time.deltaTime) : movement.z;

        // Gravity. When the controller starts falling, gravityFallMultiplier adds extra downwards force
        movement.y = (movement.y >= 0) ? movement.y - gravityForce * Time.deltaTime : movement.y - gravityForce * gravityFallMultiplier * Time.deltaTime;

        // Move character with current movement
        Move();
    }

    private void Jump()
    {
        // If the player presses space, and hasn't jumped yet
        if (spacePressed && !jumped)
        {
            jumped = true;
            movement.y = jumpForce;

            animator.SetTrigger(animatorJumpTriggerHash);
            animator.SetBool(animatorJumpedBoolHash, jumped);
        }
    }
    private void JumpReset()
    {
        jumped = false;
        animator.SetBool(animatorJumpedBoolHash, jumped);
    }

    private void Move()
    {
        // Clamp Y velocity
        movement.y = Mathf.Clamp(movement.y, -maxFallSpeed, jumpForce);

        // Moves controller
        controller.Move(movement * Time.deltaTime);
    }
    private void RotateCharacter()
    {
        // Flip character only when in ground
        if (currentControllerState == ControllerStates.Grounded)
        {
            if (movement.z != 0) isFacingRight = movement.z > 0;
        }

        // Smoothly rotates the character's mesh 
        if (isFacingRight) animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, Quaternion.Euler(0f, 0f, 0f), rotateSpeed * Time.deltaTime);
        else animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, Quaternion.Euler(0f, -180, 0f), rotateSpeed * Time.deltaTime);
    }
    #endregion

    #region Ledge Methods
    private void LedgeCheck()
    {
        // Vertical Ray Origin
        Vector3 verticalRayOrigin = new Vector3
        (transform.position.x,
        controller.bounds.max.y + ledgeVerticalOffsets.y,
        (isFacingRight) ? controller.bounds.max.z + ledgeVerticalOffsets.x : controller.bounds.min.z - ledgeVerticalOffsets.x);

        // If the vertical ray detects a collider...
        if (Physics.Raycast(verticalRayOrigin, Vector3.down, out RaycastHit verticalLedgeRayHit, ledgeRaysLength.y, ledgeMask, QueryTriggerInteraction.Ignore))
        {
            // Horizontal Ray Origin
            Vector3 horizontalRayOrigin = new Vector3
            (transform.position.x,
            verticalLedgeRayHit.point.y,
            (isFacingRight) ? controller.bounds.max.z : controller.bounds.min.z);

            // If the horizontal ray detects a collider...
            if (Physics.Raycast(horizontalRayOrigin, (isFacingRight) ? Vector3.forward : Vector3.back, out RaycastHit horizontalLedgeRayHit, ledgeRaysLength.x, ledgeMask, QueryTriggerInteraction.Ignore))
            {
                // Grab ledge
                LedgeGrab(horizontalLedgeRayHit, verticalLedgeRayHit);

                // Draw collision lines
                Debug.DrawLine(verticalRayOrigin, verticalLedgeRayHit.point, Color.red, 10f);
                Debug.DrawLine(horizontalRayOrigin, horizontalLedgeRayHit.point, Color.blue, 10f);
            }
        }
    }
    private void LedgeGrab(RaycastHit horHit, RaycastHit verHit)
    {
        // Switch to OnLedge State
        currentControllerState = ControllerStates.OnLedge;
        onLedge = true;

        // Reset movement value and disable the Character Controller
        movement = Vector3.zero;
        controller.enabled = false;

        // Adjust the controller position to the ledge
        transform.position = new Vector3
            (transform.position.x,
            transform.position.y - (ledgeRaysLength.y + ledgeVerticalOffsets.y + ledgeGrabOffsets.y),
            horHit.point.z - (isFacingRight ? 1 : -1) * (ledgeRaysLength.x - ledgeGrabOffsets.x));

        // Parent it to the vertical hit collider to move along moving platforms
        transform.parent = verHit.transform;

        animator.SetTrigger(animatorLedgeGrabTriggerHash);      // Triggers the Ledge Grab animation
        animator.SetBool(animatorOnLedgeBoolHash, onLedge);     // Updates OnLedge Animator boolean
    }

    private void OnLedgeState()
    {
        // If the player presses Space, and is on a ledge and not currently climbing it...
        if (spacePressed && onLedge && !isLedgeClimbing)
        {
            isLedgeClimbing = true;                                 // This state avoids triggering again the Climb animation
            animator.SetTrigger(animatorLedgeClimbTriggerHash);     // Trigger the Climb animation
        }
    }

    // Called through LedgeClimbBehaviour OnStateExit
    public void LedgeClimb()
    {
        // Unparent the Controller
        transform.parent = null;

        // Moves the character controller to an approximate position.
        // Z offset is applied differently taking into account the direction the controller is facing
        transform.position = new Vector3(transform.position.x, transform.position.y + ledgeClimbOffset.y, transform.position.z + (isFacingRight ? 1 : -1) * ledgeClimbOffset.z);

        // The controller has stopped climbing and is no longer on the ledge.
        onLedge = false;
        isLedgeClimbing = false;

        // Enables the controller
        controller.enabled = true;

        // Updates OnLedge Animator boolean
        animator.SetBool(animatorOnLedgeBoolHash, onLedge);   
    }
    #endregion

    #region Ladder Methods
    // Stores temporarily the nearby ladder
    public void LadderNearAssign(Ladder ladder, bool near)
    {
        ladderNear = ladder;
        isLadderNear = near;
    }

    private void LadderCheck()
    {
        // If is on Ledge, stop checking for ladders
        if (onLedge) return;

        // If the player pressed E and is Near a ladder...
        if (ePressed && isLadderNear)
        {
            // Switch current controller state to OnLadder
            currentControllerState = ControllerStates.OnLadder;

            // Set ladder to climb to the nearby ladder
            ladderToClimb = ladderNear;

            controller.enabled = false; // Disable controller to move it through transform.position
            isLadderClimbing = true;    // Controller is now ladder climbing
            Grounded = false;           // Reset Ground state
            movement = Vector3.zero;    // Reset Movement values           

            JumpReset();

            // Face towards the ladder
            isFacingRight = ladderToClimb.GetLadderTravelBounds().transform.position.z <= ladderToClimb.transform.position.z;

            // Position to Move the controller near the ladder
            // Adjusts controller Z position to the middle of the Ladder Travel Bounds
            Vector3 ladderPos = new Vector3(transform.position.x, transform.position.y, ladderToClimb.GetLadderTravelBounds().bounds.center.z);

            // Adjusts controller Y position to the Ladder Travel Bounds
            if (transform.position.y >= ladderToClimb.GetLadderTravelBounds().bounds.max.y)
            {
                ladderPos.y = ladderToClimb.GetLadderTravelBounds().bounds.max.y;
            }
            else if (transform.position.y < ladderToClimb.GetLadderTravelBounds().bounds.min.y)
            {
                ladderPos.y = ladderToClimb.GetLadderTravelBounds().bounds.min.y;
            }

            // Move controller to ladderPoss
            transform.position = ladderPos;
            controller.enabled = true;

            animator.SetTrigger(animatorLadderGrabTriggerHash);             // Triggers the Ladder Grab animation
            animator.SetBool(animatorOnLadderBoolHash, isLadderClimbing);   // Updates OnLadder Animator boolean
        }
    }
    private void OnLadderState()
    {
        // If the controller is finishing the climb, or has no reference to a ladder to climb, stop executing here
        if (isLadderFinishingClimb || ladderToClimb == null) return;

        // Vertical movement is only allowed
        controller.Move(Vector3.up * ladderClimbSpeed * moveInput.y * Time.deltaTime);

        // If the player presses Space...
        if (spacePressed)
        {
            // Move horizontally in the opposite direction the controller is facing
            movement.z = (isFacingRight ? -1 : 1) * walkSpeed;

            // Invert isFacingRight to rotate the controller to the opposite direction
            isFacingRight = !isFacingRight;

            Jump();

            LadderEndClimb();
            return;
        }

        // If the Controller touches the ground or the ladder is no longer near it, end climb
        if (Grounded || !isLadderNear)
        {
            LadderEndClimb();
            return;
        }

        // If the controller leaves travel max Y bounds, trigger climb animation
        if (!Grounded && isLadderNear && transform.position.y > ladderToClimb.GetLadderTravelBounds().bounds.max.y)
        {
            isLadderFinishingClimb = true;
            animator.SetTrigger(animatorLadderClimbTriggerHash);
            return;
        }
    }
    private void LadderEndClimb()
    {
        ladderToClimb = null;
        isLadderClimbing = false;
        animator.SetBool(animatorOnLadderBoolHash, isLadderClimbing);
    }

    // Called through LadderClimbBehaviour OnStateExit
    public void LadderEndClimbAnimation()
    {
        transform.position = ladderToClimb.GetLadderUpEnd().position;
        isLadderFinishingClimb = false;
        LadderEndClimb();
    }
    #endregion

    #region Roll Methods
    private void RollStart()
    {
        // If the player has pressed LeftShift...
        if (shiftPressed)
        {
            // Switch current controller state to Rolling
            currentControllerState = ControllerStates.Rolling;

            isRolling = true;
            rolledFacingRight = isFacingRight;  // Stores the controller's direction

            controller.height = rollControllerHeight;                       // Sets controller height to a smaller value to allow rolling through narrow spaces
            controller.center = Vector3.up * (controller.height / 2f);      // Adjusts center of the controller to avoid sinking the character model to the ground

            animator.SetTrigger(animatorRollTriggerHash);                   // Triggers the Roll animation
            animator.SetBool(animatoranimatorRollingBoolHash, isRolling);   // Updates Rolling Animator boolean
        }
    }

    private void RollState()
    {
        // Move with rollSpeed towards the direction the controller is facing
        movement.z = rollSpeed * (rolledFacingRight ? 1 : -1);

        // Gravity
        if (Grounded) movement.y = -gravityForce;
        else movement.y = (movement.y >= 0) ? movement.y - gravityForce * Time.deltaTime : movement.y - gravityForce * gravityFallMultiplier * Time.deltaTime;

        Move();
    }

    // Called through RollBehaviour OnStateExit
    public void RollEnd()
    {
        // Controller is no longer rolling
        isRolling = false;
        animator.SetBool(animatoranimatorRollingBoolHash, isRolling);

        controller.height = rollControllerOriginalHeight;           // Sets controller height back to the original value
        controller.center = Vector3.up * (controller.height / 2f);  // Restores original controller center
    }
    #endregion
    #endregion
}