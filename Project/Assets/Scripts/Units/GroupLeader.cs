using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GroupLeader : MonoBehaviour
{
    // Group info
    public int groupID = -1;
    public List<UnitBehavior> units { get; private set; } = new List<UnitBehavior>();

    // NavMeshAgent
    private NavMeshAgent m_NavMeshAgent = null;

    private void Start()
    {
        // Set NavMeshAgent reference
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void SetTransform(Vector3 location, Quaternion rotation)
    {
        transform.position = location;
        transform.rotation = rotation;
    }

    public void AddUnit(UnitBehavior unit)
    {
        units.Add(unit);
    }

    // Set a new target
    public void SetTarget(Vector3 target)
    {
        m_NavMeshAgent.SetDestination(target);
    }
}
