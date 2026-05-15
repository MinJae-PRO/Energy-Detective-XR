using UnityEngine;

public class EnergyObject : MonoBehaviour
{
    public string objectName = "Energy Object";
    public bool isFixed = false;
    public int points = 10;

    public Material wasteMaterial;
    public Material fixedMaterial;

    public Renderer objectRenderer;

    public GameObject wasteLabel;
    public GameObject fixedIndicator;

    public Renderer statusBulbRenderer;
    public Material bulbWasteMaterial;
    public Material bulbFixedMaterial;

    private void Start()
    {
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
    }

    public void FixObject()
    {
        if (isFixed)
        {
            return;
        }

        isFixed = true;

        if (objectRenderer != null && fixedMaterial != null)
        {
            objectRenderer.material = fixedMaterial;
        }

        if (statusBulbRenderer != null && bulbFixedMaterial != null)
        {
            statusBulbRenderer.material = bulbFixedMaterial;
        }

        if (wasteLabel != null)
        {
            wasteLabel.SetActive(false);
        }

        if (fixedIndicator != null)
        {
            fixedIndicator.SetActive(true);
        }
        
        // Add: Play sound effect (optional - to add an AudioSource component)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}