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

        public bool IsMoving { get; private set; }
        public bool hasFlashlight = true;// Flag to check if the player has a flashlight


        private InputAction moveAction;

        [SerializeField] private GameObject flashlight; // Reference to the flashlight GameObject



        private Rigidbody rb; // Reference to Rigidbody component
        private Vector3 movementInput; // Stores movement direction

        private PlayerInputActions playerInputActions;
        //public InfiniteCorridor infiniteCorridor;
        public AudioManager audioManager;


        private void Awake()
        {
            // Initialize input actions
            playerInputActions = new PlayerInputActions();

            // Get Rigidbody component
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found on PlayerController GameObject.");
            }



        }

        private void OnEnable()
        {
            // Set the player transform in PlayerLocator
            PlayerLocator.Set(transform);

            // Enable input actions
            moveAction = playerInputActions.Player.Move;
            moveAction.Enable();
            playerInputActions.Player.Quit.Enable();
            playerInputActions.Player.FlashLight.Enable();
            playerInputActions.Player.Reset.Enable();
            playerInputActions.Player.Sprint.Enable();


            // Subscribe to events
            playerInputActions.Player.FlashLight.performed += OnFlashLight;
            playerInputActions.Player.Sprint.performed += OnSprint;
            playerInputActions.Player.Sprint.canceled += OnSprintRelease;
            playerInputActions.Player.Quit.performed += OnQuit;
            playerInputActions.Player.Reset.performed += OnReset;


            // Set default speed
            currentSpeed = normalSpeed;
        }

        private void OnDisable()
        {
            if (PlayerLocator.Player == transform) PlayerLocator.Clear();
            moveAction.Disable();
            playerInputActions.Player.Sprint.Disable();
            playerInputActions.Player.Quit.Disable();
            playerInputActions.Player.FlashLight.Disable();
            playerInputActions.Player.Reset.Disable();
            // Unsubscribe from events
            playerInputActions.Player.Sprint.performed -= OnSprint;
            playerInputActions.Player.Sprint.canceled -= OnSprintRelease;
            playerInputActions.Player.Quit.performed -= OnQuit;
            playerInputActions.Player.FlashLight.performed -= OnFlashLight;
            playerInputActions.Player.Reset.performed -= OnReset;
        }

        private void Update()
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            movementInput = new Vector3(input.x, 0, input.y).normalized;

            IsMoving = movementInput.magnitude > 0.1f;

            HandleFootsteps();

            CheckForFall(); // Check if the player has fallen below a certain height
        }

        private void OnReset(InputAction.CallbackContext context)
        {
            // Relod the current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        }

        private void CheckForFall()
        {
            // Check if the player has fallen below a certain height
            if (transform.position.y < -10f) // Adjust the threshold as needed
            {
                Debug.Log("Player has fallen below the threshold. Reloading scene...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }

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

        void OnFlashLight(InputAction.CallbackContext context)
        {
            if(!hasFlashlight) return; // Check if the player has a flashlight
            if (flashlight == null)
            {
                Debug.LogWarning("Flashlight GameObject is not assigned.");
                return;
            }
            if (flashlight.activeSelf)
            {
                flashlight.SetActive(false); // Turn off the flashlight
                Debug.Log("Flashlight turned off");
            }
            else
            {
                flashlight.SetActive(true); // Turn on the flashlight
                Debug.Log("Flashlight turned on");
            }
            // Toggle flashlight functionality here
            // This method can be used to toggle the flashlight on/off
            Debug.Log("Flashlight toggled");
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

