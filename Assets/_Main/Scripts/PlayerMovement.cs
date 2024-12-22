using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : CharacterMovement
{
    PlayerManager player;

    [HideInInspector] public float horizontalMovement;

    [Header("Movement Settings")]
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float movementSpeedMultiplier = 1;
    [SerializeField] float airControlFactor = 4;
    [SerializeField] bool facingRight = true;
    private Vector3 moveDirection;

    [Header("Jump")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float coyoteTimeCounter;
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] float jumpBufferCounter;


    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();
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
    }

    private void FixedUpdate()
    {
        if (horizontalMovement > 0 && !facingRight)
        {
            FlipPlayer();
        }
        else if (horizontalMovement < 0 && facingRight)
        {
            FlipPlayer();
        }
    }

    public void HandleAllMovement()
    {
        if (isGrounded)
        {
            HandleGroundedMovement();
        }
        else if (!isGrounded)
        {
            HandleAerialMovement();
        }
    }

    private void GetMovementValues()
    {
        horizontalMovement = PlayerInputManager.instance.horizontal_Input;
    }

    private void FlipPlayer()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        facingRight = !facingRight;
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