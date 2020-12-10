using TMPro;
using UnityEngine;

public class LevelEditorCameraController : MonoBehaviour
{
    [SerializeField] float Speed = 1f;
    [SerializeField] float BaseSpeed = 10f;
    [SerializeField] float SpeedMultiplier = 1f;
    [SerializeField] Camera Camera = null;
    [SerializeField] TMP_Text showSpeedMultiplier = null;

    [SerializeField] bool InvertMouseX = false;
    [SerializeField] bool InvertMouseY = false;

    private void Awake()
    {
        Camera = Camera.main;
    }
    void Update()
    {
        SpeedMultiplier += Input.GetAxis("Mouse ScrollWheel");

        showSpeedMultiplier.text = "SpeedMultiplier: " + SpeedMultiplier.ToString("#0.00");

        Speed = Input.GetKey(KeyCode.LeftShift) ? BaseSpeed * SpeedMultiplier : BaseSpeed;

        float x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * Speed;
        float y = Input.GetKey(KeyCode.Space) == true ? Speed * Time.deltaTime : 0;

        Camera.transform.position += Camera.transform.forward * z + Camera.transform.right * x;
        Camera.transform.position += new Vector3(0, y, 0);

        if (Input.GetMouseButton(1))
        {
            float vertical = (InvertMouseX ? 1 : -1) * Input.GetAxis("Mouse X");
            float horizontal = (InvertMouseY ? 1 : -1) * Input.GetAxis("Mouse Y");
            Camera.transform.eulerAngles += new Vector3(horizontal, vertical, 0);
        }
    }
}
