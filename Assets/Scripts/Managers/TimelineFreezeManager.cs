using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

namespace ZhouSoftware
{
    public class TimelineFreezeManager : MonoBehaviour
    {
        [Header("References")]
        public PlayableDirector director;               // the director that plays your cutscene
        public PlayerInput playerInput;                 // player's PlayerInput
        public MonoBehaviour[] componentsToDisable;     // e.g., PlayerController, CameraController
        public Rigidbody playerRigidbody;               // optional

        [Header("Behavior")]
        [Tooltip("If true, keep UI working (switch Player->UI map). If false, all input is off.")]
        public bool allowUI = false;

        [Tooltip("If true, pause Time.timeScale during cutscene. Timeline runs unscaled.")]
        public bool freezeWorld = false;

        bool locked;
        float prevTimeScale;
        DirectorUpdateMode prevMode;
        bool prevKinematic;

        void Awake()
        {
            if (!director) director = GetComponent<PlayableDirector>();
            director.played  += OnPlayed;
            director.stopped += OnStopped;
            director.paused  += OnPaused; // for WrapMode = Hold
        }

        void OnDestroy()
        {
            if (!director) return;
            director.played  -= OnPlayed;
            director.stopped -= OnStopped;
            director.paused  -= OnPaused;
        }

        void Start()
        {
            // If the director already started before we subscribed (Play On Awake), catch up
            if (director && director.state == PlayState.Playing) OnPlayed(director);
        }

        void OnPlayed(PlayableDirector d)
        {
            if (locked) return;
            locked = true;

            if (freezeWorld)
            {
                prevMode = director.timeUpdateMode;
                director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
                prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            foreach (var m in componentsToDisable) if (m) m.enabled = false;

            if (playerRigidbody)
            {
                prevKinematic = playerRigidbody.isKinematic;
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = true;
            }

            if (playerInput)
            {
                if (allowUI) playerInput.SwitchCurrentActionMap("UI");
                else         playerInput.DeactivateInput();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        void OnPaused(PlayableDirector d)
        {
            // End reached with WrapMode.Hold appears as paused
            if (d.time >= d.duration - 0.0001f) Unlock();
        }

        void OnStopped(PlayableDirector d) => Unlock();

        void Unlock()
        {
            if (!locked) return;

            if (playerRigidbody) playerRigidbody.isKinematic = prevKinematic;

            foreach (var m in componentsToDisable) if (m) m.enabled = true;

            if (playerInput)
            {
                if (allowUI) playerInput.SwitchCurrentActionMap("Player");
                else         playerInput.ActivateInput();
            }

            if (freezeWorld)
            {
                director.timeUpdateMode = prevMode;
                Time.timeScale = prevTimeScale;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;

            locked = false;
        }
    }
}
