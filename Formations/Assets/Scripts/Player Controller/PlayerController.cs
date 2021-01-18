using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Data
    private PlayerData m_Data = null;

    [SerializeField] private GameObject m_MoveParticle = null;

    private void Start()
    {
        m_Data = GetComponent<PlayerData>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            // Get mouse location
            Vector3 mouseLocation = Input.mousePosition;
            mouseLocation.z = Camera.main.transform.position.y;

            // Convert mouse location to world space
            Vector3 worldLocation = Camera.main.ScreenToWorldPoint(mouseLocation);
            MoveSelection(worldLocation);

            GameObject go = Instantiate(m_MoveParticle, worldLocation, Quaternion.identity);
            Destroy(go, 1f);
        }
    }

    private void MoveSelection(Vector3 target)
    {
        foreach (KeyValuePair<int, UnitBehavior> unit in m_Data.selectedUnits)
        {
            unit.Value.SetTarget(target);
        }
    }
}
