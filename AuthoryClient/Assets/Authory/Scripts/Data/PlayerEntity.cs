using UnityEngine;

public class PlayerEntity : Entity
{
    public long Experience { get; internal set; }
    public Entity Target { get; set; }
    protected override void Awake()
    {
        base.Awake();
        info = GetComponentInChildren<EntityInfo>();
        SwordAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 200, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 300, LayerMask.GetMask("Terrain")))
        {
            this.transform.position = new Vector3(this.transform.position.x, hit.point.y + 0.5f, this.transform.position.z);
        }
    }

    private void SetHeightToTerrain()
    {
        float y = Terrain.activeTerrain.SampleHeight(transform.position);
        y += 0.5f;
        this.transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
