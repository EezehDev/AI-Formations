﻿using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitBehavior : MonoBehaviour
{
    // Visuals
    [SerializeField] private SkinnedMeshRenderer m_Mesh = null;
    [SerializeField] private Material m_StandardMaterial = null;
    [SerializeField] private Material m_SelectedMaterial = null;
    [SerializeField] private Animator m_Animator = null;

    // Rigidbody
    private Rigidbody m_Rigidbody = null;

    // NavMeshAgent
    private NavMeshAgent m_NavMeshAgent = null;

    private void Start()
    {
        // Change material instance
        m_Mesh.material = m_StandardMaterial;

        // Set Rigidbody reference
        m_Rigidbody = GetComponent<Rigidbody>();

        // Set NavMeshAgent reference
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
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
        // Set the animator velocity
        m_Animator.SetFloat("Velocity", m_NavMeshAgent.velocity.magnitude);
    }

    // Set a new target
    public void SetTarget(Vector3 target)
    {
        m_NavMeshAgent.SetDestination(target);
    }
}