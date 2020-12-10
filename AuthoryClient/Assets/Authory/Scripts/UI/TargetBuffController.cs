using UnityEngine;

public class TargetBuffController : MonoBehaviour
{

    [SerializeField] GameObject BuffLayoutPrefab = null;

    public void RefreshBuffs(Entity entity)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (var buff in entity.Buffs)
        {
            BuffLayoutController buffLayout = Instantiate(BuffLayoutPrefab).GetComponent<BuffLayoutController>();
            buffLayout.transform.SetParent(this.transform);
            buffLayout.SetContent(buff);
        }
    }
}
