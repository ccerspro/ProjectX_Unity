using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class CameraController : MonoBehaviour
{
    public Transform target; 

    [SerializeField]
    [Range(1f, 300f)]
    public float rotationSpeed = 300f;  

    private float xRotation = 0f;  // Track vertical pitch rotation
    private PlayerInputActions playerInputActions;
    private bool isRotating = false;  // Track if the camera is rotating
    private Vector2 lookInput = Vector2.zero;  // Store input from mouse or joystick

    private void Start()
    {
        // Hide and lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        // Initialize input actions
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Enable Look input and subscribe to events
        playerInputActions.Player.Look.performed += OnLookPerformed;
        playerInputActions.Player.Look.canceled += OnLookCanceled;  // Stop rotation when input stops
        playerInputActions.Player.Look.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        playerInputActions.Player.Look.performed -= OnLookPerformed;
        playerInputActions.Player.Look.canceled -= OnLookCanceled;
        playerInputActions.Player.Look.Disable();
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        // Store the input values and mark as rotating
        lookInput = context.ReadValue<Vector2>();
        isRotating = true;
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        // Stop rotating when input stops
        isRotating = false;
        lookInput = Vector2.zero;  // Reset input
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlock the cursor when Escape is pressed
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            // Lock the cursor when the user clicks
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!isRotating) return;

        // Calculate rotation step size
        float step = rotationSpeed * Time.deltaTime;

        // Yaw (horizontal rotation) – Rotate the player body
        Quaternion currentYaw = target.rotation;
        Quaternion targetYaw = Quaternion.Euler(0f, target.eulerAngles.y + lookInput.x, 0f);
        target.rotation = Quaternion.RotateTowards(currentYaw, targetYaw, step);

        // Pitch (vertical rotation) – Rotate the camera
        xRotation -= lookInput.y;  // Invert mouse Y-axis
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Clamp pitch rotation

        Quaternion targetPitch = Quaternion.Euler(xRotation, 0f, 0f);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetPitch, step);

    }



    
}
