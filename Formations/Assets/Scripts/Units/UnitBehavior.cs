using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnitBehavior : MonoBehaviour
{
    // Visuals
    [SerializeField] private SkinnedMeshRenderer m_Mesh = null;
    [SerializeField] private Material m_StandardMaterial = null;
    [SerializeField] private Material m_SelectedMaterial = null;

    // Rigidbody
    const int m_Weight = 70;
    private Rigidbody m_Rigidbody = null;

    // Movement
    private Vector3 m_Target;
    private bool m_OnTarget = true;
    public float maxLinearVelocity { get; private set; } = 3f; // length of velocity vector

    private void Start()
    {
        // Change material instance
        m_Mesh.material = m_StandardMaterial;

        // Set rigidbody reference, and adjust weight + restraints
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.mass = m_Weight;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Code to execute when selecting unit
    public void Select()
    {
        // Update material
        m_Mesh.material = m_SelectedMaterial;
    }

    // Code to execute when deselecting unit
    public void Deselect()
    {
        // Update material
        m_Mesh.material = m_StandardMaterial;
    }

    private void FixedUpdate()
    {
        SeekTarget(); // seek towards target
        AutoOrient(); // rotate unit
    }

    // Auto orient based on rigidbody velocity
    private void AutoOrient()
    {
        // Only execute when moving
        if (m_OnTarget)
            return;

        // Get rotation in rad based on velocity
        float rotation = Mathf.Atan2(m_Rigidbody.velocity.z, m_Rigidbody.velocity.x);
        rotation = 90 - (rotation * Mathf.Rad2Deg); // subtract from 90, to get correct rotation and convert to degrees

        // Look in the current direction
        transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    // Set a new target
    public void SetTarget(Vector3 target)
    {
        // Set new target and on target false
        m_Target = target;
        m_OnTarget = false;
    }

    // Seek towards current target
    private void SeekTarget()
    {
        // Calculate direction
        Vector3 direction = m_Target - transform.position;
        float length = direction.magnitude;

        // Only execute if not yet on target
        if (m_OnTarget)
            return;

        // If not almost on target, set velocity
        if (length > 0.1f)
        {
            // Normalize direction and set velocity
            direction.Normalize();
            m_Rigidbody.velocity = direction * maxLinearVelocity;
        }
        else
        {
            // If close enough, we are on target
            m_OnTarget = true;
        }
    }
}
