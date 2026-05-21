using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject completionText;
    public GameObject crosshairText;
    
    public GameObject crosshairObject;
    public Color defaultCrosshairColor = Color.white;
    public Color hoverCrosshairColor = Color.green;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;
    private EnergyObject currentHoverTarget;
    private Renderer crosshairRenderer;
    private UIManager uiManager;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        uiManager = FindObjectOfType<UIManager>();
        
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
        CheckForHoverTarget();
        
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    void CheckForHoverTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();
            
            if (energyObject != null && !energyObject.isFixed)
            {
                if (currentHoverTarget != energyObject)
                {
                    currentHoverTarget = energyObject;
                    // Tell UI to show hover effect
                    if (uiManager != null)
                        uiManager.SetCrosshairHover(true);
                    
                    if (crosshairRenderer != null)
                        crosshairRenderer.material.color = hoverCrosshairColor;
                }
                return;
            }
        }
        
        // No valid target - reset
        if (currentHoverTarget != null)
        {
            currentHoverTarget = null;
            if (uiManager != null)
                uiManager.SetCrosshairHover(false);
            
            if (crosshairRenderer != null)
                crosshairRenderer.material.color = defaultCrosshairColor;
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();

            if (energyObject != null && !energyObject.isFixed)
            {
                energyObject.FixObject();
                
                // Show fix feedback through UI
                if (uiManager != null)
                {
                    uiManager.ShowFixFeedback(energyObject.objectName, energyObject.points);
                }
                
                // Use GameManager for scoring
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.FixEnergyObject(energyObject.objectName, energyObject.points);
                }
                else
                {
                    Debug.LogWarning("GameManager not found - using legacy scoring");
                }
                
                // Update energyObjects array
                energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
            }
        }
    }
}