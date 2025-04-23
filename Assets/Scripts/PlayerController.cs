using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZhouSoftware
{
    public class PlayerController : MonoBehaviour
    {
        public float normalSpeed = 5f; // Normal movement speed
        public float sprintSpeed = 10f; // Sprint movement speed
        private float currentSpeed; // Active speed during movement
        

        private InputAction moveAction;
        private InputAction quitAction;



        private Rigidbody rb; // Reference to Rigidbody component
        private Vector3 movementInput; // Stores movement direction

        private PlayerInputActions playerInputActions;
        //public InfiniteCorridor infiniteCorridor;
        public AudioManager audioManager;


        private void Awake()
        {
            // Initialize input actions
            playerInputActions = new PlayerInputActions();
            quitAction = playerInputActions.Player.Quit;

            // Get Rigidbody component
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found on PlayerController GameObject.");
            }


            
        }

        private void OnEnable()
        {
            // Enable input actions
            moveAction = playerInputActions.Player.Move;
            moveAction.Enable();
            quitAction.Enable();

            // Enable sprint actions
            playerInputActions.Player.Sprint.performed += OnSprint;
            playerInputActions.Player.Sprint.canceled += OnSprintRelease;
            playerInputActions.Player.Sprint.Enable();
            quitAction.performed += OnQuit;
            playerInputActions.Player.Reset.performed += OnReset;

            // Set default speed
            currentSpeed = normalSpeed;
        }

        private void OnDisable()
        {
            moveAction.Disable();
            playerInputActions.Player.Sprint.Disable();
            quitAction.Disable();

            playerInputActions.Player.Sprint.performed -= OnSprint;
            playerInputActions.Player.Sprint.canceled -= OnSprintRelease;
            quitAction.performed -= OnQuit;
        }

        private void Update()
        {
            // Read movement input (WASD or Arrow keys)
            Vector2 input = moveAction.ReadValue<Vector2>();
            movementInput = new Vector3(input.x, 0, input.y).normalized;
            HandleFootsteps();
        }
        private void OnReset(InputAction.CallbackContext context)
        {
            // Relod the current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        }
        

        private void HandleFootsteps()
        {
            if (movementInput.magnitude > 0.1f) // Check if the player is moving
            {
                if (!audioManager.IsPlaying("FootStep")) // Check if the sound is already playing
                {
                    audioManager.Play("FootStep"); // Start playing the footstep sound
                }
            }
            else
            {
                audioManager.Stop("FootStep"); // Stop playing when the player is idle
            }
        }



        private void FixedUpdate()
        {
            // Apply movement using Rigidbody's velocity
            rb.velocity = transform.TransformDirection(movementInput) * currentSpeed + new Vector3(0, rb.velocity.y, 0);
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            currentSpeed = sprintSpeed;
        }

        private void OnSprintRelease(InputAction.CallbackContext context)
        {
            currentSpeed = normalSpeed;
        }

        
          
        

        private void OnQuit(InputAction.CallbackContext context)
        {
            // Quit the application
            Debug.Log("Quitting the game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}

