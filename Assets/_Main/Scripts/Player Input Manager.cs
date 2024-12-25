using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    public PlayerManager player;

    PLayerControls playerControls;

    [Header("Movement Input")]
    [SerializeField] float movementInput;
    [SerializeField] float climbingInput;
    public float horizontal_Input;
    public float vertical_Input;
    public float moveAmount;

    [Header("Player Action Input")]
    [SerializeField] bool jump_Input = false;
    [SerializeField] bool sprint_Input = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // when the scene changes, run this logic
        SceneManager.activeSceneChanged += OnSceneChange;

        //instance.enabled = false;
        instance.enabled = true; // delete this and uncomment above line later when main menu logic is added

        if (playerControls != null)
        {
            //playerControls.Disable();
            playerControls.Enable(); // delete this and uncomment above line later when main menu logic is added
        }
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        // If we are loading into our world scene, enable our players controls
        if (newScene.buildIndex == 0)
        {
            instance.enabled = true;

            if (playerControls != null)
            {
                playerControls.Enable();
            }
        }
        // otherwise we must be at the main menu, disable our player controls
        // so that player cant move around if we are in the character creation menu ect.
        else
        {
            instance.enabled = false;

            if (playerControls != null)
            {
                playerControls.Disable();
            }
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PLayerControls();

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<float>();
            playerControls.PlayerMovement.Climbing.performed += i => climbingInput = i.ReadValue<float>();

            // Actions
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;

            // holding the input sets the bool to true
            playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
            // releasing the input sets the bool to false
            playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;
        }

        playerControls.Enable();
    }

    private void OnDestroy()
    {
        // if we destroy this object, unsubscribe from this event
        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    // if we minimize or lower the window, stop adjusting inputs
    private void OnApplicationFocus(bool focus)
    {
        if (enabled)
        {
            if (focus)
            {
                playerControls.Enable();
            }
            else
            {
                playerControls.Disable();
            }
        }
    }

    private void Update()
    {
        HandleAllInputs();
    }

    private void HandleAllInputs()
    {
        HandlePlayerMovementInput();
        HandleJumpInput();
    }

    // Movement
    private void HandlePlayerMovementInput()
    {
        horizontal_Input = movementInput;
        vertical_Input = climbingInput;

        if(movementInput != 0)
            moveAmount = horizontal_Input;
        else if(climbingInput != 0)
            moveAmount = vertical_Input;

        if (moveAmount > 0)
        {
            moveAmount = 1;

            if (sprint_Input)
            {
                moveAmount = 2;
            }
        }
        else if (moveAmount < 0)
        {
            moveAmount = -1;

            if (sprint_Input)
            {
                moveAmount = -2;
            }
        }
    }

    // Action
    private void HandleJumpInput()
    {
        if (jump_Input)
        {
            jump_Input = false;

            // note: return; if menu or UI window is open

            // attempt to perform jump
            player.playerMovement.AttempToPerformJump();
        }
    }
}