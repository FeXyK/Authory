using UnityEngine;

/// <summary>
/// Controls the camera that is required for the minimap.
/// </summary>
public class MiniMapController : MonoBehaviour
{
    [SerializeField] Transform player = null;
    [SerializeField] GameObject MinimapCameraPrefab = null;

    [SerializeField] float orthographicSize = 100f;

    private Camera MinimapCamera;

    void Start()
    {
        player = AuthoryData.Instance.Player.transform;
        MinimapCamera = Instantiate(MinimapCameraPrefab).GetComponent<Camera>();
        MinimapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        MinimapCamera.orthographic = true;
        MinimapCamera.orthographicSize = orthographicSize;

    }

    void Update()
    {
        MinimapCamera.transform.position = player.transform.position + new Vector3(0, orthographicSize, 0);
    }
}
