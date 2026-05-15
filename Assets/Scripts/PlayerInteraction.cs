using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject completionText;
    public GameObject crosshairText;
    
    // Add: Crosshair color change for feedback
    public GameObject crosshairObject;
    public Color defaultCrosshairColor = Color.white;
    public Color hoverCrosshairColor = Color.green;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;
    
    // Add: Track current hover target for visual feedback
    private EnergyObject currentHoverTarget;
    private Renderer crosshairRenderer;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        
        if (crosshairObject != null)
        {
            crosshairRenderer = crosshairObject.GetComponent<Renderer>();
            if (crosshairRenderer != null)
                crosshairRenderer.material.color = defaultCrosshairColor;
        }
        
        if (completionText != null)
            completionText.SetActive(false);
        
        if (crosshairText != null)
            crosshairText.SetActive(true);
    }

    void Update()
    {
        // Add: Check for hover targets to give feedback
        CheckForHoverTarget();
        
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    // Add: Check what object player is looking at 
    void CheckForHoverTarget()
    {
        Ray ray = Add Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();
            
            if (energyObject != null && !energyObject.isFixed)
            {
                if (currentHoverTarget != energyObject)
                {
                    currentHoverTarget = energyObject;
                    // Change crosshair color to indicate hover
                    if (crosshairRenderer != null)
                        crosshairRenderer.material.color = hoverCrosshairColor;
                }
                return;
            }
        }
        
        // No valid target - reset crosshair
        if (currentHoverTarget != null)
        {
            currentHoverTarget = null;
            if (crosshairRenderer != null)
                crosshairRenderer.material.color = defaultCrosshairColor;
        }
    }

    void TryInteract()
    {
        Ray ray = Add Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();

            if (energyObject != null && !energyObject.isFixed)
            {
                energyObject.FixObject();
                
                // Use GameManager for scoring
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.FixEnergyObject(energyObject.objectName, energyObject.points);
                }
                else
                {
                    // Fallback to old scoring method
                    Debug.LogWarning("GameManager not found - using legacy scoring");
                }
                
                // Update energyObjects array
                energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
            }
        }
    }
}