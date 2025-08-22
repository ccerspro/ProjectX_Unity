using UnityEngine;
using UnityEngine.InputSystem;
using ZhouSoftware.Inventory;

namespace ZhouSoftware
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;   // drag same PlayerInput

        [Header("Movement")]
        public float normalSpeed = 5f;
        public float sprintSpeed = 10f;

        [Header("Equipment")]
        [SerializeField] private ItemDefinition flashlightItem; // drag flashlight item definition
        [SerializeField] private GameObject flashlight;


        public bool IsMoving { get; private set; }

        Rigidbody rb;
        Vector3 movementInput;
        float currentSpeed;

        // Actions
        InputAction moveAction, sprintAction, quitAction, resetAction, flashlightAction;

        void Awake()
        {
            if (!playerInput) playerInput = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true; // simple way to avoid physics tilting
        }

        void OnEnable()
        {
            // Register globally if you use PlayerLocator elsewhere
            PlayerLocator.Set(transform);

            var a = playerInput.actions;
            moveAction       = a.FindAction("Move",       true);
            sprintAction     = a.FindAction("Sprint",     true);
            quitAction       = a.FindAction("Quit",       true);
            resetAction      = a.FindAction("Reset",      true);
            flashlightAction = a.FindAction("FlashLight", true);

            sprintAction.performed += OnSprint;
            sprintAction.canceled  += OnSprintRelease;
            quitAction.performed   += OnQuit;
            resetAction.performed  += OnReset;
            flashlightAction.performed += OnFlashLight;

            currentSpeed = normalSpeed;
        }

        void OnDisable()
        {
            if (PlayerLocator.Player == transform) PlayerLocator.Clear();
            sprintAction.performed -= OnSprint;
            sprintAction.canceled  -= OnSprintRelease;
            quitAction.performed   -= OnQuit;
            resetAction.performed  -= OnReset;
            flashlightAction.performed -= OnFlashLight;
        }

        void Update()
        {
            Vector2 mv = moveAction.ReadValue<Vector2>();
            movementInput = new Vector3(mv.x, 0f, mv.y).normalized;

            IsMoving = movementInput.sqrMagnitude > 0.01f;
            HandleFootsteps();
            CheckForFall();
        }

        void FixedUpdate()
        {
            // velocity-based movement (keeps gravity)
            Vector3 vel = transform.TransformDirection(movementInput) * currentSpeed;
            rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);
        }

        void OnSprint(InputAction.CallbackContext _)        => currentSpeed = sprintSpeed;
        void OnSprintRelease(InputAction.CallbackContext _) => currentSpeed = normalSpeed;

        void OnFlashLight(InputAction.CallbackContext _)
        {
            if (!PlayerInventory.I || !flashlight || !flashlightItem || !PlayerInventory.I.Has(flashlightItem)) return;
            bool on = !flashlight.activeSelf;
            flashlight.SetActive(on);
            AudioManager.I.PlaySFX("flashlight_toggle", transform.position);
        }

        void OnQuit(InputAction.CallbackContext _)
        {
            Debug.Log("Quitting the game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        void OnReset(InputAction.CallbackContext _)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(s.name);
        }

        void HandleFootsteps()
        {
            if (IsMoving) { if (!AudioManager.I.IsSFXPlaying("footstep_wood")) AudioManager.I.PlaySFX("footstep_wood"); }
            else          { AudioManager.I.StopSFX("footstep_wood"); }
        }

        void CheckForFall()
        {
            if (transform.position.y < -10f)
            {
                var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.LoadScene(s.name);
            }
        }
    }
}
