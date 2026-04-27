using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public GameObject completionText;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsOfType<EnergyObject>();

        UpdateScoreText();

        if (completionText != null)
        {
            completionText.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();

            if (energyObject != null && energyObject.isFixed == false)
            {
                energyObject.FixObject();
                score += energyObject.points;

                UpdateScoreText();
                CheckCompletion();
            }
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void CheckCompletion()
    {
        foreach (EnergyObject obj in energyObjects)
        {
            if (obj.isFixed == false)
            {
                return;
            }
        }

        if (completionText != null)
        {
            completionText.SetActive(true);
        }
    }
}