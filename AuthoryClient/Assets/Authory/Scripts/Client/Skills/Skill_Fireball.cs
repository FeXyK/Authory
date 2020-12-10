using UnityEngine;

public class Skill_Fireball : MonoBehaviour
{
    public Entity Caster;
    public Entity Target;

    [SerializeField] private float Speed = 18;
    [SerializeField] private float Lifetime = 10;
    private void Update()
    {
        if (Target == null || Caster == null) return;

        this.transform.LookAt(Target.transform.position);
        float distance = Vector3.Distance(Target.EndPosition, this.transform.position);
        this.transform.position += transform.forward * Time.deltaTime * Speed;



        if ((this.transform.position - Target.transform.position).sqrMagnitude < 1)
        {
            Destroy(this.gameObject);
        }
    }
    private void FixedUpdate()
    {
        if ((Lifetime -= Time.deltaTime) < 0)
        {
            Destroy(this.gameObject);
        }
    }
}