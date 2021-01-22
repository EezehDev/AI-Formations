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
    const int m_MinGroupSize = 4;
    const int m_MaxGroupSize = 24;


    // -------------------------
    // START & UPDATE
    // -------------------------

    private void Start()
    {
        m_Data = GetComponent<PlayerData>();
    }

    private void Update()
    {
        // Get left mouse button input
        if (Input.GetMouseButton(0))
        {
            ClickDragSelect();
        }
        else
        {
            StopSelecting();
        }

        // Upon pressing escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }

        // Upon pressing G
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Check if we are holding down any of the shift keys
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Ungroup current selection
                UngroupSelection();
            }
            else
            {
                // Group current selection
                GroupSelection();
            }
        }
    }


    // -------------------------
    // BOX SELECTION
    // -------------------------

    // Handle mouse click and drag select
    private void ClickDragSelect()
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

    private void StopSelecting()
    {
        // Only execute when selecting
        if (!m_Selecting)
            return;

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


    // -------------------------
    // SELECTING UNITS
    // -------------------------

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
            // Check if gameobject they have behavior script
            UnitBehavior unit = go.GetComponent<UnitBehavior>();
            if (unit == null)
                continue;

            // If we are adding to our selection
            if (m_Adding)
            {
                // If duplicate, don't add this unit
                if (m_Data.selectedUnits.Contains(unit))
                    continue;
            }

            // Get the unit's leader
            GroupLeader leader = unit.GetLeader();

            // If unit doesn't have a leader
            if (leader == null)
            {
                // Add unit
                m_Data.selectedUnits.Add(unit);
                unit.Select();
            }
            else
            {
                // If duplicate, don't add this leader
                if (m_Data.selectedLeaders.Contains(leader))
                    continue;

                // Loop over all units in the leader's group and select them
                foreach (UnitBehavior groupUnit in leader.units)
                {
                    groupUnit.Select();
                }

                // Add leader to list
                m_Data.selectedLeaders.Add(leader);
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
        foreach (UnitBehavior unit in m_Data.selectedUnits)
        {
            unit.Deselect();
        }

        // Clear the list of units
        m_Data.selectedUnits.Clear();

        // Loop over all leaders
        foreach (GroupLeader leader in m_Data.selectedLeaders)
        {
            // Loop over all units in group
            foreach (UnitBehavior unit in leader.units)
            {
                unit.Deselect();
            }
        }

        // Clear the list of leaders
        m_Data.selectedLeaders.Clear();
    }

    // Group selected units
    private void GroupSelection()
    {
        // Only execute when we have a selection
        if (m_Data.selectedUnits.Count == 0 && m_Data.selectedLeaders.Count == 0)
            return;

        // Only execute when we have at least 2 groups
        if (m_Data.selectedUnits.Count == 0 && m_Data.selectedLeaders.Count < 2)
        {
            HUDManager.Instance.SetErrorMessage("Selected units already grouped.");
            return;
        }
        // Or minimum amount of units selected
        if (m_Data.selectedLeaders.Count == 0 && m_Data.selectedUnits.Count < m_MinGroupSize)
            return;

        // Check if we exceed maximum group size
        int amountUnits = 0;
        foreach (GroupLeader dataLeader in m_Data.selectedLeaders)
        {
            foreach (UnitBehavior dataLeaderUnit in dataLeader.units)
            {
                amountUnits++;
            }
        }
        amountUnits += m_Data.selectedUnits.Count;

        // Only execute if we are within group size limit
        if (amountUnits > m_MaxGroupSize)
        {
            HUDManager.Instance.SetErrorMessage("Too many units selected.");
            return;
        }

        bool newLeader = false;
        if (m_Data.selectedLeaders.Count == 0 || m_Data.selectedLeaders.Count >= 2)
            newLeader = true;

        // Set invalid index
        int freeIndex = -1;

        UngroupSelection(newLeader);

        if (newLeader)
        {
            for (int index = 0; index < m_Data.groups.Length; index++)
            {
                // Get the first free index
                if (m_Data.groups[index] == false)
                {
                    freeIndex = index;
                    break;
                }
            }
        }
        else
        {
            freeIndex = m_Data.selectedLeaders[0].groupID;
        }

        // Do not execute if we don't have a free index
        if (freeIndex == -1)
        {
            HUDManager.Instance.SetErrorMessage("Group limit reached.");
            return;
        }

        // Set minimum and maximum to opposite values
        Vector3 minimum = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 maximum = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

        // Loop over all units
        foreach (UnitBehavior unit in m_Data.selectedUnits)
        {
            Vector3 unitPosition = unit.transform.position;

            if (unitPosition.x < minimum.x)
                minimum.x = unitPosition.x;
            if (unitPosition.x > maximum.x)
                maximum.x = unitPosition.x;

            if (unitPosition.y < minimum.y)
                minimum.y = unitPosition.y;
            if (unitPosition.y > maximum.y)
                maximum.y = unitPosition.y;

            if (unitPosition.z < minimum.z)
                minimum.z = unitPosition.z;
            if (unitPosition.z > maximum.z)
                maximum.z = unitPosition.z;
        }

        Vector3 middle = (minimum + maximum) / 2f;

        GroupLeader leader = null;
        if (newLeader)
        {
            // Instantiate a leader in middle of furthest units
            GameObject go = Instantiate(m_GroupLeaderPrefab, middle, Quaternion.identity);
            leader = go.GetComponent<GroupLeader>();

            // Assign group ID
            leader.groupID = freeIndex;
            m_Data.groups[freeIndex] = true;
        }
        else
        {
            leader = m_Data.selectedLeaders[0];
            leader.SetLocation(middle);
            leader.units.Clear();
        }

        // Loop over all units
        foreach (UnitBehavior unit in m_Data.selectedUnits)
        {
            // Add to leader list of units, set unit leader and material to group color
            leader.units.Add(unit);
            unit.SetLeader(leader);
            unit.SetMaterial(m_Data.groupMaterials[freeIndex]);
        }

        // Clear list of selected units and create formation
        m_Data.selectedUnits.Clear();
        leader.CreateFormation();

        if (newLeader)
        {
            // Add leader to selected leaders
            m_Data.selectedLeaders.Add(leader);
        }
    }

    // Ungroup selected units
    private void UngroupSelection(bool destroyLeader = true)
    {
        // Only execute if we have leaders
        if (m_Data.selectedLeaders.Count == 0)
            return;

        // Loop over all selected leaders
        foreach (GroupLeader selectedLeader in m_Data.selectedLeaders)
        {
            // Loop over all units in group
            foreach (UnitBehavior unit in selectedLeader.units)
            {
                // Add them to our current selected units, and reset material
                m_Data.selectedUnits.Add(unit);
                unit.SetMaterial();
            }

            if (destroyLeader)
            {
                // Free group ID, destroy leader
                m_Data.groups[selectedLeader.groupID] = false;
                Destroy(selectedLeader.gameObject);
            }
        }

        if (destroyLeader)
        {
            // Clear the list of leaders
            m_Data.selectedLeaders.Clear();
        }
    }
}
