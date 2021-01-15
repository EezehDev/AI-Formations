using System.Collections.Generic;
using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    [SerializeField] private GameObject m_SelectionBoxPrefab = null;

    private SelectionBox m_SelectionBox = null;
    private bool m_Selecting = false;
    public List<GameObject> m_SelectedUnits = new List<GameObject>();

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (m_SelectionBox == null)
            {
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
                SelectUnits();
                m_Selecting = false;
            }
        }
    }

    private void SelectUnits()
    {
        if (m_SelectionBox != null)
        {
            m_SelectedUnits = m_SelectionBox.GetOverlappingObjects();

            Destroy(m_SelectionBox.gameObject);
            m_SelectionBox = null;
        }
    }
}
