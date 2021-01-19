using System.Collections.Generic;
using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    // Data
    private PlayerData m_Data = null;

    // Selection
    [SerializeField] private GameObject m_SelectionBoxPrefab = null;
    private SelectionBox m_SelectionBox = null;
    private bool m_Selecting = false;
    private bool m_Adding = false;

    // Grouping
    [SerializeField] private GameObject m_GroupLeaderPrefab = null;

    private void Start()
    {
        m_Data = GetComponent<PlayerData>();
    }

    private void Update()
    {
        // Get left mouse button input
        if (Input.GetMouseButton(0))
        {
            // See if we have a selection box present
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

            // Set selecting to true (holding down left click)
            m_Selecting = true;
        }
        else
        {
            // If we just release our left click
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

        // Upon pressing escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }

        // Upon pressing G
        if (Input.GetKeyDown(KeyCode.G))
        {
            GroupSelection();
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
                // Get the unit's leader
                GroupLeader leader = goUnit.GetLeader();

                // If unit doesn't have a leader
                if (leader == null)
                {
                    // Add unit
                    m_Data.selectedUnits.Add(new KeyValuePair<int, UnitBehavior>(goID, goUnit));
                    goUnit.Select();
                }
                else
                {
                    // Check if we already have this leader selected
                    bool duplicate = false;
                    int leaderID = leader.GetInstanceID();
                    foreach (KeyValuePair<int, GroupLeader> selectedLeader in m_Data.selectedLeaders)
                    {
                        if (selectedLeader.Key == leaderID)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    // If already selected, continue
                    if (duplicate)
                        continue;

                    // Loop over all units in the leader's group and select them
                    foreach (KeyValuePair<int, UnitBehavior> groupUnit in leader.units)
                    {
                        groupUnit.Value.Select();
                    }

                    // Add leader to list
                    m_Data.selectedLeaders.Add(new KeyValuePair<int, GroupLeader>(leaderID, leader));
                }
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
        if (m_Data.selectedUnits.Count == 0 && m_Data.selectedLeaders.Count == 0)
            return;

        // Loop over all units
        foreach (KeyValuePair<int, UnitBehavior> unit in m_Data.selectedUnits)
        {
            unit.Value.Deselect();
        }

        // Clear the list of units
        m_Data.selectedUnits.Clear();

        // Loop over all leaders
        foreach (KeyValuePair<int, GroupLeader> leader in m_Data.selectedLeaders)
        {
            // Loop over all units in group
            foreach (KeyValuePair<int, UnitBehavior> unit in leader.Value.units)
            {
                unit.Value.Deselect();
            }
        }

        // Clear the list of leaders
        m_Data.selectedLeaders.Clear();
    }

    // Group selected units
    private void GroupSelection()
    {
        // Only execute if we have units
        if (m_Data.selectedUnits.Count == 0 && m_Data.selectedLeaders.Count == 0)
            return;

        // Loop over all selected leaders
        foreach (KeyValuePair<int, GroupLeader> selectedLeader in m_Data.selectedLeaders)
        {
            // Loop over all units in group
            foreach (KeyValuePair<int, UnitBehavior> unit in selectedLeader.Value.units)
            {
                // Add them to our current selected units
                m_Data.selectedUnits.Add(new KeyValuePair<int, UnitBehavior>(unit.Key, unit.Value));
            }

            // Destroy all selected leaders
            Destroy(selectedLeader.Value.gameObject);
        }

        // Clear the list of leaders
        m_Data.selectedLeaders.Clear();

        // Get the average location of selected units
        Vector3 averageLocation = new Vector3();
        // Loop over all units
        foreach (KeyValuePair<int, UnitBehavior> unit in m_Data.selectedUnits)
        {
            averageLocation += unit.Value.transform.position;
        }
        averageLocation /= m_Data.selectedUnits.Count;

        // Instantiate a leader in middle
        GameObject go = Instantiate(m_GroupLeaderPrefab, averageLocation, Quaternion.identity);
        GroupLeader leader = go.GetComponent<GroupLeader>();

        // Loop over all units
        foreach (KeyValuePair<int, UnitBehavior> unit in m_Data.selectedUnits)
        {
            // Add to leader list of units and set unit leader
            leader.units.Add(unit);
            unit.Value.SetLeader(leader);
        }
    }
}
