using UnityEngine;

/// <summary>
/// Handles camera in the game view, if attached to a target it will rotate around it.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    public Transform Target { get; set; }

    [SerializeField] bool invertMouse = false;

    [SerializeField] float mouseSensitivity = 50f;
    [SerializeField] float scrollSpeed = 100f;

    [SerializeField] float y = 0f;
    [SerializeField] float x = 0f;
    [SerializeField] float minDistance = 0f;
    [SerializeField] float maxDistance = 40f;

    [SerializeField] Vector3 offset = Vector3.zero;

    private float distance = 10f;

    public float Distance
    {
        get { return distance; }
        set
        {
            distance = value;
            if (distance < minDistance) distance = minDistance;
            if (distance > maxDistance) distance = maxDistance;
        }
    }


    private void LateUpdate()
    {
        if (Target == null) return;
        Distance += -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
        x = y = 0;
        if (Input.GetMouseButton(1))
        {
            x = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 0.5f * (invertMouse ? 1 : -1);
            y = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * (invertMouse ? -1 : 1);
        }
        this.transform.eulerAngles += new Vector3(x, y, 0);
        this.transform.position = Target.position + offset + -this.transform.forward * distance;

        Target.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
    }
}
