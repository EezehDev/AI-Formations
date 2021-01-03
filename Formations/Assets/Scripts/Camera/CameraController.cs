using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_MovementSpeed = 10f; // Navigation speed
    [SerializeField] private float m_MaximumX = 30f;
    [SerializeField] private float m_MaximumZ = 25f;
    private Vector3 m_CurrentLocation;

    [Header("Distance")]
    [SerializeField] private float m_ScrollSpeed = 1f; // Scroll speed (distance per scroll)
    [SerializeField] private float m_StartDistance = 10f; // Camera distance on play
    [SerializeField] private float m_MinDistance = 5f; // Minimum camera distance
    [SerializeField] private float m_MaxDistance = 15f; // Maximum camera distance

    // Initialise the camera position
    public void Start()
    {
        m_CurrentLocation.y = m_StartDistance;
        transform.position = m_CurrentLocation;
    }

    public void Update()
    {
        HandleMovement();
        HandleScrolling();

        // After handling movement and scrolling update the location
        transform.position = m_CurrentLocation;
    }

    // Input handling for horizontal and vertical axis, moving the camera around
    private void HandleMovement()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movementInput.Normalize(); // Normalize to avoid faster diagonal movement

        m_CurrentLocation += (new Vector3(movementInput.x, 0f, movementInput.y) * m_MovementSpeed * Time.deltaTime);

        // Clamp to edges of environment
        m_CurrentLocation.x = Mathf.Clamp(m_CurrentLocation.x, -m_MaximumX, m_MaximumX);
        m_CurrentLocation.z = Mathf.Clamp(m_CurrentLocation.z, -m_MaximumZ, m_MaximumZ);
    }

    // Input handling for scroll wheel
    private void HandleScrolling()
    {
        float scrollInput = -(Input.GetAxis("Mouse ScrollWheel")); // Inverted to get correct direction

        m_CurrentLocation.y += scrollInput * m_ScrollSpeed;

        // Clamp between min and max
        m_CurrentLocation.y = Mathf.Clamp(m_CurrentLocation.y, m_MinDistance, m_MaxDistance);
    }
}
