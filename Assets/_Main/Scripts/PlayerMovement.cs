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
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private float wallCheckRightLength = 0.1f;
    [SerializeField] private float wallCheckLeftLength = 0.1f;
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

    private void GetMovementValues()
    {
        horizontalMovement = PlayerInputManager.instance.horizontal_Input;
        verticalMovement = PlayerInputManager.instance.vertical_Input;
    }

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
        if (isGrounded)
            return;

        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingUp = !facingUp;
        facingRight = !facingRight;
    }

    private void CheckWallCollision()
    {
        Vector2 rightPosition = (Vector2)transform.position + Vector2.right * wallCheckRightLength; // Adjust 0.5f as needed
        Vector2 leftPosition = (Vector2)transform.position + Vector2.left * wallCheckLeftLength;

        isTouchingWallRight = Physics2D.OverlapCircle(rightPosition, wallCheckRadius, wallLayer);
        isTouchingWallLeft = Physics2D.OverlapCircle(leftPosition, wallCheckRadius, wallLayer);

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

    private void HandleClimbingMovement()
    {
        GetMovementValues();

        // Apply climbing movement (vertical)
        float effectiveSpeed = climbingSpeed * Mathf.Abs(PlayerInputManager.instance.moveAmount);
        player.rb.velocity = new Vector2(player.rb.velocity.x, verticalMovement * effectiveSpeed);

        // Apply additional force towards the wall
        Vector2 wallDirection = Vector2.zero;

        if (isTouchingWallRight)
        {
            wallDirection = -transform.up;  // Pull towards the right wall
        }
        else if (isTouchingWallLeft)
        {
            wallDirection = -transform.up;   // Pull towards the left wall
        }

        // Apply force towards the wall (scaled by wallStickForce)
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
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
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

    private void OnDrawGizmos()
    {
        // Right wall check
        Gizmos.color = isTouchingWallRight ? Color.green : Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + Vector2.right * wallCheckRightLength, wallCheckRadius);

        // Left wall check
        Gizmos.color = isTouchingWallLeft ? Color.green : Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + Vector2.left * wallCheckLeftLength, wallCheckRadius);
    }

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
        // Apply an upward force for jumping
        if (coyoteTimeCounter > 0f || isGrounded)
        {
            player.rb.velocity = new Vector2(player.rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
        }
    }
}