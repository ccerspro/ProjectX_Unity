using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target; 
    [Range(1f, 300f)] public float rotationSpeed = 300f;

    public float idleSwayAmount = 0.05f;
    public float idleSwaySpeed = 1f;
    public float movementSwayAmount = 0.15f;
    public float movementSwaySpeed = 3f;

    private float xRotation = 0f;
    private PlayerInputActions playerInputActions;
    private bool isRotating = false;
    private Vector2 lookInput = Vector2.zero;

    private float swayTimer = 0f;
    private Vector3 initialLocalPosition;

    private ZhouSoftware.PlayerController playerController;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Seed xRotation from whatever you set in the Inspector
        float pitch = transform.localEulerAngles.x;
        if (pitch > 180f) pitch -= 360f;              // map to [-180, 180]
        xRotation = Mathf.Clamp(pitch, -90f, 90f);    // keep your clamp

        initialLocalPosition = transform.localPosition;
        playerController = target.GetComponent<ZhouSoftware.PlayerController>();
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Look.performed += OnLookPerformed;
        playerInputActions.Player.Look.canceled += OnLookCanceled;
        playerInputActions.Player.Look.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Look.performed -= OnLookPerformed;
        playerInputActions.Player.Look.canceled -= OnLookCanceled;
        playerInputActions.Player.Look.Disable();
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        isRotating = true;
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        isRotating = false;
        lookInput = Vector2.zero;
    }

    private void Update()
    {
        HandleCursorLock();
        HandleRotation();
        HandleIdleSway();
    }

    private void HandleCursorLock()
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
    }

    private void HandleRotation()
    {
        if (!isRotating) return;

        float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
        float mouseY = lookInput.y * rotationSpeed * Time.deltaTime;

        target.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleIdleSway()
    {
        if (playerController == null) return;

        swayTimer += Time.deltaTime * (playerController.IsMoving ? movementSwaySpeed : idleSwaySpeed);
        float swayAmount = playerController.IsMoving ? movementSwayAmount : idleSwayAmount;

        float swayX = Mathf.Sin(swayTimer) * swayAmount;
        float swayY = Mathf.Cos(swayTimer * 2f) * swayAmount * 0.5f;

        transform.localPosition = initialLocalPosition + new Vector3(swayX, swayY, 0f);
    }
}
