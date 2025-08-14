using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Enemy AI: Shadow/LightWorld can track MainWorld players; MainWorld enemy can't see higher dimensions
/// </summary>

namespace ZhouSoftware
{


    public class SampleAgentScript : MonoBehaviour
    {
        private Transform target;
        private NavMeshAgent agent;
        private Animator animator;

        [SerializeField] private float detectionRange = 10f;

        //private bool isAttacking = false;
        private bool hasLockedOn = false;

        void OnEnable()
        {
            if (PlayerLocator.TryGet(out target) == false)
			{
				PlayerLocator.OnAvailable += HandlePlayerAvailable;
			}
        }
		
		void OnDisable()
		{
			PlayerLocator.OnAvailable -= HandlePlayerAvailable;
		}

		private void HandlePlayerAvailable(Transform t)
		{
			target = t;
			PlayerLocator.OnAvailable -= HandlePlayerAvailable; // one-shot
		}

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

        }

        void Update()
        {
            if (target == null)
            {
                agent.isStopped = true;
                animator.SetBool("IsMoving", false);
                hasLockedOn = false;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            // Lock on when within detection range
            if (distanceToPlayer <= detectionRange)
            {
                hasLockedOn = true;
            }

            if (hasLockedOn)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);

                if (agent.velocity.magnitude > 0.1f && animator != null)
                {
                    animator.SetBool("IsMoving", true);
                    animator.Play("Walk_IP");
                }
                else
                {
                    animator.SetBool("IsMoving", false);
                }

            }
            else
            {
                if (animator == null) return;
                agent.isStopped = true;
                animator.SetBool("IsMoving", false);
            }
        }

        

        void OnTriggerEnter(Collider other)
        {

        }


    }
}