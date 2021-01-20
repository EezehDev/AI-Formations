using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroupLeader : MonoBehaviour
{
    // Group info
    public int groupID = -1;
    public List<UnitBehavior> units = new List<UnitBehavior>();

    // Formations
    [SerializeField] private GameObject m_FormationPointPrefab = null;
    private List<Transform> m_FormationTransforms = new List<Transform>();

    // Navigation
    [SerializeField] private float m_Speed = 3f;
    [SerializeField] private float m_StopDistance = 0.1f;
    [SerializeField] private float m_MinSpeed = 0.1f;
    private NavMeshPath m_Path;
    private Vector3 m_Target;
    private Vector3 m_Velocity;

    private void Start()
    {
        // Initialize navigation data
        m_Path = new NavMeshPath();
        m_Target = transform.position;
        m_Velocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        // If we are further away than stop distance
        if (Vector3.Distance(transform.position, m_Target) > m_StopDistance)
        {
            // Recalculate path
            NavMesh.CalculatePath(transform.position, m_Target, NavMesh.AllAreas, m_Path);

            // If we have a path
            if (m_Path.corners.Length > 1)
            {
                Vector3 direction = (m_Path.corners[1] - transform.position).normalized;
                m_Velocity = direction * m_Speed;
            }
        }
        else
        {
            // Reset velocity
            m_Velocity = Vector3.zero;
        }

        // If we have a velocity
        if (m_Velocity != Vector3.zero)
        {
            // Update position using velocity
            transform.position += (m_Velocity * Time.fixedDeltaTime);

            // Update rotation using velocity
            float rotation = 90f - (Mathf.Rad2Deg * Mathf.Atan2(m_Velocity.z, m_Velocity.x));
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        // Move the units
        MoveUnits();
    }

    public void SetTransform(Vector3 location, Quaternion rotation)
    {
        transform.position = location;
        transform.rotation = rotation;
    }

    public void CreateFormation()
    {
        // Initialise values
        int amountUnits = units.Count;
        float unitWidth = 0.5f;

        // Set start position
        Vector3 currentPosition = Vector3.zero;
        currentPosition.x = -unitWidth * (amountUnits / 2f) + (unitWidth / 2f);

        // If we have more transforms than needed, remove last ones
        if (m_FormationTransforms.Count > amountUnits)
        {
            for (int index = m_FormationTransforms.Count; index < amountUnits; index++)
            {
                Destroy(m_FormationTransforms[index].gameObject);
                m_FormationTransforms.RemoveAt(index);
            }
        }

        // Loop for each unit
        for (int index = 0; index < amountUnits; index++)
        {
            // If we have transform already, set new position
            if (m_FormationTransforms.Count > index)
            {
                m_FormationTransforms[index].localPosition = currentPosition;
            }
            else
            {
                // Instantiate a point parented to the leader, with a relative position
                GameObject go = Instantiate(m_FormationPointPrefab, transform);
                go.transform.localPosition = currentPosition;
                m_FormationTransforms.Add(go.transform);
            }

            // Update current position
            currentPosition.x += unitWidth;
        }
    }

    // Set a new target
    public void SetTarget(Vector3 target)
    {
        m_Target = target;
    }

    // Move units towards formation position
    private void MoveUnits()
    {
        for (int index = 0; index < units.Count; index++)
        {
            units[index].SetTarget(m_FormationTransforms[index].position);
        }
    }
}
