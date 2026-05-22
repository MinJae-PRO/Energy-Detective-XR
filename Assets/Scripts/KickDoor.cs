using UnityEngine;

public class KickDoor : MonoBehaviour
{
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    void Start()
    {
        closedRotation = transform.rotation;

        openRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openAngle,
            transform.eulerAngles.z
        );

        targetRotation = closedRotation;
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    public void OpenDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            targetRotation = openRotation;
        }
        else
        {
            targetRotation = closedRotation;
        }
    }
}