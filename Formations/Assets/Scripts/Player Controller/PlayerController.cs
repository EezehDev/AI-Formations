using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject m_MoveParticle = null;

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            // Get mouse location
            Vector3 mouseLocation = Input.mousePosition;
            mouseLocation.z = Camera.main.transform.position.y;

            // Convert mouse location to world space
            Vector3 worldLocation = Camera.main.ScreenToWorldPoint(mouseLocation);

            GameObject go = Instantiate(m_MoveParticle, worldLocation, Quaternion.identity);
            Destroy(go, 1f);
        }
    }
}
