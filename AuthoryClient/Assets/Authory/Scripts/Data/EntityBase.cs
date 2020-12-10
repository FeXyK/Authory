using Assets.Authory.Scripts.Enum;
using UnityEngine;

public class EntityBase : MonoBehaviour
{
    [SerializeField] public string Name;
    [SerializeField] public ushort Id;
    [SerializeField] public ModelType ModelType;

    [SerializeField] protected EntityInfo info = null;
    [SerializeField] protected float maxDespawnTime = 5f;
    [SerializeField] protected bool IsResource;
}