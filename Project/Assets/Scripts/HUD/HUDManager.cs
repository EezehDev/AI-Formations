using System.Collections;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    private static HUDManager m_Instance;
    public static HUDManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<HUDManager>();

                if (m_Instance == null)
                {
                    GameObject container = new GameObject("HUDManager");
                    m_Instance = container.AddComponent<HUDManager>();
                }
            }

            return m_Instance;
        }
    }

    [Header("Player feedback")]
    [SerializeField] private GameObject m_ErrorWindow = null;
    [SerializeField] private TextMeshProUGUI m_ErrorMessage = null;
    private IEnumerator m_ErrorCoroutine = null;

    public void SetErrorMessage(string message, float duration = 1f)
    {
        if (m_ErrorCoroutine != null)
            StopCoroutine(m_ErrorCoroutine);

        m_ErrorCoroutine = DisplayErrorMessage(message, duration);
        StartCoroutine(m_ErrorCoroutine);
    }

    private IEnumerator DisplayErrorMessage(string message, float duration)
    {
        if (m_ErrorWindow != null)
        {
            m_ErrorWindow.SetActive(true);

            if (m_ErrorMessage != null)
                m_ErrorMessage.text = message;
        }

        yield return new WaitForSecondsRealtime(duration);

        if (m_ErrorWindow != null)
        {
            m_ErrorWindow.SetActive(false);

            if (m_ErrorMessage != null)
                m_ErrorMessage.text = message;
        }

        m_ErrorCoroutine = null;
    }
}