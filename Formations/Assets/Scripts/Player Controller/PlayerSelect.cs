using System.Collections.Generic;
using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    // Data
    private PlayerData m_Data = null;

    [SerializeField] private GameObject m_SelectionBoxPrefab = null;

    private SelectionBox m_SelectionBox = null;
    private bool m_Selecting = false;
    private bool m_Adding = false;

    private void Start()
    {
        m_Data = GetComponent<PlayerData>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (m_SelectionBox == null)
            {
                // Check if we are adding to our current selection
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    m_Adding = true;
                }

                // If no selectionbox exists, create one
                GameObject go = Instantiate(m_SelectionBoxPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
                m_SelectionBox = go.GetComponent<SelectionBox>();

                // Get mouse location
                Vector3 mouseLocation = Input.mousePosition;
                mouseLocation.z = Camera.main.transform.position.y;

                // Convert mouse location to world space
                Vector3 worldLocation = Camera.main.ScreenToWorldPoint(mouseLocation);

                // Set selectionbox boundaries to mouse position
                m_SelectionBox.SetStartLocation(worldLocation);
                m_SelectionBox.SetEndLocation(worldLocation);
            }
            else
            {
                // Check if player hasn't released control
                if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    m_Adding = false;
                }

                // Get mouse location
                Vector3 mouseLocation = Input.mousePosition;
                mouseLocation.z = Camera.main.transform.position.y;

                // Convert mouse location to world space
                Vector3 worldLocation = Camera.main.ScreenToWorldPoint(mouseLocation);

                // Update end location, which will update selectionbox boundaries
                m_SelectionBox.SetEndLocation(worldLocation);
            }

            m_Selecting = true;
        }
        else
        {
            if (m_Selecting)
            {
                // If not adding to our current selection, clear it
                if (!m_Adding)
                {
                    ClearSelection();
                }

                // Select units, and stop selecting
                SelectUnits();
                m_Selecting = false;
                m_Adding = false;
            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            ClearSelection();
        }
    }

    // Select units inside selection box
    private void SelectUnits()
    {
        // Only execute if we have a box reference
        if (m_SelectionBox == null)
            return;

        // Get all overlapping objects
        List<GameObject> overlappingObjects = m_SelectionBox.GetOverlappingObjects();

        // Loop over all objects
        foreach (GameObject go in overlappingObjects)
        {
            int goID = go.GetInstanceID();

            // If we are adding to our selection
            if (m_Adding)
            {
                // Check for duplicates in our list
                bool duplicate = false;
                foreach (KeyValuePair<int, UnitBehavior> selectedUnit in m_Data.selectedUnits)
                {
                    if (selectedUnit.Key == goID)
                    {
                        duplicate = true;
                        break;
                    }
                }

                // Don't add this unit, when the ID is already present
                if (duplicate)
                    continue;
            }

            // Check if they have behavior script, and add units with their ID to our list
            UnitBehavior goUnit = go.GetComponent<UnitBehavior>();
            if (goUnit != null)
            {
                m_Data.selectedUnits.Add(new KeyValuePair<int, UnitBehavior>(goID, goUnit));
                goUnit.Select();
            }
        }

        // Destroy the box and reset reference
        Destroy(m_SelectionBox.gameObject);
        m_SelectionBox = null;
    }

    // Clear our selected units
    private void ClearSelection()
    {
        // Only execute if we have units
        if (m_Data.selectedUnits.Count == 0)
            return;

        // Loop over all units
        foreach (KeyValuePair<int, UnitBehavior> unit in m_Data.selectedUnits)
        {
            unit.Value.Deselect();
        }

        // Clear the list of units
        m_Data.selectedUnits.Clear();
    }
}
