using UnityEngine;

public class InstructionNote : MonoBehaviour
{
    public GameObject instructionPanel;

    private bool hasBeenPickedUp = false;
    private Renderer noteRenderer;
    private Collider noteCollider;

    void Start()
    {
        noteRenderer = GetComponent<Renderer>();
        noteCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (hasBeenPickedUp && Input.GetKeyDown(KeyCode.N))
        {
            if (instructionPanel != null)
            {
                instructionPanel.SetActive(!instructionPanel.activeSelf);
            }
        }
    }

    public void PickUpNote()
    {
        if (hasBeenPickedUp)
        {
            return;
        }

        hasBeenPickedUp = true;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }

        if (noteRenderer != null)
        {
            noteRenderer.enabled = false;
        }

        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }
    }
}