using UnityEngine;

public class EnergyObject : MonoBehaviour
{
    public string objectName = "Energy Object";
    public bool isFixed = false;
    public int points = 10;

    public GameObject wasteLabel;
    public GameObject fixedIndicator;

    public void FixObject()
    {
        if (isFixed == true)
        {
            return;
        }

        isFixed = true;

        if (wasteLabel != null)
        {
            wasteLabel.SetActive(false);
        }

        if (fixedIndicator != null)
        {
            fixedIndicator.SetActive(true);
        }

        Debug.Log(objectName + " fixed! +" + points + " points");
    }
}