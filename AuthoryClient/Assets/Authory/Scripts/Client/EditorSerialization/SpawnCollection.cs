using Assets.Authory.Scripts.Enum;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The game will call in here if it needs something to spawn.
/// Contains all the spawnable GameObjects except Skills, Skills are handler in SkillCollection!.
/// </summary>
public class SpawnCollection : MonoBehaviour
{
    [SerializeField] public List<GameObject> Collection;
    [SerializeField] List<GameObject> EntityCollection;

    public Dictionary<ModelType, GameObject> EntityPrefabs { get; private set; }

    private static SpawnCollection _instance;
    public static SpawnCollection Instance => _instance;


    private void Awake()
    {
        EntityPrefabs = new Dictionary<ModelType, GameObject>();
        foreach (var entity in EntityCollection)
        {
            EntityPrefabs.Add(entity.GetComponent<Entity>().ModelType, entity);
        }

        _instance = this;
    }
}
