using UnityEngine;

/// <summary>
/// Represents an energy object that can be fixed by the player.
/// Attach to energy objects in the scene and configure the quiz data.
/// </summary>
public class EnergyObject : MonoBehaviour
{
    [Header("Object Info")]
    [Tooltip("Display name for this energy object")]
    public string objectName = "Energy Object";

    [Tooltip("Has this object been fixed?")]
    public bool isFixed = false;

    [Tooltip("Points awarded for fixing this object")]
    public int points = 10;

    [Header("Quiz Data")]
    [Tooltip("Quiz question shown when player interacts")]
    [TextArea(3, 5)]
    public string question;

    [Tooltip("Correct answer text")]
    public string correctAnswer;

    [Tooltip("First wrong answer text")]
    public string wrongAnswer1;

    [Tooltip("Second wrong answer text")]
    public string wrongAnswer2;

    [Header("Visual Materials")]
    [Tooltip("Material applied when object is broken/wasteful")]
    public Material wasteMaterial;

    [Tooltip("Material applied when object is fixed")]
    public Material fixedMaterial;

    [Tooltip("Renderer to apply materials to (auto-detected if null)")]
    public Renderer objectRenderer;

    [Header("Status Indicators")]
    [Tooltip("GameObject shown when object is broken (e.g., waste label)")]
    public GameObject wasteLabel;

    [Tooltip("GameObject shown when object is fixed (e.g., checkmark)")]
    public GameObject fixedIndicator;

    [Tooltip("Status bulb renderer for color-coded status")]
    public Renderer statusBulbRenderer;

    [Tooltip("Material for status bulb when broken")]
    public Material bulbWasteMaterial;

    [Tooltip("Material for status bulb when fixed")]
    public Material bulbFixedMaterial;

    [Header("Effects")]
    [Tooltip("Particle effect played when object is fixed")]
    public ParticleSystem fixEffect;

    [Tooltip("Sound played when object is fixed")]
    public AudioClip fixSound;

    [Header("Animation")]
    [Tooltip("Should the object have a subtle idle animation when broken?")]
    public bool idleAnimationWhenBroken = true;

    [Tooltip("Idle animation intensity")]
    public float idleIntensity = 0.05f;

    [Tooltip("Idle animation speed")]
    public float idleSpeed = 1f;

    // Internal state
    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;

        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        if (objectRenderer != null && wasteMaterial != null)
        {
            objectRenderer.material = wasteMaterial;
        }

        if (statusBulbRenderer != null && bulbWasteMaterial != null)
        {
            statusBulbRenderer.material = bulbWasteMaterial;
        }

        if (wasteLabel != null)
        {
            wasteLabel.SetActive(true);
        }

        if (fixedIndicator != null)
        {
            fixedIndicator.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && fixSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Subtle idle animation for unfixed objects
        if (!isFixed && idleAnimationWhenBroken)
        {
            float yOffset = Mathf.Sin(Time.time * idleSpeed) * idleIntensity;
            transform.position = startPosition + Vector3.up * yOffset;
        }
    }

    /// <summary>
    /// Fixes this energy object, updating visuals and state.
    /// </summary>
    public void FixObject()
    {
        if (isFixed)
        {
            return;
        }

        isFixed = true;

        // Apply fixed material
        if (objectRenderer != null && fixedMaterial != null)
        {
            objectRenderer.material = fixedMaterial;
        }

        // Update status bulb
        if (statusBulbRenderer != null && bulbFixedMaterial != null)
        {
            statusBulbRenderer.material = bulbFixedMaterial;
        }

        // Toggle labels/indicators
        if (wasteLabel != null)
        {
            wasteLabel.SetActive(false);
        }

        if (fixedIndicator != null)
        {
            fixedIndicator.SetActive(true);
        }

        // Play effects
        if (fixEffect != null)
        {
            fixEffect.Play();
        }

        if (fixSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fixSound);
        }

        // Reset position from idle animation
        transform.position = startPosition;

        Debug.Log($"[EnergyObject] {objectName} has been fixed!");
    }

    /// <summary>
    /// Gets the quiz data for this object.
    /// </summary>
    public QuizData GetQuizData()
    {
        return new QuizData
        {
            question = question,
            correctAnswer = correctAnswer,
            wrongAnswer1 = wrongAnswer1,
            wrongAnswer2 = wrongAnswer2
        };
    }

    /// <summary>
    /// Resets the object to its broken state.
    /// </summary>
    public void ResetObject()
    {
        isFixed = false;

        if (objectRenderer != null && wasteMaterial != null)
        {
            objectRenderer.material = wasteMaterial;
        }

        if (statusBulbRenderer != null && bulbWasteMaterial != null)
        {
            statusBulbRenderer.material = bulbWasteMaterial;
        }

        if (wasteLabel != null)
        {
            wasteLabel.SetActive(true);
        }

        if (fixedIndicator != null)
        {
            fixedIndicator.SetActive(false);
        }

        startPosition = transform.position;
    }

    void OnDrawGizmos()
    {
        // Draw a wire sphere to visualize the object in editor
        Gizmos.color = isFixed ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Draw label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, $"{objectName}\n{(isFixed ? "Fixed" : "Broken")}");
#endif
    }
}

/// <summary>
/// Data container for quiz question and answers.
/// </summary>
public struct QuizData
{
    public string question;
    public string correctAnswer;
    public string wrongAnswer1;
    public string wrongAnswer2;
}
