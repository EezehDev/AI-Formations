using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer m_Mesh = null;
    [SerializeField] private Material m_StandardMaterial = null;
    [SerializeField] private Material m_SelectedMaterial = null;

    private void Start()
    {
        m_Mesh.material = m_StandardMaterial;
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
}
