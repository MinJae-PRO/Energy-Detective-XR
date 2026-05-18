using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light torchSpotLight;
    public Light torchAmbientLight;

    private bool isTorchOn = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleTorch();
        }
    }

    void ToggleTorch()
    {
        isTorchOn = !isTorchOn;

        if (torchSpotLight != null)
        {
            torchSpotLight.enabled = isTorchOn;
        }

        if (torchAmbientLight != null)
        {
            torchAmbientLight.enabled = isTorchOn;
        }
    }
}