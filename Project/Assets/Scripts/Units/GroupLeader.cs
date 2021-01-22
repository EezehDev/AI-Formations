using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class GroupLeader : MonoBehaviour
{
    // Group info
    [Header("Group Info")]
    public int groupID = -1;
    public List<UnitBehavior> units = new List<UnitBehavior>();
    const float unitWidth = 0.5f;

    // Formations
    [Header("Formation")]
    [SerializeField] private GameObject m_FormationPointPrefab = null;
    private List<Transform> m_FormationTransforms = new List<Transform>();
    private FormationType m_CurrentFormation;
    private bool m_Wheeling;

    // All formation types
    private enum FormationType
    {
        line,
        circle
    };

    // Navigation
    [Header("Navigation")]
    [SerializeField] private float m_SpeedAmplifier = 1.3f;
    [SerializeField] private float m_StopDistance = 0.2f;
    [SerializeField] private float m_MaxAngularSpeed = 60f;
    [SerializeField] private float m_MinAngularSpeed = 15f;
    private Rigidbody m_Rigidbody;
    private NavMeshPath m_Path;
    private Vector3 m_Target;
    private Vector3 m_TargetDirection;
    private float m_MaxSpeed = 0f;
    private float m_Speed = 0f;
    private float m_AngularSpeed = 0f;


    // -------------------------
    // INIT AND UPDATE
    // -------------------------

    // Initialise path and target
    private void Start()
    {
        // Initialize formation data
        m_CurrentFormation = FormationType.line;
        m_Wheeling = true;

        // Initialize navigation data
        m_Path = new NavMeshPath();
        m_Target = transform.position;
        m_TargetDirection = transform.position;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Update group path and move units
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
            if (m_Wheeling)
            {
                float rotation = Vector3.Cross(m_TargetDirection, transform.forward).y;
                m_Rigidbody.angularVelocity = new Vector3(0f, rotation, 0f) * -m_AngularSpeed;

                // Update position using speed
                transform.position += (m_Speed * transform.forward * Time.fixedDeltaTime);
            }
            else
            {
                if (m_Speed != 0f)
                {
                    // Update position using speed
                    transform.position += (m_Speed * m_TargetDirection * Time.fixedDeltaTime);
                }
            }
        }

        // Move the units
        MoveUnits();
    }


    // -------------------------
    // LEADER AND GROUP COMMANDS
    // -------------------------

    // Sets a new leader location
    public void SetLocation(Vector3 location)
    {
        transform.position = location;
        m_Target = location;
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


    // -------------------------
    // FORMATION METHODS
    // -------------------------

    public void NextFormation()
    {
        // Cycle formation
        switch (m_CurrentFormation)
        {
            case FormationType.line:
                m_CurrentFormation = FormationType.circle;
                break;
            case FormationType.circle:
                m_CurrentFormation = FormationType.line;
                break;
        }

        // Create the new formation
        CreateFormation();
    }

    public void CreateFormation()
    {
        RemoveExcessTransformations(); // remove points

        // Create the current active formation
        switch (m_CurrentFormation)
        {
            case FormationType.line:
                CreateLineFormation(); // create line formation
                break;
            case FormationType.circle:
                CreateCircleFormation(); // create line formation
                break;
        }

        UpdateGroupSpeed(); // update group speed
    }

    // Removes unecessary transformation points
    private void RemoveExcessTransformations()
    {
        // If we have more transforms than needed, remove last ones
        if (m_FormationTransforms.Count > units.Count)
        {
            for (int index = m_FormationTransforms.Count; index < units.Count; index++)
            {
                Destroy(m_FormationTransforms[index].gameObject);
                m_FormationTransforms.RemoveAt(index);
            }
        }
    }

    // Updates the unit speed, and sets angular/linear group speed
    private void UpdateGroupSpeed()
    {
        float slowestSpeed = Mathf.Infinity;
        foreach (UnitBehavior unit in units)
        {
            // Save the lowest speed
            float unitSpeed = unit.GetNormalSpeed();
            if (unitSpeed < slowestSpeed)
            {
                slowestSpeed = unitSpeed;
            }
        }

        // Apply speed to group, set all units to match this speed increased with amplifier
        m_MaxSpeed = slowestSpeed;
        foreach (UnitBehavior unit in units)
        {
            unit.SetMinimumSpeed(slowestSpeed * m_SpeedAmplifier);
        }

        // Set angular speed
        m_AngularSpeed = m_MaxAngularSpeed / units.Count;
        m_AngularSpeed = Mathf.Clamp(m_AngularSpeed, m_MinAngularSpeed, m_MaxAngularSpeed);
    }


    // -------------------------
    // DIFFERENT FORMATIONS
    // -------------------------

    // Creates a line formation, using the leader as center
    private void CreateLineFormation()
    {
        // Wheel true, as we need to rotate when changing direction
        m_Wheeling = true;

        // Store unit count
        int amountUnits = units.Count;

        // Multiple rows
        int minimumRowSize = 5;
        int maximumRows = 3;

        // Calculate rows
        int rows = 1;
        if (amountUnits > minimumRowSize)
            rows = ((amountUnits - 1) / minimumRowSize) + 1;

        if (rows > maximumRows)
            rows = maximumRows;

        // Define units per row
        int unitsPerRow = ((amountUnits - 1) / rows) + 1;

        // Set start position
        Vector2 bottomLeft = new Vector2(-unitWidth * (unitsPerRow / 2f) + (unitWidth / 2f), unitWidth * (rows / 2f) - (unitWidth / 2f));

        Vector3 currentPosition = Vector3.zero;
        currentPosition.x = bottomLeft.x;
        currentPosition.z = bottomLeft.y;

        // Loop to create formation
        int currentRow = 1;
        for (int index = 0; index < amountUnits; index++)
        {
            // Check if last row has a different size than other rows
            if ((rows > 1) && (currentRow == rows) && (index % unitsPerRow == 0))
            {
                int unitsLastRow = amountUnits - (unitsPerRow * (rows - 1));

                int unitDifference = unitsPerRow - unitsLastRow;
                if (unitDifference > 0)
                {
                    currentPosition.x += unitWidth * (unitDifference / 2f);
                }
            }

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

            if (((index + 1) % unitsPerRow) == 0)
            {
                // Update Z position and reset X
                currentPosition.z -= unitWidth;
                currentPosition.x = bottomLeft.x;
                currentRow++;
            }
            else
            {
                // Update X position
                currentPosition.x += unitWidth;
            }
        }
    }

    // Creates a circle formation, with a gap in the center
    private void CreateCircleFormation()
    {
        // A circle does not need to rotate, wheeling false
        m_Wheeling = false;

        // Store unit count and angle
        int amountUnits = units.Count;
        float anglePerUnit = (Mathf.PI * 2f) / amountUnits;
        float radius = amountUnits * (unitWidth / 5f);

        // Loop to create formation
        for (int index = 0; index < amountUnits; index++)
        {
            Vector3 position = new Vector3(Mathf.Cos(anglePerUnit * index), 0f, Mathf.Sin(anglePerUnit * index)) * radius;

            // If we have transform already, set new position
            if (m_FormationTransforms.Count > index)
            {
                m_FormationTransforms[index].localPosition = position;
            }
            else
            {
                // Instantiate a point parented to the leader, with a relative position
                GameObject go = Instantiate(m_FormationPointPrefab, transform);
                go.transform.localPosition = position;
                m_FormationTransforms.Add(go.transform);
            }
        }
    }
}