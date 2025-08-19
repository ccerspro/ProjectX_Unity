using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                         // your player root (yaw on this)

    [Header("Look")]
    [SerializeField] private PlayerInput playerInput; // drag Player's PlayerInput here
    [Range(1f, 300f)] public float rotationSpeed = 300f;

    [Header("Sway")]
    public float idleSwayAmount = 0.05f;
    public float idleSwaySpeed  = 1f;
    public float movementSwayAmount = 0.15f;
    public float movementSwaySpeed  = 3f;

    // internals
    private float xRotation;                         // camera (pitch)
    private bool  isRotating;
    private Vector2 lookInput;
    private float swayTimer;
    private Vector3 initialLocalPosition;
    private ZhouSoftware.PlayerController playerController;

    private InputAction lookAction;
    private bool readyForLook, ignoreNextDelta;

    void Awake()
    {
        if (!playerInput) playerInput = GetComponentInParent<PlayerInput>();
    }

    void OnEnable()
    {
        var a = playerInput.actions;
        lookAction = a.FindAction("Look", throwIfNotFound: true);
        lookAction.performed += OnLookPerformed;
        lookAction.canceled  += OnLookCanceled;
    }

    void OnDisable()
    {
        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.canceled  -= OnLookCanceled;
        }
    }

    private System.Collections.IEnumerator Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Seed pitch from authored pose
        float pitch = transform.localEulerAngles.x; if (pitch > 180f) pitch -= 360f;
        xRotation = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        initialLocalPosition = transform.localPosition;
        playerController = target ? target.GetComponent<ZhouSoftware.PlayerController>() : null;

        // Wait one frame so any other Start() that touches rotation runs first
        yield return null;
        readyForLook = true;
        ignoreNextDelta = true; // swallow first spike after lock/enable
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus && Cursor.lockState == CursorLockMode.Locked)
            ignoreNextDelta = true;
    }

    void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        if (!readyForLook || Cursor.lockState != CursorLockMode.Locked) return;
        if (ignoreNextDelta) { ignoreNextDelta = false; return; }
        lookInput = ctx.ReadValue<Vector2>();
        isRotating = true;
    }

    void OnLookCanceled(InputAction.CallbackContext _)
    {
        isRotating = false;
        lookInput = Vector2.zero;
    }

    void Update()
    {
        HandleCursorLock();
        HandleRotation();
        HandleIdleSway();
    }

    void HandleCursorLock()
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
            ignoreNextDelta = true;
        }
    }

    void HandleRotation()
    {
        if (!isRotating || target == null) return;

        // If your action is raw pixel delta, keeping Time.deltaTime is fine (tune speed).
        float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
        float mouseY = lookInput.y * rotationSpeed * Time.deltaTime;

        // yaw on player root
        target.Rotate(Vector3.up * mouseX, Space.World);

        // pitch on camera (local)
        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleIdleSway()
    {
        if (!playerController) return;

        swayTimer += Time.deltaTime * (playerController.IsMoving ? movementSwaySpeed : idleSwaySpeed);
        float amt  = playerController.IsMoving ? movementSwayAmount : idleSwayAmount;

        float swayX = Mathf.Sin(swayTimer) * amt;
        float swayY = Mathf.Cos(swayTimer * 2f) * amt * 0.5f;

        transform.localPosition = initialLocalPosition + new Vector3(swayX, swayY, 0f);
    }
}
