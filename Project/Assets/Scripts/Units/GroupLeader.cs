using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class GroupLeader : MonoBehaviour
{
    // Group info
    public int groupID = -1;
    public List<UnitBehavior> units = new List<UnitBehavior>();

    // Formations
    [SerializeField] private GameObject m_FormationPointPrefab = null;
    private List<Transform> m_FormationTransforms = new List<Transform>();

    // Navigation
    [SerializeField] private float m_SpeedAmplifier = 1.3f;
    [SerializeField] private float m_StopDistance = 0.2f;
    [SerializeField] private float m_MaxAngularSpeed = 60f;
    private Rigidbody m_Rigidbody;
    private NavMeshPath m_Path;
    private Vector3 m_Target;
    private Vector3 m_TargetDirection;
    private float m_MaxSpeed = 0f;
    private float m_Speed = 0f;
    private float m_AngularSpeed = 0f;

    private void Start()
    {
        // Initialize navigation data
        m_Path = new NavMeshPath();
        m_Target = transform.position;
        m_TargetDirection = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // If we are further away than stop distance
        float targetDistance = Vector3.Distance(transform.position, m_Target);
        if (targetDistance > m_StopDistance)
        {
            // Recalculate path
            NavMesh.CalculatePath(transform.position, m_Target, NavMesh.AllAreas, m_Path);

            // If we have a path
            if (m_Path.corners.Length > 1)
            {
                m_TargetDirection = (m_Path.corners[1] - transform.position).normalized;
                m_Speed = m_MaxSpeed;
            }
        }
        else
        {
            // Reset speed and angular velocity
            m_Speed = 0f;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }

        // If we have a speed
        if (m_Speed != 0f)
        {
            float rotation = Vector3.Cross(m_TargetDirection, transform.forward).y;
            m_Rigidbody.angularVelocity = new Vector3(0f, rotation, 0f) * -m_AngularSpeed;

            // Update position using speed
            transform.position += (m_Speed * transform.forward * Time.fixedDeltaTime);
        }

        // Move the units
        MoveUnits();
    }

    public void SetLocation(Vector3 location)
    {
        transform.position = location;
        m_Target = location;
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

        // Speed values
        float slowestSpeed = Mathf.Infinity;

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

            // Save the lowest speed
            float unitSpeed = units[index].GetNormalSpeed();
            if (unitSpeed < slowestSpeed)
            {
                slowestSpeed = unitSpeed;
            }

            // Update current position
            currentPosition.x += unitWidth;
        }

        // Apply speed to group, set all units to match this speed increased with amplifier
        m_MaxSpeed = slowestSpeed;
        foreach (UnitBehavior unit in units)
        {
            unit.SetMinimumSpeed(slowestSpeed * m_SpeedAmplifier);
        }

        // Set angular speed
        m_AngularSpeed = m_MaxAngularSpeed / amountUnits;
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
