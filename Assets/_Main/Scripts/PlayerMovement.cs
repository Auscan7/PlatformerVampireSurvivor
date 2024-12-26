using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : CharacterMovement
{
    PlayerManager player;

    [HideInInspector] public float horizontalMovement;
    [HideInInspector] public float verticalMovement;

    [Header("Movement Settings")]
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float movementSpeedMultiplier = 1;
    [SerializeField] float airControlFactor = 4;
    [SerializeField] bool facingRight = true;
    [SerializeField] bool facingUp = true;
    private Vector3 moveDirection;

    [Header("Jump")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float coyoteTimeCounter;
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] float jumpBufferCounter;

    [Header("Climb")]
    [SerializeField] float climbingSpeed = 5f;
    [SerializeField] float wallStickForce = 5f;
    [SerializeField] bool isClimbing = false;

    [Header("Wall Detection")]
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 1f);
    [SerializeField] private Vector2 wallCheckRightOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Vector2 wallCheckLeftOffset = new Vector2(-0.5f, 0f);
    private bool isTouchingWallRight;
    private bool isTouchingWallLeft;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
    }

    protected override void Start()
    {
        base.Start();

        isClimbing = false;
    }

    protected override void Update()
    {
        base.Update();

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        CheckWallCollision();        
    }

    private void FixedUpdate()
    {
        if (horizontalMovement > 0 && !facingRight && !facingUp)
        {
            FlipPlayerHorizontal();
        }
        else if (horizontalMovement < 0 && facingRight && facingUp)
        {
            FlipPlayerHorizontal();
        }

        if (isTouchingWallRight)
        {
            if (verticalMovement > 0 && !facingUp && !facingRight)
            {
                FlipPlayerVertical();
            }
            else if (verticalMovement < 0 && facingUp && facingRight)
            {
                FlipPlayerVertical();
            }
        }
        else if (isTouchingWallLeft)
        {
            if (verticalMovement > 0 && facingUp && facingRight)
            {
                FlipPlayerVertical();
            }
            else if (verticalMovement < 0 && !facingUp && !facingRight)
            {
                FlipPlayerVertical();
            }
        }
    }

    public void HandleAllMovement()
    {
        if (isGrounded)
        {
            HandleGroundedMovement();
        }
        else if (!isGrounded && !isClimbing)
        {
            HandleAerialMovement();
        }

        // Safety check: Ensure gravity is enabled if not climbing or touching a wall
        if (!isClimbing && !isTouchingWallRight && !isTouchingWallLeft)
        {
            player.rb.gravityScale = 7; // Default gravity
        }

        if (isClimbing)
        {
            HandleClimbingMovement();
        }
    }

    // Gets the movement values from the Input Manager
    private void GetMovementValues()
    {
        horizontalMovement = PlayerInputManager.instance.horizontal_Input;
        verticalMovement = PlayerInputManager.instance.vertical_Input;
    }

    //Flips the player facing towards the movement direction
    private void FlipPlayerHorizontal()
    {
        if (isClimbing)
            return;

        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
        facingUp = !facingUp;
    }

    private void FlipPlayerVertical()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingUp = !facingUp;
        facingRight = !facingRight;
    }

    // Wall Climbing logic
    private void CheckWallCollision()
    {
        Vector2 rightPosition = (Vector2)transform.position + wallCheckRightOffset;
        Vector2 leftPosition = (Vector2)transform.position + wallCheckLeftOffset;

        isTouchingWallRight = Physics2D.OverlapBox(rightPosition, wallCheckSize, 0f, groundLayer);
        isTouchingWallLeft = Physics2D.OverlapBox(leftPosition, wallCheckSize, 0f, groundLayer);

        if ((isTouchingWallRight || isTouchingWallLeft))
        {
            isClimbing = true;
            RotatePlayerForWall();
        }
        else
        {
            isClimbing = false;
            ResetPlayerRotation();
        }
    }

    private void RotatePlayerForWall()
    {
        if (isTouchingWallRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90); // Rotate for right wall
        }
        else if (isTouchingWallLeft)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90); // Rotate for left wall
        }
    }

    private void ResetPlayerRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0); // Reset rotation
    }

    private void HandleClimbingMovement()
    {
        GetMovementValues();
        float speedMultiplier = Mathf.Abs(PlayerInputManager.instance.moveAmount);
        float effectiveSpeed = (climbingSpeed * movementSpeedMultiplier) * (speedMultiplier / 1.5f);

        // Apply climbing movement (vertical)
        player.rb.velocity = new Vector2(player.rb.velocity.x, verticalMovement * effectiveSpeed);

        // Apply additional force towards the wall
        Vector2 wallDirection = Vector2.zero;

        if (isTouchingWallRight || isTouchingWallLeft)
        {
            wallDirection = -transform.up; // this direction is always towards the wall since we rotate the player parent object 90 on the Z axis
        }

        if (wallDirection != Vector2.zero)
        {
            player.rb.AddForce(wallDirection * wallStickForce, ForceMode2D.Force);
        }

        // Disable gravity while climbing
        player.rb.gravityScale = 0;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the collision object is part of the wall layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isClimbing = false;
            ResetPlayerRotation();

            // Reset gravity scale if not touching any wall or ground
            if (!isTouchingWallRight && !isTouchingWallLeft && !isGrounded)
            {
                player.rb.gravityScale = 7; // Default gravity
            }
        }
    }

    // Grounded movement logic
    private void HandleGroundedMovement()
    {
        GetMovementValues();
        // Calculate speed based on moveAmount
        float speedMultiplier = Mathf.Abs(PlayerInputManager.instance.moveAmount);
        float effectiveSpeed = (walkingSpeed * movementSpeedMultiplier) * (speedMultiplier / 1.5f);

        // Horizontal movement on the ground
        player.rb.velocity = new Vector2(horizontalMovement * effectiveSpeed, player.rb.velocity.y);

        // Add drag when not moving
        player.rb.drag = (Mathf.Abs(horizontalMovement) > 0.01f) ? 0 : 7;
    }

    // Aerial movement and jump logic
    private void HandleAerialMovement()
    {
        GetMovementValues();

        // Calculate speed based on moveAmount
        float speedMultiplier = Mathf.Abs(PlayerInputManager.instance.moveAmount);
        float effectiveSpeed = (walkingSpeed * movementSpeedMultiplier) * (speedMultiplier / 1.5f);

        // Apply horizontal control in air with limited influence
        float targetHorizontalSpeed = horizontalMovement * effectiveSpeed;
        float smoothedHorizontalSpeed = Mathf.Lerp(player.rb.velocity.x, targetHorizontalSpeed, airControlFactor * Time.deltaTime);
        player.rb.velocity = new Vector2(smoothedHorizontalSpeed, player.rb.velocity.y);

        // Lower drag in air to maintain momentum
        player.rb.drag = 0;
    }

    public void AttempToPerformJump()
    {
        if(isClimbing)
            return;

        // Apply an upward force for jumping
        if (coyoteTimeCounter > 0f || isGrounded)
        {
            player.rb.velocity = new Vector2(player.rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
        }
    }

    // Debug
    private void OnDrawGizmos()
    {

        // Right wall check
        Vector2 rightPosition = (Vector2)transform.position + wallCheckRightOffset;
        Gizmos.color = isTouchingWallRight ? Color.green : Color.red;
        Gizmos.DrawWireCube(rightPosition, wallCheckSize);

        // Left wall check
        Vector2 leftPosition = (Vector2)transform.position + wallCheckLeftOffset;
        Gizmos.color = isTouchingWallLeft ? Color.green : Color.red;
        Gizmos.DrawWireCube(leftPosition, wallCheckSize);
    }
}