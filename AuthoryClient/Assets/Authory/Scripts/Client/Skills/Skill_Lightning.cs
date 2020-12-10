using UnityEngine;

public class Skill_Lightning : MonoBehaviour
{
    public Entity Caster;
    public Entity Target;

    public Entity AlternativeEntity;

    [SerializeField] private float Lifetime = 1.0f;
    void Update()
    {
        if (Caster != null && Target != null && AlternativeEntity != null)
        {
            this.transform.localScale = new Vector3(1, 1, Vector3.Distance(AlternativeEntity.transform.position, Target.transform.position) / 10.0f);
            this.transform.position = (Target.transform.position + AlternativeEntity.transform.position) / 2.0f;
            this.transform.LookAt(Target.transform.position);
        }

        if ((Lifetime -= Time.deltaTime) < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
