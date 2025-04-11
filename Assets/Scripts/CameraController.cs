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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!isRotating) return;

        float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
        float mouseY = lookInput.y * rotationSpeed * Time.deltaTime;

        // Yaw — rotate the player horizontally
        target.Rotate(Vector3.up * mouseX);

        // Pitch — rotate the camera vertically (clamped)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    } 



    
}
