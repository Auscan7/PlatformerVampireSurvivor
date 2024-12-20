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
    [SerializeField] Vector2 movementInput;
    public float horizontal_Input;
    public float moveAmount;

    [Header("Player Action Input")]
    [SerializeField] bool dodge_Input = false;

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

            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            // Actions
            //playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true; //Gamepad: B(East), Keyboard:Space

            // holding the input sets the bool to true
            //playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true; //Gamepad: B(East), Keyboard:LShift
            // releasing the input sets the bool to false
            //playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false; //Gamepad: B(East), Keyboard:LShift
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
        HandleDodgeInput();
    }

    // Movement
    private void HandlePlayerMovementInput()
    {
        horizontal_Input = movementInput.x;

        moveAmount = horizontal_Input;

        if (moveAmount > 0)
        {
            moveAmount = 1;
        }
        else if (moveAmount < 0)
        {
            moveAmount = -1;
        }

        // why do we pass 0 on the horizontal? it is because we only want non-strafing movement
        // we use the horizontal when we are strafing or locked on

        //if (player == null)
        //    return;

        //if (moveAmount != 0)
        //{
        //    player.playerNetworkManager.isMoving.Value = true;
        //}
        //else
        //{
        //    player.playerNetworkManager.isMoving.Value = false;
        //}

        //// if we are not locked on, only use the move amount
        //if (!player.playerNetworkManager.isLockedOn.Value || player.playerNetworkManager.isSprinting.Value)
        //{
        //    player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
        //}
        //// if we are locked on pass the horizontal movement as well
        //else
        //{
        //    player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontal_Input, vertical_Input, player.playerNetworkManager.isSprinting.Value);
        //}
    }

    // Action
    private void HandleDodgeInput()
    {
        if (dodge_Input)
        {
            dodge_Input = false;

            // note: return; if menu or UI window is open

            //player.playerLocomotionManager.AttempToPerformDodge();
        }
    }
}