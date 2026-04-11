using UnityEngine;

public class CameraColor : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color materialPlaneColor = new Color(0.14f, 0.25f, 0.17f, 1f);
    [SerializeField] private Color hellColor = new Color(0.24f, 0.3f, 0.14f, 1f);

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnAfterlifeStateChanged += HandleAfterlifeStateChanged;
            HandleAfterlifeStateChanged(playerHealth.IsInAfterlife);
            return;
        }

        SetBackgroundColor(false);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnAfterlifeStateChanged -= HandleAfterlifeStateChanged;
        }
    }

    private void HandleAfterlifeStateChanged(bool isInAfterlife)
    {
        SetBackgroundColor(isInAfterlife);
    }

    private void SetBackgroundColor(bool isInAfterlife)
    {
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.backgroundColor = isInAfterlife ? hellColor : materialPlaneColor;
    }
}
