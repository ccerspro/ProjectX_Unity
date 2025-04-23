using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Enemy AI: Shadow/LightWorld can track MainWorld players; MainWorld enemy can't see higher dimensions
/// </summary>
public class SampleAgentScript : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private Animator animator;

    [SerializeField] private float detectionRange = 10f;

    //private bool isAttacking = false;
    private bool hasLockedOn = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        TryFindTarget(); // Try finding target at start
    }

    void Update()
    {
        if (target == null)
        {
            TryFindTarget();
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

                if (agent.velocity.magnitude > 0.1f)
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
            agent.isStopped = true;
            animator.SetBool("IsMoving", false);
        }
    }

    void TryFindTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            target = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
    }

    
}
