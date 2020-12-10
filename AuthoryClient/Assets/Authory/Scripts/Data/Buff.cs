using UnityEngine;

/// <summary>
/// Defines a buff.
/// </summary>
[CreateAssetMenu(fileName = "Buff", menuName = "ScriptableObjects/Create Buff", order = 1)]
public class Buff : ScriptableObject
{
    [SerializeField] public ushort BuffId;
    [SerializeField] public string BuffName;
    [SerializeField] public string Description;
    [SerializeField] public Sprite ContentSprite;

    public float BuffLifetime { get; set; }
}
