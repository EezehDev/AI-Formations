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
    public float maxAngularSpeed { get; private set; } = 90f; // in degrees, converted to rad on start

    private void Start()
    {
        // Change material instance
        m_Mesh.material = m_StandardMaterial;

        // Set rigidbody reference, and adjust weight + restraints
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.mass = m_Weight;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Convert angular speed from degrees to radians
        maxAngularSpeed = maxAngularSpeed * Mathf.Deg2Rad;
    }

    // Code to execute when selecting unit
    public void Select()
    {
        m_Mesh.material = m_SelectedMaterial;
    }

    // Code to execute when deselecting unit
    public void Deselect()
    {
        m_Mesh.material = m_StandardMaterial;
    }

    private void FixedUpdate()
    {
        SeekTarget();
    }

    public void SetTarget(Vector3 target)
    {
        m_Target = target;
        m_OnTarget = false;
    }

    private void SeekTarget()
    {
        Vector3 direction = m_Target - transform.position;
        float length = direction.magnitude;

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
            m_OnTarget = true;
        }

        transform.LookAt(m_Target);
    }
}
