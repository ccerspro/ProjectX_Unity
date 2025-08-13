using System.Collections;
using UnityEngine;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviour
    {
        public bool open;
        public float smooth = 1.0f;
        private float DoorOpenAngle = -90.0f;
        private float DoorCloseAngle = 0.0f;

        public AudioSource asource;
        public AudioClip openDoor, closeDoor;

        private Coroutine autoCloseCoroutine;

        void Start()
        {
            asource = GetComponent<AudioSource>();
        }

        void Update()
        {
            Quaternion targetRotation = open
                ? Quaternion.Euler(0, DoorOpenAngle, 0)
                : Quaternion.Euler(0, DoorCloseAngle, 0);

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * 5 * smooth
            );
        }

        public void OpenDoor()
        {
            // Toggle door state
            open = !open;

            // Play correct sound
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();

            if (open)
            {
                // Start auto-close
                if (autoCloseCoroutine != null)
                    StopCoroutine(autoCloseCoroutine);

                autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay(3f));
            }
            else
            {
                // Stop any running auto-close if manually closed
                if (autoCloseCoroutine != null)
                {
                    StopCoroutine(autoCloseCoroutine);
                    autoCloseCoroutine = null;
                }
            }
        }

        private IEnumerator AutoCloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Ensure door is still open before closing
            if (open)
            {
                OpenDoor(); // This toggles to close and keeps everything synced
            }
        }
    }
}
