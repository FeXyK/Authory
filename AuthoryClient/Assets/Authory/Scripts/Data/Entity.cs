using Assets.Authory.Scripts;
using Assets.Authory.Scripts.Enum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Every character in the game view have to use this.
/// </summary>
public class Entity : EntityBase
{
    [SerializeField] protected Animator SwordAnimator = null;

    [SerializeField] VisualEffect OnHitVFX = null;
    [SerializeField] VisualEffect LevelUpVFX = null;

    [SerializeField] bool isPlayer;
    float despawnTime;

    public bool IsPlayer { get { return isPlayer; } set { isPlayer = value; } }

    public Attributes Attributes { get; protected set; }
    public List<Buff> Buffs { get; protected set; } = new List<Buff>();

    public Resource Health { get; protected set; }
    public Resource Mana { get; protected set; }

    public bool Dead => Health.Value == 0;
    public bool Alive => Health.Value > 0;

    public Vector3 EndPosition { get; private set; }
    public Vector3 StartPosition { get; private set; }

    public ushort Level { get; set; }

    public float MovementSpeed;

    public Vector3 MovementDirection { get; set; }


    private void OnEnable()
    {
        if (LevelUpVFX != null)
            LevelUpVFX.Stop();
    }


    protected virtual void Awake()
    {
        despawnTime = maxDespawnTime;

        Health = new Resource();
        Mana = new Resource();
        Attributes = new Attributes();
    }

    void Update()
    {
        if (!IsResource)
        {
            BuffExpirationCheck();

            if (Dead)
            {
                //If dead despawn.
                Despawn();
            }
            else
            {
                //Moves the entity towards given position.
                MoveEntityTowards(EndPosition);
            }
        }

        //Keep the entity on the ground.
        SetEntityOnGround();
    }

    private void MoveEntityTowards(Vector3 endPosition)
    {
        this.transform.rotation = Quaternion.LookRotation(endPosition - this.transform.position);
        this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);

        if (Vector2.Distance(EndPosition.XZ(), this.transform.position.XZ()) > 0.3f && endPosition.sqrMagnitude > 1f)
        {
            this.transform.position += transform.forward * Time.deltaTime * MovementSpeed;
        }
    }

    /// <summary>
    /// Always keeps the entity grounded, by shooting a ray from above to the ground.
    /// </summary>
    private void SetEntityOnGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 200, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 300, LayerMask.GetMask("Terrain")))
        {
            this.transform.position = new Vector3(this.transform.position.x, hit.point.y + 0.5f, this.transform.position.z);
        }
    }

    private void BuffExpirationCheck()
    {
        for (int i = Buffs.Count - 1; i >= 0; i--)
        {
            Buffs[i].BuffLifetime -= Time.deltaTime;
        }

        Buffs.RemoveAll(x => x.BuffLifetime < 0);
    }

    public void SetLevel(ushort level)
    {
        Level = level;
        PlayLevelUpVFX();
    }

    public void PlayHitVFX()
    {
        SwordAnimator.ResetTrigger("Swing");
        SwordAnimator.SetTrigger("Swing");
    }

    public void DeSelect()
    {
        if (Alive)
        {
            info.Normal();
        }
    }

    public void Highlight()
    {
        if (Alive)
        {
            info.gameObject.SetActive(true);
            info.Highlight();
        }
    }

    public void Select()
    {
        if (Alive)
        {
            info.gameObject.SetActive(true);
            info.Selected();
        }
    }

    public void ShowInfo()
    {
        if (info == null) return;
        if (Alive)
            info.gameObject.SetActive(true);
        info.Normal();
    }

    public void Respawn()
    {
        gameObject.SetActive(true);
        EndPosition = Vector3.zero;
        despawnTime = maxDespawnTime;
        PlayOnHitVFX();
    }

    public void Despawn()
    {
        info.gameObject.SetActive(false);

        despawnTime -= Time.deltaTime;
        if (despawnTime <= 0)
        {
            Buffs.Clear();
            this.gameObject.SetActive(false);
        }
    }

    public void PlayLevelUpVFX()
    {
        if (LevelUpVFX != null)
            LevelUpVFX.Play();
    }

    public void PlayOnHitVFX()
    {
        if (OnHitVFX != null)
            OnHitVFX.Play();
    }

    public void SetHealth(int value)
    {
        this.Health.Value = value;
        if (Dead && value > 0)
        {
            if (!gameObject.activeSelf)
                Respawn();
        }
        info.UpdateHealthBar(this);
    }

    public void SetMana(int value)
    {
        Mana.Value = value;
        info.UpdateHealthBar(this);
    }

    public void SetMaxHealth(int value)
    {
        Health.MaxValue = value;
        info.UpdateHealthBar(this);
    }

    internal void SetMaxMana(int value)
    {
        Mana.MaxValue = value;
        info.UpdateHealthBar(this);
    }

    /// <summary>
    /// If current position is too far from given position it will rapidly move the entity to the new position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    public void Teleport(float x, float z)
    {
        if ((transform.position.XZ() - new Vector3(x, 0, z).XZ()).sqrMagnitude > 1f)
            this.transform.position = new Vector3(x, 0, z);
    }

    /// <summary>
    /// Sets the path for an entity and moves it to its position if it is out of sync.
    /// </summary>
    /// <param name="startPos">Entity position on server</param>
    /// <param name="endPos">End position on server</param>
    /// <param name="moveSpeed">Entity can move this amount of unit in 1 second</param>
    public void SetPath(Vector3 startPos, Vector3 endPos, float moveSpeed)
    {
        Teleport(startPos.x, startPos.z);

        StartPosition = startPos;
        EndPosition = endPos;
        MovementSpeed = moveSpeed;
    }

    public void SetEndPosition(float x, float z)
    {
        EndPosition = new Vector3(x, 0, z);
    }

    public void SetMovementSpeed(float mvSpeed)
    {
        MovementSpeed = mvSpeed;
    }

    public void SetInfo(string name, ushort id)
    {
        Id = id;
        Name = name;
        gameObject.name = name;
        info.SetInfo(name, isPlayer);
    }

    /// <summary>
    /// Creates an instance of a DamageInfoController and plays OnHitVFX.
    /// </summary>
    /// <param name="effectType"></param>
    /// <param name="damageValue"></param>
    /// <param name="crit"></param>
    public void SpawnDamageInfo(EffectType effectType, int damageValue, bool crit)
    {
        PlayOnHitVFX();
        Instantiate(SpawnCollection.Instance.Collection[0]).GetComponent<DamageInfoController>().Set(this, effectType, damageValue, crit);
    }

    public void AddBuff(Buff newBuff)
    {
        bool found = false;
        foreach (var buff in Buffs)
        {
            if (buff.BuffId == newBuff.BuffId)
            {
                buff.BuffLifetime = newBuff.BuffLifetime;
                found = true;
            }
        }
        if (!found)
        {
            Buffs.Add(newBuff);
        }
    }
}
