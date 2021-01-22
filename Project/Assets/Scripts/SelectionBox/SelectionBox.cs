using System.Collections.Generic;
using UnityEngine;

public class SelectionBox : MonoBehaviour
{
    private Vector3 m_StartLocation;
    private Vector3 m_EndLocation;
    public List<GameObject> m_OverlappingObjects = new List<GameObject>();


    // -------------------------
    // TRIGGERS
    // -------------------------

    // Add object to list on enter
    private void OnTriggerEnter(Collider other)
    {
        m_OverlappingObjects.Add(other.gameObject);
    }

    // Remove object from list on exit
    private void OnTriggerExit(Collider other)
    {
        m_OverlappingObjects.Remove(other.gameObject);
    }


    // -------------------------
    // GETTERS & SETTERS
    // -------------------------

    // Return current list of overlapping objects
    public List<GameObject> GetOverlappingObjects()
    {
        return m_OverlappingObjects;
    }

    // Set start draw location
    public void SetStartLocation(Vector3 startLocation)
    {
        m_StartLocation = startLocation;
    }

    // Set end draw location, and update transform
    public void SetEndLocation(Vector3 endLocation)
    {
        m_EndLocation = endLocation;
        UpdateTransform();
    }

    // Update transform to match the draw locations
    private void UpdateTransform()
    {
        // Calculate new size to adjust position
        Vector3 size = m_EndLocation - m_StartLocation;
        size.y = 1; // Y scale has to always be at least 1

        // Object pivot is center of the plane, have to adjust position relative to it
        transform.position = new Vector3(m_StartLocation.x + size.x / 2f, 0f, m_StartLocation.z + size.z / 2f);
        transform.localScale = size; // Set the scale
    }
}
