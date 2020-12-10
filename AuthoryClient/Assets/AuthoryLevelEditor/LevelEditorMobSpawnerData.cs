using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorMobSpawnerData : MonoBehaviour
{
    [SerializeField] Vector3 Position = Vector3.zero;
    [SerializeField] float Radius = 30;
    [SerializeField] float MobCount = 100;

    [SerializeField] Canvas MobSpawnerCanvas = null;

    [SerializeField] Slider MobCountSlider = null;
    [SerializeField] TMP_Text MobCountText = null;
    [SerializeField] Slider RadiusSlider = null;
    [SerializeField] TMP_Text RadiusText = null;


    [SerializeField] Transform RadiusDisplayObject = null;


    private Camera mainCamera;

    public bool Spawned { get; set; } = false;

    private void Awake()
    {

        mainCamera = FindObjectOfType<Camera>();
        MobCountSlider.onValueChanged.AddListener(OnMobCountValueChanged);
        RadiusSlider.onValueChanged.AddListener(OnRadiusValueChanged);
        MobCountSlider.value = 100;
    }

    private void OnRadiusValueChanged(float value)
    {
        RadiusText.text = "Radius: " + value;
        RadiusDisplayObject.localScale = new Vector3(value, 30, value);
        Radius = value;
    }

    private void OnMobCountValueChanged(float value)
    {
        MobCountText.text = "MobCount: " + value;
        MobCount = value;
    }

    void Update()
    {
        MobSpawnerCanvas.transform.rotation = mainCamera.transform.rotation;
        Position = transform.position;
        if (!Spawned && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f, LayerMask.GetMask("Terrain")))
            {
                Radius = Vector3.Distance(hit.point, this.transform.position);
                OnRadiusValueChanged(Radius);
                RadiusSlider.SetValueWithoutNotify(Radius);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Spawned = true;
        }
    }

    public override string ToString()
    {
        return string.Format($"{(int)Position.x};{(int)Position.z};{Radius};{MobCount}");
    }
}
