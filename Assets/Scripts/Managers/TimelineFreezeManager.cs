using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

namespace ZhouSoftware
{
    public class TimelineFreezeManager : MonoBehaviour
    {
        [Header("References")]
        public PlayableDirector playableDirector;

        [Tooltip("Gameplay scripts to disable during the cutscene (e.g., PlayerController, CameraController).")]
        public MonoBehaviour[] componentsToDisable;

        [Tooltip("Player Rigidbody (optional, set kinematic during cutscene).")]
        public Rigidbody playerRigidbody;

        [Tooltip("PlayerInput component (new Input System). We will Deactivate/Activate input here.")]
        public PlayerInput playerInput;

        [SerializeField] private ZhouSoftware.PlayerController playerController;

        [SerializeField] private CameraController cameraController;

        [Header("Options")]
        [Tooltip("If true, pause Time.timeScale during the cutscene (world freezes). The Timeline will run in UnscaledGameTime.")]
        public bool freezeWorld = false;

        bool locked;
        float prevTimeScale;
        DirectorUpdateMode prevDirectorUpdateMode;
        bool prevIsKinematic;
        Vector3 savedVel, savedAngVel;

        void OnEnable()
        {
            if (playableDirector)
            {
                playableDirector.played += OnTimelinePlayed;
                playableDirector.stopped += OnTimelineStopped;
            }
        }

        void OnDisable()
        {
            
            if (playableDirector)
            {
                playableDirector.played -= OnTimelinePlayed;
                playableDirector.stopped -= OnTimelineStopped;
            }
            if (locked) Unlock();
        }

        void OnTimelinePlayed(PlayableDirector d)
        {
            Debug.Log("Timeline started, freezing gameplay...");
            if (locked) return;
            locked = true;

            // Optionally pause the world, but keep Timeline running
            if (freezeWorld)
            {
                prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;

                prevDirectorUpdateMode = playableDirector.timeUpdateMode;
                playableDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            }

            if (playerController) playerController.enabled = false;
            if (cameraController) cameraController.enabled = false;

            // Disable gameplay components (movement, camera controller, etc.)
            foreach (var c in componentsToDisable)
                if (c) c.enabled = false;

            // Freeze physics on the player so animation/tracks don't fight it
            if (playerRigidbody)
            {
                prevIsKinematic = playerRigidbody.isKinematic;
                savedVel    = playerRigidbody.velocity;
                savedAngVel = playerRigidbody.angularVelocity;

                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.isKinematic = true;
            }

            // Turn off input (new Input System)
            if (playerInput) playerInput.DeactivateInput();
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        void OnTimelineStopped(PlayableDirector d) => Unlock();

        void Unlock()
        {
            Debug.Log("Timeline stopped, restoring gameplay...");
            // Restore timescale / director mode
            if (freezeWorld)
            {
                playableDirector.timeUpdateMode = prevDirectorUpdateMode;
                Time.timeScale = prevTimeScale;
            }

            // Restore physics
            if (playerRigidbody)
            {
                playerRigidbody.isKinematic = prevIsKinematic;
                playerRigidbody.velocity    = Vector3.zero;      // usually better than restoring old vel
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            if (playerController) playerController.enabled = true;
            if (cameraController) cameraController.enabled = true;

            // Re-enable gameplay
            foreach (var c in componentsToDisable)
                if (c) c.enabled = true;

            if (playerInput) playerInput.ActivateInput();
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;

            locked = false;
        }
    }
}
