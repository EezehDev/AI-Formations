using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitBehavior : MonoBehaviour
{
    // Visuals
    [SerializeField] private SkinnedMeshRenderer m_Mesh = null;
    [SerializeField] private GameObject m_SelectionCircle = null;
    [SerializeField] private Material m_StandardMaterial = null;

    // Animation
    [SerializeField] private Animator m_Animator = null;

    // Rigidbody
    private Rigidbody m_Rigidbody = null;

    // NavMeshAgent
    private NavMeshAgent m_NavMeshAgent = null;
    private float m_NormalSpeed = 0;

    // Group
    private GroupLeader m_Leader = null;

    private void Start()
    {
        // Change material instance
        m_Mesh.material = m_StandardMaterial;

        // Set Rigidbody reference
        m_Rigidbody = GetComponent<Rigidbody>();

        // Set NavMeshAgent reference
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_NormalSpeed = m_NavMeshAgent.speed;
    }

    // Code to execute when selecting unit
    public void Select()
    {
        // Display the selection circle
        m_SelectionCircle.SetActive(true);
    }

    // Code to execute when deselecting unit
    public void Deselect()
    {
        // Hide the selection circle
        m_SelectionCircle.SetActive(false);
    }

    private void FixedUpdate()
    {
        // Set the animator velocity
        m_Animator.SetFloat("Velocity", m_NavMeshAgent.velocity.magnitude);
    }

    // Set a new target
    public void SetTarget(Vector3 target)
    {
        m_NavMeshAgent.SetDestination(target);
    }

    // Get the units speed
    public float GetNormalSpeed()
    {
        return m_NormalSpeed;
    }

    // If our speed is lower than given speed, set new speed
    public void SetMinimumSpeed(float speed)
    {
        if (m_NavMeshAgent.speed < speed)
        {
            m_NavMeshAgent.speed = speed;
        }
    }

    // Get current leader
    public GroupLeader GetLeader()
    {
        return m_Leader;
    }

    // Set new leader
    public void SetLeader(GroupLeader leader)
    {
        // If we remove leader, set speed back to normal
        if (leader == null)
        {
            m_NavMeshAgent.speed = m_NormalSpeed;
        }

        m_Leader = leader;
    }

    // Change mesh material
    public void SetMaterial(Material material = null)
    {
        if (material == null)
        {
            m_Mesh.material = m_StandardMaterial;
        }
        else
        {
            m_Mesh.material = material;
        }
    }
}
