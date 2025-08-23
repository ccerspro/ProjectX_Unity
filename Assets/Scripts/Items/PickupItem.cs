using UnityEngine;
using UnityEngine.InputSystem; // optional (Mouse.current check)
using ZhouSoftware.Inventory; // ItemDefinition, PlayerInventory
using ZhouSoftware;
using UnityEngine.TextCore.Text;           // PlayerLocator

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    [Header("Item")]
    public ItemDefinition item;
    public int amount = 1;

    [Header("Interaction")]
    [Tooltip("How close the player must be to pick up (u = meters if 1u=1m).")]
    public float pickupRange = 2.0f;
    [Tooltip("Require left click while hovering? If off, auto-pick when hovering in range.")]
    public bool requireClick = true;

    [Header("FX")]
    public AudioSource sfxOnPickup;
    public bool destroyOnPickup = true;

    Transform player;


    void OnEnable()
    {
        // Grab player now or when it becomes available
        if (!PlayerLocator.TryGet(out player))
            PlayerLocator.OnAvailable += HandlePlayerAvailable;
    }
    void OnDisable()
    {
        PlayerLocator.OnAvailable -= HandlePlayerAvailable;
    }
    void HandlePlayerAvailable(Transform t)
    {
        player = t;
        PlayerLocator.OnAvailable -= HandlePlayerAvailable;
    }

    // Called every frame while the mouse hovers this object's collider
    void OnMouseOver()
    {
        AimManager.I?.SetFocusedAim();
        if (!player || !item) return;

        // Optional: only allow pickup while in gameplay (cursor locked)
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Range check (fast sqr distance)
        if ((player.position - transform.position).sqrMagnitude > pickupRange * pickupRange) return;

        // Require a click, or auto-pick if not required
        bool clicked =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            Input.GetMouseButtonDown(0);

        if (!requireClick || clicked)
            TryGiveTo(player);
    }

    void OnMouseExit()
    {
        AimManager.I?.SetDefaultAim();
    }

    void TryGiveTo(Transform who)
    {
        if (!PlayerInventory.I) return;

        if (PlayerInventory.I.Add(item, Mathf.Max(1, amount)))
        {
            if (sfxOnPickup) sfxOnPickup.Play();
            if (destroyOnPickup) Destroy(gameObject);
            else gameObject.SetActive(false);
            AimManager.I?.SetDefaultAim();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.75f);
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
#endif
}
