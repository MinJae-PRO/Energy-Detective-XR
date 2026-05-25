using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamic crosshair controller that changes appearance based on what the player is looking at.
/// Attach to Canvas GameObject with an Image component for the crosshair.
/// </summary>
public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Images")]
    [Tooltip("The default crosshair image (dot or small cross)")]
    public Image crosshairImage;

    [Tooltip("Optional: Secondary crosshair for interaction hint")]
    public Image interactionRingImage;

    [Header("Crosshair Sprites")]
    [Tooltip("Sprite for default idle state")]
    public Sprite defaultSprite;

    [Tooltip("Sprite when hovering over an interactable object")]
    public Sprite interactableSprite;

    [Tooltip("Sprite when hovering over a fixed object")]
    public Sprite fixedSprite;

    [Tooltip("Sprite when hovering over a note/instruction")]
    public Sprite noteSprite;

    [Header("Colors")]
    [Tooltip("Default crosshair color")]
    public Color defaultColor = new Color(1f, 1f, 1f, 0.8f);

    [Tooltip("Color when hovering over interactable")]
    public Color interactableColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Tooltip("Color when hovering over fixed object")]
    public Color fixedColor = new Color(0.3f, 0.7f, 1f, 0.7f);

    [Tooltip("Color when hovering over note")]
    public Color noteColor = new Color(1f, 0.9f, 0.2f, 1f);

    [Tooltip("Color when interaction is out of range")]
    public Color outOfRangeColor = new Color(1f, 0.4f, 0.2f, 0.6f);

    [Header("Size & Animation")]
    [Tooltip("Default crosshair size")]
    public float defaultSize = 12f;

    [Tooltip("Size when hovering over interactable")]
    public float hoverSize = 18f;

    [Tooltip("Speed of size transition")]
    public float sizeLerpSpeed = 12f;

    [Tooltip("Speed of color transition")]
    public float colorLerpSpeed = 10f;

    [Tooltip("Subtle pulse animation when hovering")]
    public bool enableHoverPulse = true;

    [Tooltip("Pulse scale amount")]
    public float pulseAmount = 0.15f;

    [Tooltip("Pulse speed")]
    public float pulseSpeed = 4f;

    [Header("Range Indicator")]
    [Tooltip("Show a range indicator on the crosshair")]
    public bool showRangeIndicator = true;

    [Tooltip("Maximum interaction distance")]
    public float maxInteractionDistance = 8f;

    [Header("References")]
    [Tooltip("The player's camera for raycasting")]
    public Camera playerCamera;

    [Tooltip("The interaction script for distance checking")]
    public PlayerInteraction playerInteraction;

    // Internal state
    private RectTransform crosshairRect;
    private RectTransform ringRect;
    private float currentTargetSize;
    private Color currentTargetColor;
    private Sprite currentTargetSprite;
    private float pulseTime;
    private bool isHoveringInteractable = false;
    private float currentSize;

    void Start()
    {
        if (crosshairImage != null)
        {
            crosshairRect = crosshairImage.GetComponent<RectTransform>();
        }

        if (interactionRingImage != null)
        {
            ringRect = interactionRingImage.GetComponent<RectTransform>();
            interactionRingImage.gameObject.SetActive(false);
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            if (playerInteraction != null)
            {
                maxInteractionDistance = playerInteraction.interactDistance;
            }
        }

        currentTargetSize = defaultSize;
        currentTargetColor = defaultColor;
        currentTargetSprite = defaultSprite;
        currentSize = defaultSize;

        ApplySizeAndColor(defaultSize, defaultColor);
    }

    void Update()
    {
        DetectHoveredObject();
        AnimateCrosshair();
    }

    /// <summary>
    /// Forces the crosshair to a specific state.
    /// </summary>
    public void SetCrosshairState(CrosshairState state)
    {
        switch (state)
        {
            case CrosshairState.Default:
                currentTargetSize = defaultSize;
                currentTargetColor = defaultColor;
                currentTargetSprite = defaultSprite;
                isHoveringInteractable = false;
                break;

            case CrosshairState.Interactable:
                currentTargetSize = hoverSize;
                currentTargetColor = interactableColor;
                currentTargetSprite = interactableSprite;
                isHoveringInteractable = true;
                break;

            case CrosshairState.InteractableOutOfRange:
                currentTargetSize = hoverSize * 0.8f;
                currentTargetColor = outOfRangeColor;
                currentTargetSprite = defaultSprite;
                isHoveringInteractable = false;
                break;

            case CrosshairState.Fixed:
                currentTargetSize = defaultSize;
                currentTargetColor = fixedColor;
                currentTargetSprite = fixedSprite;
                isHoveringInteractable = false;
                break;

            case CrosshairState.Note:
                currentTargetSize = hoverSize;
                currentTargetColor = noteColor;
                currentTargetSprite = noteSprite;
                isHoveringInteractable = true;
                break;
        }

        if (crosshairImage != null && currentTargetSprite != null)
        {
            crosshairImage.sprite = currentTargetSprite;
        }
    }

    /// <summary>
    /// Hides the crosshair.
    /// </summary>
    public void HideCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.enabled = false;
        }
        if (interactionRingImage != null)
        {
            interactionRingImage.enabled = false;
        }
    }

    /// <summary>
    /// Shows the crosshair.
    /// </summary>
    public void ShowCrosshair()
    {
        if (crosshairImage != null)
        {
            crosshairImage.enabled = true;
        }
        if (interactionRingImage != null)
        {
            interactionRingImage.enabled = true;
        }
    }

    private void DetectHoveredObject()
    {
        if (playerCamera == null)
        {
            SetCrosshairState(CrosshairState.Default);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance * 1.5f))
        {
            float distance = hit.distance;
            bool inRange = distance <= maxInteractionDistance;

            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();
            if (energyObject != null)
            {
                if (energyObject.isFixed)
                {
                    SetCrosshairState(CrosshairState.Fixed);
                }
                else if (inRange)
                {
                    SetCrosshairState(CrosshairState.Interactable);
                }
                else
                {
                    SetCrosshairState(CrosshairState.InteractableOutOfRange);
                }
                return;
            }

            InstructionNote note = hit.collider.GetComponentInParent<InstructionNote>();
            if (note != null && inRange)
            {
                SetCrosshairState(CrosshairState.Note);
                return;
            }

            KickDoor door = hit.collider.GetComponentInParent<KickDoor>();
            if (door != null && inRange)
            {
                SetCrosshairState(CrosshairState.Interactable);
                return;
            }
        }

        SetCrosshairState(CrosshairState.Default);
    }

    private void AnimateCrosshair()
    {
        if (crosshairImage == null) return;

        // Lerp size
        currentSize = Mathf.Lerp(currentSize, currentTargetSize, Time.deltaTime * sizeLerpSpeed);

        // Apply pulse if hovering
        float finalSize = currentSize;
        if (enableHoverPulse && isHoveringInteractable)
        {
            pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(pulseTime) * pulseAmount;
            finalSize = currentSize * (1f + pulse);
        }

        ApplySizeAndColor(finalSize, Color.Lerp(crosshairImage.color, currentTargetColor, Time.deltaTime * colorLerpSpeed));

        // Update ring
        if (interactionRingImage != null && ringRect != null)
        {
            interactionRingImage.gameObject.SetActive(isHoveringInteractable);
            if (isHoveringInteractable)
            {
                float ringSize = finalSize * 1.8f;
                ringRect.sizeDelta = Vector2.one * ringSize;
            }
        }
    }

    private void ApplySizeAndColor(float size, Color color)
    {
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = Vector2.one * size;
        }

        if (crosshairImage != null)
        {
            crosshairImage.color = color;
        }
    }
}

/// <summary>
/// Crosshair visual states.
/// </summary>
public enum CrosshairState
{
    Default,
    Interactable,
    InteractableOutOfRange,
    Fixed,
    Note
}
